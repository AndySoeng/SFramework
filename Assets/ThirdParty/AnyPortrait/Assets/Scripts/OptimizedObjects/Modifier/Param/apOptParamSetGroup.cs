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
	/// Modifier에서 ParamSet 중 비슷한 것끼리 묶어서 처리하는 클래스
	/// Editor에서 - 작업용 기능을 제공하고, UI의 편의성을 높이는 기능을 제공한다.
	/// Realtime에서 - matrix 계산시 도와주도록 한다.
	/// </summary>
	[Serializable]
	public class apOptParamSetGroup
	{
		// Members
		//--------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		[NonSerialized]
		public apOptModifierUnitBase _parentModifier = null;

		//어떤 값에 의해서 영향을 받는가
		public apModifierParamSetGroup.SYNC_TARGET _syncTarget = apModifierParamSetGroup.SYNC_TARGET.Static;

		//타겟의 어떤 값에 연동할 것인가
		//1. None.. 없다. 그냥 고정값

		//2. Controller -> Controller Param
		//Controller 방식일때
		//public string _keyControlParamName = "";
		public int _keyControlParamID = -1;

		[NonSerialized]
		public apControlParam _keyControlParam = null;



		//3. KeyFrame으로 정의될 때
		//이 값은 Modifier에 있는게 아니라 Keyframe 데이터에 포함된다.
		public int _keyAnimClipID = -1;
		public int _keyAnimTimelineID = -1;
		public int _keyAnimTimelineLayerID = -1;


		[NonSerialized]
		public apAnimClip _keyAnimClip = null;

		[NonSerialized]
		public apAnimTimeline _keyAnimTimeline = null;

		[NonSerialized]
		public apAnimTimelineLayer _keyAnimTimelineLayer = null;


		//추가
		//AnimClip이면
		//이게 재생중일 때에만 처리가 가능하다
		//처리 순서도 봐야한다.
		public bool IsAnimEnabled
		{
			get
			{
				return _keyAnimClip != null && _keyAnimClip._parentPlayUnit != null && _keyAnimClip._parentPlayUnit.IsUpdatable;
			}
		}



		[SerializeField]
		public List<apOptParamSet> _paramSetList = new List<apOptParamSet>();

		//해당 파라미터가 적용 중인지 체크한다.
		[SerializeField]
		public bool _isEnabled = true;

		// 추가 - 레이어
		public int _layerIndex = 0;//레이어 값이 낮을 수록 먼저 계산된다. (주의 : Anim 타입인 경우 실행 순서로 LayerIndex를 수정한다)
		public float _layerWeight = 0.0f;//(주의 : Anim 타입인 경우 Weight가 Fade 값으로 대체된다)

		
		public apModifierParamSetGroup.BLEND_METHOD _blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;


		/// <summary>
		/// Color/Visible을 제외하는 Modifier라 할지라도 ParamSetGroup에서 색상 옵션이 꺼져있으면 색상이 계산되지 않는다.
		/// </summary>
		[SerializeField]
		public bool _isColorPropertyEnabled = true;


		//추가 20.2.24 : 알파 변화 없이 Hide <-> Show만 토글되는 경우, Alpha Blending이 아닌 Weight 0.5를 기점으로 아예 토글되어야 한다.
		//기본값 false
		[SerializeField]
		public bool _isToggleShowHideWithoutBlend = false;

		// 삭제 19.5.20 : 이 변수는 더이상 사용되지 않는다.
		//[SerializeField]
		//public List<apOptParamSetGroupVertWeight> _calculatedWeightedVertexList = new List<apOptParamSetGroupVertWeight>();


		//추가 21.9.1 : 회전 옵션 (컨트롤 파라미터만 해당)
		public enum TF_ROTATION_LERP_METHOD : int
		{
			/// <summary>기본값. 값 그대로 보간하기</summary>
			Default = 0,
			/// <summary>최소 범위로 회전</summary>
			RotationByVector = 1
		}

		[SerializeField]
		public TF_ROTATION_LERP_METHOD _tfRotationLerpMethod = TF_ROTATION_LERP_METHOD.Default;




		// Init
		//--------------------------------------------
		public apOptParamSetGroup()
		{

		}

		public void Bake(apPortrait portrait, apOptModifierUnitBase parentModifier, apModifierParamSetGroup srcParamSetGroup, bool isAnimated, bool isUseModMeshSet)
		{
			_portrait = portrait;
			_parentModifier = parentModifier;

			_syncTarget = srcParamSetGroup._syncTarget;

			//_keyControlParamName = srcParamSetGroup._keyControlParamName;
			_keyControlParamID = srcParamSetGroup._keyControlParamID;
			_keyControlParam = null;//<<이건 링크로 해결하자

			//애니메이션 값도 넣어주자
			_keyAnimClipID = srcParamSetGroup._keyAnimClipID;
			_keyAnimTimelineID = srcParamSetGroup._keyAnimTimelineID;
			_keyAnimTimelineLayerID = srcParamSetGroup._keyAnimTimelineLayerID;
			_keyAnimClip = null;
			_keyAnimTimeline = null;
			_keyAnimTimelineLayer = null;

			_paramSetList.Clear();

			for (int i = 0; i < srcParamSetGroup._paramSetList.Count; i++)
			{
				apModifierParamSet srcParamSet = srcParamSetGroup._paramSetList[i];

				apOptParamSet optParamSet = new apOptParamSet();
				optParamSet.LinkParamSetGroup(this, portrait);
				optParamSet.BakeModifierParamSet(srcParamSet, portrait, isUseModMeshSet);


				_paramSetList.Add(optParamSet);
			}

			_isEnabled = srcParamSetGroup._isEnabled;
			_layerIndex = srcParamSetGroup._layerIndex;
			_layerWeight = srcParamSetGroup._layerWeight;
			if (!isAnimated)
			{
				_blendMethod = srcParamSetGroup._blendMethod;
			}
			else
			{
				_blendMethod = apModifierParamSetGroup.BLEND_METHOD.Additive;//<<애니메이션에서는 Additive 강제
			}

			_isColorPropertyEnabled = srcParamSetGroup._isColorPropertyEnabled;//<<추가.
			_isToggleShowHideWithoutBlend = srcParamSetGroup._isToggleShowHideWithoutBlend;//<<추가.

			// 삭제 19.5.20 : _calculatedWeightedVertexList 변수 삭제
			//_calculatedWeightedVertexList.Clear();

			//for (int i = 0; i < srcParamSetGroup._calculatedWeightedVertexList.Count; i++)
			//{
			//	apModifierParamSetGroupVertWeight srcWV = srcParamSetGroup._calculatedWeightedVertexList[i];

			//	apOptParamSetGroupVertWeight optWV = new apOptParamSetGroupVertWeight();
			//	optWV.Bake(srcWV);

			//	optWV.Link(portrait.GetOptTransform(optWV._meshTransform_ID));//OptTransform을 연결한다.

			//	_calculatedWeightedVertexList.Add(optWV);
			//}


			//추가 21.9.2 : 회전 보정 방식
			_tfRotationLerpMethod = TF_ROTATION_LERP_METHOD.Default;

			switch (srcParamSetGroup._tfRotationLerpMethod)
			{
				case apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.Default:
					_tfRotationLerpMethod = TF_ROTATION_LERP_METHOD.Default;
					break;

				case apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector:
					_tfRotationLerpMethod = TF_ROTATION_LERP_METHOD.RotationByVector;
					break;
			}
			


			LinkPortrait(portrait, parentModifier);
		}

		public void LinkPortrait(apPortrait portrait, apOptModifierUnitBase parentModifier)
		{
			_portrait = portrait;
			_parentModifier = parentModifier;

			switch (_syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Static:
					break;

				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					//_keyControlParam = _portrait.GetControlParam(_keyControlParamName);
					_keyControlParam = _portrait.GetControlParam(_keyControlParamID);
					break;

				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
					_keyAnimClip = _portrait.GetAnimClip(_keyAnimClipID);
					if (_keyAnimClip == null)
					{
						Debug.LogError("Error : No AnimClip [" + _keyAnimClipID + "]");
						break;
					}

					_keyAnimTimeline = _keyAnimClip.GetTimeline(_keyAnimTimelineID);
					if (_keyAnimTimeline == null)
					{
						Debug.LogError("Error : No AnimTimeline [" + _keyAnimTimelineID + "]");
						break;
					}

					_keyAnimTimelineLayer = _keyAnimTimeline.GetTimelineLayer(_keyAnimTimelineLayerID);

					if (_keyAnimTimelineLayer == null)
					{
						Debug.LogError("Error : No AnimTimelineLayer [" + _keyAnimTimelineLayerID + "]");
						break;
					}

					break;

				default:
					Debug.LogError("apOptParamSetGroup : 알수 없는 타입 : " + _syncTarget);
					break;
			}

			for (int i = 0; i < _paramSetList.Count; i++)
			{
				_paramSetList[i].LinkParamSetGroup(this, portrait);
			}

			// 삭제 19.5.20 : _calculatedWeightedVertexList 변수 삭제
			//for (int i = 0; i < _calculatedWeightedVertexList.Count; i++)
			//{
			//	_calculatedWeightedVertexList[i].Link(portrait.GetOptTransform(_calculatedWeightedVertexList[i]._meshTransform_ID));
			//}
		}



		public IEnumerator LinkPortraitAsync(apPortrait portrait, apOptModifierUnitBase parentModifier, apAsyncTimer asyncTimer)
		{
			_portrait = portrait;
			_parentModifier = parentModifier;

			switch (_syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Static:
					break;

				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					//_keyControlParam = _portrait.GetControlParam(_keyControlParamName);
					_keyControlParam = _portrait.GetControlParam(_keyControlParamID);
					break;

				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
					_keyAnimClip = _portrait.GetAnimClip(_keyAnimClipID);
					if (_keyAnimClip == null)
					{
						Debug.LogError("Error : No AnimClip [" + _keyAnimClipID + "]");
						break;
					}

					_keyAnimTimeline = _keyAnimClip.GetTimeline(_keyAnimTimelineID);
					if (_keyAnimTimeline == null)
					{
						Debug.LogError("Error : No AnimTimeline [" + _keyAnimTimelineID + "]");
						break;
					}

					_keyAnimTimelineLayer = _keyAnimTimeline.GetTimelineLayer(_keyAnimTimelineLayerID);

					if (_keyAnimTimelineLayer == null)
					{
						Debug.LogError("Error : No AnimTimelineLayer [" + _keyAnimTimelineLayerID + "]");
						break;
					}

					break;

				default:
					Debug.LogError("apOptParamSetGroup : 알수 없는 타입 : " + _syncTarget);
					break;
			}

			for (int i = 0; i < _paramSetList.Count; i++)
			{
				yield return _paramSetList[i].LinkParamSetGroupAsync(this, portrait, asyncTimer);
			}

			//Async Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
		}


		// Functions
		//--------------------------------------------
		/// <summary>
		/// Animation인 경우 실행 순서와 Weight가 실시간으로 바뀐다.
		/// Weight는 상관없지만 재생 순서가 바뀐 경우 Sort를 다시 해야한다.
		/// Sort가 필요한 경우 True를 리턴한다.
		/// </summary>
		/// <returns></returns>
		public bool UpdateAnimLayer()
		{
			if (!IsAnimEnabled)
			{
				//애니메이션이 아니거나 실행중이 아니다
				_layerWeight = 0.0f;
				if (_layerIndex != -10)
				{
					_layerIndex = -10;
					return true;
				}
				return false;
			}

			//TODO : Mecanim일때는 어떻게 할까..

			//PlayUnit의 Weight를 가져온다.
			_layerWeight = Mathf.Clamp01(_keyAnimClip._parentPlayUnit.UnitWeight);
			
			switch (_keyAnimClip._parentPlayUnit.BlendMethod)
			{
				case apAnimPlayUnit.BLEND_METHOD.Interpolation:
					_blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;
					break;

				case apAnimPlayUnit.BLEND_METHOD.Additive:
					_blendMethod = apModifierParamSetGroup.BLEND_METHOD.Additive;
					break;
			}
			//이전
			//if (_layerIndex != _keyAnimClip._parentPlayUnit._playOrder)
			//{
			//	_layerIndex = _keyAnimClip._parentPlayUnit._playOrder;
			//	return true;//<Sort가 필요하다.
			//}

			//변경 20.4.18 : PlayOrder 정보 + 레이어 값도 반영되어야 Sort가 정상적으로 된다.
			int nextLayerIndex = (_keyAnimClip._parentPlayUnit._layer * 100) + _keyAnimClip._parentPlayUnit._playOrder;
			if (_layerIndex != nextLayerIndex)
			{
				_layerIndex = nextLayerIndex;
				return true;//<Sort가 필요하다.
			}

			return false;
		}



		// Get / Set
		//--------------------------------------------
		// 삭제 19.5.20 : _calculatedWeightedVertexList 변수 삭제
		//public apOptParamSetGroupVertWeight GetWeightVertexData(apOptTransform targetOptTransform)
		//{
		//	return _calculatedWeightedVertexList.Find(delegate (apOptParamSetGroupVertWeight a)
		//	{
		//		return a._optTransform == targetOptTransform;
		//	});
		//}
	}

}