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
using System.Collections;
using System.Collections.Generic;
using System;
using AnyPortrait;

namespace AnyPortrait
{
	//추가 20.4.12
	// 객체의 "임시"의 보여주기 여부를 중앙에서 컨트롤한다.
	// apEditor의 멤버로 속하며, 옵션에 따라 초기화를 수행한다.
	// 휘발성인 RenderUnit 대신 Mesh/MeshGroup Transform을 기준으로 값을 저장한다. (Bone은 그대로)
	// Visiblity 변화 여부 + 보여주기 요청 결과값을 저장한다.
	
	// 작동 방식
	// > 작업시 : RenderUnit이나 Bone의 Visible을 변경하면, 이 컨트롤러에도 반영한다. (Save)
	// > 작업 해제시, 메뉴 변경시 : 저장된 Visible 데이터를 기반으로 복구한다. (Load)
	// Sync는 객체 동기화만 하며, 이때 RenderUnit도 같이 연결한다.

	//요청 함수는 Save/Load만 하도록 한다. (Clear까지)

	public class apVisiblityController
	{
		// Sub Class
		//--------------------------------------------
		public enum UNIT_TYPE
		{
			MeshTransform,
			MeshGroupTransform,
			Bone
		}

		public enum VISIBLE_TYPE
		{
			None,
			Shown,
			Hidden
		}

		public class VisibleData
		{
			public apMeshGroup _parentMeshGroup = null;

			public UNIT_TYPE _unitType = UNIT_TYPE.MeshTransform;
			public apTransform_Mesh _key_MeshTF = null;
			public apTransform_MeshGroup _key_MeshGroupTF = null;
			public apBone _key_Bone = null;

			public VISIBLE_TYPE _visibleType = VISIBLE_TYPE.None;

			

			private bool _isChecked = false;


			public VisibleData(apMeshGroup parentMeshGroup, apTransform_Mesh meshTransform)
			{
				_parentMeshGroup = parentMeshGroup;
				_unitType = UNIT_TYPE.MeshTransform;
				_key_MeshTF = meshTransform;
				_key_MeshGroupTF = null;
				_key_Bone = null;

				_visibleType = VISIBLE_TYPE.None;
			}

			public VisibleData(apMeshGroup parentMeshGroup, apTransform_MeshGroup meshGroupTransform)
			{
				_parentMeshGroup = parentMeshGroup;
				_unitType = UNIT_TYPE.MeshGroupTransform;
				_key_MeshTF = null;
				_key_MeshGroupTF = meshGroupTransform;
				_key_Bone = null;
				_visibleType = VISIBLE_TYPE.None;
			}

			public VisibleData(apMeshGroup parentMeshGroup, apBone bone)
			{
				_parentMeshGroup = parentMeshGroup;
				_unitType = UNIT_TYPE.Bone;
				_key_MeshTF = null;
				_key_MeshGroupTF = null;
				_key_Bone = bone;

				_visibleType = VISIBLE_TYPE.None;
			}	

			public void SetVisibility(VISIBLE_TYPE visibleType)
			{
				_visibleType = visibleType;
			}

			public void ReadyToCheck()
			{
				_isChecked = false;
			}
			public void SetChecked()
			{
				_isChecked = true;
			}
			public bool IsValid
			{
				get { return _isChecked && (_key_MeshTF != null || _key_MeshGroupTF != null || _key_Bone != null); }
			}
		}


		public class VisibleGroupData
		{
			public apMeshGroup _parentMeshGroup = null;

			public List<VisibleData> _dataList_MeshTF = null;
			public List<VisibleData> _dataList_MeshGroupTF = null;
			public List<VisibleData> _dataList_Bone = null;

			public Dictionary<apTransform_Mesh, VisibleData> _meshTF_2_Data = null;
			public Dictionary<apTransform_MeshGroup, VisibleData> _meshGroupTF_2_Data = null;
			public Dictionary<apBone, VisibleData> _bone_2_Data = null;

			
			//Init
			public VisibleGroupData(apMeshGroup parentMeshGroup)
			{
				_parentMeshGroup = parentMeshGroup;

				Clear();
			}


			public void Clear()
			{
				if(_dataList_MeshTF == null)
				{
					_dataList_MeshTF = new List<VisibleData>();
				}
				if(_dataList_MeshGroupTF == null)
				{
					_dataList_MeshGroupTF = new List<VisibleData>();
				}
				if(_dataList_Bone == null)
				{
					_dataList_Bone = new List<VisibleData>();
				}
				
				if(_meshTF_2_Data == null)
				{
					_meshTF_2_Data = new Dictionary<apTransform_Mesh, VisibleData>();
				}
				if(_meshGroupTF_2_Data == null)
				{
					_meshGroupTF_2_Data = new Dictionary<apTransform_MeshGroup, VisibleData>();
				}
				if(_bone_2_Data == null)
				{
					_bone_2_Data = new Dictionary<apBone, VisibleData>();
				}


				_dataList_MeshTF.Clear();
				_dataList_MeshGroupTF.Clear();
				_dataList_Bone.Clear();

				_meshTF_2_Data.Clear();
				_meshGroupTF_2_Data.Clear();
				_bone_2_Data.Clear();
				
			}

