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
	//추가 19.8.8
	/// <summary>
	/// 여러개의 VertRig를 복사하고 붙여넣는 (Pos-Copy, Pos-Paste) 기능을 구현하기 위한 클래스
	/// apSnapShotBase의 상속을 받지 않는다. (별도로 계산)
	/// </summary>
	public class apSnapShot_MultipleVertRig
	{
		// Sub Class
		//-----------------------------------------------------
		public class PosWeightPair
		{
			public Vector2 _posWorld = Vector2.zero;
			public class WeightPairData
			{
				public apBone _bone = null;
				public float _weight = 0.0f;

				public WeightPairData(apBone bone, float weight)
				{
					_bone = bone;
					_weight = weight;
				}
			}

			public List<WeightPairData> _weightPairs = new List<WeightPairData>();

			public PosWeightPair()
			{
				if(_weightPairs == null)
				{
					_weightPairs = new List<WeightPairData>();
				}
				_weightPairs.Clear();
			}

			public bool IsApproxPos(Vector2 targetPosW)
			{
				if(Mathf.Approximately(targetPosW.x, _posWorld.x) &&
					Mathf.Approximately(targetPosW.y, _posWorld.y))
				{
					return true;
				}
				return false;
			}

			//ModVertRig > WeightPairData
			public void SetModVertRig(apModifiedVertexRig modVertRig)
			{
				//위치 저장 (Modifier 적용 없이)
				_posWorld = modVertRig._renderVertex._pos_World_NoMod;
				

				//WeightPair 리스트를 저장한다.
				if (_weightPairs == null)
				{
					_weightPairs = new List<WeightPairData>();
				}
				_weightPairs.Clear();

				
				if(modVertRig._weightPairs.Count == 0)
				{
					return;
				}

				float totalWeight = 0.0f;
				for (int iSrc = 0; iSrc < modVertRig._weightPairs.Count; iSrc++)
				{
					apModifiedVertexRig.WeightPair srcWeightPair = modVertRig._weightPairs[iSrc];
					if(srcWeightPair._bone == null)
					{
						continue;
					}

					totalWeight += srcWeightPair._weight;
					_weightPairs.Add(new WeightPairData(srcWeightPair._bone, srcWeightPair._weight));
				}
				
				if(_weightPairs.Count > 0 && totalWeight > 0.0f)
				{
					//Normalize를 하자
					for (int i = 0; i < _weightPairs.Count; i++)
					{
						_weightPairs[i]._weight /= totalWeight;
					}
				}

				//Debug.Log("Pos-Copy : " + _posWorld + " / Pairs : " + _weightPairs.Count);
			}

			//저장된 값을 대상 ModVertRig에 복사한다.
			public void PasteToModVertRig(apMeshGroup keyMeshGroup, apModifiedVertexRig targetModVertRig)
			{
				if (targetModVertRig == null)
				{
					return;
				}


				targetModVertRig._weightPairs.Clear();

				for (int iSrcPair = 0; iSrcPair < _weightPairs.Count; iSrcPair++)
				{
					WeightPairData srcPair = _weightPairs[iSrcPair];
					apModifiedVertexRig.WeightPair dstPair = new apModifiedVertexRig.WeightPair(srcPair._bone);

					//이전 코드 > 자식 메시 그룹의 본을 대상으로 하는 경우 버그가 발생한다.
					//버그 조건 : 부모 메시 그룹의 Rigging Modifier > 자식 메시 그룹의 본을 대상 >> Bake 에러
					dstPair._meshGroup = keyMeshGroup;
					dstPair._meshGroupID = keyMeshGroup._uniqueID;

					//변경 21.7.19
					//keyMeshGroup이 Bone을 가진 MeshGroup이 아닌 Parent나 Root 메시 그룹인 경우가 있다.
					//이러면 Bake에서 에러가 발생한다.
					//실제 Bone의 MeshGroup을 대상으로 해야한다.
					//보정 코드를 추가한다.
					if (srcPair._bone._meshGroup != null)
					{
						//이미 연결된 본의 메시 그룹이 있다면 KeyMeshGroup이 아닌 이걸 사용하자
						dstPair._meshGroup = srcPair._bone._meshGroup;
						dstPair._meshGroupID = srcPair._bone._meshGroup._uniqueID;
					}


					dstPair._weight = srcPair._weight;

					targetModVertRig._weightPairs.Add(dstPair);
				}
			}

			public void AddPosWeightPair(PosWeightPair srcPosWeightPair, float weight)
			{
				WeightPairData srcWeightPairData = null;
				WeightPairData targetWeightPairData = null;
				for (int i = 0; i < srcPosWeightPair._weightPairs.Count; i++)
				{
					srcWeightPairData = srcPosWeightPair._weightPairs[i];
					if(srcWeightPairData._bone == null)
					{
						continue;
					}

					//동일한 Bone의 기록이 있다면 Weight 더하기
					//없다면 새로운 기록 만들기
					targetWeightPairData = null;
					targetWeightPairData = _weightPairs.Find(delegate(WeightPairData a)
					{
						return a._bone == srcWeightPairData._bone;
					});

					if(targetWeightPairData != null)
					{
						//동일한 Bone에 대한 기록이 있다면 Weight 더하기
						targetWeightPairData._weight += srcWeightPairData._weight * weight;
					}
					else
					{
						//새로운 기록 만들기
						targetWeightPairData = new WeightPairData(srcWeightPairData._bone, srcWeightPairData._weight * weight);
						_weightPairs.Add(targetWeightPairData);
					}
				}
			}

			public void Normalize()
			{
				//Weight를 Normalize하자.
				if(_weightPairs.Count == 0)
				{
					return;
				}

				float totalWeight = 0.0f;
				for (int i = 0; i < _weightPairs.Count; i++)
				{
					totalWeight += _weightPairs[i]._weight;
				}

				if (totalWeight > 0.0f)
				{
					for (int i = 0; i < _weightPairs.Count; i++)
					{
						_weightPairs[i]._weight /= totalWeight;
					}
				}
			}

			//Lerp가 0이면 A에 100%
			public void Lerp(PosWeightPair srcPosA, PosWeightPair srcPosB, float lerp)
			{
				_posWorld = srcPosA._posWorld * (1.0f - lerp) + srcPosB._posWorld * lerp;
				
				float weightA = (1.0f - lerp);
				float weightB = lerp;

				_weightPairs.Clear();

				//A와 B의 PairData를 weight를 이용하여 대입한다.
				WeightPairData srcData = null;
				WeightPairData dstData = null;

				//일단 A부터
				for (int i = 0; i < srcPosA._weightPairs.Count; i++)
				{
					srcData = srcPosA._weightPairs[i];
					
					//Bone이 있는지 확인
					dstData = _weightPairs.Find(delegate(WeightPairData a)
					{
						return a._bone == srcData._bone;
					});

					if(dstData != null)
					{
						dstData._weight += srcData._weight * weightA;
					}
					else
					{
						dstData = new WeightPairData(srcData._bone, srcData._weight * weightA);
						_weightPairs.Add(dstData);
					}
				}

				//B도 적용
				for (int i = 0; i < srcPosB._weightPairs.Count; i++)
				{
					srcData = srcPosB._weightPairs[i];
					
					//Bone이 있는지 확인
					dstData = _weightPairs.Find(delegate(WeightPairData a)
					{
						return a._bone == srcData._bone;
					});

					if(dstData != null)
					{
						dstData._weight += srcData._weight * weightB;
					}
					else
					{
						dstData = new WeightPairData(srcData._bone, srcData._weight * weightB);
						_weightPairs.Add(dstData);
					}
				}

				//마지막으로 Normalize
				Normalize();
			}


			public void Interpolation3_NotTri(PosWeightPair srcPosA, PosWeightPair srcPosB, PosWeightPair srcPosC, Vector2 targetPosW)
			{
				float dstA = Vector2.Distance(srcPosA._posWorld, targetPosW);
				float dstB = Vector2.Distance(srcPosB._posWorld, targetPosW);
				float dstC = Vector2.Distance(srcPosC._posWorld, targetPosW);

				float totalDst = dstA + dstB + dstC;

				float weightA = 1.0f;
				float weightB = 1.0f;
				float weightC = 1.0f;
				float totalWeight = 0.0f;

				if (totalDst > 0.0f)
				{
					weightA = 1.0f - (dstA / totalDst);
					weightB = 1.0f - (dstB / totalDst);
					weightC = 1.0f - (dstC / totalDst);
				}

				totalWeight = weightA + weightB + weightC;
				weightA /= totalWeight;
				weightB /= totalWeight;
				weightC /= totalWeight;

				_posWorld = targetPosW;

				AddPosWeightPair(srcPosA, weightA);
				AddPosWeightPair(srcPosB, weightB);
				AddPosWeightPair(srcPosC, weightC);

				Normalize();
			}

		}
		

		// Members
		//-------------------------------------------------------------
		public apMeshGroup _keyMeshGroup = null;
		public List<PosWeightPair> _posWeightPairs = new List<PosWeightPair>();

		// Init
		//-------------------------------------------------------------
		public apSnapShot_MultipleVertRig()
		{
			Clear();
		}

		public void Clear()
		{
			_keyMeshGroup = null;
			if(_posWeightPairs == null)
			{
				_posWeightPairs = new List<PosWeightPair>();
			}
			_posWeightPairs.Clear();
		}

		// Functions
		//-------------------------------------------------------------
		public bool IsPastable(apMeshGroup meshGroup)
		{
			if(_keyMeshGroup == null || _keyMeshGroup != meshGroup || _posWeightPairs.Count == 0)
			{
				return false;
			}
			return true;
		}

		public bool Copy(apMeshGroup meshGroup, List<apModifiedVertexRig> srcModVertRigs)
		{
			if(meshGroup == null ||
				srcModVertRigs == null ||
				srcModVertRigs.Count == 0)
			{
				return false;
			}

			Clear();

			_keyMeshGroup = meshGroup;

			//Debug.LogWarning("Pos-Copy Start");
			for (int i = 0; i < srcModVertRigs.Count; i++)
			{
				PosWeightPair newPosWeightPair = new PosWeightPair();
				newPosWeightPair.SetModVertRig(srcModVertRigs[i]);

				_posWeightPairs.Add(newPosWeightPair);
			}
			//Debug.LogWarning("Result : " + _posWeightPairs.Count);

			return true;
		}

		public bool Paste(apMeshGroup meshGroup, List<apModifiedVertexRig> targetModVertRigs)
		{
			if (meshGroup == null ||
				targetModVertRigs == null ||
				targetModVertRigs.Count == 0)
			{
				return false;
			}
			if (_keyMeshGroup == null || _keyMeshGroup != meshGroup || _posWeightPairs.Count == 0)
			{
				return false;
			}

			//Debug.LogError("Paste Start----");

			apModifiedVertexRig targetModVertRig = null;
			Vector2 targetPosW = Vector2.zero;

			PosWeightPair src = null;
			Vector2 srcPosW = Vector2.zero;

			PosWeightPair near_mX = null;
			PosWeightPair near_pX = null;
			PosWeightPair near_mY = null;
			PosWeightPair near_pY = null;
			float minDst_mX = 0.0f;
			float minDst_pX = 0.0f;
			float minDst_mY = 0.0f;
			float minDst_pY = 0.0f;
			float dst = 0.0f;

			//알고리즘 변경
			//1. 일단 -X, +X, -Y, +Y 방향으로 가장 가까운 포인트를 찾는다. (축값이 아닌 실제 Dst임)
			//2. 최대 4개의 포인트에 대하여 "최대 거리"를 구한다
			//3. 4개의 포인트를 포함하여 최대 거리 안에 포함된 포인트들을 리스트에 넣는다.
			//4. 포인트를 3개씩 묶어서 "삼각형"이 되어 이 점을 포함하는지 확인
			//5. "삼각형"이 되는 경우, 전체 Dist의 합이 가장 작은 삼각형을 선택한다.
			//6. 최적화된 삼각형을 찾은 상태에서 Barycentric 방식으로 보간한다.


			for (int iTarget = 0; iTarget < targetModVertRigs.Count; iTarget++)
			{
				targetModVertRig = targetModVertRigs[iTarget];
				targetPosW = targetModVertRig._renderVertex._pos_World_NoMod;

				near_mX = null;
				near_pX = null;
				near_mY = null;
				near_pY = null;
				minDst_mX = 0.0f;
				minDst_pX = 0.0f;
				minDst_mY = 0.0f;
				minDst_pY = 0.0f;

				for (int iSrc = 0; iSrc < _posWeightPairs.Count; iSrc++)
				{
					src = _posWeightPairs[iSrc];
					srcPosW = src._posWorld;

					dst = Vector2.Distance(srcPosW, targetPosW);

					if(srcPosW.x < targetPosW.x)
					{
						//-X에 대해서
						if(dst < minDst_mX || near_mX == null)
						{
							near_mX = src;
							minDst_mX = dst;
						}
					}
					else
					{
						//+X에 대해서
						if(dst < minDst_pX || near_pX == null)
						{
							near_pX = src;
							minDst_pX = dst;
						}
					}

					if(srcPosW.y < targetPosW.y)
					{
						//-Y에 대해서
						if(dst < minDst_mY || near_mY == null)
						{
							near_mY = src;
							minDst_mY = dst;
						}
					}
					else
					{
						//+Y에 대해서
						if(dst < minDst_pY || near_pY == null)
						{
							near_pY = src;
							minDst_pY = dst;
						}
					}
				}

				//아주 가까운 점이 있다면 그 점을 그대로 대입한다.
				if(near_mX != null && near_mX.IsApproxPos(targetPosW))
				{
					near_mX.PasteToModVertRig(meshGroup, targetModVertRig);
					//Debug.Log(">> Case0 : 1개의 겹치는 점 대입");
					continue;
				}
				if(near_pX != null && near_pX.IsApproxPos(targetPosW))
				{
					near_pX.PasteToModVertRig(meshGroup, targetModVertRig);
					//Debug.Log(">> Case0 : 1개의 겹치는 점 대입");
					continue;
				}
				if(near_mY != null && near_mY.IsApproxPos(targetPosW))
				{
					near_mY.PasteToModVertRig(meshGroup, targetModVertRig);
					//Debug.Log(">> Case0 : 1개의 겹치는 점 대입");
					continue;
				}
				if(near_pY != null && near_pY.IsApproxPos(targetPosW))
				{
					near_pY.PasteToModVertRig(meshGroup, targetModVertRig);
					//Debug.Log(">> Case0 : 1개의 겹치는 점 대입");
					continue;
				}

				//2. 최대 4개의 포인트에 대하여 "최대 거리"를 구한다
				//3. 4개의 포인트를 포함하여 최대 거리 안에 포함된 포인트들을 리스트에 넣는다.


				List<PosWeightPair> pairList = new List<PosWeightPair>();
				float maxDist = 0.0f;

				if(near_mX != null)									{ pairList.Add(near_mX); maxDist = Mathf.Max(minDst_mX, maxDist); }
				if(near_pX != null && !pairList.Contains(near_pX))	{ pairList.Add(near_pX); maxDist = Mathf.Max(minDst_pX, maxDist); }
				if(near_mY != null && !pairList.Contains(near_mY))	{ pairList.Add(near_mY); maxDist = Mathf.Max(minDst_mY, maxDist); }
				if(near_pY != null && !pairList.Contains(near_pY))	{ pairList.Add(near_pY); maxDist = Mathf.Max(minDst_pY, maxDist); }

				if(pairList.Count < 3)
				{
					maxDist *= 2;
				}
				//이제 다시 돌면서 maxDist 거리 안에 있는 모든 포인트를 리스트에 넣는다.
				for (int iSrc = 0; iSrc < _posWeightPairs.Count; iSrc++)
				{
					src = _posWeightPairs[iSrc];

					if(pairList.Contains(src))
					{
						//이미 추가되었다.
						continue;
					}
					srcPosW = src._posWorld;

					dst = Vector2.Distance(srcPosW, targetPosW);
					if(dst < maxDist)
					{
						pairList.Add(src);
					}
				}

				if(pairList.Count == 0)
				{
					//1개도 없다면
					continue;
				}

				if (pairList.Count == 1)
				{
					//가까운 점이 1개라면
					//그대로 대입
					pairList[0].PasteToModVertRig(meshGroup, targetModVertRig);

					//Debug.Log(">> Case1 : 1개의 점 대입");
					continue;
				}

				PosWeightPair pairResult = new PosWeightPair();

				if (pairList.Count == 2)
				{
					//4-1. 가까운 점이 2개라면
					//두개의 거리 비로 계산한다.
					PosWeightPair pairA = pairList[0];
					PosWeightPair pairB = pairList[0];
					float dstA = Vector2.Distance(pairA._posWorld, targetPosW);
					float dstB = Vector2.Distance(pairB._posWorld, targetPosW);

					if (dstA + dstB > 0.0f)
					{	
						pairResult.Lerp(pairA, pairB, dstA / (dstA + dstB));
					}
					else
					{
						pairResult.Lerp(pairA, pairB, 0.5f);
					}
					pairResult.PasteToModVertRig(meshGroup, targetModVertRig);

					//Debug.Log(">> Case2 : 2개의 점 대입");
					continue;
				}
				//4-2. 포인트를 3개씩 묶어서 "삼각형"이 되어 이 점을 포함하는지 확인
				
				//Recursive 함수를 이용하자
				List<TriangleSet> triSet = GetTriangleSet_LV1(targetPosW, pairList, 0);

				if(triSet.Count == 0)
				{
					//삼각형 메시에 포함되지 않았다면,
					//가장 가까운 3개의 점을 보간하자
					pairList.Sort(delegate(PosWeightPair a, PosWeightPair b)
					{
						float dstA = Vector2.Distance(a._posWorld, targetPosW);
						float dstB = Vector2.Distance(b._posWorld, targetPosW);

						return (int)((dstA - dstB) * 1000.0f);
					});
					
					pairResult.Interpolation3_NotTri(pairList[0], pairList[1], pairList[2], targetPosW);
					pairResult.PasteToModVertRig(meshGroup, targetModVertRig);

					//Debug.Log(">> Case3 : 삼각형이 안되는 3개의 점 대입");
					continue;
				}

				//5. "삼각형"이 되는 경우, 전체 Dist의 합이 가장 작은 삼각형을 선택한다.
				TriangleSet minTriSet = null;
				float minTriSize = 0.0f;

				TriangleSet curTriSet = null;
				for (int iTri = 0; iTri < triSet.Count; iTri++)
				{
					curTriSet = triSet[iTri];
					if(minTriSet == null || curTriSet._totalDist < minTriSize)
					{
						minTriSet = curTriSet;
						minTriSize = curTriSet._totalDist;
					}
				}
				
				//6. 최적화된 삼각형을 찾은 상태에서 Barycentric 방식으로 보간한다.
				minTriSet.CalculateBaryCentricWeights(targetPosW);
				pairResult.AddPosWeightPair(minTriSet._pair_A, minTriSet._baryCentricWeightA);
				pairResult.AddPosWeightPair(minTriSet._pair_B, minTriSet._baryCentricWeightB);
				pairResult.AddPosWeightPair(minTriSet._pair_C, minTriSet._baryCentricWeightC);
				pairResult.Normalize();

				pairResult.PasteToModVertRig(meshGroup, targetModVertRig);

				//Debug.Log(">> Case4 : 삼각형 보간 대입");
			}

			return true;
		}


		private class TriangleSet
		{
			public PosWeightPair _pair_A = null;
			public PosWeightPair _pair_B = null;
			public PosWeightPair _pair_C = null;
			public float _distA = 0.0f;
			public float _distB = 0.0f;
			public float _distC = 0.0f;
			public float _angleAB = 0.0f;
			public float _angleBC = 0.0f;
			public float _angleCA = 0.0f;
			public float _totalDist = 0.0f;

			public float _baryCentricWeightA = 0.0f;
			public float _baryCentricWeightB = 0.0f;
			public float _baryCentricWeightC = 0.0f;

			public TriangleSet(PosWeightPair pairA, PosWeightPair pairB, PosWeightPair pairC, Vector2 targetPosW, float angleAB, float angleBC, float angleCA)
			{
				_pair_A = pairA;
				_pair_B = pairB;
				_pair_C = pairC;

				_distA = Vector2.Distance(targetPosW, pairA._posWorld);
				_distB = Vector2.Distance(targetPosW, pairB._posWorld);
				_distC = Vector2.Distance(targetPosW, pairC._posWorld);

				_angleAB = angleAB;
				_angleBC = angleBC;
				_angleCA = angleCA;

				_totalDist = _distA + _distB + _distC;
			}

			public void CalculateBaryCentricWeights(Vector2 targetPosW)
			{
				Vector2 posA = _pair_A._posWorld;
				Vector2 posB = _pair_B._posWorld;
				Vector2 posC = _pair_C._posWorld;

				_baryCentricWeightA = 0.0f;
				_baryCentricWeightB = 0.0f;
				_baryCentricWeightC = 0.0f;

				float paramL = (posB.y - posC.y) * (posA.x - posC.x) + (posC.x - posB.x) * (posA.y - posC.y);
				if(paramL == 0.0f)
				{
					_baryCentricWeightA = 0.5f;
					_baryCentricWeightB = 0.5f;
					_baryCentricWeightC = 0.0f;
					return;
				}

				_baryCentricWeightA = (posB.y - posC.y) * (targetPosW.x - posC.x) + (posC.x - posB.x) * (targetPosW.y - posC.y);
				_baryCentricWeightA = Mathf.Clamp01(_baryCentricWeightA / paramL);

				_baryCentricWeightB = (posC.y - posA.y) * (targetPosW.x - posC.x) + (posA.x - posC.x) * (targetPosW.y - posC.y);
				_baryCentricWeightB = Mathf.Clamp01(_baryCentricWeightB / paramL);

				_baryCentricWeightC = Mathf.Clamp01(1.0f - (_baryCentricWeightA + _baryCentricWeightB));
			}
		}

		private List<TriangleSet> GetTriangleSet_LV1(Vector2 targetPosW, List<PosWeightPair> pairList, int startIndex)
		{	
			if(startIndex >= pairList.Count)
			{
				return null;
			}

			PosWeightPair curPair = null;
			List<TriangleSet> triSetList = new List<TriangleSet>();

			for (int i = startIndex; i < pairList.Count; i++)
			{
				curPair = pairList[i];
				//1개의 포인트 선택
				//두번째 포인트를 찾자
				GetTriangleSet_LV2(targetPosW, pairList, curPair, i + 1, triSetList);
			}


			return triSetList;
		}

		private void GetTriangleSet_LV2(Vector2 targetPosW, List<PosWeightPair> pairList, PosWeightPair pair1, int startIndex, List<TriangleSet> resultTriList)
		{
			if(startIndex >= pairList.Count)
			{
				return;
			}

			PosWeightPair curPair = null;

			for (int i = startIndex; i < pairList.Count; i++)
			{
				curPair = pairList[i];
				//2개의 포인트 선택
				//세번째 포인트를 찾자
				GetTriangleSet_LV3(targetPosW, pairList, pair1, curPair, i + 1, resultTriList);
			}
		}

		private void GetTriangleSet_LV3(Vector2 targetPosW, List<PosWeightPair> pairList, PosWeightPair pair1, PosWeightPair pair2, int startIndex, List<TriangleSet> resultTriList)
		{
			if(startIndex >= pairList.Count)
			{
				return;
			}

			PosWeightPair pair3 = null;

			for (int i = startIndex; i < pairList.Count; i++)
			{
				pair3 = pairList[i];

				//3개의 포인트가 다 모였다.
				//targetPosW를 기준으로 삼각형이 되는지 확인하자.
				bool isTriCondition = false;

				float angle12 = Vector2.Angle(pair1._posWorld - targetPosW, pair2._posWorld - targetPosW);
				float angle23 = Vector2.Angle(pair2._posWorld - targetPosW, pair3._posWorld - targetPosW);
				float angle31 = Vector2.Angle(pair3._posWorld - targetPosW, pair1._posWorld - targetPosW);

				float angleSum = angle12 + angle23 + angle31;
				if(Mathf.Abs(angleSum - 360.0f) < 2.0f)
				{
					//대략 삼각형 안에 포인트가 있는 듯 하다.
					isTriCondition = true;
				}

				if(!isTriCondition)
				{
					continue;
				}

				//삼각형 메시를 구했다.
				TriangleSet newTriSet = new TriangleSet(pair1, pair2, pair3, targetPosW, angle12, angle23, angle31);
				resultTriList.Add(newTriSet);
			}
		}
















		#region [미사용 코드]
		//public bool Paste_Old(apMeshGroup meshGroup, List<apModifiedVertexRig> targetModVertRigs)
		//{
		//	if(meshGroup == null ||
		//		targetModVertRigs == null ||
		//		targetModVertRigs.Count == 0)
		//	{
		//		return false;
		//	}
		//	if(_keyMeshGroup == null || _keyMeshGroup != meshGroup || _posWeightPairs.Count == 0)
		//	{
		//		return false;
		//	}

		//	Debug.LogError("Pos-Paste Start");

		//	//대상 버텍스를 기준으로 계산한다.
		//	//1. TargetModVertRig를 하나 선택한다.
		//	//2. 저장된 포인트를 기준으로 L, T, R, B 방향으로 가장 가까운 포인트를 찾는다.
		//	//3-1. 만약 1개의 포인트만 발견되었다면 그 값을 복사한다. > 끝
		//	//3-2. LT, RT, LB, RB에 대해서 보간용 포인트를 만든다. 실제 포인트 또는 가상 포인트를 이용 > 4로 이동
		//	//4. Bi-Linear 방식으로 포인트 보간을 하고 값을 복사한다.

		//	apModifiedVertexRig targetModVertRig = null;
		//	Vector2 targetPosW = Vector2.zero;

		//	PosWeightPair src = null;
		//	Vector2 srcPosW = Vector2.zero;

		//	PosWeightPair nearL = null;
		//	PosWeightPair nearT = null;
		//	PosWeightPair nearR = null;
		//	PosWeightPair nearB = null;
		//	float minDst_L = 0.0f;
		//	float minDst_T = 0.0f;
		//	float minDst_R = 0.0f;
		//	float minDst_B = 0.0f;
		//	float dstX = 0.0f;
		//	float dstY = 0.0f;

		//	//타겟 ModVertRig 하나씩 체크하자
		//	for (int iTarget = 0; iTarget < targetModVertRigs.Count; iTarget++)
		//	{
		//		targetModVertRig = targetModVertRigs[iTarget];
		//		targetPosW = targetModVertRig._renderVertex._pos_World_NoMod;

		//		Debug.Log("[" + iTarget + "] Target Pos : " + targetPosW);

		//		nearL = null;
		//		nearT = null;
		//		nearR = null;
		//		nearB = null;
		//		minDst_L = 0.0f;
		//		minDst_T = 0.0f;
		//		minDst_R = 0.0f;
		//		minDst_B = 0.0f;

		//		for (int iSrc = 0; iSrc < _posWeightPairs.Count; iSrc++)
		//		{
		//			src = _posWeightPairs[iSrc];
		//			srcPosW = src._posWorld;

		//			//X 비교
		//			dstX = Mathf.Abs(srcPosW.x - targetPosW.x);
		//			dstY = Mathf.Abs(srcPosW.y - targetPosW.y);

		//			//가장 가까운 거리의 LTRB인지 체크
		//			if (srcPosW.x < targetPosW.x)
		//			{
		//				if(dstX < minDst_L || nearL == null)
		//				{
		//					nearL = src;
		//					minDst_L = dstX;
		//				}
		//			}
		//			else
		//			{
		//				if(dstX < minDst_R || nearR == null)
		//				{
		//					nearR = src;
		//					minDst_R = dstX;
		//				}
		//			}

		//			//T는 Y가 큰거
		//			if (srcPosW.y > targetPosW.y)
		//			{
		//				if(dstY < minDst_T || nearT == null)
		//				{
		//					nearT = src;
		//					minDst_T = dstY;
		//				}
		//			}
		//			else
		//			{
		//				if(dstY < minDst_B || nearB == null)
		//				{
		//					nearB = src;
		//					minDst_B = dstY;
		//				}
		//			}
		//		}

		//		//만약 대상이 되는 LTRB가 하나도 없다면?? (그럴리가)
		//		if(nearL == null && nearT == null && nearR == null && nearB == null)
		//		{
		//			Debug.LogError(">>> No Nearest Point");
		//			continue;
		//		}

		//		//반대로 가까운 점이 1개라면?
		//		List<PosWeightPair> checkList = new List<PosWeightPair>();
		//		if(nearL != null)
		//		{
		//			checkList.Add(nearL);
		//		}
		//		if(nearT != null && !checkList.Contains(nearT))
		//		{
		//			checkList.Add(nearT);
		//		}
		//		if(nearR != null && !checkList.Contains(nearR))
		//		{
		//			checkList.Add(nearR);
		//		}
		//		if(nearB != null && !checkList.Contains(nearB))
		//		{
		//			checkList.Add(nearB);
		//		}

		//		if(checkList.Count == 1)
		//		{
		//			Debug.LogError(">>> Check Point : 1");
		//			//가까운 점이 1개이다 => 3-1. 그 값을 대입하자
		//			checkList[0].PasteToModVertRig(_keyMeshGroup, targetModVertRig);

		//			//종료
		//			continue;
		//		}

		//		//3-2. LT, RT, LB, RB에 대해서 보간용 포인트를 만든다. 실제 포인트 또는 가상 포인트를 이용 > 4로 이동
		//		//4. Bi-Linear 방식으로 포인트 보간을 하고 값을 복사한다.

		//		PosWeightPair LT = null;
		//		PosWeightPair RT = null;
		//		PosWeightPair LB = null;
		//		PosWeightPair RB = null;

		//		float posL = targetPosW.x - minDst_L;
		//		float posT = targetPosW.y + minDst_T;
		//		float posR = targetPosW.x + minDst_R;
		//		float posB = targetPosW.y - minDst_B;

		//		Debug.Log(">> LT : " + (new Vector2(posL, posT)) + " / RB : " + (new Vector2(posR, posB)));

		//		//축 방향을 기준으로 LT, RT, LB, RB를 만들자
		//		//X축, Y축의 near 포인트가 같다면 그걸 이용하고, 아니면 위치 기반 보간으로 가상 포인트를 만들자.
		//		//LT
		//		if(nearL != null && nearT != null && nearL == nearT)
		//		{	
		//			LT = nearL;
		//		}
		//		else
		//		{
		//			LT = MakeVirtualPosWeightPair(new Vector2(posL, posT));
		//		}

		//		//RT
		//		if(nearR != null && nearT != null && nearR == nearT)
		//		{	
		//			RT = nearR;
		//		}
		//		else
		//		{
		//			RT = MakeVirtualPosWeightPair(new Vector2(posR, posT));
		//		}

		//		//LB
		//		if(nearL != null && nearB != null && nearL == nearB)
		//		{	
		//			LB = nearL;
		//		}
		//		else
		//		{
		//			LB = MakeVirtualPosWeightPair(new Vector2(posL, posB));
		//		}

		//		//RB
		//		if(nearR != null && nearB != null && nearR == nearB)
		//		{	
		//			RB = nearR;
		//		}
		//		else
		//		{
		//			RB = MakeVirtualPosWeightPair(new Vector2(posR, posB));
		//		}


		//		//Bi-Linear 방식으로 만든다.
		//		//LT-RT, LB-RB를 기준으로 X-Lerp를 한다.

		//		float lerpX = 0.0f;
		//		float lerpY = 0.0f;
		//		if(posR - posL > 0.0f)
		//		{
		//			lerpX = Mathf.Clamp01((targetPosW.x - posL) / (posR - posL));
		//		}
		//		if(posT - posB > 0.0f)
		//		{
		//			lerpY = Mathf.Clamp01((targetPosW.y - posB) / (posT - posB));
		//		}

		//		Debug.Log("lerpX : " + lerpX + " / lerpY : " + lerpY);

		//		//X축 보간 먼저
		//		PosWeightPair pairT = new PosWeightPair();
		//		PosWeightPair pairB = new PosWeightPair();

		//		pairT.Lerp(LT, RT, lerpX);
		//		pairB.Lerp(LB, RB, lerpX);

		//		//Y축 보간
		//		PosWeightPair result = new PosWeightPair();
		//		result.Lerp(pairB, pairT, lerpY);

		//		//보간 끝! 대입하자.
		//		result.PasteToModVertRig(_keyMeshGroup, targetModVertRig);
		//	}


		//	Debug.LogError("Pos-Paste End");
		//	return true;
		//} 
		#endregion

		//위치를 기반으로 가상 포인트를 만들자.
		private PosWeightPair MakeVirtualPosWeightPair(Vector2 posW)
		{
			//L, T, R, B의 가장 가까운 포인트를 찾자
			//이걸 리스트에 넣고 Dst를 기반으로 보간을 하자 (Bi-Linear 아님)
			PosWeightPair nearL = null;
			PosWeightPair nearT = null;
			PosWeightPair nearR = null;
			PosWeightPair nearB = null;

			float minDst_L = 0.0f;
			float minDst_T = 0.0f;
			float minDst_R = 0.0f;
			float minDst_B = 0.0f;

			PosWeightPair curPair = null;
			float dstX = 0.0f;
			float dstY = 0.0f;

			for (int iSrc = 0; iSrc < _posWeightPairs.Count; iSrc++)
			{
				curPair = _posWeightPairs[iSrc];

				dstX = Mathf.Abs(curPair._posWorld.x - posW.x);
				dstY = Mathf.Abs(curPair._posWorld.y - posW.y);

				//X, Y 축별로 최단 거리 포인트를 찾자
				if(curPair._posWorld.x < posW.x)
				{
					if(dstX < minDst_L || nearL == null)
					{
						nearL = curPair;
						minDst_L = dstX;
					}
				}
				else
				{
					if(dstX < minDst_R || nearR == null)
					{
						nearR = curPair;
						minDst_R = dstX;
					}
				}


				if(curPair._posWorld.y > posW.y)
				{
					if(dstY < minDst_T || nearT == null)
					{
						nearT = curPair;
						minDst_T = dstY;
					}
				}
				else
				{
					if(dstY < minDst_B || nearB == null)
					{
						nearB = curPair;
						minDst_B = dstY;
					}
				}
			}

			//LTRB를 체크한 후, 4개의 포인트에 대한 거리 최대 값을 구한다.
			float maxDstToCheck = 0.0f;
			float dst = 0.0f;
			if(nearL != null)
			{
				dst = Vector2.Distance(nearL._posWorld, posW);
				if(dst > maxDstToCheck)
				{
					maxDstToCheck = dst;
				}
			}
			if(nearT != null)
			{
				dst = Vector2.Distance(nearT._posWorld, posW);
				if(dst > maxDstToCheck)
				{
					maxDstToCheck = dst;
				}
			}
			if(nearR != null)
			{
				dst = Vector2.Distance(nearR._posWorld, posW);
				if(dst > maxDstToCheck)
				{
					maxDstToCheck = dst;
				}
			}
			if(nearB != null)
			{
				dst = Vector2.Distance(nearB._posWorld, posW);
				if(dst > maxDstToCheck)
				{
					maxDstToCheck = dst;
				}
			}

			//이제 다시 포인트에 대하여 다시 체크


			//L, T, R, B를 체크했다면 (어떤건 Null일 수 있다)
			//리스트에 정리한다.
			//일단 L, T, R, B는 넣어야 한다.
			List<PosWeightPair> pairList = new List<PosWeightPair>();
			//float bias = 0.01f;

			if (nearL != null)								{ pairList.Add(nearL); }
			if (nearT != null && !pairList.Contains(nearT))	{ pairList.Add(nearT); }
			if (nearR != null && !pairList.Contains(nearR))	{ pairList.Add(nearR); }
			if (nearB != null && !pairList.Contains(nearB))	{ pairList.Add(nearB); }

			for (int iSrc = 0; iSrc < _posWeightPairs.Count; iSrc++)
			{
				curPair = _posWeightPairs[iSrc];
				if(pairList.Contains(curPair))
				{
					continue;
				}

				dst = Vector2.Distance(curPair._posWorld, posW);
				if(dst < maxDstToCheck)
				{
					//이것도 체크에 포함하자.
					pairList.Add(curPair);
				}
			}

			if (pairList.Count == 0)
			{
				//잉.. 하나도 없는데요?
				return null;
			}

			if(pairList.Count == 1)
			{
				//하나라면 가상 포인트는 아니지만
				//그걸 리턴하면 된다.
				return pairList[0];
			}


			//역 선형 가중치를 만들기 위해서 dst와 totalDst를 알아야 한다.
			float totalDst = 0.0f;
			List<float> dstList = new List<float>();
			float curDst = 0.0f;

			for (int i = 0; i < pairList.Count; i++)
			{
				curPair = pairList[i];

				curDst = Vector2.Distance(curPair._posWorld, posW);//축과 관계없는 거리이다.
				dstList.Add(curDst);

				totalDst += curDst;
			}

			//Dst를 기준으로 Weight를 계산하고, totalWeight를 만들자.
			float totalWeight = 0.0f;
			List<float> weightList = new List<float>();
			float curWeight = 0.0f;

			for (int i = 0; i < dstList.Count; i++)
			{
				//curWeight = Mathf.Pow(1.0f - (dstList[i] / totalDst), 2.0f);//<<2제곱으로 가까이 있을 수록 가중치 증가
				curWeight = 1.0f / (dstList[i] + 0.01f);//<<분수로 가중치를 계산하는 경우
				weightList.Add(curWeight);

				totalWeight += curWeight;
			}

			//이제 새로운 가상 포인트를 만들고 실제 Weight만큼 값을 더하자
			PosWeightPair resultPair = new PosWeightPair();
			resultPair._posWorld = posW;

			//Weight를 이용하여 데이터를 더해주자.
			for (int i = 0; i < pairList.Count; i++)
			{
				resultPair.AddPosWeightPair(pairList[i], weightList[i] / totalWeight);
			}

			//데이터 Normalize
			resultPair.Normalize();

			return resultPair;
		}
	}
}