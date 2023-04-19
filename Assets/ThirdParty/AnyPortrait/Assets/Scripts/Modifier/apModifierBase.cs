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
//using UnityEngine.Profiling;


using AnyPortrait;
using System.Runtime.InteropServices;

namespace AnyPortrait
{

	[Serializable]
	public class apModifierBase : MonoBehaviour
	{
		// Members
		//----------------------------------------------
		public enum MODIFIER_TYPE
		{
			Base = 0,
			Volume = 1,
			Morph = 2,
			AnimatedMorph = 3,
			Rigging = 4,
			Physic = 5,
			TF = 6,
			AnimatedTF = 7,
			FFD = 8,
			AnimatedFFD = 9,
			
			//추가 21.7.20 : 색상 전용 모디파이어 추가
			ColorOnly = 10,
			AnimatedColorOnly = 11,
		}

		/// <summary>
		/// 다른 Modifier와 Blend 되는 방식
		/// </summary>
		public enum BLEND_METHOD
		{
			/// <summary>기존 값을 유지하면서 변화값을 덮어 씌운다.</summary>
			Additive = 0,
			/// <summary>기존 값과 선형 보간을 하며 덮어씌운다.</summary>
			Interpolation = 1
		}

		[NonSerialized]
		public apPortrait _portrait = null;

		//고유 ID. 모디파이어도 고유 아이디를 갖는다.
		public int _uniqueID = -1;

		//레이어
		public int _layer = -1;//낮을수록 먼저 처리된다. (오름차순으로 배열)

		//레이어 병합시 가중치 (0~1)
		public float _layerWeight = 0.0f;

		//메인 MeshGroup
		public int _meshGroupUniqueID = -1;

		//지금 처리가 되는가
		[NonSerialized]
		public bool _isActive = true;


		public enum MOD_EDITOR_ACTIVE
		{
			//삭제 및 변경
			//Enabled = 0,
			//ExclusiveEnabled = 1,
			//Disabled = 2,
			//SubExEnabled = 3,//<<추가 : 기본적으로는 Disabled이다. 다만, 대상이 "선택한 Mod"에 "등록되지 않은 경우"에는 이 Modifier가 계산될 수 있다.
			//OnlyColorEnabled = 4,//<<추가 : 기본적으로는 Disabled이다. Color만 업데이트되는 ParamSetGroup이 한개 이상 존재하는 경우

			//변경 21.2.14 : 편집 모드에 대한 값이 바뀐다.
			/// <summary>편집 모드가 아니다. isActive가 true라면 항상 실행</summary>
			Enabled_Run = 0,
			/// <summary>현재 편집중인 모디파이어/PSG/AnimTimeline이다.</summary>
			Enabled_Edit = 1,
			/// <summary>편집 중인 모디파이어는 아니지만, 등록 여부 상관없이 동작하는 Rigging과 Physics(제한적)이다.</summary>
			Enabled_Background = 2,
			/// <summary>편집 중이 아니어서 적용되지 않는다. 다만, 색상은 적용한다.</summary>
			Disabled_ExceptColor = 3,
			/// <summary>편집 중이 아니어서 적용되지 않는다. 옵션에 따라선 이 상태만 실행될 수 있다.</summary>
			Disabled_NotEdit = 4,
			/// <summary>편집 여부에 상관없이 무조건 실행되지 않는다. 이 값은 PSG가 아닌 Modifier에만 적용되며, 다른 옵션에 의해서도 동작하지 않는다.</summary>
			Disabled_Force = 5,
		}

		[NonSerialized]
		public MOD_EDITOR_ACTIVE _editorExclusiveActiveMod = MOD_EDITOR_ACTIVE.Enabled_Run;//<<에디터에서만 적용되는 배제에 따른 적용 여부



		[NonSerialized]
		public apMeshGroup _meshGroup = null;


		public BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;

		//[SerializeField]
		//public List<apModifierParamSet> _paramSetList = new List<apModifierParamSet>();


		//수정 -> 이것도 Serialize + Layer로 정의합니다.
		[SerializeField]
		public List<apModifierParamSetGroup> _paramSetGroup_controller = new List<apModifierParamSetGroup>();

		//추가 : AnimClip을 키값으로 하는 ParamSetGroup들을 한번에 참조하는 AnimPack
		[NonSerialized]
		public List<apModifierParamSetGroupAnimPack> _paramSetGroupAnimPacks = new List<apModifierParamSetGroupAnimPack>();



		//계산 결과가 저장되는 변수들
		//[NonSerialized]
		//public apCalculatedRenderUnit _calculateResult = null;


		[NonSerialized]
		public Dictionary<apVertex, apMatrix3x3> _vertWorldMatrix = new Dictionary<apVertex, apMatrix3x3>();

		//각 RenderUnit으로 계산 결과를 보내주는 Param들
		[NonSerialized]
		public List<apCalculatedResultParam> _calculatedResultParams = new List<apCalculatedResultParam>();


		//추가
		//색상 값이 일괄적으로 들어가니 값 처리에 문제가 생긴다.
		//설정값으로 켜고 끄게 해야한다.
		/// <summary>
		/// 기본적으로 색상값이 적용되는 모디파이어에서 사용자가 선택적으로 색상은 설정하지 않도록 만들 수 있다.
		/// CalculatedValueType에 Color값이 없다면 이 변수는 의미가 없다.
		/// </summary>
		[SerializeField]
		public bool _isColorPropertyEnabled = true;


		//추가 11.26
		//Depth 변환이나 Texture 변환같은 특수한 값을 가질 수 있는지 여부
		[SerializeField]
		public bool _isExtraPropertyEnabled = false;//<<이건 기본값이 False

		//삭제 20.7.9 : Portrait에서 공통의 타이머를 이용한다.
		//[NonSerialized]
		//protected System.Diagnostics.Stopwatch _stopwatch = null;

		[NonSerialized]
		protected float _tDeltaFixed = 0.0f;

		private const float PHYSIC_DELTA_TIME = 0.033f;//20FPS (0.05), 30FPS (0.033)//<<고정 프레임으로 하자

		
		// Init
		//----------------------------------------------
		//생성자가 없다.
		//public apModifierBase()
		//{
		//	Init();
		//}

		void Start()
		{
			//업데이트 하지 않습니다. 데이터로만 존재
			this.enabled = false;
		}

		public void LinkPortrait(apPortrait portrait)
		{
			_portrait = portrait;
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				_paramSetGroup_controller[i].LinkPortrait(portrait, this);
			}

			_portrait.RegistUniqueID(apIDManager.TARGET.Modifier, _uniqueID);
		}

		//이 함수는 사용하지 않는다. 삭제
		//public virtual void Init()
		//{

		//}

		public virtual void SetInitSetting(int uniqueID, int layer, int meshGroupID, apMeshGroup meshGroup)
		{
			_uniqueID = uniqueID;
			_layer = layer;
			_layerWeight = 1.0f;
			_meshGroupUniqueID = meshGroupID;
			_meshGroup = meshGroup;

			//_paramSetGroup_controller.Clear();//클리어하면 안됩니더;;

			//_calculateResult = new apCalculatedRenderUnit();
			//_calculateResult.SetMeshGroup(_meshGroup);
			//_calculateResult.Clear();

			// 변경 3.23 : 이전과 달리 색상 속성을 초기에 비활성화
			
			_isColorPropertyEnabled = false;
			_isExtraPropertyEnabled = false;

			RefreshParamSet(null);
		}





		/// <summary>
		/// ParamSet의 추가 / 삭제시 한번씩 호출해주자
		/// + Editor / Realtime 첫 시작시 호출 필요
		/// 추가 20.4.3 : targetAnimClip을 추가하면, AnimatedModifier의 경우 targetAnimClip을 대상으로 하지 않을 경우 생략한다.
		/// </summary>
		public virtual void RefreshParamSet(apUtil.LinkRefreshRequest linkRefreshRequest)
		{
			bool isSkipUnselectedAnimPSGs = false;
			apAnimClip curSelectedAnimClip = null;
			if(IsAnimated && linkRefreshRequest != null)
			{
				//Debug.LogWarning("애니메이션 Refresh : " + (linkRefreshRequest == null ? "Null Request" : "AnyRequest"));
				//if(linkRefreshRequest != null)
				//{
				//	Debug.Log("Request MeshGroup : " + linkRefreshRequest.Request_MeshGroup);
				//	Debug.Log("Request Modifier : " + linkRefreshRequest.Request_Modifier);
				//	Debug.Log("Request PSG : " + linkRefreshRequest.Request_PSG);
				//}
				if(linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.AllModifiers_ExceptAnimMods)
				{
					//애니메이션 모디파이어는 처리하지 않는다.
					return;
				}
				if(linkRefreshRequest.Request_PSG == apUtil.LR_REQUEST__PSG.SelectedAnimClipPSG_IfAnimModifier)
				{
					//선택된 AnimClip 외의 PSG는 생략할 수 있다.
					isSkipUnselectedAnimPSGs = true;
					curSelectedAnimClip = linkRefreshRequest.AnimClip;

					//Debug.Log("대상 AnimClip : " + (curSelectedAnimClip != null ? curSelectedAnimClip._name : "(Unknown)"));
				}
			}
				
			
			apModifierParamSetGroup curParamSetGroup = null;

			//if (isSkipUnselectedAnimPSGs)
			//{
			//	//Anim 모디파이어에서 특정 PSG는 생략한다.
			//	//(애니메이션 모디파이어가 아니거나 대상이 되는 애니메이션 PSG만 처리함)
			//	for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			//	{
			//		curParamSetGroup = _paramSetGroup_controller[i];

			//		//여기서는 조건에 의한 스킵은 하지 말자.
			//		//단순히 잘못된 데이터를 삭ㅈ하는 것인데 필요할 때가 있다.
			//		//if (curParamSetGroup._keyAnimClip != linkRefreshRequest.AnimClip)
			//		//{
			//		//	//선택된 AnimClip 외에는 스킵하자
			//		//	continue;
			//		//}

			//		curParamSetGroup.RemoveInvalidParamSet();
			//	}
			//}
			//else
			//{
			//	for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			//	{
			//		curParamSetGroup = _paramSetGroup_controller[i];
			//		curParamSetGroup.RemoveInvalidParamSet();
			//	}
			//}

			//여기서 문제가 발생함
			//이전
			//for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			//{
			//	curParamSetGroup = _paramSetGroup_controller[i];
			//	curParamSetGroup.RemoveInvalidParamSet();
			//}

			//변경 21.4.18 : 선택되지 않은 애니메이션에 대해서는 InvalidParamSet 체크를 하지 않는다.
			if (isSkipUnselectedAnimPSGs)
			{
				for (int i = 0; i < _paramSetGroup_controller.Count; i++)
				{
					curParamSetGroup = _paramSetGroup_controller[i];
					//해당하지 않는 애니메이션 클립은 생략한다. 
					if(curParamSetGroup._keyAnimClip != curSelectedAnimClip)
					{
						//Debug.LogWarning("해당되지 않는 애니메이션 클립 생략 : " + curParamSetGroup._keyAnimClip._name);
						continue;
					}
					curParamSetGroup.RemoveInvalidParamSet();
				}
			}
			else
			{
				//전체 체크
				for (int i = 0; i < _paramSetGroup_controller.Count; i++)
				{
					curParamSetGroup = _paramSetGroup_controller[i];
					curParamSetGroup.RemoveInvalidParamSet();
				}
			}
			


			//ParamSet이 없는 경우는 삭제. > 삭제된 데이터를 찾아서 삭제하는 것이므로 모두 해당
			_paramSetGroup_controller.RemoveAll(delegate (apModifierParamSetGroup a)
			{
				return a._paramSetList.Count == 0;
			});


			//추가 : SyncTarget에 따라서 삭제 여부를 체크하자
			_paramSetGroup_controller.RemoveAll(delegate (apModifierParamSetGroup a)
			{
				switch (a._syncTarget)
				{
					case apModifierParamSetGroup.SYNC_TARGET.Static:
						//자동으로 삭제 안됨
						break;

					case apModifierParamSetGroup.SYNC_TARGET.Controller:
						{
							if (a._keyControlParam == null)
							{
								//해당 ControlParam이 없다면 삭제
								return true;
							}
							//ControlParam이 있다고 해도, "현재" 없다면 삭제
							if (!_portrait._controller._controlParams.Contains(a._keyControlParam))
							{
								return true;
							}
						}
						break;

					case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
						//Debug.LogError("TODO : KeyFrame에 따라서 삭제 여부 체크");
						{
							if (a._keyAnimClip == null ||
								a._keyAnimTimeline == null ||
								a._keyAnimTimelineLayer == null)
							{
								return true;//<<삭제
							}
						}
						break;
				}
				return false;

			});

			if (isSkipUnselectedAnimPSGs)
			{
				//선택된 애니메이션만 검사
				for (int i = 0; i < _paramSetGroup_controller.Count; i++)
				{
					curParamSetGroup = _paramSetGroup_controller[i];
					if (curParamSetGroup._keyAnimClip != curSelectedAnimClip)
					{
						continue;//스킵
					}
					curParamSetGroup.SortParamSet();
					curParamSetGroup.RefreshSync();
				}
			}
			else
			{
				//전체 검사
				for (int i = 0; i < _paramSetGroup_controller.Count; i++)
				{
					curParamSetGroup = _paramSetGroup_controller[i];
					curParamSetGroup.SortParamSet();
					curParamSetGroup.RefreshSync();
				}
			}



			if (IsAnimated)
			{
				//추가 : AnimPack에 대한 ParamSetGroup을 다시 만들어주자
				if (_paramSetGroupAnimPacks == null)
				{
					_paramSetGroupAnimPacks = new List<apModifierParamSetGroupAnimPack>();
				}

				//전체 검사일 경우에만 삭제
				if (!isSkipUnselectedAnimPSGs)
				{	
					_paramSetGroupAnimPacks.RemoveAll(delegate (apModifierParamSetGroupAnimPack a)
					{
						if (!_portrait._animClips.Contains(a.LinkedAnimClip))
						{
							//AnimClip이 없으면 삭제
							return true;
						}
						return false;
					});

					for (int i = 0; i < _paramSetGroupAnimPacks.Count; i++)
					{
						//존재하지 않는 paramSetGroup을 삭제하자
						_paramSetGroupAnimPacks[i].RemoveInvalidParamSetGroup(_paramSetGroup_controller);
					}
				}



				//paramSetGroup을 돌면서 새로 생성 또는 AnimPack에 추가를 해보자
				//Anim 타입인 경우에만
				if (!isSkipUnselectedAnimPSGs)
				{
					//모든 AnimClip 체크
					for (int i = 0; i < _paramSetGroup_controller.Count; i++)
					{
						curParamSetGroup = _paramSetGroup_controller[i];
						if (curParamSetGroup._keyAnimClip == null)
						{
							continue;
						}

						curParamSetGroup = _paramSetGroup_controller[i];
						apModifierParamSetGroupAnimPack targetAnimPack = GetParamSetGroupAnimPack(curParamSetGroup._keyAnimClip);
						if (targetAnimPack == null)
						{
							targetAnimPack = new apModifierParamSetGroupAnimPack(this, curParamSetGroup._keyAnimClip);
							_paramSetGroupAnimPacks.Add(targetAnimPack);
						}

						targetAnimPack.AddParamSetGroup(curParamSetGroup);
					}
				}
				else
				{
					//특정 AnimClip만 체크
					for (int i = 0; i < _paramSetGroup_controller.Count; i++)
					{
						curParamSetGroup = _paramSetGroup_controller[i];
						if (curParamSetGroup._keyAnimClip == null
							|| curParamSetGroup._keyAnimClip != curSelectedAnimClip)//<<이 부분이 추가됨
						{
							continue;
						}

						curParamSetGroup = _paramSetGroup_controller[i];
						apModifierParamSetGroupAnimPack targetAnimPack = GetParamSetGroupAnimPack(curParamSetGroup._keyAnimClip);
						if (targetAnimPack == null)
						{
							targetAnimPack = new apModifierParamSetGroupAnimPack(this, curParamSetGroup._keyAnimClip);
							_paramSetGroupAnimPacks.Add(targetAnimPack);
						}

						targetAnimPack.AddParamSetGroup(curParamSetGroup);
					}
				}
			}

			//그리고 정렬 (전체 검사일 때만)
			if(!isSkipUnselectedAnimPSGs)
			{
				SortParamSetGroups();
			}
		}

