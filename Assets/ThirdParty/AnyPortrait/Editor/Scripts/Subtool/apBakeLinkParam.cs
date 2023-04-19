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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	public class apBakeLinkManager
	{
		// Members
		//-----------------------------------------------
		public apBakeLinkParam _rootParam = null;
		public List<apBakeLinkParam> _totalParams = new List<apBakeLinkParam>();

		//Opt 계열만 따로 정리한다.
		//일부는 ID를 키값으로 정리한다. (중복될 수 있으므로 List 형태로 Value를 만든다)
		public List<apBakeLinkParam> _totalParams_OptRootUnit = new List<apBakeLinkParam>();
		public Dictionary<int, List<apBakeLinkParam>> _totalParams_OptTransform = new Dictionary<int, List<apBakeLinkParam>>();
		public Dictionary<int, List<apBakeLinkParam>> _totalParams_OptBone = new Dictionary<int, List<apBakeLinkParam>>();
		public List<apBakeLinkParam> _totalParams_OptNode = new List<apBakeLinkParam>();
		public Dictionary<int, List<apBakeLinkParam>> _totalParams_OptMesh = new Dictionary<int, List<apBakeLinkParam>>();//<<상위의 MeshTransform의 ID를 사용한다.
		public List<apBakeLinkParam> _totalParams_ExObjectComp = new List<apBakeLinkParam>();

		//디버깅용 타이머
		//private enum TIMER_TYPE
		//{
		//	OptTransform,
		//	OptBone,
		//	OptNode,
		//	OptMesh,
		//	ExObject
		//}

		//private Dictionary<TIMER_TYPE, System.Diagnostics.Stopwatch> _stopwatches = new Dictionary<TIMER_TYPE, System.Diagnostics.Stopwatch>();
		//private Dictionary<TIMER_TYPE, int> _timerCalls = new Dictionary<TIMER_TYPE, int>();

		// Init
		//-----------------------------------------------
		public apBakeLinkManager()
		{
			_rootParam = null;
			_totalParams.Clear();
			_totalParams_OptRootUnit.Clear();
			_totalParams_OptTransform.Clear();
			_totalParams_OptBone.Clear();
			_totalParams_OptNode.Clear();
			_totalParams_OptMesh.Clear();
			_totalParams_ExObjectComp.Clear();

			//_stopwatches.Clear();
			//_stopwatches.Add(TIMER_TYPE.OptTransform, new System.Diagnostics.Stopwatch());
			//_stopwatches.Add(TIMER_TYPE.OptBone, new System.Diagnostics.Stopwatch());
			//_stopwatches.Add(TIMER_TYPE.OptNode, new System.Diagnostics.Stopwatch());
			//_stopwatches.Add(TIMER_TYPE.OptMesh, new System.Diagnostics.Stopwatch());
			//_stopwatches.Add(TIMER_TYPE.ExObject, new System.Diagnostics.Stopwatch());

			//_timerCalls.Clear();
			//_timerCalls.Add(TIMER_TYPE.OptTransform, 0);
			//_timerCalls.Add(TIMER_TYPE.OptBone, 0);
			//_timerCalls.Add(TIMER_TYPE.OptNode, 0);
			//_timerCalls.Add(TIMER_TYPE.OptMesh, 0);
			//_timerCalls.Add(TIMER_TYPE.ExObject, 0);
		}


		// Functions
		//-----------------------------------------------
		public void Parse(GameObject rootOptTransformGameObject, GameObject groupObject)
		{
			_rootParam = null;
			_totalParams.Clear();
			_totalParams_OptRootUnit.Clear();
			_totalParams_OptTransform.Clear();
			_totalParams_OptBone.Clear();
			_totalParams_OptNode.Clear();
			_totalParams_OptMesh.Clear();
			_totalParams_ExObjectComp.Clear();

			//재귀적으로 파싱하자
			ParseRecursive(rootOptTransformGameObject, null);
			
		}

		

		private void ParseRecursive(GameObject targetGameObject, apBakeLinkParam parentLinkParam)
		{
			apBakeLinkParam newLinkParam = new apBakeLinkParam();
			newLinkParam.Parse(targetGameObject, parentLinkParam);

			if(parentLinkParam == null)
			{
				_rootParam = newLinkParam;
			}

			_totalParams.Add(newLinkParam);

			//Param 타입에 따라 따로 넣자
			if(newLinkParam._optRoot != null)
			{
				_totalParams_OptRootUnit.Add(newLinkParam);
			}
			if(newLinkParam._optTransform != null)
			{
				int transformID = newLinkParam._optTransform._transformID;
				if(!_totalParams_OptTransform.ContainsKey(transformID))
				{
					_totalParams_OptTransform.Add(transformID, new List<apBakeLinkParam>());
				}
				_totalParams_OptTransform[transformID].Add(newLinkParam);
			}
			if(newLinkParam._optBone != null)
			{
				int boneID = newLinkParam._optBone._uniqueID;
				if(!_totalParams_OptBone.ContainsKey(boneID))
				{
					_totalParams_OptBone.Add(boneID, new List<apBakeLinkParam>());
				}
				_totalParams_OptBone[boneID].Add(newLinkParam);
			}
			if(newLinkParam._optNode != null)
			{
				_totalParams_OptNode.Add(newLinkParam);
			}
			if(newLinkParam._optMesh != null && newLinkParam._optMesh._parentTransform != null)
			{
				int parentTFID = newLinkParam._optMesh._parentTransform._transformID;
				if(!_totalParams_OptMesh.ContainsKey(parentTFID))
				{
					_totalParams_OptMesh.Add(parentTFID, new List<apBakeLinkParam>());
				}
				_totalParams_OptMesh[parentTFID].Add(newLinkParam);
			}
			if(!newLinkParam._isOptGameObject || !newLinkParam._isOtherComponentExist)
			{
				_totalParams_ExObjectComp.Add(newLinkParam);
			}


			//자식 GameObject에 대해서 차례대로 넣자
			int nChild = targetGameObject.transform.childCount;
			for (int i = 0; i < nChild; i++)
			{
				Transform childTransform = targetGameObject.transform.GetChild(i);

				ParseRecursive(childTransform.gameObject, newLinkParam);
			}
		}

		/// <summary>
		/// 재활용을 위해 OptTransform 객체를 찾자
		/// </summary>
		public apOptTransform FindOptTransform(apTransform_Mesh meshTransform, apTransform_MeshGroup meshGroupTransform)
		{
			//return null;

			//_stopwatches[TIMER_TYPE.OptTransform].Start();

			apBakeLinkParam resultLinkParam = null;
			if(meshTransform != null)
			{
				int transformID = meshTransform._transformUniqueID;

				if (_totalParams_OptTransform.ContainsKey(transformID))
				{
					apBakeLinkParam curParam = null;
					for (int i = 0; i < _totalParams_OptTransform[transformID].Count; i++)
					{
						curParam = (_totalParams_OptTransform[transformID])[i];
						if (!curParam._isRecycled && curParam._isOptGameObject && curParam._optTransform != null)
						{
							if (curParam._optTransform._unitType == apOptTransform.UNIT_TYPE.Mesh
								&& curParam._optTransform._transformID == meshTransform._transformUniqueID)
							{
								resultLinkParam = curParam;
								break;
							}
						}
					}
				}
			}
			else if(meshGroupTransform != null)
			{
				int transformID = meshGroupTransform._transformUniqueID;
				if (_totalParams_OptTransform.ContainsKey(transformID))
				{
					apBakeLinkParam curParam = null;
					for (int i = 0; i < _totalParams_OptTransform[transformID].Count; i++)
					{
						curParam = (_totalParams_OptTransform[transformID])[i];

						if (!curParam._isRecycled && curParam._isOptGameObject && curParam._optTransform != null)
						{
							if (curParam._optTransform._unitType == apOptTransform.UNIT_TYPE.Group
								&& curParam._optTransform._transformID == meshGroupTransform._transformUniqueID)
							{
								resultLinkParam = curParam;
								break;
							}
						}

					}
				}
			}

			//_stopwatches[TIMER_TYPE.OptTransform].Stop();
			//_timerCalls[TIMER_TYPE.OptTransform] = _timerCalls[TIMER_TYPE.OptTransform] + 1;

			if(resultLinkParam == null)
			{
				return null;
			}

			resultLinkParam._isRecycled = true;//<<Recycle 된걸로 전환



			return resultLinkParam._optTransform;
		}


		/// <summary>
		/// 재활용을 위해 OptMesh 객체를 찾자
		/// ParentOptTransform은 Recycle 되었을 수 있다.
		/// </summary>
		public apOptMesh FindOptMesh(apOptTransform parentOptTransform)
		{
			//return null;

			//_stopwatches[TIMER_TYPE.OptMesh].Start();

			apBakeLinkParam resultLinkParam = null;

			int transformID = parentOptTransform._transformID;

			if (_totalParams_OptMesh.ContainsKey(transformID))
			{
				apBakeLinkParam curParam = null;
				for (int i = 0; i < _totalParams_OptMesh[transformID].Count; i++)
				{
					curParam = (_totalParams_OptMesh[transformID])[i];

					if (!curParam._isRecycled && curParam._isOptGameObject && curParam._optMesh != null && curParam._parentLinkParam != null)
					{
						if (curParam._parentLinkParam._optTransform != null &&
						curParam._parentLinkParam._optTransform == parentOptTransform)
						{
							resultLinkParam = curParam;
							break;
						}
					}
				}
			}
			
			//_stopwatches[TIMER_TYPE.OptMesh].Stop();
			//_timerCalls[TIMER_TYPE.OptMesh] = _timerCalls[TIMER_TYPE.OptMesh] + 1;

			if(resultLinkParam == null)
			{
				return null;
			}

			resultLinkParam._isRecycled = true;//<<Recycle 된걸로 전환
			return resultLinkParam._optMesh;
		}

		/// <summary>
		/// 재활용을 위해 OptNode를 리턴한다.
		/// 용도는 Transform 소켓
		/// </summary>
		/// <param name="parentOptTransform"></param>
		/// <returns></returns>
		public apOptNode FindOptTransformSocket(apOptTransform parentOptTransform)
		{
			//_stopwatches[TIMER_TYPE.OptNode].Start();

			apBakeLinkParam resultLinkParam = null;

			resultLinkParam = _totalParams_OptNode.Find(delegate (apBakeLinkParam a)
			{
				if(!a._isRecycled && a._isOptGameObject && a._optNode != null && a._parentLinkParam != null)
				{
					if(a._parentLinkParam._optTransform != null && 
					a._parentLinkParam._optTransform == parentOptTransform)
					{
						return true;
					}
				}
				return false;
			});
			

			//_stopwatches[TIMER_TYPE.OptNode].Stop();
			//_timerCalls[TIMER_TYPE.OptNode] = _timerCalls[TIMER_TYPE.OptNode] + 1;

			if(resultLinkParam == null)
			{
				return null;
			}

			resultLinkParam._isRecycled = true;//<<Recycle 된걸로 전환
			return resultLinkParam._optNode;
		}

		public apOptBone FindOptBone(apBone bone)
		{
			//_stopwatches[TIMER_TYPE.OptBone].Start();

			apBakeLinkParam resultLinkParam = null;

			int boneID = bone._uniqueID;
			if (_totalParams_OptBone.ContainsKey(boneID))
			{
				apBakeLinkParam curParam = null;
				for (int i = 0; i < _totalParams_OptBone[boneID].Count; i++)
				{
					curParam = (_totalParams_OptBone[boneID])[i];

					if (!curParam._isRecycled && curParam._isOptGameObject && curParam._optBone != null)
					{
						if (curParam._optBone._uniqueID == bone._uniqueID)
						{
							resultLinkParam = curParam;
							break;
						}
					}
				}
			}
			
			//_stopwatches[TIMER_TYPE.OptBone].Stop();
			//_timerCalls[TIMER_TYPE.OptBone] = _timerCalls[TIMER_TYPE.OptBone] + 1;

			if(resultLinkParam == null)
			{
				return null;
			}

			resultLinkParam._isRecycled = true;//<<Recycle 된걸로 전환
			return resultLinkParam._optBone;
		}

		/// <summary>
		/// 재활용을 위해 OptNode를 리턴한다.
		/// 용도는 Bone 소켓
		/// </summary>
		/// <param name="parentOptBone"></param>
		/// <returns></returns>
		public apOptNode FindOptBoneSocket(apOptBone parentOptBone)
		{
			//_stopwatches[TIMER_TYPE.OptNode].Start();

			apBakeLinkParam resultLinkParam = null;

			resultLinkParam = _totalParams_OptNode.Find(delegate (apBakeLinkParam a)
			{
				if(!a._isRecycled && a._isOptGameObject && a._optNode != null && a._parentLinkParam != null)
				{
					if(a._parentLinkParam._optBone != null && 
					a._parentLinkParam._optBone == parentOptBone)
					{
						return true;
					}
				}
				return false;
			});
			
			//_stopwatches[TIMER_TYPE.OptNode].Stop();
			//_timerCalls[TIMER_TYPE.OptNode] = _timerCalls[TIMER_TYPE.OptNode] + 1;


			if(resultLinkParam == null)
			{
				return null;
			}

			resultLinkParam._isRecycled = true;//<<Recycle 된걸로 전환
			return resultLinkParam._optNode;
		}

		/// <summary>
		/// BoneGroup에 해당하는 Node를 가져온다.
		/// Param 값이 100이므로 구분하기 쉽다.
		/// </summary>
		/// <returns></returns>
		public apOptNode FindOptBoneGroupNode()
		{
			//_stopwatches[TIMER_TYPE.OptNode].Start();

			apBakeLinkParam resultLinkParam = null;

			resultLinkParam = _totalParams_OptNode.Find(delegate (apBakeLinkParam a)
			{
				if(!a._isRecycled && a._isOptGameObject && a._optNode != null && a._optNode._param == 100)
				{
					return true;
				}
				return false;
			});
			
			//_stopwatches[TIMER_TYPE.OptNode].Stop();
			//_timerCalls[TIMER_TYPE.OptNode] = _timerCalls[TIMER_TYPE.OptNode] + 1;

			if(resultLinkParam == null)
			{
				return null;
			}

			resultLinkParam._isRecycled = true;//<<Recycle 된걸로 전환
			return resultLinkParam._optNode;
		}


		//public void PrintTimes()
		//{
		//	Debug.LogError("----------------------------------------------------------------");
		//	Debug.Log("Opt Transform [" + _timerCalls[TIMER_TYPE.OptTransform] + "] : " + _stopwatches[TIMER_TYPE.OptTransform].Elapsed.TotalSeconds);
		//	Debug.Log("Opt Mesh [" + _timerCalls[TIMER_TYPE.OptMesh] + "] : " + _stopwatches[TIMER_TYPE.OptMesh].Elapsed.TotalSeconds);
		//	Debug.Log("Opt Bone [" + _timerCalls[TIMER_TYPE.OptBone] + "] : " + _stopwatches[TIMER_TYPE.OptBone].Elapsed.TotalSeconds);
		//	Debug.Log("Opt Node [" + _timerCalls[TIMER_TYPE.OptNode] + "] : " + _stopwatches[TIMER_TYPE.OptNode].Elapsed.TotalSeconds);
		//	Debug.LogError("----------------------------------------------------------------");
			
		//}


		/// <summary>
		/// 처리 후 재배치를 한다.
		/// RootUnit이 재활용된 경우에는 이 함수를 호출한다.
		/// </summary>
		/// <param name="group1_Recycled"></param>
		/// <param name="group2_Remove"></param>
		/// <param name="group3_Unlinked"></param>
		public void SetHierarchyNotRecycledObjects(GameObject group1_Recycled, GameObject group2_Remove, GameObject group3_Unlinked, apBakeResult bakeResult)
		{
			//재활용되지 않은 GameObject들을 적절히 나누어서 배치한다.
			//재귀적으로 호출한다.
			if(_rootParam == null)
			{
				return;
			}
			SetHierarchyNotRecycledObjectsRecursive(_rootParam, group1_Recycled, group2_Remove, group3_Unlinked, bakeResult);
		}

		private void SetHierarchyNotRecycledObjectsRecursive(apBakeLinkParam targetLinkParam, GameObject group1_Recycled, GameObject group2_Remove, GameObject group3_Unlinked, apBakeResult bakeResult)
		{

			//재활용되지 않은 GameObject들을 적절히 나누어서 배치한다.
			//재귀적으로 호출한다.
			bool isChildCall = true;
			if (!targetLinkParam._isRecycled && !targetLinkParam._isReGroupCompleted)
			{
				//재활용 안된거 발견
				if (targetLinkParam._isOptGameObject)
				{
					//1. OptGameObject인데 재활용이 안되었다.
					//-> 컴포넌트 상태에 따라 [삭제 예정] 그룹에 넣을지 [링크 깨짐] 그룹에 넣을지 결정
					if (targetLinkParam._isOtherComponentExist)
					{
						//알 수 없는 컴포넌트 -> 링크 깨짐 그룹
						targetLinkParam._prevGameObject.transform.parent = group3_Unlinked.transform;

						//Count+1 : Unlink
						bakeResult.Add_UnlinkedExternalObject(targetLinkParam._prevGameObject.name);
					}
					else
					{
						//걍 재활용 실패 -> 삭제 예정
						targetLinkParam._prevGameObject.transform.parent = group2_Remove.transform;

						//Count+1 : Removed
						bakeResult.AddCount_RemovedOptGameObject();
					}
					
					targetLinkParam._isReGroupCompleted = true;//재배치 끝
				}
				else
				{
					//2. OptGameObject가 아니다.
					//-> 음.. 뭘 참조하고 있었을까요..
					//참조는 모르겠고 하이라키만 맞춰줍시다.
					//Parent가 있을때 + 해당 Parent가 Opt이며 + 재활용에 성공했을 경우에 한해서 재연결을 해준다.
					//재연결에 성공한 경우 -> Child Transform들을 재귀적으로 다시 연결해준다. (조건 안봄)
					//그 외에는 [링크 깨짐] 그룹에 넣는다.
					
					if(targetLinkParam._parentLinkParam != null && 
						targetLinkParam._parentLinkParam._isOptGameObject &&
						targetLinkParam._parentLinkParam._isRecycled)
					{
						targetLinkParam._isReGroupCompleted = true;//재배치 끝
						SetHierarchyForceRecursive(targetLinkParam, targetLinkParam._parentLinkParam._prevGameObject, bakeResult, true);
						isChildCall = false;

					}
					else
					{
						targetLinkParam._prevGameObject.transform.parent = group3_Unlinked.transform;
						targetLinkParam._isReGroupCompleted = true;//재배치 끝
					}
				}
			}

			//자식 객체들도 넣어주자
			if (isChildCall)
			{
				if (targetLinkParam._childLinkParams != null)
				{
					for (int i = 0; i < targetLinkParam._childLinkParams.Count; i++)
					{
						SetHierarchyNotRecycledObjectsRecursive(targetLinkParam._childLinkParams[i], group1_Recycled, group2_Remove, group3_Unlinked, bakeResult);
					}
				}
			}
		}



		/// <summary>
		/// 처리 후 재배치를 한다.
		/// RootUnit이 재활용이 "안된" 경우에 이 함수를 호출한다.
		/// Unlink 그룹으로 옮길 것을 옮겨준다.
		/// </summary>
		/// <param name="group1_Recycled"></param>
		/// <param name="group2_Remove"></param>
		/// <param name="group3_Unlinked"></param>
		public void SetHierarchyToUnlink(GameObject group3_Unlinked, apBakeResult bakeResult)
		{
			//재활용되지 않은 GameObject들을 적절히 나누어서 배치한다.
			//재귀적으로 호출한다.
			if(_rootParam == null)
			{
				return;
			}
			SetHierarchyToUnlinkRecursive(_rootParam, group3_Unlinked, bakeResult);
		}

		private void SetHierarchyToUnlinkRecursive(apBakeLinkParam targetLinkParam, GameObject group3_Unlinked, apBakeResult bakeResult)
		{
			//재활용되지 않은 GameObject들을 적절히 나누어서 배치한다. (모두 Recycle은 안되었을 것)
			//재귀적으로 호출한다.
			bool isChildCall = true;
			if (!targetLinkParam._isReGroupCompleted)
			{
				//재활용 안된거 발견
				if (targetLinkParam._isOptGameObject)
				{
					//1. OptGameObject인데 재활용이 안되었다.
					//-> 컴포넌트 상태에 따라 [삭제 예정] 그룹에 넣을지 [링크 깨짐] 그룹에 넣을지 결정
					if (targetLinkParam._isOtherComponentExist)
					{
						//알 수 없는 컴포넌트 -> 링크 깨짐 그룹
						targetLinkParam._prevGameObject.transform.parent = group3_Unlinked.transform;

						bakeResult.Add_UnlinkedExternalObject(targetLinkParam._prevGameObject.name);
					}
					else
					{
						//걍 재활용 실패 -> 삭제 예정
						//따로 처리는 안합니더
						//targetLinkParam._prevGameObject.transform.parent = group2_Remove.transform;
					}
					
					targetLinkParam._isReGroupCompleted = true;//재배치 끝
				}
				else
				{
					//2. OptGameObject가 아니다.
					//Parent가 재활용에 성공할 리가 없으므로
					//그냥 무조건 Unlink로 연결
					SetHierarchyForceRecursive(targetLinkParam, group3_Unlinked, bakeResult, false);
					targetLinkParam._isReGroupCompleted = true;//재배치 끝	
					isChildCall = false;//자식 객체로의 호출은 더이상 네이버
					
				}
			}

			if (isChildCall)
			{
				//자식 객체들도 넣어주자
				if (targetLinkParam._childLinkParams != null)
				{
					for (int i = 0; i < targetLinkParam._childLinkParams.Count; i++)
					{
						SetHierarchyToUnlinkRecursive(targetLinkParam._childLinkParams[i], group3_Unlinked, bakeResult);
					}
				}
			}
		}



		private void SetHierarchyForceRecursive(apBakeLinkParam targetLinkParam, GameObject parentGameObject, apBakeResult bakeResult, bool isAddToRelink)
		{
			if (!targetLinkParam._isRecycled)
			{
				targetLinkParam._prevGameObject.transform.parent = parentGameObject.transform;
				targetLinkParam._isReGroupCompleted = true;
				
				//Bake Result도 추가
				if(isAddToRelink)
				{
					//Count+1 : Relink
					bakeResult.Add_ReLinkedExternalObject(targetLinkParam._prevGameObject.name);
				}
				else
				{
					//Count+1 : Unlink
					bakeResult.Add_UnlinkedExternalObject(targetLinkParam._prevGameObject.name);
				}
			}

			if(targetLinkParam._childLinkParams != null)
			{
				for (int i = 0; i < targetLinkParam._childLinkParams.Count; i++)
				{
					SetHierarchyForceRecursive(targetLinkParam._childLinkParams[i], targetLinkParam._prevGameObject, bakeResult, isAddToRelink);
				}
			}
		}
	}

	





	public class apBakeLinkParam
	{
		// Members
		//-----------------------------------------
		public GameObject _prevGameObject = null;

		/// <summary>재활용이 되었는가</summary>
		public bool _isRecycled = false;
		public bool _isReGroupCompleted = false;//<<Recycled가 안된 객체들을 재배치하였는가

		/// <summary>
		/// apOpt 계열의 GameObject인가
		/// 아니라면 이 GameObject 자체가 외부에서 만든 커스터마이즈 GameObject이다.
		/// </summary>
		public bool _isOptGameObject = false;

		//주요 컴포넌트를 확인하자
		//만약 [제거될 apOptGameObject]라고 할지라도
		//[예상치 못한 컴포넌트]가 포함되어 있다면 제거하는게 아니라 "Unlinked" 그룹에 옮겨야 한다.

		public apOptRootUnit _optRoot = null;
		public apOptNode _optNode = null;
		public apOptBone _optBone = null;
		public apOptTransform _optTransform = null;
		public apOptMesh _optMesh = null;

		//OptMesh에 한해서
		public MeshRenderer _meshRenderer = null;
		public MeshFilter _meshFilter = null;

		//그 외의 컴포넌트가 있다면..
		//삭제 대상의 GameObject를 삭제하지 말고 Unlinked Group에 옮겨야 한다.
		public bool _isOtherComponentExist = false;


		//연결 방식은 트리 구조
		public apBakeLinkParam _parentLinkParam = null;
		public List<apBakeLinkParam> _childLinkParams = null;


		// Init
		//-----------------------------------------
		public apBakeLinkParam()
		{
			_parentLinkParam = null;
			_childLinkParams = null;
		}


		public void Parse(GameObject targetGameObject, apBakeLinkParam parentLinkParam)
		{
			_prevGameObject = targetGameObject;
			_isRecycled = false;
			_isReGroupCompleted = false;

			//Parent-Child를 서로 연결하자
			_parentLinkParam = parentLinkParam;
			if (parentLinkParam != null)
			{
				if (_parentLinkParam._childLinkParams == null)
				{
					_parentLinkParam._childLinkParams = new List<apBakeLinkParam>();
				}
				_parentLinkParam._childLinkParams.Add(this);
			}


			//Component를 보고 이 객체의 성격을 정의하자
			_isOptGameObject = false;

			_optRoot = null;
			_optNode = null;
			_optBone = null;
			_optTransform = null;
			_optMesh = null;

			_meshRenderer = null;
			_meshFilter = null;

			_isOtherComponentExist = false;


			MeshRenderer compMeshRenderer = null;
			MeshFilter compMeshFilter = null;

			Component[] components = _prevGameObject.GetComponents(typeof(Component));
			if (components != null)
			{
				Component curComp = null;
				for (int i = 0; i < components.Length; i++)
				{
					curComp = components[i];
					if(curComp is Transform)
					{
						//? 이건 필수로 들어가는 겁니더..
						continue;
					}
					

					if(curComp is apOptRootUnit)
					{
						_optRoot = curComp as apOptRootUnit;
					}
					else if(curComp is apOptTransform)
					{
						_optTransform = curComp as apOptTransform;
					}
					else if(curComp is apOptNode)
					{
						_optNode = curComp as apOptNode;
					}
					else if(curComp is apOptBone)
					{
						_optBone = curComp as apOptBone;
					}
					else if(curComp is apOptMesh)
					{
						_optMesh = curComp as apOptMesh;
					}
					else if(curComp is MeshRenderer)
					{
						compMeshRenderer = curComp as MeshRenderer;
					}
					else if(curComp is MeshFilter)
					{
						compMeshFilter = curComp as MeshFilter;
					}
					else
					{
						//알 수 없는 컴포넌트가 있다.
						_isOtherComponentExist = true;
					}
				}
			}

			if(	_optRoot != null ||
				_optNode != null ||
				_optBone != null ||
				_optTransform != null ||
				_optMesh != null)
			{
				// Opt 계열의 컴포넌트가 있다면
				_isOptGameObject = true;
			}

			//만약 MeshRenderer/MeshFilter가 있을때
			//_optMesh가 있다면 포함가능,
			//그 외에는 "알 수 없는 컴포넌트"로 지정한다.
			if(compMeshRenderer != null)
			{
				if(_optMesh != null)
				{
					_meshRenderer = compMeshRenderer;
				}
				else
				{
					_isOtherComponentExist = true;
				}
			}

			if(compMeshFilter != null)
			{
				if(_optMesh != null)
				{
					_meshFilter = compMeshFilter;
				}
				else
				{
					_isOtherComponentExist = true;
				}
			}
			
		}
	}
	
	/// <summary>
	/// Bake 결과를 저장했다가 리턴해주는 함수
	/// 새로 생성한 수, 다시 재활용된 수, 다시 연결된 외부 오브젝트 수, 연결이 끊긴 오브젝트 수(와 이름)를 기록한다.
	/// </summary>
	public class apBakeResult
	{
		// Members
		//------------------------------------------------
		private int _numNewGameObject = 0;
		private int _numRecycledGameObject = 0;
		private int _numRemovedGameObject = 0;
		private List<string> _relinkedExternalObjects = new List<string>();
		private List<string> _unlinkedExternalObjects = new List<string>();

		// Init
		//------------------------------------------------
		public apBakeResult()
		{
			Clear();
		}

		public void Clear()
		{
			_numNewGameObject = 0;
			_numRecycledGameObject = 0;
			_numRemovedGameObject = 0;
			_relinkedExternalObjects.Clear();
			_unlinkedExternalObjects.Clear();
		}

		// Add Result
		//------------------------------------------------
		public void AddCount_NewOptGameObject()
		{
			_numNewGameObject++;
		}

		public void AddCount_RecycledOptGameObject()
		{
			_numRecycledGameObject++;
		}

		public void AddCount_RemovedOptGameObject()
		{
			_numRemovedGameObject++;
		}

		public void Add_ReLinkedExternalObject(string gameObjectName)
		{
			_relinkedExternalObjects.Add(gameObjectName);
		}

		public void Add_UnlinkedExternalObject(string gameObjectName)
		{
			_unlinkedExternalObjects.Add(gameObjectName);
		}



		// Get
		//------------------------------------------------
		public int NumOptGameObject_New { get { return _numNewGameObject; } }
		public int NumOptGameObject_Recycled { get { return _numRecycledGameObject; } }
		public int NumOptGameObject_Removed { get
			{
				return _numRemovedGameObject;
			}
		}

		public int NumRelinkedExternalObject
		{
			get
			{
				return _relinkedExternalObjects.Count;
			}
		}
		public int NumUnlinkedExternalObject
		{
			get
			{
				return _unlinkedExternalObjects.Count;
			}
		}

		

	}
}