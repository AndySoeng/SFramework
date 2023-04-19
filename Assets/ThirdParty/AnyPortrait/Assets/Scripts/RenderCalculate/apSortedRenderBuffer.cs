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
	/// apRenderUnit을 정렬할 때 사용되는 버퍼.
	/// 1차적으로 정렬된 리스트를 입력하고, "Depth 변경"이벤트를 입력하면 적당히 정렬된 리스트를 제공한다.
	/// </summary>
	public class apSortedRenderBuffer
	{
		// Members
		//--------------------------------------------------

		//"정렬된" RenderUnit을 받으면, Unit은 1차적으로 동일한 순서로 생성되어 리스트에 정리됨과 동시에
		//나중에 Depth Swap에 사용될 "GroupSize"를 계산한다.
		//GroupSize만 있다면 순서 정렬이 쉽다.
		public class BufferData
		{
			public int _indexOriginal = 0;
			public int _indexChanged = 0;
			public int _level = 0;
			public int _groupSize = 0;
			public apRenderUnit _renderUnit = null;
			public bool _isMesh = false;
			public bool _isClippedChild = false;
			public bool _isClippingParent = false;
			public BufferData _parent = null;//상위 MeshGroup

			//추가 21.8.6 : Depth 변경이 요청된 경우엔 별도의 데이터가 들어간다.
			//"Requested"인 Buffer끼리 비교하고자 한다면, Depth 변화량 값을 저장해서 비교해야한다. (indexChanged로 하면 안됨)
			public bool _isRequested = false;
			public bool _isRequestedAndProcessed = false;//Requested였는데 처리가 완료되었는가
			public DepthRequestMeta _linkedRequestedMeta = null;


			
			public BufferData(int index, apRenderUnit renderUnit)
			{
				_indexOriginal = index;
				_indexChanged = index;
				_renderUnit = renderUnit;
				_level = renderUnit._level;

				_isMesh = (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh);
				_isClippedChild = false;
				_isClippingParent = false;

				if (_isMesh)
				{
					_isClippedChild = _renderUnit._meshTransform._isClipping_Child;
					_isClippingParent = _renderUnit._meshTransform._isClipping_Parent;
				}
				_parent = null;

				_isRequested = false;
				_isRequestedAndProcessed = false;
				_linkedRequestedMeta = null;
			}
			
		}

		
		private Dictionary<apRenderUnit, BufferData> _renderUnit2Buff = new Dictionary<apRenderUnit, BufferData>();
		private int _nRenderUnits = 0;
		private BufferData[] _buffers = null;
		private BufferData[] _buffers_DepthChanged = null;
		
		private bool _isDepthChanged = false;
		private bool _isNeedToSortDepthChangedBuffers = true;
		private int _nDepthChangedRequest = 0;
		private Dictionary<apRenderUnit, int> _depthChangedRequests = new Dictionary<apRenderUnit, int>();
		
		private Dictionary<apRenderUnit, int> _depthChangedCache = new Dictionary<apRenderUnit, int>();


		//추가 21.8.6 : 정렬된 List 타입으로 해야 "일관성있는 변경"이 가능하다.
		//단, 메모리 생성/해제를 막기 위해, Pool을 이용한다.
		public class DepthRequestMeta
		{
			public apRenderUnit _renderUnit = null;
			public BufferData _bufferData = null;
			public int _deltaDepth = 0;
			public int _indexOriginal = 0;
			public int _indexExpected = 0;

			public DepthRequestMeta()
			{
				Clear();
			}

			public void Clear()
			{
				_renderUnit = null;
				_bufferData = null;
				_deltaDepth = 0;
				_indexOriginal = 0;
				_indexExpected = 0;
			}

			public void SetRequest(apRenderUnit renderUnit, BufferData bufferData, int deltaDepth)
			{
				_renderUnit = renderUnit;
				_bufferData = bufferData;
				_deltaDepth = deltaDepth;
				_indexOriginal = bufferData._indexOriginal;
				_indexExpected = bufferData._indexOriginal + _deltaDepth;

				//이 버퍼 데이터는 Depth가 변경될 예정임을 알려준다.
				_bufferData._isRequested = true;
				_bufferData._isRequestedAndProcessed = false;
				_bufferData._linkedRequestedMeta = this;
			}
		}
		private List<DepthRequestMeta> _depthRequstMeta_Pool = null;
		private const int INIT_REQUEST_META_POOL_SIZE = 10;
		private const int INCREMENT_REQUEST_META_POOL_SIZE = 5;
		private int _nDepthRequestMeta_PoolSize = 0;
		private int _nDepthRequestMeta_PoolPopped = 0;
		private DepthRequestMeta _cal_CurRequestMeta = null;

		private List<DepthRequestMeta> _sortedDepthRequestMeta = null;
		private int _nSortedDepthRequestMeta = 0;
		
		//참조를 위한 RednerUnit 리스트 변수
		//처음 설정과 정렬시 갱신된다.
		private List<apRenderUnit> _renderUnits_Original = new List<apRenderUnit>();
		private List<apRenderUnit> _renderUnits_Sorted = new List<apRenderUnit>();


		// Init
		//--------------------------------------------------
		public apSortedRenderBuffer()
		{
			Init();
		}

		public void Init()
		{
			_renderUnit2Buff.Clear();
			_nRenderUnits = 0;

			_buffers = null;
			_buffers_DepthChanged = null;
			

			_isDepthChanged = false;
			_isNeedToSortDepthChangedBuffers = true;//<<캐시가 초기화되었으므로 일단 무조건 다시 Sort해야한다.
			_nDepthChangedRequest = 0;
			_depthChangedRequests.Clear();
			_depthChangedCache.Clear();

			_renderUnits_Original.Clear();
			_renderUnits_Sorted.Clear();


			//추가 21.8.6 : Depth를 적용할때 Request 들어온 순서대로 할게 아니라, 한번 Sort를 하고 나서 순차적으로 하는 것이 좋다.
			if (_depthRequstMeta_Pool == null)
			{
				_depthRequstMeta_Pool = new List<DepthRequestMeta>();
				
				//미리 메타 정보를 만들어두자.				
				for (int i = 0; i < INIT_REQUEST_META_POOL_SIZE; i++)
				{
					_depthRequstMeta_Pool.Add(new DepthRequestMeta());
				}
			}
			_nDepthRequestMeta_PoolSize = _depthRequstMeta_Pool.Count;
			_nDepthRequestMeta_PoolPopped = 0;

			

			if(_sortedDepthRequestMeta == null)
			{
				_sortedDepthRequestMeta = new List<DepthRequestMeta>();
			}
			_sortedDepthRequestMeta.Clear();
			_nSortedDepthRequestMeta = 0;

		}


		// Functions
		//--------------------------------------------------
		/// <summary>
		/// MeshGroup의 _renderUnits_All이 생성되어 정렬된 직후 이 함수를 호출해야한다.
		/// "기본적인 RenderUnit 순서" 데이터를 생성한다.
		/// 이때 "RenderUnit의 그룹 크기"를 미리 생성하기 때문에 꼭 필요하다.
		/// </summary>
		/// <param name="renderUnits"></param>
		public void SetSortedRenderUnits(List<apRenderUnit> renderUnits)
		{
			Init();

			apRenderUnit renderUnit = null;
			_nRenderUnits = renderUnits.Count;
			if(_nRenderUnits == 0)
			{
				//렌더링할 게 없는데용..
				return;
			}
			
			//버퍼를 생성한다.
			_isNeedToSortDepthChangedBuffers = true;
			_buffers = new BufferData[_nRenderUnits];
			_buffers_DepthChanged = new BufferData[_nRenderUnits];
			

			for (int i = 0; i < renderUnits.Count; i++)
			{
				renderUnit = renderUnits[i];

				//일단 순서대로 버퍼에 넣는다.
				//RenderUnit -> Buffer를 참조하기 위한 매핑 리스트에도 추가
				BufferData newBuff = new BufferData(i, renderUnit);
				_buffers[i] = newBuff;
				_renderUnit2Buff.Add(renderUnit, newBuff);

				//리스트에도 넣는다.
				_renderUnits_Original.Add(renderUnit);
			}

			
			//Buffer Unit을 앞에서부터 돌면서 "Group Size"와 "Parent"를 계산한다.
			BufferData curBuf = null;
			for (int i = 0; i < _buffers.Length; i++)
			{
				curBuf = _buffers[i];

				//Parent 먼저 연결한다.
				if (curBuf._renderUnit._parentRenderUnit != null)
				{
					if (_renderUnit2Buff.ContainsKey(curBuf._renderUnit._parentRenderUnit))
					{
						BufferData parentBuf = _renderUnit2Buff[curBuf._renderUnit._parentRenderUnit];
						curBuf._parent = parentBuf;
					}
				}

				curBuf._groupSize = 1;//일단 자기 자신 포함
									  //GroupSize

				if (curBuf._renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh
					&& curBuf._renderUnit._meshTransform != null)
				{
					//1. Mesh인 경우 : Clipping Parent일 때 -> Clipped Child 만큼이 GroupSize
					if (curBuf._renderUnit._meshTransform._isClipping_Parent)
					{
						//Clipped되는 메시 개수만큼 추가
						curBuf._groupSize += curBuf._renderUnit._meshTransform._clipChildMeshes.Count;
					}
				}
				else if (curBuf._renderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode
					&& curBuf._renderUnit._meshGroupTransform != null)
				{
					//2. MeshGroup인 경우 : 자식들 모두 확인
					//다음 리스트를 확인하면서
					//"자식들(더 큰 Level)을 카운트"한다.
					//자신과 동일/상위 레벨 (같거나 작은 Level)인 경우 카운트를 중단한다.
					int curIndex = curBuf._indexOriginal + 1;
					BufferData nextBuff = null;

					while (true)
					{
						if (curIndex >= _buffers.Length)
						{
							break;
						}
						nextBuff = _buffers[curIndex];
						if (nextBuff._level > curBuf._level)
						{
							//자식이면 카운트
							curBuf._groupSize++;
						}
						else
						{
							//형제거나 부모라면 카운트 중지
							break;
						}

						curIndex++;
					}
				}


			}
		}


		/// <summary>
		/// 업데이트 초반에 호출하자
		/// </summary>
		public void ReadyToUpdate()
		{
			_isDepthChanged = false;
			if(_nDepthChangedRequest > 0)
			{
				_nDepthChangedRequest = 0;
				_depthChangedRequests.Clear();
			}
			
		}

		/// <summary>
		/// Extra 이벤트에 의해서 Depth를 바꿔야 하는 경우 호출
		/// </summary>
		/// <param name="renderUnit"></param>
		/// <param name="deltaDepth"></param>
		public void OnExtraDepthChanged(apRenderUnit renderUnit, int deltaDepth)
		{
			if(deltaDepth == 0)
			{
				return;
			}
			if(renderUnit._meshTransform != null && renderUnit._meshTransform._isClipping_Child)
			{
				//Clipping Child의 Depth 이동은 허용하지 않는다.
				return;
			}

			//Debug.Log("OnExtraDepthChanged [" + renderUnit.Name + " - "+ deltaDepth + "]");

			//일단 DepthChanged 이벤트가 발생했음을 알리고
			_isDepthChanged = true;
			_nDepthChangedRequest++;

			//어떤 RenderUnit의 Depth가 바뀌었는지 저장한다.
			if(_depthChangedRequests.ContainsKey(renderUnit))
			{
				//키가 있을리가 없는데..
				_depthChangedRequests[renderUnit] = deltaDepth;
			}
			else
			{
				_depthChangedRequests.Add(renderUnit, deltaDepth);
			}

			//캐시 미스 여부를 찾는다.
			if(!_depthChangedCache.ContainsKey(renderUnit))
			{
				//만약 캐시에 없는 거라면 -> 정렬을 다시 해야한다.
				_isNeedToSortDepthChangedBuffers = true;
				//Debug.LogError(">> Cache Miss (New Data)");
			}
			else if(_depthChangedCache[renderUnit] != deltaDepth)
			{
				//만약 캐시와 값이 다르다면 -> 정렬을 다시 해야한다.
				_isNeedToSortDepthChangedBuffers = true;
				//Debug.LogError(">> Cache Miss (Delta Changed)");
			}
		}
		

		/// <summary>
		/// 모든 RenderUnit 업데이트가 끝나고, Depth 이벤트에 따라 출력 순서를 바꾸어야 하는지 확인한다.
		/// </summary>
		public void UpdateDepthChangedEventAndBuffers()
		{
			if(_nRenderUnits == 0)
			{
				//렌더 유닛이 없당..
				return;
			}
			if(!_isDepthChanged || _nDepthChangedRequest == 0)
			{
				//Depth가 바뀐 적이 없다.
				return;
			}

			//Debug.LogWarning("Extra Depth 변경됨 : " + _nDepthChangedRequest + "개의 요청");
			//foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)
			//{
			//	Debug.LogWarning("[" + request.Key.Name + "] Depth : " + request.Value);
			//}

			if(!_isNeedToSortDepthChangedBuffers)
			{
				//Depth 변경 캐시가 모두 히트했을 때,
				//개수까지 같아야 인정.
				if(_depthChangedRequests.Count != _depthChangedCache.Count)
				{
					_isNeedToSortDepthChangedBuffers = true;
				}
			}


			if(!_isNeedToSortDepthChangedBuffers)
			{
				//재정렬을 할 필요가 없다. 예쓰!
				//Debug.Log("ReSort -> Cache Hit");
				return;
			}

			//Debug.LogError("Cache Miss : 다시 정렬을 해야한다. [" + _nDepthChangedRequest + "개의 요청]");
			


			//추가 21.8.6
			//기존에 Request > Index Changed를 했다면,
			//Request > Request Meta + Sorted > Index Changed를 해서 일관성을 확보하자
			//추가 21.8.6 : Depth를 적용할때 Request 들어온 순서대로 할게 아니라, 한번 Sort를 하고 나서 순차적으로 하는 것이 좋다.
			_nDepthRequestMeta_PoolPopped = 0;
			_sortedDepthRequestMeta.Clear();
			_nSortedDepthRequestMeta = 0;
			_cal_CurRequestMeta = null;



			
			//재정렬을 해야한다.
			//흐아앙

			//변경 21.8.6 : 이 부분 호출 순서가 Request보다 먼저 하도록 변경
			//먼저 Buffer_DepthChanged를 복사하여 붙여넣는다.
			//이때, Index_Changed는 원래대로 돌려놓는다.
			BufferData curBufferData = null;
			for (int i = 0; i < _nRenderUnits; i++)
			{
				curBufferData = _buffers[i];
				curBufferData._indexChanged = curBufferData._indexOriginal;

				//추가 21.8.6 : 변경 요청과 관련된 변수 초기화 (일단..)
				curBufferData._isRequested = false;
				curBufferData._linkedRequestedMeta = null;

				_buffers_DepthChanged[i] = curBufferData;
			}


			
			//_buffers_DepthChanged.Clear();

			//일단 재정렬을 할 예정이니 캐시는 현재 값으로 갱신
			_isNeedToSortDepthChangedBuffers = false;
			_depthChangedCache.Clear();
			foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)
			{
				_depthChangedCache.Add(request.Key, request.Value);



				//추가 21.8.6 : RequestMeta를 생성하자
				//Pool에서 하나 가져온다.
				if(_nDepthRequestMeta_PoolPopped >= _nDepthRequestMeta_PoolSize)
				{
					
					//Pool을 더 크게 만들어야 한다.
					for (int iInc = 0; iInc < INCREMENT_REQUEST_META_POOL_SIZE; iInc++)
					{
						_depthRequstMeta_Pool.Add(new DepthRequestMeta());
					}
					//Debug.Log("풀 확장 : " + _nDepthRequestMeta_PoolSize + " > " + _depthRequstMeta_Pool.Count);

					_nDepthRequestMeta_PoolSize = _depthRequstMeta_Pool.Count;
					
				}

				_cal_CurRequestMeta = _depthRequstMeta_Pool[_nDepthRequestMeta_PoolPopped];
				_cal_CurRequestMeta.SetRequest(request.Key, _renderUnit2Buff[request.Key], request.Value);
				_nDepthRequestMeta_PoolPopped += 1;//Pop 카운트 증가

				//Pool에서 꺼내와서 리스트에 넣는다.
				_sortedDepthRequestMeta.Add(_cal_CurRequestMeta);
				_nSortedDepthRequestMeta += 1;
			}





			#region [미사용 코드] 이전 방식 (Request를 바로 하나씩 가져와서 Index 변경을 한다.)
			////이제 다시 정렬을 해보자
			////- 인덱스 스왑만 먼저 한다.
			////- 버퍼 정렬을 하려면.. Array가 필요하당..

			//apRenderUnit curRenderUnit = null;

			////Debug.Log("------------------------------");
			////DebugBuffers(_buffers, "원래 순서", false);

			//int deltaDepth = 0;


			////이 부분에 버그가 있다.
			////> Depth가 전체 범위를 벗어나는 경우, 즉 맨 앞이나 맨 뒤로 이동하는 요청이 2개 이상 있는 경우 우열을 가릴 수 없다.
			//foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)
			//{
			//	curRenderUnit = request.Key;
			//	curBufferData = _renderUnit2Buff[curRenderUnit];
			//	deltaDepth = request.Value;
			//	if(deltaDepth == 0)
			//	{
			//		continue;
			//	}

			//	//Debug.Log("> Request : [" + request.Key.Name + "]의 " + request.Value + " Depth 변경 요청 (현재 Index : " + curBufferData._indexChanged + " / Group Size : " + curBufferData._groupSize + ")");

			//	//BufferData의 인덱스 스왑을 한 후,
			//	//_buffers_DepthChanged에 변경된 인덱스에 맞게 넣는다.
			//	//얼마큼 많이 이동하는지 체크
			//	//실제 이동되는 인덱스
			//	int realMovedOffset = 0;
			//	//Depth만큼 이동하기 위한 카운트와 최대치
			//	int depthCount = 0;
			//	int maxDepthCount = Mathf.Abs(deltaDepth);
			//	int moveDir = (deltaDepth > 0) ? 1 : -1;


			//	int iCheck = (deltaDepth > 0) ? (curBufferData._indexChanged + curBufferData._groupSize) : (curBufferData._indexChanged - 1);

			//	BufferData nextBuff = null;
			//	while(true)
			//	{

			//		if(iCheck < 0 || iCheck >= _nRenderUnits)
			//		{
			//			//렌더 유닛 범위를 넘어갔다면
			//			break;
			//		}
			//		if(depthCount >= maxDepthCount)
			//		{
			//			//Depth 카운트를 모두 셌다면
			//			break;
			//		}

			//		nextBuff = _buffers_DepthChanged[iCheck];

			//		//- 자신보다 Level이 높은 경우(하위인 경우) : 카운트하지 않고 이동한다.
			//		//- 자신보다 Level이 같고 같은 Parent를 공유하는 경우 : 카운트 1개 하고 이동한다. 카운트 찬 경우 종료
			//		//- 자신보다 Level이 낮은 경우(상위인 경우) 또는 Level이 같아도 Parent를 공유하지 않는 경우(에러) : 이동 종료
			//		//- 만약 이동 도중 ClippingChild를 만나면 : 카운트하지 않고 이동한다.
			//		if(nextBuff._level > curBufferData._level)
			//		{
			//			//Level이 높거나(하위 레벨)이라면 패스
			//			realMovedOffset += moveDir;
			//			iCheck += moveDir;
			//			//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Next가 하위 레벨이다. (" + nextBuff._level + " > " + curBufferData._level + ")");
			//		}
			//		else if(nextBuff._level == curBufferData._level)
			//		{
			//			if(nextBuff._isClippedChild)
			//			{
			//				//같은 레벨의 ClippedChild라면 패스
			//				realMovedOffset += moveDir;
			//				iCheck += moveDir;

			//				//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Clipped Child이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
			//			}
			//			else if(nextBuff._parent != curBufferData._parent)
			//			{
			//				//Level이 같지만 Parent가 다르다면 사촌이다. 이 경우 이동 종료
			//				//Debug.LogError("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [이동 종료] Parent가 다른 사촌이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
			//				break;
			//			}
			//			else
			//			{
			//				//Level이 같고 Parent를 공유하면 같은 형제이므로 Depth 카운트를 하나 올리고 이동
			//				depthCount++;
			//				realMovedOffset += moveDir;
			//				iCheck += moveDir;
			//				//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트하고 이동(" + depthCount + ")] 형제 Unit이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
			//			}
			//		}
			//		else
			//		{
			//			//상위 레벨이라면 바로 종료
			//			//Debug.LogError("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [이동 종료] 상위 레벨이다. (" + nextBuff._level + " < " + curBufferData._level + ")");
			//			break;
			//		}

			//	}

			//	if(realMovedOffset == 0)
			//	{
			//		continue;
			//	}

			//	//Debug.Log("- 이동 범위 : " + realMovedOffset + " (Depth : " + deltaDepth + ")");

			//	//"이동할 간격"이 결정되면, 그 영역만큼 Index를 바꾸자
			//	int swappedIndex_Start = 0;
			//	int swappedIndex_End = 0;
			//	int nSwapped = Mathf.Abs(realMovedOffset);



			//	if(deltaDepth > 0)
			//	{
			//		swappedIndex_Start = curBufferData._indexChanged + curBufferData._groupSize;
			//		swappedIndex_End = swappedIndex_Start + (nSwapped - 1);

			//		//Debug.Log(">> Target Swap 범위 [" + swappedIndex_Start + "~" + swappedIndex_End + " : " + (-curBufferData._groupSize));

			//		//Depth가 증가했다면, 상대 위치는 GroupSize 만큼 감소해야한다.
			//		for (int i = swappedIndex_Start; i <= swappedIndex_End; i++)
			//		{
			//			_buffers_DepthChanged[i]._indexChanged -= curBufferData._groupSize;
			//		}
			//	}
			//	else
			//	{
			//		swappedIndex_Start = curBufferData._indexChanged - nSwapped;
			//		swappedIndex_End = swappedIndex_Start + (nSwapped - 1);

			//		//Debug.Log(">> Target Swap 범위 [" + swappedIndex_Start + "~" + swappedIndex_End + " : " + (+curBufferData._groupSize));

			//		//Depth가 감소했다면, 상대 위치는 GroupSize 만큼 증가해야한다.
			//		for (int i = swappedIndex_Start; i <= swappedIndex_End; i++)
			//		{
			//			_buffers_DepthChanged[i]._indexChanged += curBufferData._groupSize;
			//		}
			//	}

			//	//이제 해당 그룹을 이동시키자
			//	int groupIndex_Start = curBufferData._indexChanged;
			//	int groupIndex_End = curBufferData._indexChanged + (curBufferData._groupSize - 1);

			//	//Debug.Log(">> 움직이는 Group 범위 [" + groupIndex_Start + "~" + groupIndex_End + " : " + realMovedOffset);

			//	for (int i = groupIndex_Start; i <= groupIndex_End; i++)
			//	{
			//		_buffers_DepthChanged[i]._indexChanged += realMovedOffset;
			//	}

			//	//리스트 순서를 다시 정리
			//	//이번에는 변화된 위치로 직접 넣는다.
			//	BufferData movedBuf = null;

			//	//일단 배열은 초기화
			//	for (int i = 0; i < _nRenderUnits; i++)
			//	{
			//		_buffers_DepthChanged[i] = null;
			//	}

			//	//변경된 인덱스를 그대로 입력 (이게 결국 Sort)
			//	for (int i = 0; i < _nRenderUnits; i++)
			//	{
			//		movedBuf = _buffers[i];
			//		_buffers_DepthChanged[movedBuf._indexChanged] = movedBuf;
			//	}

			//	//DebugBuffers(_buffers_DepthChanged, "[" + request.Key.Name + "]의 " + request.Value + " Depth 변경 요청 후 결과", true);
			//} 
			#endregion


			//변경 21.8.6 : Request를 Request Meta로 옮긴 이후에 할 것
			if(_nSortedDepthRequestMeta > 1)
			{
				//Request Meta를 Sort먼저 한다.
				//"예상되는 위치가 높은 것" (앞에 렌더링될 것)을 먼저 계산해보자
				_sortedDepthRequestMeta.Sort(FuncMetaSort);
				
				//디버그 코드
				//Debug.Log("<<< Depth 요청값 리스트 [" + _nSortedDepthRequestMeta + "] >>>");
				
				//for (int i = 0; i < _nSortedDepthRequestMeta; i++)
				//{
				//	_cal_CurRequestMeta = _sortedDepthRequestMeta[i];
				//	Debug.Log("[" + i + "] : " + _cal_CurRequestMeta._renderUnit.Name + " / " + _cal_CurRequestMeta._indexOriginal + " > " 
				//		+ _cal_CurRequestMeta._indexExpected + " (" + _cal_CurRequestMeta._deltaDepth + " : " + (_cal_CurRequestMeta._deltaDepth > 0 ? "증가" : "감소") + ")");
				//}
			}



			//이제 다시 정렬을 해보자
			//- 인덱스 스왑만 먼저 한다.
			//- 버퍼 정렬을 하려면.. Array가 필요하당..

			//apRenderUnit curRenderUnit = null;//변경된 코드에서는 불필요 (삭제 21.8.6)
			
			
			int deltaDepth = 0;

			//이전
			//foreach (KeyValuePair<apRenderUnit, int> request in _depthChangedRequests)

			//변경 21.8.6 : Meta 단위로 체크한다.
			for (int iMeta = 0; iMeta < _nSortedDepthRequestMeta; iMeta++)
			{

				_cal_CurRequestMeta = _sortedDepthRequestMeta[iMeta];

				//이전
				//curRenderUnit = request.Key;
				//curBufferData = _renderUnit2Buff[curRenderUnit];
				//deltaDepth = request.Value;

				//변경 21.8.6
				curBufferData = _cal_CurRequestMeta._bufferData;
				deltaDepth = _cal_CurRequestMeta._deltaDepth;

				//처리 완료 표시
				curBufferData._isRequestedAndProcessed = true;

				if(deltaDepth == 0)
				{
					continue;
				}

				//Debug.LogWarning("[" + curBufferData._renderUnit.Name + "] : " + _cal_CurRequestMeta._indexOriginal + ">" + _cal_CurRequestMeta._indexExpected + " (" + _cal_CurRequestMeta._deltaDepth + ")");

				//Debug.Log("> Request : [" + request.Key.Name + "]의 " + request.Value + " Depth 변경 요청 (현재 Index : " + curBufferData._indexChanged + " / Group Size : " + curBufferData._groupSize + ")");

				//BufferData의 인덱스 스왑을 한 후,
				//_buffers_DepthChanged에 변경된 인덱스에 맞게 넣는다.
				//얼마큼 많이 이동하는지 체크
				//실제 이동되는 인덱스
				int realMovedOffset = 0;
				
				//Depth만큼 이동하기 위한 카운트와 최대치
				int depthCount = 0;
				int maxDepthCount = Mathf.Abs(deltaDepth);
				int moveDir = (deltaDepth > 0) ? 1 : -1;


				int iCheck = (deltaDepth > 0) ? (curBufferData._indexChanged + curBufferData._groupSize) : (curBufferData._indexChanged - 1);

				BufferData nextBuff = null;
				while(true)
				{
					
					if(iCheck < 0 || iCheck >= _nRenderUnits)
					{
						//렌더 유닛 범위를 넘어갔다면
						//Debug.LogError("  >> [" + iCheck + "] 범위를 초과했다.");
						break;
					}
					if(depthCount >= maxDepthCount)
					{
						//Depth 카운트를 모두 셌다면
						//Debug.LogWarning("  >> [" + iCheck + "] 이동 횟수를 충족했다. : " + maxDepthCount);
						break;
					}

					nextBuff = _buffers_DepthChanged[iCheck];
					
					//- 자신보다 Level이 높은 경우(하위인 경우) : 카운트하지 않고 이동한다.
					//- 자신보다 Level이 같고 같은 Parent를 공유하는 경우 : 카운트 1개 하고 이동한다. 카운트 찬 경우 종료
					//- 자신보다 Level이 낮은 경우(상위인 경우) 또는 Level이 같아도 Parent를 공유하지 않는 경우(에러) : 이동 종료
					//- 만약 이동 도중 ClippingChild를 만나면 : 카운트하지 않고 이동한다.
					if(nextBuff._level > curBufferData._level)
					{
						//Level이 높거나(하위 레벨)이라면 패스
						realMovedOffset += moveDir;
						iCheck += moveDir;
						//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Next가 하위 레벨이다. (" + nextBuff._level + " > " + curBufferData._level + ")");
					}
					else if(nextBuff._level == curBufferData._level)
					{
						if(nextBuff._isClippedChild)
						{
							//같은 레벨의 ClippedChild라면 패스
							realMovedOffset += moveDir;
							iCheck += moveDir;

							//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트없이 이동] Clipped Child이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
						}
						else if(nextBuff._parent != curBufferData._parent)
						{
							//Level이 같지만 Parent가 다르다면 사촌이다. 이 경우 이동 종료
							//Debug.LogError("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [이동 종료] Parent가 다른 사촌이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
							break;
						}
						else
						{
							//Level이 같고 Parent를 공유하면 같은 형제이므로 Depth 카운트를 하나 올리고 이동

							//추가 21.8.6
							//중요) 비교 대상이 "다른 Request"인 경우엔 추월을 못하는 조건
							//(추월 불가 조건)
							//- "마지막 이동"에서 만난 경우 (이거 삭제)
							//- "expectedIndex"가 작거나(증가방향) 클경우(감소방향) 추월 실패
							//- "expectedIndex"가 같다면 기존 IndexOrginal 비교
							if (nextBuff._isRequested
								&& nextBuff._isRequestedAndProcessed
								&& (
									nextBuff._indexChanged != nextBuff._linkedRequestedMeta._indexExpected//아직 더 이동할 수 있는데 이동하지 못했던 Request. 여기가 해당 위치가 마지막일 것이다.
									|| _cal_CurRequestMeta._indexExpected == nextBuff._linkedRequestedMeta._indexExpected//또는 도달 위치가 같다.
									)
								//&& depthCount == maxDepthCount - 1
								)
							{
								//Debug.Log("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name 
								//	+ " : Requested Buff : Changed " + nextBuff._indexChanged + " / Expected : " + nextBuff._linkedRequestedMeta._indexExpected);

								//Index Expected가 동일한 상태라면
								if(moveDir > 0)
								{
									//증가 방향에서
									if(_cal_CurRequestMeta._indexExpected < nextBuff._linkedRequestedMeta._indexExpected)
									{
										//Expected Index가 작아서 추월 불가
										//Debug.LogError("  >> [" + iCheck + "] 추월 불가 : Expected Index가 작다. (" + _cal_CurRequestMeta._indexExpected + " < " + nextBuff._linkedRequestedMeta._indexExpected +")");
										break;
									}
									else if(_cal_CurRequestMeta._indexExpected == nextBuff._linkedRequestedMeta._indexExpected
										&& _cal_CurRequestMeta._indexOriginal < nextBuff._linkedRequestedMeta._indexOriginal)
									{										
										//Expected Index가 동일한 경우, 기존 레이어 위치가 아래인 경우 추월 불가
										//Debug.LogError("  >> [" + iCheck + "] 추월 불가 : 동일한 위치에서 기본 순서를 유지한다. (" + _cal_CurRequestMeta._indexExpected + " / 기본 순서 : "
										//	+ _cal_CurRequestMeta._indexOriginal + ", " + nextBuff._linkedRequestedMeta._indexOriginal + ")");
										break;
									}
								}
								else
								{
									//감소 방향에서
									if(_cal_CurRequestMeta._indexExpected > nextBuff._linkedRequestedMeta._indexExpected)
									{
										//Expected Index가 커서 추월 불가
										//Debug.LogError("  >> [" + iCheck + "] 추월 불가 : Expected Index가 크다. (" + _cal_CurRequestMeta._indexExpected + " < " + nextBuff._linkedRequestedMeta._indexExpected +")");
										break;
									}
									else if(_cal_CurRequestMeta._indexExpected == nextBuff._linkedRequestedMeta._indexExpected
										&& _cal_CurRequestMeta._indexOriginal > nextBuff._linkedRequestedMeta._indexOriginal)
									{
										//Expected Index가 동일한 경우, 기존 레이어 위치가 위쪽인 경우 추월 불가
										//Debug.LogError("  >> [" + iCheck + "] 추월 불가 : 동일한 위치에서 기본 순서를 유지한다. (" + _cal_CurRequestMeta._indexExpected + " / 기본 순서 : "
										//	+ _cal_CurRequestMeta._indexOriginal + ", " + nextBuff._linkedRequestedMeta._indexOriginal + ")");
										break;
									}
								}
							}

							//한칸 이동한다.
							depthCount++;
							realMovedOffset += moveDir;
							iCheck += moveDir;
							//Debug.Log("  >> 이동 [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [카운트하고 이동(" + depthCount + ")] 형제 Unit이다. (" + nextBuff._level + " = " + curBufferData._level + ")");
						}
					}
					else
					{
						//상위 레벨이라면 바로 종료
						//Debug.LogError("  >> [" + iCheck + "] " + nextBuff._renderUnit.Name + " -> [이동 종료] 상위 레벨이다. (" + nextBuff._level + " < " + curBufferData._level + ")");
						break;
					}
					
				}

				if (realMovedOffset == 0)
				{
					//Debug.Log("이동량이 없다.");
					continue;
				}

				//Debug.Log("- 이동 범위 : " + realMovedOffset + " (Depth : " + deltaDepth + ")");

				//"이동할 간격"이 결정되면, 그 영역만큼 Index를 바꾸자
				int swappedIndex_Start = 0;
				int swappedIndex_End = 0;
				int nSwapped = Mathf.Abs(realMovedOffset);



				if(deltaDepth > 0)
				{
					swappedIndex_Start = curBufferData._indexChanged + curBufferData._groupSize;
					swappedIndex_End = swappedIndex_Start + (nSwapped - 1);

					//Debug.Log(">> Target Swap 범위 [" + swappedIndex_Start + "~" + swappedIndex_End + " : " + (-curBufferData._groupSize));

					//Depth가 증가했다면, 상대 위치는 GroupSize 만큼 감소해야한다.
					for (int i = swappedIndex_Start; i <= swappedIndex_End; i++)
					{
						_buffers_DepthChanged[i]._indexChanged -= curBufferData._groupSize;
					}
				}
				else
				{
					swappedIndex_Start = curBufferData._indexChanged - nSwapped;
					swappedIndex_End = swappedIndex_Start + (nSwapped - 1);

					//Debug.Log(">> Target Swap 범위 [" + swappedIndex_Start + "~" + swappedIndex_End + " : " + (+curBufferData._groupSize));

					//Depth가 감소했다면, 상대 위치는 GroupSize 만큼 증가해야한다.
					for (int i = swappedIndex_Start; i <= swappedIndex_End; i++)
					{
						_buffers_DepthChanged[i]._indexChanged += curBufferData._groupSize;
					}
				}

				//이제 해당 그룹을 이동시키자
				int groupIndex_Start = curBufferData._indexChanged;
				int groupIndex_End = curBufferData._indexChanged + (curBufferData._groupSize - 1);

				//Debug.Log(">> 움직이는 Group 범위 [" + groupIndex_Start + "~" + groupIndex_End + " : " + realMovedOffset);

				for (int i = groupIndex_Start; i <= groupIndex_End; i++)
				{
					_buffers_DepthChanged[i]._indexChanged += realMovedOffset;
				}

				//리스트 순서를 다시 정리
				//이번에는 변화된 위치로 직접 넣는다.
				BufferData movedBuf = null;

				//일단 배열은 초기화
				for (int i = 0; i < _nRenderUnits; i++)
				{
					_buffers_DepthChanged[i] = null;
				}

				
				//변경된 인덱스를 그대로 입력 (이게 결국 Sort)
				for (int i = 0; i < _nRenderUnits; i++)
				{
					movedBuf = _buffers[i];
					_buffers_DepthChanged[movedBuf._indexChanged] = movedBuf;
				}

				//DebugBuffers(_buffers_DepthChanged, "[" + request.Key.Name + "]의 " + request.Value + " Depth 변경 요청 후 결과", true);

				//Debug.Log("--- 중간 결과 ---");
				//for (int i = 0; i < _nRenderUnits; i++)
				//{
				//	Debug.Log("[" + i + "] " + _buffers_DepthChanged[i]._renderUnit != null ? _buffers_DepthChanged[i]._renderUnit.Name : "<None>");
				//}

			}







			//Debug.Log("------------------------------");
			_nDepthChangedRequest = 0;
			_depthChangedRequests.Clear();

			//리스트에도 넣자
			_renderUnits_Sorted.Clear();

			BufferData sortedBufferData = null;
			for (int i = 0; i < _buffers_DepthChanged.Length; i++)
			{
				sortedBufferData = _buffers_DepthChanged[i];
				//Debug.Log("[" + i + "] : " + sortedBufferData._renderUnit.Name);

				if (sortedBufferData != null && sortedBufferData._renderUnit != null)
				{
					_renderUnits_Sorted.Add(sortedBufferData._renderUnit);
				}
				else
				{
					//Debug.LogError("Sort 에러 : Null값이 발생했다.");
				}
			}
		}


		/// <summary>
		/// Meta 데이터를 정렬하는 함수.
		/// </summary>
		private int FuncMetaSort(DepthRequestMeta a, DepthRequestMeta b)
		{
			//도달 위치가 바깥쪽을 먼저 한다.
			
			//증가 방향 : Expected가 큰게 먼저 (내림차순)
			//감소 방향 : Expected가 작은게 먼저 (오름차순)
			//방향이 다른 경우 : 증가 방향 먼저
			//Expected가 같은 경우 : 해당 순서로 Original 비교
			if(a._deltaDepth > 0)
			{
				if(b._deltaDepth > 0)
				{
					//둘다 증가 방향인 경우 : 내림차순
					if(b._indexExpected == a._indexExpected)
					{
						return b._indexOriginal - a._indexOriginal;
					}
					return b._indexExpected - a._indexExpected;
				}
				else
				{
					//A 증가, B 감소인 경우 : A가 먼저
					return -1;
				}
			}
			else
			{
				if(b._deltaDepth > 0)
				{
					//A 감소, B 증가인 경우 : B가 먼저
					return 1;
				}
				else
				{
					//둘다 감소 방향인 경우 : 오름차순
					if(a._indexExpected == b._indexExpected)
					{
						return a._indexOriginal - b._indexOriginal;
					}
					return a._indexExpected - b._indexExpected;
				}
			}

			//return b._indexExpected - a._indexExpected;
			//return a._indexOriginal - b._indexOriginal;
		}

		private void DebugBuffers(BufferData[] buffers, string label, bool isImportant)
		{
			string strText = "[ " + label + " ]\n";
			BufferData curBuff = null;
			for (int i = buffers.Length - 1; i >= 0; i--)
			{
				curBuff = buffers[i];
				if(curBuff == null)
				{
					strText += "< Null > \n";
				}
				else
				{
					if(curBuff._renderUnit._level == 1) { strText += "- "; }
					else if(curBuff._renderUnit._level == 2) { strText += "-- "; }
					else if(curBuff._renderUnit._level == 3) { strText += "--- "; }
					else if(curBuff._renderUnit._level == 4) { strText += "---- "; }
					else { strText += "---- "; }

					strText += curBuff._renderUnit.Name + "  (" + curBuff._indexOriginal + " > " + curBuff._indexChanged + " | Group : " + curBuff._groupSize + ")\n";
				}
				
			}
			if(isImportant)
			{
				Debug.LogWarning(strText);
				
			}
			else
			{
				Debug.Log(strText);
			}
			
		}


		// Iteration
		//--------------------------------------------------------------------------
		

		// Get / Set
		//--------------------------------------------------
		public bool IsDepthChanged
		{
			get { return _isDepthChanged; }
		}

		/// <summary>
		/// RenderUnit 리스트를 가져온다. Depth가 바뀌었다면 다시 정렬된 리스트가 리턴된다.
		/// </summary>
		public List<apRenderUnit> SortedRenderUnits
		{
			get
			{
				if(_isDepthChanged)
				{
					return _renderUnits_Sorted;
				}
				else
				{
					return _renderUnits_Original;
				}
			}
		}

		public BufferData GetBufferData(apRenderUnit renderUnit)
		{
			for (int i = 0; i < _buffers.Length; i++)
			{
				if(_buffers[i]._renderUnit == renderUnit)
				{
					return _buffers[i];
				}
			}
			return null;
		}
	}
};