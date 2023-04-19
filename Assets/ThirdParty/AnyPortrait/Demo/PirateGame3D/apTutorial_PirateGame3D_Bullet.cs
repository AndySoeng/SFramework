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

	public class apTutorial_PirateGame3D_Bullet : MonoBehaviour
	{
		// Members
		//---------------------------------------------------------
		public MeshRenderer bulletMesh;
		public ParticleSystem fireFx;
		public ParticleSystem hitFx;
		public ParticleSystem hitOnGroundFx;
		public apTutorial_PirateGame3D_Manager gameManager;

		public float startVelocity = 30.0f;

		private Vector3 _startPos;
		private Vector3 _endPos;
		private bool _isLive = false;
		private float _tLive = 0.0f;
		private float _tLiveLength = 0.0f;

		// Initialize
		//---------------------------------------------------------
		// Use this for initialization
		void Start()
		{
			bulletMesh.enabled = false;
			_isLive = false;
			fireFx.Clear();
			hitFx.Clear(true);
			hitOnGroundFx.Clear();
		}


		// Update
		//---------------------------------------------------------
		// Update is called once per frame
		void Update()
		{
			if (!_isLive)
			{
				return;
			}

			_tLive += Time.deltaTime;
			if (_tLive > _tLiveLength)
			{
				hitOnGroundFx.Play();
				Hide();
				return;
			}
			float lerp = _tLive / _tLiveLength;
			transform.position = _startPos * (1.0f - lerp) + _endPos * lerp;

			//TODO : 체크
			if (gameManager.HitTestToEnemy(transform.position))
			{
				hitFx.Play();
				Hide();

			}
		}

		//---------------------------------------------------------
		public void Shoot(Vector3 startPos, Vector3 direction)
		{
			transform.position = startPos;
			_startPos = startPos;
			fireFx.Play();
			if (direction.sqrMagnitude < 0.00001f)
			{
				direction = new Vector3(0.0f, 1.0f, 0.0f);
			}
			direction.Normalize();

			RaycastHit hitResult;
			bool isHit = Physics.Raycast(_startPos, direction, out hitResult, 100.0f);


			if (isHit && hitResult.collider != null)
			{
				_endPos = hitResult.point;
			}
			else
			{
				_endPos = _startPos + direction * 100.0f;
			}
			_endPos.z = _startPos.z;


			bulletMesh.enabled = true;
			bulletMesh.sortingOrder = 30;
			_tLive = 0.0f;
			_tLiveLength = (_endPos - _startPos).magnitude / startVelocity;

			_isLive = true;
		}

		public void Hide()
		{
			bulletMesh.enabled = false;
			_isLive = false;

			fireFx.Stop();
		}

		public bool IsLive()
		{
			return _isLive;
		}
	}
}