			//Functions

			/// <summary>
			/// 메시 그룹의 객체들과 VisibleData를 동기화하자.
			/// 기존의 데이터가 있다면 그대로 사용하고, 없으면 새로 추가한다.
			/// </summary>
			public void SyncObjects()
			{
				if(_parentMeshGroup == null)
				{
					return;
				}

				//일단 삭제할게 있는지 플래그를 걸자
				for (int i = 0; i < _dataList_MeshTF.Count; i++)
				{
					_dataList_MeshTF[i].ReadyToCheck();
				}
				for (int i = 0; i < _dataList_MeshGroupTF.Count; i++)
				{
					_dataList_MeshGroupTF[i].ReadyToCheck();
				}
				for (int i = 0; i < _dataList_Bone.Count; i++)
				{
					_dataList_Bone[i].ReadyToCheck();
				}

				//만약, 개수가 하나도 없다면, 동기화시 중복 체크를 하지 않는다.
				int nData_MeshTF = _dataList_MeshTF.Count;
				int nData_MeshGroupTF = _dataList_MeshGroupTF.Count;
				int nData_Bone = _dataList_Bone.Count;


				//재귀함수를 이용해서 모든 Mesh/MeshGroup Transform / Bone에 대해서 동기화를 하자.
				FindAndAddData(_parentMeshGroup, nData_MeshTF > 0 || nData_MeshGroupTF > 0 || nData_Bone > 0);


				//Check가 안된 기존 데이터는 모두 삭제한다.
				if (nData_MeshTF > 0 || nData_MeshGroupTF > 0 || nData_Bone > 0)
				{
					int nRemove_MeshTF = _dataList_MeshTF.RemoveAll(delegate(VisibleData a)
					{
						return !a.IsValid;
					});

					int nRemove_MeshGroupTF = _dataList_MeshGroupTF.RemoveAll(delegate(VisibleData a)
					{
						return !a.IsValid;
					});

					int nRemove_Bone = _dataList_Bone.RemoveAll(delegate(VisibleData a)
					{
						return !a.IsValid;
					});

					//하나라도 삭제된게 있다면
					//매핑을 다시 하자
					VisibleData curData = null;

					if(nRemove_MeshTF > 0)
					{
						_meshTF_2_Data.Clear();
						for (int i = 0; i < _dataList_MeshTF.Count; i++)
						{
							curData = _dataList_MeshTF[i];
							_meshTF_2_Data.Add(curData._key_MeshTF, curData);
						}
					}

					if(nRemove_MeshGroupTF > 0)
					{
						_meshGroupTF_2_Data.Clear();
						for (int i = 0; i < _dataList_MeshGroupTF.Count; i++)
						{
							curData = _dataList_MeshGroupTF[i];
							_meshGroupTF_2_Data.Add(curData._key_MeshGroupTF, curData);
						}
					}

					if(nRemove_Bone > 0)
					{
						_bone_2_Data.Clear();
						for (int i = 0; i < _dataList_Bone.Count; i++)
						{
							curData = _dataList_Bone[i];
							_bone_2_Data.Add(curData._key_Bone, curData);
						}
					}

				}
			}




