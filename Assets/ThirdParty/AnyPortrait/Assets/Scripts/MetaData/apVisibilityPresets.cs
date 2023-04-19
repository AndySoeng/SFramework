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
//using UnityEditor;
//using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	[Serializable]
	public class apVisibilityPresets
	{
		// Enums
		//-----------------------------------------------
		[Serializable]
		public enum OBJECT_TYPE : int
		{
			Transform = 0,
			Bone = 1,
		}

		[Serializable]
		public enum VISIBLE_OPTION : int
		{
			/// <summary>그대로</summary>
			None = 0,//사실 이 값을 설정하면 Refresh시 옵션에서 사라진다. 메모리 아껴야징
			/// <summary>강제로 보이게 하기</summary>
			Show = 1,
			/// <summary>강제로 숨기기</summary>
			Hide = 2
		}

		[Serializable]
		public enum RULE_TYPE : int
		{
			/// <summary>커스텀 : 이 규칙에 의해 지정된 보이기 옵션이 실행된다.</summary>
			Custom = 0,
			/// <summary>프리셋 : 모디파이어에 등록된 것만 보이기</summary>
			ShowOnlyModified = 1,
			/// <summary>프리셋 : 본이 속한 서브 메시 그룹이 보여질 때에만 본들을 출력하기</summary>
			ShowBonesIfMeshGroupVisible = 2,
		}


		//단축키를 할당할 수 있다. 최대 5개
		[Serializable]
		public enum HOTKEY : int
		{
			None = 0,
			Hotkey1 = 1,
			Hotkey2 = 2,
			Hotkey3 = 3,
			Hotkey4 = 4,
			Hotkey5 = 5
		}


		// Sub-Class
		//-----------------------------------------------
		[Serializable]
		public class ObjectVisibilityData
		{
			[SerializeField]
			public OBJECT_TYPE _objectType = OBJECT_TYPE.Transform;

			[SerializeField]
			public int _targetID = -1;

			[SerializeField]
			public VISIBLE_OPTION _visibleOption = VISIBLE_OPTION.None;

			[NonSerialized]
			public apTransform_Mesh _linkedMeshTF = null;

			[NonSerialized]
			public apTransform_MeshGroup _linkedMeshGroupTF = null;

			[NonSerialized]
			public apBone _linkedBone = null;

			public ObjectVisibilityData(OBJECT_TYPE objectType, int ID)
			{
				_objectType = objectType;
				_targetID = ID;
				_visibleOption = VISIBLE_OPTION.None;
			}

			public ObjectVisibilityData(ObjectVisibilityData srcData)
			{
				_objectType = srcData._objectType;
				_targetID = srcData._targetID;
				_visibleOption = srcData._visibleOption;
			}
		}


		/// <summary>
		/// 규칙 내의 메시 그룹 데이터. 
		/// </summary>
		[Serializable]
		public class MeshGroupVisibilityData
		{
			[SerializeField]
			public int _meshGroupID = -1;

			[SerializeField]
			public List<ObjectVisibilityData> _objectDataList = null;

			[NonSerialized]
			public apMeshGroup _linkedMeshGroup = null;

			public MeshGroupVisibilityData(int meshGroupID)
			{
				_meshGroupID = meshGroupID;
			}

			
			public ObjectVisibilityData GetObjData_TF(int meshTFID)
			{
				if(_objectDataList == null)
				{
					return null;
				}

				return _objectDataList.Find(delegate(ObjectVisibilityData a)
				{
					return a._objectType == OBJECT_TYPE.Transform && a._targetID == meshTFID;
				});
			}

			public ObjectVisibilityData AddOrGetObjData_TF(int meshTFID)
			{
				ObjectVisibilityData targetObjData = null;
				if(_objectDataList == null)
				{
					_objectDataList = new List<ObjectVisibilityData>();
				}

				targetObjData = _objectDataList.Find(delegate(ObjectVisibilityData a)
				{
					return a._objectType == OBJECT_TYPE.Transform && a._targetID == meshTFID;
				});

				if(targetObjData == null)
				{
					//없으면 새로 생성
					targetObjData = new ObjectVisibilityData(OBJECT_TYPE.Transform, meshTFID);
					_objectDataList.Add(targetObjData);
				}

				return targetObjData;
			}


			public ObjectVisibilityData GetObjData_Bone(int boneID)
			{
				if(_objectDataList == null)
				{
					return null;
				}

				return _objectDataList.Find(delegate(ObjectVisibilityData a)
				{
					return a._objectType == OBJECT_TYPE.Bone && a._targetID == boneID;
				});
			}


			public ObjectVisibilityData AddOrGetObjData_Bone(int boneID)
			{
				ObjectVisibilityData targetObjData = null;
				if(_objectDataList == null)
				{
					_objectDataList = new List<ObjectVisibilityData>();
				}

				targetObjData = _objectDataList.Find(delegate(ObjectVisibilityData a)
				{
					return a._objectType == OBJECT_TYPE.Bone && a._targetID == boneID;
				});

				if(targetObjData == null)
				{
					//없으면 새로 생성
					targetObjData = new ObjectVisibilityData(OBJECT_TYPE.Bone, boneID);
					_objectDataList.Add(targetObjData);
				}

				return targetObjData;
			}

			public void CopyFromSrc(MeshGroupVisibilityData srcData)
			{
				if(_objectDataList == null)
				{
					_objectDataList = new List<ObjectVisibilityData>();
				}
				_objectDataList.Clear();

				if(srcData._objectDataList != null && srcData._objectDataList.Count > 0)
				{
					ObjectVisibilityData src = null;
					ObjectVisibilityData dst = null;
					for (int iSrc = 0; iSrc < srcData._objectDataList.Count; iSrc++)
					{
						src = srcData._objectDataList[iSrc];
						dst = new ObjectVisibilityData(src);

						_objectDataList.Add(dst);
					}
				}
			}

			public void RemoveTF(int meshTFID)
			{
				_objectDataList.RemoveAll(delegate(ObjectVisibilityData a)
				{
					return a._objectType == OBJECT_TYPE.Transform && a._targetID == meshTFID;
				});
			}

			public void RemoveBone(int boneID)
			{
				_objectDataList.RemoveAll(delegate(ObjectVisibilityData a)
				{
					return a._objectType == OBJECT_TYPE.Bone && a._targetID == boneID;
				});
			}
		}

		/// <summary>
		/// 규칙 데이터
		/// </summary>
		[Serializable]
		public class RuleData
		{	
			[SerializeField]
			public int _index = -1;//순서 인덱스
			
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public HOTKEY _hotKey = HOTKEY.None;

			[SerializeField]
			public RULE_TYPE _ruleType = RULE_TYPE.Custom;

			//커스텀인 경우
			[SerializeField]
			public List<MeshGroupVisibilityData> _meshGroupData = null;

			public RuleData(int index)
			{
				_index = index;
				_name = "";
				_hotKey = HOTKEY.None;
				_ruleType = RULE_TYPE.Custom;

				_meshGroupData = null;
			}

			public void ClearMeshGroupData()
			{
				_meshGroupData = null;
			}

			public MeshGroupVisibilityData GetMeshGroupData(apMeshGroup srcMeshGroup)
			{
				if(_meshGroupData == null)
				{
					return null;
				}
				return _meshGroupData.Find(delegate(MeshGroupVisibilityData a)
				{
					return a._meshGroupID == srcMeshGroup._uniqueID;
				});
			}

			public void CopyFromSrc(RuleData srcRule)
			{
				_hotKey = HOTKEY.None;
				_ruleType = srcRule._ruleType;

				_meshGroupData = null;

				if(_ruleType == RULE_TYPE.Custom && srcRule._meshGroupData != null)
				{
					//커스텀 타입이면 그것도 복제하자
					_meshGroupData = new List<MeshGroupVisibilityData>();
					for (int iData = 0; iData < srcRule._meshGroupData.Count; iData++)
					{
						MeshGroupVisibilityData srcMGData = srcRule._meshGroupData[iData];
						MeshGroupVisibilityData newData = new MeshGroupVisibilityData(srcMGData._meshGroupID);
						newData.CopyFromSrc(srcMGData);
						_meshGroupData.Add(newData);
					}
				}
			}

			public ObjectVisibilityData SetCustomData_TF(apMeshGroup meshGroup, int meshTFID, VISIBLE_OPTION targetOption)
			{
				MeshGroupVisibilityData targetMGData = GetMeshGroupData(meshGroup);
				if(targetMGData == null)
				{
					targetMGData = new MeshGroupVisibilityData(meshGroup._uniqueID);
					if(_meshGroupData == null)
					{
						_meshGroupData = new List<MeshGroupVisibilityData>();
					}
					_meshGroupData.Add(targetMGData);
				}

				ObjectVisibilityData objData = targetMGData.AddOrGetObjData_TF(meshTFID);
				objData._visibleOption = targetOption;

				return objData;

			}

			public ObjectVisibilityData SetCustomData_Bone(apMeshGroup meshGroup, int boneID, VISIBLE_OPTION targetOption)
			{
				MeshGroupVisibilityData targetMGData = GetMeshGroupData(meshGroup);
				if(targetMGData == null)
				{
					targetMGData = new MeshGroupVisibilityData(meshGroup._uniqueID);
					if(_meshGroupData == null)
					{
						_meshGroupData = new List<MeshGroupVisibilityData>();
					}
					_meshGroupData.Add(targetMGData);
				}

				ObjectVisibilityData objData = targetMGData.AddOrGetObjData_Bone(boneID);
				objData._visibleOption = targetOption;

				return objData;
			}

			public void ClearCustomData_TF(apMeshGroup meshGroup, int meshTFID)
			{
				MeshGroupVisibilityData targetMGData = GetMeshGroupData(meshGroup);
				if(targetMGData == null)
				{
					return;
				}

				targetMGData.RemoveTF(meshTFID);
			}

			public void ClearCustomData_Bone(apMeshGroup meshGroup, int boneID)
			{
				MeshGroupVisibilityData targetMGData = GetMeshGroupData(meshGroup);
				if(targetMGData == null)
				{
					return;
				}

				targetMGData.RemoveBone(boneID);
			}

			//-----------------------------------------------------------------
			//동기화
			
		}






		// Members
		//-----------------------------------------------
		[SerializeField]
		public List<RuleData> _rules = new List<RuleData>();


		//동기화용
		//에디터에서 이 값을 입력받자
		//이 중에서 하나라도 바뀌면 다시 동기화를 해야한다.
		[NonSerialized]
		public bool _sync_IsUsePreset = false;

		[NonSerialized]
		public apMeshGroup _sync_MeshGroup = null;

		[NonSerialized]
		public apModifierBase _sync_Modifier = null;

		[NonSerialized]
		public apModifierParamSetGroup _sync_ModParamSetGroup = null;

		[NonSerialized]
		public apAnimTimeline _sync_AnimTimeline = null;

		[NonSerialized]
		public RuleData _sync_SelectedRule = null;

		//ShowBonesIfMeshGroupVisible 타입의 규칙인 경우에만
		//BoneSetList의 SubMeshGroup들을 확인하여, "보이기 결과"가 다른지 확인하자
		//SubMeshGroup 리스트, 보이기 상태 등 하나라도 다르면 동기화가 필요하다.
		//Bone을 가진 RenderUnit > 보이기 상태를 Dictionary로 가지자 (결과값이므로 bool)
		[NonSerialized]
		private Dictionary<apRenderUnit, bool> _sync_SubMeshGroupTFForBones = null;



		//Link용
		//[모디파이어에 적용된 객체만 보여지는 Rule]
		//MeshGroup이 바뀔때 모든 모디파이어/타임라인마다 오브젝트 포함 여부를 기록하자
		

		

		//[NonSerialized]
		//private Dictionary<>


		// Init
		//-----------------------------------------------
		public apVisibilityPresets()
		{
			ClearSync();
		}

		public void ClearSync()
		{	
			_sync_IsUsePreset = false;
			_sync_MeshGroup = null;
			_sync_Modifier = null;
			_sync_ModParamSetGroup = null;
			_sync_AnimTimeline = null;
			_sync_SelectedRule = null;
			if(_sync_SubMeshGroupTFForBones == null)
			{
				_sync_SubMeshGroupTFForBones = new Dictionary<apRenderUnit, bool>();
			}
			_sync_SubMeshGroupTFForBones.Clear();

			//Debug.LogError("동기화 해제");
		}

		// Functions - Sync
		//-----------------------------------------------
		/// <summary>
		/// 동기화를 할 지 체크한다. 새로운 동기화가 필요하다면 true리턴
		/// </summary>
		public bool CheckSync(	bool isUsePreset,
								apMeshGroup meshGroup,
								apModifierBase modifier,
								apModifierParamSetGroup modParamSetGroup,
								apAnimTimeline animTimeline,
								RuleData selectedRule)
		{
			//값은 대입하되, 하나라도 바뀌면 다시 동기화를 해야한다고 리턴하자
			bool isNeedToSync = false;
			if(_sync_IsUsePreset != isUsePreset
				|| _sync_MeshGroup != meshGroup
				|| _sync_SelectedRule != selectedRule)
			{
				_sync_IsUsePreset = isUsePreset;
				_sync_MeshGroup = meshGroup;
				_sync_SelectedRule = selectedRule;
				isNeedToSync = true;
			}

			//특정 Rule은 추가적인 조건도 봐야한다.
			if(_sync_SelectedRule != null)
			{
				switch (_sync_SelectedRule._ruleType)
				{
					case RULE_TYPE.Custom:
						//커스텀인 경우, 모디파이어는 중요하지 않다.
						//동기화에 상관없이 null
						_sync_Modifier = null;
						_sync_ModParamSetGroup = null;
						_sync_AnimTimeline = null;

						if(_sync_SubMeshGroupTFForBones.Count > 0)
						{
							//규칙에 맞지 않게 불필요하게 정보를 가지고 있다.
							//삭제. 단, 동기화는 필요없겠다.
							_sync_SubMeshGroupTFForBones.Clear();
						}
						break;

					case RULE_TYPE.ShowOnlyModified:
						//모디파이어를 체크하는 경우
						if (_sync_Modifier != modifier
							|| _sync_ModParamSetGroup != modParamSetGroup
							|| _sync_AnimTimeline != animTimeline)
						{
							_sync_Modifier = modifier;
							_sync_ModParamSetGroup = modParamSetGroup;
							_sync_AnimTimeline = animTimeline;
							isNeedToSync = true;
						}

						if(_sync_SubMeshGroupTFForBones.Count > 0)
						{
							//규칙에 맞지 않게 불필요하게 정보를 가지고 있다.
							//삭제. 단, 동기화는 필요없겠다.
							_sync_SubMeshGroupTFForBones.Clear();
						}
						break;

					case RULE_TYPE.ShowBonesIfMeshGroupVisible:
						//여기서는 SubMeshGroup만 본다.
						_sync_Modifier = null;
						_sync_ModParamSetGroup = null;
						_sync_AnimTimeline = null;

						//정보가 하나라도 다르면 재설정+Sync
						if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
						{
							int nBoneSetList = meshGroup._boneListSets.Count;


							apRenderUnit linkedRenderUnit = null;
							apTransform_MeshGroup subMeshGroupTF = null;
							apMeshGroup.BoneListSet boneSetList = null;
							bool isNeedToResetList = false;//아예 리셋을 해야하나
							for (int iBoneSet = 0; iBoneSet < nBoneSetList; iBoneSet++)
							{
								boneSetList = meshGroup._boneListSets[iBoneSet];
								if (boneSetList._isRootMeshGroup)
								{
									//Root는 스킵
									continue;
								}
								subMeshGroupTF = boneSetList._meshGroupTransform;
								if (subMeshGroupTF == null
									|| subMeshGroupTF._linkedRenderUnit == null)
								{
									continue;
								}

								linkedRenderUnit = subMeshGroupTF._linkedRenderUnit;

								if (_sync_SubMeshGroupTFForBones.ContainsKey(linkedRenderUnit))
								{
									bool sync_IsVisible = _sync_SubMeshGroupTFForBones[linkedRenderUnit];
									bool cur_IsVisible = linkedRenderUnit._isVisible;


									if (cur_IsVisible != sync_IsVisible)
									{
										//값이 다르면 동기화를 해야한다.
										_sync_SubMeshGroupTFForBones[linkedRenderUnit] = cur_IsVisible;
										isNeedToSync = true;
									}
								}
								else
								{
									//MeshGroup TF가 없다면,
									//아예 리셋을 해야한다.
									isNeedToResetList = true;
									isNeedToSync = true;
									break;
								}
							}

							if (isNeedToResetList)
							{
								//리셋을 해야한다면
								_sync_SubMeshGroupTFForBones.Clear();

								for (int iBoneSet = 0; iBoneSet < nBoneSetList; iBoneSet++)
								{
									boneSetList = meshGroup._boneListSets[iBoneSet];
									if (boneSetList._isRootMeshGroup)
									{
										//Root는 스킵
										continue;
									}
									subMeshGroupTF = boneSetList._meshGroupTransform;
									if (subMeshGroupTF == null
										|| subMeshGroupTF._linkedRenderUnit == null)
									{
										continue;
									}

									linkedRenderUnit = subMeshGroupTF._linkedRenderUnit;

									_sync_SubMeshGroupTFForBones.Add(linkedRenderUnit, linkedRenderUnit._isVisible);
								}
							}
						}
						else if (_sync_SubMeshGroupTFForBones.Count > 0)
						{
							//본이 없는데 불필요하게 정보를 가지고 있다.
							//동기화 필요
							_sync_SubMeshGroupTFForBones.Clear();
							isNeedToSync = true;
						}
						break;

				}
			}

			return isNeedToSync;
		}


		/// <summary>
		/// 동기화를 한다.
		/// </summary>
		/// <param name="isAnim"></param>
		public void Sync(bool isAnim)
		{
			//Debug.Log("동기화");
			//규칙 종류에 따라 동기화 방식이 아예 다르다
			switch (_sync_SelectedRule._ruleType)
			{
				case RULE_TYPE.Custom:					
					Sync_Custom();
					break;

				case RULE_TYPE.ShowOnlyModified:					
					Sync_ShowOnlyModified(isAnim);
					break;

				case RULE_TYPE.ShowBonesIfMeshGroupVisible:	
					Sync_ShowBonesIfMeshGroupVisible();
					break;
			}
		}


		private void Sync_Custom()
		{
			//커스텀 데이터에 따라서 직접 설정한다.
			if(_sync_MeshGroup == null)
			{
				return;
			}
			int nMeshGroupData = _sync_SelectedRule._meshGroupData != null ? _sync_SelectedRule._meshGroupData.Count : 0;
			MeshGroupVisibilityData targetMeshGroupData = null;
			MeshGroupVisibilityData curMeshGroupData = null;
			for (int i = 0; i < nMeshGroupData; i++)
			{
				curMeshGroupData = _sync_SelectedRule._meshGroupData[i];
				if(curMeshGroupData._meshGroupID == _sync_MeshGroup._uniqueID)
				{
					//찾았당
					targetMeshGroupData = curMeshGroupData;
					break;
				}
			}
			if(targetMeshGroupData != null)
			{
				//대상이 되는 데이터를 찾았다.
				//하나씩 연결하자
				int nObjList = targetMeshGroupData._objectDataList != null ? targetMeshGroupData._objectDataList.Count : 0;
				
				Dictionary<int, ObjectVisibilityData> meshTFID2Data = new Dictionary<int, ObjectVisibilityData>();
				Dictionary<int, ObjectVisibilityData> boneID2Data = new Dictionary<int, ObjectVisibilityData>();
				
				ObjectVisibilityData objData = null;
				for (int iData = 0; iData < nObjList; iData++)
				{
					objData = targetMeshGroupData._objectDataList[iData];
					if(objData._objectType == OBJECT_TYPE.Transform)
					{	
						if(!meshTFID2Data.ContainsKey(objData._targetID))
						{
							meshTFID2Data.Add(objData._targetID, objData);
						}
					}
					else
					{
						if(!boneID2Data.ContainsKey(objData._targetID))
						{
							boneID2Data.Add(objData._targetID, objData);
						}
					}
				}

				//데이터를 할당하자
				int nRenderUnits = _sync_MeshGroup._renderUnits_All.Count;
				apRenderUnit renderUnit = null;
				ObjectVisibilityData targetObjData = null;
				for (int i = 0; i < nRenderUnits; i++)
				{
					renderUnit = _sync_MeshGroup._renderUnits_All[i];
					
					//이게 저장되어 있는지 보자
					int transformID = -1;
					targetObjData = null;

					if (renderUnit._meshTransform != null)				{ transformID = renderUnit._meshTransform._transformUniqueID; }
					else if (renderUnit._meshGroupTransform != null)	{ transformID = renderUnit._meshGroupTransform ._transformUniqueID; }

					if(transformID > -1 && meshTFID2Data.ContainsKey(transformID))
					{
						targetObjData = meshTFID2Data[transformID];
					}

					if(targetObjData != null)
					{
						//저장된 데이터를 이용하자
						switch (targetObjData._visibleOption)
						{
							case VISIBLE_OPTION.None: renderUnit._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None; break;
							case VISIBLE_OPTION.Show: renderUnit._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.ToShow; break;
							case VISIBLE_OPTION.Hide: renderUnit._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.ToHide; break;
						}
					}
					else
					{
						//그 외에는 None
						renderUnit._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
					}
				}

				apBone bone = null;
				if (_sync_MeshGroup._boneListSets != null && _sync_MeshGroup._boneListSets.Count > 0)
				{
					List<apBone> boneList = null;
					for (int iBontSet = 0; iBontSet < _sync_MeshGroup._boneListSets.Count; iBontSet++)
					{	
						boneList = _sync_MeshGroup._boneListSets[iBontSet]._bones_All;
						if (boneList != null && boneList.Count > 0)
						{
							for (int iBone = 0; iBone < boneList.Count; iBone++)
							{
								bone = boneList[iBone];
								targetObjData = null;

								if(boneID2Data.ContainsKey(bone._uniqueID))
								{
									targetObjData = boneID2Data[bone._uniqueID];
								}
								if (targetObjData != null)
								{
									//저장된 데이터를 이용하자
									switch (targetObjData._visibleOption)
									{
										case VISIBLE_OPTION.None: bone.SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None); break;
										case VISIBLE_OPTION.Show: bone.SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.Show); break;
										case VISIBLE_OPTION.Hide: bone.SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.Hide); break;
									}
								}
								else
								{
									//저장된게 없으면 None
									bone.SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None);
								}
								
							}
						}
					}
				}
			}
			else
			{
				//대상이 되는 데이터를 못찾았다면 초기화

				//Debug.LogWarning("해당 메시 그룹에서 정의되지 않은 규칙");
				//리셋을 하자
				int nRenderUnits = _sync_MeshGroup._renderUnits_All.Count;
				for (int i = 0; i < nRenderUnits; i++)
				{
					_sync_MeshGroup._renderUnits_All[i]._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
				}

				//본 초기화
				if (_sync_MeshGroup._boneListSets != null && _sync_MeshGroup._boneListSets.Count > 0)
				{
					List<apBone> boneList = null;
					for (int iBontSet = 0; iBontSet < _sync_MeshGroup._boneListSets.Count; iBontSet++)
					{	
						boneList = _sync_MeshGroup._boneListSets[iBontSet]._bones_All;
						if (boneList != null && boneList.Count > 0)
						{
							for (int iBone = 0; iBone < boneList.Count; iBone++)
							{
								boneList[iBone].SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None);
							}
						}
					}
				}
			}
		}

		private void Sync_ShowOnlyModified(bool isAnim)
		{
			//현재 모디파이어(+ParamSetGroup / AnimTimeline)에 연결된 것들만 보여주고, 그렇지 않으면 숨긴다.
			if(_sync_MeshGroup == null)
			{
				return;
			}

			//이건 링크를 해야한다.
			//Modifier-PSG 또는 Timeline 별로 보여지는 객체를 갱신해야함
			List<apRenderUnit> editedRenderUnits = null;
			List<apBone> editedBones = null;

			bool isExclusiveVisiblity = false;
			if(_sync_Modifier != null)
			{
				editedRenderUnits = new List<apRenderUnit>();
				if (isAnim)
				{
					if (_sync_AnimTimeline != null)
					{
						//타임라인 레이어로 편집하는 대상을 저장하자
						int nLayers = _sync_AnimTimeline._layers != null ? _sync_AnimTimeline._layers.Count : 0;
						editedBones = new List<apBone>();
						
						apAnimTimelineLayer curLayer = null;
						for (int iLayer = 0; iLayer < nLayers; iLayer++)
						{
							curLayer = _sync_AnimTimeline._layers[iLayer];
							if(curLayer._linkedMeshTransform != null)
							{
								if(curLayer._linkedMeshTransform._linkedRenderUnit != null)
								{
									editedRenderUnits.Add(curLayer._linkedMeshTransform._linkedRenderUnit);
								}
							}
							else if(curLayer._linkedMeshGroupTransform != null)
							{
								if(curLayer._linkedMeshGroupTransform._linkedRenderUnit != null)
								{
									editedRenderUnits.Add(curLayer._linkedMeshGroupTransform._linkedRenderUnit);
								}
							}
							else if(curLayer._linkedBone != null)
							{
								editedBones.Add(curLayer._linkedBone);
							}
						}

						isExclusiveVisiblity = true;
					}
				}
				else
				{
					//메시그룹의 모디파이어 선택시 편집하는 대상을 저장하자.
					//이 경우는 ParamSetGroup에 한정한다.
					if(_sync_ModParamSetGroup != null)
					{
						//Bone은 그대로 이용한다.
						
						editedBones = _sync_ModParamSetGroup._syncBone;
						
						int nMeshTFs = _sync_ModParamSetGroup._syncTransform_Mesh != null ? _sync_ModParamSetGroup._syncTransform_Mesh.Count : 0;
						int nMeshGroupTFs = _sync_ModParamSetGroup._syncTransform_MeshGroup != null ? _sync_ModParamSetGroup._syncTransform_MeshGroup.Count : 0;

						apRenderUnit curRenderUnit = null;
						for (int iMeshTF = 0; iMeshTF < nMeshTFs; iMeshTF++)
						{
							curRenderUnit = _sync_ModParamSetGroup._syncTransform_Mesh[iMeshTF]._linkedRenderUnit;
							if(curRenderUnit != null)
							{
								editedRenderUnits.Add(curRenderUnit);
							}
						}

						for (int iMeshGroupTF = 0; iMeshGroupTF < nMeshGroupTFs; iMeshGroupTF++)
						{
							curRenderUnit = _sync_ModParamSetGroup._syncTransform_MeshGroup[iMeshGroupTF]._linkedRenderUnit;
							if(curRenderUnit != null)
							{
								editedRenderUnits.Add(curRenderUnit);
							}
						}

						isExclusiveVisiblity = true;
					}
				}
			}
			
			//편집중이라면
			if(isExclusiveVisiblity)
			{
				//Debug.Log("보여야 하는 RenderUnit수 : " + editedRenderUnits.Count);
				//Debug.Log("보여야 하는 Bone 수 : " + (editedBones != null ? editedBones.Count : 0));
				apRenderUnit renderUnit = null;
				int nRenderUnits = _sync_MeshGroup._renderUnits_All.Count;
				for (int i = 0; i < nRenderUnits; i++)
				{
					renderUnit = _sync_MeshGroup._renderUnits_All[i];
					if(editedRenderUnits.Contains(renderUnit) || renderUnit._parentRenderUnit == null)
					{	
						renderUnit._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
					}
					else
					{
						//편집중이 아니라면
						renderUnit._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
					}
				}

				//본 초기화
				apBone bone = null;
				if (_sync_MeshGroup._boneListSets != null && _sync_MeshGroup._boneListSets.Count > 0)
				{
					List<apBone> boneList = null;
					for (int iBontSet = 0; iBontSet < _sync_MeshGroup._boneListSets.Count; iBontSet++)
					{	
						boneList = _sync_MeshGroup._boneListSets[iBontSet]._bones_All;
						if (boneList != null && boneList.Count > 0)
						{
							for (int iBone = 0; iBone < boneList.Count; iBone++)
							{
								bone = boneList[iBone];
								if(editedBones != null && editedBones.Contains(bone))
								{
									bone.SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None);
								}
								else
								{
									//편집 중이 아니다.
									bone.SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.Hide);
								}
								
							}
						}
					}
				}
			}
			else
			{
				//Debug.LogWarning("편집 중이 아니다.");
				//편집중이 아니라면(실패시), 그냥 다 보여줘야 한다.
				//Debug.LogWarning("동기화 해제");
				//리셋을 하자
				int nRenderUnits = _sync_MeshGroup._renderUnits_All.Count;
				for (int i = 0; i < nRenderUnits; i++)
				{
					_sync_MeshGroup._renderUnits_All[i]._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
				}

				//본 초기화
				if (_sync_MeshGroup._boneListSets != null && _sync_MeshGroup._boneListSets.Count > 0)
				{
					List<apBone> boneList = null;
					for (int iBontSet = 0; iBontSet < _sync_MeshGroup._boneListSets.Count; iBontSet++)
					{	
						boneList = _sync_MeshGroup._boneListSets[iBontSet]._bones_All;
						if (boneList != null && boneList.Count > 0)
						{
							for (int iBone = 0; iBone < boneList.Count; iBone++)
							{
								boneList[iBone].SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None);
							}
						}
					}
				}
			}
		}



		private void Sync_ShowBonesIfMeshGroupVisible()
		{
			//메시는 모두 None
			//본은 루트 본은 모두 None
			//자식 메시의 본들은 자식 SubMeshGroup의 현재 보이는 값에 따라 결정된다.
			if(_sync_MeshGroup == null)
			{
				return;
			}
			int nRenderUnits = _sync_MeshGroup._renderUnits_All.Count;
			for (int i = 0; i < nRenderUnits; i++)
			{
				_sync_MeshGroup._renderUnits_All[i]._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
			}

			//본 설정
			if (_sync_MeshGroup._boneListSets != null && _sync_MeshGroup._boneListSets.Count > 0)
			{
				List<apBone> boneList = null;
				apMeshGroup.BoneListSet boneSet = null;
				apBone.GUI_VISIBLE_TYPE nextVisibleType = apBone.GUI_VISIBLE_TYPE.None;
				for (int iBontSet = 0; iBontSet < _sync_MeshGroup._boneListSets.Count; iBontSet++)
				{	
					boneSet = _sync_MeshGroup._boneListSets[iBontSet];

					if(boneSet._isRootMeshGroup)
					{
						nextVisibleType = apBone.GUI_VISIBLE_TYPE.None;
					}
					else if(boneSet._meshGroupTransform != null && boneSet._meshGroupTransform._linkedRenderUnit != null)
					{
						//본들의 부모인 MeshGroup TF가 보일때는 None, 그렇지 않으면 Hide
						if(boneSet._meshGroupTransform._linkedRenderUnit._isVisible)
						{
							nextVisibleType = apBone.GUI_VISIBLE_TYPE.None;
						}
						else
						{
							nextVisibleType = apBone.GUI_VISIBLE_TYPE.Hide;
						}
					}
					else
					{
						nextVisibleType = apBone.GUI_VISIBLE_TYPE.Hide;
					}


					boneList = boneSet._bones_All;
					if (boneList != null && boneList.Count > 0)
					{
						for (int iBone = 0; iBone < boneList.Count; iBone++)
						{
							boneList[iBone].SetGUIVisible_Rule(nextVisibleType);
						}
					}
				}
			}
		}


		// Functions - Editor
		//-----------------------------------------------
		public RuleData AddNewRule()
		{
			if(_rules == null)
			{
				_rules = new List<RuleData>();
			}
			RuleData newRule = new RuleData(_rules.Count);
			newRule._name = "New Rule";
			_rules.Add(newRule);

			RefreshRules();

			return newRule;
		}

		public bool IsContains(RuleData rule)
		{
			return _rules.Contains(rule);
		}

		private void RefreshRules()
		{
			for (int i = 0; i < _rules.Count; i++)
			{
				_rules[i]._index = i;
			}
		}

		public void Sort()
		{
			if(_rules == null)
			{
				return;
			}

			_rules.Sort(delegate(RuleData a, RuleData b)
			{
				return a._index - b._index;
			});
			RefreshRules();
		}

		public void RemoveRule(RuleData targetRule)
		{
			_rules.Remove(targetRule);
			RefreshRules();
		}

		public RuleData GetRuleByHotkey(HOTKEY hotkey)
		{
			return _rules.Find(delegate(RuleData a)
			{
				return a._hotKey == hotkey;
			});
		}
		

		// Get / Set
		//-----------------------------------------------
	}



	
}