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
	public class apModifier_Morph : apModifierBase
	{
		// Members
		//----------------------------------------------
		//[NonSerialized]
		//private MODIFIER_TYPE[] _generalExEditableModType = new MODIFIER_TYPE[] {   MODIFIER_TYPE.Morph,
		//																		MODIFIER_TYPE.Rigging,
		//																		MODIFIER_TYPE.TF };

		// Init
		//----------------------------------------------
		//public apModifier_Morph() : base()
		//{
		//}

		//public override void Init()
		//{
		//	base.Init();
		//}


		public override void SetInitSetting(int uniqueID, int layer, int meshGroupID, apMeshGroup meshGroup)
		{
			base.SetInitSetting(uniqueID, layer, meshGroupID, meshGroup);
		}



		//public override void RefreshParamSet()
		//{
		//	base.RefreshParamSet();

		//	//ModifiedMesh를 ParamSet에 추가해준다면 CalculatedSet에 자동으로 추가된다.

		//	////테스트로 쓸 MeshTransform 하나를 가져오자
		//	//if (_meshGroup._childMeshTransforms.Count > 0)
		//	//{
		//	//	apTransform_Mesh testMeshTransform = _meshGroup._childMeshTransforms[0];
		//	//	if (testMeshTransform._mesh != null)
		//	//	{
		//	//		bool isNewAddedModMesh = false;

		//	//		// 테스트 코드
		//	//		//파라미터 셋을 돌며, ModMesh가 없는 경우 하나씩 추가해주자
		//	//		for (int i = 0; i < _paramSetList.Count; i++)
		//	//		{
		//	//			if (_paramSetList[i]._meshData.Count == 0)
		//	//			{
		//	//				apModifiedMesh modMesh = new apModifiedMesh();
		//	//				modMesh.Init_VertexMorph(
		//	//					_meshGroup._uniqueID,
		//	//					testMeshTransform._transformUniqueID,
		//	//					testMeshTransform._mesh._uniqueID);

		//	//				modMesh.Link_VertexMorph(_meshGroup, testMeshTransform, _meshGroup.GetRenderUnit(testMeshTransform));
		//	//				_paramSetList[i]._meshData.Add(modMesh);

		//	//				isNewAddedModMesh = true;
		//	//			}
		//	//		}

		//	//		//Calculated 리스트를 갱신해주자
		//	//		if (isNewAddedModMesh)
		//	//		{
		//	//			_meshGroup.RefreshModifierLink();
		//	//		}
		//	//	}
		//	//}
		//}

		// Get / Set
		//----------------------------------------------
		public override MODIFIER_TYPE ModifierType
		{
			get { return MODIFIER_TYPE.Morph; }
		}

		public override apModifierParamSetGroup.SYNC_TARGET SyncTarget
		{
			get { return apModifierParamSetGroup.SYNC_TARGET.Controller; }
		}

		private const string NAME_MORPH_LONG = "Morph (Controller)";
		private const string NAME_MORPH_SHORT = "Morph (Ctrl)";

		public override string DisplayName
		{
			//get { return "Morph (Controller)"; }
			get { return NAME_MORPH_LONG; }
		}

		public override string DisplayNameShort
		{
			//get { return "Morph (Ctrl)"; }
			get { return NAME_MORPH_SHORT; }
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
				return apModifiedMesh.MOD_VALUE_TYPE.VertexPosList |
						apModifiedMesh.MOD_VALUE_TYPE.Color;
			}
		}


		// MeshTransform만 적용
		public override bool IsTarget_MeshTransform { get { return true; } }
		public override bool IsTarget_MeshGroupTransform { get { return false; } }
		public override bool IsTarget_Bone { get { return false; } }
		public override bool IsTarget_ChildMeshTransform { get { return true; } }

		public override bool IsUseParamSetWeight { get { return true; } }//ParamSet 자체의 OverlapWeight를 사용한다.

		//추가
		public override bool IsPhysics { get { return false; } }
		public override bool IsVolume { get { return false; } }

		//[NonSerialized]
		//private int _prevOutputParams = -1;



		///// <summary>
		///// ExEdit 중 GeneralEdit 모드에서 "동시에 작업 가능하도록 허용 된 Modifier Type들"을 리턴한다.
		///// </summary>
		///// <returns></returns>
		//public override MODIFIER_TYPE[] GetGeneralExEditableModTypes()
		//{
		//	return _generalExEditableModType;
		//}


		//[NonSerialized]
		//private bool _isPrevSelectedMatched = false;

		// Functions
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

		//private float _lastDebug = 0.0f;

		public override void Calculate(float tDelta)
		{
			base.Calculate(tDelta);

			CalculatePattern_Morph(tDelta);
		}

		public override void Calculate_DLL(float tDelta)
		{
			base.Calculate_DLL(tDelta);

			CalculatePattern_Morph_DLL(tDelta);
		}
	}

}