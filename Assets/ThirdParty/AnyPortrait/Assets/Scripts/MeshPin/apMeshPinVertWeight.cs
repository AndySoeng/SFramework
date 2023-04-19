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
	/// 핀과 버텍스간의 가중치 정보.
	/// 버텍스에 포함된다.
	/// 버텍스 > Pin 가중치를 연결하자 (버텍스 인덱스를 이용해서 직접 가중치 연결)
	/// 이 정보는 Pin에 저장된다.
	/// 가중치는 Prev Pin을 기준으로 동작하며, 커브 상에서 연결되었다면 NextPin과의 Lerp (t) 값을 같이 가진다.
	/// </summary>
	public class apMeshPinVertWeight
	{
		// Members
		//------------------------------------------------------
		[SerializeField]
		public int _vertID = -1;

		[NonSerialized]
		public apVertex _parentVert = null;

		[Serializable]
		public class VertPinPair
		{	
			public int _pinID = -1;

			[NonSerialized]
			public apMeshPin _linkedPin = null;

			[SerializeField] public float _weight = 0.0f;//이건 무조건 Normalized된 값이다.
			[SerializeField] public float _dist = 0.0f;
			//만약 커브에 연결되었다면
			[SerializeField] public bool _isCurveWeighted = false;//true면 Next Pin과의 커브 가중치를 계산한다. Lerp(t)를 같이 확인할 것
			[SerializeField] public float _curveLerp = 0.0f;

			

			//Default 에서의 커브 좌표를 미리 저장한다.
			[SerializeField] public apMatrix3x3 _curveDefaultMatrix;
			[SerializeField] public apMatrix3x3 _curveDefaultMatrix_Inv;

			public VertPinPair() { }

			public void InitSingle(	apMeshPin targetPin,
									float weight,
									float distToPin)
			{
				_pinID = targetPin._uniqueID;				
				_linkedPin = targetPin;

				_weight = weight;
				_dist = distToPin;
				
				//커브 정보는 비활성
				_isCurveWeighted = false;
				_curveLerp = 0.0f;

				_curveDefaultMatrix.SetIdentity();
				_curveDefaultMatrix_Inv.SetIdentity();
			}

			public void InitCurve(	apMeshPin targetPin,
									float weight,
									float distToCurve,
									float curveLerp,
									apMatrix3x3 curveDefaultMatrix
									)
			{
				_pinID = targetPin._uniqueID;
				_linkedPin = targetPin;

				_weight = weight;
				_dist = distToCurve;

				//커브 정보 활성
				_isCurveWeighted = true;
				_curveLerp = curveLerp;

				_curveDefaultMatrix = curveDefaultMatrix;
				_curveDefaultMatrix_Inv = curveDefaultMatrix.inverse;
			}


			/// <summary>
			/// 만약 저장된 정보가 다르다면 동기화를 위해 True를 리턴한다.
			/// </summary>
			/// <param name="targetPin"></param>
			/// <returns></returns>
			public bool Link(apMeshPin targetPin)
			{
				_linkedPin = targetPin;

				if(_linkedPin == null)
				{
					return true;
				}

				if(_isCurveWeighted && _linkedPin._nextCurve == null)
				{
					//커브와 연결되었다고 했는데 커브가 없넹?
					return true;
				}
				else if(!_isCurveWeighted && _linkedPin._nextCurve != null)
				{
					//커브가 없다고 했는데 커브가 있넹?
					return true;
				}
				return false;
			}


			public bool CheckNeedSync()
			{
				if(_linkedPin == null)
				{
					return true;
				}

				if(_isCurveWeighted && _linkedPin._nextCurve == null)
				{
					//커브와 연결되었다고 했는데 커브가 없넹?
					return true;
				}
				else if(!_isCurveWeighted && _linkedPin._nextCurve != null)
				{
					//커브가 없다고 했는데 커브가 있넹?
					return true;
				}
				return false;
			}
		}

		[SerializeField]
		public int _nPairs = 0;

		[SerializeField]
		public List<VertPinPair> _vertPinPairs = new List<VertPinPair>();

		[SerializeField]
		public float _totalWeight = 0.0f;//이건 1 미만일 수 있다.




		// Init
		//-----------------------------------------------------------------
		public apMeshPinVertWeight()
		{
			if(_vertPinPairs == null)
			{
				_vertPinPairs = new List<VertPinPair>();
			}
		}

		public void Init(apVertex parentVert)
		{
			_vertID = parentVert._uniqueID;
			_parentVert = parentVert;

			if(_vertPinPairs == null)
			{
				_vertPinPairs = new List<VertPinPair>();
			}
			
			_vertPinPairs.Clear();
			_nPairs = 0;
			_totalWeight = 0.0f;
		}

		/// <summary>
		/// 초기화를 한다.
		/// </summary>
		public void Clear()
		{
			if(_vertPinPairs == null)
			{
				_vertPinPairs = new List<VertPinPair>();
			}
			_vertPinPairs.Clear();
			_nPairs = 0;
			_totalWeight = 0.0f;
		}

		/// <summary>
		/// 버텍스-핀 그룹을 이용해서 VertPinPair를 연결한다.
		/// 만약 저장된 정보와 다르다면 true를 리턴한다.
		/// </summary>
		/// <param name="parentVert"></param>
		/// <param name="pinGroup"></param>
		/// <returns></returns>
		public bool Link(apVertex parentVert, apMeshPinGroup pinGroup)
		{	
			bool isAnyNeedResync = false;//다시 싱크를 해야하는가

			_parentVert = parentVert;

			_nPairs = _vertPinPairs != null ? _vertPinPairs.Count : 0;
			
			if(_nPairs > 0)
			{
				bool isAnyInvalidVert = false;
				VertPinPair curPair = null;
				for (int i = 0; i < _nPairs; i++)
				{
					curPair = _vertPinPairs[i];
					if(curPair._linkedPin == null)
					{
						//연결이 풀려있다면
						bool isNeedSyncPair = curPair.Link(pinGroup.GetPin(curPair._pinID));
						if(isNeedSyncPair)
						{
							//새로 동기화가 필요한 Pair가 발견되었다.
							isAnyNeedResync = true;
						}
					}
					else
					{
						//이미 연결되어 있어도 다시 체크를 한다. [v1.4.1]
						bool isNeedSyncPair = curPair.CheckNeedSync();
						if(isNeedSyncPair)
						{
							//새로 동기화가 필요한 Pair가 발견되었다.
							isAnyNeedResync = true;
						}
					}



					if(curPair._linkedPin == null)
					{
						isAnyInvalidVert = true;
					}
				}

				//유효하지 않은 데이터는 삭제한다.
				if(isAnyInvalidVert)
				{
					_vertPinPairs.RemoveAll(delegate(VertPinPair a)
					{
						return a._linkedPin == null;
					});

					_nPairs = _vertPinPairs.Count;
				}
			}

			return isAnyNeedResync;
		}

		public void AddWeight_Single(apMeshPin pin, 			
										float weight,
										float distToPin)
		{
			VertPinPair newPair = new VertPinPair();
			newPair.InitSingle(	pin,
								weight,
								distToPin);//Init에 Link 포함

			_vertPinPairs.Add(newPair);
		}

		public void AddWeight_Curve(apMeshPin pin,
									float weight,
									float distToCurve,
									float curveLerp,
									ref apMatrix3x3 curveDefaultMatrix)
		{
			VertPinPair newPair = new VertPinPair();
			newPair.InitCurve(pin, weight, distToCurve, curveLerp, curveDefaultMatrix);

			newPair._weight = weight;
			newPair._isCurveWeighted = true;
			newPair._curveLerp = curveLerp;

			_vertPinPairs.Add(newPair);
		}
		
		/// <summary>
		/// 가중치들을 계산한다.
		/// </summary>
		public void CompletePairs()
		{
			_nPairs = _vertPinPairs != null ? _vertPinPairs.Count : 0;
			if(_nPairs == 0)
			{
				_totalWeight = 0.0f;
				return;
			}

			VertPinPair curPair = null;
			_totalWeight = 0.0f;

			
			if (_nPairs > 1)
			{
				//만약 Pair가 2개 이상이라면, 거리의 역 비율에 따라 weight를 조정해야한다.
				float totalDist = 0.0f;
				for (int i = 0; i < _nPairs; i++)
				{
					curPair = _vertPinPairs[i];
					totalDist += curPair._dist;
				}

				if(totalDist < 0.001f)
				{
					totalDist = 0.001f;
				}

				for (int i = 0; i < _nPairs; i++)
				{
					curPair = _vertPinPairs[i];
					//거리가 멀 수록 Weight가 줄어야 한다.
					curPair._weight *= 1.0f - Mathf.Clamp01(curPair._dist / totalDist);

					_totalWeight += curPair._weight;
				}
			}
			else
			{
				//한개라면
				for (int i = 0; i < _nPairs; i++)
				{
					curPair = _vertPinPairs[i];
					_totalWeight += curPair._weight;
				}
			}
			
			
			

			//각각의 Weight는 무조건 Normalize한다.
			//전체 Weight는 최대 1이다.
			if (_totalWeight > 0.0f)
			{
				float normalizeRatio = 1.0f / _totalWeight;

				for (int i = 0; i < _nPairs; i++)
				{
					curPair = _vertPinPairs[i];
					curPair._weight *= normalizeRatio;
				}

				//총 Weight는 1 이하여야 한다.
				if (_totalWeight > 1.0f)
				{
					_totalWeight = 1.0f;
				}
			}
			else
			{
				_vertPinPairs.Clear();
				_nPairs = 0;
			}
		}


		// Calculate
		//------------------------------------------------------------------
		/// <summary>
		/// Test 모드의 Vertex 위치를 계산한다.
		/// </summary>
		/// <param name="srcVert"></param>
		public void Calculate_Test(apVertex srcVert)
		{
			if(_nPairs == 0)
			{
				//Pair가 없다면
			}
			else
			{

			}
		}

	}
}