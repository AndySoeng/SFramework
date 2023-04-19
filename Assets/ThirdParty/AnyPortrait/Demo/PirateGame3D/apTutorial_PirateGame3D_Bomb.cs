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
	public class apTutorial_PirateGame3D_Bomb : MonoBehaviour
	{
		// Members
		//--------------------------------------------------------
		public MeshRenderer bombMesh;
		public ParticleSystem fireFx;
		public ParticleSystem hitFx;
		public ParticleSystem tailFx;
		public apTutorial_PirateGame3D_Manager gameManager;

		public float radius = 3.0f;
		public float gravity = -10.0f;
		public float startVelocity = 30.0f;
		private Vector3 _velocity;
		private bool _isLive = false;
		private float _rotationVelocity = 30;

		// Initialize
		//--------------------------------------------------------
		// Use this for initialization
		void Start()
		{
			bombMesh.enabled = false;
			_isLive = false;
			hitFx.Clear(true);
			fireFx.Clear(true);
			tailFx.Clear(true);
		}

		// Update
		//--------------------------------------------------------
		// Update is called once per frame
		void Update()
		{
			if (!_isLive)
			{
				return;
			}

			_velocity += new Vector3(0.0f, gravity * Time.deltaTime, 0.0f);
			transform.position = transform.position + _velocity * Time.deltaTime;
			transform.Rotate(new Vector3(0.0f, 0.0f, _rotationVelocity * Time.deltaTime));

			//Check terrain
			if (gameManager.HitTestBombToEnemy(transform.position, radius))
			{
				gameManager.HitTestBombToEnemy(transform.position, radius * 2.0f);
				hitFx.Play();
				Hide();
				return;
			}

			RaycastHit hitResult;
			bool isHit = Physics.Raycast(transform.position + new Vector3(0.0f, 30.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), out hitResult, 40.0f);
			if (isHit && hitResult.collider != null && hitResult.point.y > transform.position.y)
			{
				gameManager.HitTestBombToEnemy(transform.position, radius * 2.0f);
				hitFx.Play();
				Hide();
				return;
			}

			if (transform.position.y < -100)
			{
				Hide();
			}
		}

		//--------------------------------------------------------
		//The bomb is launched and the update begins.
		public void Fire(Vector3 startPos, Vector3 direction)
		{
			startPos.z = 0;
			direction.z = 0;
			if(direction.sqrMagnitude <= 0.0f)
			{
				direction = new Vector3(0, 1, 0);
			}
			transform.position = startPos;
			if (direction.sqrMagnitude > 0.0f)
			{
				_velocity = direction.normalized * startVelocity;
			}
			else
			{
				_velocity = Vector3.zero;
			}
			bombMesh.enabled = true;
			bombMesh.sortingOrder = 20;

			_rotationVelocity = UnityEngine.Random.Range(-100, -400);

			_isLive = true;

			fireFx.Play();
			tailFx.Play();
		}

		public void Hide()
		{
			bombMesh.enabled = false;
			_isLive = false;

			fireFx.Stop();
			tailFx.Stop();
		}

		public bool IsLive()
		{
			return _isLive;
		}
	}
}