			private void FindAndAddData(apMeshGroup targetMeshGroup, bool isCheckPrevData)
			{
				if(targetMeshGroup == null)
				{
					return;
				}
				int nMeshTF = (targetMeshGroup._childMeshTransforms != null) ? targetMeshGroup._childMeshTransforms.Count : 0;
				int nMeshGroupTF = (targetMeshGroup._childMeshGroupTransforms != null) ? targetMeshGroup._childMeshGroupTransforms.Count : 0;
				int nBones = (targetMeshGroup._boneList_All != null) ? targetMeshGroup._boneList_All.Count : 0;

				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;
				apBone curBone = null;

				for (int i = 0; i < nMeshTF; i++)
				{
					curMeshTF = targetMeshGroup._childMeshTransforms[i];

					if(isCheckPrevData)
					{
						if(_meshTF_2_Data.ContainsKey(curMeshTF))
						{
							//이미 등록되었다. > 삭제 안되게 체크만 하자
							_meshTF_2_Data[curMeshTF].SetChecked();
							continue;
						}
					}

					//Mesh Transform의 Visibility를 새로 만들어 등록한다.
					VisibleData newData = new VisibleData(_parentMeshGroup, curMeshTF);
					newData.SetChecked();

					_meshTF_2_Data.Add(curMeshTF, newData);
					_dataList_MeshTF.Add(newData);
				}


				
				for (int i = 0; i < nBones; i++)
				{
					curBone = targetMeshGroup._boneList_All[i];

					if(isCheckPrevData)
					{
						if(_bone_2_Data.ContainsKey(curBone))
						{
							//이미 등록되었다. > 삭제 안되게 체크만 하자
							_bone_2_Data[curBone].SetChecked();
							continue;
						}
					}

					//Bne의 Visibility를 새로 만들어 등록한다.
					VisibleData newData = new VisibleData(_parentMeshGroup, curBone);
					newData.SetChecked();

					_bone_2_Data.Add(curBone, newData);
					_dataList_Bone.Add(newData);
				}


				for (int i = 0; i < nMeshGroupTF; i++)
				{
					curMeshGroupTF = targetMeshGroup._childMeshGroupTransforms[i];

					if(curMeshGroupTF != null
						&& curMeshGroupTF._meshGroup != null
						&& curMeshGroupTF._meshGroup != _parentMeshGroup
						&& curMeshGroupTF._meshGroup != targetMeshGroup)
					{
						//하위 메시 그룹에 대해서 처리를 해보자
						FindAndAddData(curMeshGroupTF._meshGroup, isCheckPrevData);
					}


					if(isCheckPrevData)
					{
						if(_meshGroupTF_2_Data.ContainsKey(curMeshGroupTF))
						{
							//이미 등록되었다. > 삭제 안되게 체크만 하자
							_meshGroupTF_2_Data[curMeshGroupTF].SetChecked();
							continue;
						}
					}

					//MeshGroup Transform의 Visibility를 새로 만들어 등록한다.
					VisibleData newData = new VisibleData(_parentMeshGroup, curMeshGroupTF);
					newData.SetChecked();

					_meshGroupTF_2_Data.Add(curMeshGroupTF, newData);
					_dataList_MeshGroupTF.Add(newData);
				}
			}




			/// <summary>
			/// 모든 Visibility를 None으로 초기화한다.
			/// </summary>
			public void ResetVisibility()
			{
				VisibleData curData = null;
				for (int i = 0; i < _dataList_MeshTF.Count; i++)
				{
					curData = _dataList_MeshTF[i];
					curData.SetVisibility(VISIBLE_TYPE.None);
				}

				for (int i = 0; i < _dataList_MeshGroupTF.Count; i++)
				{
					curData = _dataList_MeshGroupTF[i];
					curData.SetVisibility(VISIBLE_TYPE.None);
				}

				for (int i = 0; i < _dataList_Bone.Count; i++)
				{
					curData = _dataList_Bone[i];
					curData.SetVisibility(VISIBLE_TYPE.None);
				}
			}

			// Get
			//-----------------------------------------------------
			/// <summary>
			/// Render Unit의 Visible 데이터를 가져온다. 없다면 새로 생성하고 리턴한다.
			/// </summary>
			/// <param name="renderUnit"></param>
			/// <returns></returns>
			public VisibleData GetData(apRenderUnit renderUnit)
			{
				if(renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh
					&& renderUnit._meshTransform != null)
				{
					return GetData(renderUnit._meshTransform);
				}
				else if(renderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode
					&& renderUnit._meshGroupTransform != null)
				{
					return GetData(renderUnit._meshGroupTransform);
				}
				else
				{
					return null;
				}
			}

			/// <summary>
			/// Mesh Transform Visible 데이터를 가져온다. 없다면 새로 생성한다.
			/// </summary>
			public VisibleData GetData(apTransform_Mesh meshTransform)
			{
				if(meshTransform == null)
				{
					return null;
				}

				VisibleData resultData = null;
				if(_meshTF_2_Data.ContainsKey(meshTransform))
				{
					//기존 데이터 가져오기
					resultData = _meshTF_2_Data[meshTransform];
				}
				else
				{
					//새로 추가
					resultData = new VisibleData(_parentMeshGroup, meshTransform);

					_meshTF_2_Data.Add(meshTransform, resultData);
					_dataList_MeshTF.Add(resultData);
				}

				return resultData;
			}

