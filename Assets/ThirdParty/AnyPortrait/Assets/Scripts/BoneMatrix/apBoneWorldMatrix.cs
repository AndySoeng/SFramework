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
	/// 개선된 Bone World Matrix. 옵션에 따라 일반 RMultiply 방식의 연산과 Skew Scale을 지원하는 혼합 연산을 모두 지원한다.
	/// 기존의 apMatrix와 다르게, Bone 처리용 함수를 모두 각각 만든 래핑 클래스이다.
	/// WorldMatrix 전용이므로 직렬화는 안되며, 초기화시 모드를 설정해야한다.
	/// </summary>
	public class apBoneWorldMatrix
	{
		// Members
		//-------------------------------------------------
		private apBone _linkedBone = null;

		public apMatrix _mtx_Def = null;
		public apComplexMatrix _mtx_Skew = null;
		

		private apPortrait.ROOT_BONE_SCALE_METHOD _scaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;

		

		//Static Member
		//-------------------------------------------------
		private static apBoneWorldMatrix[] s_tempMatrix = null;
		private const int NUM_TEMP_MATRIX = 10;

		public static apBoneWorldMatrix GetTemp(int iTemp, apPortrait portrait)
		{
			if(s_tempMatrix == null)
			{
				s_tempMatrix = new apBoneWorldMatrix[NUM_TEMP_MATRIX];
				for (int i = 0; i < NUM_TEMP_MATRIX; i++)
				{
					s_tempMatrix[i] = new apBoneWorldMatrix(null, portrait._rootBoneScaleMethod);
				}
			}
			s_tempMatrix[iTemp].SetScaleMethod(portrait._rootBoneScaleMethod);
			s_tempMatrix[iTemp].SetIdentity();
			return s_tempMatrix[iTemp];
		}


		// Init
		//-------------------------------------------------
		public apBoneWorldMatrix(apBone bone, apPortrait.ROOT_BONE_SCALE_METHOD scaleMethod)
		{
			_linkedBone = bone;
			SetScaleMethod(scaleMethod);
		}

		public void SetScaleMethod(apPortrait.ROOT_BONE_SCALE_METHOD scaleMethod)
		{
			_scaleMethod = scaleMethod;

			if(scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				//기본 모드이다. RMultiply를 이용한다.
				_mtx_Def = new apMatrix();
				_mtx_Skew = null;
			}
			else
			{
				//Skew Scale 모드이다. 
				_mtx_Def = null;
				_mtx_Skew = new apComplexMatrix();
			}
		}


		// Functions : Init
		//-------------------------------------------------
		public void SetIdentity()
		{
			if (_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				//< 일반 모드 >
				_mtx_Def.SetIdentity();
			}
			else
			{
				//< Skew 모드 >
				_mtx_Skew.SetIdentity();
			}

			
		}




		// Functions : 일반 업데이트
		//-------------------------------------------------
		/// <summary>
		/// 행렬 업데이트를 한다. (일반 행렬 용)
		/// Local과 Rig 정보를 포함하며, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public void MakeWorldMatrix_ModRig(	apMatrix localMatrix, apMatrix rigTestMatrix, 
											apBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				//< 일반 모드 >
				_mtx_Def.SetMatrix(_linkedBone._defaultMatrix, false);
				
				//Rig + Mod
				_mtx_Def.Add(rigTestMatrix);
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
			}
			else
			{
				//< Skew 모드 >
				_mtx_Skew.SetMatrix_Step1(_linkedBone._defaultMatrix, false);

				//Rig + Mod
				_mtx_Skew.Add(rigTestMatrix);
				_mtx_Skew.Add(localMatrix);

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

		}

		/// <summary>
		/// 행렬 업데이트를 한다. (일반 행렬 용)
		/// Local 정보를 포함하고, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public void MakeWorldMatrix_Mod(apMatrix localMatrix, 
										apBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix
										//, bool isDebug = false
										)
		{

			//if(isDebug)
			//{
			//	Debug.LogWarning("-->> Make Matrix (Mod) <<--");
			//}

			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
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
			}
			else
			{
				//< Skew 모드 >
				_mtx_Skew.SetMatrix_Step1(_linkedBone._defaultMatrix, false);
				
				//if(isDebug)
				//{
				//	Debug.Log("x Default Matrix\n" + _linkedBone._defaultMatrix.ToString());
				//}

				//Mod
				_mtx_Skew.Add(localMatrix);

				//if(isDebug)
				//{
				//	Debug.Log("+ Local Matrix\n" + localMatrix.MtrxToSpace.ToString());
				//}

				_mtx_Skew.OnBeforeMultiply();

				if(parentBoneMatrix != null)
				{
					//Parent Bone은 ComplexMultiply 적용
					_mtx_Skew.ComplexMultiply(parentBoneMatrix._mtx_Skew,
						false, 0.0f
						//, isDebug
						//, (_linkedBone != null && _linkedBone._name.StartsWith("Bone Debug"))
						);
				}
				else if(parentRenderUnitMatrix != null)
				{
					//Parent RenderUnit은 SMultiply 적용
					_mtx_Skew.SMultiply(parentRenderUnitMatrix
						//, (_linkedBone != null && _linkedBone._name.StartsWith("Bone Debug"))
						);
				}
				else
				{
					_mtx_Skew.MakeMatrix();
				}
			}

		}

		/// <summary>
		/// 행렬 업데이트를 한다. (NonModified 행렬 용)
		/// 다른 변형 정보 없이, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix는 null이 될 수 있다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public void MakeWorldMatrix_NoMod(apBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
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
			}
			else
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

		}



		/// <summary>
		/// 행렬 업데이트를 한다. (IK 용)
		/// Local 정보를 포함하고, 부모 본 또는 렌더 유닛의 행렬을 곱한다.
		/// parentMatrix를 제외하고는 null로 만들면 안된다.
		/// 우선순위는 parentBone이 parentRenderUnit보다 먼저이다.
		/// </summary>
		public void MakeWorldMatrix_IK(apMatrix localMatrix,
										apBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix,
										float nextIKAngle)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
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
			}
			else
			{
				//< Skew 모드 >
				_mtx_Skew.SetMatrix_Step1(_linkedBone._defaultMatrix, false);
				
				//Mod
				_mtx_Skew.Add(localMatrix);

				//Skew 모드에서는 IK가 여기 들어가야 한다.
				//_mtx_Skew.RotateAngleAsStep1(nextIKAngle - nonIKAngle);

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

				//_mtx_Skew.RotateAsPostResult(nextIKAngle - _mtx_Skew._angleDeg);
			}

		}



		


		// Functions : MakeMatrix
		//-------------------------------------------------
		public void MakeMatrix(bool isInverseCalculate)
		{
			if (_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def.MakeMatrix(isInverseCalculate);
			}
			else
			{
				_mtx_Skew.MakeMatrix(isInverseCalculate);
			}
		}



		// Functions : Inverse 계열의 함수들
		//-------------------------------------------------
		/// <summary>
		/// [Inverse 계열]
		/// 이 Matrix가 WorldMatrix일 때, Default Matrix로 낮추어서 변형한다.
		/// 인자는 Null이 되어서 안된다.
		/// </summary>
		/// <param name="parentWorldMatrix"></param>
		/// <param name="localMatrix"></param>
		public void SetWorld2Default(apBoneWorldMatrix parentWorldMatrix, apMatrix localMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def.RInverse(parentWorldMatrix._mtx_Def, false);
				//_mtx_Def.RInverse(localMatrix, false);//<<원래 코드는 이거. 근데 틀린 것 같다.
				_mtx_Def.Subtract(localMatrix);//이게 맞는것 같다.
				_mtx_Def._angleDeg = apUtil.AngleTo180(_mtx_Def._angleDeg);//Default의 각도는 180 이내로 제한된다.
				_mtx_Def.MakeMatrix(false);
			}
			else
			{
				//SInverse > Subtract로 연산하자
				_mtx_Skew.ComplexInverse(parentWorldMatrix._mtx_Skew);
				_mtx_Skew.Subtract(localMatrix);
				_mtx_Skew._angleDeg_Step1 = apUtil.AngleTo180(_mtx_Skew._angleDeg_Step1);//Default의 각도는 180 이내로 제한된다.
				_mtx_Skew._angleDeg = _mtx_Skew._angleDeg_Step1;
				_mtx_Skew.MakeMatrix(false);
			}
		}


		public void SetWorld2Default(apBoneWorldMatrix parentWorldMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def.RInverse(parentWorldMatrix._mtx_Def, false);
				_mtx_Def._angleDeg = apUtil.AngleTo180(_mtx_Def._angleDeg);//Default의 각도는 180 이내로 제한된다.
				_mtx_Def.MakeMatrix(false);
			}
			else
			{

				

				//SInverse > Subtract로 연산하자
				_mtx_Skew.ComplexInverse(parentWorldMatrix._mtx_Skew);
				_mtx_Skew._angleDeg_Step1 = apUtil.AngleTo180(_mtx_Skew._angleDeg_Step1);//Default의 각도는 180 이내로 제한된다.
				_mtx_Skew._angleDeg = _mtx_Skew._angleDeg_Step1;

				_mtx_Skew.MakeMatrix(false);
			}
		}


		// Functions : Copy
		//-------------------------------------------------
		public void CopyFromMatrix(apBoneWorldMatrix srcBoneWorldMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def.SetMatrix(srcBoneWorldMatrix._mtx_Def, true);
			}
			else
			{
				_mtx_Skew.CopyFromComplexMatrix(srcBoneWorldMatrix._mtx_Skew);
			}
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
		public Vector2 ConvertForIK(Vector2 IKTargetPos)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				return IKTargetPos;//그대로 사용
			}
			else
			{
				return _mtx_Skew.MtrxToLowerSpace_Step2.MultiplyPoint(IKTargetPos);
				//return IKTargetPos;
			}
		}

		// Functions : Move/Rotate/Scale as Result < 이거 수정해야한다.
		//-------------------------------------------------
		//올바르게 동작하지 않아서 사용되지 않는 함수
		//나중에 지우자
		//public void RotateAsResult(float deltaAngle)
		//{
		//	if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
		//	{
		//		_mtx_Def._angleDeg += deltaAngle;
		//		_mtx_Def.MakeMatrix();
		//	}
		//	else
		//	{
		//		_mtx_Skew.RotateAsPostResult(deltaAngle);//여기서 MakeMatrix까지 호출함
		//	}
		//}

		public void MoveAsResult(Vector2 deltaPos)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def._pos += deltaPos;
				_mtx_Def.MakeMatrix();
			}
			else
			{
				_mtx_Skew.MoveAsPostResult(deltaPos);//이 함수에서 MakeMatrix까지 호출한다.
			}
		}

		//public void SetAngleAsResult(float angle)
		//{
		//	Debug.LogError("TODO : 이 함수는 정상적으로 동작하지 않을 가능성이 높다.");
		//	if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
		//	{
		//		_mtx_Def._angleDeg = angle;
		//		_mtx_Def.MakeMatrix();
		//	}
		//	else
		//	{
		//		//_mtx_Skew.SetAngleAsPostResult(angle);//이 함수에서 MakeMatrix까지 호출한다.
		//	}
		//}


		// Functions : Add / Subtract in Step1
		//-------------------------------------------------
		public void SubtractAsLocalValue(apMatrix localMatrix, bool isMakeMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def.Subtract(localMatrix);
				if(isMakeMatrix)
				{
					_mtx_Def.MakeMatrix();
				}
			}
			else
			{
				_mtx_Skew.Subtract(localMatrix);
				if(isMakeMatrix)
				{
					_mtx_Skew.MakeMatrix();
				}
			}
		}


		public void RotateAsStep1(float deltaAngle, bool isMakeMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def._angleDeg += deltaAngle;
				if(isMakeMatrix)
				{
					_mtx_Def.MakeMatrix();
				}
			}
			else
			{
				_mtx_Skew._angleDeg_Step1 += deltaAngle;
				if(isMakeMatrix)
				{
					_mtx_Skew.MakeMatrix();
				}
			}
		}

		public void SetAngleAsStep1(float angle, bool isMakeMatrix)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				_mtx_Def._angleDeg = angle;
				if(isMakeMatrix)
				{
					_mtx_Def.MakeMatrix();
				}
			}
			else
			{
				float angle_Step2 = _mtx_Skew._angleDeg - _mtx_Skew._angleDeg_Step1;//Step 2
				_mtx_Skew._angleDeg_Step1 = angle;
				
				if(_mtx_Skew._mode == apComplexMatrix.MODE.Step2_SMul)
				{
					_mtx_Skew._angleDeg = _mtx_Skew._angleDeg_Step1 + angle_Step2;
				}

				if(isMakeMatrix)
				{
					_mtx_Skew.MakeMatrix();
				}
			}
		}



		// Functions : Multiply Point
		//-------------------------------------------------
		public Vector2 MulPoint2(Vector2 point)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				return _mtx_Def.MulPoint2(point);
			}
			else
			{
				return _mtx_Skew.MulPoint2(point);
			}
		}

		public Vector2 InvMulPoint2(Vector2 point)
		{
			if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				return _mtx_Def.InvMulPoint2(point);
			}
			else
			{
				return _mtx_Skew.InvMulPoint2(point);
			}
		}

		// Get
		//-------------------------------------------------
		public Vector2 Pos
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def._pos;
				}
				else
				{
					return _mtx_Skew._pos;
				}
			}
		}

		

		public float Angle
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def._angleDeg;
				}
				else
				{
					return _mtx_Skew._angleDeg;
				}
			}
		}

		public Vector2 Scale
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def._scale;
				}
				else
				{
					return _mtx_Skew._scale;
				}
			}
		}


		public apMatrix3x3 MtrxToSpace
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def.MtrxToSpace;
				}
				else
				{
					return _mtx_Skew.MtrxToSpace;
				}
			}
		}

		public apMatrix3x3 MtrxToLowerSpace
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def.MtrxToLowerSpace;
				}
				else
				{
					return _mtx_Skew.MtrxToLowerSpace;
				}
			}
		}




		/// <summary>
		/// IK 계산시 사용되는 좌표계에서의 위치값.
		/// Def 모드에서는 일반 월드 좌표이며, Skew 모드에서는 Step1에서의 위치이다.
		/// </summary>
		public Vector2 Pos_IKSpace
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def._pos;
				}
				else
				{
					return _mtx_Skew._pos_Step1;
					//return _mtx_Skew._pos;
				}
			}
		}


		/// <summary>
		/// IK 계산시 사용되는 좌표계에서의 각도.
		/// Def 모드에서는 World 회전값과 동일하며, Skew 모드에서는 Step1에서의 각도이다.
		/// </summary>
		public float Angle_IKSpace
		{
			get
			{
				if(_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					return _mtx_Def._angleDeg;
				}
				else
				{
					return _mtx_Skew._angleDeg_Step1;
				}
			}
		}

		public bool Is1AxisFlipped()
		{
			if (_scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				//< 일반 모드 >
				return _mtx_Def.Is1AxisFlipped();
			}
			else
			{
				//< Skew 모드 >
				return _mtx_Skew.Is1AxisFlipped();
			}

			
		}

		// Static Functions : Make Parent World Matrix
		//-------------------------------------------------
		/// <summary>
		/// 에디터에서 새로운 본을 만들때 참고하는 부모 본 행렬을 임시로 만듭니다.
		/// 부모로서 참조되는 행렬은 "부모 본"으로서의 행렬이나 "부모 렌더 유닛"으로서의 행렬이므로,
		/// 거기에 해당되는 값을 입력.
		/// 우선순위는 ParentBoneMatrix가 우선됨
		/// </summary>
		/// <returns></returns>
		public static apBoneWorldMatrix MakeTempParentWorldMatrix(	int iTemp, apPortrait portrait, 
																apBoneWorldMatrix parentBoneMatrix, apMatrix parentRenderUnitMatrix)
		{
			apBoneWorldMatrix tempMatrix = GetTemp(iTemp, portrait);

			
			if(parentBoneMatrix != null)
			{
				tempMatrix.CopyFromMatrix(parentBoneMatrix);
			}
			else if(parentRenderUnitMatrix != null)
			{
				if(tempMatrix._scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					tempMatrix._mtx_Def.SetMatrix(parentRenderUnitMatrix, true);
				}
				else
				{
					//RenderUnit Matrix는 Step2로 적용한다. (SMultiply로 보기 때문)
					tempMatrix._mtx_Skew.SetMatrix_Step2(parentRenderUnitMatrix, true);
				}
			}

			return tempMatrix;
		}

		/// <summary>
		/// 본 행렬 계산시, 기존의 WorldMatrix를 복사하여 임시 행렬을 생성하는 함수.
		/// </summary>
		/// <returns></returns>
		public static apBoneWorldMatrix MakeTempWorldMatrix(	int iTemp, apPortrait portrait, apBoneWorldMatrix srcWorldMatrix)
		{
			apBoneWorldMatrix tempMatrix = GetTemp(iTemp, portrait);

			tempMatrix.CopyFromMatrix(srcWorldMatrix);

			return tempMatrix;
		}


		// Static Function : Make Default Matrix From World
		//-------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iTemp"></param>
		/// <param name="portrait"></param>
		/// <param name="srcWorldMatrix"></param>
		/// <param name="parentWorldMatrix"></param>
		/// <param name="localMatrix"></param>
		/// <returns></returns>
		public static apBoneWorldMatrix MakeDefaultMatrixFromWorld(
													int iTemp, apPortrait portrait,
													apBoneWorldMatrix srcWorldMatrix,
													apBoneWorldMatrix parentWorldMatrix,
													apMatrix localMatrix)
		{
			apBoneWorldMatrix tempMatrix = GetTemp(iTemp, portrait);

			tempMatrix.CopyFromMatrix(srcWorldMatrix);
			tempMatrix.SetWorld2Default(parentWorldMatrix, localMatrix);

			return tempMatrix;
		}
	}
}