		public apModifierParamSetGroup GetParamSetGroup(apControlParam keyControlParam)
		{
			return _paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
			{
				return a._keyControlParam == keyControlParam;
			});
		}

		public apModifierParamSetGroupAnimPack GetParamSetGroupAnimPack(apAnimClip animClip)
		{
			return _paramSetGroupAnimPacks.Find(delegate (apModifierParamSetGroupAnimPack a)
			{
				return a.LinkedAnimClip == animClip;
			});
		}

		/// <summary>
		/// ParamSetGroup을 LayerIndex에 따라 Sort하고, Index를 넣어준다.
		/// </summary>
		public void SortParamSetGroups()
		{
			_paramSetGroup_controller.Sort(delegate (apModifierParamSetGroup a, apModifierParamSetGroup b)
			{
				return a._layerIndex - b._layerIndex;
			});

			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				_paramSetGroup_controller[i]._layerIndex = i;
			}
		}

		public int GetNextParamSetLayerIndex()
		{
			int maxIndex = -1;
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				if (maxIndex < _paramSetGroup_controller[i]._layerIndex)
				{
					maxIndex = _paramSetGroup_controller[i]._layerIndex;
				}
			}
			return maxIndex + 1;
		}

		public void ChangeParamSetGroupLayerIndex(apModifierParamSetGroup paramSetGroup, int nextIndex)
		{
			bool isIncrese = true;
			if (nextIndex < paramSetGroup._layerIndex)
			{
				isIncrese = false;
			}

			paramSetGroup._layerIndex = nextIndex;

			//해당 Index를 기준으로 작으면 -1, 크면 1을 더한다.
			//해당 Index의 기존 객체에 대해서
			//Increase이면 1 감소,
			//Decrease이면 1 증가
			for (int i = 0; i < _paramSetGroup_controller.Count; i++)
			{
				apModifierParamSetGroup curParamSetGroup = _paramSetGroup_controller[i];

				if (curParamSetGroup == paramSetGroup)
				{
					continue;
				}

				if (curParamSetGroup._layerIndex == nextIndex)
				{
					if (isIncrese)
					{
						curParamSetGroup._layerIndex--;
					}
					else
					{
						curParamSetGroup._layerIndex++;
					}
				}
				else if (curParamSetGroup._layerIndex < nextIndex)
				{
					curParamSetGroup._layerIndex--;
				}
				else
				{
					curParamSetGroup._layerIndex++;
				}

			}

			//그리고 재정렬
			SortParamSetGroups();
		}

		// Get / Set
		//----------------------------------------------
		public virtual MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Base; }
		}


		public virtual apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.Static; }
		}

		private const string NAME_BASE_MODIFIER = "Base Modifier";

		public virtual string DisplayName
		{
			//get { return "Base Modifier"; }
			get { return NAME_BASE_MODIFIER; }
		}

		public virtual string DisplayNameShort
		{
			//get { return "Base Modifier"; }
			get { return NAME_BASE_MODIFIER; }
		}

		/// <summary>
		/// Calculate 계산시 어떤 값을 사용하는가 (저장과 관련없이 처리 결과만 본다)
		/// </summary>
		public virtual apCalculatedResultParam.CALCULATED_VALUE_TYPE CalculatedValueType
		{
			get { return apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos; }
		}

		/// <summary>
		/// Calculate 계산시 어느 단계에서 값이 처리되는가
		/// </summary>
		public virtual apCalculatedResultParam.CALCULATED_SPACE CalculatedSpace
		{
			get { return apCalculatedResultParam.CALCULATED_SPACE.Object; }
		}

		/// <summary>
		/// Modified Mesh에 저장되는 데이터의 종류 (Calculated 처리 전이므로 범위가 조금 더 넓다)
		/// 중복 처리가 가능하다 (switch 불가)
		/// </summary>
		public virtual apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get { return apModifiedMesh.MOD_VALUE_TYPE.Unknown; }
		}

		//public virtual apModifiedMesh.TARGET_TYPE ModifiedTargetType
		//{
		//	get { return apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform; }
		//}

		public virtual bool IsTarget_MeshTransform { get { return false; } }
		public virtual bool IsTarget_MeshGroupTransform { get { return false; } }
		public virtual bool IsTarget_Bone { get { return false; } }
		public virtual bool IsTarget_ChildMeshTransform { get { return false; } }//<<객체 상관없이 Child MeshTransform에 대해서도 값을 넣을 수 있다.

		public virtual bool IsAnimated { get { return false; } }



		/// <summary>
		/// Update는 RenderUnit 갱신전에 하는 Pre-Update와 Bone Matrix까지 계산한 후에 처리되는 Post-Update가 있다.
		/// 대부분은 PreUpdate이며, Rigging, Physic과 같은 경우엔 Post Update이다.
		/// </summary>
		public virtual bool IsPreUpdate { get { return true; } }

		//이전
		//public bool IsPhysics { get { return (int)(ModifierType & MODIFIER_TYPE.Physic) != 0; } }
		//public bool IsVolume { get { return (int)(ModifierType & MODIFIER_TYPE.Volume) != 0; } }

		//변경
		public virtual bool IsPhysics { get { return false; } }
		public virtual bool IsVolume { get { return false; } }

		public virtual bool IsUseParamSetWeight { get { return false; } }

		//삭제 21.2.17 : 모디파이어 잠금이 사라지면이 이 옵션은 필요없게 되었다.
		///// <summary>
		///// ExEdit 중 GeneralEdit 모드에서 "동시에 작업 가능하도록 허용 된 Modifier Type들"을 리턴한다.
		///// 매번 만들지 말고 멤버 변수로 만들어서 넣자
		///// </summary>
		///// <returns></returns>
		//public virtual MODIFIER_TYPE[] GetGeneralExEditableModTypes()
		//{
		//	return new MODIFIER_TYPE[] { ModifierType };
		//}


		// Find
		//-------------------------------------------------------------
		//public List<apCalculatedResultParam> _calculatedResultParams = new List<apCalculatedResultParam>();
		/// <summary>
		/// CalculateParam을 찾는다.
		/// 적용되는 RenderUnit을 키값으로 삼으며, Bone은 Null인 대상만 고려한다.
		/// </summary>
		/// <param name="targetRenderUnit"></param>
		/// <returns></returns>
		public apCalculatedResultParam GetCalculatedResultParam(apRenderUnit targetRenderUnit)
		{
			return _calculatedResultParams.Find(delegate (apCalculatedResultParam a)
			{
				return a._targetRenderUnit == targetRenderUnit && a._targetBone == null;
			});
		}


		/// <summary>
		/// 추가 : GetCalculatedResultParam 타입의 ModBone 버전.
		/// Bone까지 비교하여 동일한 CalculateResultParam을 찾는다.
		/// </summary>
		/// <param name="targetRenderUnit"></param>
		/// <returns></returns>
		public apCalculatedResultParam GetCalculatedResultParam_Bone(apRenderUnit targetRenderUnit, apBone bone, apRenderUnit ownerRenderUnit)
		{
			return _calculatedResultParams.Find(delegate (apCalculatedResultParam a)
			{
				//return a._targetRenderUnit == targetRenderUnit && a._targetBone == bone;
				return a._targetRenderUnit == targetRenderUnit && a._targetBone == bone && a._ownerRenderUnit == ownerRenderUnit;
			});
		}

		// Functions
		//----------------------------------------------

		public virtual void InitCalculate(float tDelta)
		{
			//계산이 불가능한 상황일 때, 계산 값만 초기화한다.

		}

		public virtual void Calculate(float tDelta)
		{
			//TODO
			//ParamSet을 계산한 후
			//Dictionay에 [Vertex / WorldMatrix] 를 만들어 넣는다.
			//
			//_calculateResult.ReadyToCalculate();
			//..오버라이드!
		}

		public virtual void Calculate_DLL(float tDelta)
		{
			//TODO
			//ParamSet을 계산한 후
			//Dictionay에 [Vertex / WorldMatrix] 를 만들어 넣는다.
			//
			//_calculateResult.ReadyToCalculate();
			//..오버라이드!
		}




		// Add / Remove
		//----------------------------------------------
		public virtual void AddParamSet()
		{

		}

		public virtual void RemoveParamSet(apModifiedVertex modVertex)
		{

		}


		//----------------------------------------------


		//TODO : Bone

		// 일부 파라미터에만 넣기
		//---------------------------------------------------------------
		/// <summary>
		/// MeshTransform을 해당 ParamSet에 ModMesh의 형태로 넣는다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="meshTransform"></param>
		/// <param name="targetParamSet"></param>
		/// <param name="isExclusive">meshData리스트에 단 한개만 넣는 경우에는 True. 기본값은 false</param>
		/// <param name="isRecursiveAvailable">True이면 해당 MeshGroup이 아닌 하위 MeshGroup의 Transform을 허용한다. 기본값은 false</param>
		/// <param name="isRefreshLink">Link를 다시 한다. 기본값은 true</param>
		/// <param name="isUseMeshDefaultColor">색상 기본값으로 메시 기본 값을 이용</param>
		/// <returns></returns>
		public apModifiedMesh AddMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apModifierParamSet targetParamSet,
												//bool isExclusive = false, bool isRecursiveAvailable = false, bool isRefreshLink = true, bool isUseMeshDefaultColor = true
												bool isExclusive, bool isRecursiveAvailable, bool isRefreshLink
												)
		{
			//현재 타입에서 추가 가능한가.
			if (!IsTarget_MeshTransform)
			{
				return null;
			}
			//if (ModifiedTargetType != apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform &&
			//	ModifiedTargetType != apModifiedMesh.TARGET_TYPE.MeshTransformOnly)
			//{
			//	return null;
			//}

			apMeshGroup parentMeshGroupOfTransform = null;


			if (isRecursiveAvailable)
			{
				if (meshGroup._childMeshTransforms.Contains(meshTransform))
				{
					//이 MeshGroup에 포함된다면
					parentMeshGroupOfTransform = meshGroup;
				}
				else
				{
					//그렇지 않다면 모든 MeshGroup을 기준으로 검색하자
					for (int i = 0; i < _portrait._meshGroups.Count; i++)
					{
						if (_portrait._meshGroups[i]._childMeshTransforms.Contains(meshTransform))
						{
							//찾았다!
							parentMeshGroupOfTransform = _portrait._meshGroups[i];
							break;
						}
					}
				}

				if (parentMeshGroupOfTransform == null)
				{
					//못찾았다..
					return null;
				}
			}
			else
			{
				//Recursive한 Transform 접근을 허용하지 않느다.

				//MeshGroup이 Mesh Transform을 가지고 있지 않으면 실패
				if (!meshGroup._childMeshTransforms.Contains(meshTransform))
				{
					return null;
				}

				parentMeshGroupOfTransform = meshGroup;
			}

			if (meshTransform._mesh == null)
			{
				return null;
			}

			apRenderUnit renderUnit = null;

			//Child MeshTransform을 허용하늗가
			if (IsTarget_ChildMeshTransform)
			{
				//재귀적으로 모든 Child MeshTransform을 허용한다.
				renderUnit = meshGroup.GetRenderUnit(meshTransform);
			}
			else
			{
				//현재 MeshGroup의 MeshTransform만 허용한다.
				renderUnit = meshGroup.GetRenderUnit_NoRecursive(meshTransform);
			}

			if (renderUnit == null)
			{
				return null;
			}

			apModifiedMesh modMesh = targetParamSet._meshData.Find(delegate (apModifiedMesh a)
				{
					return a.IsContains_MeshTransform(meshGroup, meshTransform, meshTransform._mesh);
				});


			if (modMesh == null)
			{
				//Debug.Log("Add Mod Mesh - MeshTransform");

				modMesh = new apModifiedMesh();

				modMesh.Init(meshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, ModifiedValueType);

				if (IsTarget_MeshTransform)
				{
					modMesh.SetTarget_MeshTransform(meshTransform._transformUniqueID, meshTransform._mesh._uniqueID, meshTransform._meshColor2X_Default, meshTransform._isVisible_Default);
					modMesh.Link_MeshTransform(meshGroup, parentMeshGroupOfTransform, meshTransform, renderUnit, _portrait);
				}


				targetParamSet._meshData.Add(modMesh);
			}

			if (isExclusive)
			{
				//MeshTransform에 해당하지 않는 ModMesh는 아예 삭제한다.
				//int nRemoved = targetParamSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				targetParamSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a._transform_Mesh != meshTransform;
				});

				//if (nRemoved > 0)
				//{
				//	//테스트
				//	Debug.LogError("ModMesh Removed (Exclusive/MeshTransform) : " + nRemoved + "[" + DisplayName + "]");
				//}
			}

			if (isRefreshLink)
			{
				_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(meshGroup, this));
			}


			return modMesh;
		}

		/// <summary>
		/// MeshGroupTransform을 해당 ParamSet에 ModMesh의 형태로 넣는다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="meshGroupTransform"></param>
		/// <param name="targetParamSet"></param>
		/// <param name="isExclusive">meshData리스트에 단 한개만 넣는 경우에는 True</param>
		/// <param name="isRecursiveAvailable">True이면 해당 MeshGroup이 아닌 하위 MeshGroup의 Transform을 허용한다. 기본값은 false</param>
		/// <returns></returns>
		public apModifiedMesh AddMeshGroupTransform(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform, apModifierParamSet targetParamSet,
													bool isExclusive = false, bool isRecursiveAvailable = false, bool isRefreshLink = true)
		{
			//현재 타입에서 추가 가능한가.
			if (!IsTarget_MeshGroupTransform)
			{
				return null;
			}
			//if(ModifiedTargetType != apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly)
			//{
			//	return null;
			//}



			apMeshGroup parentMeshGroupOfTransform = null;


			if (isRecursiveAvailable)
			{
				if (meshGroup._childMeshGroupTransforms.Contains(meshGroupTransform))
				{
					//이 MeshGroup에 포함된다면
					parentMeshGroupOfTransform = meshGroup;
				}
				else
				{
					//그렇지 않다면 모든 MeshGroup을 기준으로 검색하자
					for (int i = 0; i < _portrait._meshGroups.Count; i++)
					{
						if (_portrait._meshGroups[i]._childMeshGroupTransforms.Contains(meshGroupTransform))
						{
							//찾았다!
							parentMeshGroupOfTransform = _portrait._meshGroups[i];
							break;
						}
					}
				}

				if (parentMeshGroupOfTransform == null)
				{
					//못찾았다..
					return null;
				}
			}
			else
			{
				//Recursive한 Transform 접근을 허용하지 않느다.

				//MeshGroup이 Mesh Group Transform을 가지고 있지 않으면 실패
				if (!meshGroup._childMeshGroupTransforms.Contains(meshGroupTransform))
				{
					return null;
				}

				parentMeshGroupOfTransform = meshGroup;
			}



			apRenderUnit renderUnit = meshGroup.GetRenderUnit(meshGroupTransform);
			if (renderUnit == null)
			{
				return null;
			}

			apModifiedMesh modMesh = targetParamSet._meshData.Find(delegate (apModifiedMesh a)
			{
				return a.IsContains_MeshGroupTransform(meshGroup, meshGroupTransform);
			});

			if (modMesh == null)
			{
				//Debug.Log("Add Mod Mesh - MeshGroupTransform");

				modMesh = new apModifiedMesh();

				modMesh.Init(meshGroup._uniqueID, parentMeshGroupOfTransform._uniqueID, ModifiedValueType);

				modMesh.SetTarget_MeshGroupTransform(meshGroupTransform._transformUniqueID, meshGroupTransform._meshColor2X_Default, meshGroupTransform._isVisible_Default);

				//modMesh.Init_MeshGroupTransform(meshGroup._uniqueID,
				//								meshGroupTransform._transformUniqueID);

				modMesh.Link_MeshGroupTransform(meshGroup, parentMeshGroupOfTransform, meshGroupTransform, renderUnit);

				targetParamSet._meshData.Add(modMesh);
			}

			if (isExclusive)
			{
				//MeshTransform에 해당하지 않는 ModMesh는 아예 삭제한다.
				targetParamSet._meshData.RemoveAll(delegate (apModifiedMesh a)
				{
					return a._transform_MeshGroup != meshGroupTransform;
				});

				//if (nRemoved > 0)
				//{
				//	//Debug.LogError("ModMesh Removed (Exclusive/MeshGroupTransform) : " + nRemoved + "[" + DisplayName + "]");
				//}
			}

			if (isRefreshLink)
			{
				_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(meshGroup, this));
			}


			return modMesh;
		}



		public apModifiedBone AddBone(apBone bone, apModifierParamSet targetParamSet,
													bool isRecursiveAvailable = false, bool isRefreshLink = true)
		{
			if (!IsTarget_Bone)
			{
				return null;
			}

			if (bone == null)
			{
				return null;
			}


			apMeshGroup meshGroupOfBone = bone._meshGroup;
			//해당 MeshGroup이 Modifier의 MeshGroup에 포함되는가
			apMeshGroup meshGroupOfModifier = _meshGroup;

			if (meshGroupOfBone == null)
			{
				Debug.LogError("AnyPortrait : AddBone Failed : MeshGroup [" + (meshGroupOfBone != null) + "]");
				return null;
			}
			apTransform_MeshGroup meshGroupTransform = null;
			if (meshGroupOfBone == meshGroupOfModifier)
			{
				meshGroupTransform = meshGroupOfModifier._rootMeshGroupTransform;
			}
			else
			{
				meshGroupTransform = meshGroupOfModifier.FindChildMeshGroupTransform(meshGroupOfBone);
			}
			if (meshGroupTransform == null)
			{
				Debug.LogError("AnyPortrait : AddBone Failed : MeshGroupTF [" + (meshGroupTransform != null) + "] <" + meshGroupOfBone._name + " : " + meshGroupOfModifier._name + ">");
				return null;
			}

			apRenderUnit renderUnit = meshGroupOfModifier.GetRenderUnit(meshGroupTransform);
			if (renderUnit == null)
			{
				Debug.LogError("AnyPortrait : AddBone Failed : No Render Unit");
				return null;
			}

			//이미 존재하는지 확인
			apModifiedBone modBone = targetParamSet._boneData.Find(delegate (apModifiedBone a)
			{
				return a._boneID == bone._uniqueID;
			});

			if (modBone == null)
			{
				modBone = new apModifiedBone();
				modBone.Init(meshGroupOfModifier._uniqueID, meshGroupOfBone._uniqueID, meshGroupTransform._transformUniqueID, bone);
				modBone.Link(meshGroupOfModifier, meshGroupOfBone, bone, renderUnit, meshGroupTransform);

				targetParamSet._boneData.Add(modBone);
			}

			if (isRefreshLink)
			{
				_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(meshGroupOfModifier, this));
			}

			return modBone;

		}
		
		
		//--------------------------------------------------------------------------------
		// Modifier에서 Calculate 함수에서 사용할 수 있는 공통 패턴
		//--------------------------------------------------------------------------------

		//계산용 임시 변수들
		private static apCalculatedResultParam _cal_curCalParam = null;
		private static Vector2[] _cal_ResultPosList = null;
		private static apMatrix3x3[] _cal_ResultVertMatrixList = null;

		//초기화하지 말고 생성후엔 계속 재사용해야하는 4개의 변수 (중요)
		//이건 Static으로 만들지 않는다.
		private apMatrixCal _cal_TmpMatrix = null;
		private Vector2[] _cal_TmpPosList = null;
		private apMatrix3x3[] _cal_TmpVertMatrixList = null;
		private float[] _cal_TmpVertWeightList = null;

		private static int _cal_NumVerts = 0;

		//추가 22.3.28 [v1.4.0] Pin 계산 변수
		private static Vector2[] _cal_ResultPinPosList = null;
		private Vector2[] _cal_TmpPinPosList = null;
		private int _cal_NumPins = 0;
		private apMeshPinGroup _cal_TargetPinGroup = null;
		


		private static List<apCalculatedResultParamSubList> _cal_SubParamGroupList = null;
		private static List<apCalculatedResultParam.ParamKeyValueSet> _cal_SubParamKeyValueList = null;
		private static float _cal_LayerWeight = 0.0f;
		private static apModifierParamSetGroup _cal_KeyParamSetGroup = null;
		private static apCalculatedResultParamSubList _cal_CurSubList = null;
		private static int _cal_NumParamKeys = 0;
		private static apCalculatedResultParam.ParamKeyValueSet _cal_ParamKeyValue = null;
		private static bool _cal_IsColorProperty = false;//색상을 지원하는 Modifier인가
		private static bool _cal_IsUseParamSetWeight = false;//ParamSetWeight를 사용하는가
		private static Color _cal_TmpColor = Color.clear;
		private static bool _cal_TmpVisible = false;
		private static bool _cal_TmpIsToggleShowHideOption = false;//추가 20.2.22 : Show/Hide 토글을 할 수 있다.
		private static bool _cal_TmpToggleOpt_IsAnyKey_Shown = false;
		private static float _cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
		private static float _cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
		private static float _cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
		private static bool _cal_TmpToggleOpt_IsAny_Hidden = false;
		//private float _cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
		private static float _cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
		private static float _cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;
		private static float _cal_TmpToggleOpt_KeyIndex_Cal = 0.0f;

		private static bool _cal_TmpExtra_DepthChanged = false;//추가 11.29 : Extra Option 계산 값
		private static bool _cal_TmpExtra_TextureChanged = false;
		private static int _cal_TmpExtra_DeltaDepth = 0;
		private static int _cal_TmpExtra_TextureDataID = 0;
		private static apTextureData _cal_TmpExtra_TextureData = null;
		private static float _cal_TmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
		private static float _cal_TmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
		
		private static int _cal_iCalculatedSubParam_Main = 0;
		
		private static int _cal_iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
		private static bool _cal_TmpIsColoredKeyParamSetGroup = false;
		private static bool _cal_isExCalculatable_Transform = false;
		private static bool _cal_isExCalculatable_Color = false;
		
		//추가 21.9.1 : 회전값 보정
		private static bool _cal_isRotation180Correction = false;
		private static float _cal_Rotation180Correction_DeltaAngle = 0.0f;
		private static Vector2 _cal_Rotation180Correction_SumVector = Vector2.zero;
		private static Vector2 _cal_Rotation180Correction_CurVector = Vector2.zero;
		

		private static bool _cal_isFirstParam = true;
		private static float _cal_TotalParamSetWeight = 0.0f;
		private static int _cal_NumCalculated = 0;

		private static bool _cal_isBoneTarget = false;
		private static bool _cal_isRiggingWithIK = false;
		private static apMatrix3x3 _cal_Rig_matx_Vert2Local = apMatrix3x3.identity;
		private static apMatrix3x3 _cal_Rig_matx_Vert2Local_Inv = apMatrix3x3.identity;
		private static apMatrix _cal_tmpRig_matx_MeshW_NoMod = null; 
		private static apModifiedVertexRig _cal_Rig_curVertRig = null;
		private static Vector2 _cal_Rig_VertPosW_NoMod;
		private static float _cal_Rig_TotalBoneWeight;
		private static apModifiedVertexRig.WeightPair _cal_Rig_WeightPair = null;
		private static apBoneWorldMatrix _cal_Rig_Matx_boneWorld_Mod = null;
		private static apBoneWorldMatrix _cal_Rig_Matx_boneWorld_Default = null;
		private static Vector2 _cal_Rig_VertPos_BoneLocal;
		private static Vector2 _cal_Rig_VertPosW_BoneWorld;
		private static Vector2 _cal_Rig_VertPosL_Result;
		private static apMatrix3x3 _cal_Rig_Matx_Result;

		//물리 모디파이어 변수들
		//초당 얼마나 업데이트 요청을 받는지 체크
		//private int tmpPhysics_nUpdateCall = 0;
		//private float tmpPhysics_tUpdateCall = 0.0f;
		//private int tmpPhysics_nUpdateValid = 0;

		private static apModifiedVertexWeight _cal_Phy_modVertWeight = null;
		private static apPhysicsVertParam _cal_Phy_physicVertParam = null;
		private static apPhysicsMeshParam _cal_Phy_physicMeshParam = null;
		//private static int _cal_Phy_nVerts = 0;
		private static float _cal_Phy_Mass = 0.0f;
		private static Vector2 _cal_Phy_F_gravity = Vector2.zero;
		private static Vector2 _cal_Phy_F_wind = Vector2.zero;
		private static Vector2 _cal_Phy_F_stretch = Vector2.zero;
		private static Vector2 _cal_Phy_F_recover = Vector2.zero;
		private static Vector2 _cal_Phy_F_ext = Vector2.zero;//<<추가된 "외부 힘"
		private static Vector2 _cal_Phy_F_sum = Vector2.zero;
		//private Vector2 tmpPhysics_F_viscosity = Vector2.zero;
		private apPhysicsVertParam.LinkedVertex tmpPhysics_linkedVert = null;
		private static bool _cal_Phy_isViscosity = false;
		private static Vector2 _cal_Phy_srcVertPos_NoMod = Vector2.zero;
		private static Vector2 _cal_Phy_linkVertPos_NoMod = Vector2.zero;
		private static Vector2 _cal_Phy_srcVertPos_Cur = Vector2.zero;
		private static Vector2 _cal_Phy_linkVertPos_Cur = Vector2.zero;
		private static Vector2 _cal_Phy_deltaVec_0 = Vector2.zero;
		private static Vector2 _cal_Phy_deltaVec_Cur = Vector2.zero;
		//private float tmpPhysics_TotalWeight = 0.0f;



		protected void CalculatePattern_Morph(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			//계산용 변수 초기화
			_cal_curCalParam = null;
			_cal_ResultPosList = null;
			//tmpPosList = null;//변경 21.5.16 : 이건 이제 초기화하지 않고, 한번 생성된 배열을 계속 유지한다.
			_cal_SubParamGroupList = null;
			_cal_SubParamKeyValueList = null;
			_cal_LayerWeight = 0.0f;
			_cal_KeyParamSetGroup = null;

			//[Pin] 추가 22.3.29 [v1.4.0]
			_cal_ResultPinPosList = null;
			_cal_TmpPinPosList = null;


			// 이값 사용 안함 19.5.20
			//apModifierParamSetGroupVertWeight weightedVertData = null;

			_cal_CurSubList = null;
			_cal_NumParamKeys = 0;
			_cal_ParamKeyValue = null;

			//색상을 지원하는 Modifier인가
			_cal_IsColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;

			//ParamSetWeight를 사용하는가
			_cal_IsUseParamSetWeight = IsUseParamSetWeight;

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크
				

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				_cal_curCalParam.Calculate();
				//-------------------------------------------------------

				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<

				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				//색상 처리 초기화
				_cal_curCalParam._isColorCalculated = false;


				_cal_ResultPosList = _cal_curCalParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;
				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_LayerWeight = 0.0f;
				_cal_KeyParamSetGroup = null;

				
				//일단 초기화
				//변경 21.5.15 : 배열 초기화 함수는 이걸로..
				_cal_NumVerts = _cal_ResultPosList.Length;
				if (_cal_NumVerts > 0)
				{
					System.Array.Clear(_cal_ResultPosList, 0, _cal_NumVerts);
				}


				//[Pin] 추가 22.3.29 [v1.4.0]
				_cal_ResultPinPosList = _cal_curCalParam._result_PinPositions;
				_cal_NumPins = _cal_ResultPinPosList != null ? _cal_ResultPinPosList.Length : 0;
				if (_cal_NumPins > 0)
				{
					System.Array.Clear(_cal_ResultPinPosList, 0, _cal_NumPins);
				}

				_cal_TargetPinGroup = null;
				if(_cal_curCalParam._targetRenderUnit._meshTransform != null
					&& _cal_curCalParam._targetRenderUnit._meshTransform._mesh != null)
				{
					_cal_TargetPinGroup = _cal_curCalParam._targetRenderUnit._meshTransform._mesh._pinGroup;
				}
				if(_cal_TargetPinGroup == null)
				{
					_cal_NumPins = 0;
				}



				if (_cal_IsColorProperty)
				{
					_cal_curCalParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					_cal_curCalParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					_cal_curCalParam._result_IsVisible = true;
				}

				//추가 11.29 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				_cal_curCalParam._isExtra_DepthChanged = false;
				_cal_curCalParam._isExtra_TextureChanged = false;
				_cal_curCalParam._extra_DeltaDepth = 0;
				_cal_curCalParam._extra_TextureDataID = -1;
				_cal_curCalParam._extra_TextureData = null;


				_cal_TmpColor = Color.clear;
				_cal_TmpVisible = false;

				//추가 20.2.22 : Show/Hide 토글을 할 수 있다.
				_cal_TmpIsToggleShowHideOption = false;
				
				_cal_TmpToggleOpt_IsAnyKey_Shown = false;
				_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
				_cal_TmpToggleOpt_IsAny_Hidden = false;
				//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Cal = 0.0f;


				//추가 11.29 : Extra Option 계산 값				
				_cal_TmpExtra_DepthChanged = false;
				_cal_TmpExtra_TextureChanged = false;
				_cal_TmpExtra_DeltaDepth = 0;
				_cal_TmpExtra_TextureDataID = 0;
				_cal_TmpExtra_TextureData = null;
				_cal_TmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				_cal_TmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				_cal_iCalculatedSubParam_Main = 0;

				_cal_iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				_cal_TmpIsColoredKeyParamSetGroup = false;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;



					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;

					if(IsAnimated && !_cal_KeyParamSetGroup.IsAnimEnabledInEditor)
					{	
						//선택되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
						//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
						continue;
					}


					
					//tmpPosList = keyParamSetGroup._tmpPositions;//삭제 21.5.16

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					_cal_isExCalculatable_Transform = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Transform;
					_cal_isExCalculatable_Color = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Color;
					
					_cal_isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					//변경 21.5.16 : tmpPosList를 이용하되, 최대값으로 만들자 (첫프레임에서는 계속 생성하게 될 거임)
					if (_cal_TmpPosList == null ||
						_cal_TmpPosList.Length < _cal_NumVerts)//부등호로 만들어서 항상 최대값을 유지한다.
					{
						_cal_TmpPosList = new Vector2[_cal_NumVerts];						
					}
					
					//변경 21.5.15 : 배열 초기화 함수는 이걸로..
					System.Array.Clear(_cal_TmpPosList, 0, _cal_NumVerts);


					//[Pin] 추가 22.3.29 (v1.4.0)
					//Pin 계산을 위한 배열 크기 체크
					if(_cal_NumPins > 0)
					{
						if(_cal_TmpPinPosList == null || _cal_TmpPinPosList.Length < _cal_NumPins)
						{
							_cal_TmpPinPosList = new Vector2[_cal_NumPins];
						}
						System.Array.Clear(_cal_TmpPinPosList, 0, _cal_NumPins);
					}




					_cal_TmpColor = Color.clear;
					_cal_TmpVisible = false;

					

					_cal_TotalParamSetWeight = 0.0f;
					_cal_NumCalculated = 0;

					

					//KeyParamSetGroup이 Color를 지원하는지 체크
					_cal_TmpIsColoredKeyParamSetGroup = _cal_IsColorProperty && _cal_KeyParamSetGroup._isColorPropertyEnabled && _cal_isExCalculatable_Color;

					//추가 20.2.22 : ShowHide 토글 변수 설정 및 관련 변수 초기화
					//오직 컨트롤 파라미터 타입이여야 하며, ParamSetGroup이 Color 옵션과 Toggle 옵션을 지원해야한다.
					_cal_TmpIsToggleShowHideOption = !IsAnimated && _cal_TmpIsColoredKeyParamSetGroup && _cal_KeyParamSetGroup._isToggleShowHideWithoutBlend;

					_cal_TmpToggleOpt_IsAnyKey_Shown = false;
					_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
					_cal_TmpToggleOpt_IsAny_Hidden = false;
					//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;




					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					
					for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
					{
						_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];

						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();

						if (!_cal_ParamKeyValue._isCalculated)
						{
							continue;
						}


						_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;



						//---------------------------- Pos List
						if (_cal_isExCalculatable_Transform)//<<추가
						{
							//[Pin] 추가 22.3.29 (v1.4.0)
							//핀이 없는 경우 : (Mod Delta Pos * PKV Weight)를 바로 Tmp에 더하기
							//핀이 있는 경우 : "Mod Delta Pos"에 Pin 정보를 가공한 후, PKV Weight를 곱하여 Tmp에 더하기

							if (_cal_NumPins > 0)
							{
								//Pin이 있는 경우
								//- Pin의 변경된 위치 계산 (Tmp 아니며 Mod Delta Pos를 PinGroup에 직접 적용)
								//- Pin의 커브 계산 후 Matrix 계산
								//- Pin-Vertex 가중치에 의해 "이동된 Vertex"에 적용.
								// (단 "버텍스의 이동 정보"가 단순 더하기 연산이 아닌 Rigging과 같은 행렬 연산으로 적용되어야 한다.)
								//- Pin의 Tmp 리스트에도 저장 (이건 미리보기 용이며 가중치 적용)
								apMeshPin curPin = null;
								Vector2 curPinDeltaPos = Vector2.zero;

								apModifiedVertex curModVert = null;
								apModifiedPin curModPin = null;
								apVertex curVert = null;
								Vector2 curVertPosModMid_WOPin = Vector2.zero;
								Vector2 curVertPosModMid_SumPinWeighted = Vector2.zero;
								Vector2 curPinWeightedPos = Vector2.zero;
								Vector2 curPinWeightedResultPos = Vector2.zero;
								apMeshPinVertWeight curPinVertWeight = null;
								apMeshPinVertWeight.VertPinPair curPinWeightPair = null;
								apMatrix3x3 curveVert2WorldMatrix = apMatrix3x3.identity;
								apMatrix3x3 curveCurveMatrix = apMatrix3x3.identity;


								// (1) Pin의 변경된 위치 계산 (Mod-Final로의 저장도 포함)
								for (int iPin = 0; iPin < _cal_NumPins; iPin++)
								{	
									curModPin = _cal_ParamKeyValue._modifiedMesh._pins[iPin];
									curPin = curModPin._pin;
									curPinDeltaPos = curModPin._deltaPos;

									//- 임시 변수(Mod-Mid)에 DeltaPos를 바로 적용
									//현재 PKV에 대해서만 Curve를 생성하기 위해서 ModMid를 생성
									curPin.SetTmpPos_ModMid(curPin._defaultPos + curPinDeltaPos);

									//- Mod-Final 계산을 위한 Tmp 리스트에 가중치 적용
									_cal_TmpPinPosList[iPin] += curPinDeltaPos * _cal_ParamKeyValue._weight;
								}

								// (2) 현재 상태에서 Pin의 Matrix 계산하고 Curve 연산
								_cal_TargetPinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.Update_ModMid);

								// 버텍스마다 돌면서 다음 단계 처리
								for (int iVert = 0; iVert < _cal_NumVerts; iVert++)
								{
									curModVert = _cal_ParamKeyValue._modifiedMesh._vertices[iVert];
									curVert = curModVert._vertex;

									// (3) 핀 그룹을 이용해서 가중치 정보 가져와서 Pin에 의한 위치 계산 (합)
									curPinVertWeight = _cal_TargetPinGroup._vertWeights[iVert];
									int nPairs = curPinVertWeight._nPairs;
									if(nPairs == 0)
									{
										//이 버텍스는 핀과 연결되지 않았다.
										//바로 Delta 값을 TmpList에 할당한다. (PKV 가중치 포함)
										_cal_TmpPosList[iVert] = curModVert._deltaPos * _cal_ParamKeyValue._weight;
										continue;
									}
									
									// 버텍스의 Pin 적용 전에 순수하게 Morph에 의해 변형된 Mod-Mid 위치 계산 (Default + DeltaPos)
									//curVertPosModMid_WOPin = curVert._pos + curModVert._deltaPos;//방법 1 : 먼저 이동한 후 커브 Matrix 적용 > 단점 : 역연산 불가로 기즈모 편집 불가
									curVertPosModMid_WOPin = curVert._pos;//방법 2 : 커브 Matrix 적용 후 Vert Delta를 나중에 합산
									
									curVertPosModMid_SumPinWeighted = Vector2.zero;//합산 초기화
									curPinWeightedPos = Vector2.zero;

									for (int iWeightPair = 0; iWeightPair < nPairs; iWeightPair++)
									{
										//핀-버텍스 가중치
										curPinWeightPair = curPinVertWeight._vertPinPairs[iWeightPair];

										//Pair 정보가 "단일 핀 연결"/"핀간의 커브와 연결"에 따라 계산이 다르다.
										if(!curPinWeightPair._isCurveWeighted)
										{
											//- 단일 핀과 연결된 경우
											curPinWeightedPos = curPinWeightPair._linkedPin.TmpMultiplyVertPos(apMeshPin.TMP_VAR_TYPE.ModMid, ref curVertPosModMid_WOPin);
										}
										else
										{
											//- 핀과 핀 사이의 커브와 연결된 경우
											//커브 행렬
											curveCurveMatrix = curPinWeightPair._linkedPin._nextCurve.GetCurveMatrix_Test(apMeshPin.TMP_VAR_TYPE.ModMid, curPinWeightPair._curveLerp);
											curveVert2WorldMatrix = curveCurveMatrix * curPinWeightPair._curveDefaultMatrix_Inv;
											curPinWeightedPos = curveVert2WorldMatrix.MultiplyPoint(curVertPosModMid_WOPin);
										}

										//가중치를 이용하여 Weight에 의한 위치 합
										curVertPosModMid_SumPinWeighted += curPinWeightedPos * curPinWeightPair._weight;
									}

									//Total Weight를 이용하여 최종 위치를 계산하자
									curPinWeightedResultPos = (curVertPosModMid_WOPin * (1.0f - curPinVertWeight._totalWeight)) + (curVertPosModMid_SumPinWeighted * curPinVertWeight._totalWeight);
									
									//기본 Pos를 빼서 Tmp에 넣는다. 완료!
									Vector2 deltaPos = curPinWeightedResultPos - curVert._pos;

									
									deltaPos += curModVert._deltaPos;//방법 2 한정 : Vertex의 Mod Delta를 최종 결과에 더한다. 역연산을 위함

									_cal_TmpPosList[iVert] += deltaPos * _cal_ParamKeyValue._weight;
								}
							}
							else
							{
								//Pin이 없는 경우
								//- 원래 방식대로 계산
								for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
								{
									_cal_TmpPosList[iPos] += _cal_ParamKeyValue._modifiedMesh._vertices[iPos]._deltaPos * _cal_ParamKeyValue._weight;
								}
							}
						}
						//---------------------------- Pos List


						if (_cal_isFirstParam)
						{
							_cal_isFirstParam = false;
						}

						if (_cal_TmpIsColoredKeyParamSetGroup)
						{
							if (!_cal_TmpIsToggleShowHideOption)
							{
								//기본 방식
								if (_cal_ParamKeyValue._modifiedMesh._isVisible)
								{
									_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
									_cal_TmpVisible = true;//하나라도 Visible이면 Visible이 된다.
								}
								else
								{
									//Visible이 False
									Color paramColor = _cal_ParamKeyValue._modifiedMesh._meshColor;
									paramColor.a = 0.0f;
									_cal_TmpColor += paramColor * _cal_ParamKeyValue._weight;
								}
							}
							else
							{
								//추가 20.2.22 : 토글 방식의 ShowHide 방식
								if (_cal_ParamKeyValue._modifiedMesh._isVisible && _cal_ParamKeyValue._weight > 0.0f)
								{
									//paramKeyValue._paramSet.ControlParamValue
									_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
									_cal_TmpVisible = true;//< 일단 이것도 true
									
									//토글용 처리
									_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

									//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
									if (!_cal_TmpToggleOpt_IsAnyKey_Shown)
									{
										_cal_TmpToggleOpt_KeyIndex_Shown = _cal_TmpToggleOpt_KeyIndex_Cal;
									}
									else
									{
										//Show Key Index 중 가장 작은 값을 기준으로 한다.
										_cal_TmpToggleOpt_KeyIndex_Shown = (_cal_TmpToggleOpt_KeyIndex_Cal < _cal_TmpToggleOpt_KeyIndex_Shown ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Shown);
									}

										
									_cal_TmpToggleOpt_IsAnyKey_Shown = true;

									_cal_TmpToggleOpt_TotalWeight_Shown += _cal_ParamKeyValue._weight;
									_cal_TmpToggleOpt_MaxWeight_Shown = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Shown ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Shown);
									
								}
								else
								{
									//토글용 처리
									_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

									if (!_cal_TmpToggleOpt_IsAny_Hidden)
									{
										_cal_TmpToggleOpt_KeyIndex_Hidden = _cal_TmpToggleOpt_KeyIndex_Cal;
									}
									else
									{
										//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
										_cal_TmpToggleOpt_KeyIndex_Hidden = (_cal_TmpToggleOpt_KeyIndex_Cal > _cal_TmpToggleOpt_KeyIndex_Hidden ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Hidden);
									}

									_cal_TmpToggleOpt_IsAny_Hidden = true;
									//_cal_TmpToggleOpt_TotalWeight_Hidden += _cal_ParamKeyValue._weight;
									_cal_TmpToggleOpt_MaxWeight_Hidden = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Hidden ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Hidden);
								}
							}
							
						}


						//---------------------------------------------
						//추가 11.29 : Extra Option
						if(_isExtraPropertyEnabled)
						{
							//1. Modifier의 Extra Property가 켜져 있어야 한다.
							//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
							//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
							//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
							if (_cal_ParamKeyValue._modifiedMesh._isExtraValueEnabled
								&& (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged || _cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged)
								)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = _cal_ParamKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (_cal_ParamKeyValue._animKeyPos)
									{
										case apCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
										case apCalculatedResultParam.AnimKeyPos.NextKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}

									
								}
								else
								{
									cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + 0.01f);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
									else { extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
									else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged && _cal_isExCalculatable_Transform)
									{
										//2-1. Depth 이벤트
										if(extraWeight > _cal_TmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											_cal_TmpExtra_DepthMaxWeight = extraWeight;
											_cal_TmpExtra_DepthChanged = true;
											_cal_TmpExtra_DeltaDepth = _cal_ParamKeyValue._modifiedMesh._extraValue._deltaDepth;
										}

									}
									if (_cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged && _cal_isExCalculatable_Color)
									{
										//2-2. Texture 이벤트
										if(extraWeight > _cal_TmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											_cal_TmpExtra_TextureMaxWeight = extraWeight;
											_cal_TmpExtra_TextureChanged = true;
											_cal_TmpExtra_TextureData = _cal_ParamKeyValue._modifiedMesh._extraValue._linkedTextureData;
											_cal_TmpExtra_TextureDataID = _cal_ParamKeyValue._modifiedMesh._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------------------------

						_cal_NumCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_cal_IsUseParamSetWeight)
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);
					}
					else
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight * Mathf.Clamp01(_cal_TotalParamSetWeight));
					}


					//>>> Linked Matrix < KeyParamSetGroup >
					//keyParamSetGroup.LinkedMatrix.SetPassAndMerge(apLinkedMatrix.VALUE_TYPE.VertPos).SetWeight(layerWeight);


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					// Transform과 Color를 나눔
					if(_cal_isExCalculatable_Transform)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;
					}
					if(_cal_isExCalculatable_Color)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Color += _cal_LayerWeight;
					}
					

					if (_cal_NumCalculated == 0)
					{
						_cal_TmpVisible = true;
					}


					//버그 수정 : 무조건 _cal_iCalculatedSubParam를 카운트하면, 실제로 연산되지 않았는데도 카운트가 증가할 수 있다.
					//변경 22.5.11 : 계산되는 SubParam을 무조건 +1 카운트하는게 아니라, 상황을 보면서 카운트를 한다. (연산이 되어야 카운트되도록)

					//Morph 연산을 레이어별로 결과에 적용
					if (_cal_isExCalculatable_Transform)
					{
						if(_cal_iCalculatedSubParam_Main == 0)
						{
							//첫번째 연산
							
							//속도 개선 22.5.11
							if (_cal_LayerWeight > 0.99f)
							{
								//레이어 가중치가 1일때 : 바로 복사
								Array.Copy(_cal_TmpPosList, _cal_ResultPosList, _cal_NumVerts);
							}
							else
							{
								//레이어 가중치가 1 미만일 때 : 가중치 곱하여 일일이 할당
								for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
								{
									_cal_ResultPosList[iPos] = _cal_TmpPosList[iPos] * _cal_LayerWeight;
								}
							}
							


							//추가 22.3.31 [v1.4.0] : Pin World 위치 계산
							if(_cal_NumPins > 0)
							{
								if (_cal_LayerWeight > 0.99f)
								{
									//레이어 가중치가 1일때 : 바로 복사
									Array.Copy(_cal_TmpPinPosList, _cal_ResultPinPosList, _cal_NumPins);
								}
								else
								{
									//레이어 가중치가 1 미만일 때 : 가중치 곱하여 일일이 할당
									for (int iPin = 0; iPin < _cal_NumPins; iPin++)
									{
										_cal_ResultPinPosList[iPin] = _cal_TmpPinPosList[iPin] * _cal_LayerWeight;
									}
								}
								
							}
						}
						else
						{
							//두번째 이후의 연산
							switch (_cal_KeyParamSetGroup._blendMethod)
							{
								case apModifierParamSetGroup.BLEND_METHOD.Additive:
									{
										for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
										{
											_cal_ResultPosList[iPos] += _cal_TmpPosList[iPos] * _cal_LayerWeight;
										}

										//추가 22.3.31 [v1.4.0] : Pin World 위치 계산
										if(_cal_NumPins > 0)
										{
											for (int iPin = 0; iPin < _cal_NumPins; iPin++)
											{	
												_cal_ResultPinPosList[iPin] += _cal_TmpPinPosList[iPin] * _cal_LayerWeight;
											}
										}
									}
									break;

								case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
									{
										for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
										{
											_cal_ResultPosList[iPos] = (_cal_ResultPosList[iPos] * (1.0f - _cal_LayerWeight)) +
																		(_cal_TmpPosList[iPos] * _cal_LayerWeight);
										}

										//추가 22.3.31 [v1.4.0] : Pin World 위치 계산
										if(_cal_NumPins > 0)
										{
											for (int iPin = 0; iPin < _cal_NumPins; iPin++)
											{	
												_cal_ResultPinPosList[iPin] = (_cal_ResultPinPosList[iPin] * (1.0f - _cal_LayerWeight)) +
																				(_cal_TmpPinPosList[iPin] * _cal_LayerWeight);
											}
										}
									}
									break;

								default:
									Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + _cal_KeyParamSetGroup._blendMethod);
									break;
							}
						}

						//연산 횟수 증가
						_cal_iCalculatedSubParam_Main += 1;			
						_cal_curCalParam._isMainCalculated = true;//메인 데이터가 계산되었다.
					}


					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (_cal_TmpIsColoredKeyParamSetGroup)
					{
						if (_cal_TmpIsToggleShowHideOption)
						{
							//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.

							if (_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (_cal_TmpToggleOpt_MaxWeight_Shown > _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									_cal_TmpVisible = true;
								}
								else if (_cal_TmpToggleOpt_MaxWeight_Shown < _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									_cal_TmpVisible = false;
									_cal_TmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (_cal_TmpToggleOpt_KeyIndex_Shown > _cal_TmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										_cal_TmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										_cal_TmpVisible = false;
										_cal_TmpColor = Color.clear;
									}
								}
							}
							else if (_cal_TmpToggleOpt_IsAnyKey_Shown && !_cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								_cal_TmpVisible = true;
							}
							else if (!_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (_cal_TmpVisible && _cal_TmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								_cal_TmpColor.r = Mathf.Clamp01(_cal_TmpColor.r / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.g = Mathf.Clamp01(_cal_TmpColor.g / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.b = Mathf.Clamp01(_cal_TmpColor.b / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.a = Mathf.Clamp01(_cal_TmpColor.a / _cal_TmpToggleOpt_TotalWeight_Shown);
							}
						}

						if (_cal_iColoredKeyParamSetGroup == 0 || _cal_KeyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							_cal_curCalParam._result_Color = apUtil.BlendColor_ITP(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						else
						{
							//색상 Additive
							_cal_curCalParam._result_Color = apUtil.BlendColor_Add(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						
						_cal_iColoredKeyParamSetGroup++;
						_cal_curCalParam._isColorCalculated = true;
					}


					//추가 11.29 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(_cal_TmpExtra_DepthChanged)
						{
							_cal_curCalParam._isExtra_DepthChanged = true;
							_cal_curCalParam._extra_DeltaDepth = _cal_TmpExtra_DeltaDepth;
						}

						if(_cal_TmpExtra_TextureChanged)
						{
							_cal_curCalParam._isExtra_TextureChanged = true;
							_cal_curCalParam._extra_TextureData = _cal_TmpExtra_TextureData;
							_cal_curCalParam._extra_TextureDataID = _cal_TmpExtra_TextureDataID;
						}
					}


				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)


				//삭제 22.5.11 [v1.4.0]
				////? 처리된게 하나도 없어요?
				//if (_cal_iCalculatedSubParam == 0)//이전
				//{
				//	//Active를 False로 날린다.
				//	_cal_curCalParam._isAvailable = false;
				//}
				//else
				//{
				//	_cal_curCalParam._isAvailable = true;
				//}


				
			}
		}




		protected void CalculatePattern_Transform(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			_cal_isBoneTarget = false;//Bone을 대상으로 하는가 (Bone 대상이면 ModBone을 사용해야한다)
			_cal_curCalParam = null;

			//색상을 지원하는 Modifier인가
			_cal_IsColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;
			

			//ParamSetWeight를 사용하는가
			_cal_IsUseParamSetWeight = IsUseParamSetWeight;


			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];
				if (_cal_curCalParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					_cal_isBoneTarget = true;
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					_cal_isBoneTarget = false;
				}

				//Sub List를 돌면서 Weight 체크

				//----------------------------------------------
				//1. 계산!
				_cal_curCalParam.Calculate();
				//----------------------------------------------



				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();


				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_KeyParamSetGroup = null;


				//결과 매트릭스를 만들자
				_cal_curCalParam._result_Matrix.SetIdentity();


				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				//색상 처리 초기화
				_cal_curCalParam._isColorCalculated = false;

				if (!_cal_isBoneTarget)
				{
					if (_cal_IsColorProperty)
					{
						_cal_curCalParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						_cal_curCalParam._result_IsVisible = false;
					}
					else
					{
						_cal_curCalParam._result_IsVisible = true;
					}
				}
				else
				{
					_cal_curCalParam._result_IsVisible = true;
				}

				//추가 11.29 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				_cal_curCalParam._isExtra_DepthChanged = false;
				_cal_curCalParam._isExtra_TextureChanged = false;
				_cal_curCalParam._extra_DeltaDepth = 0;
				_cal_curCalParam._extra_TextureDataID = -1;
				_cal_curCalParam._extra_TextureData = null;

				//추가 : Bone 타겟이면 BoneIKWeight를 계산해야한다.
				//calParam._result_BoneIKWeight = 0.0f;
				//calParam._isBoneIKWeightCalculated = false;

				//변경 3.26 : 계산용 행렬 (apMatrixCal)을 사용하자
				//apMatrix tmpMatrix = null;
				if(_cal_TmpMatrix == null)
				{
					_cal_TmpMatrix = new apMatrixCal();
				}


				_cal_TmpColor = Color.clear;
				_cal_TmpVisible = false;

				//추가 20.2.22 : Show/Hide 토글을 할 수 있다.
				_cal_TmpIsToggleShowHideOption = false;
				
				_cal_TmpToggleOpt_IsAnyKey_Shown = false;
				_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
				_cal_TmpToggleOpt_IsAny_Hidden = false;
				//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Cal = 0.0f;

				

				//추가 11.29 : Extra Option 계산 값				
				_cal_TmpExtra_DepthChanged = false;
				_cal_TmpExtra_TextureChanged = false;
				_cal_TmpExtra_DeltaDepth = 0;
				_cal_TmpExtra_TextureDataID = 0;
				_cal_TmpExtra_TextureData = null;
				_cal_TmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				_cal_TmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값

				_cal_LayerWeight = 0.0f;

				_cal_iCalculatedSubParam_Main = 0;

				
			


				_cal_iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				_cal_TmpIsColoredKeyParamSetGroup = false;

				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						continue;
					}


					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;

					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;

					//추가 20.4.2 : 애니메이션 모디파이어일때.
					if(IsAnimated && !_cal_KeyParamSetGroup.IsAnimEnabledInEditor)
					{	
						//선택되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
						//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
						continue;
					}

					//tmpMatrix = keyParamSetGroup._tmpMatrix;//삭제 21.5.16

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					_cal_isExCalculatable_Transform = _cal_KeyParamSetGroup.IsExCalculatable_Transform;
					_cal_isExCalculatable_Color = _cal_KeyParamSetGroup.IsExCalculatable_Color;


					//추가 21.9.1 : <회전 보정>
					_cal_isRotation180Correction = !IsAnimated && _cal_KeyParamSetGroup._tfRotationLerpMethod == apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector;
					_cal_Rotation180Correction_DeltaAngle = 0.0f;
					

					_cal_isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					_cal_TmpMatrix.SetZero();
					_cal_TmpColor = Color.clear;
					_cal_TmpVisible = false;

					_cal_LayerWeight = 0.0f;

					_cal_TotalParamSetWeight = 0.0f;
					_cal_NumCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					_cal_TmpIsColoredKeyParamSetGroup = _cal_IsColorProperty && _cal_KeyParamSetGroup._isColorPropertyEnabled && !_cal_isBoneTarget && _cal_isExCalculatable_Color;

					//추가 20.2.22 : ShowHide 토글 변수 설정 및 관련 변수 초기화
					//오직 컨트롤 파라미터 타입이여야 하며, ParamSetGroup이 Color 옵션과 Toggle 옵션을 지원해야한다.
					_cal_TmpIsToggleShowHideOption = !IsAnimated && _cal_TmpIsColoredKeyParamSetGroup && _cal_KeyParamSetGroup._isToggleShowHideWithoutBlend;

					_cal_TmpToggleOpt_IsAnyKey_Shown = false;
					_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
					_cal_TmpToggleOpt_IsAny_Hidden = false;
					//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;


					if (!_cal_isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우

						//추가 20.9.10 : 정밀한 보간을 위해 Default Matrix가 필요하다.
						apMatrix defaultMatrixOfRenderUnit = null;
						//bool isDebug = false;
						if(_cal_curCalParam._targetRenderUnit != null)
						{
							if(_cal_curCalParam._targetRenderUnit._meshTransform != null)
							{
								defaultMatrixOfRenderUnit = _cal_curCalParam._targetRenderUnit._meshTransform._matrix_TF_ToParent;

								//if(calParam._targetRenderUnit._meshTransform._nickName.Contains("Debug"))
								//{
								//	isDebug = true;
								//}
							}
							else if(_cal_curCalParam._targetRenderUnit._meshGroupTransform != null)
							{
								defaultMatrixOfRenderUnit = _cal_curCalParam._targetRenderUnit._meshGroupTransform._matrix_TF_ToParent;
							}
						}


						
						for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
						{
							_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);

							
							if (!_cal_ParamKeyValue._isCalculated)
							{ continue; }

							//ParamSetWeight를 추가
							_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;


							if (_cal_isExCalculatable_Transform)//<<추가
							{
								//Weight에 맞게 Matrix를 만들자

								if (_cal_ParamKeyValue._isAnimRotationBias)
								{
									//RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
									_cal_TmpMatrix.AddMatrixParallel_ModMesh(_cal_ParamKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, _cal_ParamKeyValue._weight);
								}
								else
								{
									//apMatrixCal을 사용한다.
									_cal_TmpMatrix.AddMatrixParallel_ModMesh(	_cal_ParamKeyValue._modifiedMesh._transformMatrix, 
																				defaultMatrixOfRenderUnit, 
																				_cal_ParamKeyValue._weight);
								}
							}


							
							//Modifier + KeyParamSetGroup 모두 Color를 지원해야함
							if (_cal_TmpIsColoredKeyParamSetGroup)
							{
								if (!_cal_TmpIsToggleShowHideOption)
								{
									//기본 방식
									if (_cal_ParamKeyValue._modifiedMesh._isVisible)
									{
										_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
										_cal_TmpVisible = true;
									}
									else
									{
										//Visible이 False
										Color paramColor = _cal_ParamKeyValue._modifiedMesh._meshColor;
										paramColor.a = 0.0f;
										_cal_TmpColor += paramColor * _cal_ParamKeyValue._weight;
									}
								}
								else
								{
									//추가 20.2.22 : 토글 방식의 ShowHide 방식
									if (_cal_ParamKeyValue._modifiedMesh._isVisible && _cal_ParamKeyValue._weight > 0.0f)
									{
										//paramKeyValue._paramSet.ControlParamValue
										_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
										_cal_TmpVisible = true;//< 일단 이것도 true

										//토글용 처리
										_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

										//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
										if (!_cal_TmpToggleOpt_IsAnyKey_Shown)
										{
											_cal_TmpToggleOpt_KeyIndex_Shown = _cal_TmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Show Key Index 중 가장 작은 값을 기준으로 한다.
											_cal_TmpToggleOpt_KeyIndex_Shown = (_cal_TmpToggleOpt_KeyIndex_Cal < _cal_TmpToggleOpt_KeyIndex_Shown ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Shown);
										}

										
										_cal_TmpToggleOpt_IsAnyKey_Shown = true;

										_cal_TmpToggleOpt_TotalWeight_Shown += _cal_ParamKeyValue._weight;
										_cal_TmpToggleOpt_MaxWeight_Shown = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Shown ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Shown);

									}
									else
									{
										//토글용 처리
										_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

										if (!_cal_TmpToggleOpt_IsAny_Hidden)
										{
											_cal_TmpToggleOpt_KeyIndex_Hidden = _cal_TmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
											_cal_TmpToggleOpt_KeyIndex_Hidden = (_cal_TmpToggleOpt_KeyIndex_Cal > _cal_TmpToggleOpt_KeyIndex_Hidden ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Hidden);
										}

										_cal_TmpToggleOpt_IsAny_Hidden = true;
										//_cal_TmpToggleOpt_TotalWeight_Hidden += _cal_ParamKeyValue._weight;
										_cal_TmpToggleOpt_MaxWeight_Hidden = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Hidden ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Hidden);
									}
								}
								
							}

							//---------------------------------------------
							//추가 11.29 : Extra Option
							if(_isExtraPropertyEnabled)
							{
								//1. Modifier의 Extra Property가 켜져 있어야 한다.
								//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
								//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
								//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
								if (_cal_ParamKeyValue._modifiedMesh._isExtraValueEnabled
									&& (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged || _cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged)
									)
								{
									//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
									float extraWeight = _cal_ParamKeyValue._weight;//<<일단 가중치를 더한다.
									float bias = 0.0001f;
									float cutOut = 0.0f;
									bool isExactWeight = false;
									if (IsAnimated)
									{
										switch (_cal_ParamKeyValue._animKeyPos)
										{
											case apCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
											case apCalculatedResultParam.AnimKeyPos.NextKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
											case apCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
										}
									}
									else
									{
										cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout;
									}

									cutOut = Mathf.Clamp01(cutOut + 0.01f);//살짝 겹치게

									if (isExactWeight)
									{
										extraWeight = 10000.0f;
									}
									else if (cutOut < bias)
									{
										//정확하면 최대값
										//아니면 적용안함
										if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
										else { extraWeight = -1.0f; }
									}
									else
									{
										if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
										else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
									}

									if (extraWeight > 0.0f)
									{
										if (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged && _cal_isExCalculatable_Transform)
										{
											//2-1. Depth 이벤트
											if(extraWeight > _cal_TmpExtra_DepthMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												//Debug.Log("Depth Changed [" + DisplayName + "] : " + paramKeyValue._modifiedMesh._renderUnit.Name 
												//	+ " / ExtraWeight : " 
												//	+ extraWeight + " / CurMaxWeight : " + tmpExtra_DepthMaxWeight);

												_cal_TmpExtra_DepthMaxWeight = extraWeight;
												_cal_TmpExtra_DepthChanged = true;
												_cal_TmpExtra_DeltaDepth = _cal_ParamKeyValue._modifiedMesh._extraValue._deltaDepth;
											}

										}
										if (_cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged && _cal_isExCalculatable_Color)
										{
											//2-2. Texture 이벤트
											if(extraWeight > _cal_TmpExtra_TextureMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												_cal_TmpExtra_TextureMaxWeight = extraWeight;
												_cal_TmpExtra_TextureChanged = true;
												_cal_TmpExtra_TextureData = _cal_ParamKeyValue._modifiedMesh._extraValue._linkedTextureData;
												_cal_TmpExtra_TextureDataID = _cal_ParamKeyValue._modifiedMesh._extraValue._textureDataID;
											}
										}
									}
								}
							}
							//---------------------------------------------




							if (_cal_isFirstParam)
							{
								_cal_isFirstParam = false;
							}
							_cal_NumCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}


						//추가 21.9.1
						//180도 각도 보정을 위해서는 전체 파라미터를 따로 돌아서 벡터 합에 의한 회전각을 따로 계산해야한다.
						if(_cal_isRotation180Correction && _cal_isExCalculatable_Transform)
						{
							_cal_Rotation180Correction_DeltaAngle = 0.0f;
							_cal_Rotation180Correction_SumVector = Vector2.zero;
							for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
							{
								_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
								if(!_cal_ParamKeyValue._isCalculated)
								{
									continue;
								}
								float curAngle = _cal_ParamKeyValue._modifiedMesh._transformMatrix._angleDeg * Mathf.Deg2Rad;
								_cal_Rotation180Correction_CurVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
								_cal_Rotation180Correction_SumVector += (_cal_Rotation180Correction_CurVector * _cal_ParamKeyValue._weight) * 10.0f;//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							}

							//벡터합을 역산해서 현재 상태의 평균 합을 구하자
							//Weight 합이 0이거나, 서로 반대방향을 바라보는 경우가 아니라면..
							if(_cal_Rotation180Correction_SumVector.sqrMagnitude > 0.001f)
							{
								_cal_Rotation180Correction_SumVector *= 10.0f;
								_cal_Rotation180Correction_DeltaAngle = Mathf.Atan2(_cal_Rotation180Correction_SumVector.y, _cal_Rotation180Correction_SumVector.x) * Mathf.Rad2Deg;
								//Debug.Log("보정 각도 : " + _cal_Rotation180Correction_DeltaAngle + " / 벡터 : " + sumVec);
							}

							_cal_TmpMatrix._angleDeg = _cal_Rotation180Correction_DeltaAngle;
						}



						_cal_TmpMatrix.CalculateScale_FromAdd();
						_cal_TmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit/*, isDebug*/);//추가 (20.9.10) : 위치 보간이슈 수정
						
					}
					else
					{
						
						//ModBone을 활용하는 타입인 경우
						for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
						{
							//paramKeyValue = calParam._paramKeyValues[iPV];
							_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);

							if (!_cal_ParamKeyValue._isCalculated)
							{
								continue;
							}

							//ParamSetWeight를 추가
							_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;


							//Weight에 맞게 Matrix를 만들자
							if (_cal_isExCalculatable_Transform)
							{
								if (_cal_ParamKeyValue._isAnimRotationBias)
								{
									//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
									//이전 : apMatrix
									//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

									//변경 : apMatrixCal 이용
									_cal_TmpMatrix.AddMatrixParallel_ModBone(_cal_ParamKeyValue.AnimRotationBiasedMatrix, _cal_ParamKeyValue._weight);
								}
								else
								{
									//이전 : apMatrix
									//tmpMatrix.AddMatrix(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight, false);

									//변경 : apMatrixCal 이용
									_cal_TmpMatrix.AddMatrixParallel_ModBone(_cal_ParamKeyValue._modifiedBone._transformMatrix, _cal_ParamKeyValue._weight);
								}

								//if (isBoneIKControllerUsed)
								//{
								//	//추가 : Bone IK Weight 계산
								//	tmpBoneIKWeight += paramKeyValue._weight * paramKeyValue._modifiedBone._boneIKController_MixWeight;
								//}
							}

							//TODO : ModBone도 CalculateLog를 기록해야하나..


							if (_cal_isFirstParam)
							{
								_cal_isFirstParam = false;
							}
							_cal_NumCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}


						//추가 21.9.1
						//180도 각도 보정을 위해서는 전체 파라미터를 따로 돌아서 벡터 합에 의한 회전각을 따로 계산해야한다.
						if(_cal_isRotation180Correction && _cal_isExCalculatable_Transform)
						{
							_cal_Rotation180Correction_DeltaAngle = 0.0f;
							_cal_Rotation180Correction_SumVector = Vector2.zero;
							for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
							{
								_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
								if(!_cal_ParamKeyValue._isCalculated)
								{
									continue;
								}
								float curAngle = _cal_ParamKeyValue._modifiedBone._transformMatrix._angleDeg * Mathf.Deg2Rad;
								_cal_Rotation180Correction_CurVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
								_cal_Rotation180Correction_SumVector += (_cal_Rotation180Correction_CurVector * _cal_ParamKeyValue._weight) * 10.0f;//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							}

							//벡터합을 역산해서 현재 상태의 평균 합을 구하자
							//Weight 합이 0이거나, 서로 반대방향을 바라보는 경우가 아니라면..
							if(_cal_Rotation180Correction_SumVector.sqrMagnitude > 0.001f)
							{
								_cal_Rotation180Correction_SumVector *= 10.0f;
								_cal_Rotation180Correction_DeltaAngle = Mathf.Atan2(_cal_Rotation180Correction_SumVector.y, _cal_Rotation180Correction_SumVector.x) * Mathf.Rad2Deg;
								//Debug.Log("보정 각도 : " + _cal_Rotation180Correction_DeltaAngle + " / 벡터 : " + sumVec);
							}

							_cal_TmpMatrix._angleDeg = _cal_Rotation180Correction_DeltaAngle;
						}



						//위치 변경 20.9.10
						_cal_TmpMatrix.CalculateScale_FromAdd();
					}

					//이제 레이어순서에 따른 보간을 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_cal_IsUseParamSetWeight)
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);
					}
					else
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight * Mathf.Clamp01(_cal_TotalParamSetWeight));
					}


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					// Transform과 Color를 나눔
					if(_cal_isExCalculatable_Transform)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;
					}
					if(_cal_isExCalculatable_Color)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Color += _cal_LayerWeight;
					}



					if ((_cal_NumCalculated == 0 && _cal_IsColorProperty) || _cal_isBoneTarget)
					{
						_cal_TmpVisible = true;
					}

					//추가 3.26 : apMatrixCal 계산 > 이건 ModMesh, ModBone에 따라 달라서 위에서 호출하자. (20.9.10)
					//tmpMatrix.CalculateScale_FromAdd();

					//22.5.11 [v1.4.0] 레이어 연산 순서 변경
					if (_cal_isExCalculatable_Transform)
					{
						if (_cal_iCalculatedSubParam_Main == 0)
						{
							//변경 3.26 : apMatrixCal로 계산된 tmpMatrix
							_cal_curCalParam._result_Matrix.SetTRSForLerp(_cal_TmpMatrix);
						}
						else
						{
							switch (_cal_KeyParamSetGroup._blendMethod)
							{
								case apModifierParamSetGroup.BLEND_METHOD.Additive:
									{
										//변경 3.26 : apMatrixCal로 계산된 AddMatrix
										_cal_curCalParam._result_Matrix.AddMatrixLayered(_cal_TmpMatrix, _cal_LayerWeight);
									}
									break;

								case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
									{
										//변경 3.26 : apMatrixCal로 계산 된 AddMatrix
										_cal_curCalParam._result_Matrix.LerpMatrixLayered(_cal_TmpMatrix, _cal_LayerWeight);
									}
									break;

								default:
									Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + _cal_KeyParamSetGroup._blendMethod);
									break;
							}
						}
						//메인 데이터 연산 결과 추가
						_cal_iCalculatedSubParam_Main += 1;
						_cal_curCalParam._isMainCalculated = true;
					}


					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (_cal_TmpIsColoredKeyParamSetGroup)
					{
						if (_cal_TmpIsToggleShowHideOption)
						{
							//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.

							if (_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (_cal_TmpToggleOpt_MaxWeight_Shown > _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									_cal_TmpVisible = true;
								}
								else if (_cal_TmpToggleOpt_MaxWeight_Shown < _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									_cal_TmpVisible = false;
									_cal_TmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (_cal_TmpToggleOpt_KeyIndex_Shown > _cal_TmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										_cal_TmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										_cal_TmpVisible = false;
										_cal_TmpColor = Color.clear;
									}
								}
							}
							else if (_cal_TmpToggleOpt_IsAnyKey_Shown && !_cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								_cal_TmpVisible = true;
							}
							else if (!_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (_cal_TmpVisible && _cal_TmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								_cal_TmpColor.r = Mathf.Clamp01(_cal_TmpColor.r / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.g = Mathf.Clamp01(_cal_TmpColor.g / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.b = Mathf.Clamp01(_cal_TmpColor.b / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.a = Mathf.Clamp01(_cal_TmpColor.a / _cal_TmpToggleOpt_TotalWeight_Shown);
							}
						}

						if (_cal_iColoredKeyParamSetGroup == 0 || _cal_KeyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							_cal_curCalParam._result_Color = apUtil.BlendColor_ITP(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						else
						{
							//색상 Additive
							_cal_curCalParam._result_Color = apUtil.BlendColor_Add(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						_cal_iColoredKeyParamSetGroup++;
						_cal_curCalParam._isColorCalculated = true;
					}

					//추가 11.29 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(_cal_TmpExtra_DepthChanged)
						{
							_cal_curCalParam._isExtra_DepthChanged = true;
							_cal_curCalParam._extra_DeltaDepth = _cal_TmpExtra_DeltaDepth;
						}

						if(_cal_TmpExtra_TextureChanged)
						{
							_cal_curCalParam._isExtra_TextureChanged = true;
							_cal_curCalParam._extra_TextureData = _cal_TmpExtra_TextureData;
							_cal_curCalParam._extra_TextureDataID = _cal_TmpExtra_TextureDataID;
						}
					}
				}

				//이전
				//? 처리된게 하나도 없어요?
				//if (_cal_iCalculatedSubParam == 0)
				//{
				//	//Active를 False로 날린다.
				//	_cal_curCalParam._isAvailable = false;
				//}
				//else
				//{
				//	_cal_curCalParam._isAvailable = true;

				//	//이전 : apMatrix로 계산된 경우
				//	//calParam._result_Matrix.MakeMatrix();

				//	//변경 : apMatrixCal로 계산한 경우
				//	_cal_curCalParam._result_Matrix.CalculateScale_FromLerp();
				//}

				//변경 22.5.11 : 메인 데이터가 연산되었는지로 판별
				if(_cal_curCalParam._isMainCalculated)
				{
					_cal_curCalParam._result_Matrix.CalculateScale_FromLerp();
				}
			}
		}

		protected void CalculatePattern_Rigging(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				//Debug.LogError("Result Param Count : 0");
				return;
			}

			//Debug.Log("Rigging - " + _meshGroup._name);
			//Profiler.BeginSample("Rigging Calculate");

			_cal_curCalParam = null;
			_cal_ResultPosList = null;
			//tmpPosList = null;//변경 21.5.16 : 이건 이제 초기화하지 않고, 한번 생성된 것을 계속 유지한다.

			//Pos대신 Matrix
			_cal_ResultVertMatrixList = null;

			//tmpVertMatrixList = null;//21.5.16 이 둘은 초기화하지 않는다.
			//tmpVertWeightList = null;

			_cal_SubParamGroupList = null;
			_cal_SubParamKeyValueList = null;
			_cal_LayerWeight = 0.0f;
			_cal_KeyParamSetGroup = null;
			_cal_CurSubList = null;
			_cal_NumParamKeys = 0;
			_cal_ParamKeyValue = null;


			_cal_isRiggingWithIK = _meshGroup.IsRiggingWithIK;
			

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				//Profiler.BeginSample("1. Basic Calculate");

				_cal_curCalParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요! -> Static은 Weight 계산이 필요없어염
				//-------------------------------------------------------
				//1. Param Weight Calculate
				_cal_curCalParam.Calculate();
				//-------------------------------------------------------

				//Profiler.EndSample();

				//Profiler.BeginSample("2. Record Log");

				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<

				//Profiler.EndSample();


				//Profiler.BeginSample("3. Init");

				_cal_ResultPosList = _cal_curCalParam._result_Positions;
				_cal_ResultVertMatrixList = _cal_curCalParam._result_VertMatrices;
			
				//tmpPosList = calParam._tmp_Positions;
				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_LayerWeight = 0.0f;

				//일단 초기화
				//이전
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//변경 21.5.15 : 배열 초기화 함수는 이걸로..
				_cal_NumVerts = _cal_ResultPosList.Length;				
				System.Array.Clear(_cal_ResultPosList, 0, _cal_NumVerts);


				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;


				_cal_curCalParam._result_IsVisible = true;


				_cal_TmpColor = Color.clear;
				_cal_iCalculatedSubParam_Main = 0;

				//Profiler.EndSample();

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{

					//Profiler.BeginSample("4. ParamSetGroup Calculate");

					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;


					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;//<<


					//변경 21.5.16 : 최대값으로 한번 생성하고 재사용하도록 변경
					if (_cal_TmpPosList == null ||
						_cal_TmpVertMatrixList == null ||
						_cal_TmpVertWeightList == null ||
						_cal_TmpPosList.Length < _cal_NumVerts || 
						_cal_TmpVertMatrixList.Length < _cal_NumVerts||
						_cal_TmpVertWeightList.Length < _cal_NumVerts)
					{
						_cal_TmpPosList = new Vector2[_cal_NumVerts];
						_cal_TmpVertMatrixList = new apMatrix3x3[_cal_NumVerts];
						_cal_TmpVertWeightList = new float[_cal_NumVerts];
					}

					//변경 21.5.15 : 배열 초기화 함수는 이걸로.. (행렬은 3x2 초기화라 어쩔 수 없다.)
					System.Array.Clear(_cal_TmpPosList, 0, _cal_NumVerts);
					System.Array.Clear(_cal_TmpVertWeightList, 0, _cal_NumVerts);

					for (int iMatrix = 0; iMatrix < _cal_NumVerts; iMatrix++)
					{
						_cal_TmpVertMatrixList[iMatrix].SetZero3x2();
					}


					_cal_TmpColor = Color.clear;
					//tmpVisible = false;

					float totalWeight = 0.0f;
					_cal_NumCalculated = 0;


					//Profiler.BeginSample("4-2. ParamKey Calculate");

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
					{
						_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];

						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();

						_cal_ParamKeyValue._weight = 1.0f;

						totalWeight += _cal_ParamKeyValue._weight;

						//Modified가 안된 Vert World Pos + Bone의 Modified 안된 World Matrix + Bone의 World Matrix (변형됨) 순으로 계산한다.
						_cal_Rig_matx_Vert2Local = _cal_ParamKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal;
						
						//역행렬 생성 이슈
						//이전 : 이것도 매번 생성하는건 좋지 않다.
						//apMatrix3x3 matx_Vert2Local_Inv = matx_Vert2Local.inverse;
						
						//변경 21.5.16
						_cal_Rig_matx_Vert2Local_Inv = _cal_ParamKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal_Inverse;

						_cal_tmpRig_matx_MeshW_NoMod = _cal_ParamKeyValue._modifiedMesh._renderUnit._meshTransform._matrix_TFResult_WorldWithoutMod;
						


						//Profiler.BeginSample("4-2-1. Vert Pos Calculate");

						//---------------------------- Pos List

						for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
						{
							//1. Mod가 적용안된 Vert의 World Pos
							_cal_Rig_curVertRig = _cal_ParamKeyValue._modifiedMesh._vertRigs[iPos];
							_cal_Rig_VertPosW_NoMod = _cal_tmpRig_matx_MeshW_NoMod.MulPoint2(_cal_Rig_matx_Vert2Local.MultiplyPoint(_cal_Rig_curVertRig._vertex._pos));


							//2. Bone의 (Mod가 적용 안된) World Matrix의 역행렬을 계산하여 Local Vert by Bone을 만든다.
							//3. Bone의 World Matrix를 계산하여 연산한다.
							_cal_Rig_TotalBoneWeight = 0.0f;
							_cal_Rig_WeightPair = null;
							
							//기존 방식 [Skew 이슈]
							//apMatrix matx_boneWorld_Mod = null;
							//apMatrix matx_boneWorld_Default = null;

							//변경 20.8.12 : apComplexMatrix > 20.8.17 : 래핑
							_cal_Rig_Matx_boneWorld_Mod = null;
							_cal_Rig_Matx_boneWorld_Default = null;

							//수정 : Rigging을 vertPos가 아닌 Matrix의 합으로 계산한다.
							_cal_Rig_Matx_Result = apMatrix3x3.identity;
							 
							for (int iWeight = 0; iWeight < _cal_Rig_curVertRig._weightPairs.Count; iWeight++)
							{
								_cal_Rig_WeightPair = _cal_Rig_curVertRig._weightPairs[iWeight];

								if (_cal_Rig_WeightPair._weight <= 0.0001f)
								{
									continue;
								}

								//Profiler.BeginSample("4-2-1-1. Matrix Calculate");

								if(_cal_isRiggingWithIK)
								{
									_cal_Rig_Matx_boneWorld_Mod = _cal_Rig_WeightPair._bone._worldMatrix_IK;//<<추가 : IK가 포함된 Rigging으로 계산한다.
								}
								else
								{
									_cal_Rig_Matx_boneWorld_Mod = _cal_Rig_WeightPair._bone._worldMatrix;
								}
								

								_cal_Rig_Matx_boneWorld_Default = _cal_Rig_WeightPair._bone._worldMatrix_NonModified;

								//World -> Bone Local
								_cal_Rig_VertPos_BoneLocal = _cal_Rig_Matx_boneWorld_Default.InvMulPoint2(_cal_Rig_VertPosW_NoMod);

								//Bone Local -> World
								_cal_Rig_VertPosW_BoneWorld = _cal_Rig_Matx_boneWorld_Mod.MulPoint2(_cal_Rig_VertPos_BoneLocal);

								//vertPos_OnlyReverseMesh = matx_Vert2Local_Inv.MultiplyPoint(matx_MeshW_NoMod.InvMulPoint2(vertPosW_NoMod));

								//다시 이것의 Local Pos를 구한다.
								_cal_Rig_VertPosL_Result = _cal_Rig_matx_Vert2Local_Inv.MultiplyPoint(_cal_tmpRig_matx_MeshW_NoMod.InvMulPoint2(_cal_Rig_VertPosW_BoneWorld));

								
								//TODO : 이거 Vert가 아닌 Mesh 단계에서 미리 만들 수 없나 (Lookup 방식)
								//여기서 성능 많이 향상될 듯
								//Mesh와 Bone 조합별로 미리 만들면 Vert에서 가져다 쓰면 되지

								//<Vert2Local> 단계를 제외한 Bone matrix 계산식
								_cal_Rig_Matx_Result = _cal_tmpRig_matx_MeshW_NoMod.MtrxToLowerSpace
									* _cal_Rig_Matx_boneWorld_Mod.MtrxToSpace
									* _cal_Rig_Matx_boneWorld_Default.MtrxToLowerSpace
									* _cal_tmpRig_matx_MeshW_NoMod.MtrxToSpace;


								//Vert에 저장하는 방식
								_cal_TmpPosList[iPos] += new Vector2(_cal_Rig_VertPosL_Result.x, _cal_Rig_VertPosL_Result.y) * _cal_Rig_WeightPair._weight;
								
								//Matrix에 저장하는 방식
								_cal_TmpVertMatrixList[iPos] += _cal_Rig_Matx_Result * _cal_Rig_WeightPair._weight;
								
								//Profiler.EndSample();

								_cal_Rig_TotalBoneWeight += _cal_Rig_WeightPair._weight;
							}

							
							_cal_TmpVertWeightList[iPos] = Mathf.Clamp01(_cal_Rig_TotalBoneWeight);

							if (_cal_Rig_TotalBoneWeight > 0.0f)
							{
								_cal_TmpPosList[iPos] = new Vector2(_cal_TmpPosList[iPos].x / _cal_Rig_TotalBoneWeight, _cal_TmpPosList[iPos].y / _cal_Rig_TotalBoneWeight);
								_cal_TmpVertMatrixList[iPos] /= _cal_Rig_TotalBoneWeight;
							}
							else
							{
								//Bone Weight가 지정되지 않았을 때
								_cal_TmpPosList[iPos] = _cal_Rig_curVertRig._vertex._pos;
								_cal_TmpVertMatrixList[iPos].SetIdentity();
							}
						}
						//---------------------------- Pos List

						//Profiler.EndSample();

						
						_cal_NumCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params


					//Profiler.EndSample();

					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					_cal_LayerWeight = 1.0f;

					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;

					

					//추가 22.5.12 : 배열 복사로 적용
					Array.Copy(_cal_TmpPosList, _cal_ResultPosList, _cal_NumVerts);

					for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
					{
						//사용하지 않음 22.5.12
						//_cal_ResultPosList[iPos] = _cal_TmpPosList[iPos] * _cal_LayerWeight;
						
						//Bone Weight가 1 미만인 경우도 적용하기 위해 Normalize 이전의 Weight를 곱한다.
						_cal_ResultVertMatrixList[iPos].SetMatrixWithWeight(ref _cal_TmpVertMatrixList[iPos], _cal_LayerWeight * _cal_TmpVertWeightList[iPos]);


					}

					_cal_iCalculatedSubParam_Main += 1;
					_cal_curCalParam._isMainCalculated = true;//추가 22.5.11 : 항상 True
					

					//Profiler.EndSample();

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				
				//삭제 22.5.11
				//_cal_curCalParam._isAvailable = true;


			}

			//Profiler.EndSample();
		}



		

		protected void CalculatePattern_Physics(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			bool isValidFrame = false;//유효한 프레임[물리 처리를 한다], 유효하지 않은 
			
			//삭제 20.7.9 : 타이머는 Portrait에서 공통으로 계산한다.
			//if (_stopwatch == null)
			//{
			//	_stopwatch = new System.Diagnostics.Stopwatch();
			//	_stopwatch.Start();
			//	_tDeltaFixed = 0.0f;
			//}

			//이전
			////tDelta를 별도로 받자
			//tDelta = (float)(_stopwatch.ElapsedMilliseconds / 1000.0f);

			//변경 20.7.9 : 물리 DeltaTime이 Portrait에 있다.
			tDelta = _portrait.PhysicsDeltaTime;

			_tDeltaFixed += tDelta;
			//tmpPhysics_tUpdateCall += tDelta;
			//tmpPhysics_nUpdateCall++;


			if (_tDeltaFixed > PHYSIC_DELTA_TIME)
			{
				tDelta = PHYSIC_DELTA_TIME;
				_tDeltaFixed -= PHYSIC_DELTA_TIME;
				isValidFrame = true;
			}
			else
			{
				tDelta = 0.0f;
				isValidFrame = false;
			}

			//if (isValidFrame)
			//{
			//	tmpPhysics_nUpdateValid++;
			//}
			//if (tmpPhysics_tUpdateCall > 1.0f)
			//{
			//	//Debug.Log("초당 Update Call 횟수 : " + _nUpdateCall + " / Valid : " + _nUpdateValid + " (" + _tUpdateCall + ")");
			//	tmpPhysics_tUpdateCall = 0.0f;
			//	//tmpPhysics_nUpdateCall = 0;
			//	//tmpPhysics_nUpdateValid = 0;
			//}

			//삭제 20.7.9
			//_stopwatch.Stop();
			//_stopwatch.Reset();
			//_stopwatch.Start();



			_cal_curCalParam = null;
			_cal_ResultPosList = null;
			//tmpPosList = null;//변경 21.5.16 : 이건 초기화하지 않는다.
			_cal_SubParamGroupList = null;
			_cal_SubParamKeyValueList = null;
			_cal_LayerWeight = 0.0f;
			_cal_KeyParamSetGroup = null;
			
			// 삭제 19.5.20 : 이 값을 사용하지 않음
			//apModifierParamSetGroupVertWeight weigetedVertData = null;

			_cal_CurSubList = null;
			_cal_NumParamKeys = 0;
			_cal_ParamKeyValue = null;

			//지역 변수를 여기서 일괄 선언하자
			_cal_Phy_modVertWeight = null;
			_cal_Phy_physicVertParam = null;
			_cal_Phy_physicMeshParam = null;
			_cal_Phy_Mass = 0.0f;

			_cal_Phy_F_gravity = Vector2.zero;
			_cal_Phy_F_wind = Vector2.zero;
			_cal_Phy_F_stretch = Vector2.zero;
			_cal_Phy_F_recover = Vector2.zero;

			_cal_Phy_F_ext = Vector2.zero;//<<추가된 "외부 힘"

			_cal_Phy_F_sum = Vector2.zero;
			//tmpPhysics_F_viscosity = Vector2.zero;


			tmpPhysics_linkedVert = null;
			_cal_Phy_isViscosity = false;

			_cal_Phy_srcVertPos_NoMod = Vector2.zero;
			_cal_Phy_linkVertPos_NoMod = Vector2.zero;
			_cal_Phy_srcVertPos_Cur = Vector2.zero;
			_cal_Phy_linkVertPos_Cur = Vector2.zero;
			_cal_Phy_deltaVec_0 = Vector2.zero;
			_cal_Phy_deltaVec_Cur = Vector2.zero;

			//bool isFirstDebug = true;

			//Profiler.BeginSample("Modifier : Physics");

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				_cal_curCalParam.Calculate();
				//-------------------------------------------------------


				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<



				_cal_ResultPosList = _cal_curCalParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;
				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_LayerWeight = 0.0f;
				_cal_KeyParamSetGroup = null;

				// 삭제 19.5.20 : 이 변수 삭제됨
				//weigetedVertData = calParam._weightedVertexData;

				//일단 초기화
				//기존
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//변경 21.5.15 : 배열 초기화 함수는 이걸로..
				_cal_NumVerts = _cal_ResultPosList.Length;
				System.Array.Clear(_cal_ResultPosList, 0, _cal_NumVerts);


				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;


				_cal_curCalParam._result_IsVisible = true;

				_cal_iCalculatedSubParam_Main = 0;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;



					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;



					//Vector2 calculatedValue = Vector2.zero;

					_cal_isFirstParam = true;

					

					//변경 21.5.16
					if (_cal_TmpPosList == null ||
						_cal_TmpPosList.Length < _cal_NumVerts)
					{
						_cal_TmpPosList = new Vector2[_cal_NumVerts];
					}
					System.Array.Clear(_cal_TmpPosList, 0, _cal_NumVerts);




					//tmpPhysics_TotalWeight = 0.0f;
					_cal_NumCalculated = 0;


					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					//Debug.Log("Physic " + _portrait._isPhysicsPlay_Editor + " / " + _portrait._isPhysicsSupport_Editor + " / " + tDelta);
					for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
					{
						_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];


						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();


						if (!_cal_ParamKeyValue._isCalculated)
						{ continue; }

						//tmpPhysics_TotalWeight += _cal_ParamKeyValue._weight;

						//물리 계산 순서
						//Vertex 각각의 이전프레임으로 부터의 속력 계산
						
						if (_cal_NumVerts > 0 
							&& _portrait._isPhysicsPlay_Editor 
							&& _portrait._isPhysicsSupport_Editor//<<Portrait에서 지원하는 경우만
							)
						{
							_cal_Phy_modVertWeight = null;
							_cal_Phy_physicVertParam = null;
							_cal_Phy_physicMeshParam = _cal_ParamKeyValue._modifiedMesh.PhysicParam;
							_cal_Phy_Mass = _cal_Phy_physicMeshParam._mass;
							if (_cal_Phy_Mass < 0.001f)
							{
								_cal_Phy_Mass = 0.001f;
							}

							//Vertex에 상관없이 적용되는 힘
							// 중력, 바람
							//1) 중력 : mg
							_cal_Phy_F_gravity = _cal_Phy_Mass * _cal_Phy_physicMeshParam.GetGravityAcc();

							//2) 바람 : ma
							_cal_Phy_F_wind = _cal_Phy_Mass * _cal_Phy_physicMeshParam.GetWindAcc(tDelta);

							_cal_Phy_F_stretch = Vector2.zero;
							//F_airDrag = Vector2.zero;

							//F_inertia = Vector2.zero;
							_cal_Phy_F_recover = Vector2.zero;
							_cal_Phy_F_ext = Vector2.zero;
							_cal_Phy_F_sum = Vector2.zero;

							tmpPhysics_linkedVert = null;
							_cal_Phy_isViscosity = _cal_Phy_physicMeshParam._viscosity > 0.0f;



							//---------------------------- Pos List



							for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
							{
								//여기서 물리 계산을 하자
								_cal_Phy_modVertWeight = _cal_ParamKeyValue._modifiedMesh._vertWeights[iPos];
								_cal_Phy_modVertWeight.UpdatePhysicVertex(tDelta, isValidFrame);//<<RenderVert의 위치와 속도를 계산한다.



								_cal_Phy_F_stretch = Vector2.zero;
								//F_airDrag = Vector2.zero;

								//F_inertia = Vector2.zero;
								_cal_Phy_F_recover = Vector2.zero;
								_cal_Phy_F_sum = Vector2.zero;


								if (!_cal_Phy_modVertWeight._isEnabled)
								{
									//처리 안함다
									_cal_Phy_modVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (_cal_Phy_modVertWeight._renderVertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								//최적화는 나중에 하고 일단 업데이트만이라도 하자

								_cal_Phy_physicVertParam = _cal_Phy_modVertWeight._physicParam;

								//이동 제한 범위를 초기화
								_cal_Phy_modVertWeight._isLimitPos = false;
								_cal_Phy_modVertWeight._limitScale = -1.0f;

								//추가
								//> 유효한 프레임 : 물리 계산을 한다.
								//> 생략하는 프레임 : 이전 속도를 그대로 이용한다.
								if (isValidFrame)
								{
									//1) 유효한 프레임이다.
									//Velocity_Next를 계산하자
									_cal_Phy_F_stretch = Vector2.zero;


									//Profiler.BeginSample("Physics - F-Stretch");

									//1) 장력 Strech : -k * (<delta Dist> * 기존 UnitVector)
									for (int iLinkVert = 0; iLinkVert < _cal_Phy_physicVertParam._linkedVertices.Count; iLinkVert++)
									{
										tmpPhysics_linkedVert = _cal_Phy_physicVertParam._linkedVertices[iLinkVert];
										float linkWeight = tmpPhysics_linkedVert._distWeight;

										_cal_Phy_srcVertPos_NoMod = _cal_Phy_modVertWeight._pos_World_NoMod;
										_cal_Phy_linkVertPos_NoMod = tmpPhysics_linkedVert._modVertWeight._pos_World_NoMod;
										tmpPhysics_linkedVert._deltaPosToTarget_NoMod = _cal_Phy_srcVertPos_NoMod - _cal_Phy_linkVertPos_NoMod;


										_cal_Phy_srcVertPos_Cur = _cal_Phy_modVertWeight._pos_Real;
										_cal_Phy_linkVertPos_Cur = tmpPhysics_linkedVert._modVertWeight._pos_Real;

										_cal_Phy_deltaVec_0 = _cal_Phy_srcVertPos_NoMod - _cal_Phy_linkVertPos_NoMod;
										_cal_Phy_deltaVec_Cur = _cal_Phy_srcVertPos_Cur - _cal_Phy_linkVertPos_Cur;


										//F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur - deltaVec_0) * linkWeight;//<<기존
										//F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur - deltaVec_0);
										//totalStretchWeight += linkWeight;

										//길이 차이로 힘을 만들고
										//방향은 현재 Delta

										//<추가> 만약 장력 벡터가 완전히 뒤집힌 경우
										//면이 뒤집혔다.
										if (Vector2.Dot(_cal_Phy_deltaVec_0, _cal_Phy_deltaVec_Cur) < 0)
										{
											_cal_Phy_F_stretch += _cal_Phy_physicMeshParam._stretchK * (_cal_Phy_deltaVec_0 - _cal_Phy_deltaVec_Cur) * linkWeight;
										}
										else
										{
											_cal_Phy_F_stretch += -1.0f * _cal_Phy_physicMeshParam._stretchK * (_cal_Phy_deltaVec_Cur.magnitude - _cal_Phy_deltaVec_0.magnitude) * _cal_Phy_deltaVec_Cur.normalized * linkWeight;
										}
										
									}

									//Profiler.EndSample();
									//if (totalStretchWeight > 0.0f)
									//{
									//	F_stretch /= totalStretchWeight;
									//}
									//어차피 Normalize된거라 필요없다.



									//3) 공기 저항 : "현재 이동 방향의 반대 방향"
									//F_airDrag = -1.0f * physicMeshParam._airDrag * modVertWeight._velocity_Real;

#region [미사용 코드]
									//4) 관성력 (탄성력) : "속도 변화에 따른 힘"
									//F_elastic = physicMeshParam._elasticK * ((modVertWeight._velocity_Cur - modVertWeight._velocity_Prev) / Mathf.Clamp(_tDelayedDelta, 0.1f, 0.5f)) * massPerVert;
									//F_inertia = physicMeshParam._inertiaK * ((modVertWeight._velocity_Cur - modVertWeight._velocity_Prev) / tDelta) * mass;
									//F_inertia = physicMeshParam._inertiaK * modVertWeight._acc_Cur * mass;//미리 계산된 가속도를 이용
									//F_inertia = -1.0f * physicMeshParam._inertiaK * modVertWeight._acc_Ex * mass;//미리 계산된 가속도를 이용

									//관성력을 지속하도록 하자
									//새로운 관성력이 => "방향이 비슷하고, 크기가 작다면" -> 이전 관성력을 사용
									//새로운 관성력이 => "방향이 다르거나 크기가 크다면" -> 이 관성력으로 대체하고, 타이머를 리셋
									//Vector2 unitF_inertia_Prev = modVertWeight._F_inertia_Prev.normalized;
									//Vector2 unitF_inertia_Next = F_inertia.normalized;
									//float dotProductInertia = Vector2.Dot(unitF_inertia_Prev, unitF_inertia_Next);
									//if (dotProductInertia > 0.6f && F_inertia.sqrMagnitude < modVertWeight._F_inertia_Prev.sqrMagnitude)
									//{
									//	// 방향만 바꿔주자
									//	F_inertia = unitF_inertia_Next * modVertWeight._F_inertia_Prev.magnitude;
									//}
									//else
									//{
									//	//아예 갱신
									//	modVertWeight._F_inertia_RecordMax = F_inertia;
									//	modVertWeight._tReduceInertia = 0.0f;
									//	modVertWeight._isUsePrevInertia = true;
									//}


									//F_inertia = physicMeshParam._inertiaK * (modVertWeight._velocity_Cur - modVertWeight._velocity_Prev); 
#endregion

									//5) 복원력
									_cal_Phy_F_recover = -1.0f * _cal_Phy_physicMeshParam._restoring * _cal_Phy_modVertWeight._calculatedDeltaPos;

									//6) 추가 : 외부 힘
									//이전 프레임에서의 힘을 이용한다.
									_cal_Phy_F_ext = _portrait.GetForce(_cal_Phy_modVertWeight._pos_1F);

									float inertiaK = Mathf.Clamp01(_cal_Phy_physicMeshParam._inertiaK);
									
									

									//5) 힘의 합력을 구한다.
									if (_cal_Phy_modVertWeight._physicParam._isMain)
									{
										//F_sum = F_gravity + F_wind + F_stretch + F_airDrag + F_recover + F_ext;//관성 제외
										_cal_Phy_F_sum = _cal_Phy_F_gravity + _cal_Phy_F_wind + _cal_Phy_F_stretch + _cal_Phy_F_recover + _cal_Phy_F_ext;//관성 제외 + 공기 저항도 제외
									}
									else
									{
										//F_sum = F_gravity + F_wind + F_stretch + ((F_airDrag + F_recover + F_ext) * 0.5f);//관성 제외
										_cal_Phy_F_sum = _cal_Phy_F_gravity + _cal_Phy_F_wind + _cal_Phy_F_stretch + ((_cal_Phy_F_recover + _cal_Phy_F_ext) * 0.5f);//관성 제외 + 공기 저항도 제외 //<<
										

										inertiaK *= 0.5f;//<<관성 감소
									}


									
									_cal_Phy_modVertWeight._velocity_Next = 
										//(modVertWeight._velocity_Real * inertiaK + modVertWeight._velocity_1F * (1.0f - inertiaK))
										_cal_Phy_modVertWeight._velocity_1F
										+ (_cal_Phy_modVertWeight._velocity_1F - _cal_Phy_modVertWeight._velocity_Real) * inertiaK
										+ (_cal_Phy_F_sum / _cal_Phy_Mass) * tDelta
										;

									//Air Drag식 수정
									if(_cal_Phy_physicMeshParam._airDrag > 0.0f)
									{
										_cal_Phy_modVertWeight._velocity_Next *= Mathf.Clamp01((1.0f - (_cal_Phy_physicMeshParam._airDrag * tDelta) / (_cal_Phy_Mass + 0.5f)));
									}

								}
								else
								{
									_cal_Phy_modVertWeight._velocity_Next = _cal_Phy_modVertWeight._velocity_1F;
								}

								//변경.
								//여기서 일단 속력을 미리 적용하자
								if (isValidFrame)
								{
									Vector2 nextVelocity = _cal_Phy_modVertWeight._velocity_Next;

									//V += at
									//마음대로 증가하지 않도록 한다.
									Vector2 limitedNextCalPos = _cal_Phy_modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

									//이동 제한이 걸려있다면
									if (_cal_Phy_physicMeshParam._isRestrictMoveRange)
									{
										//Profiler.BeginSample("Physics - Move Range");

										float radiusFree = _cal_Phy_physicMeshParam._moveRange * 0.5f;
										float radiusMax = _cal_Phy_physicMeshParam._moveRange;

										if (radiusMax <= radiusFree)
										{
											nextVelocity *= 0.0f;
											//둘다 0이라면 아예 이동이 불가
											if (!_cal_Phy_modVertWeight._isLimitPos)
											{
												_cal_Phy_modVertWeight._isLimitPos = true;
												_cal_Phy_modVertWeight._limitScale = 0.0f;
											}
										}
										else
										{
											float curDeltaPosSize = (limitedNextCalPos).magnitude;

											if (curDeltaPosSize < radiusFree)
											{
												//별일 없슴다
											}
											else
											{
												//기본은 선형의 사이즈이지만,
												//돌아가는 힘은 유지해야한다.
												//[deltaPos unitVector dot newVelocity] = 1일때 : 바깥으로 나가려는 힘
												// = -1일때 : 안으로 들어오려는 힘
												// -1 ~ 1 => 0 ~ 1 : 0이면 moveRatio가 1, 1이면 moveRatio가 거리에 따라 1>0
												float dotVector = Vector2.Dot(_cal_Phy_modVertWeight._calculatedDeltaPos.normalized, nextVelocity.normalized);
												dotVector = (dotVector * 0.5f) + 0.5f; //0: 속도 느려짐 없음 (안쪽으로 들어가려고 함), 1:증가하는 방향

												float outerItp = Mathf.Clamp01((curDeltaPosSize - radiusFree) / (radiusMax - radiusFree));//0 : 속도 느려짐 없음, 1:속도 0

												nextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자
																											 //limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

												if (curDeltaPosSize > radiusMax)
												{
													//limitedNextCalPos = modVertWeight._calculatedDeltaPos.normalized * radiusMax;
													if (!_cal_Phy_modVertWeight._isLimitPos || radiusMax < _cal_Phy_modVertWeight._limitScale)
													{
														_cal_Phy_modVertWeight._isLimitPos = true;
														_cal_Phy_modVertWeight._limitScale = radiusMax;
													}
												}
											}
										}

										//Profiler.EndSample();
									}

									//장력에 의한 길이 제한도 처리한다.
									if (_cal_Phy_physicMeshParam._isRestrictStretchRange)
									{

										//Profiler.BeginSample("Physics - Stretch Range");

										bool isLimitVelocity2Max = false;
										Vector2 stretchLimitPos = Vector2.zero;
										float limitCalPosDist = 0.0f;
										for (int iLinkVert = 0; iLinkVert < _cal_Phy_physicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpPhysics_linkedVert = _cal_Phy_physicVertParam._linkedVertices[iLinkVert];

											//길이의 Min/Max가 있다.
											float distStretchBase = tmpPhysics_linkedVert._deltaPosToTarget_NoMod.magnitude;

											float stretchRangeMax = (_cal_Phy_physicMeshParam._stretchRangeRatio_Max) * distStretchBase;
											float stretchRangeMax_Half = (_cal_Phy_physicMeshParam._stretchRangeRatio_Max * 0.5f) * distStretchBase;

											Vector2 curDeltaFromLinkVert = limitedNextCalPos - tmpPhysics_linkedVert._modVertWeight._calculatedDeltaPos_Prev;
											float curDistFromLinkVert = curDeltaFromLinkVert.magnitude;

											//너무 멀면 제한한다.
											//단, 제한 권장은 Weight에 맞게

											//float weight = Mathf.Clamp01(linkedVert._distWeight);
											isLimitVelocity2Max = false;

											if (curDistFromLinkVert > stretchRangeMax_Half)
											{
												isLimitVelocity2Max = true;//늘어나는 한계점으로 이동하는 중
												stretchLimitPos = tmpPhysics_linkedVert._modVertWeight._calculatedDeltaPos_Prev + curDeltaFromLinkVert.normalized * stretchRangeMax;

												if (curDistFromLinkVert > stretchRangeMax)
												{
													limitCalPosDist = (stretchLimitPos).magnitude;
												}
											}

											if (isLimitVelocity2Max)
											{
												//LinkVert간의 벡터를 기준으로 nextVelocity가 확대/축소하는 방향이라면 그 반대의 값을 넣는다.
												float dotVector = Vector2.Dot(curDeltaFromLinkVert.normalized, nextVelocity.normalized);
												//-1 : 축소하려는 방향으로 이동하는 중
												//1 : 확대하려는 방향으로 이동하는 중


												float outerItp = 0.0f;
												if (isLimitVelocity2Max)
												{
													//너무 바깥으로 이동하려고 할때, 속도를 줄인다.
													dotVector = Mathf.Clamp01(dotVector);
													if (stretchRangeMax > stretchRangeMax_Half)
													{
														outerItp = Mathf.Clamp01((curDistFromLinkVert - stretchRangeMax_Half) / (stretchRangeMax - stretchRangeMax_Half));
													}
													else
													{
														outerItp = 1.0f;//무조건 속도 0

														if (!_cal_Phy_modVertWeight._isLimitPos || limitCalPosDist < _cal_Phy_modVertWeight._limitScale)
														{
															_cal_Phy_modVertWeight._isLimitPos = true;
															_cal_Phy_modVertWeight._limitScale = limitCalPosDist;
														}
													}

												}

												nextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자

											}
										}
										//nextVelocity *= velRatio;

										//Profiler.EndSample();

										//limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);
									}

									limitedNextCalPos = _cal_Phy_modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

									_cal_Phy_modVertWeight._calculatedDeltaPos_Prev = _cal_Phy_modVertWeight._calculatedDeltaPos;
									_cal_Phy_modVertWeight._calculatedDeltaPos = limitedNextCalPos;
								}
							}

							//1차로 계산된 값을 이용하여 점성력을 체크한다.
							//수정 : 이미 위치는 계산되었다. 위치를 중심으로 처리를 하자 점성/이동한계를 계산하자
							for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
							{
								_cal_Phy_modVertWeight = _cal_ParamKeyValue._modifiedMesh._vertWeights[iPos];
								_cal_Phy_physicVertParam = _cal_Phy_modVertWeight._physicParam;

								if (!_cal_Phy_modVertWeight._isEnabled)
								{
									//처리 안함다
									_cal_Phy_modVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (_cal_Phy_modVertWeight._renderVertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								if (isValidFrame)
								{
									Vector2 nextVelocity = _cal_Phy_modVertWeight._velocity_Next;
									Vector2 nextCalPos = _cal_Phy_modVertWeight._calculatedDeltaPos;

									//[점성도]를 계산한다.
									if (_cal_Phy_isViscosity && !_cal_Phy_modVertWeight._physicParam._isMain)
									{
										//Profiler.BeginSample("Physics - Viscosity");

										//ID가 같으면 DeltaPos가 비슷해야한다.
										float linkedViscosityWeight = 0.0f;
										//Vector2 linkedViscosityNextVelocity = Vector2.zero;
										Vector2 linkedTotalCalPos = Vector2.zero;

										int curViscosityID = _cal_Phy_modVertWeight._physicParam._viscosityGroupID;

										for (int iLinkVert = 0; iLinkVert < _cal_Phy_physicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpPhysics_linkedVert = _cal_Phy_physicVertParam._linkedVertices[iLinkVert];
											float linkWeight = tmpPhysics_linkedVert._distWeight;

											if ((tmpPhysics_linkedVert._modVertWeight._physicParam._viscosityGroupID & curViscosityID) != 0)
											{
												//float subWeight = 1.0f;
												//if(!linkedVert._modVertWeight._physicParam._isMain)
												//{
												//	//subWeight *= 0.3f;
												//}
												//linkedViscosityNextVelocity += linkedVert._modVertWeight._velocity_Next * linkWeight * subWeight;//사실 Vertex의 호출 순서에 따라 값이 좀 다르다.
												linkedTotalCalPos += tmpPhysics_linkedVert._modVertWeight._calculatedDeltaPos * linkWeight;
												linkedViscosityWeight += linkWeight;
											}
										}

										//점성도를 추가한다.
										if (linkedViscosityWeight > 0.0f)
										{
											//linkedViscosityNextVelocity /= linkedViscosityWeight;
											float clampViscosity = Mathf.Clamp01(_cal_Phy_physicMeshParam._viscosity) * 0.7f;

											//if(modVertWeight._physicParam._isMain)
											//{
											//	clampViscosity *= 0.8f;
											//}

											//nextVelocity = nextVelocity * (1.0f - clampViscosity) + linkedViscosityNextVelocity * clampViscosity;
											nextCalPos = nextCalPos * (1.0f - clampViscosity) + linkedTotalCalPos * clampViscosity;
										}

										//Profiler.EndSample();

									}


									//이동 한계 한번 더 계산
									if (_cal_Phy_modVertWeight._isLimitPos && nextCalPos.magnitude > _cal_Phy_modVertWeight._limitScale)
									{
										nextCalPos = nextCalPos.normalized * _cal_Phy_modVertWeight._limitScale;
									}


									_cal_Phy_modVertWeight._calculatedDeltaPos = nextCalPos;



									//속도를 다시 계산해주자
									nextVelocity = (_cal_Phy_modVertWeight._calculatedDeltaPos - _cal_Phy_modVertWeight._calculatedDeltaPos_Prev) / tDelta;

									//-----------------------------------------------------------------------------------------
									// 속도 갱신
									_cal_Phy_modVertWeight._velocity_Next = nextVelocity;

									//modVertWeight._velocity_1F = nextVelocity;//이전 코드
									//속도 차이가 크다면 Real의 비중이 커야 한다.
									//같은 방향이면 -> 버티기 관성이 더 잘보이는게 좋다
									//다른 방향이면 Real을 관성으로 사용해야한다. (그래야 다음 프레임에 관성이 크게 보임)
									//속도 변화에 따라서 체크
									float velocityRefreshITP_X = Mathf.Clamp01(Mathf.Abs( ((_cal_Phy_modVertWeight._velocity_Real.x - _cal_Phy_modVertWeight._velocity_Real1F.x) / (Mathf.Abs(_cal_Phy_modVertWeight._velocity_Real1F.x) + 0.1f)) * 0.5f ) );
									float velocityRefreshITP_Y = Mathf.Clamp01(Mathf.Abs( ((_cal_Phy_modVertWeight._velocity_Real.y - _cal_Phy_modVertWeight._velocity_Real1F.y) / (Mathf.Abs(_cal_Phy_modVertWeight._velocity_Real1F.y) + 0.1f)) * 0.5f ) );

									_cal_Phy_modVertWeight._velocity_1F.x = nextVelocity.x * (1.0f - velocityRefreshITP_X) + (nextVelocity.x * 0.5f + _cal_Phy_modVertWeight._velocity_Real.x * 0.5f) * velocityRefreshITP_X;
									_cal_Phy_modVertWeight._velocity_1F.y = nextVelocity.y * (1.0f - velocityRefreshITP_Y) + (nextVelocity.y * 0.5f + _cal_Phy_modVertWeight._velocity_Real.y * 0.5f) * velocityRefreshITP_Y;


									_cal_Phy_modVertWeight._pos_1F = _cal_Phy_modVertWeight._pos_Real;


									//Damping
									if ((_cal_Phy_modVertWeight._calculatedDeltaPos.sqrMagnitude < _cal_Phy_physicMeshParam._damping * _cal_Phy_physicMeshParam._damping
										&& nextVelocity.sqrMagnitude < _cal_Phy_physicMeshParam._damping * _cal_Phy_physicMeshParam._damping)
										|| !_cal_Phy_modVertWeight._isPhysicsCalculatedPrevFrame)
									{
										_cal_Phy_modVertWeight._calculatedDeltaPos = Vector2.zero;
										_cal_Phy_modVertWeight.DampPhysicVertex();

										_cal_Phy_modVertWeight._isPhysicsCalculatedPrevFrame = true;
									}

								}



								_cal_TmpPosList[iPos] +=
										(_cal_Phy_modVertWeight._calculatedDeltaPos * _cal_Phy_modVertWeight._weight)
										* _cal_ParamKeyValue._weight;//<<이 값을 이용한다.




							}
							//---------------------------- Pos List
						}


						//>>>> LinkedMatrix를 만들어서 GizmoEdit를 할 수 있게 만들자
						//paramKeyValue._modifiedMesh.CalculatedLog.CalculateModified(paramKeyValue._weight, keyParamSetGroup.CalculatedLog);


						if (_cal_isFirstParam)
						{
							_cal_isFirstParam = false;
						}


						_cal_NumCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params



					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;

					
					if (_cal_iCalculatedSubParam_Main == 0)
					{
						if (_cal_LayerWeight > 0.99f)
						{
							//레이어 가중치가 1인 경우 : 직접 복사
							Array.Copy(_cal_TmpPosList, _cal_ResultPosList, _cal_NumVerts);
						}
						else
						{
							//레이어 가중치가 1 미만인 경우 : 하나씩 할당
							for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
							{
								_cal_ResultPosList[iPos] = _cal_TmpPosList[iPos] * _cal_LayerWeight;
							}
						}
						
					}
					else
					{
						switch (_cal_KeyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
									{
										_cal_ResultPosList[iPos] += _cal_TmpPosList[iPos] * _cal_LayerWeight;
									}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
									{
										_cal_ResultPosList[iPos] = (_cal_ResultPosList[iPos] * (1.0f - _cal_LayerWeight))
																	+ (_cal_TmpPosList[iPos] * _cal_LayerWeight);
									}
								}
								break;

							default:
								UnityEngine.Debug.LogError("Mod-Physics : Unknown BLEND_METHOD : " + _cal_KeyParamSetGroup._blendMethod);
								break;
						}

					}

					_cal_iCalculatedSubParam_Main += 1;
					_cal_curCalParam._isMainCalculated = true;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				
				//삭제 22.5.11
				//_cal_curCalParam._isAvailable = true;


			}

			//Profiler.EndSample();
		}




		//추가 21.7.20
		//색상만 계산하는 Calculate. Transform에서 색상 부분만 빼서 사용한다.
		protected void CalculatePattern_ColorOnly(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			_cal_isBoneTarget = false;//Bone을 대상으로 하는가 (Bone 대상이면 ModBone을 사용해야한다)
			_cal_curCalParam = null;

			//색상을 지원하는 Modifier인가			
			//_cal_IsColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;
			_cal_IsColorProperty = true;//당연~
			

			//ParamSetWeight를 사용하는가
			_cal_IsUseParamSetWeight = IsUseParamSetWeight;


			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];
				if (_cal_curCalParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					//_cal_isBoneTarget = true;//TF
					continue;//Color : Color Only 모디파이어는 ModBone을 대상으로 하지 않는다.
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					_cal_isBoneTarget = false;
				}

				//Sub List를 돌면서 Weight 체크

				//----------------------------------------------
				//1. 계산!
				_cal_curCalParam.Calculate();
				//----------------------------------------------

				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();


				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_KeyParamSetGroup = null;

				//결과 매트릭스를 만들자
				//_cal_curCalParam._result_Matrix.SetIdentity();//TF

				//메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				//색상 처리 초기화
				_cal_curCalParam._isColorCalculated = false;

				if (!_cal_isBoneTarget)
				{
					if (_cal_IsColorProperty)
					{
						_cal_curCalParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						_cal_curCalParam._result_IsVisible = false;
					}
					else
					{
						_cal_curCalParam._result_IsVisible = true;
					}
				}
				else
				{
					_cal_curCalParam._result_IsVisible = true;
				}

				//Color에서 Extra는 지원한다.
				//추가 11.29 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				_cal_curCalParam._isExtra_DepthChanged = false;
				_cal_curCalParam._isExtra_TextureChanged = false;
				_cal_curCalParam._extra_DeltaDepth = 0;
				_cal_curCalParam._extra_TextureDataID = -1;
				_cal_curCalParam._extra_TextureData = null;

				//변경 3.26 : 계산용 행렬 (apMatrixCal)을 사용하자
				//> TF
				//if(_cal_TmpMatrix == null)
				//{
				//	_cal_TmpMatrix = new apMatrixCal();
				//}


				_cal_TmpColor = Color.clear;
				_cal_TmpVisible = false;

				//추가 20.2.22 : Show/Hide 토글을 할 수 있다.
				_cal_TmpIsToggleShowHideOption = false;
				
				_cal_TmpToggleOpt_IsAnyKey_Shown = false;
				_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
				_cal_TmpToggleOpt_IsAny_Hidden = false;
				//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Cal = 0.0f;

				

				//추가 11.29 : Extra Option 계산 값				
				_cal_TmpExtra_DepthChanged = false;
				_cal_TmpExtra_TextureChanged = false;
				_cal_TmpExtra_DeltaDepth = 0;
				_cal_TmpExtra_TextureDataID = 0;
				_cal_TmpExtra_TextureData = null;
				_cal_TmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				_cal_TmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값

				_cal_LayerWeight = 0.0f;

				_cal_iCalculatedSubParam_Main = 0;

				
			


				_cal_iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				_cal_TmpIsColoredKeyParamSetGroup = false;

				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						continue;
					}


					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;

					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;

					//추가 20.4.2 : 애니메이션 모디파이어일때.
					if(IsAnimated && !_cal_KeyParamSetGroup.IsAnimEnabledInEditor)
					{	
						//선택되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
						//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
						continue;
					}

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					//TF
					//_cal_isExCalculatable_Transform = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Transform;
					//_cal_isExCalculatable_Color = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Color;

					//Color Only
					_cal_isExCalculatable_Transform = false;
					_cal_isExCalculatable_Color = true;//Color는 무조건

					
					_cal_isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					//_cal_TmpMatrix.SetZero();//TF
					_cal_TmpColor = Color.clear;
					_cal_TmpVisible = false;

					_cal_LayerWeight = 0.0f;

					_cal_TotalParamSetWeight = 0.0f;
					_cal_NumCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					//> TF
					//_cal_TmpIsColoredKeyParamSetGroup = _cal_IsColorProperty && _cal_KeyParamSetGroup._isColorPropertyEnabled && !_cal_isBoneTarget && _cal_isExCalculatable_Color;
					
					//> Color Only에선 항상 true
					_cal_TmpIsColoredKeyParamSetGroup = true;



					//추가 20.2.22 : ShowHide 토글 변수 설정 및 관련 변수 초기화
					//오직 컨트롤 파라미터 타입이여야 하며, ParamSetGroup이 Color 옵션과 Toggle 옵션을 지원해야한다.
					//_cal_TmpIsToggleShowHideOption = !IsAnimated && _cal_TmpIsColoredKeyParamSetGroup && _cal_KeyParamSetGroup._isToggleShowHideWithoutBlend;//>TF
					_cal_TmpIsToggleShowHideOption = !IsAnimated && _cal_KeyParamSetGroup._isToggleShowHideWithoutBlend;//>Color

					_cal_TmpToggleOpt_IsAnyKey_Shown = false;
					_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
					_cal_TmpToggleOpt_IsAny_Hidden = false;
					//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;


					if (!_cal_isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우
						for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
						{
							_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
							
							if (!_cal_ParamKeyValue._isCalculated) { continue; }

							//ParamSetWeight를 추가
							_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;


							
							//Modifier + KeyParamSetGroup 모두 Color를 지원해야함
							
							//TF와 달리 _cal_TmpIsColoredKeyParamSetGroup는 체크하지 않는다. (항상 true니까)

							if (!_cal_TmpIsToggleShowHideOption)
							{
								//기본 방식
								if (_cal_ParamKeyValue._modifiedMesh._isVisible)
								{
									_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
									_cal_TmpVisible = true;
								}
								else
								{
									//Visible이 False
									Color paramColor = _cal_ParamKeyValue._modifiedMesh._meshColor;
									paramColor.a = 0.0f;
									_cal_TmpColor += paramColor * _cal_ParamKeyValue._weight;
								}
							}
							else
							{
								//추가 20.2.22 : 토글 방식의 ShowHide 방식
								if (_cal_ParamKeyValue._modifiedMesh._isVisible && _cal_ParamKeyValue._weight > 0.0f)
								{
									//paramKeyValue._paramSet.ControlParamValue
									_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
									_cal_TmpVisible = true;//< 일단 이것도 true

									//토글용 처리
									_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

									//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
									if (!_cal_TmpToggleOpt_IsAnyKey_Shown)
									{
										_cal_TmpToggleOpt_KeyIndex_Shown = _cal_TmpToggleOpt_KeyIndex_Cal;
									}
									else
									{
										//Show Key Index 중 가장 작은 값을 기준으로 한다.
										_cal_TmpToggleOpt_KeyIndex_Shown = (_cal_TmpToggleOpt_KeyIndex_Cal < _cal_TmpToggleOpt_KeyIndex_Shown ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Shown);
									}

										
									_cal_TmpToggleOpt_IsAnyKey_Shown = true;

									_cal_TmpToggleOpt_TotalWeight_Shown += _cal_ParamKeyValue._weight;
									_cal_TmpToggleOpt_MaxWeight_Shown = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Shown ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Shown);

								}
								else
								{
									//토글용 처리
									_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

									if (!_cal_TmpToggleOpt_IsAny_Hidden)
									{
										_cal_TmpToggleOpt_KeyIndex_Hidden = _cal_TmpToggleOpt_KeyIndex_Cal;
									}
									else
									{
										//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
										_cal_TmpToggleOpt_KeyIndex_Hidden = (_cal_TmpToggleOpt_KeyIndex_Cal > _cal_TmpToggleOpt_KeyIndex_Hidden ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Hidden);
									}

									_cal_TmpToggleOpt_IsAny_Hidden = true;
									//_cal_TmpToggleOpt_TotalWeight_Hidden += _cal_ParamKeyValue._weight;
									_cal_TmpToggleOpt_MaxWeight_Hidden = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Hidden ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Hidden);
								}
							}

							//---------------------------------------------
							//추가 11.29 : Extra Option
							if(_isExtraPropertyEnabled)//Color 에서 Extra를 지원한다.
							{	
								//1. Modifier의 Extra Property가 켜져 있어야 한다.
								//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
								//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
								//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
								if (_cal_ParamKeyValue._modifiedMesh._isExtraValueEnabled
									&& (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged || _cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged)
									)
								{
									
									//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
									float extraWeight = _cal_ParamKeyValue._weight;//<<일단 가중치를 더한다.
									float bias = 0.0001f;
									float cutOut = 0.0f;
									bool isExactWeight = false;
									if (IsAnimated)
									{
										switch (_cal_ParamKeyValue._animKeyPos)
										{
											case apCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
											case apCalculatedResultParam.AnimKeyPos.NextKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
											case apCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
										}
									}
									else
									{
										cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout;
									}

									cutOut = Mathf.Clamp01(cutOut + 0.01f);//살짝 겹치게

									if (isExactWeight)
									{
										extraWeight = 10000.0f;
									}
									else if (cutOut < bias)
									{
										//정확하면 최대값
										//아니면 적용안함
										if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
										else { extraWeight = -1.0f; }
									}
									else
									{
										if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
										else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
									}

									if (extraWeight > 0.0f)
									{
										//if (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged && _cal_isExCalculatable_Transform)
										if (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged)//버그! 수정 21.9.8
										{
											//2-1. Depth 이벤트
											if(extraWeight > _cal_TmpExtra_DepthMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												//Debug.Log("Depth Changed [" + DisplayName + "] : " + paramKeyValue._modifiedMesh._renderUnit.Name 
												//	+ " / ExtraWeight : " 
												//	+ extraWeight + " / CurMaxWeight : " + tmpExtra_DepthMaxWeight);

												_cal_TmpExtra_DepthMaxWeight = extraWeight;
												_cal_TmpExtra_DepthChanged = true;
												_cal_TmpExtra_DeltaDepth = _cal_ParamKeyValue._modifiedMesh._extraValue._deltaDepth;
											}

										}
										//if (_cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged && _cal_isExCalculatable_Color)
										if (_cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged)//단순 코드 수정 21.9.28
										{
											//2-2. Texture 이벤트
											if(extraWeight > _cal_TmpExtra_TextureMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												_cal_TmpExtra_TextureMaxWeight = extraWeight;
												_cal_TmpExtra_TextureChanged = true;
												_cal_TmpExtra_TextureData = _cal_ParamKeyValue._modifiedMesh._extraValue._linkedTextureData;
												_cal_TmpExtra_TextureDataID = _cal_ParamKeyValue._modifiedMesh._extraValue._textureDataID;
											}
										}
									}
								}
							}
							//---------------------------------------------




							if (_cal_isFirstParam)
							{
								_cal_isFirstParam = false;
							}
							_cal_NumCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}
					}
					//이제 레이어순서에 따른 보간을 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_cal_IsUseParamSetWeight)
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);
					}
					else
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight * Mathf.Clamp01(_cal_TotalParamSetWeight));
					}


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					
					//> Color (필수)
					_cal_curCalParam._totalParamSetGroupWeight_Color += _cal_LayerWeight;

					//if ((_cal_NumCalculated == 0 && _cal_IsColorProperty) || _cal_isBoneTarget)//>TF
					if (_cal_NumCalculated == 0)//>Color
					{
						_cal_TmpVisible = true;
					}


					//변경 : 색상은 별도로 카운팅해서 처리하자
					//_cal_TmpIsColoredKeyParamSetGroup는 체크하지 않는다. 항상 true
					if (_cal_TmpIsToggleShowHideOption)
					{
						//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.

						if (_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
						{
							//Show / Hide가 모두 있다면 토글 대상
							if (_cal_TmpToggleOpt_MaxWeight_Shown > _cal_TmpToggleOpt_MaxWeight_Hidden)
							{
								//Show가 더 크다
								_cal_TmpVisible = true;
							}
							else if (_cal_TmpToggleOpt_MaxWeight_Shown < _cal_TmpToggleOpt_MaxWeight_Hidden)
							{
								//Hidden이 더 크다
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}
							else
							{
								//같다면? (Weight가 0.5 : 0.5로 같은 경우)
								if (_cal_TmpToggleOpt_KeyIndex_Shown > _cal_TmpToggleOpt_KeyIndex_Hidden)
								{
									//Show의 ParamSet의 키 인덱스가 더 크다.
									_cal_TmpVisible = true;
								}
								else
								{
									//Hidden이 더 크다
									_cal_TmpVisible = false;
									_cal_TmpColor = Color.clear;
								}
							}
						}
						else if (_cal_TmpToggleOpt_IsAnyKey_Shown && !_cal_TmpToggleOpt_IsAny_Hidden)
						{
							//Show만 있다면
							_cal_TmpVisible = true;
						}
						else if (!_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
						{
							//Hide만 있다면
							_cal_TmpVisible = false;
							_cal_TmpColor = Color.clear;
						}
						else
						{
							//둘다 없다면? 숨기자.
							_cal_TmpVisible = false;
							_cal_TmpColor = Color.clear;
						}

						//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
						if (_cal_TmpVisible && _cal_TmpToggleOpt_TotalWeight_Shown > 0.0f)
						{
							_cal_TmpColor.r = Mathf.Clamp01(_cal_TmpColor.r / _cal_TmpToggleOpt_TotalWeight_Shown);
							_cal_TmpColor.g = Mathf.Clamp01(_cal_TmpColor.g / _cal_TmpToggleOpt_TotalWeight_Shown);
							_cal_TmpColor.b = Mathf.Clamp01(_cal_TmpColor.b / _cal_TmpToggleOpt_TotalWeight_Shown);
							_cal_TmpColor.a = Mathf.Clamp01(_cal_TmpColor.a / _cal_TmpToggleOpt_TotalWeight_Shown);
						}
					}

					if (_cal_iColoredKeyParamSetGroup == 0 || _cal_KeyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
					{
						//색상 Interpolation
						_cal_curCalParam._result_Color = apUtil.BlendColor_ITP(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
						_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
					}
					else
					{
						//색상 Additive
						_cal_curCalParam._result_Color = apUtil.BlendColor_Add(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
						_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
					}
					_cal_iColoredKeyParamSetGroup++;
					_cal_curCalParam._isColorCalculated = true;

					//추가 11.29 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(_cal_TmpExtra_DepthChanged)
						{
							_cal_curCalParam._isExtra_DepthChanged = true;
							_cal_curCalParam._extra_DeltaDepth = _cal_TmpExtra_DeltaDepth;
						}

						if(_cal_TmpExtra_TextureChanged)
						{
							_cal_curCalParam._isExtra_TextureChanged = true;
							_cal_curCalParam._extra_TextureData = _cal_TmpExtra_TextureData;
							_cal_curCalParam._extra_TextureDataID = _cal_TmpExtra_TextureDataID;
						}
					}

				}

				//삭제 22.5.11
				////? 처리된게 하나도 없어요?
				//if (_cal_iCalculatedSubParam == 0)
				//{
				//	//Active를 False로 날린다.
				//	_cal_curCalParam._isAvailable = false;
				//}
				//else
				//{
				//	_cal_curCalParam._isAvailable = true;

				//	//매트릭스 계산은 하지 않는다.
				//	//_cal_curCalParam._result_Matrix.CalculateScale_FromLerp();
				//}

			}
		}


		//-------------------------------------------------------------
		// C++ DLL의 도움을 받는 Calculate Pattern 함수들
		//-------------------------------------------------------------
#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_SetWeightedPosList(	ref Vector2[] dstVectorArr, 
																ref Vector2[] srcVectorArr, 
																int arrLength, 
																float weight);


