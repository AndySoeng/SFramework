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
#if UNITY_EDITOR
using UnityEditor;
#endif
using AnyPortrait;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

namespace AnyPortrait
{
	[ExecuteInEditMode]
#if UNITY_2017_1_OR_NEWER
	[AddComponentMenu("AnyPortrait/Timeline Simulator")]
#endif
	public class apAnimPlayTimelineSimulator : MonoBehaviour
	{
		public bool _simulate = true;
#if UNITY_2017_1_OR_NEWER
		public PlayableDirector _director;
#endif

#if UNITY_2017_1_OR_NEWER
		private apPortrait _portrait;
		
		private double _playTime = -1.0f;

		private bool _isPlayed = false;
		private float _relinkCount = 0.0f;
#endif
		private const float RELINK_COUNT = 10.0f;//Play상태가 아닐 때, 타임 슬라이더 드래그하여 움직이는 경우, 10초마다 Relink를 강제로 한다.

		// Start is called before the first frame update
		void Start()
		{
#if UNITY_EDITOR
			if(Application.isPlaying)
			{
				//게임이 실행되면 이건 필요없다.
				this.enabled =  false;
				Destroy(this);
			}
			else
			{
				this.enabled = true;
			}
			
#else
			// 게임 중이라면 이건 필요없다.
			Destroy(this);
#endif
			
		}

		// Update is called once per frame
#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER
		void Update()
		{
			if(!_simulate)
			{
				return;
			}
			if(Application.isPlaying)
			{
				//게임이 실행되면 이건 필요없다.
				this.enabled =  false;
				Destroy(this);
				return;
			}

			if(_director == null)
			{
				return;
			}

			if(!_director.isActiveAndEnabled)
			{
				return;
			}

			if(_portrait == null)
			{
				_portrait = this.gameObject.GetComponent<apPortrait>();
			}

			if(_portrait == null)
			{
				return;
			}
			
			if(!_portrait._isUsingMecanim)
			{
				return;
			}

			bool isTrackUsed = _portrait._timelineTrackSets != null && _portrait._timelineTrackSets.Length > 0;
			if(!isTrackUsed)
			{
				return;
			}

			

			//(float)_playTime, (float)_director.time);
			
			//Debug.Log("[Director : " + _director.state + "]");
			//Debug.Log("[Director : " + _director.state + " / " +_director.time +"]");

			if(_director.state == PlayState.Playing)
			{
				if(!_isPlayed)
				{
					//Stopped > Played : 초기화
					_portrait.InitializeAsSimulating();
				}
				_portrait.UpdateForceAsSimulating(Time.deltaTime);

				_relinkCount = 0.0f;
			}
			else
			{
				_relinkCount += Time.deltaTime;
				
				if(Mathf.Abs((float)_playTime - (float)_director.time) > 0.001f)
				{
					if(_relinkCount > RELINK_COUNT)
					{
						//RELINK_COUNT마다 초기화
						_relinkCount = 0.0f;
						_portrait.InitializeAsSimulating();
					}

					_portrait.UpdateForceAsSimulating(Time.deltaTime);
				}
			}

			_playTime = _director.time;
			_isPlayed = _director.state == PlayState.Playing;
		}

		public void Relink()
		{
			if(_director != null && _simulate && !Application.isPlaying)
			{
				if (_portrait == null)
				{
					_portrait = this.gameObject.GetComponent<apPortrait>();
				}

				if (_portrait == null)
				{
					return;
				}

				_portrait.InitializeAsSimulating();
				_portrait.UpdateForceAsSimulating(Time.deltaTime);
			}
			
		}
#endif


#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER
			
		[CustomEditor(typeof(apAnimPlayTimelineSimulator))]
		public class SimulatorInspector : Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				apAnimPlayTimelineSimulator simulator = target as apAnimPlayTimelineSimulator;

				if(GUILayout.Button("Refresh Tracks", GUILayout.Height(24)))
				{
					if(simulator != null)
					{
						simulator.Relink();
					}
				}

			}
		}
#endif
	}
}