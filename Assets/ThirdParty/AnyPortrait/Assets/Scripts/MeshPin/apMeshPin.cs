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
	/// v1.4.0 : 메시에 배치되는 Pin.
	/// 앞뒤로 Line으로 연결될 수 있고, Morph 모디파이어로 편집될 수 있다.
	/// 플레이어는 Transform이 아닌 위치만 제어할 수 있지만, Line 관계에 의해서 자동으로 회전한다.
	/// </summary>
	[Serializable]
	public class apMeshPin
	{
		// Members
		//-------------------------------------------------
		public int _uniqueID = -1;


		//굴곡 여부
		public enum TANGENT_TYPE : int
		{
			Smooth = 0,//부드럽게 움직이는 방식
			Sharp = 1,//각이 졌다.
		}

		[SerializeField]
		public TANGENT_TYPE _tangentType = TANGENT_TYPE.Smooth;


		//범위 설정

		//기본 위치
		[SerializeField]
		public Vector2 _defaultPos = Vector2.zero;

		[SerializeField]
		public float _defaultAngle = 0.0f;

		

		//연결 정보
		[NonSerialized]
		public apMeshPinGroup _parentPinGroup = null;

		//이전 / 다음 핀
		[SerializeField]
		public int _prevPinID = -1;

		[SerializeField]
		public int _nextPinID = -1;

		[NonSerialized]
		public apMeshPin _prevPin = null;

		[NonSerialized]
		public apMeshPin _nextPin = null;

		[NonSerialized]
		public apMeshPinCurve _prevCurve = null;

		[NonSerialized]
		public apMeshPinCurve _nextCurve = null;

		

		//범위 설정
		public int _range = 200;
		public int _fade = 100;


		//매트릭스 계산 (저장되진 않음)
		[NonSerialized, NonBackupField]
		public apMatrix3x3 _defaultMatrix;

		[NonSerialized, NonBackupField]
		public apMatrix3x3 _defaultMatrix_Inv;


		//사이드 컨트롤러 위치
		[NonSerialized]
		public Vector2 _controlPointPos_Def_Prev = Vector2.zero;

		[NonSerialized]
		public Vector2 _controlPointPos_Def_Next = Vector2.zero;




		
		


		/// <summary>
		/// 위치 타입. 실제로 저장되거나 참조되는 변수는 다르지만, 외부에서는 타입만 입력해서 가져올 수 있다.
		/// 단, Default만 직접 호출 가능
		/// </summary>
		public enum TMP_VAR_TYPE : int
		{
			/// <summary>메시 메뉴에서의 테스트값.</summary>
			MeshTest = 0,
			/// <summary>보간되지 않은 모디파이어 처리 도중의 값. 버텍스에 위치 지정시 사용된다.</summary>
			ModMid = 1,

			///// <summary>모디파이어, PKV가 모두 보간된 값</summary>
			//ModFinal = 2,

		}

		//임시 변수들 (3종)
		private const int TMP_VAR__MEST_TEST = 0;
		private const int TMP_VAR__MOD_MID = 1;
		//private const int TMP_VAR__MOD_FINAL = 2;
		//private const int NUM_TMP_VARS = 3;
		private const int NUM_TMP_VARS = 2;


		//테스트 모드에서의 결과
		//월드 좌표 : 단 메시 좌표계 기준이다.
		//Delta Pos를 바탕으로 계산된다.
		[NonSerialized] private Vector2[] _tmpPos = null;
		[NonSerialized] private float[] _tmpAngles = null;

		[NonSerialized] private apMatrix3x3[] _tmpMatrices = null;
		[NonSerialized] private apMatrix3x3[] _tmpVert2MeshMatrices = null;
		
		//사이드 컨트롤러 위치 ( 테스트 )
		[NonSerialized] private Vector2[] _controlPointPos_Tmp_Prev = null;
		[NonSerialized] private Vector2[] _controlPointPos_Tmp_Next = null;
		
		
		/// <summary>
		/// 핀 사이의 거리의 특정 비율의 거리에서 컨트롤 포인트가 생성된다. 0.5 이하여야 한다.
		/// </summary>
		public const float CONTROL_POINT_LENGTH_RATIO = 0.3f;

		/// <summary>
		/// 컨트롤 포인트가 인식하는 최소 거리
		/// </summary>
		public const float CONTROL_POINT_LENGTH_MIN = 0.1f;


		// Init
		//-------------------------------------------------
		public apMeshPin()
		{
			_defaultMatrix.SetTRS(_defaultPos, _defaultAngle, Vector2.one);
			_defaultMatrix_Inv = _defaultMatrix.inverse;

			_prevCurve = null;
			_nextCurve = null;


			//임시 변수 만들기
			Init_TmpVars();
		}

		/// <summary>
		/// 마우스 클릭으로 생성 직후 호출되는 함수
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <param name="posW"></param>
		/// <param name="parentPinGroup"></param>
		public void Init(int uniqueID, Vector2 posW, apMeshPinGroup parentPinGroup, int initRange, int initFade)
		{
			_uniqueID = uniqueID;
			_tangentType = TANGENT_TYPE.Smooth;
			_defaultPos = posW;
			_defaultAngle = 0.0f;
			
			_parentPinGroup = parentPinGroup;
			_prevPinID = -1;
			_nextPinID = -1;
			_prevPin = null;
			_nextPin = null;
			
			_range = initRange;
			_fade = initFade;

			//테스트 모드의 값들
			Init_TmpVars();

			RefreshDefaultMatrix();
		}



		public void LinkPinAsNext(apMeshPin nextPin)
		{
			_nextPinID = nextPin._uniqueID;
			_nextPin = nextPin;

			nextPin._prevPin = this;
			nextPin._prevPinID = _uniqueID;
		}


		// Default 행렬 / 컨트롤 포인트 계산
		public void RefreshDefaultMatrix()
		{	
			_defaultMatrix.SetTRS(_defaultPos, _defaultAngle, Vector2.one);
			_defaultMatrix_Inv = _defaultMatrix.inverse;
		}


		public void RefreshDefaultControlPoints()
		{
			float distToPrev = 10.0f;
			float distToNext = 10.0f;
			if(_prevPin != null)
			{
				distToPrev = Vector2.Distance(_prevPin._defaultPos, _defaultPos);
			}

			if(_nextPin != null)
			{
				distToNext = Vector2.Distance(_nextPin._defaultPos, _defaultPos);
			}
			
			if(distToPrev < CONTROL_POINT_LENGTH_MIN)
			{
				distToPrev = CONTROL_POINT_LENGTH_MIN;
			}

			if(distToNext < CONTROL_POINT_LENGTH_MIN)
			{
				distToNext = CONTROL_POINT_LENGTH_MIN;
			}


			distToPrev *= CONTROL_POINT_LENGTH_RATIO;
			distToNext *= CONTROL_POINT_LENGTH_RATIO;

			_controlPointPos_Def_Prev = _defaultMatrix.MultiplyPoint(new Vector2(-distToPrev, 0.0f));
			_controlPointPos_Def_Next = _defaultMatrix.MultiplyPoint(new Vector2(distToNext, 0.0f));
		}


		public float GetWeight_Default(Vector2 pos)
		{
			float dist = Vector2.Distance(pos, _defaultPos);
			
			if(dist < (float)_range)
			{
				return 1.0f;
			}
			if(_fade <= 0)
			{
				return 0.0f;
			}

			if(dist < (float)(_range + _fade))
			{
				return 1.0f - (dist - (float)_range) / (float)(_fade);
			}

			return 0.0f;
		}


		/// <summary>
		/// 임시 변수를 초기화한다. (Default와 같은 값을 갖도록 한다.)
		/// </summary>
		public void Init_TmpVars()
		{
			if(_tmpPos == null)					{ _tmpPos = new Vector2[NUM_TMP_VARS]; }
			if(_tmpAngles == null)				{ _tmpAngles = new float[NUM_TMP_VARS]; }
			if(_tmpMatrices == null)			{ _tmpMatrices = new apMatrix3x3[NUM_TMP_VARS]; }
			if(_tmpVert2MeshMatrices == null)	{ _tmpVert2MeshMatrices = new apMatrix3x3[NUM_TMP_VARS]; }
		
			if(_controlPointPos_Tmp_Prev == null)	{ _controlPointPos_Tmp_Prev = new Vector2[NUM_TMP_VARS]; }
			if(_controlPointPos_Tmp_Next == null)	{ _controlPointPos_Tmp_Next = new Vector2[NUM_TMP_VARS]; }

			apMatrix3x3 defMatrix = apMatrix3x3.TRS(_defaultPos, _defaultAngle, Vector2.one);
			apMatrix3x3 defMatrix_Inv = defMatrix * _defaultMatrix_Inv;

			for (int i = 0; i < NUM_TMP_VARS; i++)
			{
				_tmpPos[i] = i == TMP_VAR__MEST_TEST ? Vector2.zero : _defaultPos;
				_tmpAngles[i] = _defaultAngle;
				_tmpMatrices[i] = defMatrix;
				_tmpVert2MeshMatrices[i] = defMatrix_Inv;
				_controlPointPos_Tmp_Prev[i] = _controlPointPos_Def_Prev;
				_controlPointPos_Tmp_Next[i] = _controlPointPos_Def_Next;
			}
		}

		// Test 행렬 / 컨트롤 포인트 계산
		/// <summary>
		/// 지정된 Tmp 좌표를 Default와 동일하게 리셋한다. 커브 및 Prev/Next는 연결하지 않는다.
		/// </summary>
		public void Tmp_ResetMatrix(TMP_VAR_TYPE tmpVarType)
		{
			int iTmp = (int)tmpVarType;

			_tmpPos[iTmp] = tmpVarType == TMP_VAR_TYPE.MeshTest ? Vector2.zero : _defaultPos;
			_tmpAngles[iTmp] = _defaultAngle;

			_tmpMatrices[iTmp].SetTRS(_defaultPos, _defaultAngle, Vector2.one);
			
			//Vert > World 변환 매트릭스는
			//World * Def-1
			_tmpVert2MeshMatrices[iTmp] = _tmpMatrices[iTmp] * _defaultMatrix_Inv;
		}

		
		////테스트 모드의 값들
		//	_test_DeltaPos = Vector2.zero;
		//	_test_WorldPos = Vector2.zero;
		//	_test_WorldAngle = 0.0f;

		/// <summary>
		/// 커브 계산식을 통해서 World 좌표계를 입력하고 Matrix를 계산한다.
		/// </summary>
		/// <param name="resultWorldPos"></param>
		/// <param name="resultWorldAngle"></param>
		public void Tmp_CalculateWorldMatrix(TMP_VAR_TYPE tmpVarType, float resultWorldAngle)
		{
			int iTmp = (int)tmpVarType;

			_tmpAngles[iTmp] = resultWorldAngle;

			_tmpMatrices[iTmp].SetTRS((tmpVarType == TMP_VAR_TYPE.MeshTest ? (_tmpPos[iTmp] + _defaultPos) : _tmpPos[iTmp]), _tmpAngles[iTmp], Vector2.one);
			//Vert > World 변환 매트릭스는
			//World * Def-1
			_tmpVert2MeshMatrices[iTmp] = _tmpMatrices[iTmp] * _defaultMatrix_Inv;
		}

		/// <summary>
		/// [테스트 모드] 사이드 컨트롤 포인트를 계산한다.
		/// 핀들의 Test_CalculateWorldMatrix 함수를 일괄적으로 호출하고 이후 호출하자
		/// </summary>
		public void Tmp_RefreshControlPoints(TMP_VAR_TYPE tmpVarType)
		{
			int iTmp = (int)tmpVarType;


			float distToPrev = 10.0f;
			float distToNext = 10.0f;
			if(_prevPin != null)
			{
				if(tmpVarType == TMP_VAR_TYPE.MeshTest)
				{
					distToPrev = Vector2.Distance(_prevPin._tmpPos[iTmp] + _prevPin._defaultPos, _tmpPos[iTmp] + _defaultPos);
				}
				else
				{
					distToPrev = Vector2.Distance(_prevPin._tmpPos[iTmp], _tmpPos[iTmp]);
				}
				
			}

			if(_nextPin != null)
			{
				if(tmpVarType == TMP_VAR_TYPE.MeshTest)
				{
					distToNext = Vector2.Distance(_nextPin._tmpPos[iTmp] + _nextPin._defaultPos, _tmpPos[iTmp] + _defaultPos);
				}
				else
				{
					distToNext = Vector2.Distance(_nextPin._tmpPos[iTmp], _tmpPos[iTmp]);
				}
			}
			
			if(distToPrev < CONTROL_POINT_LENGTH_MIN)
			{
				distToPrev = CONTROL_POINT_LENGTH_MIN;
			}

			if(distToNext < CONTROL_POINT_LENGTH_MIN)
			{
				distToNext = CONTROL_POINT_LENGTH_MIN;
			}

			distToPrev *= CONTROL_POINT_LENGTH_RATIO;
			distToNext *= CONTROL_POINT_LENGTH_RATIO;

			_controlPointPos_Tmp_Prev[iTmp] = _tmpMatrices[iTmp].MultiplyPoint(new Vector2(-distToPrev, 0.0f));
			_controlPointPos_Tmp_Next[iTmp] = _tmpMatrices[iTmp].MultiplyPoint(new Vector2(distToNext, 0.0f));
		}


		// 임시 변수의 값을 리턴하자
		public Vector2 TmpPos_MeshTest	{ get { return _tmpPos[TMP_VAR__MEST_TEST] + _defaultPos; } }
		public Vector2 TmpPos_ModMid	{ get { return _tmpPos[TMP_VAR__MOD_MID]; } }
		//public Vector2 TmpPos_ModFinal	{ get { return _tmpPos[TMP_VAR__MOD_FINAL]; } }

		public float TmpAngle_MeshTest	{ get { return _tmpAngles[TMP_VAR__MEST_TEST]; } }
		public float TmpAngle_ModMid	{ get { return _tmpAngles[TMP_VAR__MOD_MID]; } }
		//public float TmpAngle_ModFinal	{ get { return _tmpAngles[TMP_VAR__MOD_FINAL]; } }

		public Vector2 TmpControlPos_Prev_MeshTest	{ get { return _controlPointPos_Tmp_Prev[TMP_VAR__MEST_TEST]; } }
		public Vector2 TmpControlPos_Prev_ModMid	{ get { return _controlPointPos_Tmp_Prev[TMP_VAR__MOD_MID]; } }
		//public Vector2 TmpControlPos_Prev_ModFinal	{ get { return _controlPointPos_Tmp_Prev[TMP_VAR__MOD_FINAL]; } }
		
		public Vector2 TmpControlPos_Next_MeshTest	{ get { return _controlPointPos_Tmp_Next[TMP_VAR__MEST_TEST]; } }
		public Vector2 TmpControlPos_Next_ModMid	{ get { return _controlPointPos_Tmp_Next[TMP_VAR__MOD_MID]; } }
		//public Vector2 TmpControlPos_Next_ModFinal	{ get { return _controlPointPos_Tmp_Next[TMP_VAR__MOD_FINAL]; } }

		public void SetTmpPos_MeshTest(Vector2 resultPos)	{ _tmpPos[TMP_VAR__MEST_TEST] = resultPos - _defaultPos; }
		public void SetTmpPos_ModMid(Vector2 resultPos)		{ _tmpPos[TMP_VAR__MOD_MID] = resultPos; }
		//public void SetTmpPos_ModFinal(Vector2 resultPos)	{ _tmpPos[TMP_VAR__MOD_FINAL] = resultPos; }


		/// <summary>
		/// 핀과 연결된 버텍스 위치를 행렬에 곱해서 변경되는 위치를 리턴한다. (Tmp 버전)
		/// </summary>
		/// <param name="tmpVarType"></param>
		/// <param name="vertPos"></param>
		/// <returns></returns>
		public Vector2 TmpMultiplyVertPos(TMP_VAR_TYPE tmpVarType, ref Vector2 vertPos)
		{
			return _tmpVert2MeshMatrices[(int)tmpVarType].MultiplyPoint(vertPos);
		}
	}
}