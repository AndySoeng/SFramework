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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	[Serializable]
	public class apModifierStack
	{
		// Members
		//----------------------------------------------------
		//저장되는 Modifier들
		//Serialize는 다형성 저장이 안되어서 타입별로 따로 만들고, 실행중에 부모 클래스 리스트에 합친다.
		[SerializeField]
		private List<apModifier_Volume> _modifiers_Volume = new List<apModifier_Volume>();

		[SerializeField]
		private List<apModifier_Morph> _modifiers_Morph = new List<apModifier_Morph>();

		[SerializeField]
		private List<apModifier_AnimatedMorph> _modifiers_AnimatedMorph = new List<apModifier_AnimatedMorph>();

		[SerializeField]
		private List<apModifier_Rigging> _modifiers_Rigging = new List<apModifier_Rigging>();

		[SerializeField]
		private List<apModifier_Physic> _modifiers_Physic = new List<apModifier_Physic>();

		[SerializeField]
		private List<apModifier_TF> _modifiers_TF = new List<apModifier_TF>();

		[SerializeField]
		private List<apModifier_AnimatedTF> _modifiers_AnimatedTF = new List<apModifier_AnimatedTF>();

		[SerializeField]
		private List<apModifier_FFD> _modifiers_FFD = new List<apModifier_FFD>();

		[SerializeField]
		private List<apModifier_AnimatedFFD> _modifiers_AnimatedFFD = new List<apModifier_AnimatedFFD>();

		//추가 21.7.20 : 색상만 다루는 모디파이어 추가
		[SerializeField]
		private List<apModifier_ColorOnly> _modifiers_ColorOnly = new List<apModifier_ColorOnly>();

		[SerializeField]
		private List<apModifier_AnimatedColorOnly> _modifiers_AnimatedColorOnly = new List<apModifier_AnimatedColorOnly>();


		//실제로 작동하는 Modifier 리스트 (Layer 순서에 맞게 Sort)
		[NonSerialized]
		public List<apModifierBase> _modifiers = new List<apModifierBase>();

		[NonSerialized]
		public apPortrait _parentPortrait = null;

		[NonSerialized]
		public apMeshGroup _parentMeshGroup = null;

		[NonSerialized]
		private bool _isSorted = false;


		// Init
		//----------------------------------------------------
		public apModifierStack()
		{

		}


		// Validate
		//-------------------------------------------------------
		/// <summary>
		/// 유효하지 않은 모디파이어들을 소스 리스트에서 삭제하고, 삭제된 개수를 리턴합니다.
		/// </summary>
		/// <returns></returns>
		public int RemoveInvalidModifiers()
		{
			int nRemoved = 0;
			if (_modifiers_Volume != null)
			{
				int curRmv = _modifiers_Volume.RemoveAll(delegate (apModifier_Volume a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_Morph != null)
			{
				int curRmv = _modifiers_Morph.RemoveAll(delegate (apModifier_Morph a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_AnimatedMorph != null)
			{
				int curRmv = _modifiers_AnimatedMorph.RemoveAll(delegate (apModifier_AnimatedMorph a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_Rigging != null)
			{
				int curRmv = _modifiers_Rigging.RemoveAll(delegate (apModifier_Rigging a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_Physic != null)
			{
				int curRmv = _modifiers_Physic.RemoveAll(delegate (apModifier_Physic a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_TF != null)
			{
				int curRmv = _modifiers_TF.RemoveAll(delegate (apModifier_TF a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_AnimatedTF != null)
			{
				int curRmv = _modifiers_AnimatedTF.RemoveAll(delegate (apModifier_AnimatedTF a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_FFD != null)
			{
				int curRmv = _modifiers_FFD.RemoveAll(delegate (apModifier_FFD a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_AnimatedFFD != null)
			{
				int curRmv = _modifiers_AnimatedFFD.RemoveAll(delegate (apModifier_AnimatedFFD a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_ColorOnly != null)
			{
				int curRmv = _modifiers_ColorOnly.RemoveAll(delegate (apModifier_ColorOnly a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			if (_modifiers_AnimatedColorOnly != null)
			{
				int curRmv = _modifiers_AnimatedColorOnly.RemoveAll(delegate (apModifier_AnimatedColorOnly a) { return a == null; });
				nRemoved += curRmv > 0 ? curRmv : 0;
			}

			//만약, Runtime용 변수인 _modifiers에도 Null이 있다면 일단 여기서 없애자 (RefreshAndSort 함수가 더 적절하다)
			if (_modifiers != null)
			{
				_modifiers.RemoveAll(delegate(apModifierBase a)
				{
					return a == null;
				});
			}

			//if(nRemoved > 0)
			//{
			//	Debug.Log("Null 모디파이어 발견 (Invalid 체크에서)");
			//}

			return nRemoved;
		}




		public enum REFRESH_OPTION_ACTIVE
		{
			/// <summary>이 함수를 호출함과 함께 가능한 모디파이어를 Active한다. (기존의 true에 해당)</summary>
			ActiveAllModifierIfPossible,
			/// <summary>Active 여부를 그대로 둔다. (기존의 false에 해당)</summary>
			Keep,
		}

		public enum REFRESH_OPTION_REMOVE
		{
			/// <summary>Null인 모디파이어를 삭제한다. (Undo 체크 필요)</summary>
			RemoveNullModifiers,
			/// <summary>Null인 데이터를 그대로 둔다. (기본값)</summary>
			Ignore,
		}


		//public void RefreshAndSort(bool isSetActiveAllModifier)
		//변경 22.12.13 : 옵션이 하나 더 추가됨
		//(1) 함수 호출후 가능한 모든 모디파이어를 Active로 만들지 여부 (기존 bool 인자)
		//(2) Null 상태의 모디파이어를 소스 리스트에서 완전히 삭제할지 여부
		public void RefreshAndSort(REFRESH_OPTION_ACTIVE activeModOption, REFRESH_OPTION_REMOVE removeNullOption)
		{
			_modifiers.Clear();

			apModifierBase curMod = null;

			bool isAnyNullMod = false;//Null 데이터가 있는지 감지하자

			if (_modifiers_Volume != null)
			{
				for (int i = 0; i < _modifiers_Volume.Count; i++)
				{
					curMod = _modifiers_Volume[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_Morph != null)
			{
				for (int i = 0; i < _modifiers_Morph.Count; i++)
				{
					curMod = _modifiers_Morph[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_AnimatedMorph != null)
			{
				for (int i = 0; i < _modifiers_AnimatedMorph.Count; i++)
				{
					curMod = _modifiers_AnimatedMorph[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_Rigging != null)
			{
				for (int i = 0; i < _modifiers_Rigging.Count; i++)
				{
					curMod = _modifiers_Rigging[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_Physic != null)
			{
				for (int i = 0; i < _modifiers_Physic.Count; i++)
				{
					curMod = _modifiers_Physic[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_TF != null)
			{
				for (int i = 0; i < _modifiers_TF.Count; i++)
				{
					curMod = _modifiers_TF[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_AnimatedTF != null)
			{
				for (int i = 0; i < _modifiers_AnimatedTF.Count; i++)
				{
					curMod = _modifiers_AnimatedTF[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_FFD != null)
			{
				for (int i = 0; i < _modifiers_FFD.Count; i++)
				{
					curMod = _modifiers_FFD[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_AnimatedFFD != null)
			{
				for (int i = 0; i < _modifiers_AnimatedFFD.Count; i++)
				{
					curMod = _modifiers_AnimatedFFD[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}


			//추가 21.7.20 : 색상 전용 모디파이어
			if (_modifiers_ColorOnly != null)
			{
				for (int i = 0; i < _modifiers_ColorOnly.Count; i++)
				{
					curMod = _modifiers_ColorOnly[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}

			if (_modifiers_AnimatedColorOnly != null)
			{
				for (int i = 0; i < _modifiers_AnimatedColorOnly.Count; i++)
				{
					curMod = _modifiers_AnimatedColorOnly[i];
					if(curMod == null)
					{
						isAnyNullMod = true;
						continue;
					}
					_modifiers.Add(curMod);
				}
			}
			

			//Null 데이터가 발견되었다면 옵션에 따라 삭제하자
			if(isAnyNullMod 
				&& removeNullOption == REFRESH_OPTION_REMOVE.RemoveNullModifiers)
			{	
				if(_modifiers_Volume != null)				{ _modifiers_Volume.RemoveAll(delegate(apModifier_Volume a) { return a == null; }); }
				if(_modifiers_Morph != null)				{ _modifiers_Morph.RemoveAll(delegate(apModifier_Morph a) { return a == null; }); }
				if(_modifiers_AnimatedMorph != null)		{ _modifiers_AnimatedMorph.RemoveAll(delegate(apModifier_AnimatedMorph a) { return a == null; }); }
				if(_modifiers_Rigging != null)				{ _modifiers_Rigging.RemoveAll(delegate(apModifier_Rigging a) { return a == null; }); }
				if(_modifiers_Physic != null)				{ _modifiers_Physic.RemoveAll(delegate(apModifier_Physic a) { return a == null; }); }
				if(_modifiers_TF != null)					{ _modifiers_TF.RemoveAll(delegate(apModifier_TF a) { return a == null; }); }
				if(_modifiers_AnimatedTF != null)			{ _modifiers_AnimatedTF.RemoveAll(delegate(apModifier_AnimatedTF a) { return a == null; }); }
				if(_modifiers_FFD != null)					{ _modifiers_FFD.RemoveAll(delegate(apModifier_FFD a) { return a == null; }); }
				if(_modifiers_AnimatedFFD != null)			{ _modifiers_AnimatedFFD.RemoveAll(delegate(apModifier_AnimatedFFD a) { return a == null; }); }
				if(_modifiers_ColorOnly != null)			{ _modifiers_ColorOnly.RemoveAll(delegate(apModifier_ColorOnly a) { return a == null; }); }
				if(_modifiers_AnimatedColorOnly != null)	{ _modifiers_AnimatedColorOnly.RemoveAll(delegate(apModifier_AnimatedColorOnly a) { return a == null; }); }
			}


			_modifiers.Sort(delegate (apModifierBase a, apModifierBase b)
			{
				return (a._layer * 10) - (b._layer * 10);
			});

			for (int i = 0; i < _modifiers.Count; i++)
			{
				_modifiers[i]._layer = i;
			}

			_isSorted = true;

			//if (isSetActiveAllModifier)
			if(activeModOption == REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible)
			{
				ActiveAllModifierFromExclusiveEditing();
			}
		}



		// Functions - Validate
		//----------------------------------------------------
		/// <summary>
		/// 모디파이어가 이 모디파이어스택에 포함되어 있는지 확인하는 유효성 검사용 함수.
		/// 모디파이어 삭제/복구 과정에서 제대로 연결되었는지 체크한다.
		/// </summary>
		public bool IsContain(apModifierBase modifier)
		{
			if(modifier == null)
			{
				return false;
			}
			switch (modifier.ModifierType)
			{
				case apModifierBase.MODIFIER_TYPE.Volume:
					if(_modifiers_Volume != null && modifier is apModifier_Volume)
					{
						return _modifiers_Volume.Contains(modifier as apModifier_Volume);
					}
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
				case apModifierBase.MODIFIER_TYPE.Rigging:
				case apModifierBase.MODIFIER_TYPE.Physic:
				case apModifierBase.MODIFIER_TYPE.TF:
				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
				case apModifierBase.MODIFIER_TYPE.FFD:
				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					break;
			}
			return false;
		}




		// Functions - Update
		//----------------------------------------------------
		public void Update_Pre(float tDelta)
		{
			if (_modifiers.Count == 0 && !_isSorted)
			{
				//RefreshAndSort(false);
				RefreshAndSort(REFRESH_OPTION_ACTIVE.Keep, REFRESH_OPTION_REMOVE.Ignore);//변경 22.12.13
			}

			//Profiler.BeginSample("Modifier Calculate");
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (!_modifiers[i].IsPreUpdate)
				{
					//Post-Update라면 패스
					continue;
				}
				if (_modifiers[i]._isActive
#if UNITY_EDITOR
					//&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled//<<이건 에디터에서만 작동한다.
					&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force//변경 21.2.14 : 편집 모드에 의한 값 헤분화
#endif
				)

				{
					_modifiers[i].Calculate(tDelta);
				}
				else
				{
					//Debug.LogError("Not Update Mod Stack : " + _modifiers[i].DisplayName + " / " + _parentMeshGroup._name);
					_modifiers[i].InitCalculate(tDelta);
				}
			}

			//Profiler.EndSample();
		}



		/// <summary>
		/// 추가 21.5.14 : C++ DLL을 이용하여 업데이트를 한다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update_Pre_DLL(float tDelta)
		{
			if (_modifiers.Count == 0 && !_isSorted)
			{
				//RefreshAndSort(false);
				RefreshAndSort(REFRESH_OPTION_ACTIVE.Keep, REFRESH_OPTION_REMOVE.Ignore);//변경 22.12.13
			}

			//Profiler.BeginSample("Modifier Calculate");
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (!_modifiers[i].IsPreUpdate)
				{
					//Post-Update라면 패스
					continue;
				}
				if (_modifiers[i]._isActive
#if UNITY_EDITOR
					//&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled//<<이건 에디터에서만 작동한다.
					&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force//변경 21.2.14 : 편집 모드에 의한 값 헤분화
#endif
				)
				{
					//_modifiers[i].Calculate(tDelta);//기본
					_modifiers[i].Calculate_DLL(tDelta);//C++ DLL
				}
				else
				{
					//Debug.LogError("Not Update Mod Stack : " + _modifiers[i].DisplayName + " / " + _parentMeshGroup._name);
					_modifiers[i].InitCalculate(tDelta);
				}
			}

			//Profiler.EndSample();
		}





		public void Update_Post(float tDelta)
		{
			//Profiler.BeginSample("Modifier Calculate - Post");
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (_modifiers[i].IsPreUpdate)
				{
					//Pre-Update라면 패스
					continue;
				}
				if (_modifiers[i]._isActive
#if UNITY_EDITOR
				//&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled//<<이건 에디터에서만 작동한다.
				&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
#endif
				)

				{
					_modifiers[i].Calculate(tDelta);
				}
				else
				{
					//Debug.Log("Not Update Mod Stack : " + _modifiers[i].DisplayName + " / " + _parentMeshGroup._name);
					_modifiers[i].InitCalculate(tDelta);
				}
			}

			//Profiler.EndSample();
		}


		/// <summary>
		/// 추가 21.5.14 : C++ DLL을 이용하여 업데이트를 한다.
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update_Post_DLL(float tDelta)
		{
			//Profiler.BeginSample("Modifier Calculate - Post");
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (_modifiers[i].IsPreUpdate)
				{
					//Pre-Update라면 패스
					continue;
				}
				if (_modifiers[i]._isActive
#if UNITY_EDITOR
				//&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled//<<이건 에디터에서만 작동한다.
				&& _modifiers[i]._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
#endif
				)
				{
					//_modifiers[i].Calculate(tDelta);//기본
					_modifiers[i].Calculate_DLL(tDelta);//DLL을 이용하여 업데이트
				}
				else
				{
					//Debug.Log("Not Update Mod Stack : " + _modifiers[i].DisplayName + " / " + _parentMeshGroup._name);
					_modifiers[i].InitCalculate(tDelta);
				}
			}

			//Profiler.EndSample();
		}



		// 에디터 관련 코드
		//----------------------------------------------------
		public void ActiveAllModifierFromExclusiveEditing()
		{
			apModifierBase modifier = null;
			for (int i = 0; i < _modifiers.Count; i++)
			{
				modifier = _modifiers[i];
				//modifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled;//이전
				modifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run;//변경

				for (int iP = 0; iP < modifier._paramSetGroup_controller.Count; iP++)
				{
					//modifier._paramSetGroup_controller[iP]._isEnabledExclusive = true;//이전

					//변경>>
					modifier._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
					modifier._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
				}
				
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						}
					}
				}
			}
		}

		/// <summary>
		/// 모든 모디파이어를 강제로 비활성화 한다. 편집용은 아니다.
		/// </summary>
		public void SetDisableForceAllModifier()
		{
			apModifierBase curModifier = null;
			apModifierParamSetGroup curParamSetGroup = null;

			for (int i = 0; i < _modifiers.Count; i++)
			{
				curModifier = _modifiers[i];

				curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force;

				for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
				{
					curParamSetGroup = curModifier._paramSetGroup_controller[iP];
					curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
					curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
				}
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							meshGroupTransform._meshGroup._modifierStack.SetDisableForceAllModifier();
						}
					}
				}
			}
		}

		/// <summary>
		/// 선택되지 않는 다른 모디파이어/PSG가 어떻게 동작할 것인지 옵션 (함수 인자용)
		/// </summary>
		public enum OTHER_MOD_RUN_OPTION
		{
			/// <summary>선택되지 않는 모디파이어/PSG는 비활성화한다. (Background 설정 제외)</summary>
			Disabled,
			/// <summary>선택되지 않은 모디파이어/PSG는 비활성화하되, 미리보기 용으로 색상만 동작한다.</summary>
			ActiveColorOnly,
			/// <summary>충돌되지 않은 모든 경우에 대해서 선택되지 않은 모든 모디파이어/PSG가 동작한다.</summary>
			ActiveAllPossible
		}

		/// <summary>
		/// [선택한 Modifier]와 [선택한 ParamSetGroup]만 활성화하고 나머지는 비활성한다.
		/// 한개의 ParamSetGroup만 활성화하므로 "한개의 ControlParam만 작업할 때" 호출된다.
		/// </summary>
		/// <param name="targetModifier"></param>
		/// <param name="targetParamSetGroup"></param>
		public void SetExclusiveModifierInEditing(	apModifierBase targetModifier, apModifierParamSetGroup targetParamSetGroup, 

													//이전
													//bool isAllowColorCalculated, 
													//bool isEnablePSGEvenIfDisabledModifier//추가 21.2.17 : 이게 True라면 선택되지 않은 모디파이어의 PSG도 일단 활성화

													OTHER_MOD_RUN_OPTION multipleModType//변경 22.5.13. bool 변수는 너무 어렵다.
													)
		{
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;

			//추가 : isColorCalculated가 추가되었다.
			//isColorCalculated라면 Exclusive여서 처리가 안되는 경우라도 무조건 Color 계산은 할 수 있다.

			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			bool isRiggingAvailable = false;
			if (targetModifier != null 
				&& targetModifier.IsTarget_Bone 
				&& targetModifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				isRiggingAvailable = true;//Rigging은 허용하자
			}

			//추가 22.5.13 [v1.4.0]
			//선택된 모디파이어가 Transform을 관여하지 않는 타입이라면,
			//다른 모디파이어의 Transform을 막을 필요가 없다.
			bool isTransformUpdatableOnNotSelectedModifier = false;//이게 True라면 선택되지 않은 모디파이어도 Transform계열 연산을 지원한다.
			if(targetModifier != null && 
				(targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.ColorOnly
				|| targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly
				|| targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic))
			{
				isTransformUpdatableOnNotSelectedModifier = true;
			}
			

			apModifierBase curModifier = null;
			apModifierParamSetGroup curParamSetGroup = null;

			for (int i = 0; i < _modifiers.Count; i++)
			{
				curModifier = _modifiers[i];

				if (curModifier == targetModifier && targetModifier != null && targetParamSetGroup != null)
				{
					//동일한 Modifier이다. 
					// ParamSetGroup이 같은 경우 무조건 활성
					// 다를 경우 : Color 제외하고 무조건 비활성

					//curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;//이전
					curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit;//변경 21.2.14 : 편집 중인 모디파이어

					
					for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
					{
						curParamSetGroup = curModifier._paramSetGroup_controller[iP];
						if (targetParamSetGroup == curParamSetGroup)
						{
							//편집 중인 PSG
							curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
							curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
						}
						else
						{
							curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

							//색상 미리보기는 지원하는 경우
							if(multipleModType == OTHER_MOD_RUN_OPTION.ActiveColorOnly
								|| multipleModType == OTHER_MOD_RUN_OPTION.ActiveAllPossible)
							{
								//색상은 분리해서 따로 Enable이 가능
								curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
							}
							else
							{
								curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
							}
						}
					}
				}
				else if (isRiggingAvailable && curModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//만약 Rigging 타입은 예외로 친다면..
					//curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;//이전
					curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background;//변경 21.2.15

					for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
					{
						curParamSetGroup = curModifier._paramSetGroup_controller[iP];

						curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
						curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;//<<사실 색상은 상관 없는뎅
					}
				}
				else
				{
					//Exclusive에서 다른 것들은 무조건 제외한다.
					for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
					{
						curParamSetGroup = curModifier._paramSetGroup_controller[iP];

						switch (multipleModType)
						{
							case OTHER_MOD_RUN_OPTION.Disabled:
								{
									//선택되지 않은 모든 모디파이어의 PSG는 무조건 비활성화
									curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
									curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
								{
									//색상 옵션에 한해서, 색상 처리는 허가한다.
									curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
									curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
								{
									//가능한 다른 모디파이어의 PSG들도 실행하는 옵션이므로, PSG는 활성화
									curParamSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
									curParamSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								}
								break;
						}
					}

					//변경 22.5.13 [1.4.0]
					switch (multipleModType)
					{
						case OTHER_MOD_RUN_OPTION.Disabled:
							{
								//선택되지 않은 모든 모디파이어는 무조건 비활성화
								curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit;
							}
							break;

						case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
							{
								//색상 옵션에 한해서, 색상 처리는 허가한다.
								curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;//값 변경 21.2.15
							}
							break;

						case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
							{
								//가능한 다른 모디파이어를 허가한다.
								if(isTransformUpdatableOnNotSelectedModifier)
								{
									//Transform을 제어하지 않는 모디파이어가 선택된 상태에서는 Enabled이 가능하다
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run;
								}
								else
								{
									//색상만 허가한다.
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
								}
							}
							break;
					}
				}
			}

			//Child MeshGroup에도 모두 적용하자
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
						if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
						{
							//meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(null, null, isColorCalculated);
							meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(
																	targetModifier, null, 
																	//isAllowColorCalculated,
																	//isEnablePSGEvenIfDisabledModifier

																	//변경
																	multipleModType
																	);//변경 20.9.2 : 자식 메시 그룹의 모디파이어에 적용할 때, 현재 타겟 정보를 넘기자.(Rigging 때문에)
						}
					}
				}
			}
		}


		/// <summary>
		/// TODO : 이거 애니메이션 용으로 고쳐야 한다.
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="paramSetGroups"></param>
		/// <param name="isColorCalculated"></param>
		public void SetExclusiveModifierInEditing_Anim(apModifierBase modifier,
														List<apModifierParamSetGroup> paramSetGroups,
														//bool isColorCalculated,
														//bool isEnablePSGEvenIfDisabledModifier

														OTHER_MOD_RUN_OPTION multipleModType//변경 22.5.13. bool 변수는 너무 어렵다.
														)
		{
			
			//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
			//추가
			//요청한 Modifier가 BoneTransform을 지원하는 경우
			//Rigging은 비활성화 되어서는 안된다.
			bool isRiggingAvailable = false;
			if (modifier != null && modifier.IsTarget_Bone && modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				isRiggingAvailable = true;//Rigging은 허용하자
			}

			//추가 22.5.13 [v1.4.0]
			//선택된 모디파이어가 Transform을 관여하지 않는 타입이라면,
			//다른 모디파이어의 Transform을 막을 필요가 없다.
			bool isTransformUpdatableOnNotSelectedModifier = false;//이게 True라면 선택되지 않은 모디파이어도 Transform계열 연산을 지원한다.
			if(modifier != null && 
				(modifier.ModifierType == apModifierBase.MODIFIER_TYPE.ColorOnly
				|| modifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly
				|| modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic))
			{
				isTransformUpdatableOnNotSelectedModifier = true;
			}
			

			apModifierBase curModifier = null;
			int nModifiers = _modifiers != null ? _modifiers.Count : 0;

			for (int i = 0; i < nModifiers; i++)
			{
				curModifier = _modifiers[i];
				if (curModifier == modifier && modifier != null && paramSetGroups != null && paramSetGroups.Count > 0)
				{
					//편집중인 모디파이어이다.
					//허가된 PSG (TimelineLayer)는 Enable, 그렇지 않다면 Disable로 만들자

					//_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;
					curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit;

					for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
					{
						apModifierParamSetGroup paramSetGroup = curModifier._paramSetGroup_controller[iP];
						if (paramSetGroups.Contains(paramSetGroup))
						{
							//허용되는 ParamSetGroup이다.
							//무조건 허용
							//paramSetGroup._isEnabledExclusive = true;//<<이전
							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
						}
						else
						{
							//허용되지 않는 ParamSetGroup이다.
							//색상은 따로 처리 가능하다.
							//paramSetGroup._isEnabledExclusive = false;//<<이전
							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;//<<이게 왜 Enabled
							
							//색상 미리보기는 지원하는 경우
							if(multipleModType == OTHER_MOD_RUN_OPTION.ActiveColorOnly
								|| multipleModType == OTHER_MOD_RUN_OPTION.ActiveAllPossible)
							{
								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
							}
							else
							{
								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
							}
						}
					}
				}
				else if (isRiggingAvailable && curModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//만약 Rigging 타입은 예외로 친다면..
					//_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;
					curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background;

					for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
					{
						//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
						curModifier._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
						curModifier._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;//<<Rigging은 상관 없는뎅..
					}
				}
				else
					//if(!curModifier.IsAnimated
					//&& isEnablePSGEvenIfDisabledModifier
					//)
				{
					//추가 21.2.17
					//애니메이션이 아닌 모디파이어라면, 모디파이어는 Disabled이지만, PSG는 다 켜보자
					//비활성 객체를 대상으로 적용될 수 있다.
					

					for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
					{
						apModifierParamSetGroup paramSetGroup = curModifier._paramSetGroup_controller[iP];

						switch (multipleModType)
						{
							case OTHER_MOD_RUN_OPTION.Disabled:
								{
									//선택되지 않은 모든 모디파이어의 PSG는 무조건 비활성화
									paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
									paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
								{
									//색상 옵션에 한해서, 색상 처리는 허가한다.
									paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
									paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								}
								break;

							case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
								{
									//가능한 다른 모디파이어의 PSG들도 실행하는 옵션이므로, PSG는 활성화
									paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
									paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
								}
								break;
						}
						
					}

					switch (multipleModType)
					{
						case OTHER_MOD_RUN_OPTION.Disabled:
							{
								curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit;
							}
							break;

						case OTHER_MOD_RUN_OPTION.ActiveColorOnly:
							{
								//색상 옵션에 한해서, 색상 처리는 허가한다.
								curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
							}
							break;

						case OTHER_MOD_RUN_OPTION.ActiveAllPossible:
							{
								//가능한 다른 모디파이어를 허가한다.
								if(isTransformUpdatableOnNotSelectedModifier)
								{
									//Transform을 제어하지 않는 모디파이어가 선택된 상태에서는 Enabled이 가능하다
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run;
								}
								else
								{
									//색상만 허가한다.
									curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
								}
							}
							break;
					}
				}
				//else
				//{
				//	//[ 애니메이션 타입의 선택되지 않은 모디파이어 == 다른 타임라인 ]
				//	//선택되지 않은 모디파이어이다.

				//	//일단 다 빼보자
				//	//색상은 적용 가능
				//	//_modifiers[i]._isActive_InEditorExclusive = false;

				//	//_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;
				//	curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit;

				//	bool isAnyColorUpdate = false;

				//	for (int iP = 0; iP < curModifier._paramSetGroup_controller.Count; iP++)
				//	{
				//		apModifierParamSetGroup paramSetGroup = curModifier._paramSetGroup_controller[iP];
				//		//paramSetGroup._isEnabledExclusive = false;
						
				//		//이전
				//		//paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

				//		//변경 22.5.13
				//		//Transform을 제어하지 않는 모디파이어가 선택된 상태에서는 Enabled이 가능하다
				//		if (isTransformUpdatableOnNotSelectedModifier)
				//		{
				//			paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
				//		}
				//		else
				//		{
				//			//그냥 다른 모디파이어의 Transform(메인 변형)은 비활성
				//			paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
				//		}


				//		if(isColorCalculated)
				//		{
				//			paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
				//			isAnyColorUpdate = true;
				//		}
				//		else
				//		{
				//			paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
				//		}
				//	}

				//	if(isAnyColorUpdate)
				//	{
				//		//Color 업데이트가 되는 ParamSetGroup이 존재한다.
				//		//_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
				//		curModifier._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor;
				//	}
				//}
			}

			//Child MeshGroup에도 모두 적용하자 - False로.. << 이게 문제였네
			if (_parentMeshGroup != null)
			{
				if (_parentMeshGroup._childMeshGroupTransforms != null)
				{
					apTransform_MeshGroup childMeshGroupTF = null;
					for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
					{
						childMeshGroupTF = _parentMeshGroup._childMeshGroupTransforms[i];
						if (childMeshGroupTF._meshGroup != null && childMeshGroupTF._meshGroup != _parentMeshGroup)
						{
							//meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing(null, null, isColorCalculated);
							//childMeshGroupTF._meshGroup._modifierStack.SetExclusiveModifierInEditing(modifier, null, isColorCalculated, isEnablePSGEvenIfDisabledModifier);
							childMeshGroupTF._meshGroup._modifierStack.SetExclusiveModifierInEditing(modifier, null, multipleModType);
						}
					}
				}
			}
		}





		#region [미사용 코드] General Edit는 삭제되었다. 다만 코드는 확인할 것
		///// <summary>
		///// [선택한 Modifier] + [해당 Modifier가 허용하는 다른 Modifier]만 허용한다.
		///// 모든 ParamSetGroup을 허용하므로 에디팅이 조금 다를 수는 있다.
		///// Animation 버전은 따로 만들 것
		///// Mod Unlock 모드이다.
		///// </summary>
		///// <param name="modifier"></param>
		//public void SetExclusiveModifierInEditingGeneral(apModifierBase modifier, bool isColorCalculated, bool isOtherModCalcualte)
		//{

		//	//TODO : 이 내용이 SetExclusiveModifierInEditing 함수에 같이 포함되어야 한다.

		//	//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
		//	apModifierBase.MODIFIER_TYPE[] exGeneralTypes = modifier.GetGeneralExEditableModTypes();
		//	if (exGeneralTypes == null)
		//	{
		//		exGeneralTypes = new apModifierBase.MODIFIER_TYPE[] { modifier.ModifierType };
		//	}

		//	//추가
		//	//요청한 Modifier가 BoneTransform을 지원하는 경우
		//	//Rigging은 비활성화 되어서는 안된다.
		//	for (int i = 0; i < _modifiers.Count; i++)
		//	{
		//		bool isValidType = false;
		//		for (int iGT = 0; iGT < exGeneralTypes.Length; iGT++)
		//		{
		//			if (exGeneralTypes[iGT] == _modifiers[i].ModifierType)
		//			{
		//				isValidType = true;
		//				break;
		//			}
		//		}

		//		if (isValidType)
		//		{
		//			//_modifiers[i]._isActive_InEditorExclusive = true;
		//			_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

		//			for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//			{
		//				//ParamSetGroup도 모두다 허용
		//				//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = true;
		//				_modifiers[i]._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//				_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//			}
		//		}
		//		else
		//		{
		//			//불가
		//			//다만, OtherMod 처리 가능시 실행할 수도 있다. < 추가 3.22
		//			if(isOtherModCalcualte)
		//			{
		//				_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled;

		//				//여기선 완전히 Disabled가 아니라 SubExEnabled로 처리한다.

		//				for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//				{
		//					//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;//<<
		//					_modifiers[i]._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//					if(isColorCalculated)
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//					}
		//					else
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//					}
		//				}
		//			}
		//			else
		//			{
		//				_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

		//				bool isAnyColorUpdate = false;

		//				for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//				{
		//					//_modifiers[i]._paramSetGroup_controller[iP]._isEnabledExclusive = false;//<<
		//					_modifiers[i]._paramSetGroup_controller[iP]._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//					if(isColorCalculated)
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//						isAnyColorUpdate = true;
		//					}
		//					else
		//					{
		//						_modifiers[i]._paramSetGroup_controller[iP]._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//					}
		//				}

		//				if(isAnyColorUpdate)
		//				{
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
		//				}
		//			}




		//		}
		//	}

		//	//Child MeshGroup에도 모두 적용하자
		//	if (_parentMeshGroup != null)
		//	{
		//		if (_parentMeshGroup._childMeshGroupTransforms != null)
		//		{
		//			for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
		//			{
		//				apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
		//				if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
		//				{
		//					meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditingGeneral(modifier, isColorCalculated, isOtherModCalcualte);
		//				}
		//			}
		//		}
		//	}
		//}


		///// <summary>
		///// AnimTimeline을 선택하고, 그 안의 AnimTimeLayer를 모두 활성화한다.
		///// 일반적으로 [선택하지 않은 AnimTimeline]들을 모두 해제하는 반면에, 
		///// 여기서는 해당 ParamSetGroup에 연동된 AnimTimeline이 AnimClip에 포함된다면 모두 포함시킨다.
		///// </summary>
		///// <param name="modifier"></param>
		///// <param name="paramSetGroups"></param>
		//public void SetExclusiveModifierInEditing_MultipleParamSetGroup_General(apModifierBase modifier, apAnimClip targetAnimClip,
		//																		bool isColorCalculated, bool isOtherModCalcualte)
		//{
		//	//Debug.Log("---- SetExclusiveModifierInEditing_MultipleParamSetGroup_General (" + _parentMeshGroup.name + ")----");

		//	//apCalculatedResultParam.RESULT_TYPE targetResultType = modifier.CalculatedResultType;
		//	//추가
		//	//요청한 Modifier가 BoneTransform을 지원하는 경우
		//	//Rigging은 비활성화 되어서는 안된다.
		//	apModifierBase.MODIFIER_TYPE[] exGeneralTypes = modifier.GetGeneralExEditableModTypes();
		//	if (exGeneralTypes == null)
		//	{
		//		exGeneralTypes = new apModifierBase.MODIFIER_TYPE[] { modifier.ModifierType };
		//	}

		//	for (int i = 0; i < _modifiers.Count; i++)
		//	{
		//		bool isValidType = false;
		//		for (int iGT = 0; iGT < exGeneralTypes.Length; iGT++)
		//		{
		//			if (exGeneralTypes[iGT] == _modifiers[i].ModifierType)
		//			{
		//				isValidType = true;
		//				break;
		//			}
		//		}

		//		if (isValidType)
		//		{
		//			//AnimClip을 포함하는 ParamSetGroup에 한해서 
		//			_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled;

		//			for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//			{
		//				apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//				if (paramSetGroup._keyAnimClip == targetAnimClip)
		//				{
		//					//무조건 활성
		//					//paramSetGroup._isEnabledExclusive = true;
		//					paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//					paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//				}
		//				else
		//				{
		//					//이건 완전히 불가 (Color, Other Mod 상관없다)
		//					//다른 애니메이션이다.
		//					//paramSetGroup._isEnabledExclusive = false;
		//					paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//					paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			//지원하는 타입이 아니다.
		//			//모두 Disabled한다.
		//			//..> 변경
		//			//지원하는 타입이 아닐때 Other Mod가 켜진 상태 또는 Color라면 업데이트를 해야한다.
		//			//Color + Transform이 항상 Disabled인 경우
		//			//-> Animation Type이며 ParamSetGroup의 AnimClip이 다른 경우
		//			//그게 아니라면 Color까지 다 체크해봐야 한다.

		//			//- Animation 타입이 아닌 경우
		//			//- Animation 타입일 때, 지금 AnimClip에 해당하는 경우

		//			if(_modifiers[i].IsAnimated)
		//			{
		//				//애니메이션 타입이다.
		//				//ParamSetGroup의 AnimClip이 다르면 무조건 Disabled이다.
		//				if (isOtherModCalcualte)
		//				{
		//					//완전히 불가 -> SubEx
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						if (paramSetGroup._keyAnimClip == targetAnimClip)
		//						{
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;

		//							if (isColorCalculated)
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//							}
		//							else
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//							}
		//						}
		//						else
		//						{
		//							//AnimClip이 다르다면 얄짤없이 Disabled
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//						}
		//					}
		//				}
		//				else
		//				{
		//					//애니메이션 타입인데 동시에 실행이 안되는 타입
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

		//					bool isAnyColorUpdate = false;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						if (paramSetGroup._keyAnimClip == targetAnimClip)
		//						{
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

		//							if (isColorCalculated)
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//								isAnyColorUpdate = true;
		//							}
		//							else
		//							{
		//								paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//							}
		//						}
		//						else
		//						{
		//							//이건 얄짤없이 Disabled
		//							paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//						}
		//					}

		//					if(isAnyColorUpdate)
		//					{
		//						//Color 업데이트가 존재한다.
		//						_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
		//					}
		//				}
		//			}
		//			else
		//			{
		//				//애니메이션 타입이 아니다.
		//				//무조건 Disabled인 경우는 없다.
		//				if (isOtherModCalcualte)
		//				{
		//					//완전히 불가 -> SubEx
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;

		//						if(isColorCalculated)
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//						}
		//						else
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.SubExEnabled;
		//						}
		//					}
		//				}
		//				else
		//				{
		//					//완전히 불가
		//					_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.Disabled;

		//					bool isAnyColorUpdate = false;

		//					for (int iP = 0; iP < _modifiers[i]._paramSetGroup_controller.Count; iP++)
		//					{
		//						apModifierParamSetGroup paramSetGroup = _modifiers[i]._paramSetGroup_controller[iP];
		//						//paramSetGroup._isEnabledExclusive = false;
		//						paramSetGroup._modExType_Transform = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;

		//						if(isColorCalculated)
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Enabled;
		//							isAnyColorUpdate = true;
		//						}
		//						else
		//						{
		//							paramSetGroup._modExType_Color = apModifierParamSetGroup.MOD_EX_CALCULATE.Disabled;
		//						}
		//					}

		//					if(isAnyColorUpdate)
		//					{
		//						//Color 업데이트가 있다.
		//						_modifiers[i]._editorExclusiveActiveMod = apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled;
		//					}
		//				}
		//			}


		//		}
		//	}

		//	//Child MeshGroup에도 모두 적용하자
		//	if (_parentMeshGroup != null)
		//	{
		//		if (_parentMeshGroup._childMeshGroupTransforms != null)
		//		{
		//			for (int i = 0; i < _parentMeshGroup._childMeshGroupTransforms.Count; i++)
		//			{
		//				apTransform_MeshGroup meshGroupTransform = _parentMeshGroup._childMeshGroupTransforms[i];
		//				if (meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != _parentMeshGroup)
		//				{
		//					meshGroupTransform._meshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(modifier, targetAnimClip, isColorCalculated, isOtherModCalcualte);
		//				}
		//			}
		//		}
		//	}
		//} 
		#endregion



		// Add / Remove
		//----------------------------------------------------
		public void AddModifier(apModifierBase modifier, apModifierBase.MODIFIER_TYPE modifierType)
		{
			switch (modifierType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:

					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					_modifiers_Volume.Add((apModifier_Volume)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					_modifiers_Morph.Add((apModifier_Morph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					_modifiers_AnimatedMorph.Add((apModifier_AnimatedMorph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					_modifiers_Rigging.Add((apModifier_Rigging)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
					_modifiers_Physic.Add((apModifier_Physic)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					_modifiers_TF.Add((apModifier_TF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					_modifiers_AnimatedTF.Add((apModifier_AnimatedTF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					_modifiers_FFD.Add((apModifier_FFD)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					_modifiers_AnimatedFFD.Add((apModifier_AnimatedFFD)modifier);
					break;


					//추가 21.7.20 : 색상 모디파이어 추가
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					_modifiers_ColorOnly.Add((apModifier_ColorOnly)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					_modifiers_AnimatedColorOnly.Add((apModifier_AnimatedColorOnly)modifier);
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + modifier + "]");
					break;
			}
		}


		public void RemoveModifier(apModifierBase modifier)
		{
			apModifierBase.MODIFIER_TYPE modType = modifier.ModifierType;

			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:

					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					_modifiers_Volume.Remove((apModifier_Volume)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					_modifiers_Morph.Remove((apModifier_Morph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					_modifiers_AnimatedMorph.Remove((apModifier_AnimatedMorph)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					_modifiers_Rigging.Remove((apModifier_Rigging)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.Physic:
					_modifiers_Physic.Remove((apModifier_Physic)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					_modifiers_TF.Remove((apModifier_TF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					_modifiers_AnimatedTF.Remove((apModifier_AnimatedTF)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					_modifiers_FFD.Remove((apModifier_FFD)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					_modifiers_AnimatedFFD.Remove((apModifier_AnimatedFFD)modifier);
					break;

					//추가 21.7.20 : 색상 모디파이어
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					_modifiers_ColorOnly.Remove((apModifier_ColorOnly)modifier);
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					_modifiers_AnimatedColorOnly.Remove((apModifier_AnimatedColorOnly)modifier);
					break;
			}
		}


		// Link
		//----------------------------------------------------
		public void ClearAllCalculateParams(apModifierBase targetSelectedModifier)
		{
			apRenderUnit renderUnit = null;
			if (targetSelectedModifier == null)
			{
				//모든 모디파이어의 ResultParam을 초기화한다면 (기존)
				for (int i = 0; i < _modifiers.Count; i++)
				{
					_modifiers[i]._calculatedResultParams.Clear();
				}
				//렌더 유닛들의 Stack도 리셋한다.
				for (int i = 0; i < _parentMeshGroup._renderUnits_All.Count; i++)
				{
					renderUnit = _parentMeshGroup._renderUnits_All[i];
					renderUnit._calculatedStack.ClearResultParams();
				}
			}
			else
			{
				//특정 모디파이어에 대해서만 초기화하려고 한다면 (추가 20.4.21)
				for (int i = 0; i < _modifiers.Count; i++)
				{
					if(_modifiers[i] == targetSelectedModifier)
					{
						//같을때만 (없을 수도 있어서..)
						_modifiers[i]._calculatedResultParams.Clear();
					}
				}
				//렌더 유닛들의 Stack도 리셋한다.
				for (int i = 0; i < _parentMeshGroup._renderUnits_All.Count; i++)
				{
					renderUnit = _parentMeshGroup._renderUnits_All[i];
					renderUnit._calculatedStack.ClearResultParamsOfModifier(targetSelectedModifier);
				}
			}
			
		}

		
		public void LinkModifierStackToRenderUnitCalculateStack(bool isRoot = true, apMeshGroup rootMeshGroup = null, apUtil.LinkRefreshRequest linkRefreshRequest = null)
		{
			//전체 Modifier중에서 RenderUnit을 포함한 Modifer를 찾는다.
			//그 중, RenderUnit에 대한것만 처리할 CalculateResultParam을 만들고 연동한다.
			//ResultParam을 RenderUnit의 CalculateStack에 넣는다.

			
			//수정
			//각 ModMesh에서 계층적인 Link를 할 수 있도록 RenderUnit을 매번 바꾸어주자
			if (isRoot)
			{
				rootMeshGroup = _parentMeshGroup;

				//Modifier-ParamSetGroup-ParamSet + ModMesh가 "실제 RenderUnit"과 링크되지 않으므로
				//Calculate Param을 만들기 전에 이 링크를 먼저 해주어야 한다.
			}
			
			//최적화 20.4.4
			bool isOnlySelectedModifier = false;//선택한 모디파이어만 처리
			bool isSkipAllAnimModifiers = false;//모든 Anim 모디파이어 생략
			bool isSkipUnselectedAnimPSGs = false;//선택되지 않은 AnimClip에 대한 Anim-PSG 생략
			apAnimClip curSelectedAnimClip = null;
			apModifierBase curSelectedModifier = null;
			if(linkRefreshRequest != null)
			{
				if(linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.SelectedModifier)
				{
					//선택한 모디파이어만 처리
					isOnlySelectedModifier = true;
					curSelectedModifier = linkRefreshRequest.Modifier;
				}
				else if(linkRefreshRequest.Request_Modifier == apUtil.LR_REQUEST__MODIFIER.AllModifiers_ExceptAnimMods)
				{
					//애니메이션 모디파이어는 모두 생략
					isSkipAllAnimModifiers = true;
				}
				else
				{
					//모든 모디파이어 처리
					if(linkRefreshRequest.Request_PSG == apUtil.LR_REQUEST__PSG.SelectedAnimClipPSG_IfAnimModifier)
					{
						//Anim 모디파이어 중) 선택한 AnimClip에 해당하지 않는 PSG는 생략
						isSkipUnselectedAnimPSGs = true;
						curSelectedAnimClip = linkRefreshRequest.AnimClip;
					}
				}
			}


			//Modifier를 돌면서 ParamSet 데이터를 Calculated 데이터로 변환해서 옮긴다.
			for (int iMod = 0; iMod < _modifiers.Count; iMod++)
			{
				//Modifier ->..
				apModifierBase modifier = _modifiers[iMod];
				//Debug.Log("--- Check Modifier [" + modifier.DisplayName + "]");

				if(isOnlySelectedModifier && modifier != curSelectedModifier)
				{
					//최적화 1) 선택되지 않은 모디파이어는 생략
					continue;
				}

				
				if(isSkipAllAnimModifiers && modifier.IsAnimated)
				{
					//최적화 2) 모든 Anim 모디파이어를 생략
					continue;
				}

				List<apModifierParamSetGroup> paramSetGroups = modifier._paramSetGroup_controller;

				for (int iGroup = 0; iGroup < paramSetGroups.Count; iGroup++)
				{
					//Modifier -> ParamSetGroup ->..
					apModifierParamSetGroup paramSetGroup = paramSetGroups[iGroup];

					
					if(isSkipUnselectedAnimPSGs && modifier.IsAnimated)
					{	
						if(paramSetGroup._keyAnimClip != curSelectedAnimClip)
						{
							//최적화 3) 만약 스킵할 수 있는 "애니메이션 모디파이어의 PSG"라면, 처리를 생략한다.
							continue;
						}
					}

					List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

					

					for (int iParam = 0; iParam < paramSets.Count; iParam++)
					{
						//Modifier -> ParamSetGroup -> ParamSet ->...
						apModifierParamSet paramSet = paramSets[iParam];

						List<apModifiedMesh> modMeshes = paramSet._meshData;
						List<apModifiedBone> modBones = paramSet._boneData;


						//1. Mod Mesh => Calculate Param으로 연결한다.
						for (int iModMesh = 0; iModMesh < modMeshes.Count; iModMesh++)
						{
							//[핵심]
							//Modifier -> ParamSetGroup -> ParamSet -> ModMeh 
							//이제 이 ModMesh와 타겟 Transform을 연결하자.
							//연결할땐 Calculated 오브젝트를 만들어서 연결
							apModifiedMesh modMesh = modMeshes[iModMesh];

							if (modMesh._renderUnit == null)
							{
								//>> 당장 링크가 안될 수도 있다. (선택한 MeshGroup이 아닐 경우)
								//이때는 걍 무시한다.
								continue;
							}
							
							//이미 만든 Calculate Param이 있는지 확인
							apCalculatedResultParam existParam = modifier.GetCalculatedResultParam(modMesh._renderUnit);

							
							if (existParam != null)
							{
								existParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, modMesh, null);
								existParam.RefreshResultVertices();

								// > 추가 12.03 <
								//ParamKeyValue가 추가될 때에도 CalculateStack을 갱신할 필요가 있다.
								modMesh._renderUnit._calculatedStack.OnParamKeyValueAddedOnCalculatedResultParam(existParam);
							}
							else
							{
								//새로 Calculate Param을 만들고..
								apCalculatedResultParam newCalParam = new apCalculatedResultParam(
									modifier.CalculatedValueType,
									modifier.CalculatedSpace,
									modifier,
									modMesh._renderUnit,
									modMesh._renderUnit,
									null//<Bone은 없으닝께..
									//weightedVertexData // << 19.5.20 : 삭제
									);

								newCalParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, modMesh, null);

								// Modifier에 등록하고
								modifier._calculatedResultParams.Add(newCalParam);

								//RenderUnit에도 등록을 하자
								//<< 여기가 호출되어야 하는데 안되는 것 같다 >>
								modMesh._renderUnit._calculatedStack.AddCalculatedResultParam(newCalParam, modMesh._renderUnit);
							}
							
						}

						//변경 : ModBone의 계산을 위해서 모든 ModBone이 계산되는 RenderUnit은 "Root  MeshGroup"의 "Root RenderUnit"이다.
						apRenderUnit modBoneRenderUnit = rootMeshGroup._rootRenderUnit;
						if (modBoneRenderUnit != null)
						{

							//2. Mod Bone => Calculate Param으로 연결한다.
							for (int iModBone = 0; iModBone < modBones.Count; iModBone++)
							{
								apModifiedBone modBone = modBones[iModBone];

								if (modBone._bone == null || modBone._renderUnit == null)
								{
									//일단 무시하자. Stack에 넣을 필요가 없다는 것
									continue;
								}

								//apCalculatedResultParam existParam = modifier.GetCalculatedResultParam_Bone(modBone._renderUnit, modBone._bone);//이전
								apCalculatedResultParam existParam = modifier.GetCalculatedResultParam_Bone(modBoneRenderUnit, modBone._bone, modBone._renderUnit);//변경

								if (existParam != null)
								{	
									//Debug.LogWarning(" < Add > : " + modBone._bone._name);

									//이미 있다면 ModBone만 추가해주자
									existParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, null, modBone);
									existParam.RefreshResultVertices();
								}
								else
								{
									apCalculatedResultParam newCalParam = new apCalculatedResultParam(
										modifier.CalculatedValueType,
										modifier.CalculatedSpace,
										modifier,
										modBoneRenderUnit,//<<변경
										modBone._renderUnit,
										modBone._bone
										//null//WeightedVertex // 19.5.20 : 삭제
										);

									newCalParam.AddParamSetAndModifiedValue(paramSetGroup, paramSet, null, modBone);

									// Modifier에 등록하고
									modifier._calculatedResultParams.Add(newCalParam);

									//RenderUnit에도 등록을 하자
									modBoneRenderUnit._calculatedStack.AddCalculatedResultParam(newCalParam, modBone._renderUnit);
								}
							}
						}
					}
				}


				//SubList를 한번 정렬하자
				for (int iCal = 0; iCal < modifier._calculatedResultParams.Count; iCal++)
				{
					modifier._calculatedResultParams[iCal].SortSubList();
				}
			}

			//추가>>
			//하위 객체에 대해서도 Link를 자동으로 수행한다.
			//다 끝나고 Sort
			List<apTransform_MeshGroup> childMeshGroupTransforms = _parentMeshGroup._childMeshGroupTransforms;

			apTransform_MeshGroup childMeshGroup = null;

			if (childMeshGroupTransforms != null && childMeshGroupTransforms.Count > 0)
			{
				for (int i = 0; i < childMeshGroupTransforms.Count; i++)
				{
					childMeshGroup = childMeshGroupTransforms[i];
					if (childMeshGroup._meshGroup != null && childMeshGroup._meshGroup != _parentMeshGroup)
					{
						//Debug.Log(">> Child MeshGroup Check : " + childMeshGroup._nickName);
						childMeshGroup._meshGroup._modifierStack.LinkModifierStackToRenderUnitCalculateStack(false, rootMeshGroup, linkRefreshRequest);//<<여기서도 같이 수행
					}
				}
			}

			if (isRoot)
			{
				//Debug.Log("Start Sort : " + _parentMeshGroup._name);
				//Root인 경우
				//RenderUnit들을 검사하면서 Calculated Stack에 대해서 Sort를 해주자
				List<apRenderUnit> renderUnits = _parentMeshGroup._renderUnits_All;
				for (int i = 0; i < renderUnits.Count; i++)
				{
					renderUnits[i]._calculatedStack.Sort();
				}
			}

		}


		/// <summary>
		/// Modifier들의 계산 값들을 초기화한다.
		/// </summary>
		public void InitModifierCalculatedValues()
		{
			for (int iMod = 0; iMod < _modifiers.Count; iMod++)
			{
				//Modifier ->..
				apModifierBase modifier = _modifiers[iMod];

				List<apModifierParamSetGroup> paramSetGroups = modifier._paramSetGroup_controller;

				for (int iGroup = 0; iGroup < paramSetGroups.Count; iGroup++)
				{
					//Modifier -> ParamSetGroup ->..
					apModifierParamSetGroup paramSetGroup = paramSetGroups[iGroup];

					List<apModifierParamSet> paramSets = paramSetGroup._paramSetList;

					for (int iParam = 0; iParam < paramSets.Count; iParam++)
					{
						//Modifier -> ParamSetGroup -> ParamSet ->...
						apModifierParamSet paramSet = paramSets[iParam];

						List<apModifiedMesh> modMeshes = paramSet._meshData;
						List<apModifiedBone> modBones = paramSet._boneData;

						for (int iModMesh = 0; iModMesh < modMeshes.Count; iModMesh++)
						{
							apModifiedMesh modMesh = modMeshes[iModMesh];
							if (modMesh._vertices != null && modMesh._vertices.Count > 0)
							{
								//ModVert 초기화 => 현재는 초기화 할게 없다.

							}
							if (modMesh._vertRigs != null && modMesh._vertRigs.Count > 0)
							{
								//ModVertRig 초기화 => 현재는 초기화 할게 없다.
							}
							if (modMesh._vertWeights != null && modMesh._vertWeights.Count > 0)
							{
								apModifiedVertexWeight vertWeight = null;
								for (int iVW = 0; iVW < modMesh._vertWeights.Count; iVW++)
								{
									vertWeight = modMesh._vertWeights[iVW];
									vertWeight.InitCalculatedValue();//<<초기화를 하자. (여기서는 물리값)
								}
							}
						}

						for (int iModBone = 0; iModBone < modBones.Count; iModBone++)
						{
							apModifiedBone modBone = modBones[iModBone];
							//ModBone도 현재는 초기화 할게 없다.
						}
					}
				}
			}
		}


		// Get / Set
		//----------------------------------------------------
		public int GetNewModifierID(int modifierType, int validationKey)
		{
			return apVersion.I.GetNextModifierID(modifierType, validationKey, IsModifierExist);
		}

		public apModifierBase GetModifier(int uniqueID)
		{
			return _modifiers.Find(delegate (apModifierBase a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public bool IsModifierExist(int uniqueID)
		{
			return _modifiers.Exists(delegate (apModifierBase a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public int GetLastLayer()
		{
			int maxLayer = -1;
			for (int i = 0; i < _modifiers.Count; i++)
			{
				if (maxLayer < _modifiers[i]._layer)
				{
					maxLayer = _modifiers[i]._layer;
				}
			}
			return maxLayer;

		}



	}
}