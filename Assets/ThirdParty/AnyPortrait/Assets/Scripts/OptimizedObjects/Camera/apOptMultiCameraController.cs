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
//using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// VR등의 이유로 카메라가 여러개 있는 경우에 한해서 생성되는 스크립트
	/// 이 스크립트는 Clipped optMesh (Child)에서 카메라와 연결하는 과정에서 실시간으로 생성된다.
	/// OnPreRender 이벤트를 optMesh로 보내주기 위함
	/// 
	/// </summary>
	public class apOptMultiCameraController : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		private Camera _camera = null;

		public delegate void FUNC_MESH_PRE_RENDERED(Camera camera);
		private Dictionary<apOptMesh, FUNC_MESH_PRE_RENDERED> _meshPreRenderedEvents = new Dictionary<apOptMesh, FUNC_MESH_PRE_RENDERED>();
		private int _nEvent = 0;
		
		private bool _isInit = false;
		private bool _isDestroyed = false;//삭제 중이라면..

		// Init
		//------------------------------------------------
		void Start()
		{
			if(!_isInit)
			{
				Init();
			}
		}

		public void Init()
		{
			if(_camera == null)
			{
				_camera = gameObject.GetComponent<Camera>();
			}

			_nEvent = 0;
			_isDestroyed = false;
			_isInit = true;
		}


		// Functions
		//------------------------------------------------
		/// <summary>
		/// OptMesh -> PreRender 이벤트 등록
		/// </summary>
		/// <param name="optMesh"></param>
		/// <param name="preRenderEvent"></param>
		public void AddPreRenderEvent(apOptMesh optMesh, FUNC_MESH_PRE_RENDERED preRenderEvent)
		{
			if(_meshPreRenderedEvents == null)
			{
				_meshPreRenderedEvents = new Dictionary<apOptMesh, FUNC_MESH_PRE_RENDERED>();
			}

			if(!_meshPreRenderedEvents.ContainsKey(optMesh))
			{
				_meshPreRenderedEvents.Add(optMesh, preRenderEvent);
			}
			else
			{
				_meshPreRenderedEvents[optMesh] = preRenderEvent;
			}

			//Debug.Log("PreRenderEvent Added [" + optMesh.name + " > " + name + "]");

			_nEvent = _meshPreRenderedEvents.Count;
		}

		/// <summary>
		/// OptMesh -> PreRender 이벤트 삭제
		/// </summary>
		/// <param name="optMesh"></param>
		public void RemovePreRenderEvent(apOptMesh optMesh)
		{
			if (_meshPreRenderedEvents == null)
			{
				return;
			}

			//Debug.LogWarning("RemovePreRenderEvent [" + optMesh.name + "]");
			if (_meshPreRenderedEvents.ContainsKey(optMesh))
			{
				_meshPreRenderedEvents.Remove(optMesh);
			}

			_nEvent = _meshPreRenderedEvents.Count;
			if(_nEvent == 0)
			{
				//Debug.LogError("[" + name + "] Event is 0");
				_isDestroyed = true;
				Destroy(this);
			}
		}

		//Pre Render Event
		private void OnPreRender()
		{
			if (_nEvent == 0)
			{
				return;
			}

			apOptMesh optMesh = null;
			FUNC_MESH_PRE_RENDERED funcMeshPreRendered = null;

			foreach (KeyValuePair<apOptMesh, FUNC_MESH_PRE_RENDERED> pair in _meshPreRenderedEvents)
			{
				optMesh = pair.Key;
				funcMeshPreRendered = pair.Value;

				if(optMesh == null || funcMeshPreRendered == null)
				{
					//메시가 없다면 리스트를 다시 봐야 한다.
					continue;
				}

				funcMeshPreRendered(_camera);
			}
		}


		// Events
		//------------------------------------------------
		private void OnDisable()
		{
			if(!_isDestroyed)
			{
				//Debug.LogError("[" + name + "] On Disable");
				_isDestroyed = true;
				Destroy(this);
			}
		}


		// Get / Set
		//-------------------------------------------------
		public Dictionary<apOptMesh, FUNC_MESH_PRE_RENDERED> GetPreRenderedEvents()
		{
			return _meshPreRenderedEvents;
		}

	}
}