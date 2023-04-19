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

	/// <summary>
	/// Timeline 1개에 해당하는 데이터
	/// MeshGroup의 어떤 데이터에 연동되는지 결정한다.
	/// </summary>
	[Serializable]
	public class apAnimTimeline
	{
		// Members
		//------------------------------------------
		[SerializeField]
		public int _uniqueID = -1;

		[NonSerialized]
		public apAnimClip _parentAnimClip = null;

		[NonSerialized]
		public bool _isActiveInEditing = true;

		//GUI에서 보이는 색상
		[SerializeField]
		public Color _guiColor = Color.black;

		//애니메이션이 적용되는 대상 타입 (모디파이어 / 본)
		public apAnimClip.LINK_TYPE _linkType = apAnimClip.LINK_TYPE.AnimatedModifier;
		public int _modifierUniqueID = -1;
		
		[NonSerialized]
		public apModifierBase _linkedModifier = null;
		
		//Opt용
		[NonSerialized]
		public apOptModifierUnitBase _linkedOptModifier = null;


		[SerializeField]
		public List<apAnimTimelineLayer> _layers = new List<apAnimTimelineLayer>();


		[SerializeField]
		public bool _guiTimelineFolded = false;//GUI에 보여지는 값


		// <Modifier 타입>
		// Timeline - Modifier
		// ㄴ> Layer : Mesh/MeshGroup Transform

		// <Bone 타입>
		// Timeline - [Bone 타입만 선언]
		// ㄴ> Layer : Bone

		// <Control Param 타입>
		// Timeline - [Control Param 타입만 선언]
		// ㄴ> Layer : ControlParam


		

		// Init
		//------------------------------------------
		public apAnimTimeline()
		{

		}


		public void Init(apAnimClip.LINK_TYPE linkType, int uniqueID, int modifierUniqueID, apAnimClip animClip)
		{
			_uniqueID = uniqueID;

			_layers.Clear();
			_parentAnimClip = animClip;

			_linkType = linkType;
			_modifierUniqueID = modifierUniqueID;

			//_guiColor = new Color(0.4f, 0.8f, 1.0f);
			_guiColor = new Color(0.15f, 0.5f, 0.7f);

			_guiTimelineFolded = false;
		}


		public void Link(apAnimClip animClip)
		{
			//[1.4.2] TargetMeshGroup의 Null 체크를 할 것

			_parentAnimClip = animClip;

			animClip._portrait.RegistUniqueID(apIDManager.TARGET.AnimTimeline, _uniqueID);

			_linkedModifier = null;
			//TODO : linkedBone 연결하자

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						//_boneUniqueID = -1;

						if (_parentAnimClip._targetMeshGroup != null)
						{
							_linkedModifier = _parentAnimClip._targetMeshGroup.GetModifier(_modifierUniqueID);

							//연결된 모디파이어가 없다면 삭제되도록 만든다.
							if (_linkedModifier == null)
							{
								_modifierUniqueID = -1;
							}
						}
					}

					break;

				//case apAnimClip.LINK_TYPE.Bone:
				case apAnimClip.LINK_TYPE.ControlParam:
					_modifierUniqueID = -1;
					break;
			}


			for (int i = 0; i < _layers.Count; i++)
			{
				_layers[i].Link(animClip, this);
			}
		}

		public void RemoveUnlinkedLayer()
		{
			for (int i = 0; i < _layers.Count; i++)
			{
				_layers[i].Link(_parentAnimClip, this);
			}

			_layers.RemoveAll(delegate (apAnimTimelineLayer a)
			{
				switch (a._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						if (a._transformID < 0 && a._boneID < 0)//Bone도 추가
						{
							return true;
						}
						break;


				case apAnimClip.LINK_TYPE.ControlParam:
						if (a._controlParamID < 0)
						{
							return true;
						}
						break;
				}
				return false;
			});
		}


		//[1.4.2] 에디터에서 AnimClip 선택시 Link를 새로 해야할지 여부를 빠르게 판단하는 함수
		public bool ValidateForLinkEditor()
		{
			if(_parentAnimClip == null)
			{
				return false;
			}

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					if (_linkedModifier == null)
					{
						return false;
					}
					break;
			}

			int nLayers = _layers != null ? _layers.Count : 0;

			if (nLayers > 0)
			{
				for (int i = 0; i < nLayers; i++)
				{
					bool isValid = _layers[i].ValidateForLinkEditor();
					if(!isValid)
					{
						return false;
					}
				}
			}

			return true;
			
		}






		public void LinkOpt(apAnimClip animClip)
		{
			_parentAnimClip = animClip;

			animClip._portrait.RegistUniqueID(apIDManager.TARGET.AnimTimeline, _uniqueID);


			_linkedOptModifier = null;
			//TODO : linkedBone 연결하자

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						//_boneUniqueID = -1;

						if (_parentAnimClip._targetOptTranform != null)
						{
							_linkedOptModifier = _parentAnimClip._targetOptTranform.GetModifier(_modifierUniqueID);
							if (_linkedOptModifier == null)
							{
								Debug.LogError("AnyPortrait Error : [" + _parentAnimClip._targetOptTranform.name + "] Runtime Timeline Link Error - No Modifier [" + _modifierUniqueID + "]");
								_modifierUniqueID = -1;

							}
						}
					}

					break;

				//case apAnimClip.LINK_TYPE.Bone:
				case apAnimClip.LINK_TYPE.ControlParam:
					_modifierUniqueID = -1;
					break;
			}


			for (int i = 0; i < _layers.Count; i++)
			{
				_layers[i].LinkOpt(animClip, this);
			}
		}



		public IEnumerator LinkOptAsync(apAnimClip animClip, apAsyncTimer asyncTimer)
		{
			_parentAnimClip = animClip;

			animClip._portrait.RegistUniqueID(apIDManager.TARGET.AnimTimeline, _uniqueID);

			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}

			_linkedOptModifier = null;
			//TODO : linkedBone 연결하자

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						//_boneUniqueID = -1;

						if (_parentAnimClip._targetOptTranform != null)
						{
							_linkedOptModifier = _parentAnimClip._targetOptTranform.GetModifier(_modifierUniqueID);
							if (_linkedOptModifier == null)
							{
								Debug.LogError("AnyPortrait Error : Runtime Timeline Link Error - No Modifier [" + _modifierUniqueID + "]");
								_modifierUniqueID = -1;

							}
						}
					}

					break;

				//case apAnimClip.LINK_TYPE.Bone:
				case apAnimClip.LINK_TYPE.ControlParam:
					_modifierUniqueID = -1;
					break;
			}


			for (int i = 0; i < _layers.Count; i++)
			{
				_layers[i].LinkOpt(animClip, this);
			}
		}
		// Functions
		//------------------------------------------
		/// <summary>
		/// 레이어들을 갱신한다.
		/// 타겟을 지정하는 경우 해당 레이어만 갱신한다.
		/// </summary>
		/// <param name="targetTimelineLayer">갱신하고자 하는 레이어. null인 경우 전체 갱신</param>
		public void RefreshLayers(	apAnimTimelineLayer targetTimelineLayer)
		{
			//변경 19.5.21 : 항상 모든 레이어를 Refresh하는게 아니라 타겟을 받아서 하는 걸로 변경
			if (targetTimelineLayer == null)
			{
				//타겟이 없다면 전체 Refresh
				for (int i = 0; i < _layers.Count; i++)
				{
					_layers[i].SortAndRefreshKeyframes();
				}
			}
			else
			{
				//타겟만 Refresh하자
				targetTimelineLayer.SortAndRefreshKeyframes();
			}
		}




		// Get / Set
		//------------------------------------------
		public bool IsTimelineLayerContain(apAnimTimelineLayer animTimelineLayer)
		{
			return _layers.Contains(animTimelineLayer);
		}

		private const string NAME_CONTROL_PARAMETERS = "Control Parameters";

		public string DisplayName
		{
			get
			{
				switch (_linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						if (_linkedModifier != null)
						{
							return _linkedModifier.DisplayName;
						}
						return "(Unknown Modifier)";

					//case apAnimClip.LINK_TYPE.Bone:
					//	return "Bones";

					case apAnimClip.LINK_TYPE.ControlParam:
						//return "Control Parameters";
						return NAME_CONTROL_PARAMETERS;
				}
				return "?";
			}
		}



		/// <summary>
		/// 해당 오브젝트 타입은 이 타임라인에 추가 가능한가
		/// (추가 되었는지 여부는 확인하지 않는다)
		/// </summary>
		/// <param name="selectedObject"></param>
		/// <returns></returns>
		public bool IsLayerAddableType(object selectedObject)
		{
			bool isTarget = IsTargetObject(selectedObject);
			if (!isTarget)
			{
				//타겟이 아니면 false
				return false;
			}

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					if (selectedObject is apTransform_Mesh)
					{
						apTransform_Mesh meshTransform = selectedObject as apTransform_Mesh;

						//타입이 맞는가
						if (!_linkedModifier.IsTarget_MeshTransform)
						{
							//Mesh Transform이 아니면 False
							return false;
						}

						if (!_linkedModifier.IsTarget_ChildMeshTransform)
						{
							//Child Mesh Transform을 허용하지 않을때
							//재귀적 Child Mesh Transform이라면 False
							if (!_linkedModifier._meshGroup.IsContainMeshTransform(meshTransform))
							{
								return false;
							}
						}
						return true;
					}
					else if (selectedObject is apTransform_MeshGroup)
					{
						//apTransform_MeshGroup meshGroupTransform = selectedObject as apTransform_MeshGroup;

						//타입이 맞는가
						if (!_linkedModifier.IsTarget_MeshGroupTransform)
						{
							//Mesh Group Transform이 아니면 False
							return false;
						}

						return true;
					}
					else if (selectedObject is apBone)
					{
						//추가
						//Bone 타입
						//apBone bone = selectedObject as apBone;
						if (!_linkedModifier.IsTarget_Bone)
						{
							//Bone 타입을 지원하지 않으면 False
							return false;
						}
						return true;

					}
					break;

				//case apAnimClip.LINK_TYPE.Bone:
				//	//TODO:
				//	return false;

				case apAnimClip.LINK_TYPE.ControlParam:
					if (selectedObject is apControlParam)
					{
						return true;//대상 객체가 맞고 레이어에는 없다.
					}
					break;
			}
			return false;
		}

		/// <summary>
		/// 현재 선택된 객체가 레이어로 이미 등록되었는지 체크
		/// (LinkType에 따라 다른 객체를 레이어로 넣을 수 있다.)
		/// </summary>
		/// <param name="selectedObject"></param>
		/// <returns>True : 이미 추가가 되었다(또는 추가할 수 없다) / False : 추가되지 않은 상태이다.</returns>
		public bool IsObjectAddedInLayers(object selectedObject)
		{
			bool isTarget = IsTargetObject(selectedObject);
			if (!isTarget)
			{
				//타겟이 아니면 false
				return true;
			}

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					if (selectedObject is apTransform_Mesh)
					{
						apTransform_Mesh meshTransform = selectedObject as apTransform_Mesh;

						for (int i = 0; i < _layers.Count; i++)
						{
							if (meshTransform == _layers[i]._linkedMeshTransform)
							{
								return true;//이미 있다.
							}
						}
						//추가되지 않았다.
						return false;
					}
					else if (selectedObject is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroupTransform = selectedObject as apTransform_MeshGroup;

						for (int i = 0; i < _layers.Count; i++)
						{
							if (meshGroupTransform == _layers[i]._linkedMeshGroupTransform)
							{
								return true;//이미 있다.
							}
						}
						//추가되지 않았다.
						return false;
					}
					else if (selectedObject is apBone)
					{
						apBone bone = selectedObject as apBone;

						for (int i = 0; i < _layers.Count; i++)
						{
							if (bone == _layers[i]._linkedBone)
							{
								//Bone이 이미 등록되어 있다.
								return true;
							}
						}
						//등록되지 않았다.
						return false;
					}
					break;


				//case apAnimClip.LINK_TYPE.Bone:
				//	Debug.LogError("TODO : AnimTimeline에서 Bone은 아직 구현되지 않았다.");
				//	//TODO:
				//	return false;

				case apAnimClip.LINK_TYPE.ControlParam:
					if (selectedObject is apControlParam)
					{
						apControlParam controlParam = selectedObject as apControlParam;
						for (int i = 0; i < _layers.Count; i++)
						{
							if (controlParam == _layers[i]._linkedControlParam)
							{
								return true;//이미 있다.
							}
						}
						//추가되지 않았다.
						return false;
					}
					break;
			}
			return true;
		}

		/// <summary>
		/// Layer에 추가 가능하거나 이미 Layer에 포함된 오브젝트인가
		/// </summary>
		/// <param name="selectedObject"></param>
		/// <returns></returns>
		public bool IsTargetObject(object selectedObject)
		{
			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					if (selectedObject is apTransform_Mesh ||
						selectedObject is apTransform_MeshGroup ||
						selectedObject is apBone)
					{
						return true;
					}
					break;

				//case apAnimClip.LINK_TYPE.Bone:
				//	//TODO:
				//	break;

				case apAnimClip.LINK_TYPE.ControlParam:
					if (selectedObject is apControlParam)
					{
						return true;
					}
					break;
			}
			return false;
		}



		public apAnimTimelineLayer GetTimelineLayer(object targetObject)
		{
			if (!IsTargetObject(targetObject))
			{
				return null;//대상 오브젝트가 아니다.
			}

			return _layers.Find(delegate (apAnimTimelineLayer a)
					{
						return a.IsContainTargetObject(targetObject);
					});
		}

		public apAnimTimelineLayer GetTimelineLayer(int timelineLayerID)
		{
			return _layers.Find(delegate (apAnimTimelineLayer a)
			{
				return a._uniqueID == timelineLayerID;
			});
		}

		// 실제로 연결된 데이터를 가져오자
		//-------------------------------------------------------------------------------
		/// <summary>
		/// Animated Modifier 타입의 타임라인인 경우에, layer와 keyframe을 선택한 경우,
		/// 해당 데이터와 연동된 paramSet을 가져온다. (paramSet에는 ModMesh가 포함되어있다)
		/// </summary>
		/// <param name="targetLayer"></param>
		/// <param name="keyframe"></param>
		/// <returns></returns>
		public apModifierParamSet GetModifierParamSet(apAnimTimelineLayer targetLayer, apAnimKeyframe keyframe)
		{
			if (_linkType != apAnimClip.LINK_TYPE.AnimatedModifier ||
				_linkedModifier == null)
			{
				return null;
			}
			if (targetLayer == null || !_layers.Contains(targetLayer) || keyframe == null)
			{
				return null;
			}

			apModifierParamSetGroup selectedParamSetGroup = _linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
			{
				return a._keyAnimTimelineLayer == targetLayer;
			});

			if (selectedParamSetGroup == null)
			{
				return null;
			}

			return selectedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
			{
				return a.SyncKeyframe == keyframe;
			});

		}


		//------------------------------------------------------------------------------------------
		// Copy For Bake
		//------------------------------------------------------------------------------------------
		public void CopyFromTimeline(apAnimTimeline srcTimeline, apAnimClip parentAnimClip)
		{
			_uniqueID = srcTimeline._uniqueID;
			_guiColor = srcTimeline._guiColor;

			_linkType = srcTimeline._linkType;
			_modifierUniqueID = srcTimeline._modifierUniqueID;

			_parentAnimClip = parentAnimClip;

			_layers.Clear();
			for (int iLayer = 0; iLayer < srcTimeline._layers.Count; iLayer++)
			{
				apAnimTimelineLayer srcLayer = srcTimeline._layers[iLayer];

				//복사해준다.
				apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
				newLayer.CopyFromTimelineLayer(srcLayer, parentAnimClip, this);



				_layers.Add(newLayer);
			}

			_guiTimelineFolded = false;//<<이건 에디트에서 사용되는 값

		}
	}

}