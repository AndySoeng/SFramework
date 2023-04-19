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
	/// Pin에 연결되어서 커브 데이터를 알려준다.
	/// 저장되지는 않으며 Link시 생성된다.
	/// 연결이 안되었다면 생성되지 않는다.
	/// 커브 연산을 모아놓은 유틸 클래스에 가깝다.
	/// </summary>
	public class apMeshPinCurve
	{
		// Members
		//---------------------------------------------------
		public apMeshPin _prevPin;
		public apMeshPin _nextPin;

		// Init
		//---------------------------------------------------
		public apMeshPinCurve(apMeshPin prevPin, apMeshPin nextPin)
		{
			_prevPin = prevPin;
			_nextPin = nextPin;
		}

		//참고
		//베지어 커브
		//CP 3개, 2차식
		//: A(1-t)^2 + 2B(1-t)t + Ct^3

		//CP 4개, 3차식
		//: A(1-t)^3 + 3B(1-t)^2t + 3C(1-t)t^2 + Dt^3

		// Functions
		//---------------------------------------------------
		/// <summary>
		/// 둘다 Sharp인 직선 커브인가
		/// </summary>
		/// <returns></returns>
		public bool IsLinear()
		{
			return _prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp && _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp;
		}

		/// <summary>
		/// [Default] Lerp를 기준으로 위치를 가져온다.
		/// </summary>
		/// <param name="lerp"></param>
		/// <returns></returns>
		public Vector2 GetCurvePos_Default(float lerp)
		{
			lerp = Mathf.Clamp01(lerp);

			Vector2 resultPos = Vector2.zero;
			//Default 위치를 바탕으로 커브를 계산한다.
			//양쪽의 Tangent 타입을 확인하자
			if(_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//둘다 Sharp인 경우 > [직선]
				resultPos = (_prevPin._defaultPos * (1.0f - lerp)) + (_nextPin._defaultPos * lerp);
			}
			else if(_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth)
			{
				//Prev가 Sharp + Next가 Smooth인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				resultPos = (_prevPin._defaultPos * revLerp * revLerp)
							+ (2.0f * _nextPin._controlPointPos_Def_Prev * revLerp * lerp)
							+ (_nextPin._defaultPos * lerp * lerp);
			}
			else if(_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//Prev가 Smooth + Next가 Sharp인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				resultPos = (_prevPin._defaultPos * revLerp * revLerp)
							+ (2.0f * _prevPin._controlPointPos_Def_Next * revLerp * lerp)
							+ (_nextPin._defaultPos * lerp * lerp);
			}
			else
			{
				//둘다 Smooth인 경우 : 컨트롤 파라미터 4개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				resultPos = (_prevPin._defaultPos * revLerp * revLerp * revLerp)
							+ (3.0f * _prevPin._controlPointPos_Def_Next * revLerp * revLerp * lerp)
							+ (3.0f * _nextPin._controlPointPos_Def_Prev * revLerp * lerp * lerp)
							+ (_nextPin._defaultPos * lerp * lerp * lerp);
			}

			return resultPos;
		}




		/// <summary>
		/// [Default] Lerp를 기준으로 Matrix를 가져온다.
		/// </summary>
		/// <param name="lerp"></param>
		/// <returns></returns>
		public apMatrix3x3 GetCurveMatrix_Default(float lerp)
		{
			//이웃한 지점 (lerp +- offset)과의 위치를 비교하여 
			lerp = Mathf.Clamp01(lerp);

			float offset = 0.1f;
			Vector2 curPos = GetCurvePos_Default(lerp);
			
			//선형 보간에 의한 Angle (에러시 이용)
			float interpolatedAngle = apUtil.AngleTo180((_prevPin._defaultAngle * (1.0f - lerp)) + (_nextPin._defaultAngle * lerp));
			float angleToPrev = 0.0f;
			float angleToNext = 0.0f;

			Vector2 prevPos = Vector2.zero;

			if(lerp - offset < 0.0f)
			{
				//시작 부분이다.
				prevPos = _prevPin._controlPointPos_Def_Prev;
				//angleToPrev = _prevPin._defaultAngle;
			}
			else
			{
				prevPos = GetCurvePos_Default(lerp - offset);
			}

			Vector2 dirPrev2Cur = curPos - prevPos;
			if(dirPrev2Cur.sqrMagnitude > 0.0f)//길이가 충분하다면 
			{	
				angleToPrev = apUtil.AngleTo180(Mathf.Atan2(dirPrev2Cur.y, dirPrev2Cur.x) * Mathf.Rad2Deg);
			}
			else
			{
				angleToPrev = interpolatedAngle;//보간값을 이용
			}


			Vector2 nextPos = Vector2.zero;
			if(lerp + offset > 1.0f)
			{
				//끝부분이다.
				//angleToNext = _nextPin._defaultAngle;
				nextPos = _nextPin._controlPointPos_Def_Next;
			}
			else
			{
				nextPos = GetCurvePos_Default(lerp + offset);				
			}
			Vector2 dirCur2Next = nextPos - curPos;
			if(dirCur2Next.sqrMagnitude > 0.0f)//길이가 충분하다면 
			{
				
				angleToNext = apUtil.AngleTo180(Mathf.Atan2(dirCur2Next.y, dirCur2Next.x) * Mathf.Rad2Deg);
			}
			else
			{
				angleToNext = interpolatedAngle;//보간값을 이용
			}

			//합산해서 계산하자
			//float avgAngle = apUtil.AngleTo180((angleToPrev * 0.5f) + (angleToNext * 0.5f));
			float avgAngle = apUtil.AngleSlerp(angleToPrev, angleToNext, 0.5f);

			return apMatrix3x3.TRS(curPos, avgAngle, Vector2.one);
		}



		/// <summary>
		/// [Default] 빠른 비교를 위해 커브의 AABB를 리턴한다.
		/// </summary>
		/// <param name="minPos"></param>
		/// <param name="maxPos"></param>
		public void GetCurveAABBPos_Default(ref Vector2 minPos, ref Vector2 maxPos)
		{
			if (_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//둘다 Sharp인 경우 > [직선]
				minPos.x = Mathf.Min(_prevPin._defaultPos.x, _nextPin._defaultPos.x);
				minPos.y = Mathf.Min(_prevPin._defaultPos.y, _nextPin._defaultPos.y);

				maxPos.x = Mathf.Max(_prevPin._defaultPos.x, _nextPin._defaultPos.x);
				maxPos.y = Mathf.Max(_prevPin._defaultPos.y, _nextPin._defaultPos.y);
			}
			else if (_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth)
			{
				//Prev가 Sharp + Next가 Smooth인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				minPos.x = Mathf.Min(_prevPin._defaultPos.x, _nextPin._defaultPos.x, _nextPin._controlPointPos_Def_Prev.x);
				minPos.y = Mathf.Min(_prevPin._defaultPos.y, _nextPin._defaultPos.y, _nextPin._controlPointPos_Def_Prev.y);

				maxPos.x = Mathf.Max(_prevPin._defaultPos.x, _nextPin._defaultPos.x, _nextPin._controlPointPos_Def_Prev.x);
				maxPos.y = Mathf.Max(_prevPin._defaultPos.y, _nextPin._defaultPos.y, _nextPin._controlPointPos_Def_Prev.y);
			}
			else if (_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//Prev가 Smooth + Next가 Sharp인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				minPos.x = Mathf.Min(_prevPin._defaultPos.x, _nextPin._defaultPos.x, _prevPin._controlPointPos_Def_Next.x, _nextPin._controlPointPos_Def_Prev.x);
				minPos.y = Mathf.Min(_prevPin._defaultPos.y, _nextPin._defaultPos.y, _prevPin._controlPointPos_Def_Next.y, _nextPin._controlPointPos_Def_Prev.y);

				maxPos.x = Mathf.Max(_prevPin._defaultPos.x, _nextPin._defaultPos.x, _prevPin._controlPointPos_Def_Next.x, _nextPin._controlPointPos_Def_Prev.x);
				maxPos.y = Mathf.Max(_prevPin._defaultPos.y, _nextPin._defaultPos.y, _prevPin._controlPointPos_Def_Next.y, _nextPin._controlPointPos_Def_Prev.y);
			}
			else
			{
				//둘다 Smooth인 경우 : 컨트롤 파라미터 4개짜리 베지어 커브
				minPos.x = Mathf.Min(_prevPin._defaultPos.x, _nextPin._defaultPos.x, _prevPin._controlPointPos_Def_Next.x, _nextPin._controlPointPos_Def_Prev.x);
				minPos.y = Mathf.Min(_prevPin._defaultPos.y, _nextPin._defaultPos.y, _prevPin._controlPointPos_Def_Next.y, _nextPin._controlPointPos_Def_Prev.y);

				maxPos.x = Mathf.Max(_prevPin._defaultPos.x, _nextPin._defaultPos.x, _prevPin._controlPointPos_Def_Next.x, _nextPin._controlPointPos_Def_Prev.x);
				maxPos.y = Mathf.Max(_prevPin._defaultPos.y, _nextPin._defaultPos.y, _prevPin._controlPointPos_Def_Next.y, _nextPin._controlPointPos_Def_Prev.y);
			}
		}

		



		/// <summary>
		/// [Test] Lerp를 기준으로 위치를 가져온다.
		/// </summary>
		/// <param name="lerp"></param>
		/// <returns></returns>
		public Vector2 GetCurvePos_Test(apMeshPin.TMP_VAR_TYPE tmpVarType, float lerp)
		{
			lerp = Mathf.Clamp01(lerp);

			Vector2 resultPos = Vector2.zero;
			//Default 위치를 바탕으로 커브를 계산한다.
			//양쪽의 Tangent 타입을 확인하자
			if(_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//둘다 Sharp인 경우 > [직선]
				switch (tmpVarType)
				{
					case apMeshPin.TMP_VAR_TYPE.MeshTest:
						resultPos = (_prevPin.TmpPos_MeshTest * (1.0f - lerp)) + (_nextPin.TmpPos_MeshTest * lerp);
						break;

					case apMeshPin.TMP_VAR_TYPE.ModMid:
						resultPos = (_prevPin.TmpPos_ModMid * (1.0f - lerp)) + (_nextPin.TmpPos_ModMid * lerp);
						break;

					//case apMeshPin.TMP_VAR_TYPE.ModFinal:
					//	resultPos = (_prevPin.TmpPos_ModFinal * (1.0f - lerp)) + (_nextPin.TmpPos_ModFinal * lerp);
					//	break;
				}
				
			}
			else if(_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth)
			{
				//Prev가 Sharp + Next가 Smooth인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				switch (tmpVarType)
				{
					case apMeshPin.TMP_VAR_TYPE.MeshTest:
						resultPos =		(_prevPin.TmpPos_MeshTest * revLerp * revLerp)
										+ (2.0f * _nextPin.TmpControlPos_Prev_MeshTest * revLerp * lerp)
										+ (_nextPin.TmpPos_MeshTest * lerp * lerp);
						break;

					case apMeshPin.TMP_VAR_TYPE.ModMid:
						resultPos =		(_prevPin.TmpPos_ModMid * revLerp * revLerp)
										+ (2.0f * _nextPin.TmpControlPos_Prev_ModMid * revLerp * lerp)
										+ (_nextPin.TmpPos_ModMid * lerp * lerp);
						break;

					//case apMeshPin.TMP_VAR_TYPE.ModFinal:
					//	resultPos =		(_prevPin.TmpPos_ModFinal * revLerp * revLerp)
					//					+ (2.0f * _nextPin.TmpControlPos_Prev_ModFinal * revLerp * lerp)
					//					+ (_nextPin.TmpPos_ModFinal * lerp * lerp);
					//	break;
				}
				
			}
			else if(_prevPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth
				&& _nextPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//Prev가 Smooth + Next가 Sharp인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				switch (tmpVarType)
				{
					case apMeshPin.TMP_VAR_TYPE.MeshTest:
						resultPos =		(_prevPin.TmpPos_MeshTest * revLerp * revLerp)
										+ (2.0f * _prevPin.TmpControlPos_Next_MeshTest * revLerp * lerp)
										+ (_nextPin.TmpPos_MeshTest * lerp * lerp);
						break;

					case apMeshPin.TMP_VAR_TYPE.ModMid:
						resultPos =		(_prevPin.TmpPos_ModMid * revLerp * revLerp)
										+ (2.0f * _prevPin.TmpControlPos_Next_ModMid * revLerp * lerp)
										+ (_nextPin.TmpPos_ModMid * lerp * lerp);
						break;

					//case apMeshPin.TMP_VAR_TYPE.ModFinal:
					//	resultPos =		(_prevPin.TmpPos_ModFinal * revLerp * revLerp)
					//					+ (2.0f * _prevPin.TmpControlPos_Next_ModFinal * revLerp * lerp)
					//					+ (_nextPin.TmpPos_ModFinal * lerp * lerp);
					//	break;
				}
				
			}
			else
			{
				//둘다 Smooth인 경우 : 컨트롤 파라미터 4개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				switch (tmpVarType)
				{
					case apMeshPin.TMP_VAR_TYPE.MeshTest:
						resultPos =		(_prevPin.TmpPos_MeshTest * revLerp * revLerp * revLerp)
										+ (3.0f * _prevPin.TmpControlPos_Next_MeshTest * revLerp * revLerp * lerp)
										+ (3.0f * _nextPin.TmpControlPos_Prev_MeshTest * revLerp * lerp * lerp)
										+ (_nextPin.TmpPos_MeshTest * lerp * lerp * lerp);
						break;

					case apMeshPin.TMP_VAR_TYPE.ModMid:
						resultPos =		(_prevPin.TmpPos_ModMid * revLerp * revLerp * revLerp)
										+ (3.0f * _prevPin.TmpControlPos_Next_ModMid * revLerp * revLerp * lerp)
										+ (3.0f * _nextPin.TmpControlPos_Prev_ModMid * revLerp * lerp * lerp)
										+ (_nextPin.TmpPos_ModMid * lerp * lerp * lerp);
						break;

					//case apMeshPin.TMP_VAR_TYPE.ModFinal:
					//	resultPos =		(_prevPin.TmpPos_ModFinal * revLerp * revLerp * revLerp)
					//					+ (3.0f * _prevPin.TmpControlPos_Next_ModFinal * revLerp * revLerp * lerp)
					//					+ (3.0f * _nextPin.TmpControlPos_Prev_ModFinal * revLerp * lerp * lerp)
					//					+ (_nextPin.TmpPos_ModFinal * lerp * lerp * lerp);
					//	break;
				}
				
			}

			return resultPos;
		}




		/// <summary>
		/// [Test] Lerp를 기준으로 Matrix를 가져온다.
		/// </summary>
		/// <param name="lerp"></param>
		/// <returns></returns>
		public apMatrix3x3 GetCurveMatrix_Test(apMeshPin.TMP_VAR_TYPE tmpVarType, float lerp)
		{
			//이웃한 지점 (lerp +- offset)과의 위치를 비교하여 
			lerp = Mathf.Clamp01(lerp);

			float offset = 0.1f;
			Vector2 curPos = GetCurvePos_Test(tmpVarType, lerp);
			
			//선형 보간에 의한 Angle (에러시 이용)
			float prevPinAngle = 0.0f;
			float nextPinAngle = 0.0f;
			
			switch (tmpVarType)
			{
				case apMeshPin.TMP_VAR_TYPE.MeshTest:
					prevPinAngle = _prevPin.TmpAngle_MeshTest;
					nextPinAngle = _nextPin.TmpAngle_MeshTest;
					break;

				case apMeshPin.TMP_VAR_TYPE.ModMid:
					prevPinAngle = _prevPin.TmpAngle_ModMid;
					nextPinAngle = _nextPin.TmpAngle_ModMid;
					break;

				//case apMeshPin.TMP_VAR_TYPE.ModFinal:
				//	prevPinAngle = _prevPin.TmpAngle_ModFinal;
				//	nextPinAngle = _nextPin.TmpAngle_ModFinal;
				//	break;
			}


			float interpolatedAngle = apUtil.AngleTo180((prevPinAngle * (1.0f - lerp)) + (nextPinAngle * lerp));
			
			float angleToPrev = 0.0f;
			float angleToNext = 0.0f;

			Vector2 prevPos = Vector2.zero;
			if(lerp - offset < 0.0f)
			{
				//시작 부분이다.
				switch (tmpVarType)
				{
					case apMeshPin.TMP_VAR_TYPE.MeshTest:	prevPos = _prevPin.TmpControlPos_Prev_MeshTest; break;
					case apMeshPin.TMP_VAR_TYPE.ModMid:		prevPos = _prevPin.TmpControlPos_Prev_ModMid; break;
					//case apMeshPin.TMP_VAR_TYPE.ModFinal:	prevPos = _prevPin.TmpControlPos_Prev_ModFinal; break;
				}				
			}
			else
			{
				prevPos = GetCurvePos_Test(tmpVarType, lerp - offset);				
			}
			Vector2 dirPrev2Cur = curPos - prevPos;
			if(dirPrev2Cur.sqrMagnitude > 0.0f)
			{
				//길이가 충분하다면 
				angleToPrev = apUtil.AngleTo180(Mathf.Atan2(dirPrev2Cur.y, dirPrev2Cur.x) * Mathf.Rad2Deg);
			}
			else
			{
				angleToPrev = interpolatedAngle;//보간값을 이용
			}


			Vector2 nextPos = Vector2.zero;
			if(lerp + offset > 1.0f)
			{
				//끝부분이다.
				switch (tmpVarType)
				{
					case apMeshPin.TMP_VAR_TYPE.MeshTest:	nextPos = _nextPin.TmpControlPos_Next_MeshTest; break;
					case apMeshPin.TMP_VAR_TYPE.ModMid:		nextPos = _nextPin.TmpControlPos_Next_ModMid; break;
					//case apMeshPin.TMP_VAR_TYPE.ModFinal:	nextPos = _nextPin.TmpControlPos_Next_ModFinal; break;
				}
			}
			else
			{
				nextPos = GetCurvePos_Test(tmpVarType, lerp + offset);
			}

			Vector2 dirCur2Next = nextPos - curPos;
			if(dirCur2Next.sqrMagnitude > 0.0f)
			{
				//길이가 충분하다면 
				angleToNext = apUtil.AngleTo180(Mathf.Atan2(dirCur2Next.y, dirCur2Next.x) * Mathf.Rad2Deg);
			}
			else
			{
				angleToNext = interpolatedAngle;//보간값을 이용
			}



			//합산해서 계산하자			
			//float avgAngle = apUtil.AngleTo180((angleToPrev * 0.5f) + (angleToNext * 0.5f));
			float avgAngle = apUtil.AngleSlerp(angleToPrev, angleToNext, 0.5f);

			return apMatrix3x3.TRS(curPos, avgAngle, Vector2.one);
		}
		

		
	}
}