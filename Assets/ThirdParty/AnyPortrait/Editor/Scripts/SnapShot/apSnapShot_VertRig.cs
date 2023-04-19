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

	public class apSnapShot_VertRig : apSnapShotBase
	{
		// Members
		//-----------------------------------------------------
		// 키값 : 같은 키값일때 복사가 가능하다
		//VertRig인 경우, Modifier가 같아야한다.

		//Vertex 다중 선택한 경우에는 Copy가 불가능하다
		//apModifiedVertRig.WeightPair의 값을 복사한다.
		//거의 대부분의 값을 복사한다.
		public class WeightPair
		{
			public apBone _bone = null;
			public apMeshGroup _meshGroup = null;
			public float _weight = 0.0f;

			public WeightPair(apBone bone, apMeshGroup meshGroup, float weight)
			{
				_bone = bone;
				_meshGroup = meshGroup;
				_weight = weight;
			}
		}

		private List<WeightPair> _weightPairs = new List<WeightPair>();


		// Init
		//------------------------------------------------------------
		public apSnapShot_VertRig() : base()
		{

		}



		// Functions
		//------------------------------------------------------------
		public override void Clear()
		{
			if(_weightPairs == null)
			{
				_weightPairs = new List<WeightPair>();
			}
			_weightPairs.Clear();
		}



		public override bool IsKeySyncable(object target)
		{
			if (!(target is apModifiedVertexRig))
			{
				return false;
			}

			//키가 없이 자연스럽게 복붙가능
			return true;
		}

		public override bool Save(object target, string strParam)
		{
			base.Save(target, strParam);

			if (!(target is apModifiedVertexRig))
			{
				return false;
			}

			apModifiedVertexRig vertRig = target as apModifiedVertexRig;
			if (vertRig == null)
			{
				return false;
			}

			_weightPairs.Clear();
			for (int i = 0; i < vertRig._weightPairs.Count; i++)
			{
				apModifiedVertexRig.WeightPair srcWP = vertRig._weightPairs[i];
				_weightPairs.Add(new WeightPair(srcWP._bone, srcWP._meshGroup, srcWP._weight));
			}

			return true;
		}

		public override bool Load(object targetObj)
		{
			apModifiedVertexRig vertRig = targetObj as apModifiedVertexRig;
			if (vertRig == null)
			{
				return false;
			}

			//일단 값을 모두 초기화한다.
			for (int i = 0; i < vertRig._weightPairs.Count; i++)
			{
				vertRig._weightPairs[i]._weight = 0.0f;
			}

			//저장된 값을 넣어준다.
			for (int iSrc = 0; iSrc < _weightPairs.Count; iSrc++)
			{
				WeightPair src = _weightPairs[iSrc];
				if (src._bone == null || src._meshGroup == null)
				{
					continue;
				}

				apModifiedVertexRig.WeightPair dstWeightPair = vertRig._weightPairs.Find(delegate (apModifiedVertexRig.WeightPair a)
				{
					return a._bone == src._bone;
				});

				if (dstWeightPair == null)
				{
					apModifiedVertexRig.WeightPair newWP = new apModifiedVertexRig.WeightPair(src._bone);
					newWP._weight = src._weight;
					vertRig._weightPairs.Add(newWP);
				}
				else
				{
					dstWeightPair._weight = src._weight;
				}
			}

			vertRig.CalculateTotalWeight();

			return true;
		}

	}

}