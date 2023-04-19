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
using UnityEngine.Events;
using System;

using AnyPortrait;


namespace AnyPortrait
{
	//애니메이션 이벤트와 UnityEvent를 묶는 래퍼 클래스
	//이건 Bake하면 Portrait에 저장된다. (단 애니메이션 이벤트가 같다면 그대로 유지해야한다.)
	[Serializable]
	public class apUnityEventWrapper
	{
		// Members
		//------------------------------------------------------
		//유니티 이벤트 값만 저장한다.
		[SerializeField, NonBackupField]
		public List<apUnityEvent> _unityEvents = null;

		//Link후 AnimEvent > UnityEvent 값을 받자
		[NonSerialized]
		private Dictionary<apAnimEvent, apUnityEvent> _animEvent2UnityEvent = null;


		// Init
		//------------------------------------------------------
		public apUnityEventWrapper()
		{

		}


		

		// Bake (Editor)
		//------------------------------------------------------
		/// <summary>
		/// Bake 또는 모드 변경시 이 함수를 호출하자.
		/// 단, 애니메이션 이벤트가 완성된 상태여야 한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void Bake(apPortrait portrait)
		{
			//모든 애니메이션 
			if(portrait == null)
			{
				return;
			}

			if(portrait._animEventCallMode == apPortrait.ANIM_EVENT_CALL_MODE.SendMessage)
			{
				//SendMessage면 Bake를 하지 않는다.
				//(삭제하지는 않는다.)
				return;
			}

			//1. 모든 애니메이션 이벤트를 찾자.
			int nAnimClips = portrait._animClips != null ? portrait._animClips.Count : 0;
			if(nAnimClips == 0)
			{
				//애니메이션 클립이 없다.
				_unityEvents = null;
				return;
			}



			//기존의 이벤트를 남겨야 하므로
			//새로 리스트를 만들자.
			List<apUnityEvent> newUnityEvents = new List<apUnityEvent>();
			int nPrevUnityEvent = _unityEvents != null ? _unityEvents.Count : 0;

			//변환 정보를 모두 기록하자 (ID를 저장하기 위해)
			Dictionary<apAnimEvent, apUnityEvent> animEvent2UnityEvent = new Dictionary<apAnimEvent, apUnityEvent>();

			apAnimClip curAnimClip = null;
			apAnimEvent curAnimEvent = null;

			apUnityEvent findUnityEvent = null;

			for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
			{
				curAnimClip = portrait._animClips[iAnimClip];
				int nAnimEventOfClip = curAnimClip._animEvents != null ? curAnimClip._animEvents.Count : 0;
				if(nAnimEventOfClip == 0)
				{
					continue;
				}

				for (int iEvent = 0; iEvent < nAnimEventOfClip; iEvent++)
				{
					curAnimEvent = curAnimClip._animEvents[iEvent];
					curAnimEvent._cachedUnityEventID = -1;//일단 캐시된 ID를 초기화

					findUnityEvent = null;

					//1. 이미 생성된 이벤트(New) 중에 중복되는게 있는지 확인
					if(newUnityEvents.Count > 0)
					{
						findUnityEvent = newUnityEvents.Find(delegate(apUnityEvent a)
						{
							return a.IsWrappableEvent(curAnimEvent);
						});
					}

					//이미 추가되어 있다면 패스
					if(findUnityEvent != null)
					{
						animEvent2UnityEvent.Add(curAnimEvent, findUnityEvent);//변환 정보만 저장
						continue;
					}

					//2. 기존의 이벤트 중에 연결될만한게 있는지 확인 (유지)
					if(findUnityEvent == null && nPrevUnityEvent > 0)
					{
						findUnityEvent = _unityEvents.Find(delegate(apUnityEvent a)
						{
							return a.IsWrappableEvent(curAnimEvent);
						});
					}

					
					if(findUnityEvent != null)
					{
						//기존 이벤트 데이터를 유지할 수 있다면
						
						newUnityEvents.Add(findUnityEvent);//New 리스트로 옮기자
						animEvent2UnityEvent.Add(curAnimEvent, findUnityEvent);
					}
					else
					{
						//새로운 이벤트 데이터를 만들자
						apUnityEvent newUnityEvent = new apUnityEvent();
						newUnityEvent.SetSrcEventData_Bake(curAnimEvent);

						newUnityEvents.Add(newUnityEvent);//New 리스트에 추가
						animEvent2UnityEvent.Add(curAnimEvent, newUnityEvent);
					}
					
				}
			}

			//newUnityEvents를 기존 리스트로 옮기자
			if(newUnityEvents.Count > 0)
			{
				if(_unityEvents == null)
				{
					_unityEvents = new List<apUnityEvent>();
				}
				_unityEvents.Clear();

				apUnityEvent curUnityEvent = null;
				int curID = 0;
				for (int iEvent = 0; iEvent < newUnityEvents.Count; iEvent++)
				{
					curUnityEvent = newUnityEvents[iEvent];
					
					curUnityEvent.SetUniqueID(curID);//ID 할당
					curID += 1;

					//멤버 리스트에 넣는다.
					_unityEvents.Add(curUnityEvent);
				}

				//모든 애니메이션 이벤트의 멤버에 빠른 접근을 위한 ID를 저장하자
				//이 ID가 저장되면 Link시에 빠르게 접근할 수 있다.
				foreach (KeyValuePair<apAnimEvent, apUnityEvent> eventPair in animEvent2UnityEvent)
				{
					eventPair.Key._cachedUnityEventID = eventPair.Value._uniqueID;
				}
			}
			else
			{
				//없덩..
				_unityEvents = null;
			}
		}
		
		
		// Link (Runtime)
		//------------------------------------------------------
		public void Link(apPortrait portrait)
		{
			if(portrait._animEventCallMode == apPortrait.ANIM_EVENT_CALL_MODE.SendMessage)
			{
				return;
			}

			if(_animEvent2UnityEvent == null)
			{
				_animEvent2UnityEvent = new Dictionary<apAnimEvent, apUnityEvent>();
			}
			_animEvent2UnityEvent.Clear();



			//모든 애니메이션 이벤트를 돌면서 연결을 하자
			int nUnityEvents = _unityEvents != null ? _unityEvents.Count : 0;
			int nAnimClips = portrait._animClips != null ? portrait._animClips.Count : 0;

			if(nAnimClips == 0 || nUnityEvents == 0)
			{
				//애니메이션 클립이나 저장된 유니티 이벤트가 없다.
				return;
			}

			apUnityEvent curUnityEvent = null;

			//ID를 바탕으로 빠른 접근을 위한 맵을 만들자
			Dictionary<int, apUnityEvent> id2UnityEvents = new Dictionary<int, apUnityEvent>();
			for (int i = 0; i < nUnityEvents; i++)
			{
				curUnityEvent = _unityEvents[i];
				curUnityEvent.Validate();//유효성 검사를 해야 Invoke가 가능하다
				id2UnityEvents.Add(curUnityEvent._uniqueID, curUnityEvent);
			}

			//애니메이션 이벤트를 돌자
			apAnimClip curAnimClip = null;
			apAnimEvent curAnimEvent = null;
			apUnityEvent linkableUnityEvent = null;
			for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
			{
				curAnimClip = portrait._animClips[iAnimClip];
				int nAnimEventOfClip = curAnimClip._animEvents != null ? curAnimClip._animEvents.Count : 0;
				if (nAnimEventOfClip == 0)
				{
					continue;
				}

				for (int iEvent = 0; iEvent < nAnimEventOfClip; iEvent++)
				{
					curAnimEvent = curAnimClip._animEvents[iEvent];
					curAnimEvent._linkedUnityEvent = null;

					linkableUnityEvent = null;

					//1. 캐시된 ID를 바탕으로 연결할 수 있는 걸 찾자
					if(curAnimEvent._cachedUnityEventID >= 0)
					{
						if(id2UnityEvents.ContainsKey(curAnimEvent._cachedUnityEventID))
						{
							linkableUnityEvent = id2UnityEvents[curAnimEvent._cachedUnityEventID];
						}
						
					}

					if(linkableUnityEvent == null)
					{
						//Debug.Log("전체 검색 : " + curAnimEvent._cachedUnityEventID);
						//빠른 검색이 실패라면
						//전체 리스트에서 찾아야 한다.
						linkableUnityEvent = _unityEvents.Find(delegate(apUnityEvent a)
						{
							return a.IsWrappableEvent(curAnimEvent);
						});
					}

					//연결하자
					curAnimEvent._linkedUnityEvent = linkableUnityEvent;

					if(linkableUnityEvent == null)
					{
						//단 에러 문구는 연결을 하고자 했을때만 나오도록 하자(22.7.2)
						if (curAnimEvent._cachedUnityEventID >= 0)
						{
							Debug.LogError("AnyPortrait : Failed to connect Callback of Animation Event [" + curAnimEvent._eventName + "]");
						}
						//else
						//{
						//	Debug.Log("유니티 이벤트가 설정되지 않았다.");
						//}
					}
				}
			}
		}

	}

	

}