			/// <summary>
			/// Mesh Group Transform Visible 데이터를 가져온다. 없다면 새로 생성한다.
			/// </summary>
			public VisibleData GetData(apTransform_MeshGroup meshGroupTransform)
			{
				if(meshGroupTransform == null)
				{
					return null;
				}

				VisibleData resultData = null;
				if(_meshGroupTF_2_Data.ContainsKey(meshGroupTransform))
				{
					//기존 데이터 가져오기
					resultData = _meshGroupTF_2_Data[meshGroupTransform];
				}
				else
				{
					//새로 추가
					resultData = new VisibleData(_parentMeshGroup, meshGroupTransform);

					_meshGroupTF_2_Data.Add(meshGroupTransform, resultData);
					_dataList_MeshGroupTF.Add(resultData);
				}

				return resultData;
			}


			/// <summary>
			/// Bone Visible 데이터를 가져온다. 없다면 새로 생성한다.
			/// </summary>
			public VisibleData GetData(apBone bone)
			{
				if(bone == null)
				{
					return null;
				}

				VisibleData resultData = null;
				if(_bone_2_Data.ContainsKey(bone))
				{
					//기존 데이터 가져오기
					resultData = _bone_2_Data[bone];
				}
				else
				{
					//새로 추가
					resultData = new VisibleData(_parentMeshGroup, bone);

					_bone_2_Data.Add(bone, resultData);
					_dataList_Bone.Add(resultData);
				}

				return resultData;
			}
		}

		// Members
		//--------------------------------------------
		//- MeshGroup > Transform / Bone 순서로 매핑을 한다. (즉 자식 메시 그룹을 선택하면 다른 가시성을 보인다.)
		//- 모든 값을 저장하는게 아니라, 하나라도 Show <-> Hide가 전환되면 데이터를 생성하여 저장을 한다.
		//- 모두 초기화할 때, 또는 Portrait를 전환할 때 초기화를 한다.
		//- 해당 객체가 저장된게 없다면 일단 None으로 간주한다.
		//- None : 연산된 값을 따른다.
		//- Shown : 연산 결과에 관계없이 보여준다. (Bone은 None과 Shown의 결과가 같다.)
		//- Hidden : 연산 결과에 관계없이 숨긴다.

		private List<VisibleGroupData> _visibleGroupDataList = null;
		private Dictionary<apMeshGroup, VisibleGroupData> _meshGroup_2_GroupData = null;


		// Init
		//--------------------------------------------
		public apVisiblityController()
		{
			ClearAll();
		}

		/// <summary>
		/// 모든 데이터를 초기화한다.
		/// </summary>
		public void ClearAll()
		{
			if(_visibleGroupDataList == null)
			{
				_visibleGroupDataList = new List<VisibleGroupData>();
			}
			_visibleGroupDataList.Clear();
			
			if(_meshGroup_2_GroupData == null)
			{
				_meshGroup_2_GroupData = new Dictionary<apMeshGroup, VisibleGroupData>();
			}
			_meshGroup_2_GroupData.Clear();
		}


		/// <summary>
		/// 특정 메시 그룹에 대한 가시성 데이터를 초기화한다.
		/// 저장된 값이 없다면 리턴한다. (데이터를 추가하지는 않는다.)
		/// 대상의 값을 아예 초기화하는 것이므로 주의할 것
		/// </summary>
		/// <param name="meshGroup"></param>
		public void ClearDataOfMeshGroup(apMeshGroup meshGroup)
		{
			if(meshGroup == null
				|| !_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				//저장된 값이 없어서 리턴할 게 없다.
				return;
			}

			VisibleGroupData groupData = _meshGroup_2_GroupData[meshGroup];
			groupData.Clear();
		}


		// Functions
		//--------------------------------------------
		/// <summary>
		/// 모든 Visibility를 None으로 리셋한다.
		/// 이 함수 이후에는 "동기화"를 실행해야한다. 
		/// </summary>
		public void ResetVisibilityAll()
		{
			VisibleGroupData curGroupData = null;
			for (int i = 0; i < _visibleGroupDataList.Count; i++)
			{
				curGroupData = _visibleGroupDataList[i];
				curGroupData.ResetVisibility();
			}
		}

		/// <summary>
		/// 특정 메시 그룹의 가시성을 모두 초기화한다.
		/// 만약 메시 그룹이 없다면, 이번 기회에 추가를 한다. (추가 후, 현재의 하위 객체에 대한 데이터도 생성한다.)
		/// 이 함수 이후에는 "동기화"를 실행해야한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void ResetVisibilityOfMeshGroup(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return;
			}

			VisibleGroupData groupData = null;

