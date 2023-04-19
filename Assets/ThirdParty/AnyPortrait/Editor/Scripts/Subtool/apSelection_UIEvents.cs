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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnyPortrait
{

	//apSelection의 UI를 그릴때 발생하거나 처리되는 이벤트를 모아놓은 부분 클래스
	public partial class apSelection
	{
		//----------------------------------------------------------------
		// 공통 단축키 이벤트
		//----------------------------------------------------------------
		/// <summary>
		/// 단축키 [A]를 눌러서 Editing 상태를 토글하자
		/// </summary>
		/// <param name="paramObject"></param>
		public apHotKey.HotKeyResult OnHotKeyEvent_ToggleModifierEditing(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				//|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None
				)
			{
				return null;
			}

			//v1.4.2 : FFD일땐 토글이 제한될 수 있다.
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return null;
			}


			ToggleRigEditBinding();

			//변경 22.5.15
			return apHotKey.HotKeyResult.MakeResult(_exclusiveEditing != EX_EDIT.None ? apStringFactory.I.ON : apStringFactory.I.OFF);
			
		}


		/// <summary>
		/// 단축키 [S]에 의해서도 SelectionLock(Modifier)를 바꿀 수 있다.
		/// </summary>
		public apHotKey.HotKeyResult OnHotKeyEvent_ToggleExclusiveEditKeyLock(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				//|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None
				)
			{
				return null;
			}
			_isSelectionLock = !_isSelectionLock;

			return apHotKey.HotKeyResult.MakeResult(_isSelectionLock ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}


		//-----------------------------------------------------------------------
		// 루트 유닛 UI 이벤트
		//-----------------------------------------------------------------------
		/// <summary>
		/// [루트 유닛] 화면 캡쳐 중 "썸네일" 처리가 완료되었다.
		/// </summary>
		private void OnThumbnailCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			_captureMode = CAPTURE_MODE.None;

			//우왕 왔당
			if (!isSuccess || captureImage == null)
			{
				//Debug.LogError("Failed..");
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				return;
			}
			if (_captureLoadKey != loadKey)
			{
				//Debug.LogError("LoadKey Mismatched");

				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				return;
			}


			//이제 처리합시당 (Destroy도 포함되어있다)
			string filePathWOExtension = _editor._portrait._imageFilePath_Thumbnail.Substring(0, _editor._portrait._imageFilePath_Thumbnail.Length - 4);
			bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, true);

			if (isSaveSuccess)
			{
				AssetDatabase.Refresh();

				_editor._portrait._thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_editor._portrait._imageFilePath_Thumbnail);
			}
		}

		/// <summary>
		/// [루트 유닛] 화면 캡쳐 중 "스크린샷" 처리가 완료되었다.
		/// </summary>
		private void OnScreeenShotCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			_captureMode = CAPTURE_MODE.None;

			//우왕 왔당
			if (!isSuccess || captureImage == null || string.IsNullOrEmpty(filePath))
			{
				//Debug.LogError("Failed..");
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				return;
			}
			if (_captureLoadKey != loadKey)
			{
				//Debug.LogError("LoadKey Mismatched");

				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				return;
			}

			//이제 파일로 저장하자
			try
			{
				string filePathWOExtension = filePath.Substring(0, filePath.Length - 4);

				//AutoDestroy = true
				bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, true);

				if (isSaveSuccess)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(filePath);//Path 빈 문자열 확인했음 (21.9.10)

					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + filePath);

					//_prevFilePath = filePath;
					_capturePrevFilePath_Directory = fi.Directory.FullName;
				}
			}
			catch (Exception)
			{

			}
		}





		/// <summary>
		/// [루트 유닛] 화면 캡쳐 중 "MP4" 처리가 완료되었다.
		/// </summary>
		private void OnGIFMP4AnimationSaved(bool isResult)
		{
			_captureMode = CAPTURE_MODE.None;
		}


		/// <summary>
		/// [루트 유닛] 화면 캡쳐 중 "스프라이트 시트" 처리가 완료되었다.
		/// </summary>
		private void OnSpritesheetSaved(bool isResult)
		{
			//Debug.LogError("OnSpritesheetSaved : " + isResult);
			_captureMode = CAPTURE_MODE.None;
		}







		//----------------------------------------------------------------
		// 이미지 선택시의 UI 이벤트
		//----------------------------------------------------------------
		/// <summary>
		/// 텍스쳐 에셋 선택 결과 콜백
		/// </summary>
		private void OnTextureAssetSelected(bool isSuccess, apTextureData targetTextureData, object loadKey, Texture2D resultTexture2D)
		{
			if (_loadKey_SelectTextureAsset != loadKey || !isSuccess)
			{
				_loadKey_SelectTextureAsset = null;
				return;
			}
			_loadKey_SelectTextureAsset = null;
			if (targetTextureData == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Image_SettingChanged, 
												Editor, 
												Editor._portrait, 
												//targetTextureData, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			targetTextureData._image = resultTexture2D;
			//이미지가 추가되었다.
			if (targetTextureData._image != null)
			{
				targetTextureData._name = targetTextureData._image.name;
				targetTextureData._width = targetTextureData._image.width;
				targetTextureData._height = targetTextureData._image.height;

				//이미지 에셋의 Path를 확인하고, PSD인지 체크한다.
				if (targetTextureData._image != null)
				{
					string fullPath = AssetDatabase.GetAssetPath(targetTextureData._image);
					//Debug.Log("Image Path : " + fullPath);

					if (string.IsNullOrEmpty(fullPath))
					{
						targetTextureData._assetFullPath = "";
						//targetTextureData._isPSDFile = false;
					}
					else
					{
						targetTextureData._assetFullPath = fullPath;
					}
				}
				else
				{
					targetTextureData._assetFullPath = "";
					//targetTextureData._isPSDFile = false;
				}
			}
			

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);
		}


		//----------------------------------------------------------------------
		// 메시 UI에서의 이벤트
		//----------------------------------------------------------------------
		
		/// <summary>
		/// 메시의 이름 변경 이벤트
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_RenameMesh(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.Mesh
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Setting
				|| Mesh == null)
			{
				return null;
			}

			//이름바꾸기 칸으로 포커스를 옮기자
			apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__MeshName);

			return apHotKey.HotKeyResult.MakeResult();
		}


		
		/// <summary>
		/// 추가 20.12.4 : 단축키를 눌러서 메시 편집 모드를 바꿀 수 있다.
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_SetMeshTab(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.Mesh
				|| Mesh == null
				|| !(paramObj is int))
			{
				return null;
			}

			int iParam = (int)paramObj;

			//v1.4.2 탭 전환 전에 모달 상태를 확인하자
			bool isExecutable = Editor.CheckModalAndExecutable();

			if (!isExecutable)
			{
				return null;
			}

			switch (iParam)
			{
				case 0://Setting탭
					{
						if(Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Setting)
						{
							Editor.Controller.CheckMeshEdgeWorkRemained();
							Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Setting;
							Editor._isMeshEdit_AreaEditing = false;
							_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

							Editor.Gizmos.Unlink();
						}

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Setting);
					}
					//break;

				case 1://Make Mesh - Add
					{
						//v1.4.2 : 이미 MakeMesh / AddTools 탭이 선택된 상태라면
						//AddTool의 대상을 하나씩 전환한다.
						//그 외의 경우엔 MakeMesh / AddTools를 연다.

						if (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.MakeMesh
							&& Editor._meshEditeMode_MakeMesh_Tab == apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AddTools)
						{
							//< MakeMesh / AddTools 탭이 선택된 상태 >
							
							//다음 툴로 순서대로 넘어간다.
							apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS nextTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge;

							switch (Editor._meshEditeMode_MakeMesh_AddTool)
							{
								case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge:
									nextTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly;
									break;

								case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly:
									nextTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly;
									break;

								case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly:
									nextTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon;
									break;

								case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon:
									nextTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge;
									break;
							}

							Editor._meshEditeMode_MakeMesh_AddTool = nextTool;
							Editor.VertController.UnselectVertex();
						}
						else
						{
							//< 다른 화면이 열린 상태 >
							//일단 Make Mesh 체크
							if (Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh)
							{
								Editor.Controller.CheckMeshEdgeWorkRemained();
								Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
								Editor._isMeshEdit_AreaEditing = false;
								_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

								Editor.Controller.StartMeshEdgeWork();
								Editor.VertController.SetMesh(_mesh);
								Editor.VertController.UnselectVertex();

								Editor.Gizmos.Unlink();
							}

							//이어서 Make Mesh 모드 체크
							if (Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AddTools)
							{
								//Add Tools로 변경
								Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AddTools;
								Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
								Editor._isMeshEdit_AreaEditing = false;
								_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

								//미러도 초기화
								Editor.MirrorSet.Clear();
								Editor.MirrorSet.ClearMovedVertex();
								Editor.VertController.UnselectVertex();
							}
						}
						

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.AddTool);
					}
					//break;

				case 2://Make Mesh - Edit
					{
						//일단 Make Mesh 체크
						if(Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh)
						{
							Editor.Controller.CheckMeshEdgeWorkRemained();
							Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
							Editor.Controller.StartMeshEdgeWork();
							Editor.VertController.SetMesh(_mesh);
							Editor.VertController.UnselectVertex();

							Editor.Gizmos.Unlink();
						}
						if(Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
						{
							Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS;
							Editor._isMeshEdit_AreaEditing = false;
							_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;
							
							//기즈모 이벤트 변경
							Editor.Gizmos.Unlink();
							Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshTRS());
							
							//미러도 초기화
							Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
							Editor.MirrorSet.Clear();
							Editor.MirrorSet.ClearMovedVertex();
							Editor.VertController.UnselectVertex();
						}

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.EditTool);
					}
					//break;

				case 3://Make Mesh - Auto
					{
						//일단 Make Mesh 체크
						if(Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh)
						{
							Editor.Controller.CheckMeshEdgeWorkRemained();
							Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
							Editor.Controller.StartMeshEdgeWork();
							Editor.VertController.SetMesh(_mesh);
							Editor.VertController.UnselectVertex();

							Editor.Gizmos.Unlink();
						}
						if(Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen)
						{
							Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen;
							Editor._isMeshEdit_AreaEditing = false;
							_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;
							
							//기즈모 이벤트 변경
							Editor.Gizmos.Unlink();
							//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshAreaEdit());

							//미러도 초기화
							Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
							Editor.MirrorSet.Clear();
							Editor.MirrorSet.ClearMovedVertex();
						}

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.AutoTool);
					}
					//break;

				case 4://Pivot
					{
						if (Editor._meshEditMode != apEditor.MESH_EDIT_MODE.PivotEdit)
						{
							Editor.Controller.CheckMeshEdgeWorkRemained();
							Editor._meshEditMode = apEditor.MESH_EDIT_MODE.PivotEdit;
							Editor._isMeshEdit_AreaEditing = false;
							_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

							Editor.Gizmos.Unlink();
						}

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Pivot);
					}
					//break;

				case 5://Modify
					{
						if (Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Modify)
						{
							Editor.Controller.CheckMeshEdgeWorkRemained();
							Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Modify;
							Editor._isMeshEdit_AreaEditing = false;
							_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

							Editor.Gizmos.Unlink();
							Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshEdit_Modify());
						}

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Modify);
					}
				//break;
				case 6:
					{
						if(Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin)
						{
							Editor.Controller.CheckMeshEdgeWorkRemained();
							Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Pin;
							Editor._isMeshEdit_AreaEditing = false;
							_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

							Editor.Gizmos.Unlink();
							//기즈모 추가 필요
							RefreshPinModeEvent();
						}
						else
						{
							//v1.4.2 : 이미 핀 화면이라면 순서대로 다음 Pin 툴로 넘어간다.

							switch (Editor._meshEditMode_Pin_ToolMode)
							{
								case apEditor.MESH_EDIT_PIN_TOOL_MODE.Select:
									SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Add);
									break;

								case apEditor.MESH_EDIT_PIN_TOOL_MODE.Add:
									SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Link);
									break;

								case apEditor.MESH_EDIT_PIN_TOOL_MODE.Link:
									SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Test);
									break;

								case apEditor.MESH_EDIT_PIN_TOOL_MODE.Test:
									SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Select);
									break;
							}
						}

						return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Pin);
					}
			}
			return null;
		}


		
		/// <summary>
		/// 메시의 TextureData(이미지) 선택 결과 이벤트
		/// </summary>
		private void OnSelectTextureDataToMesh(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if (!isSuccess || resultTextureData == null || _mesh != targetMesh || _loadKey_SelectTextureDataToMesh != loadKey)
			{
				_loadKey_SelectTextureDataToMesh = null;
				return;
			}

			_loadKey_SelectTextureDataToMesh = null;

			//Undo
			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SetImage, 
											Editor, 
											targetMesh, 
											//resultTextureData, 
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			//이전 코드
			//_mesh._textureData = resultTextureData;

			//변경 코드 4.1
			_mesh.SetTextureData(resultTextureData);

			//_isShowTextureDataList = false;

		}

		
		private object _loadKey_QuickMeshWizard = null;

		/// <summary>
		/// 자동 메시 마법사 다이얼로그 결과 이벤트
		/// </summary>
		private void OnQuickMeshWizardCompleted(bool isSuccess, object loadKey, List<apDialog_QuickMeshWizard.MakeRequest> requests)
		{
			if(!isSuccess 
				|| loadKey != _loadKey_QuickMeshWizard
				|| requests == null
				|| requests.Count == 0)
			{
				_loadKey_QuickMeshWizard = null;
				return;
			}

			_loadKey_QuickMeshWizard = null;



			int nRequest = requests.Count;
			apDialog_QuickMeshWizard.MakeRequest curRequest = null;

			//먼저 대상이 되는 객체를 모은다.
			//이미 대상을 확인했겠지만, 한번더
			List<apDialog_QuickMeshWizard.MakeRequest> targetRequests = new List<apDialog_QuickMeshWizard.MakeRequest>();
			List<apMesh> targetMeshes = new List<apMesh>();
			for (int iRequest = 0; iRequest < nRequest; iRequest++)
			{
				curRequest = requests[iRequest];

				if(!curRequest._isAvailable || !curRequest._isMake || curRequest._linkedMesh == null)
				{
					//조건이 충족되지 않는다.
					continue;
				}
				//여기서 미리 SetRecord + Replace를 하자
				if(targetMeshes.Contains(curRequest._linkedMesh))
				{
					//이미 포함되었다면
					continue;
				}

				targetMeshes.Add(curRequest._linkedMesh);
				targetRequests.Add(curRequest);
			}

			if(targetRequests.Count == 0)
			{
				Debug.LogError("AnyPortrait : No proper Auto-Mesh Request");
				return;
			}


			

			//Set Record
			apEditorUtil.SetRecord_Meshes(	apUndoGroupData.ACTION.MeshEdit_AutoGen, 
															Editor, 
															targetMeshes, 
															apEditorUtil.UNDO_STRUCT.StructChanged);



			Editor.MeshGeneratorV2.ReadyToRequest(OnMeshAutoGeneratedV2);

			
			nRequest = targetRequests.Count;
			apMesh curMesh = null;

			for (int i = 0; i < nRequest; i++)
			{
				curRequest = targetRequests[i];
				curMesh = curRequest._linkedMesh;

				//먼저, Replace 옵션인 경우, 버텍스 개수를 초기화한다. (묻지않고)
				if(curRequest._replaceOption == apDialog_QuickMeshWizard.ReplaceOption.Replace)
				{
					curMesh._vertexData.Clear();
					curMesh._indexBuffer.Clear();
					curMesh._edges.Clear();
					curMesh._polygons.Clear();

					curMesh.MakeEdgesToPolygonAndIndexBuffer();					
				}

				bool preset_IsInnerMargin = false;
				int preset_Density = 1;
				int preset_InnerMargin = 5;
				int preset_OuterMargin = 10;
				switch (curRequest._genOption)
				{
					case apDialog_QuickMeshWizard.GenerateOption.Simple:
						preset_IsInnerMargin = false;
						preset_Density = 1;
						preset_InnerMargin = 1;
						preset_OuterMargin = 5;
						break;

					case apDialog_QuickMeshWizard.GenerateOption.Moderate:
						preset_IsInnerMargin = true;
						preset_Density = 2;
						preset_InnerMargin = 5;
						preset_OuterMargin = 10;
						break;

					case apDialog_QuickMeshWizard.GenerateOption.Complex:
					default:
						preset_IsInnerMargin = true;
						preset_Density = 5;
						preset_InnerMargin = 5;
						preset_OuterMargin = 10;
						break;
				}

				Editor.MeshGeneratorV2.AddRequest(	curMesh,
													preset_Density,
													preset_OuterMargin,
													preset_InnerMargin,
													preset_IsInnerMargin);
			}

			//버텍스 선택 여부 초기화
			Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

			Editor.VertController.UnselectVertex();
			Editor.VertController.UnselectNextVertex();


			Editor.MeshGeneratorV2.StartGenerate();//시작!

			//프로그래스바도 출현
			Editor.StartProgressPopup("Mesh Generation", "Generating..", true, OnAutoGenProgressCancel);
		}


		
		/// <summary>
		/// 추가 21.1.4 : 메시 자동 생성을 강제로 종료한다.
		/// </summary>
		public void OnAutoGenProgressCancel()
		{
			Editor.MeshGeneratorV2.EndGenerate();
		}

		
		/// <summary>
		/// 추가 20.12.4 : 단축키로 Make Polygon을 실행한다.
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_MakeMeshPolygon(object paramObject)
		{
			if(SelectionType != SELECTION_TYPE.Mesh
				|| Mesh == null
				|| paramObject != (object)Mesh)
			{
				return null;
			}

			//v1.4.2 탭 전환 전에 모달 상태를 확인하자
			bool isExecutable = Editor.CheckModalAndExecutable();

			if (!isExecutable)
			{
				return null;
			}

			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MakeEdges, 
											Editor, 
											Mesh, 
											//Mesh, 
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			//Editor.VertController.StopEdgeWire();

			Mesh.MakeEdgesToPolygonAndIndexBuffer();
			Mesh.RefreshPolygonsToIndexBuffer();
			Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

			//Pin-Weight 갱신
			//옵션이 없어도 무조건 Weight 갱신
			if(_mesh != null && _mesh._pinGroup != null)
			{
				_mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
			}

			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}


		

		/// <summary>
		/// 메시 UI의 TRS 편집 모드에서 단축키로 버텍스 삭제하기 이벤트
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RemoveVertexOnTRS(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Mesh
				|| _mesh == null
				|| Editor.Gizmos.IsFFDMode
				|| Editor.VertController.Vertex == null
				|| Editor.VertController.Vertices.Count == 0)
			{
				return null;
			}

			//v1.4.2 탭 전환 전에 모달 상태를 확인하자
			bool isExecutable = Editor.CheckModalAndExecutable();

			if (!isExecutable)
			{
				return null;
			}

			bool isShift = Event.current.shift;

			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_RemoveVertex, 
											Editor, 
											_mesh, 
											//_mesh, 
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);

			List<apVertex> vertices = Editor.VertController.Vertices;
			for (int i = 0; i < vertices.Count; i++)
			{
				_mesh.RemoveVertex(vertices[i], isShift);
				Editor.SetRepaint();
			}
			_mesh.RefreshPolygonsToIndexBuffer();
			Editor.VertController.UnselectVertex();

			return apHotKey.HotKeyResult.MakeResult();
		}


		//추가 20.12.9
		/// <summary>
		/// 자동 메시 생성 기능 v2에 대한 콜백 이벤트
		/// </summary>
		/// <param name="curState"></param>
		/// <param name="subState"></param>
		/// <param name="results"></param>
		/// <param name="iCurRequest"></param>
		/// <param name="nRequests"></param>
		public void OnMeshAutoGeneratedV2(apMeshGeneratorV2.PROCESS_STATE curState, apMeshGeneratorV2.SUB_STEP subState, List<apMeshGeneratorV2.GenResult> results, int iCurRequest, int nRequests)
		{
			//Debug.LogWarning("<< OnMeshAutoGenerated >> : " + curState + " / Requests : " + iCurRequest + "/" + nRequests);

			bool isCompleted = false;
			float ratio = 0.0f;
			switch (curState)
			{
				case apMeshGeneratorV2.PROCESS_STATE.Step1_Ready:
					ratio = 0.0f;
					break;

				case apMeshGeneratorV2.PROCESS_STATE.Step2_Processing:
					{
						//Request에 따라서 다르다.
						float requestRatio = Mathf.Clamp((float)iCurRequest / (float)nRequests, 0.0f, 1.0f);
						float ratioPerRequest = 1.0f / (float)nRequests;
						float subRatio = 0.0f;

						switch (subState)
						{
							case apMeshGeneratorV2.SUB_STEP.None:
								subRatio = 0.0f;
								break;

							case apMeshGeneratorV2.SUB_STEP.Step1_PopRequest:
								subRatio = 0.1f;
								break;

							case apMeshGeneratorV2.SUB_STEP.Step2_FindRoots:
								//case apMeshGeneratorV2.SUB_STEP.Step3_MergeRoots://미사용
								//case apMeshGeneratorV2.SUB_STEP.Step4_MakeCircleTree://미사용
								//case apMeshGeneratorV2.SUB_STEP.Step5_MergeOuterVertices://미사용
								subRatio = 0.3f;
								break;
							case apMeshGeneratorV2.SUB_STEP.Step6_MakeOuterEdges:
								subRatio = 0.5f;
								break;
							case apMeshGeneratorV2.SUB_STEP.Step7_MakeInnerVertices:
								subRatio = 0.7f;
								break;

							case apMeshGeneratorV2.SUB_STEP.Step8_MakeTriangles:
								subRatio = 0.8f;
								break;

							case apMeshGeneratorV2.SUB_STEP.Step9_ApplyToMesh:
								subRatio = 0.9f;
								break;
						}

						//0.2~0.8
						ratio = requestRatio + (subRatio * ratioPerRequest);
						ratio *= 0.6f;
						ratio += 0.2f;
						ratio = Mathf.Clamp(ratio, 0.2f, 0.8f);
					}
					break;

				case apMeshGeneratorV2.PROCESS_STATE.Step4_Complete_Success:
				case apMeshGeneratorV2.PROCESS_STATE.Step4_Complete_SomeFailed:
				case apMeshGeneratorV2.PROCESS_STATE.Step4_Complete_AllFailed:
					{
						//로딩바 없애기
						isCompleted = true;
						ratio = 1.0f;

						//다이얼로그도 띄우자
						if (curState == apMeshGeneratorV2.PROCESS_STATE.Step4_Complete_SomeFailed ||
							curState == apMeshGeneratorV2.PROCESS_STATE.Step4_Complete_AllFailed)
						{
							if(results != null)
							{
								apStringWrapper strError = new apStringWrapper(1000);
								apMeshGeneratorV2.GenResult curResult = null;
								
								strError.Append("Some Processes are failed", false);
								
								for (int iResult = 0; iResult < results.Count; iResult++)
								{
									curResult = results[iResult];
									if(curResult._resultType == apMeshGeneratorV2.MESH_GEN_RESULT.Success)
									{
										continue;
									}

									strError.Append(apStringFactory.I.Return, false);
									strError.Append((curResult._mesh != null && !string.IsNullOrEmpty(curResult._mesh._name)) ? (curResult._mesh._name + " : ") : "(Unknown Mesh) : ", false);
									if (curResult._resultType == apMeshGeneratorV2.MESH_GEN_RESULT.Failed)
									{											
										strError.Append(!string.IsNullOrEmpty(curResult._errorMsg) ? curResult._errorMsg : "Unknown Error", false);
									}
									if (curResult._resultType == apMeshGeneratorV2.MESH_GEN_RESULT.Processing)
									{
										strError.Append("Processing was aborted", false);
									}
								}

								strError.MakeString();

								EditorUtility.DisplayDialog("Failed", strError.ToString(), Editor.GetText(TEXT.Okay));
							}
						}
					}
					break;
			}

			Editor.SetProgressPopupRatio(isCompleted, ratio);
			
		}




		//메시의 버텍스/핀 붙여넣기
		private object _loadKey_PasteMestVertPin = null;

		public void OnCopyMeshVerts(bool isSuccess,
												object loadKey, 
												apMesh dstMesh,
												apDialog_CopyMeshVertPin.POSITION_SPACE positionSpace)
		{
			if(!isSuccess
				|| _loadKey_PasteMestVertPin == null
				|| _loadKey_PasteMestVertPin != loadKey
				|| Mesh == null
				|| Mesh != dstMesh)
			{
				_loadKey_PasteMestVertPin = null;
				return;
			}

			_loadKey_PasteMestVertPin = null;

			//붙여넣기를 하자

			//아래 코드 참조
			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_VertexCopied,
													Editor,
													Mesh,
													//mesh,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

			List<apVertex> copiedVert = apSnapShotManager.I.Paste_MeshVertices(Mesh, positionSpace);

			Mesh.RefreshPolygonsToIndexBuffer();

			//복사된걸 선택하자
			if (copiedVert != null)
			{
				Editor.VertController.UnselectVertex();
				Editor.VertController.SelectVertices(copiedVert, apGizmos.SELECT_TYPE.New);
			}

			int nVerts = copiedVert != null ? copiedVert.Count : 0;

			if(nVerts > 1)
			{
				Editor.Notification(nVerts + " Vertices have been copied.", false, false);
			}
			else
			{
				Editor.Notification(nVerts + " Vertex has been copied.", false, false);
			}

			Editor.SetRepaint();
			Editor.OnAnyObjectAddedOrRemoved();
			
		}



		public void OnCopyMeshPins(bool isSuccess,
									object loadKey, 
									apMesh dstMesh,
									apDialog_CopyMeshVertPin.POSITION_SPACE positionSpace)
		{
			if(!isSuccess
				|| _loadKey_PasteMestVertPin == null
				|| _loadKey_PasteMestVertPin != loadKey
				|| Mesh == null
				|| Mesh != dstMesh)
			{
				_loadKey_PasteMestVertPin = null;
				return;
			}

			_loadKey_PasteMestVertPin = null;

			//붙여넣기를 하자

			//아래 코드 참조
			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddPin,
													Editor,
													Mesh,
													//mesh,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

			List<apMeshPin> copiedPin = apSnapShotManager.I.Paste_MeshPins(Mesh, positionSpace);

			//붙여넣기를 했다면 핀 그룹을 전체 갱신하자
			if(Mesh._pinGroup != null)
			{
				Mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
			}
			//복사된걸 선택하자
			if (copiedPin != null)
			{
				Editor.Select.SelectMeshPins(copiedPin, apGizmos.SELECT_TYPE.New);
			}

			int nPins = copiedPin != null ? copiedPin.Count : 0;

			if(nPins > 1)
			{
				Editor.Notification(nPins + " Pins have been copied.", false, false);
			}
			else
			{
				Editor.Notification(nPins + " Pin has been copied.", false, false);
			}

			Editor.SetRepaint();
			Editor.OnAnyObjectAddedOrRemoved();
			
		}


		//----------------------------------------------------------------------
		// 메시 그룹 UI에서의 이벤트
		//----------------------------------------------------------------------
		
		/// <summary>
		/// 메시 그룹 단축키 이벤트. 단축키(F2)를 누르면 조건에 따라 메시 그룹의 이름을 바꿀 수 있다.
		/// </summary>
		/// <param name="paramObj"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RenameMeshGroup(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.MeshGroup
				|| Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Setting
				|| MeshGroup == null
				|| SubObjects.NumMeshTF > 0
				|| SubObjects.NumMeshGroupTF > 0
				|| paramObj != (object)MeshGroup)
			{
				return null;
			}

			//이름바꾸기 칸으로 포커스를 옮기자
			apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__MeshGroupName);

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// 메시 그룹 탭에서 "본 구조 파일 임포트"시의 콜백 이벤트
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="loadKey"></param>
		/// <param name="retargetData"></param>
		/// <param name="targetMeshGroup"></param>
		private void OnBoneStruceLoaded(bool isSuccess, object loadKey, apRetarget retargetData, apMeshGroup targetMeshGroup)
		{
			if (!isSuccess || _loadKey_OnBoneStructureLoaded != loadKey || _meshGroup != targetMeshGroup || targetMeshGroup == null)
			{
				_loadKey_OnBoneStructureLoaded = null;
				return;
			}
			_loadKey_OnBoneStructureLoaded = null;

			if (retargetData.IsBaseFileLoaded)
			{
				Editor.Controller.ImportBonesFromRetargetBaseFile(targetMeshGroup, retargetData);
			}
		}


		

		/// <summary>
		/// 메시 그룹 탭에서 모디파이어 추가 다이얼로그의 결과 이벤트
		/// </summary>
		private void OnAddModifier(bool isSuccess, object loadKey, apModifierBase.MODIFIER_TYPE modifierType, apMeshGroup targetMeshGroup, int validationKey)
		{
			if (!isSuccess || _loadKey_AddModifier != loadKey || MeshGroup != targetMeshGroup)
			{
				_loadKey_AddModifier = null;
				return;
			}

			if (modifierType != apModifierBase.MODIFIER_TYPE.Base)
			{
				Editor.Controller.AddModifier(modifierType, validationKey);
			}
			_loadKey_AddModifier = null;
		}


		

		/// <summary>
		/// 메시 그룹 Right2 UI : 단축키(F2)를 누르면 조건에 따라 메시 그룹의 자식 트랜스폼의 이름을 바꿀 수 있다.
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_RenameTransform(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.MeshGroup
				|| Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Setting
				|| MeshGroup == null
				|| (SubObjects.NumMeshTF == 0 && SubObjects.NumMeshGroupTF == 0)
				)
			{
				return null;
			}

			//대상이 동일한가
			if(SubObjects.SelectedObject != paramObj)
			{
				return null;
			}

			//이름바꾸기 칸으로 포커스를 옮기자
			apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__SubTransformName);

			return apHotKey.HotKeyResult.MakeResult();
		}



		



		/// <summary>
		/// 메시 그룹 Right2 UI : TF Detach 단축키
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_DetachTransform(object paramObject)
		{	
			if(MeshGroup == null)
			{
				return null;
			}
			
			List<apTransform_Mesh> selectedMeshTFs = GetSubSeletedMeshTFs(false);
			List<apTransform_MeshGroup> selectedMeshGroupTFs = GetSubSeletedMeshGroupTFs(false);
			int nSubSelectedMeshTFs_All = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
			int nSubSelectedMeshGroupTFs_All = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;


			//변경사항 v1.4.2
			//이제 MeshTF와 MeshGroupTF가 섞여있어도 삭제 가능하다.

			if(nSubSelectedMeshTFs_All == 0 && nSubSelectedMeshGroupTFs_All == 0)
			{
				//선택된게 없으면 동작하지 않는다.
				return null;
			}

			//삭제 > 혼합되어도 가능합
			//if(nSubSelectedMeshTFs_All > 0 && nSubSelectedMeshGroupTFs_All > 0)
			//{
			//	//두 종류가 섞여서 선택되었어도 동작하지 않는다.
			//	return null;
			//}

			string strDialogInfo = Editor.GetText(TEXT.Detach_Body);


			//다이얼로그 메시지를 만들자
			#region [미사용 코드]
			//if(nSubSelectedMeshTFs_All > 0)
			//{
			//	//메시가 선택되었을 때
			//	if(nSubSelectedMeshTFs_All == 1 && MeshTF_Main != null)
			//	{
			//		//메시 단일 선택
			//		strDialogInfo = Editor.Controller.GetRemoveItemMessage(
			//															_portrait,
			//															MeshTF_Main,
			//															5,
			//															Editor.GetText(TEXT.Detach_Body),
			//															Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));
			//	}
			//}
			//else if(nSubSelectedMeshGroupTFs_All > 0)
			//{
			//	//메시 그룹이 선택되었을 때
			//	if(nSubSelectedMeshGroupTFs_All == 1 && MeshGroupTF_Main != null)
			//	{
			//		//메시 그룹 단일 선택
			//		strDialogInfo = Editor.Controller.GetRemoveItemMessage(
			//															_portrait,
			//															MeshGroupTF_Main,
			//															5,
			//															Editor.GetText(TEXT.Detach_Body),
			//															Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));
			//	}
			//}
			//else
			//{
			//	return null;
			//} 
			#endregion

			//변경 v1.4.2 : 여러개의 타입에 대해 동시에 변경 기록 조회하기
			strDialogInfo = Editor.Controller.GetRemoveItemsMessage(	_portrait,
																		selectedMeshTFs, selectedMeshGroupTFs,
																		5,
																		Editor.GetText(TEXT.Detach_Body),
																		Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

			bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																				//Editor.GetText(TEXT.Detach_Body),
																				strDialogInfo,
																				Editor.GetText(TEXT.Detach_Ok),
																				Editor.GetText(TEXT.Cancel)
																				);
			if(!isResult)
			{
				return null;
			}

			//이전
			//if (nSubSelectedMeshTFs_All > 0)
			//{
			//	//메시가 선택되었을 때
			//	if (nSubSelectedMeshTFs_All == 1 && MeshTF_Main != null)
			//	{
			//		//단일 메시 삭제
			//		Editor.Controller.DetachMeshTransform(MeshTF_Main, MeshGroup);
			//		Editor.Select.SelectMeshTF(null, MULTI_SELECT.Main);
			//	}
			//	else
			//	{
			//		//다중 메시 삭제
			//		Editor.Controller.DetachMeshTransforms(selectedMeshTFs, MeshGroup);
			//		Editor.Select.SelectMeshTF(null, MULTI_SELECT.Main);
			//	}
			//}
			//else if (nSubSelectedMeshGroupTFs_All > 0)
			//{
			//	//메시 그룹이 선택되었을 때
			//	if (nSubSelectedMeshGroupTFs_All == 1 && MeshGroupTF_Main != null)
			//	{
			//		//단일 메시 그룹 삭제
			//		Editor.Controller.DetachMeshGroupTransform(MeshGroupTF_Main, MeshGroup);
			//		Editor.Select.SelectMeshGroupTF(null, MULTI_SELECT.Main);
			//	}
			//	else
			//	{
			//		//다중 메시 그룹 삭제
			//		Editor.Controller.DetachMeshGroupTransforms(selectedMeshGroupTFs, MeshGroup);
			//		Editor.Select.SelectMeshGroupTF(null, MULTI_SELECT.Main);
			//	}
			//}

			//변경 v1.4.2 : 타입에 관계없이 다중 Detach 지원
			//단일 제거부터 하자
			if (nSubSelectedMeshTFs_All == 1 && MeshTF_Main != null && nSubSelectedMeshGroupTFs_All == 0)
			{
				//단일 메시 삭제
				Editor.Controller.DetachMeshTransform(MeshTF_Main, MeshGroup);
				Editor.Select.SelectMeshTF(null, MULTI_SELECT.Main);
			}
			else if (nSubSelectedMeshGroupTFs_All == 1 && MeshGroupTF_Main != null && nSubSelectedMeshTFs_All == 0)
			{
				//단일 메시 그룹 삭제
				Editor.Controller.DetachMeshGroupTransform(MeshGroupTF_Main, MeshGroup);
				Editor.Select.SelectMeshGroupTF(null, MULTI_SELECT.Main);
			}
			else
			{
				//그 외에는 다중 삭제
				Editor.Controller.DetachMultipleTransforms(selectedMeshTFs, selectedMeshGroupTFs, MeshGroup);
				Editor.Select.SelectSubObject(null, null, null, MULTI_SELECT.Main, TF_BONE_SELECT.Exclusive);
			}

			MeshGroup.SetDirtyToSort();//TODO : Sort에서 자식 객체 변한것 체크 : Clip 그룹 체크
			MeshGroup.RefreshForce();
			Editor.SetRepaint();

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// 메시 그룹 Right2 UI : 선택된 MeshTF의 재질 세트 변경하기 이벤트
		/// </summary>
		private void OnMaterialSetOfMeshTFSelected(bool isSuccess, object loadKey, apMaterialSet resultMaterialSet, bool isNoneSelected, object savedObject)
		{
			if (!isSuccess || loadKey != _loadKey_SelectMaterialSetOfMeshTransform || resultMaterialSet == null || savedObject != MeshTF_Main)
			{
				_loadKey_SelectMaterialSetOfMeshTransform = null;
				return;
			}
			_loadKey_SelectMaterialSetOfMeshTransform = null;

			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
												Editor, 
												MeshGroup, 
												//MeshGroup, 
												false, true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			MeshTF_Main._linkedMaterialSet = resultMaterialSet;
			MeshTF_Main._materialSetID = resultMaterialSet._uniqueID;

			Editor.SetRepaint();
			apEditorUtil.ReleaseGUIFocus();
		}




		/// <summary>
		/// 메시 그룹의 Right 2 UI : 설정 복사를 위한 다른 Transform 선택 이벤트
		/// </summary>
		private void OnSelectOtherMeshTransformsForCopyingSettings(bool isSuccess,
																	object loadKey,
																	apTransform_Mesh srcMeshTransform,
																	List<apTransform_Mesh> selectedObjects,
																	List<apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES> copiedProperties)
		{
			//다른 메시에 속성을 복사하는 다이얼로그의 이벤트
			if (!isSuccess || _loadKey_SelectOtherMeshTransformForCopyingSettings != loadKey || selectedObjects == null || srcMeshTransform == null || MeshGroup == null)
			{
				_loadKey_SelectOtherMeshTransformForCopyingSettings = null;
				return;
			}


			_loadKey_SelectOtherMeshTransformForCopyingSettings = null;


			if (MeshTF_Main == null || srcMeshTransform == null || MeshTF_Main != srcMeshTransform)
			{
				//요청한 MeshTransform이 현재 MeshTransform과 다르다.
				return;
			}

			if (selectedObjects.Contains(MeshTF_Main))
			{
				selectedObjects.Remove(MeshTF_Main);
			}

			if (selectedObjects.Count == 0 || copiedProperties.Count == 0)
			{
				//복사할 대상이 없다.
				return;
			}



			//속성들을 복사하자.
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
												Editor, 
												MeshGroup, 
												//MeshTF_Main, 
												false, true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apTransform_Mesh meshTF = null;
			for (int iMesh = 0; iMesh < selectedObjects.Count; iMesh++)
			{
				meshTF = selectedObjects[iMesh];

				//속성들을 하나씩 복사한다.
				for (int iProp = 0; iProp < copiedProperties.Count; iProp++)
				{
					switch (copiedProperties[iProp])
					{
						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.DefaultColor:
							//_meshColor2X_Default
							meshTF._meshColor2X_Default = srcMeshTransform._meshColor2X_Default;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.ShaderType:
							//_shaderType
							meshTF._shaderType = srcMeshTransform._shaderType;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.CustomShader:
							//_isCustomShader, _customShader
							meshTF._isCustomShader = srcMeshTransform._isCustomShader;
							meshTF._customShader = srcMeshTransform._customShader;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.RenderTextureSize:
							//_renderTexSize
							meshTF._renderTexSize = srcMeshTransform._renderTexSize;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.TwoSides:
							//_isAlways2Side
							meshTF._isAlways2Side = srcMeshTransform._isAlways2Side;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.ShadowSettings:
							//_isUsePortraitShadowOption, _shadowCastingMode, _receiveShadow
							meshTF._isUsePortraitShadowOption = srcMeshTransform._isUsePortraitShadowOption;
							meshTF._shadowCastingMode = srcMeshTransform._shadowCastingMode;
							meshTF._receiveShadow = srcMeshTransform._receiveShadow;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.MaterialSet:
							//_materialSetID
							meshTF._isUseDefaultMaterialSet = srcMeshTransform._isUseDefaultMaterialSet;
							meshTF._materialSetID = srcMeshTransform._materialSetID;
							meshTF._linkedMaterialSet = srcMeshTransform._linkedMaterialSet;
							break;

						case apDialog_SelectMeshTransformsToCopy.COPIED_PROPERTIES.MaterialProperties:
							//_customMaterialProperties
							{
								if (meshTF._customMaterialProperties == null)
								{
									meshTF._customMaterialProperties = new List<apTransform_Mesh.CustomMaterialProperty>();
								}
								meshTF._customMaterialProperties.Clear();

								if (srcMeshTransform._customMaterialProperties != null)
								{
									for (int iCustomProp = 0; iCustomProp < srcMeshTransform._customMaterialProperties.Count; iCustomProp++)
									{
										apTransform_Mesh.CustomMaterialProperty newCustomProp = new apTransform_Mesh.CustomMaterialProperty();
										newCustomProp.CopyFromSrc(srcMeshTransform._customMaterialProperties[iCustomProp]);

										meshTF._customMaterialProperties.Add(newCustomProp);
									}
								}
							}
							break;
					}
				}
			}

			MeshGroup.RefreshForce();

			//_loadKey_SelectOtherMeshTransformForCopyingSettings = apDialog_SelectMultipleObjects.ShowDialog(
			//														Editor, 
			//														MeshGroup, 
			//														apDialog_SelectMultipleObjects.REQUEST_TARGET.MeshAndMeshGroups, 
			//														OnSelectOtherMeshTransformsForCopyingSettings, 
			//														_editor.GetText(TEXT.DLG_Apply),
			//														SubMeshInGroup, SubMeshInGroup);
		}



		/// <summary>
		/// 메시 그룹의 Right 2 UI : 데이터 이주를 위한 대상 메시 그룹 선택 결과 이벤트
		/// </summary>
		private void OnSelectMeshGroupToMigrate(	bool isSuccess, object loadKey, apMeshGroup dstMeshGroup, 
													bool isSingleSelected,
													apTransform_Mesh targetMeshTransform, 
													List<apTransform_Mesh> targetMeshTransforms,
													apMeshGroup srcMeshGroup, bool isSelectParent)
		{
			if (!isSuccess
				|| dstMeshGroup == null
				|| loadKey == null
				|| _loadKey_MigrateMeshTransform == null
				|| loadKey != _loadKey_MigrateMeshTransform
				|| targetMeshTransform == null
				|| srcMeshGroup == null)
			{
				//실패
				_loadKey_MigrateMeshTransform = null;

				Debug.LogError("AnyPortrait : Migrating is failed. > Dialog Canceled.");
				return;
			}
			_loadKey_MigrateMeshTransform = null;

			//Debug.Log("AnyPortrait : Migrating Start! [" + srcMeshGroup._name + "] > [" + dstMeshGroup._name + "] (" + targetMeshTransform._nickName + ")");

			//Transform을 복제하자
			bool result = false;
			if(isSingleSelected)
			{
				result = Editor.Controller.MigrateMeshTransformToOtherMeshGroup(targetMeshTransform, null, srcMeshGroup, dstMeshGroup);
			}
			else
			{
				result = Editor.Controller.MigrateMeshTransformToOtherMeshGroup(targetMeshTransform, targetMeshTransforms, srcMeshGroup, dstMeshGroup);
			}
			

			if (!result)
			{
				Debug.LogError("AnyPortrait : Migrating is failed or canceled.");
			}

		}



		

		/// <summary>
		/// 메시 그룹 Right 2 UI : 단축키(F2)를 눌러서 본 이름 바꾸기
		/// </summary>
		/// <param name="paramObj"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RenameBone(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.MeshGroup
				|| Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Bone
				|| MeshGroup == null
				|| Bone == null
				|| SubObjects.NumBone != 1
				)
			{
				return null;
			}

			//대상이 동일한가
			if(SubObjects.SelectedObject != paramObj)
			{
				return null;
			}

			//이름바꾸기 칸으로 포커스를 옮기자
			apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__BoneName);

			return apHotKey.HotKeyResult.MakeResult();
		}

		

		/// <summary>
		/// 메시 그룹 Right 2 UI : 여러 경우에서의 본 선택 다이얼로그의 이벤트 콜백
		/// </summary>
		private void OnDialogSelectBone(bool isSuccess, object loadKey, bool isNullBone, apBone selectedBone, apBone targetBone, apDialog_SelectLinkedBone.REQUEST_TYPE requestType)
		{
			if (_loadKey_SelectBone != loadKey)
			{
				_loadKey_SelectBone = null;
				return;
			}
			if (!isSuccess)
			{
				_loadKey_SelectBone = null;
				return;
			}


			_loadKey_SelectBone = null;
			switch (requestType)
			{
				case apDialog_SelectLinkedBone.REQUEST_TYPE.AttachChild:
					{
						Editor.Controller.AttachBoneToChild(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.ChangeParent:
					{
						Editor.Controller.SetBoneAsParent(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKTarget:
					{
						Editor.Controller.SetBoneAsIKTarget(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKPositionControllerEffector:
					{
						Editor.Controller.SetBoneAsIKPositionControllerEffector(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKLookAtControllerEffector:
					{
						Editor.Controller.SetBoneAsIKLookAtControllerEffectorOrStartBone(targetBone, selectedBone, true);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKLookAtControllerStartBone:
					{
						Editor.Controller.SetBoneAsIKLookAtControllerEffectorOrStartBone(targetBone, selectedBone, false);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.Mirror:
					{
						Editor.Controller.SetBoneAsMirror(targetBone, selectedBone);
					}
					break;

				case apDialog_SelectLinkedBone.REQUEST_TYPE.Select1LevelChildToSnap:
					{
						Editor.Controller.SnapBoneEndToChildBone(targetBone, selectedBone, MeshGroup);
					}
					break;
			}
		}


		
		/// <summary>
		/// 메시 그룹 Right 2 UI : IK 컨트롤러를 위한 컨트롤 파라미터 선택 결과 이벤트
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="loadKey"></param>
		/// <param name="resultControlParam"></param>
		/// <param name="savedObject"></param>
		private void OnSelectControlParamForIKController(bool isSuccess, object loadKey, apControlParam resultControlParam, object savedObject)
		{
			if (!isSuccess || savedObject == null)
			{
				_loadKey_SelectControlParamForIKController = null;
				return;
			}
			if (_loadKey_SelectControlParamForIKController != loadKey)
			{
				return;
			}
			if (savedObject is apBone)
			{
				apBone targetBone = savedObject as apBone;
				if (targetBone == null)
				{
					return;
				}
				if (resultControlParam != null)
				{
					targetBone._IKController._weightControlParam = resultControlParam;
					targetBone._IKController._weightControlParamID = resultControlParam._uniqueID;
				}
				else
				{
					targetBone._IKController._weightControlParam = null;
					targetBone._IKController._weightControlParamID = -1;
				}

			}
		}



		/// <summary>
		/// 메시 그룹 Right 2 UI : 본 복제를 위한 선택 이벤트
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="targetBone"></param>
		/// <param name="loadKey"></param>
		/// <param name="offsetX"></param>
		/// <param name="offsetY"></param>
		/// <param name="isDuplicateChildren"></param>
		private void OnDuplicateBoneResult(bool isSuccess, apBone targetBone, object loadKey, float offsetX, float offsetY, bool isDuplicateChildren)
		{
			if (!isSuccess
				|| _loadKey_DuplicateBone != loadKey
				|| targetBone != Bone
				|| targetBone == null
				|| SelectionType != SELECTION_TYPE.MeshGroup
				|| Bone == null)
			{
				_loadKey_DuplicateBone = null;
				return;
			}
			_loadKey_DuplicateBone = null;

			//복제 함수를 호출하자.
			Editor.Controller.DuplicateBone(MeshGroup, targetBone, offsetX, offsetY, isDuplicateChildren);
		}


		

		/// <summary>
		/// 단축키를 이용하여 본을 삭제하자.
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RemoveBone(object paramObject)
		{
			if (paramObject != Bone)
			{
				return null;
			}

			if (SelectionType != SELECTION_TYPE.MeshGroup ||
				Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Bone ||
				Bone == null)
			{
				return null;
			}

			SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);

			//본 개수에 따라서 삭제되는 것이 다르다. (20.9.16)
			int nBones = _subObjects.AllBones.Count;
			if (nBones <= 1)
			{
				apBone curBone = Bone;

				//단일 삭제 (기존 코드)
				string strRemoveBoneText = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																curBone,
																5,
																Editor.GetTextFormat(TEXT.RemoveBone_Body, curBone._name),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																);

				int btnIndex = EditorUtility.DisplayDialogComplex(
																	Editor.GetText(TEXT.RemoveBone_Title),
																	strRemoveBoneText,
																	Editor.GetText(TEXT.Remove),
																	Editor.GetText(TEXT.RemoveBone_RemoveAllChildren),
																	Editor.GetText(TEXT.Cancel));
				if (btnIndex == 0)
				{
					//Bone을 삭제한다.
					Editor.Controller.RemoveBone(curBone, false);
				}
				else if (btnIndex == 1)
				{
					//Bone과 자식을 모두 삭제한다.
					Editor.Controller.RemoveBone(curBone, true);
				}
			}
			else
			{
				//다중 삭제
				int btnIndex = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.RemoveBone_Title),
																		Editor.GetTextFormat(TEXT.DLG_RemoveBone_Multiple_Body, nBones),
																		Editor.GetText(TEXT.Remove),
																		Editor.GetText(TEXT.RemoveBone_RemoveAllChildren),
																		Editor.GetText(TEXT.Cancel));

				if (btnIndex == 0)
				{
					//Bone을 삭제한다.
					//추가 20.9.15 : 다중 삭제
					Editor.Controller.RemoveBones(_subObjects.AllBones, MeshGroup, false);
				}
				else if (btnIndex == 1)
				{
					//Bone과 자식을 모두 삭제한다.
					//추가 20.9.15 : 다중 삭제 : AllGizmoBones를 이용해야한다.
					Editor.Controller.RemoveBones(_subObjects.AllGizmoBones, MeshGroup, true);
				}
			}

			
			SelectBone(null, MULTI_SELECT.Main);
			Editor.RefreshControllerAndHierarchy(false);
			Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(MeshGroup));

			return apHotKey.HotKeyResult.MakeResult();
		}



		/// <summary>
		/// 메시 그룹 Right 2 UI에서 포즈 Import 결과
		/// </summary>
		private void OnRetargetSinglePoseImportMod(	object loadKey, bool isSuccess, apRetarget resultRetarget,
													apMeshGroup targetMeshGroup,
													apModifierBase targetModifier, apModifierParamSet targetParamSet,
													apDialog_RetargetSinglePoseImport.IMPORT_METHOD importMethod)
		{
			if(loadKey != _loadKey_SinglePoseImport_Mod || !isSuccess)
			{
				_loadKey_SinglePoseImport_Mod = null;
				return;
			}

			_loadKey_SinglePoseImport_Mod = null;

			//Import 처리를 하자
			Editor.Controller.ImportBonePoseFromRetargetSinglePoseFileToModifier(targetMeshGroup, resultRetarget, targetModifier, targetParamSet, importMethod);
		}


		
		/// <summary>
		/// 추가 3.22 : Make Key가 아닌 Add Control Parameter 기능에 의한 
		/// "컨트롤 파라미터 > 모디파이어"를 연결할때 컨트롤 파라미터 선택 결과 이벤트
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="loadKey"></param>
		/// <param name="resultControlParam"></param>
		/// <param name="savedObject"></param>
		public void OnAddControlParameterToModifierAsParamSetGroup(bool isSuccess, object loadKey, apControlParam resultControlParam, object savedObject)
		{
			if(loadKey != _loadKey_AddControlParam 
				|| !isSuccess
				|| resultControlParam == null)
			{
				_loadKey_AddControlParam = null;
				return;
			}
			
			_loadKey_AddControlParam = null;
			
			//현재 모디파이어 메뉴가 아니거나, 저장된 모디파이어가 아니라면 종료
			if(SelectionType != SELECTION_TYPE.MeshGroup ||
				MeshGroup == null ||
				Modifier == null)
			{
				return;
			}

			object curObject = Modifier;
			if (savedObject != curObject)
			{
				return;
			}

			//인자 2 : 기본값으로 생성하려면 False를 입력 (true는 현재값으로 생성)
			//인자 3 : 현재 메시나 본을 바로 등록하려면 True, 여기서는 False
			Editor.Controller.AddControlParamToModifier(resultControlParam, false, false);
		}



		// Rigging 툴 단축키들		

		/// <summary>
		/// Rigging 툴 단축키 : 가중치 증감 (0.5)
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingValueChanged_05(object paramObject)
		{
			if(!(paramObject is bool)) { return null; }
			bool isIncrease = (bool)paramObject;

			if(Bone == null) { return null; }
			int CALCULATE_ADD = 1;

			if(isIncrease)	{ Editor.Controller.SetBoneWeight(0.05f, CALCULATE_ADD); }
			else			{ Editor.Controller.SetBoneWeight(-0.05f, CALCULATE_ADD); }

			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// Rigging 툴 단축키 : 가중치 증감 (0.2)
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingValueChanged_02(object paramObject)
		{
			if(!(paramObject is bool)) { return null; }
			bool isIncrease = (bool)paramObject;

			if(Bone == null) { return null; }
			int CALCULATE_ADD = 1;

			if(isIncrease)	{ Editor.Controller.SetBoneWeight(0.02f, CALCULATE_ADD); }
			else			{ Editor.Controller.SetBoneWeight(-0.02f, CALCULATE_ADD); }

			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}

		
		
		



		//Rigging의 브러시 모드에서의 단축키들
		/// <summary>
		/// Rigging 툴 단축키 : 브러시 크기 변경
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingBrushSizeChanged(object paramObject)
		{
			if(!(paramObject is bool)) { return null; }
			bool isSizeUp = (bool)paramObject;

			//이전
			//if (isSizeUp)	{ _rigEdit_BrushRadius = Mathf.Clamp(_rigEdit_BrushRadius + 10, 1, apGizmos.MAX_BRUSH_RADIUS); }
			//else			{ _rigEdit_BrushRadius = Mathf.Clamp(_rigEdit_BrushRadius - 10, 1, apGizmos.MAX_BRUSH_RADIUS); }

			//변경 22.1.9 : 인덱스 증감
			if (isSizeUp)	{ _rigEdit_BrushRadius_Index = Mathf.Clamp(_rigEdit_BrushRadius_Index + 1, 0, apGizmos.MAX_BRUSH_INDEX); }
			else			{ _rigEdit_BrushRadius_Index = Mathf.Clamp(_rigEdit_BrushRadius_Index - 1, 0, apGizmos.MAX_BRUSH_INDEX); }
			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}
		
		/// <summary>
		/// Rigging 툴 단축키 : 모드 변경 [Add]
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingBrushMode_Add(object paramObject)
		{
			if(_rigEdit_BrushToolMode != RIGGING_BRUSH_TOOL_MODE.Add)	{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.Add; Editor.Gizmos.StartBrush(); }
			else														{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None; Editor.Gizmos.EndBrush(); }
			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}

		/// <summary>
		/// Rigging 툴 단축키 : 모드 변경 [Multiply]
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingBrushMode_Multiply(object paramObject)
		{
			if(_rigEdit_BrushToolMode != RIGGING_BRUSH_TOOL_MODE.Multiply)	{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.Multiply; Editor.Gizmos.StartBrush(); }
			else														{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None; Editor.Gizmos.EndBrush(); }
			apEditorUtil.ReleaseGUIFocus();
			
			return apHotKey.HotKeyResult.MakeResult();
		}
		


		/// <summary>
		/// Rigging 툴 단축키 : 모드 변경 [Blur]
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingBrushMode_Blur(object paramObject)
		{
			if(_rigEdit_BrushToolMode != RIGGING_BRUSH_TOOL_MODE.Blur)	{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.Blur; Editor.Gizmos.StartBrush(); }
			else														{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None; Editor.Gizmos.EndBrush(); }
			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// Rigging 툴 단축키 : 브러시 세기 변경
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_RiggingBrushIntensity(object paramObject)
		{
			if(!(paramObject is bool)) { return null; }
			bool isIncrease = (bool)paramObject;

			switch (_rigEdit_BrushToolMode)
			{
				case RIGGING_BRUSH_TOOL_MODE.Add:
					if(isIncrease)	{ _rigEdit_BrushIntensity_Add = Mathf.Clamp(_rigEdit_BrushIntensity_Add + 0.1f, -1, 1); }
					else			{ _rigEdit_BrushIntensity_Add = Mathf.Clamp(_rigEdit_BrushIntensity_Add - 0.1f, -1, 1); }
					break;
				case RIGGING_BRUSH_TOOL_MODE.Multiply:
					if(isIncrease)	{ _rigEdit_BrushIntensity_Multiply = Mathf.Clamp(_rigEdit_BrushIntensity_Multiply + 0.05f, 0.5f, 1.5f); }
					else			{ _rigEdit_BrushIntensity_Multiply = Mathf.Clamp(_rigEdit_BrushIntensity_Multiply - 0.05f, 0.5f, 1.5f); }
					break;
				case RIGGING_BRUSH_TOOL_MODE.Blur:
					if(isIncrease)	{ _rigEdit_BrushIntensity_Blur = Mathf.Clamp(_rigEdit_BrushIntensity_Blur + 10, 0, 100); }
					else			{ _rigEdit_BrushIntensity_Blur = Mathf.Clamp(_rigEdit_BrushIntensity_Blur - 10, 0, 100); }
					break;
			}
			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}

		
		/// <summary>
		/// Auto-Rig를 위한 본 선택 다이얼로그 결과 이벤트
		/// </summary>
		/// <param name="isSuccess"></param>
		/// <param name="loadKey"></param>
		/// <param name="selectedBones"></param>
		private void OnSelectBonesForAutoRig(bool isSuccess, object loadKey, List<apBone> selectedBones)
		{
			//TODO
			if(!isSuccess || _loadKey_SelectBonesForAutoRig != loadKey)
			{
				_loadKey_SelectBonesForAutoRig = null;
				return;
			}
			_loadKey_SelectBonesForAutoRig = null;
			if(selectedBones != null && selectedBones.Count > 0)
			{
				Editor.Controller.SetBoneAutoRig(selectedBones);
			}
			
		}


		// Physics Modifer의 이벤트들

		/// <summary>
		/// Physic Modifier에서 Gravity/Wind를 Control Param에 연결할 때, Dialog를 열어서 선택하도록 한다.
		/// </summary>
		public void OnSelectControlParamToPhysicGravity(bool isSuccess, object loadKey, apControlParam resultControlParam, object savedObject)
		{
			//Debug.Log("Select Control Param : OnSelectControlParamToPhysicGravity (" + isSuccess + ")");
			if (_loadKey_SelectControlParamToPhyGravity != loadKey || !isSuccess)
			{
				//Debug.LogError("AnyPortrait : Wrong loadKey");
				_loadKey_SelectControlParamToPhyGravity = null;
				return;
			}

			_loadKey_SelectControlParamToPhyGravity = null;
			if (Modifier == null
				|| (Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				|| ModMesh_Main == null)
			{
				return;
			}
			if (ModMesh_Main.PhysicParam == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
												Editor, 
												Modifier, 
												//ModMesh_Main, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			ModMesh_Main.PhysicParam._gravityControlParam = resultControlParam;
			if (resultControlParam == null)
			{
				ModMesh_Main.PhysicParam._gravityControlParamID = -1;
			}
			else
			{
				ModMesh_Main.PhysicParam._gravityControlParamID = resultControlParam._uniqueID;
			}
		}

		
		/// <summary>
		/// Wind 효과를 적용하기 위한 컨트롤 파라미터 선택 결과 이벤트
		/// </summary>
		public void OnSelectControlParamToPhysicWind(bool isSuccess, object loadKey, apControlParam resultControlParam, object savedObject)
		{
			if (_loadKey_SelectControlParamToPhyWind != loadKey || !isSuccess)
			{
				_loadKey_SelectControlParamToPhyWind = null;
				return;
			}

			_loadKey_SelectControlParamToPhyWind = null;
			if (Modifier == null
				|| (Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				|| ModMesh_Main == null)
			{
				return;
			}
			if (ModMesh_Main.PhysicParam == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
												Editor, 
												Modifier, 
												//ModMesh_Main, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			ModMesh_Main.PhysicParam._windControlParam = resultControlParam;
			if (resultControlParam == null)
			{
				ModMesh_Main.PhysicParam._windControlParamID = -1;
			}
			else
			{
				ModMesh_Main.PhysicParam._windControlParamID = resultControlParam._uniqueID;
			}
		}

		
		/// <summary>
		/// Physics Preset 선택 결과 이벤트
		/// </summary>
		private void OnSelectPhysicsPreset(bool isSuccess, object loadKey, apPhysicsPresetUnit physicsUnit, apModifiedMesh targetModMesh)
		{
			if (!isSuccess || physicsUnit == null || targetModMesh == null || loadKey != _loadKey_SelectPhysicsParam || targetModMesh != ModMesh_Main)
			{
				_loadKey_SelectPhysicsParam = null;
				return;
			}
			_loadKey_SelectPhysicsParam = null;
			if (targetModMesh.PhysicParam == null || SelectionType != SELECTION_TYPE.MeshGroup)
			{
				return;
			}
			//값 복사를 해주자
			
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SetPhysicsProperty, 
												Editor, 
												Modifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			apPhysicsMeshParam physicsMeshParam = targetModMesh.PhysicParam;

			physicsMeshParam._presetID = physicsUnit._uniqueID;
			physicsMeshParam._moveRange = physicsUnit._moveRange;

			physicsMeshParam._isRestrictMoveRange = physicsUnit._isRestrictMoveRange;
			physicsMeshParam._isRestrictStretchRange = physicsUnit._isRestrictStretchRange;

			//physicsMeshParam._stretchRangeRatio_Min = physicsUnit._stretchRange_Min;
			physicsMeshParam._stretchRangeRatio_Max = physicsUnit._stretchRange_Max;
			physicsMeshParam._stretchK = physicsUnit._stretchK;
			physicsMeshParam._inertiaK = physicsUnit._inertiaK;
			physicsMeshParam._damping = physicsUnit._damping;
			physicsMeshParam._mass = physicsUnit._mass;

			physicsMeshParam._gravityConstValue = physicsUnit._gravityConstValue;
			physicsMeshParam._windConstValue = physicsUnit._windConstValue;
			physicsMeshParam._windRandomRange = physicsUnit._windRandomRange;

			physicsMeshParam._airDrag = physicsUnit._airDrag;
			physicsMeshParam._viscosity = physicsUnit._viscosity;
			physicsMeshParam._restoring = physicsUnit._restoring;

		}






		//----------------------------------------------------------------------
		// 애니메이션 UI에서의 이벤트
		//----------------------------------------------------------------------
		
		/// <summary>
		/// 애니메이션 이름 변경 단축키 콜백
		/// </summary>
		private apHotKey.HotKeyResult OnHotKey_RenameAnimClip(object paramObject)
		{
			if(SelectionType != SELECTION_TYPE.Animation
				|| AnimClip == null
				|| paramObject != (object)AnimClip)
			{
				return null;
			}

			//이름바꾸기 칸으로 포커스를 옮기자
			apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__AnimClipName);

			return apHotKey.HotKeyResult.MakeResult();
		}

		

		/// <summary>
		/// 애니메이션에 연결된 메시 그룹을 선택하는 다이얼로그의 콜백
		/// </summary>
		private void OnSelectMeshGroupToAnimClip(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip)
		{
			if (!isSuccess || _loadKey_SelectMeshGroupToAnimClip != loadKey
				|| meshGroup == null || _animClip != targetAnimClip)
			{
				_loadKey_SelectMeshGroupToAnimClip = null;
				return;
			}

			_loadKey_SelectMeshGroupToAnimClip = null;

			//추가 3.29 : 누군가의 자식 메시 그룹인 경우
			if (meshGroup._parentMeshGroup != null)
			{
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DLG_ChildMeshGroupAndAnimClip_Title),
																Editor.GetText(TEXT.DLG_ChildMeshGroupAndAnimClip_Body),
																Editor.GetText(TEXT.Okay),
																Editor.GetText(TEXT.Cancel)
																);

				if (!isResult)
				{
					return;
				}
			}

			if (_animClip._targetMeshGroup != null)
			{
				if (_animClip._targetMeshGroup == meshGroup)
				{
					//바뀐게 없다 => Pass
					return;
				}

				//추가 19.8.20 : 데이터가 유지될 수도 있다.
				//조건 : 대상이 기존의 메시 그룹의 최상위 부모여야 한다. ( 그 대상은 Parent가 없다. )
				if (meshGroup._parentMeshGroup == null && IsRootParentMeshGroup(meshGroup, _animClip._targetMeshGroup))
				{
					int iBtn = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_MigrateAnimationDataToParentMeshGroup_Title),
																	Editor.GetText(TEXT.DLG_MigrateAnimationDataToParentMeshGroup_Body),
																	Editor.GetText(TEXT.Keep_data),
																	Editor.GetText(TEXT.Clear_data),
																	Editor.GetText(TEXT.Cancel)
																	);

					if (iBtn == 0)
					{
						//데이터 유지 > 별도의 함수
						Editor.Controller.MigrateAnimClipToMeshGroup(_animClip, meshGroup);
						return;
					}
					else if (iBtn == 1)
					{
						//데이터 삭제 > 리턴없이 그냥 진행
					}
					else
					{
						//취소
						return;
					}
				}
				else
				{
					//그 외의 경우
					//bool isResult = EditorUtility.DisplayDialog("Is Change Mesh Group", "Is Change Mesh Group?", "Change", "Cancel");
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimClipMeshGroupChanged_Title),
																	Editor.GetText(TEXT.AnimClipMeshGroupChanged_Body),
																	Editor.GetText(TEXT.Okay),
																	Editor.GetText(TEXT.Cancel)
																	);
					if (!isResult)
					{
						//기존 것에서 변경을 하지 않는다 => Pass
						return;
					}
				}


			}

			//Undo
			//apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_SetMeshGroup, Editor, Editor._portrait, meshGroup, null, false);

			//변경 20.3.19 : 되돌아갈때를 위해서 원래 두개의 메시 그룹과 해당 모든 모디파이어를 저장해야하지만, 그냥 다 하자.
			apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(	apUndoGroupData.ACTION.Anim_SetMeshGroup, 
																		Editor, 
																		Editor._portrait, 
																		//meshGroup, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);


			//기존의 Timeline이 있다면 다 날리자

			//_isAnimAutoKey = false;
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			_isAnimSelectionLock = false;

			SelectAnimTimeline(null, true);
			SelectMeshTF_ForAnimEdit(null, true, false, MULTI_SELECT.Main);//하나만 null을 하면 모두 선택이 취소된다.

			_animClip._timelines.Clear();//<<그냥 클리어
			bool isChanged = _animClip._targetMeshGroup != meshGroup;
			_animClip._targetMeshGroup = meshGroup;
			
			if(meshGroup != null)
			{
				_animClip._targetMeshGroupID = meshGroup._uniqueID;
			}
			else
			{
				_animClip._targetMeshGroupID = -1;
			}
			


			if (meshGroup != null)
			{
				//meshGroup._modifierStack.RefreshAndSort(true);
				meshGroup._modifierStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
															apModifierStack.REFRESH_OPTION_REMOVE.Ignore);//변경 22.12.13
				meshGroup.ResetBoneGUIVisible();
			}
			if (isChanged)
			{
				//MeshGroup 선택 후 초기화
				if (_animClip._targetMeshGroup != null)
				{
					//이전 방식
					//_animClip._targetMeshGroup.SetDirtyToReset();
					//_animClip._targetMeshGroup.SetDirtyToSort();
					////_animClip._targetMeshGroup.SetAllRenderUnitForceUpdate();
					//_animClip._targetMeshGroup.RefreshForce(true);

					//_animClip._targetMeshGroup.LinkModMeshRenderUnits();
					//_animClip._targetMeshGroup.RefreshModifierLink();

					//Debug.LogError("TODO : Check 이거 정상 작동되나");
					apUtil.LinkRefresh.Set_AnimClip(_animClip);
					
					//추가 21.3.20 : RenderUnit들을 삭제하여 재활용없이 새로 생성되게 만든다.
					_animClip._targetMeshGroup.ClearRenderUnits();

					_animClip._targetMeshGroup.SetDirtyToReset();
					_animClip._targetMeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);
					_animClip._targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

					//_animClip._targetMeshGroup._modifierStack.RefreshAndSort(true);
					//변경 22.12.13
					_animClip._targetMeshGroup._modifierStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
																				apModifierStack.REFRESH_OPTION_REMOVE.Ignore);


					//변경 20.4.13 : VisibilityController를 이용하여 작업용 출력 여부를 초기화 및 복구하자
					//동기화 후 옵션에 따라 결정
					Editor.VisiblityController.SyncMeshGroup(_animClip._targetMeshGroup);
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	_animClip._targetMeshGroup, 
																		apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																		apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

					//추가 21.1.32 : Rule 가시성도 초기화
					Editor.Controller.ResetMeshGroupRuleVisibility(_animClip._targetMeshGroup);
					Editor.Controller.ResetVisibilityPresetSync();

					RefreshMeshGroupExEditingFlags(true);
				}


				Editor.Hierarchy_AnimClip.ResetSubUnits();
			}
			Editor.RefreshControllerAndHierarchy(true);

		}


		//두개의 MeshGroup의 관계가 재귀적인 부자관계인지 확인
		private bool IsRootParentMeshGroup(apMeshGroup parentMeshGroup, apMeshGroup childMeshGroup)
		{
			if (childMeshGroup._parentMeshGroup == null)
			{
				return false;
			}
			int cnt = 0;
			apMeshGroup curMeshGroup = childMeshGroup;

			while (true)
			{
				if (curMeshGroup == null) { return false; }
				if (curMeshGroup._parentMeshGroup == null) { return false; }

				if (curMeshGroup._parentMeshGroup == parentMeshGroup)
				{
					//성공
					return true;
				}

				//에러 검출
				if (curMeshGroup._parentMeshGroup == curMeshGroup ||
					curMeshGroup._parentMeshGroup == childMeshGroup)
				{
					return false;
				}

				//1레벨 위로 
				curMeshGroup = curMeshGroup._parentMeshGroup;
				cnt++;

				if (cnt > 100)
				{
					//100레벨이나 위로 올라갔다고? > 에러
					Debug.LogError("AnyPortrait : IsRootParentMeshGroup Error");
					break;
				}
			}
			return false;
		}



		/// <summary>
		/// 애니메이션에서 Dialog 이벤트에 의해서 Timeline을 추가하자
		/// </summary>
		private void OnAddTimelineToAnimClip(bool isSuccess, object loadKey, apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip)
		{
			if (!isSuccess || _loadKey_AddTimelineToAnimClip != loadKey ||
				_animClip != targetAnimClip)
			{
				_loadKey_AddTimelineToAnimClip = null;
				return;
			}

			_loadKey_AddTimelineToAnimClip = null;

			Editor.Controller.AddAnimTimeline(linkType, modifierUniqueID, targetAnimClip);
		}



		
		/// <summary>
		/// 애니메이션 Right 2 UI에서의 포즈 가져오기/내보내기 이벤트
		/// </summary>
		private void OnImportAnimClipRetarget(bool isSuccess, object loadKey, apRetarget retargetData, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, bool isMerge)
		{
			if(!isSuccess 
				|| loadKey != _loadKey_ImportAnimClipRetarget 
				|| retargetData == null 
				|| targetMeshGroup == null
				|| targetAnimClip == null
				|| AnimClip != targetAnimClip
				|| AnimClip == null)
			{
				_loadKey_ImportAnimClipRetarget = null;
				return;
			}

			_loadKey_ImportAnimClipRetarget = null;

			if(AnimClip._targetMeshGroup != targetMeshGroup)
			{
				return;
			}

			//로드를 합시다.
			if(retargetData.IsAnimFileLoaded)
			{
				Editor.Controller.ImportAnimClip(retargetData, targetMeshGroup, targetAnimClip, isMerge);

				//[v1.4.2] Import 후에 기즈모 이벤트를 다시 체크해야한다.
				SetAnimClipGizmoEvent();
			}
		}




		
		/// <summary>
		/// 애니메이션 : 단축키 [A]로 Anim의 Editing 상태를 토글한다.
		/// </summary>
		/// <param name="paramObject"></param>
		public apHotKey.HotKeyResult OnHotKey_AnimEditingToggle(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				return null;
			}


			//v1.4.2 : FFD 모드에서는 토글이 되지 않을 수 있다.
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return null;
			}

			SetAnimEditingToggle();

			return apHotKey.HotKeyResult.MakeResult(ExAnimEditingMode != EX_EDIT.None ? apStringFactory.I.ON : apStringFactory.I.OFF);
			
		}

		/// <summary>
		/// 애니메이션 : 단축키 [S]로 Anim의 SelectionLock을 토글한다.
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		public apHotKey.HotKeyResult OnHotKey_AnimSelectionLockToggle(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _exAnimEditingMode == EX_EDIT.None)
			{
				return null;
			}
			_isAnimSelectionLock = !_isAnimSelectionLock;

			return apHotKey.HotKeyResult.MakeResult(_isAnimSelectionLock ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}

		

		/// <summary>
		/// 애니메이션 UI 단축키 : 키프레임 추가
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKey_AnimAddKeyframe(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null)
			{
				return null;
			}

			//v1.4.2 : FFD 모드에서는 키프레임 추가가 제한될 수 있다.
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return null;
			}

			//이전 : 단일 타임라인 레이어에 대해서만 키 추가

			//변경 20.6.12 : 선택된 모든 타임라인 레이어에 대해서 키프레임을 생성한다.
			if(AnimTimelineLayer_Main != null)
			{
				if(AnimTimelineLayers_All.Count > 1)
				{
					//1. 여러개의 타임라인 레이어에 대해서 키프레임 추가
					List<apAnimKeyframe> addedKeyframes = Editor.Controller.AddAnimKeyframes(AnimClip.CurFrame, AnimClip, AnimTimelineLayers_All, true);
					if(addedKeyframes != null && addedKeyframes.Count == 0)
					{
						//프레임을 이동하자
						_animClip.SetFrame_Editor(AnimClip.CurFrame);
						SelectAnimMultipleKeyframes(addedKeyframes, apGizmos.SELECT_TYPE.New, true);
					}
				}
				else
				{
					//2. 키프레임 한개 추가
					apAnimKeyframe addedKeyframe = Editor.Controller.AddAnimKeyframe(AnimClip.CurFrame, AnimTimelineLayer_Main, true);
					if (addedKeyframe != null)
					{
						//프레임을 이동하자
						_animClip.SetFrame_Editor(addedKeyframe._frameIndex);
						SelectAnimKeyframe(addedKeyframe, true, apGizmos.SELECT_TYPE.New);
					}
				}

				//추가 : 자동 스크롤
				AutoSelectAnimTimelineLayer(true, false);
				Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.

				return apHotKey.HotKeyResult.MakeResult();
			}

			return null;
		}


		/// <summary>
		/// 애니메이션 UI 단축키 : 키프레임 이동
		/// </summary>
		private apHotKey.HotKeyResult OnHotKey_AnimMoveFrame(object paramObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation 
				|| _animClip == null 
				)
			{
				//Debug.LogError("애니메이션 단축키 처리 실패");
				return null;
			}

			bool isValidParam = paramObject is int;

			if(!isValidParam)
			{
				//유효하지 않은 단축키
				return null;
			}

			//[v1.4.2] 모든 애니메이션 제어 단축키를 처리하기 위해서는 FFD가 해제되어야 한다.
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return null;
			}


			int iParam = (int)paramObject;

			bool isWorkKeyframeChanged = false;

			switch (iParam)
			{
				case 0:						
					{
						// Play/Pause 전환하기
						if (AnimClip.IsPlaying_Editor)
						{
							// 플레이 -> 일시 정지
							AnimClip.Pause_Editor();
						}
						else
						{
							//마지막 프레임이라면 첫 프레임으로 이동하여 재생한다.
							if (AnimClip.CurFrame == AnimClip.EndFrame)
							{
								AnimClip.SetFrame_Editor(AnimClip.StartFrame);
								Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
							}
							// 일시 정지 -> 플레이
							AnimClip.Play_Editor();
						}

						//Play 전환 여부에 따라서도 WorkKeyframe을 전환한다.
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
						Editor.SetRepaint();
						Editor.Gizmos.SetUpdate();
					}
					break;

				case 1:
					// [Prev Frame] : 이전 프레임으로 이동
					{
						int prevFrame = AnimClip.CurFrame - 1;
						if (prevFrame < AnimClip.StartFrame)
						{
							if (AnimClip.IsLoop)
							{
								prevFrame = AnimClip.EndFrame;
							}
						}
						AnimClip.SetFrame_Editor(prevFrame);
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
					break;

				case 2:
					// [Next Frame] : 다음 프레임으로 이동
					{
						int nextFrame = AnimClip.CurFrame + 1;
						if (nextFrame > AnimClip.EndFrame)
						{
							if (AnimClip.IsLoop)
							{
								nextFrame = AnimClip.StartFrame;
							}
						}
						AnimClip.SetFrame_Editor(nextFrame);
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
					break;

				case 3:
					// [First Frame] : 첫 프레임으로 이동
					{
						AnimClip.SetFrame_Editor(AnimClip.StartFrame);
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
					break;

				case 4:
					// [Last Frame] : 마지막 프레임으로 이동
					{
						AnimClip.SetFrame_Editor(AnimClip.EndFrame);
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
					break;

				case 5:
					// [Prev Keyframe] : 이전 "키프레임"을 찾아서 이동
					{
						//추가 20.12.4
						//이전의 가장 가까운 키프레임을 찾아서 이동한다.
						FindAndMoveToNearestKeyframe(false);
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

						Editor.SetMeshGroupChanged();//강제 업데이트를 해야한다.
					}
					break;

				case 6:
					// [Next Keyframe] : 다음 "키프레임"을 찾아서 이동
					{
						//추가 20.12.4
						//다음의 가장 가까운 키프레임을 찾아서 이동한다.
						FindAndMoveToNearestKeyframe(true);
						AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

						Editor.SetMeshGroupChanged();//강제 업데이트를 해야한다.
					}
					break;

				default:
					Debug.LogError("애니메이션 단축키 처리 실패 - 알 수 없는 코드");
					break;
			}

			return apHotKey.HotKeyResult.MakeResult();
		}

		/// <summary>
		/// 가장 가까운 키프레임 찾기
		/// </summary>
		private void FindAndMoveToNearestKeyframe(bool isMoveToNext)
		{
			if(NumAnimTimelineLayers == 0)
			{
				return;
			}
			int curFrame = AnimClip.CurFrame;
			bool isLoop = AnimClip.IsLoop;
			int animLength = Mathf.Abs(AnimClip.EndFrame - AnimClip.StartFrame) + 1;

			//curFrame과 다른 가장 가까운 키프레임을 찾자
			//Loop인 경우엔.. Length만큼 더 추가
			//Valid가 아닌건 생략한다.
			int resultFrame = curFrame;
			apAnimKeyframe nearestKeyframe = null;

			apAnimTimelineLayer curLayer = null;
			apAnimKeyframe curKeyframe = null;
			int curKeyframePos = 0;
			for (int iLayer = 0; iLayer < NumAnimTimelineLayers; iLayer++)
			{
				curLayer = AnimTimelineLayers_All[iLayer];

				int nKeyframes = (curLayer._keyframes != null) ? curLayer._keyframes.Count : 0;
				for (int iKey = 0; iKey < nKeyframes; iKey++)
				{
					curKeyframe = curLayer._keyframes[iKey];
					curKeyframePos = curKeyframe._frameIndex;

					if(curKeyframePos < AnimClip.StartFrame || curKeyframePos > AnimClip.EndFrame)
					{
						//체크할 필요가 없는 프레임
						continue;
					}

					if(isMoveToNext)
					{
						//다음 프레임으로 이동하려고 할 때

						if (curKeyframePos < curFrame)
						{
							//이전 프레임인뎅? > Loop인 경우에 이동 가능
							//그외에는 생략
							if(isLoop)
							{
								curKeyframePos += animLength;
							}
							else
							{
								continue;
							}
						}

						
						if(curKeyframePos > curFrame)
						{
							//이동 가능하다
							//Min 체크
							if(nearestKeyframe == null || curKeyframePos < resultFrame)
							{
								nearestKeyframe = curKeyframe;
								resultFrame = curKeyframePos;

								if(curKeyframePos == curFrame + 1)
								{
									//이게 바로 다음 프레임이라면 이 레이어에서는 더 체크할 필요가 없다.
									continue;
								}
							}
						}
					}
					else
					{
						//이전 프레임으로 이동하려고 할 때
						if (curKeyframePos > curFrame)
						{
							//다음 프레임인뎅? > Loop인 경우에 이동 가능
							//그외에는 생략
							if(isLoop)
							{
								curKeyframePos -= animLength;
							}
							else
							{
								continue;
							}
						}

						
						if(curKeyframePos < curFrame)
						{
							//이동 가능하다
							//Max 체크
							if(nearestKeyframe == null || curKeyframePos > resultFrame)
							{
								nearestKeyframe = curKeyframe;
								resultFrame = curKeyframePos;

								if(curKeyframePos == curFrame - 1)
								{
									//이게 바로 이전 프레임이라면 이 레이어에서는 더 체크할 필요가 없다.
									continue;
								}
							}
						}
					}
				}
			}
			
			if(nearestKeyframe == null)
			{
				return;
			}

			AnimClip.SetFrame_Editor(nearestKeyframe._frameIndex);
		}


		
		/// <summary>
		/// 애니메이션 UI 단축키 : 키프레임 복사하기
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKey_AnimCopyKeyframes(object paramObject)
		{
			//Debug.Log("TODO : 키프레임 복사");
			if(AnimClip == null || _subAnimKeyframeList == null || _subAnimKeyframeList.Count == 0)
			{
				return null;
			}

			apSnapShotManager.I.Copy_KeyframesOnTimelineUI(AnimClip, _subAnimKeyframeList);

			return apHotKey.HotKeyResult.MakeResult();
		}

		/// <summary>
		/// 애니메이션 UI 단축키 : 키프레임 붙여넣기
		/// </summary>
		/// <param name="paramObject"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKey_AnimPasteKeyframes(object paramObject)
		{
			if(AnimClip == null)
			{
				return null;
			}

			//[v1.4.2] FFD 체크
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return null;
			}

			Editor.Controller.CopyAnimKeyframeFromSnapShot(AnimClip, AnimClip.CurFrame);
			
			return apHotKey.HotKeyResult.MakeResult();
		}




		//--------------------------------------------------------------------
		// 컨트롤 파라미터 UI에서의 이벤트
		//--------------------------------------------------------------------
		
		/// <summary>
		/// 컨트롤 파라미터 이름 변경 이벤트 콜백
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_RenameControlParam(object paramObject)
		{
			if(SelectionType != SELECTION_TYPE.Param
				|| Param == null
				|| paramObject != (object)Param)
			{
				return null;
			}

			//이름바꾸기 칸으로 포커스를 옮기자
			apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__ControlParamName);

			return apHotKey.HotKeyResult.MakeResult();
		}

		/// <summary>
		/// 컨트롤 파라미터 프리셋 변경 결과 이벤트
		/// </summary>
		private void OnSelectControlParamPreset(bool isSuccess, object loadKey, apControlParamPresetUnit controlParamPresetUnit, apControlParam controlParam)
		{
			if (!isSuccess
				|| _loadKey_OnSelectControlParamPreset != loadKey
				|| controlParamPresetUnit == null
				|| controlParam != Param)
			{
				_loadKey_OnSelectControlParamPreset = null;
				return;
			}
			_loadKey_OnSelectControlParamPreset = null;

			//ControlParam에 프리셋 정보를 넣어주자
			Editor.Controller.SetControlParamPreset(controlParam, controlParamPresetUnit);
		}



		//---------------------------------------------------------------------
		// 하단 UI 이벤트
		//---------------------------------------------------------------------
		/// <summary>
		/// 추가 20.12.4 : 단축키로 스크롤 움직이기
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_AnimTimelineUIScroll(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.Animation
				|| AnimClip == null
				|| !(paramObj is int)
				)
			{
				return null;
			}

			int iParam = (int)paramObj;
			if(iParam == 0)
			{
				//스크롤 위로 이동
				_scroll_Timeline.y -= 15;
			}
			else
			{
				//스크롤 아래로 이동
				_scroll_Timeline.y += 15;
			}

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// 애니메이션 하단 UI 중 AutoKey 토글 단축키
		/// </summary>
		/// <param name="paramObj"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_AnimAutoKeyToggle(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.Animation
				|| AnimClip == null)
			{
				return null;
			}

			Editor._isAnimAutoKey = !Editor._isAnimAutoKey;

			return apHotKey.HotKeyResult.MakeResult(Editor._isAnimAutoKey ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}



		/// <summary>
		/// 애니메이션 하단 UI (오른쪽아래) 중 단일 키프레임의 애니메이션 커브 변경 이벤트
		/// </summary>
		/// <param name="paramObj"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_AnimCurve_SingleKeyframe(object paramObj)
		{
			if(SelectionType != SELECTION_TYPE.Animation
				|| AnimClip == null
				|| !(paramObj is int)
				|| AnimKeyframe == null)
			{
				return null;
			}
			_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Curve;
			int iParam = (int)paramObj;
			apAnimCurveResult curveResult = null;

			if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
			{
				curveResult = AnimKeyframe._curveKey._prevCurveResult;
			}
			else
			{
				curveResult = AnimKeyframe._curveKey._nextCurveResult;
			}

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
												Editor, 
												_portrait, 
												//curveResult, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			switch (iParam)
			{
				case 0://Linear
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Linear);
					break;

				case 1://Constant
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Constant);
					break;

				case 2://Smooth - Default
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
					curveResult.SetCurvePreset_Default();
					break;

				case 3://Smooth - AccAndDec
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
					curveResult.SetCurvePreset_Hard();
					break;

				case 4://Smooth - Acc
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
					curveResult.SetCurvePreset_Acc();
					break;

				case 5://Smooth - Dec
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
					curveResult.SetCurvePreset_Dec();
					break;
			}

			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// 애니메이션 우측 하단 UI 중 포즈 가져오기 이벤트
		/// </summary>
		private void OnRetargetSinglePoseImportAnim(object loadKey, bool isSuccess, apRetarget resultRetarget,
																apMeshGroup targetMeshGroup,
																apAnimClip targetAnimClip,
																apAnimTimeline targetTimeline, int targetFrame,
																apDialog_RetargetSinglePoseImport.IMPORT_METHOD importMethod)
		{
			if (loadKey != _loadKey_SinglePoseImport_Anim || !isSuccess)
			{
				_loadKey_SinglePoseImport_Anim = null;
				return;
			}

			_loadKey_SinglePoseImport_Anim = null;

			//Pose Import 처리를 하자
			Editor.Controller.ImportBonePoseFromRetargetSinglePoseFileToAnimClip(targetMeshGroup, resultRetarget, targetAnimClip, targetTimeline, targetFrame, importMethod);

		}



		

		/// <summary>
		/// 추가 20.12.5 : 다중 선택에 대한 커브 변경 단축키 이벤트
		/// </summary>
		/// <param name="paramObj"></param>
		/// <returns></returns>
		private apHotKey.HotKeyResult OnHotKeyEvent_AnimCurve_MultipleKeyframes(object paramObj)
		{
			if (SelectionType != SELECTION_TYPE.Animation
				|| AnimClip == null
				|| !(paramObj is int)
				|| AnimKeyframes == null)
			{
				return null;
			}
			if (AnimKeyframes.Count == 0)
			{
				return null;
			}

			int iCurveType = 0;
			switch (_animPropertyCurveUI_Multi)
			{
				case ANIM_MULTI_PROPERTY_CURVE_UI.Prev: iCurveType = 0; break;
				case ANIM_MULTI_PROPERTY_CURVE_UI.Middle: iCurveType = 1; break;
				case ANIM_MULTI_PROPERTY_CURVE_UI.Next: iCurveType = 2; break;
			}
			apTimelineCommonCurve.SYNC_STATUS curveSync = _animTimelineCommonCurve.GetSyncStatus(iCurveType);

			bool isCurves_NoKey = (curveSync == apTimelineCommonCurve.SYNC_STATUS.NoKeyframes);
			//bool isCurves_Sync = (curveSync == apTimelineCommonCurve.SYNC_STATUS.Sync);
			bool isCurves_NotSync = (curveSync == apTimelineCommonCurve.SYNC_STATUS.NotSync);

			if (isCurves_NoKey)
			{
				return null;
			}

			apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
												Editor, 
												_portrait, 
												//_animTimelineCommonCurve, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (isCurves_NotSync)
			{	
				_animTimelineCommonCurve.NotSync2SyncStatus(iCurveType);
			}

			int iParam = (int)paramObj;
			//_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Curve;
			
			//apAnimCurveResult curveResult = null;

			//if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
			//{
			//	curveResult = AnimKeyframe._curveKey._prevCurveResult;
			//}
			//else
			//{
			//	curveResult = AnimKeyframe._curveKey._nextCurveResult;
			//}

			
			switch (iParam)
			{
				case 0://Linear
					_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Linear, iCurveType);
					break;

				case 1://Constant
					_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Constant, iCurveType);
					break;

				case 2://Smooth - Default
					_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Smooth, iCurveType);
					_animTimelineCommonCurve.SetCurvePreset_Default(iCurveType);
					break;

				case 3://Smooth - AccAndDec
					_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Smooth, iCurveType);
					_animTimelineCommonCurve.SetCurvePreset_Hard(iCurveType);
					break;

				case 4://Smooth - Acc
					_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Smooth, iCurveType);
					_animTimelineCommonCurve.SetCurvePreset_Acc(iCurveType);
					break;

				case 5://Smooth - Dec
					_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Smooth, iCurveType);
					_animTimelineCommonCurve.SetCurvePreset_Dec(iCurveType);
					break;
			}

			apEditorUtil.ReleaseGUIFocus();

			return apHotKey.HotKeyResult.MakeResult();
		}


		/// <summary>
		/// 애니메이션 UI : 키프레임 삭제 단축키
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyRemoveKeyframes(object paramObject)
		{
			if (SelectionType != SELECTION_TYPE.Animation ||
				AnimClip == null)
			{
				return null;
			}

			if (paramObject is apAnimKeyframe)
			{
				apAnimKeyframe keyframe = paramObject as apAnimKeyframe;

				if (keyframe != null)
				{
					Editor.Controller.RemoveKeyframe(keyframe);
				}
			}
			else if (paramObject is List<apAnimKeyframe>)
			{
				List<apAnimKeyframe> keyframes = paramObject as List<apAnimKeyframe>;
				if (keyframes != null && keyframes.Count > 0)
				{
					Editor.Controller.RemoveKeyframes(keyframes);
				}
			}

			return apHotKey.HotKeyResult.MakeResult();
		}

		//--------------------------------------------------------------------------
	}
}