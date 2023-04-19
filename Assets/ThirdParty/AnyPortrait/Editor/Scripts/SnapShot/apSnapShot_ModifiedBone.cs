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
	/// ModBone 복사를 위한 스냅샷 객체
	/// Pose를 복사하는 기능은 따로 만들자
	/// (Pose 복사를 위해서는 Bone의 다중 선택이 필요하다)
	/// </summary>
	public class apSnapShot_ModifiedBone : apSnapShotBase
	{
		// Members
		//-------------------------------------------------
		// 키 + 데이터
		private apMeshGroup _key_MeshGroupOfMod = null;
		private apMeshGroup _key_MeshGroupOfBone = null;
		private apBone _key_Bone = null;

		//데이터
		public apMatrix _transformMatrix = new apMatrix();


		// Init
		//--------------------------------------------
		public apSnapShot_ModifiedBone() : base()
		{

		}


		// Functions
		//--------------------------------------------
		public override void Clear()
		{
			base.Clear();

			_key_MeshGroupOfMod = null;
			_key_MeshGroupOfBone = null;
			_key_Bone = null;

			if(_transformMatrix == null)
			{
				_transformMatrix = new apMatrix();
			}
			_transformMatrix.SetIdentity();
			
		}





		public override bool IsKeySyncable(object target)
		{
			if (!(target is apModifiedBone))
			{
				return false;
			}

			apModifiedBone targetModBone = target as apModifiedBone;
			if (targetModBone == null)
			{
				return false;
			}

			//Key 체크를 하자
			if (targetModBone._meshGroup_Modifier != _key_MeshGroupOfMod ||
				targetModBone._meshGroup_Bone != _key_MeshGroupOfBone ||
				targetModBone._bone != _key_Bone)
			{
				return false;
			}

			return true;
		}


		//추가 21.3.19 : TF에서는 그냥 본 타입이면 아무렇게나 복사할 수 있다. 조건이 많이 완화됨
		public override bool IsKeySyncable_TFMod(object target)
		{
			if (!(target is apModifiedBone))
			{
				return false;
			}

			apModifiedBone targetModBone = target as apModifiedBone;
			if (targetModBone == null)
			{
				return false;
			}

			//Key 체크를 하자
			//그냥 둘다 Bone 대상이면 된다.
			if (_key_Bone != null && targetModBone._bone != null)
			{
				return true;
			}

			return false;
		}



		public override bool Save(object target, string strParam)
		{
			base.Save(target, strParam);

			apModifiedBone modBone = target as apModifiedBone;
			if (modBone == null)
			{
				return false;
			}

			_key_MeshGroupOfMod = modBone._meshGroup_Modifier;
			_key_MeshGroupOfBone = modBone._meshGroup_Bone;
			_key_Bone = modBone._bone;

			_transformMatrix.SetMatrix(modBone._transformMatrix, true);
			return true;
		}

		public override bool Load(object targetObj)
		{
			apModifiedBone modBone = targetObj as apModifiedBone;
			if (modBone == null)
			{
				return false;
			}

			modBone._transformMatrix.SetMatrix(_transformMatrix, true);
			modBone._transformMatrix.MakeMatrix();

			return true;
		}


		//다중 모드 메시 복사-붙여넣기용
		//-------------------------------------------------------
		/// <summary>
		/// 여러개의 스냅샷을 누적하기 전에 이 함수를 호출하자
		/// </summary>
		public void ReadyToAddMultipleSnapShots(bool isReadyToSum)
		{
			if(_transformMatrix == null)
			{
				_transformMatrix = new apMatrix();
			}
			if(isReadyToSum)
			{
				//Sum 방식이라면 > Scale을 곱할 것이므로 Vector2.One이어야 한다.
				_transformMatrix.SetIdentity();
			}
			else
			{
				//Average 방식이라면 > 모두 더해서 나눌 것이므로 Vector2.Zero여야 한다.
				_transformMatrix.SetZero();//누적시켜야 하므로 Zero
			}
		}

		/// <summary>
		/// 다른 SnapShot의 데이터를 누적시키자 (다중 복붙용)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="strParam"></param>
		/// <returns></returns>
		public void AddSnapShot(apSnapShot_ModifiedBone otherSnapShot, float weight, bool isSumMethod)
		{	
			if(otherSnapShot == null)
			{
				return;
			}
			//기본 TF 정보들
			if(otherSnapShot._transformMatrix != null)
			{
				_transformMatrix._pos += otherSnapShot._transformMatrix._pos * weight;
				_transformMatrix._angleDeg += otherSnapShot._transformMatrix._angleDeg * weight;
				if(isSumMethod)
				{
					//Sum 방식이면 : 1 > 모두 곱하기 (가중치 없음)
					_transformMatrix._scale.x *= otherSnapShot._transformMatrix._scale.x;
					_transformMatrix._scale.y *= otherSnapShot._transformMatrix._scale.y;
				}
				else
				{
					//Average 방식이면 : 0 > 가중치 더하기
					_transformMatrix._scale += otherSnapShot._transformMatrix._scale * weight;
				}
			}
		}
	}

}