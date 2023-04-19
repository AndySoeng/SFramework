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
	/// apModifiedVertex와 유사하지만 Rigging 용으로만 따로 정의되었다.
	/// Bone 좌표계에 대한 Local Position과 Weight 쌍으로 구성되어있다.
	/// 최대 8개의 Bone에 연동될 수 있다. (리스트 사용 안함)
	/// 이 값은 RenderVertex에 직접 적용이 된다.
	/// </summary>
	[Serializable]
	public class apModifiedVertexRig
	{
		// Members
		//-----------------------------------------------
		[NonSerialized]
		public apModifiedMesh _modifiedMesh = null;

		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apVertex _vertex = null;

		[NonSerialized]
		public apRenderVertex _renderVertex = null;//RenderUnit과 연동된 경우 RenderVert도 넣어주자



		[Serializable]
		public class WeightPair
		{
			[NonSerialized]
			public apBone _bone = null;

			[SerializeField]
			public int _boneID = -1;

			[NonSerialized]
			public apMeshGroup _meshGroup = null;

			[SerializeField]
			public int _meshGroupID = -1;

			[SerializeField]
			public float _weight = 0.0f;

			/// <summary>
			/// 백업용 생성자
			/// </summary>
			public WeightPair()
			{

			}


			public WeightPair(apBone bone)
			{
				_bone = bone;
				_boneID = _bone._uniqueID;

				_meshGroup = _bone._meshGroup;
				_meshGroupID = _meshGroup._uniqueID;

				_weight = 0.0f;
			}
		}
		[SerializeField]
		public List<WeightPair> _weightPairs = new List<WeightPair>();

		public float _totalWeight = 0.0f;


		// Init
		//-----------------------------------------------
		public apModifiedVertexRig()
		{

		}


		public void Init(int vertUniqueID, apVertex vertex)
		{
			_vertexUniqueID = vertUniqueID;
			_vertex = vertex;
			_vertIndex = _vertex._index;

			ResetWeightTable();
		}


		public void Link(apModifiedMesh modifiedMesh, apMesh mesh, apVertex vertex)
		{
			_modifiedMesh = modifiedMesh;
			_mesh = mesh;
			_vertex = vertex;
			if (_vertex != null)
			{
				_vertIndex = _vertex._index;
			}
			else
			{
				_vertIndex = -1;
			}

			_renderVertex = null;
			if (modifiedMesh._renderUnit != null && _vertex != null)
			{
				//이전
				//_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
				//{
				//	return a._vertex == _vertex;
				//});

				//변경 22.3.23 : 배열로 변경된 RenderVerts
				_renderVertex = modifiedMesh._renderUnit.FindRenderVertex(_vertex);
			}
		}

		public bool CheckAndLinkModMeshAndRenderVertex(apModifiedMesh modifiedMesh)
		{
			if (_modifiedMesh != modifiedMesh || _renderVertex == null)
			{
				_modifiedMesh = modifiedMesh;
				if (_modifiedMesh != null && modifiedMesh._renderUnit != null && _vertex != null)
				{
					//이전
					//_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
					//{
					//	return a._vertex == _vertex;
					//});

					//변경 22.3.23 : 배열로 변경된 RenderVerts
					_renderVertex = modifiedMesh._renderUnit.FindRenderVertex(_vertex);

					if(_renderVertex == null)
					{
						//Debug.LogError("Render Vertex가 Null");
						return false;
					}
					//RenderVertex가 갱신됨
					return true;
				}
			}
			return true;
		}

		/// <summary>
		/// WeightTable의 값과 연동을 하고 Sort를 한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void LinkWeightPair(apPortrait portrait, apMeshGroup parentMeshGroup)
		{
			
			_totalWeight = 0.0f;
			WeightPair weightPair = null;
			bool isAnyRemove = false;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				weightPair = _weightPairs[i];
				//이 코드는 필요하지만 일단 주석. 최적화 작업을 더 하자
				//if(weightPair._meshGroup != null && weightPair._bone != null)
				//{
				//	//이미 링크가 되었다.
				//	_totalWeight += weightPair._weight;
				//	continue;
				//}

				if (weightPair._meshGroupID >= 0)
				{
					//이 부분 고쳐야 한다.
					//검출 코드 추가 (20.3.30)
					//무조건 GetMeshGroup을 하면, 유효하지 않은 메시 그룹(부모-자식 연결이 안된)을 참조할 수도 있다.
					//weightPair._meshGroup = portrait.GetMeshGroup(weightPair._meshGroupID);//이전
					if(parentMeshGroup != null)
					{
						weightPair._meshGroup = parentMeshGroup.GetMeshGroupWithChildren(weightPair._meshGroupID);
					}
					else
					{
						weightPair._meshGroup = portrait.GetMeshGroup(weightPair._meshGroupID);
					}
					

					if (weightPair._meshGroup != null)
					{
						//<BONE_EDIT>
						//weightPair._bone = weightPair._meshGroup.GetBone(weightPair._boneID);

						//>>Recursive로 변경
						weightPair._bone = weightPair._meshGroup.GetBoneRecursive(weightPair._boneID);
						if (weightPair._bone == null)
						{
							isAnyRemove = true;
							//Debug.LogWarning("AnyPortrait : Invalid Rigging Data Deleted: The connected bone could be found. (Temporary message. Will be hidden after update)");
						}
						else
						{
							_totalWeight += weightPair._weight;

							//검출 코드 추가 21.7.19
							if(weightPair._bone._meshGroup != null && weightPair._bone._meshGroup != weightPair._meshGroup)
							{
								//Bone의 MeshGroup과 Pair의 MeshGroup이 다른 경우
								//Debug.LogError("검출 : MeshGroup이 서로 맞지 않는다. [Bone MeshGroup : " + weightPair._bone._meshGroup._name + " / Pair MeshGroup : " + weightPair._meshGroup);
								weightPair._meshGroup = weightPair._bone._meshGroup;
								weightPair._meshGroupID = weightPair._bone._meshGroup._uniqueID;
							}
						}
					}
					else
					{
						weightPair._bone = null;
						isAnyRemove = true;
						//로그 확인하였으므로 주석 처리
						//Debug.LogWarning("AnyPortrait : Invalid Rigging Data Deleted: The target mesh group could not be found.");
					}
				}
				else
				{
					weightPair._meshGroup = null;
					weightPair._bone = null;
					isAnyRemove = true;
				}

			}
			if (isAnyRemove)
			{
				//뭔가 삭제할게 생겼다. 삭제하자
				_weightPairs.RemoveAll(delegate (WeightPair a)
				{
					return a._meshGroup == null || a._bone == null;
				});

				//뭔가 삭제되었다면 Normalize를 해야한다.
				Normalize();
			}
		}




		// Functions
		//-----------------------------------------------
		/// <summary>
		/// Weight 정보를 모두 초기화한다.
		/// </summary>
		public void ResetWeightTable()
		{
			_weightPairs.Clear();
			_totalWeight = 0.0f;
		}


		public void CalculateTotalWeight()
		{
			_totalWeight = 0.0f;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				_totalWeight += _weightPairs[i]._weight;
			}
		}

		public void Normalize()
		{
			_totalWeight = 0.0f;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				_totalWeight += _weightPairs[i]._weight;
			}

			if (_totalWeight > 0.0f && _weightPairs.Count > 0)
			{
				for (int i = 0; i < _weightPairs.Count; i++)
				{
					_weightPairs[i]._weight /= _totalWeight;
				}

				_totalWeight = 1.0f;
			}
		}

		/// <summary>
		/// Normalize와 유사하지만, 해당 Pair를 일단 제쳐두고,
		/// "나머지 Weight"에 한해서 우선 Normalize
		/// 그리고 해당 Pair를 포함시킨다.
		/// 요청한 Pair의 Weight가 1이 넘으면 1로 맞추고 나머지는 0
		/// </summary>
		/// <param name="pair"></param>
		public void NormalizeExceptPair(WeightPair pair, bool isSetOtherRigValue0or1 = false)
		{
			if (!_weightPairs.Contains(pair))
			{
				Normalize();
				return;
			}

			float reqWeight = Mathf.Clamp01(pair._weight);

			//변경 19.7.27 : 리깅 데이터에 Lock이 걸리면 Normalize에서 값 보정이 안된다.
			//락 여부에 따라서 각각 RigData의 개수와 Weight 총합 구하기
			int nPair_Locked = 0;
			int nPair_Unlocked = 0;
			float prevWeight_Locked = 0.0f;
			float prevWeight_Unlocked = 0.0f;

			WeightPair curPair = null;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				curPair = _weightPairs[i];
				if (curPair == pair)
				{
					continue;
				}
				else if (curPair._bone == null)
				{
					continue;
				}
				else if(curPair._bone._isRigLock)
				{
					//Lock이 걸린 Bone이다.
					nPair_Locked++;
					prevWeight_Locked += curPair._weight;
				}
				else
				{
					//Lock이 걸리지 않은 Bone이다.
					nPair_Unlocked++;
					prevWeight_Unlocked += curPair._weight;
				}
			}
			prevWeight_Locked = Mathf.Clamp01(prevWeight_Locked);
			prevWeight_Unlocked = Mathf.Clamp01(prevWeight_Unlocked);

			//float remainedWeight = 1.0f - reqWeight;//기존

			//변경 : Unlocked된 Bone에 대해서만 계산한다.
			if(reqWeight + prevWeight_Locked > 1.0f)
			{
				//Lock + 요청된 Weight의 합이 1이 넘으면 정상적으로 값을 설정할 수 없다.
				//요청값을 감소시킨다.
				reqWeight = 1.0f - prevWeight_Locked;
			}

			float remainedWeight = 1.0f - (reqWeight + prevWeight_Locked);
			if(remainedWeight < 0.0f)
			{
				//요청된 Weight를 적용할 수 없다.
				remainedWeight = 0.0f;
			}

			//float totalWeightExceptReq = 0.0f;
			//int nOtherPairs = 0;
			for (int i = 0; i < _weightPairs.Count; i++)
			{
				curPair = _weightPairs[i];
				if (curPair == pair)
				{
					curPair._weight = reqWeight;
					break;
				}
				//else
				//{
				//	totalWeightExceptReq += _weightPairs[i]._weight;
				//	nOtherPairs++;
				//}
			}

			//나머지 Unlocked Pair에 값을 지정해야하는 경우
			if(nPair_Unlocked > 0)
			{
				if (prevWeight_Unlocked > 0.0f)
				{
					//Unlocked Pair의 값의 합이 0보다 큰 경우 (Normalize를 위한 준비 필요)
					float convertRatio = remainedWeight / prevWeight_Unlocked;

					for (int i = 0; i < _weightPairs.Count; i++)
					{
						curPair = _weightPairs[i];
						if (curPair == pair || curPair._bone == null || curPair._bone._isRigLock)
						{
							//Unlocked Bone이 아닌 경우
							continue;
						}
						else
						{
							//Weight를 변경하자
							curPair._weight *= convertRatio;
						}
					}
				}
				else if (isSetOtherRigValue0or1)
				{
					//Unlocked Pair의 값이 0이었으나, 남은 Weight를 1/n로 나누어서 분배할 필요가 있는 경우
					float perWeight = remainedWeight / nPair_Unlocked;

					for (int i = 0; i < _weightPairs.Count; i++)
					{
						curPair = _weightPairs[i];
						if (curPair == pair || curPair._bone == null || curPair._bone._isRigLock)
						{
							//Unlocked Bone이 아닌 경우
							continue;
						}
						else
						{
							_weightPairs[i]._weight = perWeight;
						}
					}
				}
			}
			
			//그리고 마지막으로 Normalize
			Normalize();


		}

		/// <summary>
		/// 일정값 이하의 Weight를 가지는 WeightPair를 삭제한다.
		/// Normalize를 자동으로 수행한다.
		/// </summary>
		public bool Prune()
		{
			Normalize();

			int nRemoved = _weightPairs.RemoveAll(delegate (WeightPair a)
			{
				return a._weight < 0.01f;//1%
			});

			Normalize();

			return nRemoved > 0;
		}

		// Get / Set 
		//-----------------------------------------------
	}
}