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

	[Serializable]
	public class apModifier_Physic : apModifierBase
	{
		// Members
		//----------------------------------------------
		//[NonSerialized]
		//private MODIFIER_TYPE[] _generalExEditableModType = new MODIFIER_TYPE[] { MODIFIER_TYPE.Physic };

		//F 중심의 처리 방식
		//F 내부 : 


		// Init
		//----------------------------------------------
		//public apModifier_Physic() : base()
		//{
		//}

		//public override void Init()
		//{

		//}

		public override void SetInitSetting(int uniqueID, int layer, int meshGroupID, apMeshGroup meshGroup)
		{
			base.SetInitSetting(uniqueID, layer, meshGroupID, meshGroup);
		}

		// Get / Set
		//----------------------------------------------
		public override MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Physic; }
		}

		public override apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.Static; }
		}

		private const string NAME_PHYSICS_LONG = "Physics";
		private const string NAME_PHYSICS_SHORT = "Physics";

		public override string DisplayName
		{
			//get { return "Physics"; }
			get { return NAME_PHYSICS_LONG; }
		}

		public override string DisplayNameShort
		{
			//get { return "Physics"; }
			get { return NAME_PHYSICS_SHORT ; }
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
			get { return apCalculatedResultParam.CALCULATED_SPACE.World; }
		}

		public override apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get { return apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics; }
		}


		// MeshTransform에만 적용
		public override bool IsTarget_MeshTransform { get { return true; } }
		public override bool IsTarget_MeshGroupTransform { get { return false; } }
		public override bool IsTarget_Bone { get { return false; } }
		public override bool IsTarget_ChildMeshTransform { get { return true; } }

		//추가
		public override bool IsPhysics { get { return true; } }//<<True
		public override bool IsVolume { get { return false; } }

		//중요 : Physics은 Post 업데이트이다.
		public override bool IsPreUpdate { get { return false; } }


		///// <summary>
		///// ExEdit 중 GeneralEdit 모드에서 "동시에 작업 가능하도록 허용 된 Modifier Type들"을 리턴한다.
		///// </summary>
		///// <returns></returns>
		//public override MODIFIER_TYPE[] GetGeneralExEditableModTypes()
		//{
		//	return _generalExEditableModType;
		//}


		// Functions
		//----------------------------------------------
		public override void InitCalculate(float tDelta)
		{
			base.InitCalculate(tDelta);

			_tDeltaFixed = 0.0f;

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

			CalculatePattern_Physics(tDelta);
		}

		public override void Calculate_DLL(float tDelta)
		{
			base.Calculate_DLL(tDelta);

			CalculatePattern_Physics_DLL(tDelta);
		}
	}

}