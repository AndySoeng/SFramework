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
	public class apOptBoneWorldMatrix_Skew : apOptBoneWorldMatrix
	{
		// Members
		//----------------------------------------------
		

		// Init
		//----------------------------------------------
		public apOptBoneWorldMatrix_Skew(apOptBone bone) : base(bone)
		{
			_mtx_Def = null;
			_mtx_Skew = new apComplexMatrix();
		}

		public override apPortrait.ROOT_BONE_SCALE_METHOD ScaleMethod
		{
			get
			{
				return apPortrait.ROOT_BONE_SCALE_METHOD.SkewScale;
			}
		}

		// Functions : Init
		//----------------------------------------------
		public override void SetIdentity()
		{
			//< Skew 모드 >
			_mtx_Skew.SetIdentity();
		}



		// Functions : 일반 업데이트
		//----------------------------------------------
		

		/// <summary>
		/// 행렬 업데이트를 한다. (일반 행렬 용)
		/// Local 정보를 포함하고, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public override void MakeWorldMatrix_Mod(	apMatrix localMatrix, 
													apOptBoneWorldMatrix parentBoneMatrix, 
													apMatrix parentRenderUnitMatrix)
		{
			//< Skew 모드 >
			_mtx_Skew.SetMatrix_Step1(_linkedBone._defaultMatrix, false);

			//Mod
			_mtx_Skew.Add(localMatrix);

			_mtx_Skew.OnBeforeMultiply();

			if(parentBoneMatrix != null)
			{
				//Parent Bone은 ComplexMultiply 적용
				_mtx_Skew.ComplexMultiply(parentBoneMatrix._mtx_Skew, false, 0.0f);
			}
			else if(parentRenderUnitMatrix != null)
			{
				//Parent RenderUnit은 SMultiply 적용
				_mtx_Skew.SMultiply(parentRenderUnitMatrix);
			}
			else
			{
				_mtx_Skew.MakeMatrix();
			}
		}

		/// <summary>
		/// 행렬 업데이트를 한다. (NonModified 행렬 용)
		/// 다른 변형 정보 없이, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix는 null이 될 수 있다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public override void MakeWorldMatrix_NoMod(	apOptBoneWorldMatrix parentBoneMatrix, 
													apMatrix parentRenderUnitMatrix)
		{
			//< Skew 모드 >
			_mtx_Skew.SetMatrix_Step1(_linkedBone._defaultMatrix, false);
			_mtx_Skew.OnBeforeMultiply();

			if(parentBoneMatrix != null)
			{
				//Parent Bone은 ComplexMultiply 적용
				_mtx_Skew.ComplexMultiply(parentBoneMatrix._mtx_Skew);
			}
			else if(parentRenderUnitMatrix != null)
			{
				//Parent RenderUnit은 SMultiply 적용
				_mtx_Skew.SMultiply(parentRenderUnitMatrix);
			}
			else
			{
				_mtx_Skew.MakeMatrix();
			}
		}


		/// <summary>
		/// 행렬 업데이트를 한다. (IK 용)
		/// Local 정보를 포함하고, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public override void MakeWorldMatrix_IK(	apMatrix localMatrix, 
													apOptBoneWorldMatrix parentBoneMatrix,  
													apMatrix parentRenderUnitMatrix, 
													float nextIKAngle)
		{
			//< Skew 모드 >
			_mtx_Skew.SetMatrix_Step1(_linkedBone._defaultMatrix, false);
				
			//Mod
			_mtx_Skew.Add(localMatrix);

			_mtx_Skew.OnBeforeMultiply();


			//Skew에서는 IK를 넣기 전에 MakeMatrix를 해야한다.
			if(parentBoneMatrix != null)
			{
				//Parent Bone은 ComplexMultiply 적용
				_mtx_Skew.ComplexMultiply(parentBoneMatrix._mtx_Skew, true, nextIKAngle);
			}
			else if(parentRenderUnitMatrix != null)
			{
				//Parent RenderUnit은 SMultiply 적용
				_mtx_Skew.SetAngleToStep1(nextIKAngle);
				_mtx_Skew.SMultiply(parentRenderUnitMatrix);
			}
			else
			{
				_mtx_Skew.SetAngleToStep1(nextIKAngle);
				_mtx_Skew.MakeMatrix();
			}
		}


		// Functions : MakeMatrix
		//-------------------------------------------------
		public override void MakeMatrix(bool isInverseCalculate)
		{
			_mtx_Skew.MakeMatrix(isInverseCalculate);
		}

		// Functions : Copy
		//-------------------------------------------------
		public override void CopyFromMatrix(apOptBoneWorldMatrix srcBoneWorldMatrix)
		{
			_mtx_Skew.CopyFromComplexMatrix(srcBoneWorldMatrix._mtx_Skew);
		}
		

		// Functions : World To IK Space
		//-------------------------------------------------
		/// <summary>
		/// IK나 지글본의 대상이 되는 위치를 적절한 좌표계로 이동시킨다.
		/// "렌더 유닛"의 영향을 받지 않는 좌표계로서, Skew에 의한 문제를 해결한다.
		/// 만약 Skew방식이 아니면 이 함수는 의미가 없다. (RMultiply만 있다면 잘 작동하므로)
		/// </summary>
		/// <param name="IKTargetPos"></param>
		/// <returns></returns>
		public override Vector2 ConvertForIK(Vector2 IKTargetPos)
		{
			return _mtx_Skew.MtrxToLowerSpace_Step2.MultiplyPoint(IKTargetPos);
		}



		// Functions : Move / Rotate / Scale
		//-------------------------------------------------
		public override void MoveAsResult(Vector2 deltaPos)
		{
			_mtx_Skew.MoveAsPostResult(deltaPos);//이 함수에서 MakeMatrix까지 호출한다.
		}

		public override void RotateAsStep1(float deltaAngle, bool isMakeMatrix)
		{
			_mtx_Skew._angleDeg_Step1 += deltaAngle;
			if(isMakeMatrix)
			{
				_mtx_Skew.MakeMatrix();
			}
		}
		
		public override void SetAngleAsStep1(float angle, bool isMakeMatrix)
		{
			_mtx_Skew._angleDeg_Step1 = angle;
			if(isMakeMatrix)
			{
				_mtx_Skew.MakeMatrix();
			}
		}

		

		public override void SetTRSAsResult(	bool isMove, Vector2 pos, 
												bool isRotate, float angle, 
												bool isScale, Vector2 scale,
												bool isIKAngle, float IKAngle_World, float IKAngle_Delta, float IKWeight)
		{

			if(isIKAngle && IKWeight > 0.0f)
			{
				//IK인 경우 별도의 처리를
				_mtx_Skew.SetAngleToStep1ForIK(IKAngle_World, IKWeight);
				//_mtx_Skew.SetAngleToStep1ForIK(IKAngle_Delta, IKWeight);
			}

			if (isMove || isRotate || isScale)
			{
				//Skew 방식에서는 MakeMatrix를 먼저하고 행렬을 조작한다.
				_mtx_Skew.SetTRSAsPostResult(isMove, pos,
												isRotate, angle,
												isScale, scale);
			}
		}


		// Functions : Multiply Point
		//-------------------------------------------------
		public override Vector2 MulPoint2(Vector2 point)
		{
			return _mtx_Skew.MulPoint2(point);
		}
		public override Vector2 InvMulPoint2(Vector2 point)
		{
			return _mtx_Skew.InvMulPoint2(point);
		}

		// Get
		//-------------------------------------------------
		public override Vector2 Pos { get { return _mtx_Skew._pos; } }
		public override float Angle { get { return _mtx_Skew._angleDeg; } }
		public override Vector2 Scale { get { return _mtx_Skew._scale; } }

		public override apMatrix3x3 MtrxToSpace { get { return _mtx_Skew.MtrxToSpace; } }
		public override apMatrix3x3 MtrxToLowerSpace { get { return _mtx_Skew.MtrxToLowerSpace; } }

		//IK Space
		/// <summary>
		/// IK 계산시 사용되는 좌표계에서의 위치값.
		/// Def 모드에서는 일반 월드 좌표이며, Skew 모드에서는 Step1에서의 위치이다.
		/// </summary>
		public override Vector2 Pos_IKSpace { get { return _mtx_Skew._pos_Step1; } }
		public override float Angle_IKSpace { get { return _mtx_Skew._angleDeg_Step1; } }
	}
}