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
	public class apOptBoneWorldMatrix_Default : apOptBoneWorldMatrix
	{
		// Members
		//----------------------------------------------
		

		// Init
		//----------------------------------------------
		public apOptBoneWorldMatrix_Default(apOptBone bone) : base(bone)
		{
			_mtx_Def = new apMatrix();
			_mtx_Skew = null;
		}

		public override apPortrait.ROOT_BONE_SCALE_METHOD ScaleMethod
		{
			get
			{
				return apPortrait.ROOT_BONE_SCALE_METHOD.Default;
			}
		}

		// Functions : Init
		//----------------------------------------------
		public override void SetIdentity()
		{
			_mtx_Def.SetIdentity();
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
			

			//< 일반 모드 >
			_mtx_Def.SetMatrix(_linkedBone._defaultMatrix, false);
				
			//Mod
			_mtx_Def.Add(localMatrix);

			_mtx_Def.OnBeforeRMultiply();

			if(parentBoneMatrix != null)
			{
				//RMultiply 연산 후에 MakeMatrix 실행
				_mtx_Def.RMultiply(parentBoneMatrix._mtx_Def, true);
			}
			else if(parentRenderUnitMatrix != null)
			{
				//RMultiply 연산 후에 MakeMatrix 실행
				_mtx_Def.RMultiply(parentRenderUnitMatrix, true);
			}
			else
			{
				_mtx_Def.MakeMatrix();
			}

			//if(_linkedBone != null && _linkedBone._name.Contains("Main Helper"))
			//{
			//	Debug.LogWarning("<Default> Make World Matrix [Mod] (" + _linkedBone._name + ")");
			//	Debug.Log(" - Local  : " + localMatrix.ToString());
			//	Debug.Log(" - Result  : " + _mtx_Def.ToString());
			//}
			
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
			//< 일반 모드 >
			_mtx_Def.SetMatrix(_linkedBone._defaultMatrix, false);
			_mtx_Def.OnBeforeRMultiply();

			if(parentBoneMatrix != null)
			{
				//RMultiply 연산 후에 MakeMatrix 실행
				_mtx_Def.RMultiply(parentBoneMatrix._mtx_Def, true);
			}
			else if(parentRenderUnitMatrix != null)
			{
				//RMultiply 연산 후에 MakeMatrix 실행
				_mtx_Def.RMultiply(parentRenderUnitMatrix, true);
			}
			else
			{
				_mtx_Def.MakeMatrix();
			}


			//if(_linkedBone != null && _linkedBone._name.Contains("Main Helper"))
			//{
			//	Debug.LogWarning("<Default> Make World Matrix [No-Mod] (" + _linkedBone._name + ")");
			//	Debug.Log(" - Result  : " + _mtx_Def.ToString());
			//}


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
			//< 일반 모드 >
			_mtx_Def.SetMatrix(_linkedBone._defaultMatrix, false);
				
			//Mod
			_mtx_Def.Add(localMatrix);

			_mtx_Def.OnBeforeRMultiply();

			if(parentBoneMatrix != null)
			{
				//RMultiply 연산 후에 MakeMatrix 실행
				_mtx_Def.RMultiply(parentBoneMatrix._mtx_Def, false);
			}
			else if(parentRenderUnitMatrix != null)
			{
				//RMultiply 연산 후에 MakeMatrix 실행
				_mtx_Def.RMultiply(parentRenderUnitMatrix, false);
			}
				
			//IK 각도를 지정하자
			_mtx_Def.SetRotate(nextIKAngle, true);		
			

			//if(_linkedBone != null && _linkedBone._name.Contains("Main Helper"))
			//{
			//	Debug.LogError("<Default> Make World Matrix [IK] (" + _linkedBone._name + ")");
			//	Debug.Log(" - Local  : " + localMatrix.ToString());
			//	Debug.Log(" - IK  : " + nextIKAngle);
			//	Debug.Log(" - Result  : " + _mtx_Def.ToString());
			//}
		}


		// Functions : MakeMatrix
		//-------------------------------------------------
		public override void MakeMatrix(bool isInverseCalculate)
		{
			_mtx_Def.MakeMatrix(isInverseCalculate);
		}

		// Functions : Copy
		//-------------------------------------------------
		public override void CopyFromMatrix(apOptBoneWorldMatrix srcBoneWorldMatrix)
		{
			_mtx_Def.SetMatrix(srcBoneWorldMatrix._mtx_Def, true);
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
			return IKTargetPos;//그대로 사용
		}



		// Functions : Move / Rotate / Scale
		//-------------------------------------------------
		public override void MoveAsResult(Vector2 deltaPos)
		{
			_mtx_Def._pos += deltaPos;
			_mtx_Def.MakeMatrix();
		}

		public override void RotateAsStep1(float deltaAngle, bool isMakeMatrix)
		{
			_mtx_Def._angleDeg += deltaAngle;
			if(isMakeMatrix)
			{
				_mtx_Def.MakeMatrix();
			}
		}
		
		public override void SetAngleAsStep1(float angle, bool isMakeMatrix)
		{
			_mtx_Def._angleDeg = angle;
			if(isMakeMatrix)
			{
				_mtx_Def.MakeMatrix();
			}
		}


		
		public override void SetTRSAsResult(	bool isMove, Vector2 pos, 
												bool isRotate, float angle, 
												bool isScale, Vector2 scale,
												bool isIKAngle, float IKAngle_World, float IKAngle_Delta, float IKWeight)
		{
			if (isMove)
			{
				_mtx_Def._pos.x = pos.x;
				_mtx_Def._pos.y = pos.y;
			}
			
			
			if(isIKAngle && IKWeight > 0.0f)
			{
				//IK에 의한 선형 보간을 한다.
				angle = ((1.0f - IKWeight) * angle) + (IKAngle_World * IKWeight);
				isRotate = true;
			}

			if(isRotate)
			{	
				_mtx_Def._angleDeg = angle;
			}

			if(isScale)
			{
				_mtx_Def._scale.x = scale.x;
				_mtx_Def._scale.y = scale.y;
			}
			
			_mtx_Def.MakeMatrix();
		}


		// Functions : Multiply Point
		//-------------------------------------------------
		public override Vector2 MulPoint2(Vector2 point)
		{
			return _mtx_Def.MulPoint2(point);
		}
		public override Vector2 InvMulPoint2(Vector2 point)
		{
			return _mtx_Def.InvMulPoint2(point);
		}

		// Get
		//-------------------------------------------------
		public override Vector2 Pos { get { return _mtx_Def._pos; } }
		public override float Angle { get { return _mtx_Def._angleDeg; } }
		public override Vector2 Scale { get { return _mtx_Def._scale; } }

		public override apMatrix3x3 MtrxToSpace { get { return _mtx_Def.MtrxToSpace; } }
		public override apMatrix3x3 MtrxToLowerSpace { get { return _mtx_Def.MtrxToLowerSpace; } }

		//IK Space
		/// <summary>
		/// IK 계산시 사용되는 좌표계에서의 위치값.
		/// Def 모드에서는 일반 월드 좌표이며, Skew 모드에서는 Step1에서의 위치이다.
		/// </summary>
		public override Vector2 Pos_IKSpace { get { return _mtx_Def._pos; } }
		public override float Angle_IKSpace { get { return _mtx_Def._angleDeg; } }
		
	}
}