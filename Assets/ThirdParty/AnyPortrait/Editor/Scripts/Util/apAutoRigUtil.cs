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
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{
	//Auto Rigging 기능을 구현하기 위한 함수를 모은 Static 클래스
	public static class apAutoRigUtil
	{
		


		//자동으로 계산되는 Envelope의 정보
		//이 데이터를 기준으로 Auto Rigging이 계산된다.
		public class EnvelopeInfo
		{
			public apBone _bone = null;

			//Envelope 계산을 위해서 추가된 버텍스
			//실제의 Auto Rig 되는 버텍스는 이 변수보다 더 많다.
			public List<apModifiedVertexRig> _nearVertRigs = new List<apModifiedVertexRig>();
			public List<apModifiedVertexRig> _nearVertRigs_OnlyForCheck = new List<apModifiedVertexRig>();

			public float _size = 0.0f;

			public Vector2 _posStart = Vector2.zero;
			public Vector2 _posEnd = Vector2.zero;
			public Vector2 _posCenter = Vector2.zero;
			public bool _isHelper = false;
			public float _length = 0.0f;

			//연결된 본(Helper인 경우 무시)
			//Start, End 방향의 연결된 본(Parent, Child 상관 없음)
			public List<apBone> _linkedBone_ToStart = new List<apBone>();
			public List<apBone> _linkedBone_ToEnd = new List<apBone>();

			public List<EnvelopeInfo> _linkedEnvInfo_ToStart = new List<EnvelopeInfo>();
			public List<EnvelopeInfo> _linkedEnvInfo_ToEnd = new List<EnvelopeInfo>();

			public EnvelopeInfo(apBone bone)
			{
				_bone = bone;
				if(_nearVertRigs == null)
				{
					_nearVertRigs = new List<apModifiedVertexRig>();
				}
				if(_nearVertRigs_OnlyForCheck == null)
				{
					_nearVertRigs_OnlyForCheck = new List<apModifiedVertexRig>();
				}
				_nearVertRigs.Clear();
				_nearVertRigs_OnlyForCheck.Clear();

				_posStart = _bone._worldMatrix_NonModified.Pos;

				if (_bone._shapeHelper)
				{
					_isHelper = true;
					_posEnd = _posStart;
					_length = 0.0f;
				}
				else
				{
					_isHelper = false;
					_posEnd = _bone._worldMatrix_NonModified.MtrxToSpace.MultiplyPoint(new Vector2(0.0f, bone._shapeLength));
					_length = Vector2.Distance(_posStart, _posEnd);

					if(_length < 0.0001f)
					{
						//너무 짧은 경우
						_isHelper = true;
					}
				}


				_posCenter = (_posStart * 0.5f) + (_posEnd * 0.5f);

				//연결된 본을 체크하여 Start 방향에 있는지, End 방향에 있는지 확인한다. (Helper가 아닐 때)
				_linkedBone_ToStart.Clear();
				_linkedBone_ToEnd.Clear();

				_linkedEnvInfo_ToStart.Clear();
				_linkedEnvInfo_ToEnd.Clear();

				if (!_isHelper)
				{
					List<apBone> linkedBones = new List<apBone>();
					if (_bone._parentBone != null)
					{
						linkedBones.Add(_bone._parentBone);
					}

					if (_bone._childBones != null && _bone._childBones.Count > 0)
					{
						for (int iChild = 0; iChild < _bone._childBones.Count; iChild++)
						{
							linkedBones.Add(_bone._childBones[iChild]);
						}
					}

					//연결된 본들의 위치가 Start, End에서 어디에 더 가까운지 분류

					Vector2 posCenter = (_posStart * 0.5f) + (_posEnd * 0.5f);

					apBone linkedBone = null;
					Vector2 linkedPosCenter = Vector2.zero;
					Vector2 linkedPosStart = Vector2.zero;
					Vector2 linkedPosEnd = Vector2.zero;

					for (int iBone = 0; iBone < linkedBones.Count; iBone++)
					{
						linkedBone = linkedBones[iBone];
						if(linkedBone._shapeHelper)
						{
							//연결된 본이 Helper라면 생략
							continue;
						}
						linkedPosStart = linkedBone._worldMatrix_NonModified.Pos;
						linkedPosEnd = linkedBone._worldMatrix_NonModified.MtrxToSpace.MultiplyPoint(new Vector2(0.0f, linkedBone._shapeLength));
						linkedPosCenter = (linkedPosStart * 0.5f) + (linkedPosEnd * 0.5f);

						float angleToStart = Vector2.Angle(linkedPosCenter - posCenter, _posStart - posCenter);
						float angleToEnd = Vector2.Angle(linkedPosCenter - posCenter, _posEnd - posCenter);

						if(angleToStart < angleToEnd)
						{
							//Start쪽에 가까운 Bone이다.
							_linkedBone_ToStart.Add(linkedBone);
						}
						else if(angleToEnd < angleToStart)
						{
							//End쪽에 가까운 Bone이다.
							_linkedBone_ToEnd.Add(linkedBone);
						}
						//그외에는 우연히 가운데 위치하여 무관할 지도..
					}
				}
			}

			public void AddNearVertRig(apModifiedVertexRig vertRig, bool isOnlyForCheck)
			{
				_nearVertRigs_OnlyForCheck.Add(vertRig);
				if (!isOnlyForCheck)
				{
					_nearVertRigs.Add(vertRig);
				}
			}

			public float GetDistance(Vector2 targetPos)
			{
				if(_isHelper)
				{
					return Vector2.Distance(_posStart, targetPos);
				}
				return apEditorUtil.DistanceFromLine(_posStart, _posEnd, targetPos);
			}

			//VertRig가 등록된 상태에서 Size 계산하기
			public void CalculateSize()
			{
				float maxDist = 0.0f;
				apModifiedVertexRig curVertRig = null;
				float dst = 0.0f;
				for (int iVert = 0; iVert < _nearVertRigs_OnlyForCheck.Count; iVert++)
				{
					curVertRig = _nearVertRigs_OnlyForCheck[iVert];
					dst = GetDistance(curVertRig._renderVertex._pos_World_NoMod);

					if(dst > maxDist)
					{
						maxDist = dst;
					}
				}

				if(maxDist <= 0.0f)
				{
					maxDist = 0.01f;
				}

				//"가장 먼 거리의 버텍스"가 Envelope의 크기
				_size = maxDist;

				//if (!_isHelper)
				//{
				//	if(_size > _length)
				//	{
				//		_size = _length;
				//	}
				//}

				//Debug.LogError("Bone Envelope Info : " + _bone._name);
				//Debug.Log("Positon : " + _posStart + ", " + _posEnd);
				//Debug.Log("Length : " + _length);
				//Debug.Log("Size : " + _size);
			}

			public void LinkEnvelopeInfo(Dictionary<apBone, EnvelopeInfo> bone2EnvInfo)
			{
				_linkedEnvInfo_ToStart.Clear();
				_linkedEnvInfo_ToEnd.Clear();

				//Debug.LogError("Linked Env List : [" + _bone._name + "]");
				//Debug.LogWarning(">> Start");

				//"연결된 본" 정보를 기준으로 "연결된 EnvelopeInfo"도 만들자. 
				apBone linkedBone = null;
				for (int i = 0; i < _linkedBone_ToStart.Count; i++)
				{
					linkedBone = _linkedBone_ToStart[i];
					if(bone2EnvInfo.ContainsKey(linkedBone))
					{
						_linkedEnvInfo_ToStart.Add(bone2EnvInfo[linkedBone]);

						//Debug.Log("- " + linkedBone._name);
					}
				}

				//Debug.LogWarning(">> End");
				for (int i = 0; i < _linkedBone_ToEnd.Count; i++)
				{
					linkedBone = _linkedBone_ToEnd[i];
					if(bone2EnvInfo.ContainsKey(linkedBone))
					{
						_linkedEnvInfo_ToEnd.Add(bone2EnvInfo[linkedBone]);

						//Debug.Log("- " + linkedBone._name);
					}
				}
			}
			
			
		}



		//1. 체크
		public static float GetDistanceToBone(Vector2 targetPosW, apBone bone)
		{
			Vector2 posBoneStart = bone._worldMatrix_NonModified.Pos;

			if(bone._shapeHelper)
			{
				//헬퍼 타입이면 선분이 없다.
				return Vector2.Distance(posBoneStart, targetPosW);
			}
			
			Vector2 posBoneEnd = bone._worldMatrix_NonModified.MtrxToSpace.MultiplyPoint(new Vector2(0.0f, bone._shapeLength));
			//DistanceFromLine 함수는 Envelope에서 요구하는 방식의 길이를 계산해준다.
			return apEditorUtil.DistanceFromLine(posBoneStart, posBoneEnd, targetPosW);

		}


		public static EnvelopeInfo MakeEnvelopeWithoutVertRigs(apBone bone, Dictionary<apBone, EnvelopeInfo> existEnvelopes)
		{
			EnvelopeInfo newEnvelopeInfo = new EnvelopeInfo(bone);

			//3-2. Envelope가 생성되지 못한 Bone에 대해서도 Envelope 크기를 만든다.
			//(1) 인접한 본 (Parent 또는 Child) 중에 Envelope가 만들어진 본이 있다면, 그 크기의 70%를 크기로 이용한다.
			//(2) 그렇지 않다면, 본 길이 의 20%를 크기로 이용한다.
			//(3) 헬퍼인 경우 아주 작은 0.1의 값을 가진다.
			//(4) 실제 크기는 (1), (2), (3) 중에서 가장 큰 값을 이용한다.

			//연결된 본을 리스트로 모으자.
			List<apBone> linkedBones = new List<apBone>();
			if(bone._parentBone != null)
			{
				linkedBones.Add(bone._parentBone);
			}
			if(bone._childBones != null && bone._childBones.Count > 0)
			{
				for (int i = 0; i < bone._childBones.Count; i++)
				{
					linkedBones.Add(bone._childBones[i]);
				}
			}

			float maxSize = 0.0f;
			for (int i = 0; i < linkedBones.Count; i++)
			{
				apBone linkedBone = linkedBones[i];

				//연결된 본이 기존의 리스트에 있을 때
				//> 연결된 본의 Env 크기의 70%를 이용한다.
				if(existEnvelopes.ContainsKey(linkedBone))
				{
					EnvelopeInfo envlopeInfo = existEnvelopes[linkedBone];
					float properSize = envlopeInfo._size * 0.7f;
					if(properSize > maxSize)
					{
						maxSize = properSize;
					}
				}
			}

			//본 길이를 계산하자
			if(!bone._shapeHelper)
			{
				Vector2 posBoneStart = bone._worldMatrix_NonModified.Pos;
				Vector2 posBoneEnd = bone._worldMatrix_NonModified.MtrxToSpace.MultiplyPoint(new Vector2(0.0f, bone._shapeLength));
				float boneLength = Vector2.Distance(posBoneStart, posBoneEnd);
				float properSize = boneLength * 0.2f;//본 길이의 20%를 이용
				if(properSize > maxSize)
				{
					maxSize = properSize;
				}
			}
			else
			{
				//헬퍼 본이라면
				float properSize = 0.1f;
				if(properSize > maxSize)
				{
					maxSize = properSize;
				}
			}
			if(maxSize < 0.1f)
			{
				maxSize = 0.1f;
			}

			//계산된 크기를 이용하여 EnvelopeInfo를 구성한다.
			newEnvelopeInfo._size = maxSize;

			return newEnvelopeInfo;
		}


		public static float GetVertRigWeight(apModifiedVertexRig targetVertRig, EnvelopeInfo envInfo, EnvelopeInfo nearestEnvInfo)
		{
			Vector2 pos = targetVertRig._renderVertex._pos_World_NoMod;
			bool isExtendedArea = envInfo._nearVertRigs.Contains(targetVertRig) || IsIn2LevelLinkedBone(envInfo._bone, nearestEnvInfo._bone); //연결된 본이면 확장 영역으로 체크해야한다.
			//isExtendedArea = true;

			//Debug.LogError("Vert Rig : " + pos + " -->> EnvInfo : " + envInfo._bone._name);
			//Debug.LogWarning("[Env Info] Pos Start : " + envInfo._posStart + " / End " + envInfo._posEnd + " ( Center : " + envInfo._posCenter + ")");
			//Debug.LogWarning("[Env Info] Size : " + envInfo._size);

			//4가지 Weight가 있다.
			//1) Distance 배수 : 거리에 따른 배수. 최대 영역 밖으로 나가면 실패값 리턴
			//2) Center 배수 : Center를 기점으로 Box인 경우, 직교하는 위치에서의 거리, Circle인 경우 전체 거리를 기준으로 배수를 정한다. (Helper인 경우 1)
			//3) Angle 배수 : Circle에 위치한 경우, 각도가 90도에서 0도로 갈수록 감소. (Box에 위치한 경우 1)
			//4) Linked 배수 : 연결된 본이 있는 경우, 해당 각도로의 리깅을 제한한다.
			
			
			//Distance 배수
			//- 확장) 기존 영역까지 1 > 0.7 / 확장 영역까지 0.7 > 0 (Pow2)
			//- 기본) 기존 영역까지 1 > 0

			//Center 배수
			//- Helper인 경우 1
			//- 거리값은 "직교위치"(CosA)
			//- 확장) 최대 거리 Center ~ [Length/2 + Size*2] / [Length/2 + Size]까지 1 > 0.7 / [추가 Size]까지 0.7 > 0.4
			//- 기본) 최대 거리 Center ~ [Length/2 + Size] / [Length/2 + Size]까지 1 > 0.7

			//Angle 배수
			//- Box나 Helper인 경우 1
			//- [Start->End] 벡터와 [End 또는 Start -> Pos]벡터의 각도 / 각도가 90 > 0으로 1 > 0.8

			//Linked 배수
			//- Helper인 경우 1
			//- 현재 위치가 Start 또는 End 방향일 때, 해당 방향의 "연결된 EnvelopeInfo"가 있는 경우
			//- 연결된 EnvInfo의 중심점 까지의 Cos 거리를 계산하여 [Length/2]까진 1, [Linked Center]까지 0으로 값이 감소한다.
			//- 연결된 본이 2개 이상이라면, 각각의 본에 대해 계산한 후, 그 값을 모두 곱한다.

			float Multiplier_Dst = 1.0f;//Distance 배수
			float Multiplier_Center = 1.0f;//Center 배수
			float Multiplier_Angle = 1.0f;//Angle 배수
			float Multiplier_Linked = 1.0f;//Linked 배수

			//계산을 위한 속성들
			float dst = 0.0f;//Capsule 방식의 거리
			bool isBox = false;//위치한 곳이 Box인지 Circle인지
			float dstFromCenter = 0.0f;//선분에 직교한 위치와 Center로부터의 거리
			float angle = 0.0f;//Circle에 위치한 경우

			Vector2 centerPos = (envInfo._posStart * 0.5f) + (envInfo._posEnd * 0.5f);
			
			//미리 필요한 속성들을 계산하자.
			if (envInfo._isHelper )
			{
				dst = Vector2.Distance(pos, envInfo._posStart);
				isBox = false;
				dstFromCenter = 0.0f;
				angle = 0.0f;
			}
			else
			{
				float dotA = Vector2.Dot(pos - envInfo._posStart, (envInfo._posEnd - envInfo._posStart).normalized);
				float dotB = Vector2.Dot(pos - envInfo._posEnd, (envInfo._posStart - envInfo._posEnd).normalized);

				if (dotA < 0.0f)
				{
					//Start쪽 반원에 있다.
					dst = Vector2.Distance(envInfo._posStart, pos);
					isBox = false;
					
					if(dst < 0.0001f)
					{
						//너무 짧다면 Angle은 90으로 강제
						angle = 90.0f;
					}
					else
					{
						angle = Mathf.Abs(Vector2.Angle(envInfo._posEnd - envInfo._posStart, pos - envInfo._posStart));

						if (angle > 90.0f)
						{
							angle = 180.0f - angle;
						}
					}
					

				}
				else if (dotB < 0.0f)
				{
					//End쪽 반원에 있다.
					dst = Vector2.Distance(envInfo._posEnd, pos);
					isBox = false;
					angle = Vector2.Angle(envInfo._posEnd - envInfo._posStart, pos - envInfo._posEnd);

					if(dst < 0.0001f)
					{
						//너무 짧다면 Angle은 90으로 강제
						angle = 90.0f;
					}
					else
					{
						angle = Mathf.Abs(Vector2.Angle(envInfo._posEnd - envInfo._posStart, pos - envInfo._posEnd));

						if (angle > 90.0f)
						{
							angle = 180.0f - angle;
						}
					}
				}
				else
				{
					//Box에 위치한다.
					dst = Vector2.Distance((envInfo._posStart + (envInfo._posEnd - envInfo._posStart).normalized * dotA), pos);
					isBox = true;
					angle = 0.0f;
				}

				Vector2 center2PosVec = pos - centerPos;
				float angleFromCenter = Mathf.Abs(Vector2.Angle(center2PosVec, envInfo._posEnd - envInfo._posStart));
				if(angleFromCenter > 90.0f)
				{
					angleFromCenter = 180.0f - angleFromCenter;
				}
				dstFromCenter = Mathf.Abs(Mathf.Cos(angleFromCenter * Mathf.Deg2Rad) * center2PosVec.magnitude);
			}
			
			//Debug.Log("> Distance : " + dst);
			//Debug.Log("> Distance From Center : " + dstFromCenter);
			//Debug.Log("> Is Box : " + isBox);

			float lerp = 0.0f;

			//1. Distance 배수
			//- 확장) 기존 영역까지 1 > 0.7 / 확장 영역까지 0.7 > 0 (Pow2)
			//- 기본) 기존 영역까지 1 > 0

			float normalArea = Mathf.Max(envInfo._size, 0.001f);

			if(isExtendedArea)
			{
				float extendedArea = normalArea * 2;

				if(dst < normalArea)
				{	
					lerp = dst / normalArea;
					Multiplier_Dst = (1.0f * (1.0f - lerp)) + (0.7f * lerp);//1 ~ 0.7
				}
				else if(dst < extendedArea)
				{
					lerp = Mathf.Pow((dst - normalArea) / (extendedArea - normalArea), 2);//Pow2 타입의 보간
					Multiplier_Dst = (0.7f * (1.0f - lerp)) + (0.0f * lerp);//0.7 ~ 0
				}
				else
				{
					//영역 밖
					return -1.0f;
				}
			}
			else
			{
				if(dst < normalArea)
				{
					lerp = dst / normalArea;
					Multiplier_Dst = (1.0f * (1.0f - lerp)) + (0.0f * lerp);//1 ~ 0
				}
				else
				{
					//영역 밖
					return -1.0f;
				}
			}

			

			//Center 배수
			//- Helper인 경우 1
			//- 거리값은 "직교위치"(CosA)
			//- 확장) 최대 거리 Center ~ [Length/2 + Size*2] / [Length/2 + Size]까지 1 > 0.7 / [추가 Size]까지 0.7 > 0.4
			//- 기본) 최대 거리 Center ~ [Length/2 + Size] / [Length/2 + Size]까지 1 > 0.7
			if(envInfo._isHelper)
			{
				Multiplier_Center = 1.0f;
			}
			else
			{
				float boneLength = Vector2.Distance(envInfo._posEnd, envInfo._posStart);
				float normalLength = (boneLength * 0.5f) + envInfo._size;
				if(isExtendedArea)
				{
					float extendedLength = normalLength + envInfo._size;
					if(dstFromCenter < normalLength)
					{
						lerp = dstFromCenter / normalLength;
						Multiplier_Center = (1.0f * (1.0f - lerp)) + (0.7f * lerp);//1 > 0.7f
					}
					else if(dstFromCenter < extendedLength)
					{
						lerp = (dstFromCenter - normalLength) / (extendedLength - normalLength);
						Multiplier_Center = (0.7f * (1.0f - lerp)) + (0.4f * lerp);//0.7 > 0.4f
					}
					else
					{
						Multiplier_Center = 0.4f;
					}
				}
				else
				{
					if(dstFromCenter < normalLength)
					{
						lerp = dstFromCenter / normalLength;
						Multiplier_Center = (1.0f * (1.0f - lerp)) + (0.7f * lerp);//1 > 0.7f
					}
					else
					{
						Multiplier_Center = 0.7f;
					}
				}
			}

			//Angle 배수
			//- Box나 Helper인 경우 1
			//- [Start->End] 벡터와 [End 또는 Start -> Pos]벡터의 각도 / 각도가 90 > 0으로 1 > 0.8
			//- 거리에 따라서 한번 더 계산한다. (거리가 0이면 배수는 1이 되고, 거리가 normalLength 이상일 때 Angle 배수 이용)
			if(isBox || envInfo._isHelper)
			{
				Multiplier_Angle = 1.0f;
			}
			else
			{
				float dstMul = 0.0f;
				if(dst < envInfo._size)
				{
					dstMul = dst / envInfo._size;
				}
				else
				{
					dstMul = 1.0f;
				}
				if(angle > 90.0f)
				{
					Multiplier_Angle = 1.0f;
				}
				else
				{
					lerp = angle / 90.0f;
					Multiplier_Angle = (0.8f * (1.0f - lerp)) + (1.0f * lerp);//0도일때 0.8로 더 작다.
				}

				Multiplier_Angle = (1.0f * (1.0f - dstMul)) + (Multiplier_Angle * dstMul);
			}



			//Linked 배수
			//- Helper인 경우 1
			//- 현재 위치가 Start 또는 End 방향일 때, 해당 방향의 "연결된 EnvelopeInfo"가 있는 경우
			//- 연결된 EnvInfo의 중심점 까지의 Cos 거리를 계산하여 [Length/2]까진 1, [Linked Center]까지 0으로 값이 감소한다.
			//- 연결된 본이 2개 이상이라면, 각각의 본에 대해 계산한 후, 그 값을 모두 곱한다.
			if(envInfo._isHelper)
			{
				Multiplier_Linked = 1.0f;
			}
			else
			{
				//일단 현재 좌표가 Start 방향인지, End 방향인지 확인하자.
				Vector2 vecCenter2Pos = pos - centerPos;
				float angleToStart = Vector2.Angle(vecCenter2Pos, envInfo._posStart - envInfo._posEnd);
				float angleToEnd = Vector2.Angle(vecCenter2Pos, envInfo._posEnd - envInfo._posStart);

				//Debug.Log("Angle : Center->Pos / Start : " + angleToStart);
				//Debug.Log("Angle : Center->Pos / End : " + angleToEnd);

				List<EnvelopeInfo> linkedEnvInfoList = null;
				if(angleToStart < angleToEnd)
				{	
					//Start 방향에 있다.
					//Debug.Log("Vert가 Start 방향에 있다. [" + envInfo._linkedEnvInfo_ToStart.Count + "]");
					if(envInfo._linkedEnvInfo_ToStart.Count > 0)
					{
						linkedEnvInfoList = envInfo._linkedEnvInfo_ToStart;
					}
				}
				else if(angleToEnd < angleToStart)
				{
					//End 방향에 있다.
					//Debug.Log("Vert가 End 방향에 있다. [" + envInfo._linkedEnvInfo_ToEnd.Count + "]");
					if(envInfo._linkedEnvInfo_ToEnd.Count > 0)
					{
						linkedEnvInfoList = envInfo._linkedEnvInfo_ToEnd;
					}
				}

				if (linkedEnvInfoList == null)
				{
					//Linked 계산이 힘들다.
					Multiplier_Linked = 1.0f;
				}
				else
				{
					Multiplier_Linked = 1.0f;

					EnvelopeInfo linkedEnvInfo = null;
					Vector2 linkedPosCenter = Vector2.zero;
					float length2LinkedCenter = 0.0f;
					Vector2 vecCenter2LinkedCenter = Vector2.zero;
					float angleFromLinkedVector = 0.0f;
					float distCosLinked = 0.0f;

					for (int iLinked = 0; iLinked < linkedEnvInfoList.Count; iLinked++)
					{
						linkedEnvInfo = linkedEnvInfoList[iLinked];
						//Debug.Log("[" + iLinked + "] : " + linkedEnvInfo._bone._name);

						linkedPosCenter = linkedEnvInfo._posCenter;

						//Center 2 Center를 구한다.
						length2LinkedCenter = Vector2.Distance(centerPos, linkedPosCenter);

					
						//Debug.Log("Center > Linked Center 거리 : " + length2LinkedCenter);

						//만약 거리가 너무 짧다면 무시
						if (length2LinkedCenter < 0.0001f)
						{
							//Debug.LogError("너무 짧은 거리");
							continue;
						}

						vecCenter2LinkedCenter = linkedPosCenter - centerPos;
						angleFromLinkedVector = Vector2.Angle(vecCenter2Pos, vecCenter2LinkedCenter);

						//Debug.Log("[Center > Pos]와 [Center > Linked Center]의 각도 : " + angleFromLinkedVector);

						//연결된 Center까지 벡터 상에서의 거리
						distCosLinked = vecCenter2Pos.magnitude * Mathf.Cos(angleFromLinkedVector * Mathf.Deg2Rad);

						

						float boneLength = Vector2.Distance(envInfo._posEnd, envInfo._posStart);
						float normalLength = (boneLength * 0.5f);
						float limitLength = length2LinkedCenter;
						if(limitLength < normalLength)
						{
							//Debug.LogError("Center 2 Center가 너무 짧음 : " + limitLength + " >> " + normalLength * 1.5f);
							limitLength = normalLength * 1.5f;
						}
						//Debug.Log("Cos 거리 : " + distCosLinked);
						//Debug.Log("기본 거리 제한 (Bone Length / 2) : " + normalLength);
						//Debug.Log("전체 거리 제한 : " + length2LinkedCenter);

						if(distCosLinked < normalLength)
						{
							Multiplier_Linked *= 1.0f;//배수는 1
							//Debug.Log("- 기본 범위 안에 들어감 : x1");
						}
						else if(distCosLinked < limitLength)
						{
							lerp = (distCosLinked - normalLength) / (limitLength - normalLength);
							//1에서 0으로 감소
							float ratio = 1.0f * (1.0f - lerp) + 0.0f * lerp;
							Multiplier_Linked *= ratio;

							//Debug.LogWarning("- 제한 범위 안에 들어감 : x" + ratio);

						}
						else
						{
							Multiplier_Linked *= 0.0f;
							//Debug.LogError("- 범위 밖");
						}

						//Debug.Log(">>> 현재 Linked 값 : " + Multiplier_Linked);
					}
				}

			}

			//Debug.LogWarning("[Multiplier Dst] : " + Multiplier_Dst);
			//Debug.LogWarning("[Multiplier Center] : " + Multiplier_Center);
			//Debug.LogWarning("[Multiplier Angle] : " + Multiplier_Angle);
			//Debug.LogWarning("[Multiplier Linked] : " + Multiplier_Linked);

			float resultWeight = Multiplier_Dst * Multiplier_Center * Multiplier_Angle * Multiplier_Linked;
			if(resultWeight < 0.0001f)
			{
				return -1.0f;
			}

			//Debug.LogWarning(">> Result : " + resultWeight);

			return resultWeight;
		}

		//2레벨 이내의 연결된 본들인가?
		private static bool IsIn2LevelLinkedBone(apBone boneA, apBone boneB)
		{
			if(boneA == null || boneB == null)
			{
				return false;
			}
			if(boneA == boneB)
			{
				return true;
			}
			apBone parentA_1 = boneA._parentBone;
			apBone parentA_2 = (parentA_1 != null ? parentA_1._parentBone : null);
			apBone parentB_1 = boneB._parentBone;
			apBone parentB_2 = (parentB_1 != null ? parentB_1._parentBone : null);

			if(parentA_1 != null && parentA_1 == boneB)
			{
				return true;
			}
			if(parentA_2 != null && parentA_2 == boneB)
			{
				return true;
			}
			if(parentB_1 != null && parentB_1 == boneA)
			{
				return true;
			}
			if(parentB_2 != null && parentB_2 == boneA)
			{
				return true;
			}

			return false;
		}
	}
}