			if(!_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				//저장된 값이 없다. 새로 생성하자.
				groupData = new VisibleGroupData(meshGroup);
				groupData.SyncObjects();//<<추가할 땐 1회 동기화를 한다.

				_visibleGroupDataList.Add(groupData);
				_meshGroup_2_GroupData.Add(meshGroup, groupData);
			}
			else
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
			}

			groupData.ResetVisibility();
		}

		/// <summary>
		/// 메시 그룹에 대한 가시성 정보를 현재 객체와 함께 동기화한다.
		/// 메뉴 변경시, 이 함수를 꼭 호출해주자.
		/// 
		/// </summary>
		/// <param name="meshGroup"></param>
		public VisibleGroupData SyncMeshGroup(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return null;
			}

			VisibleGroupData groupData = null;

			if(!_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				//저장된 값이 없다. 새로 생성하자.
				groupData = new VisibleGroupData(meshGroup);

				_visibleGroupDataList.Add(groupData);
				_meshGroup_2_GroupData.Add(meshGroup, groupData);
			}
			else
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
			}

			//동기화를 하자
			groupData.SyncObjects();

			return groupData;
		}


		//----------------------------------------------------------
		// 저장하기
		//----------------------------------------------------------
		/// <summary>
		/// 메시 그룹의 모든 요소들을 동기화하고 Visibility를 저장한다.
		/// 이 단계에서 꼭 Sync가 발생한다.
		/// 이 함수는 RenderUnit들이 만들어진 다음에 호출해야한다.
		/// Visibility를 리셋한 후 이 함수를 호출하자. (일괄적으로 Visible이 변경된 경우에 호출)
		/// </summary>
		/// <param name="meshGroup"></param>
		public void SaveAll(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//메시 그룹에 대한 데이터를 찾자
			VisibleGroupData groupData = null;
			if (_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
				if (groupData != null)
				{
					groupData.SyncObjects();
				}
			}
			else
			{
				//안되면 추가를 하고 동기화를 한다.
				groupData = SyncMeshGroup(meshGroup);
			}

			if (groupData == null)
			{
				return;
			}

			//동기화된 데이터를 이용하여 한꺼번에 값을 저장하자
			int nData_MeshTF = groupData._dataList_MeshTF.Count;
			int nData_MeshGroupTF = groupData._dataList_MeshGroupTF.Count;
			int nData_Bone = groupData._dataList_Bone.Count;

			VisibleData curData = null;
			apRenderUnit curRenderUnit = null;
			apBone curBone = null;

			for (int i = 0; i < nData_MeshTF; i++)
			{
				curData = groupData._dataList_MeshTF[i];
				if (curData == null || curData._key_MeshTF == null)
				{
					continue;
				}

				curRenderUnit = curData._key_MeshTF._linkedRenderUnit;
				if (curRenderUnit == null)
				{
					continue;
				}

				//RenderUnit의 현재 상태를 보고 Visible Type을 결정하자
				//Tmp의 값을 보자
				//이전
				//if(curRenderUnit._isVisibleWorkToggle_Hide2Show)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Shown;
				//}
				//else if(curRenderUnit._isVisibleWorkToggle_Show2Hide)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Hidden;
				//}
				//else
				//{
				//	curData._visibleType = VISIBLE_TYPE.None;
				//}

				//변경 21.1.28
				switch (curRenderUnit._workVisible_Tmp)
				{
					case apRenderUnit.WORK_VISIBLE_TYPE.None: curData._visibleType = VISIBLE_TYPE.None; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: curData._visibleType = VISIBLE_TYPE.Shown; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: curData._visibleType = VISIBLE_TYPE.Hidden; break;
				}
			}

			for (int i = 0; i < nData_MeshGroupTF; i++)
			{
				curData = groupData._dataList_MeshGroupTF[i];
				if (curData == null || curData._key_MeshGroupTF == null)
				{
					continue;
				}
				curRenderUnit = curData._key_MeshGroupTF._linkedRenderUnit;
				if (curRenderUnit == null)
				{
					continue;
				}

				//RenderUnit의 현재 상태를 보고 Visible Type을 결정하자
				//이전
				//if(curRenderUnit._isVisibleWorkToggle_Hide2Show)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Shown;
				//}
				//else if(curRenderUnit._isVisibleWorkToggle_Show2Hide)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Hidden;
				//}
				//else
				//{
				//	curData._visibleType = VISIBLE_TYPE.None;
				//}

				//변경 21.1.28
				switch (curRenderUnit._workVisible_Tmp)
				{
					case apRenderUnit.WORK_VISIBLE_TYPE.None: curData._visibleType = VISIBLE_TYPE.None; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: curData._visibleType = VISIBLE_TYPE.Shown; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: curData._visibleType = VISIBLE_TYPE.Hidden; break;
				}
			}


			for (int i = 0; i < nData_Bone; i++)
			{
				curData = groupData._dataList_Bone[i];
				if (curData == null || curData._key_Bone == null)
				{
					continue;
				}

				curBone = curData._key_Bone;

				//Bone은 Hidden이 아닌 경우에는 Show이다.

				//if(curBone.IsGUIVisible)//이전
				//{
				//	curData._visibleType = VISIBLE_TYPE.None;
				//}
				//else
				//{
				//	curData._visibleType = VISIBLE_TYPE.Hidden;
				//}

				//변경 21.1.28 : Tmp 이용
				switch (curBone.VisibleType_Tmp)
				{
					case apBone.GUI_VISIBLE_TYPE.None: curData._visibleType = VISIBLE_TYPE.None; break;
					case apBone.GUI_VISIBLE_TYPE.Show: curData._visibleType = VISIBLE_TYPE.Shown; break;
					case apBone.GUI_VISIBLE_TYPE.Hide: curData._visibleType = VISIBLE_TYPE.Hidden; break;
				}
			}
		}


		/// <summary>
		/// 렌더 유닛의 가시성에 변경점이 있다면 이 함수를 호출하자. (저장이 된다.)
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="renderUnit"></param>
		public void Save_RenderUnit(apMeshGroup meshGroup, apRenderUnit renderUnit)
		{
			if(meshGroup == null || renderUnit == null)
			{
				return;
			}

			//메시 그룹에 대한 데이터를 찾자
			VisibleGroupData groupData = null;
			if(_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
			}
			else
			{
				//안되면 추가를 하고 동기화를 한다.
				groupData = SyncMeshGroup(meshGroup);
			}

			if(groupData == null)
			{
				return;
			}

			VisibleData data = groupData.GetData(renderUnit);
			if(data == null)
			{
				return;
			}

			//Visibility를 지정하자
			//이전
			//if(renderUnit._isVisibleWorkToggle_Hide2Show)
			//{
			//	//Hide > Show
			//	data.SetVisibility(VISIBLE_TYPE.Shown);
			//}
			//else if(renderUnit._isVisibleWorkToggle_Show2Hide)
			//{
			//	//Show > Hide
			//	data.SetVisibility(VISIBLE_TYPE.Hidden);
			//}
			//else
			//{
			//	//연산값 그대로 사용
			//	data.SetVisibility(VISIBLE_TYPE.None);
			//}
			//변경 21.1.28
			switch (renderUnit._workVisible_Tmp)
			{
				case apRenderUnit.WORK_VISIBLE_TYPE.None: data.SetVisibility(VISIBLE_TYPE.None); break;
				case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: data.SetVisibility(VISIBLE_TYPE.Shown); break;
				case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: data.SetVisibility(VISIBLE_TYPE.Hidden); break;
			}
			
		}

		/// <summary>
		/// 본의 가시성에 변경점이 있다면 이 함수를 호출하자.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="bone"></param>
		public void Save_Bone(apMeshGroup meshGroup, apBone bone)
		{
			if(meshGroup == null || bone == null)
			{
				return;
			}

			//메시 그룹에 대한 데이터를 찾자
			VisibleGroupData groupData = null;
			if(_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
			}
			else
			{
				//안되면 추가를 하고 동기화를 한다.
				groupData = SyncMeshGroup(meshGroup);
			}

			if(groupData == null)
			{
				return;
			}

			VisibleData data = groupData.GetData(bone);
			if(data == null)
			{
				return;
			}

			//Visibility를 지정하자
			//이전
			//if(bone.IsGUIVisible)
			//{
			//	//Show : Bone은 보여지는게 기본값이다.
			//	data.SetVisibility(VISIBLE_TYPE.None);
			//}
			//else
			//{
			//	//Hide
			//	data.SetVisibility(VISIBLE_TYPE.Hidden);
			//}

			//변경 21.1.28
			switch (bone.VisibleType_Tmp)
			{
				case apBone.GUI_VISIBLE_TYPE.None: data.SetVisibility(VISIBLE_TYPE.None); break;
				case apBone.GUI_VISIBLE_TYPE.Show: data.SetVisibility(VISIBLE_TYPE.Shown); break;
				case apBone.GUI_VISIBLE_TYPE.Hide: data.SetVisibility(VISIBLE_TYPE.Hidden); break;
			}
		}


		/// <summary>
		/// 메시 그룹의 모든 RenderUnit들을 동기화하고 Visibility를 저장한다.
		/// 이 단계에서 꼭 Sync가 발생한다.
		/// Visibility를 리셋한 후 이 함수를 호출하자. (일괄적으로 Visible이 변경된 경우에 호출)
		/// </summary>
		/// <param name="meshGroup"></param>
		public void Save_AllRenderUnits(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//메시 그룹에 대한 데이터를 찾자
			VisibleGroupData groupData = null;
			if (_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
				if (groupData != null)
				{
					groupData.SyncObjects();
				}
			}
			else
			{
				//안되면 추가를 하고 동기화를 한다.
				groupData = SyncMeshGroup(meshGroup);
			}

			if (groupData == null)
			{
				return;
			}

			//동기화된 데이터를 이용하여 한꺼번에 값을 저장하자
			int nData_MeshTF = groupData._dataList_MeshTF.Count;
			int nData_MeshGroupTF = groupData._dataList_MeshGroupTF.Count;

			VisibleData curData = null;
			apRenderUnit curRenderUnit = null;

			for (int i = 0; i < nData_MeshTF; i++)
			{
				curData = groupData._dataList_MeshTF[i];
				if (curData == null || curData._key_MeshTF == null)
				{
					continue;
				}

				curRenderUnit = curData._key_MeshTF._linkedRenderUnit;
				if (curRenderUnit == null)
				{
					continue;
				}

				//RenderUnit의 현재 상태를 보고 Visible Type을 결정하자
				//이전
				//if(curRenderUnit._isVisibleWorkToggle_Hide2Show)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Shown;
				//}
				//else if(curRenderUnit._isVisibleWorkToggle_Show2Hide)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Hidden;
				//}
				//else
				//{
				//	curData._visibleType = VISIBLE_TYPE.None;
				//}

				//변경 21.1.28
				switch (curRenderUnit._workVisible_Tmp)
				{
					case apRenderUnit.WORK_VISIBLE_TYPE.None: curData._visibleType = VISIBLE_TYPE.None; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: curData._visibleType = VISIBLE_TYPE.Shown; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: curData._visibleType = VISIBLE_TYPE.Hidden; break;
				}
			}

			for (int i = 0; i < nData_MeshGroupTF; i++)
			{
				curData = groupData._dataList_MeshGroupTF[i];
				if (curData == null || curData._key_MeshGroupTF == null)
				{
					continue;
				}
				curRenderUnit = curData._key_MeshGroupTF._linkedRenderUnit;
				if (curRenderUnit == null)
				{
					continue;
				}

				//RenderUnit의 현재 상태를 보고 Visible Type을 결정하자
				//이전
				//if(curRenderUnit._isVisibleWorkToggle_Hide2Show)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Shown;
				//}
				//else if(curRenderUnit._isVisibleWorkToggle_Show2Hide)
				//{
				//	curData._visibleType = VISIBLE_TYPE.Hidden;
				//}
				//else
				//{
				//	curData._visibleType = VISIBLE_TYPE.None;
				//}

				//변경 21.1.28
				switch (curRenderUnit._workVisible_Tmp)
				{
					case apRenderUnit.WORK_VISIBLE_TYPE.None: curData._visibleType = VISIBLE_TYPE.None; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: curData._visibleType = VISIBLE_TYPE.Shown; break;
					case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: curData._visibleType = VISIBLE_TYPE.Hidden; break;
				}
			}
		}

		/// <summary>
		/// 메시 그룹의 모든 Bone들을 동기화하고 Visibility를 저장한다.
		/// 이 단계에서 꼭 Sync가 발생한다.
		/// Visibility를 리셋한 후 이 함수를 호출하자. (일괄적으로 Visible이 변경된 경우에 호출)
		/// </summary>
		/// <param name="meshGroup"></param>
		public void Save_AllBones(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//메시 그룹에 대한 데이터를 찾자
			VisibleGroupData groupData = null;
			if (_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
				if (groupData != null)
				{
					groupData.SyncObjects();
				}
			}
			else
			{
				//안되면 추가를 하고 동기화를 한다.
				groupData = SyncMeshGroup(meshGroup);
			}

			if (groupData == null)
			{
				return;
			}

			//동기화된 데이터를 이용하여 한꺼번에 값을 저장하자
			//여기서는 본만 설정
			int nData_Bone = groupData._dataList_Bone.Count;

			VisibleData curData = null;
			apBone curBone = null;


			for (int i = 0; i < nData_Bone; i++)
			{
				curData = groupData._dataList_Bone[i];
				if (curData == null || curData._key_Bone == null)
				{
					continue;
				}

				curBone = curData._key_Bone;

				//Bone은 Hidden이 아닌 경우에는 Show이다.
				//이전
				//if(curBone.IsGUIVisible)
				//{
				//	curData._visibleType = VISIBLE_TYPE.None;
				//}
				//else
				//{
				//	curData._visibleType = VISIBLE_TYPE.Hidden;
				//}

				//변경 21.1.28
				switch (curBone.VisibleType_Tmp)
				{
					case apBone.GUI_VISIBLE_TYPE.None: curData.SetVisibility(VISIBLE_TYPE.None); break;
					case apBone.GUI_VISIBLE_TYPE.Show: curData.SetVisibility(VISIBLE_TYPE.Shown); break;
					case apBone.GUI_VISIBLE_TYPE.Hide: curData.SetVisibility(VISIBLE_TYPE.Hidden); break;
				}
			}
		}




		//----------------------------------------------------------
		// 열어서 적용하기
		//----------------------------------------------------------

		/// <summary>
		/// 작업 종료시, 메뉴 열때 등등, 가시성을 저장된 값으로 복구하는 함수이다.
		/// 초기화하려면 다른 함수를 사용하자.
		/// RenderUnit이 생성된 상태여야 한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void LoadAll(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return;
			}

			//메시 그룹에 대한 데이터를 찾자
			VisibleGroupData groupData = null;
			if(_meshGroup_2_GroupData.ContainsKey(meshGroup))
			{
				groupData = _meshGroup_2_GroupData[meshGroup];
			}
			else
			{
				//안되면 추가를 하고 동기화를 한다.
				groupData = SyncMeshGroup(meshGroup);
			}

			if(groupData == null)
			{
				return;
			}

			//로드할 때, 값이 저장안된 객체는 반영이 안된다.
			int nData_MeshTF = groupData._dataList_MeshTF.Count;
			int nData_MeshGroupTF = groupData._dataList_MeshGroupTF.Count;
			int nData_Bone = groupData._dataList_Bone.Count;

			VisibleData curData = null;
			apRenderUnit curRenderUnit = null;
			apBone curBone = null;

			for (int i = 0; i < nData_MeshTF; i++)
			{
				curData = groupData._dataList_MeshTF[i];
				if(curData == null || curData._key_MeshTF == null)
				{
					continue;
				}

				curRenderUnit = curData._key_MeshTF._linkedRenderUnit;
				if(curRenderUnit == null)
				{
					continue;
				}

				if(curData._visibleType == VISIBLE_TYPE.None)
				{
					//기본값으로 돌린다.
					curRenderUnit.ResetTmpWorkVisible(false);
				}
				else if(curData._visibleType == VISIBLE_TYPE.Shown)
				{
					//Hide > Show
					//이전
					//curRenderUnit._isVisibleWorkToggle_Hide2Show = true;
					//curRenderUnit._isVisibleWorkToggle_Show2Hide = false;

					//변경 21.1.28
					curRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;

				}
				else
				{
					//Show > Hide
					//이전
					//curRenderUnit._isVisibleWorkToggle_Hide2Show = false;
					//curRenderUnit._isVisibleWorkToggle_Show2Hide = true;

					//변경 21.1.28
					curRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
				}


			}

			for (int i = 0; i < nData_MeshGroupTF; i++)
			{
				curData = groupData._dataList_MeshGroupTF[i];
				if(curData == null || curData._key_MeshGroupTF == null)
				{
					continue;
				}
				curRenderUnit = curData._key_MeshGroupTF._linkedRenderUnit;
				if(curRenderUnit == null)
				{
					continue;
				}

				if(curData._visibleType == VISIBLE_TYPE.None)
				{
					//기본값으로 돌린다.
					curRenderUnit.ResetTmpWorkVisible(false);
				}
				else if(curData._visibleType == VISIBLE_TYPE.Shown)
				{
					//Hide > Show
					//이전
					//curRenderUnit._isVisibleWorkToggle_Hide2Show = true;
					//curRenderUnit._isVisibleWorkToggle_Show2Hide = false;

					//변경 21.1.28
					curRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;
				}
				else
				{
					//Show > Hide
					//이전
					//curRenderUnit._isVisibleWorkToggle_Hide2Show = false;
					//curRenderUnit._isVisibleWorkToggle_Show2Hide = true;

					//변경 21.1.28
					curRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
				}
			}

			for (int i = 0; i < nData_Bone; i++)
			{
				curData = groupData._dataList_Bone[i];
				if (curData == null || curData._key_Bone == null)
				{
					continue;
				}

				curBone = curData._key_Bone;

				//이전
				////Bone은 Hidden이 아닌 경우에는 Show이다.
				//if(curData._visibleType == VISIBLE_TYPE.Hidden)
				//{
				//	curBone.SetGUIVisible(false);
				//}
				//else
				//{
				//	curBone.SetGUIVisible(true);
				//}

				//변경 21.1.28
				switch (curData._visibleType)
				{
					case VISIBLE_TYPE.None:		curBone.SetGUIVisible_Tmp(apBone.GUI_VISIBLE_TYPE.None); break;
					case VISIBLE_TYPE.Shown:	curBone.SetGUIVisible_Tmp(apBone.GUI_VISIBLE_TYPE.Show); break;
					case VISIBLE_TYPE.Hidden:	curBone.SetGUIVisible_Tmp(apBone.GUI_VISIBLE_TYPE.Hide); break;
				}
			}

		}


	}
}