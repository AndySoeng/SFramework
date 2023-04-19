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
	public class apModifier_AnimatedFFD : apModifierBase
	{
		// Members
		//----------------------------------------------


		// Init
		//----------------------------------------------
		//public apModifier_AnimatedFFD() : base()
		//{
		//}

		//public override void Init()
		//{

		//}
		// Get / Set
		//----------------------------------------------
		public override MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.AnimatedFFD; }
		}

		public override apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.KeyFrame; }
		}

		private const string NAME_FFD_ANIMATION_LONG = "FFD (Animation)";
		private const string NAME_FFD_ANIMATION_SHORT = "FFD (Anim)";

		public override string DisplayName
		{
			//get { return "FFD (Animation)"; }
			get { return NAME_FFD_ANIMATION_LONG; }
		}

		public override string DisplayNameShort
		{
			//get { return "FFD (Anim)"; }
			get { return NAME_FFD_ANIMATION_SHORT; }
		}
		/// <summary>
		/// Calculate 계산시 어느 단계에서 적용되는가
		/// </summary>
		public override apCalculatedResultParam.CALCULATED_VALUE_TYPE CalculatedValueType
		{
			get
			{
				return apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos |
						apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color;
			}
		}

		public override apCalculatedResultParam.CALCULATED_SPACE CalculatedSpace
		{
			get { return apCalculatedResultParam.CALCULATED_SPACE.Object; }
		}

		public override apModifiedMesh.MOD_VALUE_TYPE ModifiedValueType
		{
			get
			{
				return apModifiedMesh.MOD_VALUE_TYPE.FFD |
						apModifiedMesh.MOD_VALUE_TYPE.Color;
			}
		}

		// MeshTransform / MeshGroupTransform에 적용한다.
		public override bool IsTarget_MeshTransform { get { return true; } }
		public override bool IsTarget_MeshGroupTransform { get { return true; } }
		public override bool IsTarget_Bone { get { return false; } }
		public override bool IsTarget_ChildMeshTransform { get { return false; } }

		public override bool IsAnimated { get { return true; } }

		//추가
		public override bool IsPhysics { get { return false; } }
		public override bool IsVolume { get { return false; } }
	}
}