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
//using UnityEditor;
//using UnityEditorInternal;

namespace AnyPortrait
{
	/// <summary>
	/// 개선된 BoneWorldMatrix의 Opt버전.
	/// RMultiply로만 구성된 수식과 Skew Scale과 혼용된 수식을 모두 지원한다.
	/// 에디터의 BoneWorldMatrix와 달리, 불필요한 조건문을 없애기 위해 상속으로 생성
	/// </summary>
	public class apOptBoneWorldMatrix
	{
		// Members
		//----------------------------------------------
		protected apOptBone _linkedBone = null;
		public apMatrix _mtx_Def = null;
		public apComplexMatrix _mtx_Skew = null;

		// Init
		//----------------------------------------------
		public apOptBoneWorldMatrix(apOptBone bone)
		{
			_linkedBone = bone;
		}

		public virtual apPortrait.ROOT_BONE_SCALE_METHOD ScaleMethod
		{
			get;
		}

		// Functions : Init
		//----------------------------------------------
		public virtual void SetIdentity() { }



		// Functions : 일반 업데이트
		//----------------------------------------------
		

		/// <summary>
		/// 행렬 업데이트를 한다. (일반 행렬 용)
		/// Local 정보를 포함하고, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public virtual void MakeWorldMatrix_Mod(apMatrix localMatrix, apOptBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix) { }

		/// <summary>
		/// 행렬 업데이트를 한다. (NonModified 행렬 용)
		/// 다른 변형 정보 없이, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix는 null이 될 수 있다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public virtual void MakeWorldMatrix_NoMod(apOptBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix) { }


		/// <summary>
		/// 행렬 업데이트를 한다. (IK 용)
		/// Local 정보를 포함하고, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public virtual void MakeWorldMatrix_IK(	apMatrix localMatrix, apOptBoneWorldMatrix parentBoneMatrix,  apMatrix parentRenderUnitMatrix, float nextIKAngle) { }


		// Functions : MakeMatrix
		//-------------------------------------------------
		public virtual void MakeMatrix(bool isInverseCalculate) { }

		// Functions : Copy
		//-------------------------------------------------
		public virtual void CopyFromMatrix(apOptBoneWorldMatrix srcBoneWorldMatrix) { }
		

		// Functions : World To IK Space
		//-------------------------------------------------
		/// <summary>
		/// IK나 지글본의 대상이 되는 위치를 적절한 좌표계로 이동시킨다.
		/// "렌더 유닛"의 영향을 받지 않는 좌표계로서, Skew에 의한 문제를 해결한다.
		/// 만약 Skew방식이 아니면 이 함수는 의미가 없다. (RMultiply만 있다면 잘 작동하므로)
		/// </summary>
		/// <param name="IKTargetPos"></param>
		/// <returns></returns>
		public virtual Vector2 ConvertForIK(Vector2 IKTargetPos)
		{
			return Vector2.zero;
		}



		// Functions : Move / Rotate / Scale
		//-------------------------------------------------
		public virtual void MoveAsResult(Vector2 deltaPos) { }

		public virtual void RotateAsStep1(float deltaAngle, bool isMakeMatrix) { }
		public virtual void SetAngleAsStep1(float angle, bool isMakeMatrix) { }



		public virtual void SetTRSAsResult(	bool isMove, Vector2 pos, 
											bool isRotate, float angle, 
											bool isScale, Vector2 scale,
											bool isIKAngle, float IKAngle_World, float IKAngle_Delta, float IKWeight) { }


		// Functions : Multiply Point
		//-------------------------------------------------
		public virtual Vector2 MulPoint2(Vector2 point)
		{
			return Vector2.zero;
		}
		public virtual Vector2 InvMulPoint2(Vector2 point)
		{
			return Vector2.zero;
		}

		// Get
		//-------------------------------------------------
		public virtual Vector2 Pos { get; }
		public virtual float Angle { get; }
		public virtual Vector2 Scale { get; }

		public virtual apMatrix3x3 MtrxToSpace { get; }
		public virtual apMatrix3x3 MtrxToLowerSpace { get; }

		//IK Space
		/// <summary>
		/// IK 계산시 사용되는 좌표계에서의 위치값.
		/// Def 모드에서는 일반 월드 좌표이며, Skew 모드에서는 Step1에서의 위치이다.
		/// </summary>
		public virtual Vector2 Pos_IKSpace { get; }
		public virtual float Angle_IKSpace { get; }
		
	}
}