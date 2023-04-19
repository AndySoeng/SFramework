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

namespace AnyPortrait
{

	/// <summary>
	/// Rigging용 Modifier
	/// Calculate시에 Bone / Weight / Dress Bone 대비 Local Matrix를 계산하여 World Matrix를 계산한다.
	/// 입력값이 없어서(Static) ParamSetGroup이 한개만 존재한다.
	/// </summary>
	[Serializable]
	public class apModifier_Rigging : apModifierBase
	{
		// Members
		//----------------------------------------------
		//[NonSerialized]
		//private MODIFIER_TYPE[] _generalExEditableModType = new MODIFIER_TYPE[] {   MODIFIER_TYPE.Morph,
		//																		MODIFIER_TYPE.Rigging,
		//																		MODIFIER_TYPE.TF };

		// Init
		//----------------------------------------------
		//public apModifier_Rigging() : base()
		//{
		//}

		//public override void Init()
		//{

		//}

		//----------------------------------------------
		public override void InitCalculate(float tDelta)
		{
			base.InitCalculate(tDelta);

			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			apCalculatedResultParam calParam = null;
			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];
				calParam.InitCalculate();
				//calParam._isAvailable = false;//삭제 22.5.11
			}

		}


		public override void Calculate(float tDelta)
		{
			base.Calculate(tDelta);

			CalculatePattern_Rigging(tDelta);
		}


		public override void Calculate_DLL(float tDelta)
		{
			base.Calculate_DLL(tDelta);

			CalculatePattern_Rigging_DLL(tDelta);
		}

		// Get / Set
		//----------------------------------------------
		public override MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Rigging; }
		}

		public override apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			//Rigging은 입력값이 없으므로 Static으로 적용한다.
			get { return apModifierParamSetGroup.SYNC_TARGET.Static; }
		}

		private const string NAME_RIGGING_LONG = "Rigging";
		private const string NAME_RIGGING_SHORT = "Rigging";

		public override string DisplayName
		{
			//get { return "Rigging"; }
			get { return NAME_RIGGING_LONG; }
		}

		public override string DisplayNameShort
		{
			//get { return "Rigging"; }
			get { return NAME_RIGGING_SHORT; }
		}

		/// <summary>
		/// Calculate 계산시 어느 단계에서 적용되는가
		/// </summary>
		public override apCalculatedResultParam.CALCULATED_VALUE_TYPE CalculatedValueType
		{
			get { return apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos; }
		}

		public override apCalculatedResultParam.CALCULATED_SPACE CalculatedSpace
		{
			get { return apCalculatedResultParam.CALCULATED_SPACE.Rigging; }
		}


		public override apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get { return apModifiedMesh.MOD_VALUE_TYPE.BoneVertexWeightList; }
		}

		// MeshTransform + Child MeshTransform에 적용한다.
		public override bool IsTarget_MeshTransform { get { return true; } }//<
		public override bool IsTarget_MeshGroupTransform { get { return false; } }
		public override bool IsTarget_Bone { get { return false; } }
		public override bool IsTarget_ChildMeshTransform { get { return true; } }//<

		//추가
		public override bool IsPhysics { get { return false; } }
		public override bool IsVolume { get { return false; } }

		//중요 : Rigging은 Post 업데이트이다.
		public override bool IsPreUpdate { get { return false; } }


		///// <summary>
		///// ExEdit 중 GeneralEdit 모드에서 "동시에 작업 가능하도록 허용 된 Modifier Type들"을 리턴한다.
		///// </summary>
		///// <returns></returns>
		//public override MODIFIER_TYPE[] GetGeneralExEditableModTypes()
		//{
		//	return _generalExEditableModType;
		//}
	}

}