#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_AddWeightedPosList(	ref Vector2[] dstVectorArr, 
																ref Vector2[] srcVectorArr, 
																int arrLength, 
																float weight);

#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_InterpolateWeightedPosList(	ref Vector2[] dstVectorArr, 
																		ref Vector2[] srcVectorArr, 
																		int arrLength, 
																		float weight);


		protected void CalculatePattern_Morph_DLL(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			//계산용 변수 초기화
			_cal_curCalParam = null;
			_cal_ResultPosList = null;
			//tmpPosList = null;//변경 21.5.16 : 이건 이제 초기화하지 않고, 한번 생성된 배열을 계속 유지한다.
			_cal_SubParamGroupList = null;
			_cal_SubParamKeyValueList = null;
			_cal_LayerWeight = 0.0f;
			_cal_KeyParamSetGroup = null;

			_cal_CurSubList = null;
			_cal_NumParamKeys = 0;
			_cal_ParamKeyValue = null;

			//색상을 지원하는 Modifier인가
			_cal_IsColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;

			//ParamSetWeight를 사용하는가
			_cal_IsUseParamSetWeight = IsUseParamSetWeight;

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				_cal_curCalParam.Calculate();
				//-------------------------------------------------------

				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				//색상 처리 초기화
				_cal_curCalParam._isColorCalculated = false;


				_cal_ResultPosList = _cal_curCalParam._result_Positions;
				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_LayerWeight = 0.0f;
				_cal_KeyParamSetGroup = null;

				//일단 초기화
				//기존
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//변경 21.5.15 : 배열 초기화 함수는 이걸로..
				_cal_NumVerts = _cal_ResultPosList.Length;
				System.Array.Clear(_cal_ResultPosList, 0, _cal_NumVerts);



				//[Pin] 추가 22.5.11 [v1.4.0]
				_cal_ResultPinPosList = _cal_curCalParam._result_PinPositions;
				_cal_NumPins = _cal_ResultPinPosList != null ? _cal_ResultPinPosList.Length : 0;
				if (_cal_NumPins > 0)
				{
					System.Array.Clear(_cal_ResultPinPosList, 0, _cal_NumPins);
				}

				_cal_TargetPinGroup = null;
				if(_cal_curCalParam._targetRenderUnit._meshTransform != null
					&& _cal_curCalParam._targetRenderUnit._meshTransform._mesh != null)
				{
					_cal_TargetPinGroup = _cal_curCalParam._targetRenderUnit._meshTransform._mesh._pinGroup;
				}
				if(_cal_TargetPinGroup == null)
				{
					_cal_NumPins = 0;
				}



				if (_cal_IsColorProperty)
				{
					_cal_curCalParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					_cal_curCalParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					_cal_curCalParam._result_IsVisible = true;
				}

				//추가 11.29 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				_cal_curCalParam._isExtra_DepthChanged = false;
				_cal_curCalParam._isExtra_TextureChanged = false;
				_cal_curCalParam._extra_DeltaDepth = 0;
				_cal_curCalParam._extra_TextureDataID = -1;
				_cal_curCalParam._extra_TextureData = null;


				_cal_TmpColor = Color.clear;
				_cal_TmpVisible = false;

				//추가 20.2.22 : Show/Hide 토글을 할 수 있다.
				_cal_TmpIsToggleShowHideOption = false;
				
				_cal_TmpToggleOpt_IsAnyKey_Shown = false;
				_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
				_cal_TmpToggleOpt_IsAny_Hidden = false;
				_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Cal = 0.0f;


				//추가 11.29 : Extra Option 계산 값				
				_cal_TmpExtra_DepthChanged = false;
				_cal_TmpExtra_TextureChanged = false;
				_cal_TmpExtra_DeltaDepth = 0;
				_cal_TmpExtra_TextureDataID = 0;
				_cal_TmpExtra_TextureData = null;
				_cal_TmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				_cal_TmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값

				_cal_iCalculatedSubParam_Main = 0;

				_cal_iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				_cal_TmpIsColoredKeyParamSetGroup = false;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;


					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;

					if(IsAnimated && !_cal_KeyParamSetGroup.IsAnimEnabledInEditor)
					{	
						//선택되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
						//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
						continue;
					}

					//tmpPosList = keyParamSetGroup._tmpPositions;//삭제 21.5.16

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					_cal_isExCalculatable_Transform = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Transform;
					_cal_isExCalculatable_Color = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Color;
					

					_cal_isFirstParam = true;


					//변경 21.5.16 : tmpPosList를 이용하되, 최대값으로 만들자 (첫프레임에서는 계속 생성하게 될 거임)
					if (_cal_TmpPosList == null ||
						_cal_TmpPosList.Length < _cal_NumVerts)//부등호로 만들어서 항상 최대값을 유지한다.
					{
						_cal_TmpPosList = new Vector2[_cal_NumVerts];						
					}
					
					//변경 21.5.15 : 배열 초기화 함수는 이걸로..
					System.Array.Clear(_cal_TmpPosList, 0, _cal_NumVerts);


					//[Pin] 추가 22.3.29 (v1.4.0)
					//Pin 계산을 위한 배열 크기 체크
					if(_cal_NumPins > 0)
					{
						if(_cal_TmpPinPosList == null || _cal_TmpPinPosList.Length < _cal_NumPins)
						{
							_cal_TmpPinPosList = new Vector2[_cal_NumPins];
						}
						System.Array.Clear(_cal_TmpPinPosList, 0, _cal_NumPins);
					}


					_cal_TmpColor = Color.clear;
					_cal_TmpVisible = false;

					_cal_TotalParamSetWeight = 0.0f;
					_cal_NumCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					_cal_TmpIsColoredKeyParamSetGroup = _cal_IsColorProperty && _cal_KeyParamSetGroup._isColorPropertyEnabled && _cal_isExCalculatable_Color;

					//추가 20.2.22 : ShowHide 토글 변수 설정 및 관련 변수 초기화
					//오직 컨트롤 파라미터 타입이여야 하며, ParamSetGroup이 Color 옵션과 Toggle 옵션을 지원해야한다.
					_cal_TmpIsToggleShowHideOption = !IsAnimated && _cal_TmpIsColoredKeyParamSetGroup && _cal_KeyParamSetGroup._isToggleShowHideWithoutBlend;

					_cal_TmpToggleOpt_IsAnyKey_Shown = false;
					_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
					_cal_TmpToggleOpt_IsAny_Hidden = false;
					//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;


					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					
					for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
					{
						_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];

						
						if (!_cal_ParamKeyValue._isCalculated)
						{
							continue;
						}

						_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;



						//---------------------------- Pos List
						if (_cal_isExCalculatable_Transform)//<<추가
						{
							//[Pin] 추가 22.3.29 (v1.4.0)
							//핀이 없는 경우 : (Mod Delta Pos * PKV Weight)를 바로 Tmp에 더하기
							//핀이 있는 경우 : "Mod Delta Pos"에 Pin 정보를 가공한 후, PKV Weight를 곱하여 Tmp에 더하기

							if (_cal_NumPins > 0)
							{
								//Pin이 있는 경우
								//- Pin의 변경된 위치 계산 (Tmp 아니며 Mod Delta Pos를 PinGroup에 직접 적용)
								//- Pin의 커브 계산 후 Matrix 계산
								//- Pin-Vertex 가중치에 의해 "이동된 Vertex"에 적용.
								// (단 "버텍스의 이동 정보"가 단순 더하기 연산이 아닌 Rigging과 같은 행렬 연산으로 적용되어야 한다.)
								//- Pin의 Tmp 리스트에도 저장 (이건 미리보기 용이며 가중치 적용)
								apMeshPin curPin = null;
								Vector2 curPinDeltaPos = Vector2.zero;

								apModifiedVertex curModVert = null;
								apModifiedPin curModPin = null;
								apVertex curVert = null;
								Vector2 curVertPosModMid_WOPin = Vector2.zero;
								Vector2 curVertPosModMid_SumPinWeighted = Vector2.zero;
								Vector2 curPinWeightedPos = Vector2.zero;
								Vector2 curPinWeightedResultPos = Vector2.zero;
								apMeshPinVertWeight curPinVertWeight = null;
								apMeshPinVertWeight.VertPinPair curPinWeightPair = null;
								apMatrix3x3 curveVert2WorldMatrix = apMatrix3x3.identity;
								apMatrix3x3 curveCurveMatrix = apMatrix3x3.identity;


								// (1) Pin의 변경된 위치 계산 (Mod-Final로의 저장도 포함)
								for (int iPin = 0; iPin < _cal_NumPins; iPin++)
								{	
									curModPin = _cal_ParamKeyValue._modifiedMesh._pins[iPin];
									curPin = curModPin._pin;
									curPinDeltaPos = curModPin._deltaPos;

									//- 임시 변수(Mod-Mid)에 DeltaPos를 바로 적용
									//현재 PKV에 대해서만 Curve를 생성하기 위해서 ModMid를 생성
									curPin.SetTmpPos_ModMid(curPin._defaultPos + curPinDeltaPos);

									//- Mod-Final 계산을 위한 Tmp 리스트에 가중치 적용
									_cal_TmpPinPosList[iPin] += curPinDeltaPos * _cal_ParamKeyValue._weight;
								}

								// (2) 현재 상태에서 Pin의 Matrix 계산하고 Curve 연산
								_cal_TargetPinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.Update_ModMid);

								// 버텍스마다 돌면서 다음 단계 처리
								for (int iVert = 0; iVert < _cal_NumVerts; iVert++)
								{
									curModVert = _cal_ParamKeyValue._modifiedMesh._vertices[iVert];
									curVert = curModVert._vertex;

									// (3) 핀 그룹을 이용해서 가중치 정보 가져와서 Pin에 의한 위치 계산 (합)
									curPinVertWeight = _cal_TargetPinGroup._vertWeights[iVert];
									int nPairs = curPinVertWeight._nPairs;
									if(nPairs == 0)
									{
										//이 버텍스는 핀과 연결되지 않았다.
										//바로 Delta 값을 TmpList에 할당한다. (PKV 가중치 포함)
										_cal_TmpPosList[iVert] = curModVert._deltaPos * _cal_ParamKeyValue._weight;
										continue;
									}
									
									// 버텍스의 Pin 적용 전에 순수하게 Morph에 의해 변형된 Mod-Mid 위치 계산 (Default + DeltaPos)
									//curVertPosModMid_WOPin = curVert._pos + curModVert._deltaPos;//방법 1 : 먼저 이동한 후 커브 Matrix 적용 > 단점 : 역연산 불가로 기즈모 편집 불가
									curVertPosModMid_WOPin = curVert._pos;//방법 2 : 커브 Matrix 적용 후 Vert Delta를 나중에 합산
									
									curVertPosModMid_SumPinWeighted = Vector2.zero;//합산 초기화
									curPinWeightedPos = Vector2.zero;

									for (int iWeightPair = 0; iWeightPair < nPairs; iWeightPair++)
									{
										//핀-버텍스 가중치
										curPinWeightPair = curPinVertWeight._vertPinPairs[iWeightPair];

										//Pair 정보가 "단일 핀 연결"/"핀간의 커브와 연결"에 따라 계산이 다르다.
										if(!curPinWeightPair._isCurveWeighted)
										{
											//- 단일 핀과 연결된 경우
											curPinWeightedPos = curPinWeightPair._linkedPin.TmpMultiplyVertPos(apMeshPin.TMP_VAR_TYPE.ModMid, ref curVertPosModMid_WOPin);
										}
										else
										{
											//- 핀과 핀 사이의 커브와 연결된 경우
											//커브 행렬
											curveCurveMatrix = curPinWeightPair._linkedPin._nextCurve.GetCurveMatrix_Test(apMeshPin.TMP_VAR_TYPE.ModMid, curPinWeightPair._curveLerp);
											curveVert2WorldMatrix = curveCurveMatrix * curPinWeightPair._curveDefaultMatrix_Inv;
											curPinWeightedPos = curveVert2WorldMatrix.MultiplyPoint(curVertPosModMid_WOPin);
										}

										//가중치를 이용하여 Weight에 의한 위치 합
										curVertPosModMid_SumPinWeighted += curPinWeightedPos * curPinWeightPair._weight;
									}

									//Total Weight를 이용하여 최종 위치를 계산하자
									curPinWeightedResultPos = (curVertPosModMid_WOPin * (1.0f - curPinVertWeight._totalWeight)) + (curVertPosModMid_SumPinWeighted * curPinVertWeight._totalWeight);
									
									//기본 Pos를 빼서 Tmp에 넣는다. 완료!
									Vector2 deltaPos = curPinWeightedResultPos - curVert._pos;

									
									deltaPos += curModVert._deltaPos;//방법 2 한정 : Vertex의 Mod Delta를 최종 결과에 더한다. 역연산을 위함

									_cal_TmpPosList[iVert] += deltaPos * _cal_ParamKeyValue._weight;
								}
							}
							else
							{
								//Pin이 없는 경우
								//- 원래 방식대로 계산
								for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
								{
									_cal_TmpPosList[iPos] += _cal_ParamKeyValue._modifiedMesh._vertices[iPos]._deltaPos * _cal_ParamKeyValue._weight;
								}
							}
						}
						//---------------------------- Pos List


						if (_cal_isFirstParam)
						{
							_cal_isFirstParam = false;
						}

						if (_cal_TmpIsColoredKeyParamSetGroup)
						{
							if (!_cal_TmpIsToggleShowHideOption)
							{
								//기본 방식
								if (_cal_ParamKeyValue._modifiedMesh._isVisible)
								{
									_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
									_cal_TmpVisible = true;//하나라도 Visible이면 Visible이 된다.
								}
								else
								{
									//Visible이 False
									Color paramColor = _cal_ParamKeyValue._modifiedMesh._meshColor;
									paramColor.a = 0.0f;
									_cal_TmpColor += paramColor * _cal_ParamKeyValue._weight;
								}
							}
							else
							{
								//추가 20.2.22 : 토글 방식의 ShowHide 방식
								if (_cal_ParamKeyValue._modifiedMesh._isVisible && _cal_ParamKeyValue._weight > 0.0f)
								{
									//paramKeyValue._paramSet.ControlParamValue
									_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
									_cal_TmpVisible = true;//< 일단 이것도 true
									
									//토글용 처리
									_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

									//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
									if (!_cal_TmpToggleOpt_IsAnyKey_Shown)
									{
										_cal_TmpToggleOpt_KeyIndex_Shown = _cal_TmpToggleOpt_KeyIndex_Cal;
									}
									else
									{
										//Show Key Index 중 가장 작은 값을 기준으로 한다.
										_cal_TmpToggleOpt_KeyIndex_Shown = (_cal_TmpToggleOpt_KeyIndex_Cal < _cal_TmpToggleOpt_KeyIndex_Shown ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Shown);
									}

										
									_cal_TmpToggleOpt_IsAnyKey_Shown = true;

									_cal_TmpToggleOpt_TotalWeight_Shown += _cal_ParamKeyValue._weight;
									_cal_TmpToggleOpt_MaxWeight_Shown = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Shown ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Shown);
									
								}
								else
								{
									//토글용 처리
									_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

									if (!_cal_TmpToggleOpt_IsAny_Hidden)
									{
										_cal_TmpToggleOpt_KeyIndex_Hidden = _cal_TmpToggleOpt_KeyIndex_Cal;
									}
									else
									{
										//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
										_cal_TmpToggleOpt_KeyIndex_Hidden = (_cal_TmpToggleOpt_KeyIndex_Cal > _cal_TmpToggleOpt_KeyIndex_Hidden ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Hidden);
									}

									_cal_TmpToggleOpt_IsAny_Hidden = true;
									//_cal_TmpToggleOpt_TotalWeight_Hidden += _cal_ParamKeyValue._weight;
									_cal_TmpToggleOpt_MaxWeight_Hidden = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Hidden ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Hidden);
								}
							}
							
						}


						//---------------------------------------------
						//추가 11.29 : Extra Option
						if(_isExtraPropertyEnabled)
						{
							//1. Modifier의 Extra Property가 켜져 있어야 한다.
							//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
							//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
							//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
							if (_cal_ParamKeyValue._modifiedMesh._isExtraValueEnabled
								&& (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged || _cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged)
								)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = _cal_ParamKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (_cal_ParamKeyValue._animKeyPos)
									{
										case apCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
										case apCalculatedResultParam.AnimKeyPos.NextKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}

									
								}
								else
								{
									cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + 0.01f);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
									else { extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
									else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged && _cal_isExCalculatable_Transform)
									{
										//2-1. Depth 이벤트
										if(extraWeight > _cal_TmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											_cal_TmpExtra_DepthMaxWeight = extraWeight;
											_cal_TmpExtra_DepthChanged = true;
											_cal_TmpExtra_DeltaDepth = _cal_ParamKeyValue._modifiedMesh._extraValue._deltaDepth;
										}

									}
									if (_cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged && _cal_isExCalculatable_Color)
									{
										//2-2. Texture 이벤트
										if(extraWeight > _cal_TmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											_cal_TmpExtra_TextureMaxWeight = extraWeight;
											_cal_TmpExtra_TextureChanged = true;
											_cal_TmpExtra_TextureData = _cal_ParamKeyValue._modifiedMesh._extraValue._linkedTextureData;
											_cal_TmpExtra_TextureDataID = _cal_ParamKeyValue._modifiedMesh._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------------------------

						_cal_NumCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_cal_IsUseParamSetWeight)
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);
					}
					else
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight * Mathf.Clamp01(_cal_TotalParamSetWeight));
					}


					//>>> Linked Matrix < KeyParamSetGroup >
					//keyParamSetGroup.LinkedMatrix.SetPassAndMerge(apLinkedMatrix.VALUE_TYPE.VertPos).SetWeight(layerWeight);


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					// Transform과 Color를 나눔
					if(_cal_isExCalculatable_Transform)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;
					}
					if(_cal_isExCalculatable_Color)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Color += _cal_LayerWeight;
					}
					

					if (_cal_NumCalculated == 0)
					{
						_cal_TmpVisible = true;
					}

					//변경 22.5.11 : 레이어 처리 방식을 변경 (버그때문에)
					if(_cal_isExCalculatable_Transform)
					{
						if(_cal_iCalculatedSubParam_Main == 0)
						{
							//C++ DLL 코드
							Modifier_SetWeightedPosList(ref _cal_ResultPosList, ref _cal_TmpPosList, _cal_NumVerts, _cal_LayerWeight);

							//추가 22.3.31 [v1.4.0] : Pin World 위치 계산
							if(_cal_NumPins > 0)
							{	
								Modifier_SetWeightedPosList(ref _cal_ResultPinPosList, ref _cal_TmpPinPosList, _cal_NumPins, _cal_LayerWeight);
							}
						}
						else
						{
							switch (_cal_KeyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//C++ DLL 코드
									Modifier_AddWeightedPosList(ref _cal_ResultPosList, ref _cal_TmpPosList, _cal_NumVerts, _cal_LayerWeight);

									//추가 22.3.31 [v1.4.0] : Pin World 위치 계산
									if(_cal_NumPins > 0)
									{
										Modifier_AddWeightedPosList(ref _cal_ResultPinPosList, ref _cal_TmpPinPosList, _cal_NumPins, _cal_LayerWeight);
									}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//C++ DLL 코드
									Modifier_InterpolateWeightedPosList(ref _cal_ResultPosList, ref _cal_TmpPosList, _cal_NumVerts, _cal_LayerWeight);

									//추가 22.3.31 [v1.4.0] : Pin World 위치 계산
									if(_cal_NumPins > 0)
									{
										Modifier_InterpolateWeightedPosList(ref _cal_ResultPinPosList, ref _cal_TmpPinPosList, _cal_NumPins, _cal_LayerWeight);
									}
								}
								break;

							default:
								Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + _cal_KeyParamSetGroup._blendMethod);
								break;
						}
						}

						_cal_iCalculatedSubParam_Main += 1;
						_cal_curCalParam._isMainCalculated = true;
					}
					

					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (_cal_TmpIsColoredKeyParamSetGroup)
					{
						if (_cal_TmpIsToggleShowHideOption)
						{
							//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.

							if (_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (_cal_TmpToggleOpt_MaxWeight_Shown > _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									_cal_TmpVisible = true;
								}
								else if (_cal_TmpToggleOpt_MaxWeight_Shown < _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									_cal_TmpVisible = false;
									_cal_TmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (_cal_TmpToggleOpt_KeyIndex_Shown > _cal_TmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										_cal_TmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										_cal_TmpVisible = false;
										_cal_TmpColor = Color.clear;
									}
								}
							}
							else if (_cal_TmpToggleOpt_IsAnyKey_Shown && !_cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								_cal_TmpVisible = true;
							}
							else if (!_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (_cal_TmpVisible && _cal_TmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								_cal_TmpColor.r = Mathf.Clamp01(_cal_TmpColor.r / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.g = Mathf.Clamp01(_cal_TmpColor.g / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.b = Mathf.Clamp01(_cal_TmpColor.b / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.a = Mathf.Clamp01(_cal_TmpColor.a / _cal_TmpToggleOpt_TotalWeight_Shown);
							}
						}

						if (_cal_iColoredKeyParamSetGroup == 0 || _cal_KeyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							_cal_curCalParam._result_Color = apUtil.BlendColor_ITP(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						else
						{
							//색상 Additive
							_cal_curCalParam._result_Color = apUtil.BlendColor_Add(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						
						_cal_iColoredKeyParamSetGroup++;
						_cal_curCalParam._isColorCalculated = true;
					}


					//추가 11.29 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(_cal_TmpExtra_DepthChanged)
						{
							_cal_curCalParam._isExtra_DepthChanged = true;
							_cal_curCalParam._extra_DeltaDepth = _cal_TmpExtra_DeltaDepth;
						}

						if(_cal_TmpExtra_TextureChanged)
						{
							_cal_curCalParam._isExtra_TextureChanged = true;
							_cal_curCalParam._extra_TextureData = _cal_TmpExtra_TextureData;
							_cal_curCalParam._extra_TextureDataID = _cal_TmpExtra_TextureDataID;
						}
					}

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)

				//삭제 22.5.11
				////? 처리된게 하나도 없어요?
				//if (_cal_iCalculatedSubParam == 0)
				//{
				//	//Active를 False로 날린다.
				//	_cal_curCalParam._isAvailable = false;
				//}
				//else
				//{
				//	_cal_curCalParam._isAvailable = true;
				//}
			}
		}




		protected void CalculatePattern_Transform_DLL(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			_cal_isBoneTarget = false;//Bone을 대상으로 하는가 (Bone 대상이면 ModBone을 사용해야한다)
			_cal_curCalParam = null;

			//색상을 지원하는 Modifier인가
			_cal_IsColorProperty = _isColorPropertyEnabled && (int)((CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color)) != 0;
			
			//ParamSetWeight를 사용하는가
			_cal_IsUseParamSetWeight = IsUseParamSetWeight;

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];
				if (_cal_curCalParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					_cal_isBoneTarget = true;
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					_cal_isBoneTarget = false;
				}

				//Sub List를 돌면서 Weight 체크

				//----------------------------------------------
				//1. 계산!
				_cal_curCalParam.Calculate();
				//----------------------------------------------

				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_KeyParamSetGroup = null;


				//결과 매트릭스를 만들자
				_cal_curCalParam._result_Matrix.SetIdentity();


				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				//색상 처리 초기화
				_cal_curCalParam._isColorCalculated = false;

				if (!_cal_isBoneTarget)
				{
					if (_cal_IsColorProperty)
					{
						_cal_curCalParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						_cal_curCalParam._result_IsVisible = false;
					}
					else
					{
						_cal_curCalParam._result_IsVisible = true;
					}
				}
				else
				{
					_cal_curCalParam._result_IsVisible = true;
				}

				//추가 11.29 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				_cal_curCalParam._isExtra_DepthChanged = false;
				_cal_curCalParam._isExtra_TextureChanged = false;
				_cal_curCalParam._extra_DeltaDepth = 0;
				_cal_curCalParam._extra_TextureDataID = -1;
				_cal_curCalParam._extra_TextureData = null;

				//변경 3.26 : 계산용 행렬 (apMatrixCal)을 사용하자
				//apMatrix tmpMatrix = null;
				if(_cal_TmpMatrix == null)
				{
					_cal_TmpMatrix = new apMatrixCal();
				}


				_cal_TmpColor = Color.clear;
				_cal_TmpVisible = false;

				//추가 20.2.22 : Show/Hide 토글을 할 수 있다.
				_cal_TmpIsToggleShowHideOption = false;
				
				_cal_TmpToggleOpt_IsAnyKey_Shown = false;
				_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
				_cal_TmpToggleOpt_IsAny_Hidden = false;
				//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;
				_cal_TmpToggleOpt_KeyIndex_Cal = 0.0f;

				//추가 11.29 : Extra Option 계산 값				
				_cal_TmpExtra_DepthChanged = false;
				_cal_TmpExtra_TextureChanged = false;
				_cal_TmpExtra_DeltaDepth = 0;
				_cal_TmpExtra_TextureDataID = 0;
				_cal_TmpExtra_TextureData = null;
				_cal_TmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				_cal_TmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값

				_cal_LayerWeight = 0.0f;

				_cal_iCalculatedSubParam_Main = 0;


				_cal_iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				_cal_TmpIsColoredKeyParamSetGroup = false;

				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						continue;
					}


					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;

					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;

					//추가 20.4.2 : 애니메이션 모디파이어일때.
					if(IsAnimated && !_cal_KeyParamSetGroup.IsAnimEnabledInEditor)
					{	
						//선택되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
						//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
						continue;
					}

					//추가 3.22
					//Transfrom / Color Update 여부를 따로 결정한다.
					_cal_isExCalculatable_Transform = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Transform;
					_cal_isExCalculatable_Color = _cal_CurSubList._keyParamSetGroup.IsExCalculatable_Color;


					//추가 21.9.1 : <회전 보정>
					_cal_isRotation180Correction = !IsAnimated && _cal_KeyParamSetGroup._tfRotationLerpMethod == apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector;
					_cal_Rotation180Correction_DeltaAngle = 0.0f;

					_cal_isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					_cal_TmpMatrix.SetZero();
					_cal_TmpColor = Color.clear;
					_cal_TmpVisible = false;

					_cal_LayerWeight = 0.0f;

					_cal_TotalParamSetWeight = 0.0f;
					_cal_NumCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					_cal_TmpIsColoredKeyParamSetGroup = _cal_IsColorProperty && _cal_KeyParamSetGroup._isColorPropertyEnabled && !_cal_isBoneTarget && _cal_isExCalculatable_Color;

					//추가 20.2.22 : ShowHide 토글 변수 설정 및 관련 변수 초기화
					//오직 컨트롤 파라미터 타입이여야 하며, ParamSetGroup이 Color 옵션과 Toggle 옵션을 지원해야한다.
					_cal_TmpIsToggleShowHideOption = !IsAnimated && _cal_TmpIsColoredKeyParamSetGroup && _cal_KeyParamSetGroup._isToggleShowHideWithoutBlend;

					_cal_TmpToggleOpt_IsAnyKey_Shown = false;
					_cal_TmpToggleOpt_TotalWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Shown = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Shown = 0.0f;
					_cal_TmpToggleOpt_IsAny_Hidden = false;
					//_cal_TmpToggleOpt_TotalWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_MaxWeight_Hidden = 0.0f;
					_cal_TmpToggleOpt_KeyIndex_Hidden = 0.0f;


					if (!_cal_isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우

						//추가 20.9.10 : 정밀한 보간을 위해 Default Matrix가 필요하다.
						apMatrix defaultMatrixOfRenderUnit = null;
						//bool isDebug = false;
						if(_cal_curCalParam._targetRenderUnit != null)
						{
							if(_cal_curCalParam._targetRenderUnit._meshTransform != null)
							{
								defaultMatrixOfRenderUnit = _cal_curCalParam._targetRenderUnit._meshTransform._matrix_TF_ToParent;
							}
							else if(_cal_curCalParam._targetRenderUnit._meshGroupTransform != null)
							{
								defaultMatrixOfRenderUnit = _cal_curCalParam._targetRenderUnit._meshGroupTransform._matrix_TF_ToParent;
							}
						}

						
						for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
						{
							_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
							
							if (!_cal_ParamKeyValue._isCalculated) { continue; }

							//ParamSetWeight를 추가
							_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;


							if (_cal_isExCalculatable_Transform)//<<추가
							{
								//Weight에 맞게 Matrix를 만들자

								if (_cal_ParamKeyValue._isAnimRotationBias)
								{
									//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
									//이전 : apMatrix를 사용할 때
									//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

									//변경 3.26 : apMatrixCal을 사용한다.
									_cal_TmpMatrix.AddMatrixParallel_ModMesh(_cal_ParamKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, _cal_ParamKeyValue._weight);
								}
								else
								{
									//기본 식
									//이전 : apMatrix를 사용할 때
									//tmpMatrix.AddMatrix(paramKeyValue._modifiedMesh._transformMatrix, paramKeyValue._weight, false);

									//변경 3.26 : apMatrixCal을 사용한다.
									_cal_TmpMatrix.AddMatrixParallel_ModMesh(_cal_ParamKeyValue._modifiedMesh._transformMatrix, defaultMatrixOfRenderUnit, _cal_ParamKeyValue._weight/*, isDebug*/);
								}
							}


							
							//Modifier + KeyParamSetGroup 모두 Color를 지원해야함
							if (_cal_TmpIsColoredKeyParamSetGroup)
							{
								if (!_cal_TmpIsToggleShowHideOption)
								{
									//기본 방식
									if (_cal_ParamKeyValue._modifiedMesh._isVisible)
									{
										_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
										_cal_TmpVisible = true;
									}
									else
									{
										//Visible이 False
										Color paramColor = _cal_ParamKeyValue._modifiedMesh._meshColor;
										paramColor.a = 0.0f;
										_cal_TmpColor += paramColor * _cal_ParamKeyValue._weight;
									}
								}
								else
								{
									//추가 20.2.22 : 토글 방식의 ShowHide 방식
									if (_cal_ParamKeyValue._modifiedMesh._isVisible && _cal_ParamKeyValue._weight > 0.0f)
									{
										//paramKeyValue._paramSet.ControlParamValue
										_cal_TmpColor += _cal_ParamKeyValue._modifiedMesh._meshColor * _cal_ParamKeyValue._weight;
										_cal_TmpVisible = true;//< 일단 이것도 true

										//토글용 처리
										_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

										//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
										if (!_cal_TmpToggleOpt_IsAnyKey_Shown)
										{
											_cal_TmpToggleOpt_KeyIndex_Shown = _cal_TmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Show Key Index 중 가장 작은 값을 기준으로 한다.
											_cal_TmpToggleOpt_KeyIndex_Shown = (_cal_TmpToggleOpt_KeyIndex_Cal < _cal_TmpToggleOpt_KeyIndex_Shown ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Shown);
										}

										
										_cal_TmpToggleOpt_IsAnyKey_Shown = true;

										_cal_TmpToggleOpt_TotalWeight_Shown += _cal_ParamKeyValue._weight;
										_cal_TmpToggleOpt_MaxWeight_Shown = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Shown ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Shown);

									}
									else
									{
										//토글용 처리
										_cal_TmpToggleOpt_KeyIndex_Cal = _cal_ParamKeyValue._paramSet.ComparableIndex;

										if (!_cal_TmpToggleOpt_IsAny_Hidden)
										{
											_cal_TmpToggleOpt_KeyIndex_Hidden = _cal_TmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
											_cal_TmpToggleOpt_KeyIndex_Hidden = (_cal_TmpToggleOpt_KeyIndex_Cal > _cal_TmpToggleOpt_KeyIndex_Hidden ? _cal_TmpToggleOpt_KeyIndex_Cal : _cal_TmpToggleOpt_KeyIndex_Hidden);
										}

										_cal_TmpToggleOpt_IsAny_Hidden = true;
										//_cal_TmpToggleOpt_TotalWeight_Hidden += _cal_ParamKeyValue._weight;
										_cal_TmpToggleOpt_MaxWeight_Hidden = (_cal_ParamKeyValue._weight > _cal_TmpToggleOpt_MaxWeight_Hidden ? _cal_ParamKeyValue._weight : _cal_TmpToggleOpt_MaxWeight_Hidden);
									}
								}
								
							}


							//---------------------------------------------
							//추가 11.29 : Extra Option
							if(_isExtraPropertyEnabled)
							{
								//1. Modifier의 Extra Property가 켜져 있어야 한다.
								//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
								//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
								//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
								if (_cal_ParamKeyValue._modifiedMesh._isExtraValueEnabled
									&& (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged || _cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged)
									)
								{
									//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
									float extraWeight = _cal_ParamKeyValue._weight;//<<일단 가중치를 더한다.
									float bias = 0.0001f;
									float cutOut = 0.0f;
									bool isExactWeight = false;
									if (IsAnimated)
									{
										switch (_cal_ParamKeyValue._animKeyPos)
										{
											case apCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
											case apCalculatedResultParam.AnimKeyPos.NextKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
											case apCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
										}
									}
									else
									{
										cutOut = _cal_ParamKeyValue._modifiedMesh._extraValue._weightCutout;
									}

									cutOut = Mathf.Clamp01(cutOut + 0.01f);//살짝 겹치게

									if (isExactWeight)
									{
										extraWeight = 10000.0f;
									}
									else if (cutOut < bias)
									{
										//정확하면 최대값
										//아니면 적용안함
										if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
										else { extraWeight = -1.0f; }
									}
									else
									{
										if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
										else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
									}

									if (extraWeight > 0.0f)
									{
										if (_cal_ParamKeyValue._modifiedMesh._extraValue._isDepthChanged && _cal_isExCalculatable_Transform)
										{
											//2-1. Depth 이벤트
											if(extraWeight > _cal_TmpExtra_DepthMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												//Debug.Log("Depth Changed [" + DisplayName + "] : " + paramKeyValue._modifiedMesh._renderUnit.Name 
												//	+ " / ExtraWeight : " 
												//	+ extraWeight + " / CurMaxWeight : " + tmpExtra_DepthMaxWeight);

												_cal_TmpExtra_DepthMaxWeight = extraWeight;
												_cal_TmpExtra_DepthChanged = true;
												_cal_TmpExtra_DeltaDepth = _cal_ParamKeyValue._modifiedMesh._extraValue._deltaDepth;
											}

										}
										if (_cal_ParamKeyValue._modifiedMesh._extraValue._isTextureChanged && _cal_isExCalculatable_Color)
										{
											//2-2. Texture 이벤트
											if(extraWeight > _cal_TmpExtra_TextureMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												_cal_TmpExtra_TextureMaxWeight = extraWeight;
												_cal_TmpExtra_TextureChanged = true;
												_cal_TmpExtra_TextureData = _cal_ParamKeyValue._modifiedMesh._extraValue._linkedTextureData;
												_cal_TmpExtra_TextureDataID = _cal_ParamKeyValue._modifiedMesh._extraValue._textureDataID;
											}
										}
									}
								}
							}
							//---------------------------------------------




							if (_cal_isFirstParam)
							{
								_cal_isFirstParam = false;
							}
							_cal_NumCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}


						//추가 21.9.1
						//180도 각도 보정을 위해서는 전체 파라미터를 따로 돌아서 벡터 합에 의한 회전각을 따로 계산해야한다.
						if(_cal_isRotation180Correction && _cal_isExCalculatable_Transform)
						{
							_cal_Rotation180Correction_DeltaAngle = 0.0f;
							_cal_Rotation180Correction_SumVector = Vector2.zero;
							for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
							{
								_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
								if(!_cal_ParamKeyValue._isCalculated)
								{
									continue;
								}
								float curAngle = _cal_ParamKeyValue._modifiedMesh._transformMatrix._angleDeg * Mathf.Deg2Rad;
								_cal_Rotation180Correction_CurVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
								_cal_Rotation180Correction_SumVector += (_cal_Rotation180Correction_CurVector * _cal_ParamKeyValue._weight) * 10.0f;//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							}

							//벡터합을 역산해서 현재 상태의 평균 합을 구하자
							//Weight 합이 0이거나, 서로 반대방향을 바라보는 경우가 아니라면..
							if(_cal_Rotation180Correction_SumVector.sqrMagnitude > 0.001f)
							{
								_cal_Rotation180Correction_SumVector *= 10.0f;
								_cal_Rotation180Correction_DeltaAngle = Mathf.Atan2(_cal_Rotation180Correction_SumVector.y, _cal_Rotation180Correction_SumVector.x) * Mathf.Rad2Deg;
								//Debug.Log("보정 각도 : " + _cal_Rotation180Correction_DeltaAngle + " / 벡터 : " + sumVec);
							}

							_cal_TmpMatrix._angleDeg = _cal_Rotation180Correction_DeltaAngle;
						}


						_cal_TmpMatrix.CalculateScale_FromAdd();
						_cal_TmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit/*, isDebug*/);//추가 (20.9.10) : 위치 보간이슈 수정

					}
					else
					{
						
						//ModBone을 활용하는 타입인 경우
						for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
						{
							//paramKeyValue = calParam._paramKeyValues[iPV];
							_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);

							if (!_cal_ParamKeyValue._isCalculated)
							{
								continue;
							}

							//ParamSetWeight를 추가
							_cal_TotalParamSetWeight += _cal_ParamKeyValue._weight * _cal_ParamKeyValue._paramSet._overlapWeight;


							//Weight에 맞게 Matrix를 만들자
							if (_cal_isExCalculatable_Transform)
							{
								if (_cal_ParamKeyValue._isAnimRotationBias)
								{
									_cal_TmpMatrix.AddMatrixParallel_ModBone(_cal_ParamKeyValue.AnimRotationBiasedMatrix, _cal_ParamKeyValue._weight);
								}
								else
								{
									_cal_TmpMatrix.AddMatrixParallel_ModBone(_cal_ParamKeyValue._modifiedBone._transformMatrix, _cal_ParamKeyValue._weight);
								}
							}

							if (_cal_isFirstParam)
							{
								_cal_isFirstParam = false;
							}
							_cal_NumCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}


						//추가 21.9.1
						//180도 각도 보정을 위해서는 전체 파라미터를 따로 돌아서 벡터 합에 의한 회전각을 따로 계산해야한다.
						if(_cal_isRotation180Correction && _cal_isExCalculatable_Transform)
						{
							_cal_Rotation180Correction_DeltaAngle = 0.0f;
							_cal_Rotation180Correction_SumVector = Vector2.zero;
							for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
							{
								_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];
								if(!_cal_ParamKeyValue._isCalculated)
								{
									continue;
								}
								float curAngle = _cal_ParamKeyValue._modifiedBone._transformMatrix._angleDeg * Mathf.Deg2Rad;
								_cal_Rotation180Correction_CurVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
								_cal_Rotation180Correction_SumVector += (_cal_Rotation180Correction_CurVector * _cal_ParamKeyValue._weight) * 10.0f;//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							}

							//벡터합을 역산해서 현재 상태의 평균 합을 구하자
							//Weight 합이 0이거나, 서로 반대방향을 바라보는 경우가 아니라면..
							if(_cal_Rotation180Correction_SumVector.sqrMagnitude > 0.001f)
							{
								_cal_Rotation180Correction_SumVector *= 10.0f;
								_cal_Rotation180Correction_DeltaAngle = Mathf.Atan2(_cal_Rotation180Correction_SumVector.y, _cal_Rotation180Correction_SumVector.x) * Mathf.Rad2Deg;
								//Debug.Log("보정 각도 : " + _cal_Rotation180Correction_DeltaAngle + " / 벡터 : " + sumVec);
							}

							_cal_TmpMatrix._angleDeg = _cal_Rotation180Correction_DeltaAngle;
						}



						//위치 변경 20.9.10
						_cal_TmpMatrix.CalculateScale_FromAdd();
					}

					//이제 레이어순서에 따른 보간을 해주자
					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_cal_IsUseParamSetWeight)
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);
					}
					else
					{
						_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight * Mathf.Clamp01(_cal_TotalParamSetWeight));
					}


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					// Transform과 Color를 나눔
					if(_cal_isExCalculatable_Transform)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;
					}
					if(_cal_isExCalculatable_Color)
					{
						_cal_curCalParam._totalParamSetGroupWeight_Color += _cal_LayerWeight;
					}



					if ((_cal_NumCalculated == 0 && _cal_IsColorProperty) || _cal_isBoneTarget)
					{
						_cal_TmpVisible = true;
					}

					//변경 22.5.11 : 처리 순서 변경
					if(_cal_isExCalculatable_Transform)
					{
						if(_cal_iCalculatedSubParam_Main == 0)
						{
							//변경 3.26 : apMatrixCal로 계산된 tmpMatrix
							_cal_curCalParam._result_Matrix.SetTRSForLerp(_cal_TmpMatrix);
						}
						else
						{
							switch (_cal_KeyParamSetGroup._blendMethod)
							{
								case apModifierParamSetGroup.BLEND_METHOD.Additive:
									{
										//변경 3.26 : apMatrixCal로 계산된 AddMatrix
										_cal_curCalParam._result_Matrix.AddMatrixLayered(_cal_TmpMatrix, _cal_LayerWeight);
									}
									break;

								case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
									{
										//변경 3.26 : apMatrixCal로 계산 된 AddMatrix
										_cal_curCalParam._result_Matrix.LerpMatrixLayered(_cal_TmpMatrix, _cal_LayerWeight);
									}
									break;

								default:
									Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + _cal_KeyParamSetGroup._blendMethod);
									break;
							}
						}

						_cal_iCalculatedSubParam_Main += 1;
						_cal_curCalParam._isMainCalculated = true;
					}

					

					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (_cal_TmpIsColoredKeyParamSetGroup)
					{
						if (_cal_TmpIsToggleShowHideOption)
						{
							//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.

							if (_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (_cal_TmpToggleOpt_MaxWeight_Shown > _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									_cal_TmpVisible = true;
								}
								else if (_cal_TmpToggleOpt_MaxWeight_Shown < _cal_TmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									_cal_TmpVisible = false;
									_cal_TmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (_cal_TmpToggleOpt_KeyIndex_Shown > _cal_TmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										_cal_TmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										_cal_TmpVisible = false;
										_cal_TmpColor = Color.clear;
									}
								}
							}
							else if (_cal_TmpToggleOpt_IsAnyKey_Shown && !_cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								_cal_TmpVisible = true;
							}
							else if (!_cal_TmpToggleOpt_IsAnyKey_Shown && _cal_TmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								_cal_TmpVisible = false;
								_cal_TmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (_cal_TmpVisible && _cal_TmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								_cal_TmpColor.r = Mathf.Clamp01(_cal_TmpColor.r / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.g = Mathf.Clamp01(_cal_TmpColor.g / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.b = Mathf.Clamp01(_cal_TmpColor.b / _cal_TmpToggleOpt_TotalWeight_Shown);
								_cal_TmpColor.a = Mathf.Clamp01(_cal_TmpColor.a / _cal_TmpToggleOpt_TotalWeight_Shown);
							}
						}

						if (_cal_iColoredKeyParamSetGroup == 0 || _cal_KeyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							_cal_curCalParam._result_Color = apUtil.BlendColor_ITP(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						else
						{
							//색상 Additive
							_cal_curCalParam._result_Color = apUtil.BlendColor_Add(_cal_curCalParam._result_Color, _cal_TmpColor, _cal_LayerWeight);
							_cal_curCalParam._result_IsVisible |= _cal_TmpVisible;
						}
						_cal_iColoredKeyParamSetGroup++;
						_cal_curCalParam._isColorCalculated = true;
					}

					//추가 11.29 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(_cal_TmpExtra_DepthChanged)
						{
							_cal_curCalParam._isExtra_DepthChanged = true;
							_cal_curCalParam._extra_DeltaDepth = _cal_TmpExtra_DeltaDepth;
						}

						if(_cal_TmpExtra_TextureChanged)
						{
							_cal_curCalParam._isExtra_TextureChanged = true;
							_cal_curCalParam._extra_TextureData = _cal_TmpExtra_TextureData;
							_cal_curCalParam._extra_TextureDataID = _cal_TmpExtra_TextureDataID;
						}
					}
				}

				////? 처리된게 하나도 없어요?
				//if (_cal_iCalculatedSubParam == 0)
				//{
				//	//Active를 False로 날린다.
				//	_cal_curCalParam._isAvailable = false;
				//}
				//else
				//{
				//	_cal_curCalParam._isAvailable = true;

				//	//이전 : apMatrix로 계산된 경우
				//	//calParam._result_Matrix.MakeMatrix();

				//	//변경 : apMatrixCal로 계산한 경우
				//	_cal_curCalParam._result_Matrix.CalculateScale_FromLerp();
				//}

				//변경 22.5.11 : 처리가 되었다면
				if(_cal_curCalParam._isMainCalculated)
				{
					//변경 : apMatrixCal로 계산한 경우
					_cal_curCalParam._result_Matrix.CalculateScale_FromLerp();
				}

			}
		}



#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_CalculateRiggingMatrix(	ref Vector2 dst_ResultPos,
																	ref apMatrix3x3 dst_Matrix,
																	ref Vector2 src_VertPosW_NoMod,
																	ref Vector2 cal_Rig_VertPos_BoneLocal,
																	ref Vector2 cal_Rig_VertPosW_BoneWorld,
																	ref Vector2 cal_Rig_VertPosL_Result,
																	ref apMatrix3x3 cal_Rig_Matx_Result,
																	ref apMatrix3x3 src_Matx_boneWorld_Default_Inv,
																	ref apMatrix3x3 src_Matx_boneWorld_Mod,
																	ref apMatrix3x3 src_Matx_MeshW_NoMod,
																	ref apMatrix3x3 src_Matx_MeshW_NoMod_Inv,
																	ref apMatrix3x3 src_Matx_Vert2Local_Inv,
																	float src_RigWeight);



		protected void CalculatePattern_Rigging_DLL(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				//Debug.LogError("Result Param Count : 0");
				return;
			}

			//Debug.Log("Rigging - " + _meshGroup._name);
			//Profiler.BeginSample("Rigging Calculate");

			_cal_curCalParam = null;
			_cal_ResultPosList = null;
			//tmpPosList = null;//변경 21.5.16 : 이건 이제 초기화하지 않고, 한번 생성된 것을 계속 유지한다.

			//Pos대신 Matrix
			_cal_ResultVertMatrixList = null;

			//tmpVertMatrixList = null;//21.5.16 이 둘은 초기화하지 않는다.
			//tmpVertWeightList = null;

			_cal_SubParamGroupList = null;
			_cal_SubParamKeyValueList = null;
			_cal_LayerWeight = 0.0f;
			_cal_KeyParamSetGroup = null;
			_cal_CurSubList = null;
			_cal_NumParamKeys = 0;
			_cal_ParamKeyValue = null;


			_cal_isRiggingWithIK = _meshGroup.IsRiggingWithIK;
			

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요! -> Static은 Weight 계산이 필요없어염
				//-------------------------------------------------------
				//1. Param Weight Calculate
				//_cal_curCalParam.Calculate();//Rigging은 필요없다.
				//-------------------------------------------------------

				_cal_ResultPosList = _cal_curCalParam._result_Positions;
				_cal_ResultVertMatrixList = _cal_curCalParam._result_VertMatrices;
			
				//tmpPosList = calParam._tmp_Positions;
				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_LayerWeight = 0.0f;

				//일단 초기화
				//이전
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//변경 21.5.15 : 배열 초기화 함수는 이걸로..
				_cal_NumVerts = _cal_ResultPosList.Length;				
				System.Array.Clear(_cal_ResultPosList, 0, _cal_NumVerts);

				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				_cal_curCalParam._result_IsVisible = true;


				_cal_TmpColor = Color.clear;
				_cal_iCalculatedSubParam_Main = 0;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;

					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;//<<

					
					//레이어 내부의 임시 데이터를 먼저 초기화
					//KeyParamSetGroup의 임시 변수들은 사용하지 않는다.
					//tmpPosList = keyParamSetGroup._tmpPositions;
					//tmpVertMatrixList = keyParamSetGroup._tmpVertMatrices;
					//tmpVertWeightList = keyParamSetGroup._tmpVertRiggingWeights;//추가



					//리깅은 Ex 편집이 아예 없다.

					//변경 21.5.16 : 최대값으로 한번 생성하고 재사용하도록 변경
					if (_cal_TmpPosList == null ||
						_cal_TmpVertMatrixList == null ||
						_cal_TmpVertWeightList == null ||
						_cal_TmpPosList.Length < _cal_NumVerts || 
						_cal_TmpVertMatrixList.Length < _cal_NumVerts||
						_cal_TmpVertWeightList.Length < _cal_NumVerts)
					{
						_cal_TmpPosList = new Vector2[_cal_NumVerts];
						_cal_TmpVertMatrixList = new apMatrix3x3[_cal_NumVerts];
						_cal_TmpVertWeightList = new float[_cal_NumVerts];
					}

					//변경 21.5.15 : 배열 초기화 함수는 이걸로.. (행렬은 3x2 초기화라 어쩔 수 없다.)
					System.Array.Clear(_cal_TmpPosList, 0, _cal_NumVerts);
					System.Array.Clear(_cal_TmpVertWeightList, 0, _cal_NumVerts);

					for (int iMatrix = 0; iMatrix < _cal_NumVerts; iMatrix++)
					{
						_cal_TmpVertMatrixList[iMatrix].SetZero3x2();
					}


					_cal_TmpColor = Color.clear;
					//tmpVisible = false;

					_cal_NumCalculated = 0;


					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
					{
						_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];

						//>>>> Cal Log 초기화
						//paramKeyValue._modifiedMesh.CalculatedLog.ReadyToRecord();

						_cal_ParamKeyValue._weight = 1.0f;

						//Modified가 안된 Vert World Pos + Bone의 Modified 안된 World Matrix + Bone의 World Matrix (변형됨) 순으로 계산한다.
						_cal_Rig_matx_Vert2Local = _cal_ParamKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal;
						
						//역행렬 생성 이슈
						//이전 : 이것도 매번 생성하는건 좋지 않다.
						//apMatrix3x3 matx_Vert2Local_Inv = matx_Vert2Local.inverse;
						
						//변경 21.5.16
						_cal_Rig_matx_Vert2Local_Inv = _cal_ParamKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal_Inverse;

						_cal_tmpRig_matx_MeshW_NoMod = _cal_ParamKeyValue._modifiedMesh._renderUnit._meshTransform._matrix_TFResult_WorldWithoutMod;
						
						//UnityEngine.Profiling.Profiler.BeginSample("Rigging - Matrix");

						//---------------------------- Pos List
						// < TODO : C++ DLL로 개선할 것 >
						for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
						{
							//1. Mod가 적용안된 Vert의 World Pos
							_cal_Rig_curVertRig = _cal_ParamKeyValue._modifiedMesh._vertRigs[iPos];
							_cal_Rig_VertPosW_NoMod = _cal_tmpRig_matx_MeshW_NoMod.MulPoint2(_cal_Rig_matx_Vert2Local.MultiplyPoint(_cal_Rig_curVertRig._vertex._pos));


							//2. Bone의 (Mod가 적용 안된) World Matrix의 역행렬을 계산하여 Local Vert by Bone을 만든다.
							//3. Bone의 World Matrix를 계산하여 연산한다.
							_cal_Rig_TotalBoneWeight = 0.0f;
							_cal_Rig_WeightPair = null;
							
							//기존 방식 [Skew 이슈]
							//apMatrix matx_boneWorld_Mod = null;
							//apMatrix matx_boneWorld_Default = null;

							//변경 20.8.12 : apComplexMatrix > 20.8.17 : 래핑
							_cal_Rig_Matx_boneWorld_Mod = null;
							_cal_Rig_Matx_boneWorld_Default = null;

							//수정 : Rigging을 vertPos가 아닌 Matrix의 합으로 계산한다.
							_cal_Rig_Matx_Result = apMatrix3x3.identity;
							 
							for (int iWeight = 0; iWeight < _cal_Rig_curVertRig._weightPairs.Count; iWeight++)
							{
								_cal_Rig_WeightPair = _cal_Rig_curVertRig._weightPairs[iWeight];

								if (_cal_Rig_WeightPair._weight <= 0.0001f)
								{
									continue;
								}

								//Profiler.BeginSample("4-2-1-1. Matrix Calculate");

								if(_cal_isRiggingWithIK)
								{
									_cal_Rig_Matx_boneWorld_Mod = _cal_Rig_WeightPair._bone._worldMatrix_IK;//<<추가 : IK가 포함된 Rigging으로 계산한다.
								}
								else
								{
									_cal_Rig_Matx_boneWorld_Mod = _cal_Rig_WeightPair._bone._worldMatrix;
								}

								//[ C# 코드 ]
								//_cal_Rig_Matx_boneWorld_Default = _cal_Rig_WeightPair._bone._worldMatrix_NonModified;

								////World -> Bone Local
								//_cal_Rig_VertPos_BoneLocal = _cal_Rig_Matx_boneWorld_Default.InvMulPoint2(_cal_Rig_VertPosW_NoMod);

								////Bone Local -> World
								//_cal_Rig_VertPosW_BoneWorld = _cal_Rig_Matx_boneWorld_Mod.MulPoint2(_cal_Rig_VertPos_BoneLocal);

								////vertPos_OnlyReverseMesh = matx_Vert2Local_Inv.MultiplyPoint(matx_MeshW_NoMod.InvMulPoint2(vertPosW_NoMod));

								////다시 이것의 Local Pos를 구한다.
								//_cal_Rig_VertPosL_Result = _cal_Rig_matx_Vert2Local_Inv.MultiplyPoint(_cal_tmpRig_matx_MeshW_NoMod.InvMulPoint2(_cal_Rig_VertPosW_BoneWorld));


								////TODO : 이거 Vert가 아닌 Mesh 단계에서 미리 만들 수 없나 (Lookup 방식)
								////여기서 성능 많이 향상될 듯
								////Mesh와 Bone 조합별로 미리 만들면 Vert에서 가져다 쓰면 되지

								////<Vert2Local> 단계를 제외한 Bone matrix 계산식
								//_cal_Rig_Matx_Result = _cal_tmpRig_matx_MeshW_NoMod.MtrxToLowerSpace
								//	* _cal_Rig_Matx_boneWorld_Mod.MtrxToSpace
								//	* _cal_Rig_Matx_boneWorld_Default.MtrxToLowerSpace
								//	* _cal_tmpRig_matx_MeshW_NoMod.MtrxToSpace
								//	;


								////Vert에 저장하는 방식
								//_cal_TmpPosList[iPos] += _cal_Rig_VertPosL_Result * _cal_Rig_WeightPair._weight;

								////Matrix에 저장하는 방식
								//_cal_TmpVertMatrixList[iPos] += _cal_Rig_Matx_Result * _cal_Rig_WeightPair._weight;


								apMatrix3x3 matx_boneWorld_Default_Inv = _cal_Rig_WeightPair._bone._worldMatrix_NonModified.MtrxToLowerSpace;
								apMatrix3x3 matx_boneWorld_Mod = _cal_Rig_Matx_boneWorld_Mod.MtrxToSpace;
								apMatrix3x3 matx_meshWorld_NoMod = _cal_tmpRig_matx_MeshW_NoMod.MtrxToSpace;
								apMatrix3x3 matx_meshWorld_NoMod_Inv = _cal_tmpRig_matx_MeshW_NoMod.MtrxToLowerSpace;

								//[ C++ 코드 ]
								Modifier_CalculateRiggingMatrix(ref _cal_TmpPosList[iPos],
																ref _cal_TmpVertMatrixList[iPos],
																ref _cal_Rig_VertPosW_NoMod,
																ref _cal_Rig_VertPos_BoneLocal,
																ref _cal_Rig_VertPosW_BoneWorld,
																ref _cal_Rig_VertPosL_Result,
																ref _cal_Rig_Matx_Result,
																ref matx_boneWorld_Default_Inv,
																ref matx_boneWorld_Mod,
																ref matx_meshWorld_NoMod,
																ref matx_meshWorld_NoMod_Inv,
																ref _cal_Rig_matx_Vert2Local_Inv,
																_cal_Rig_WeightPair._weight
																);
								
								_cal_Rig_TotalBoneWeight += _cal_Rig_WeightPair._weight;
							}

							//추가
							_cal_TmpVertWeightList[iPos] = Mathf.Clamp01(_cal_Rig_TotalBoneWeight);

							if (_cal_Rig_TotalBoneWeight > 0.0f)
							{
								_cal_TmpPosList[iPos] = new Vector2(_cal_TmpPosList[iPos].x / _cal_Rig_TotalBoneWeight, _cal_TmpPosList[iPos].y / _cal_Rig_TotalBoneWeight);
								_cal_TmpVertMatrixList[iPos] /= _cal_Rig_TotalBoneWeight;
							}
							else
							{
								//Bone Weight가 지정되지 않았을 때
								_cal_TmpPosList[iPos] = _cal_Rig_curVertRig._vertex._pos;
								_cal_TmpVertMatrixList[iPos].SetIdentity();
							}
						}
						//---------------------------- Pos List

						
						_cal_NumCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					_cal_LayerWeight = 1.0f;

					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;

					
					//< TODO : C++ DLL로 바꿀 것 >
					for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
					{
						_cal_ResultPosList[iPos] = _cal_TmpPosList[iPos] * _cal_LayerWeight;
						
						//이전 코드 : 일반 Matrix
						//vertMatrixList[iPos].SetMatrixWithWeight(tmpVertMatrixList[iPos], layerWeight);

						//변경 : Bone Weight가 1 미만인 경우도 적용하기 위해 Normalize 이전의 Weight를 곱한다.
						_cal_ResultVertMatrixList[iPos].SetMatrixWithWeight(ref _cal_TmpVertMatrixList[iPos], _cal_LayerWeight * _cal_TmpVertWeightList[iPos]);


					}

					//Profiler.EndSample();

					//Profiler.BeginSample("4-3. Save Log");

					//>>> CalculatedLog
					//keyParamSetGroup.CalculatedLog.CalculateParamSetGroup(layerWeight,
					//														iCalculatedSubParam,
					//														apModifierParamSetGroup.BLEND_METHOD.Interpolation,
					//														null,
					//														calParam.CalculatedLog);

					_cal_iCalculatedSubParam_Main += 1;
					_cal_curCalParam._isMainCalculated = true;
					//Profiler.EndSample();


					//Profiler.EndSample();

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				//_cal_curCalParam._isAvailable = true;//삭제 22.5.11


			}

			//Profiler.EndSample();
		}



		

		protected void CalculatePattern_Physics_DLL(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			bool isValidFrame = false;//유효한 프레임[물리 처리를 한다], 유효하지 않은 
			
			//삭제 20.7.9 : 타이머는 Portrait에서 공통으로 계산한다.
			//if (_stopwatch == null)
			//{
			//	_stopwatch = new System.Diagnostics.Stopwatch();
			//	_stopwatch.Start();
			//	_tDeltaFixed = 0.0f;
			//}

			//이전
			////tDelta를 별도로 받자
			//tDelta = (float)(_stopwatch.ElapsedMilliseconds / 1000.0f);

			//변경 20.7.9 : 물리 DeltaTime이 Portrait에 있다.
			tDelta = _portrait.PhysicsDeltaTime;

			_tDeltaFixed += tDelta;
			//tmpPhysics_tUpdateCall += tDelta;
			//tmpPhysics_nUpdateCall++;


			if (_tDeltaFixed > PHYSIC_DELTA_TIME)
			{
				tDelta = PHYSIC_DELTA_TIME;
				_tDeltaFixed -= PHYSIC_DELTA_TIME;
				isValidFrame = true;
			}
			else
			{
				tDelta = 0.0f;
				isValidFrame = false;
			}

			

			_cal_curCalParam = null;
			_cal_ResultPosList = null;
			//tmpPosList = null;//변경 21.5.16 : 이건 초기화하지 않는다.
			_cal_SubParamGroupList = null;
			_cal_SubParamKeyValueList = null;
			_cal_LayerWeight = 0.0f;
			_cal_KeyParamSetGroup = null;
			
			// 삭제 19.5.20 : 이 값을 사용하지 않음
			//apModifierParamSetGroupVertWeight weigetedVertData = null;

			_cal_CurSubList = null;
			_cal_NumParamKeys = 0;
			_cal_ParamKeyValue = null;

			//지역 변수를 여기서 일괄 선언하자
			_cal_Phy_modVertWeight = null;
			_cal_Phy_physicVertParam = null;
			_cal_Phy_physicMeshParam = null;
			_cal_Phy_Mass = 0.0f;

			_cal_Phy_F_gravity = Vector2.zero;
			_cal_Phy_F_wind = Vector2.zero;
			_cal_Phy_F_stretch = Vector2.zero;
			_cal_Phy_F_recover = Vector2.zero;

			_cal_Phy_F_ext = Vector2.zero;//<<추가된 "외부 힘"

			_cal_Phy_F_sum = Vector2.zero;
			//tmpPhysics_F_viscosity = Vector2.zero;


			tmpPhysics_linkedVert = null;
			_cal_Phy_isViscosity = false;

			_cal_Phy_srcVertPos_NoMod = Vector2.zero;
			_cal_Phy_linkVertPos_NoMod = Vector2.zero;
			_cal_Phy_srcVertPos_Cur = Vector2.zero;
			_cal_Phy_linkVertPos_Cur = Vector2.zero;
			_cal_Phy_deltaVec_0 = Vector2.zero;
			_cal_Phy_deltaVec_Cur = Vector2.zero;

			//bool isFirstDebug = true;

			//Profiler.BeginSample("Modifier : Physics");

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				_cal_curCalParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				//_cal_curCalParam.Calculate();//Physics는 필요없다.
				//-------------------------------------------------------


				//>>> LinkedMatrix를 초기화
				//calParam.CalculatedLog.ReadyToRecord();//<<<<<<



				_cal_ResultPosList = _cal_curCalParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;
				_cal_SubParamGroupList = _cal_curCalParam._subParamKeyValueList;
				_cal_SubParamKeyValueList = null;
				_cal_LayerWeight = 0.0f;
				_cal_KeyParamSetGroup = null;

				// 삭제 19.5.20 : 이 변수 삭제됨
				//weigetedVertData = calParam._weightedVertexData;

				//일단 초기화
				//기존
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//변경 21.5.15 : 배열 초기화 함수는 이걸로..
				_cal_NumVerts = _cal_ResultPosList.Length;
				System.Array.Clear(_cal_ResultPosList, 0, _cal_NumVerts);

				//추가 22.5.11 : 메인 데이터 초기화
				_cal_curCalParam._isMainCalculated = false;

				_cal_curCalParam._result_IsVisible = true;

				_cal_iCalculatedSubParam_Main = 0;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < _cal_SubParamGroupList.Count; iSubList++)
				{
					_cal_CurSubList = _cal_SubParamGroupList[iSubList];

					if (_cal_CurSubList._keyParamSetGroup == null ||
						!_cal_CurSubList._keyParamSetGroup.IsCalculateEnabled)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					_cal_NumParamKeys = _cal_CurSubList._subParamKeyValues.Count;//Sub Params
					_cal_SubParamKeyValueList = _cal_CurSubList._subParamKeyValues;



					_cal_ParamKeyValue = null;

					_cal_KeyParamSetGroup = _cal_CurSubList._keyParamSetGroup;



					
					_cal_isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화

					//변경 21.5.16
					if (_cal_TmpPosList == null ||
						_cal_TmpPosList.Length < _cal_NumVerts)
					{
						_cal_TmpPosList = new Vector2[_cal_NumVerts];
					}
					System.Array.Clear(_cal_TmpPosList, 0, _cal_NumVerts);

					//tmpPhysics_TotalWeight = 0.0f;
					_cal_NumCalculated = 0;


					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					for (int iPV = 0; iPV < _cal_NumParamKeys; iPV++)
					{
						_cal_ParamKeyValue = _cal_SubParamKeyValueList[iPV];


						if (!_cal_ParamKeyValue._isCalculated)
						{ continue; }

						//tmpPhysics_TotalWeight += _cal_ParamKeyValue._weight;

						//물리 계산 순서
						//Vertex 각각의 이전프레임으로 부터의 속력 계산
						
						if (_cal_NumVerts > 0 
							&& _portrait._isPhysicsPlay_Editor 
							&& _portrait._isPhysicsSupport_Editor//<<Portrait에서 지원하는 경우만
							)
						{
							_cal_Phy_modVertWeight = null;
							_cal_Phy_physicVertParam = null;
							_cal_Phy_physicMeshParam = _cal_ParamKeyValue._modifiedMesh.PhysicParam;
							_cal_Phy_Mass = _cal_Phy_physicMeshParam._mass;
							if (_cal_Phy_Mass < 0.001f)
							{
								_cal_Phy_Mass = 0.001f;
							}

							//Vertex에 상관없이 적용되는 힘
							// 중력, 바람
							//1) 중력 : mg
							_cal_Phy_F_gravity = _cal_Phy_Mass * _cal_Phy_physicMeshParam.GetGravityAcc();

							//2) 바람 : ma
							_cal_Phy_F_wind = _cal_Phy_Mass * _cal_Phy_physicMeshParam.GetWindAcc(tDelta);

							_cal_Phy_F_stretch = Vector2.zero;
							//F_airDrag = Vector2.zero;

							//F_inertia = Vector2.zero;
							_cal_Phy_F_recover = Vector2.zero;
							_cal_Phy_F_ext = Vector2.zero;
							_cal_Phy_F_sum = Vector2.zero;

							tmpPhysics_linkedVert = null;
							_cal_Phy_isViscosity = _cal_Phy_physicMeshParam._viscosity > 0.0f;



							//---------------------------- Pos List


							//< TODO : C++ DLL >

							for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
							{
								//여기서 물리 계산을 하자
								_cal_Phy_modVertWeight = _cal_ParamKeyValue._modifiedMesh._vertWeights[iPos];
								_cal_Phy_modVertWeight.UpdatePhysicVertex(tDelta, isValidFrame);//<<RenderVert의 위치와 속도를 계산한다.



								_cal_Phy_F_stretch = Vector2.zero;
								//F_airDrag = Vector2.zero;

								//F_inertia = Vector2.zero;
								_cal_Phy_F_recover = Vector2.zero;
								_cal_Phy_F_sum = Vector2.zero;


								if (!_cal_Phy_modVertWeight._isEnabled)
								{
									//처리 안함다
									_cal_Phy_modVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (_cal_Phy_modVertWeight._renderVertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								//최적화는 나중에 하고 일단 업데이트만이라도 하자

								_cal_Phy_physicVertParam = _cal_Phy_modVertWeight._physicParam;

								//이동 제한 범위를 초기화
								_cal_Phy_modVertWeight._isLimitPos = false;
								_cal_Phy_modVertWeight._limitScale = -1.0f;

								//추가
								//> 유효한 프레임 : 물리 계산을 한다.
								//> 생략하는 프레임 : 이전 속도를 그대로 이용한다.
								if (isValidFrame)
								{
									//1) 유효한 프레임이다.
									//Velocity_Next를 계산하자
									_cal_Phy_F_stretch = Vector2.zero;


									//Profiler.BeginSample("Physics - F-Stretch");

									//1) 장력 Strech : -k * (<delta Dist> * 기존 UnitVector)
									for (int iLinkVert = 0; iLinkVert < _cal_Phy_physicVertParam._linkedVertices.Count; iLinkVert++)
									{
										tmpPhysics_linkedVert = _cal_Phy_physicVertParam._linkedVertices[iLinkVert];
										float linkWeight = tmpPhysics_linkedVert._distWeight;

										_cal_Phy_srcVertPos_NoMod = _cal_Phy_modVertWeight._pos_World_NoMod;
										_cal_Phy_linkVertPos_NoMod = tmpPhysics_linkedVert._modVertWeight._pos_World_NoMod;
										tmpPhysics_linkedVert._deltaPosToTarget_NoMod = _cal_Phy_srcVertPos_NoMod - _cal_Phy_linkVertPos_NoMod;


										_cal_Phy_srcVertPos_Cur = _cal_Phy_modVertWeight._pos_Real;
										_cal_Phy_linkVertPos_Cur = tmpPhysics_linkedVert._modVertWeight._pos_Real;

										_cal_Phy_deltaVec_0 = _cal_Phy_srcVertPos_NoMod - _cal_Phy_linkVertPos_NoMod;
										_cal_Phy_deltaVec_Cur = _cal_Phy_srcVertPos_Cur - _cal_Phy_linkVertPos_Cur;


										//F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur - deltaVec_0) * linkWeight;//<<기존
										//F_stretch += -1.0f * physicMeshParam._stretchK * (deltaVec_Cur - deltaVec_0);
										//totalStretchWeight += linkWeight;

										//길이 차이로 힘을 만들고
										//방향은 현재 Delta

										//<추가> 만약 장력 벡터가 완전히 뒤집힌 경우
										//면이 뒤집혔다.
										if (Vector2.Dot(_cal_Phy_deltaVec_0, _cal_Phy_deltaVec_Cur) < 0)
										{
											_cal_Phy_F_stretch += _cal_Phy_physicMeshParam._stretchK * (_cal_Phy_deltaVec_0 - _cal_Phy_deltaVec_Cur) * linkWeight;
										}
										else
										{
											_cal_Phy_F_stretch += -1.0f * _cal_Phy_physicMeshParam._stretchK * (_cal_Phy_deltaVec_Cur.magnitude - _cal_Phy_deltaVec_0.magnitude) * _cal_Phy_deltaVec_Cur.normalized * linkWeight;
										}
										
									}



									//5) 복원력
									_cal_Phy_F_recover = -1.0f * _cal_Phy_physicMeshParam._restoring * _cal_Phy_modVertWeight._calculatedDeltaPos;

									//6) 추가 : 외부 힘
									//이전 프레임에서의 힘을 이용한다.
									_cal_Phy_F_ext = _portrait.GetForce(_cal_Phy_modVertWeight._pos_1F);

									float inertiaK = Mathf.Clamp01(_cal_Phy_physicMeshParam._inertiaK);
									
									

									//5) 힘의 합력을 구한다.
									if (_cal_Phy_modVertWeight._physicParam._isMain)
									{
										//F_sum = F_gravity + F_wind + F_stretch + F_airDrag + F_recover + F_ext;//관성 제외
										_cal_Phy_F_sum = _cal_Phy_F_gravity + _cal_Phy_F_wind + _cal_Phy_F_stretch + _cal_Phy_F_recover + _cal_Phy_F_ext;//관성 제외 + 공기 저항도 제외
									}
									else
									{
										//F_sum = F_gravity + F_wind + F_stretch + ((F_airDrag + F_recover + F_ext) * 0.5f);//관성 제외
										_cal_Phy_F_sum = _cal_Phy_F_gravity + _cal_Phy_F_wind + _cal_Phy_F_stretch + ((_cal_Phy_F_recover + _cal_Phy_F_ext) * 0.5f);//관성 제외 + 공기 저항도 제외 //<<
										

										inertiaK *= 0.5f;//<<관성 감소
									}


									
									_cal_Phy_modVertWeight._velocity_Next = 
										//(modVertWeight._velocity_Real * inertiaK + modVertWeight._velocity_1F * (1.0f - inertiaK))
										_cal_Phy_modVertWeight._velocity_1F
										+ (_cal_Phy_modVertWeight._velocity_1F - _cal_Phy_modVertWeight._velocity_Real) * inertiaK
										+ (_cal_Phy_F_sum / _cal_Phy_Mass) * tDelta
										;

									//Air Drag식 수정
									if(_cal_Phy_physicMeshParam._airDrag > 0.0f)
									{
										_cal_Phy_modVertWeight._velocity_Next *= Mathf.Clamp01((1.0f - (_cal_Phy_physicMeshParam._airDrag * tDelta) / (_cal_Phy_Mass + 0.5f)));
									}

								}
								else
								{
									_cal_Phy_modVertWeight._velocity_Next = _cal_Phy_modVertWeight._velocity_1F;
								}

								//변경.
								//여기서 일단 속력을 미리 적용하자
								if (isValidFrame)
								{
									Vector2 nextVelocity = _cal_Phy_modVertWeight._velocity_Next;

									//V += at
									//마음대로 증가하지 않도록 한다.
									Vector2 limitedNextCalPos = _cal_Phy_modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

									//이동 제한이 걸려있다면
									if (_cal_Phy_physicMeshParam._isRestrictMoveRange)
									{
										//Profiler.BeginSample("Physics - Move Range");

										float radiusFree = _cal_Phy_physicMeshParam._moveRange * 0.5f;
										float radiusMax = _cal_Phy_physicMeshParam._moveRange;

										if (radiusMax <= radiusFree)
										{
											nextVelocity *= 0.0f;
											//둘다 0이라면 아예 이동이 불가
											if (!_cal_Phy_modVertWeight._isLimitPos)
											{
												_cal_Phy_modVertWeight._isLimitPos = true;
												_cal_Phy_modVertWeight._limitScale = 0.0f;
											}
										}
										else
										{
											float curDeltaPosSize = (limitedNextCalPos).magnitude;

											if (curDeltaPosSize < radiusFree)
											{
												//별일 없슴다
											}
											else
											{
												//기본은 선형의 사이즈이지만,
												//돌아가는 힘은 유지해야한다.
												//[deltaPos unitVector dot newVelocity] = 1일때 : 바깥으로 나가려는 힘
												// = -1일때 : 안으로 들어오려는 힘
												// -1 ~ 1 => 0 ~ 1 : 0이면 moveRatio가 1, 1이면 moveRatio가 거리에 따라 1>0
												float dotVector = Vector2.Dot(_cal_Phy_modVertWeight._calculatedDeltaPos.normalized, nextVelocity.normalized);
												dotVector = (dotVector * 0.5f) + 0.5f; //0: 속도 느려짐 없음 (안쪽으로 들어가려고 함), 1:증가하는 방향

												float outerItp = Mathf.Clamp01((curDeltaPosSize - radiusFree) / (radiusMax - radiusFree));//0 : 속도 느려짐 없음, 1:속도 0

												nextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자
																											 //limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

												if (curDeltaPosSize > radiusMax)
												{
													//limitedNextCalPos = modVertWeight._calculatedDeltaPos.normalized * radiusMax;
													if (!_cal_Phy_modVertWeight._isLimitPos || radiusMax < _cal_Phy_modVertWeight._limitScale)
													{
														_cal_Phy_modVertWeight._isLimitPos = true;
														_cal_Phy_modVertWeight._limitScale = radiusMax;
													}
												}
											}
										}

										//Profiler.EndSample();
									}

									//장력에 의한 길이 제한도 처리한다.
									if (_cal_Phy_physicMeshParam._isRestrictStretchRange)
									{

										//Profiler.BeginSample("Physics - Stretch Range");

										bool isLimitVelocity2Max = false;
										Vector2 stretchLimitPos = Vector2.zero;
										float limitCalPosDist = 0.0f;
										for (int iLinkVert = 0; iLinkVert < _cal_Phy_physicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpPhysics_linkedVert = _cal_Phy_physicVertParam._linkedVertices[iLinkVert];

											//길이의 Min/Max가 있다.
											float distStretchBase = tmpPhysics_linkedVert._deltaPosToTarget_NoMod.magnitude;

											float stretchRangeMax = (_cal_Phy_physicMeshParam._stretchRangeRatio_Max) * distStretchBase;
											float stretchRangeMax_Half = (_cal_Phy_physicMeshParam._stretchRangeRatio_Max * 0.5f) * distStretchBase;

											Vector2 curDeltaFromLinkVert = limitedNextCalPos - tmpPhysics_linkedVert._modVertWeight._calculatedDeltaPos_Prev;
											float curDistFromLinkVert = curDeltaFromLinkVert.magnitude;

											//너무 멀면 제한한다.
											//단, 제한 권장은 Weight에 맞게

											//float weight = Mathf.Clamp01(linkedVert._distWeight);
											isLimitVelocity2Max = false;

											if (curDistFromLinkVert > stretchRangeMax_Half)
											{
												isLimitVelocity2Max = true;//늘어나는 한계점으로 이동하는 중
												stretchLimitPos = tmpPhysics_linkedVert._modVertWeight._calculatedDeltaPos_Prev + curDeltaFromLinkVert.normalized * stretchRangeMax;

												if (curDistFromLinkVert > stretchRangeMax)
												{
													limitCalPosDist = (stretchLimitPos).magnitude;
												}
											}

											if (isLimitVelocity2Max)
											{
												//LinkVert간의 벡터를 기준으로 nextVelocity가 확대/축소하는 방향이라면 그 반대의 값을 넣는다.
												float dotVector = Vector2.Dot(curDeltaFromLinkVert.normalized, nextVelocity.normalized);
												//-1 : 축소하려는 방향으로 이동하는 중
												//1 : 확대하려는 방향으로 이동하는 중


												float outerItp = 0.0f;
												if (isLimitVelocity2Max)
												{
													//너무 바깥으로 이동하려고 할때, 속도를 줄인다.
													dotVector = Mathf.Clamp01(dotVector);
													if (stretchRangeMax > stretchRangeMax_Half)
													{
														outerItp = Mathf.Clamp01((curDistFromLinkVert - stretchRangeMax_Half) / (stretchRangeMax - stretchRangeMax_Half));
													}
													else
													{
														outerItp = 1.0f;//무조건 속도 0

														if (!_cal_Phy_modVertWeight._isLimitPos || limitCalPosDist < _cal_Phy_modVertWeight._limitScale)
														{
															_cal_Phy_modVertWeight._isLimitPos = true;
															_cal_Phy_modVertWeight._limitScale = limitCalPosDist;
														}
													}

												}

												nextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자

											}
										}
										//nextVelocity *= velRatio;

										//Profiler.EndSample();

										//limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);
									}

									limitedNextCalPos = _cal_Phy_modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);

									_cal_Phy_modVertWeight._calculatedDeltaPos_Prev = _cal_Phy_modVertWeight._calculatedDeltaPos;
									_cal_Phy_modVertWeight._calculatedDeltaPos = limitedNextCalPos;
								}
							}

							//1차로 계산된 값을 이용하여 점성력을 체크한다.
							//수정 : 이미 위치는 계산되었다. 위치를 중심으로 처리를 하자 점성/이동한계를 계산하자
							for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
							{
								_cal_Phy_modVertWeight = _cal_ParamKeyValue._modifiedMesh._vertWeights[iPos];
								_cal_Phy_physicVertParam = _cal_Phy_modVertWeight._physicParam;

								if (!_cal_Phy_modVertWeight._isEnabled)
								{
									//처리 안함다
									_cal_Phy_modVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (_cal_Phy_modVertWeight._renderVertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								if (isValidFrame)
								{
									Vector2 nextVelocity = _cal_Phy_modVertWeight._velocity_Next;
									Vector2 nextCalPos = _cal_Phy_modVertWeight._calculatedDeltaPos;

									//[점성도]를 계산한다.
									if (_cal_Phy_isViscosity && !_cal_Phy_modVertWeight._physicParam._isMain)
									{
										//Profiler.BeginSample("Physics - Viscosity");

										//ID가 같으면 DeltaPos가 비슷해야한다.
										float linkedViscosityWeight = 0.0f;
										//Vector2 linkedViscosityNextVelocity = Vector2.zero;
										Vector2 linkedTotalCalPos = Vector2.zero;

										int curViscosityID = _cal_Phy_modVertWeight._physicParam._viscosityGroupID;

										for (int iLinkVert = 0; iLinkVert < _cal_Phy_physicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpPhysics_linkedVert = _cal_Phy_physicVertParam._linkedVertices[iLinkVert];
											float linkWeight = tmpPhysics_linkedVert._distWeight;

											if ((tmpPhysics_linkedVert._modVertWeight._physicParam._viscosityGroupID & curViscosityID) != 0)
											{
												//float subWeight = 1.0f;
												//if(!linkedVert._modVertWeight._physicParam._isMain)
												//{
												//	//subWeight *= 0.3f;
												//}
												//linkedViscosityNextVelocity += linkedVert._modVertWeight._velocity_Next * linkWeight * subWeight;//사실 Vertex의 호출 순서에 따라 값이 좀 다르다.
												linkedTotalCalPos += tmpPhysics_linkedVert._modVertWeight._calculatedDeltaPos * linkWeight;
												linkedViscosityWeight += linkWeight;
											}
										}

										//점성도를 추가한다.
										if (linkedViscosityWeight > 0.0f)
										{
											//linkedViscosityNextVelocity /= linkedViscosityWeight;
											float clampViscosity = Mathf.Clamp01(_cal_Phy_physicMeshParam._viscosity) * 0.7f;

											nextCalPos = nextCalPos * (1.0f - clampViscosity) + linkedTotalCalPos * clampViscosity;
										}


									}


									//이동 한계 한번 더 계산
									if (_cal_Phy_modVertWeight._isLimitPos && nextCalPos.magnitude > _cal_Phy_modVertWeight._limitScale)
									{
										nextCalPos = nextCalPos.normalized * _cal_Phy_modVertWeight._limitScale;
									}


									_cal_Phy_modVertWeight._calculatedDeltaPos = nextCalPos;



									//속도를 다시 계산해주자
									nextVelocity = (_cal_Phy_modVertWeight._calculatedDeltaPos - _cal_Phy_modVertWeight._calculatedDeltaPos_Prev) / tDelta;

									//-----------------------------------------------------------------------------------------
									// 속도 갱신
									_cal_Phy_modVertWeight._velocity_Next = nextVelocity;

									//modVertWeight._velocity_1F = nextVelocity;//이전 코드
									//속도 차이가 크다면 Real의 비중이 커야 한다.
									//같은 방향이면 -> 버티기 관성이 더 잘보이는게 좋다
									//다른 방향이면 Real을 관성으로 사용해야한다. (그래야 다음 프레임에 관성이 크게 보임)
									//속도 변화에 따라서 체크
									float velocityRefreshITP_X = Mathf.Clamp01(Mathf.Abs( ((_cal_Phy_modVertWeight._velocity_Real.x - _cal_Phy_modVertWeight._velocity_Real1F.x) / (Mathf.Abs(_cal_Phy_modVertWeight._velocity_Real1F.x) + 0.1f)) * 0.5f ) );
									float velocityRefreshITP_Y = Mathf.Clamp01(Mathf.Abs( ((_cal_Phy_modVertWeight._velocity_Real.y - _cal_Phy_modVertWeight._velocity_Real1F.y) / (Mathf.Abs(_cal_Phy_modVertWeight._velocity_Real1F.y) + 0.1f)) * 0.5f ) );

									_cal_Phy_modVertWeight._velocity_1F.x = nextVelocity.x * (1.0f - velocityRefreshITP_X) + (nextVelocity.x * 0.5f + _cal_Phy_modVertWeight._velocity_Real.x * 0.5f) * velocityRefreshITP_X;
									_cal_Phy_modVertWeight._velocity_1F.y = nextVelocity.y * (1.0f - velocityRefreshITP_Y) + (nextVelocity.y * 0.5f + _cal_Phy_modVertWeight._velocity_Real.y * 0.5f) * velocityRefreshITP_Y;


									_cal_Phy_modVertWeight._pos_1F = _cal_Phy_modVertWeight._pos_Real;


									//Damping
									if ((_cal_Phy_modVertWeight._calculatedDeltaPos.sqrMagnitude < _cal_Phy_physicMeshParam._damping * _cal_Phy_physicMeshParam._damping
										&& nextVelocity.sqrMagnitude < _cal_Phy_physicMeshParam._damping * _cal_Phy_physicMeshParam._damping)
										|| !_cal_Phy_modVertWeight._isPhysicsCalculatedPrevFrame)
									{
										_cal_Phy_modVertWeight._calculatedDeltaPos = Vector2.zero;
										_cal_Phy_modVertWeight.DampPhysicVertex();

										_cal_Phy_modVertWeight._isPhysicsCalculatedPrevFrame = true;
									}

								}



								_cal_TmpPosList[iPos] +=
										(_cal_Phy_modVertWeight._calculatedDeltaPos * _cal_Phy_modVertWeight._weight)
										* _cal_ParamKeyValue._weight;//<<이 값을 이용한다.




							}
							//---------------------------- Pos List
						}



						if (_cal_isFirstParam)
						{
							_cal_isFirstParam = false;
						}


						_cal_NumCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params



					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					_cal_LayerWeight = Mathf.Clamp01(_cal_KeyParamSetGroup._layerWeight);


					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.
					_cal_curCalParam._totalParamSetGroupWeight_Transform += _cal_LayerWeight;

					if (_cal_iCalculatedSubParam_Main == 0)//<<변경
					{
						//< C++ DLL >
						//C# 코드
						//for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
						//{
						//	_cal_ResultPosList[iPos] = _cal_TmpPosList[iPos] * _cal_LayerWeight;
						//}

						//C++ DLL 코드
						Modifier_SetWeightedPosList(ref _cal_ResultPosList, ref _cal_TmpPosList, _cal_NumVerts, _cal_LayerWeight);
					}
					else
					{
						switch (_cal_KeyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//< C++ DLL >
									//C# 코드
									//for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
									//{
									//	_cal_ResultPosList[iPos] += _cal_TmpPosList[iPos] * _cal_LayerWeight;
									//}

									//C++ DLL 코드
									Modifier_AddWeightedPosList(ref _cal_ResultPosList, ref _cal_TmpPosList, _cal_NumVerts, _cal_LayerWeight);
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//< C++ DLL >
									//C# 코드
									//for (int iPos = 0; iPos < _cal_NumVerts; iPos++)
									//{
									//	_cal_ResultPosList[iPos] = (_cal_ResultPosList[iPos] * (1.0f - _cal_LayerWeight)) +
									//					(_cal_TmpPosList[iPos] * _cal_LayerWeight);
									//}

									//C++ DLL 코드
									Modifier_InterpolateWeightedPosList(ref _cal_ResultPosList, ref _cal_TmpPosList, _cal_NumVerts, _cal_LayerWeight);
								}
								break;

							default:
								UnityEngine.Debug.LogError("Mod-Physics : Unknown BLEND_METHOD : " + _cal_KeyParamSetGroup._blendMethod);
								break;
						}

					}

					_cal_iCalculatedSubParam_Main += 1;
					_cal_curCalParam._isMainCalculated = true;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				//_cal_curCalParam._isAvailable = true;//삭제 22.5.11


			}

			//Profiler.EndSample();
		}


		// 특수한 경우의 함수
		//---------------------------------------------------------------------------------------
		/// <summary>
		/// 추가 22.6.11 [v1.4.0] 물리 처리 시간을 리셋한다.
		/// </summary>
		public void ResetPhysicsTime()
		{
			_tDeltaFixed = 0.0f;
		}
	}

}