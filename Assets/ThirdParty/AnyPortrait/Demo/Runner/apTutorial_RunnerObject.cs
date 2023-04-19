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

using AnyPortrait;

namespace AnyPortrait
{

	public class apTutorial_RunnerObject : MonoBehaviour
	{
		// Members
		//--------------------------------------------
		public MeshRenderer _renderer;
		public ParticleSystem _particle;
		public apTutorial_RunnerGame.ObjectType _objectType;

		public delegate void OnFxEnded(apTutorial_RunnerObject targetObject);
		private OnFxEnded _eventOnFxEnded = null;
		private float _fxCount = 0.0f;

		private bool _isVisible = true;

		public Transform _colliderLB;
		public Transform _colliderRT;


		// Initialize
		//--------------------------------------------
		void Start()
		{
			if (_objectType == apTutorial_RunnerGame.ObjectType.Coin ||
				_objectType == apTutorial_RunnerGame.ObjectType.Obstacle)
			{
				this.enabled = false;
			}

			Hide();
		}

		void Update()
		{
			if (!_isVisible)
			{
				return;
			}
			_fxCount -= Time.deltaTime;
			if (_fxCount < 0.0f)
			{
				if (_eventOnFxEnded != null)
				{
					_eventOnFxEnded(this);
				}

				_fxCount = 1.0f;
				_eventOnFxEnded = null;
				Hide();
			}
		}

		public void Show()
		{
			if (_renderer == null)
			{
				return;
			}
			_renderer.enabled = true;
			_isVisible = true;
		}

		public void Hide()
		{
			if (_renderer != null)
			{
				_renderer.enabled = false;
			}
			if (_particle != null)
			{
				_particle.Clear();
			}

			_isVisible = false;
		}

		public void Emit(OnFxEnded onFxEnded)
		{
			if (_particle == null)
			{
				return;
			}
			_particle.Play();

			_eventOnFxEnded = onFxEnded;

			_fxCount = 3.0f;
			_isVisible = true;
		}



		public void SetWorldPos(Vector3 posWorld)
		{
			transform.position = posWorld;
		}

		public void SetWorldPos(float posX, float posY)
		{
			transform.position = new Vector3(posX, posY, transform.position.z);
		}

		public void SetLocalPos(Vector3 posLocal)
		{
			transform.localPosition = posLocal;
		}
	}
}