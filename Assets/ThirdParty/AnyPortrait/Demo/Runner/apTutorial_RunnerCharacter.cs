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

	public class apTutorial_RunnerCharacter : MonoBehaviour
	{

		// Members
		//---------------------------------------
		public apPortrait _portrait = null;


		public float _runVelocity = 10.0f;
		public float _runMaxVelocity = 10.0f;
		public float _maxVelocityCount = 10.0f;
		public float _jumpVelocity = 50.0f;
		public float _doubleJumpVelocity = 50.0f;
		public Vector2 _gravity = new Vector2(0, -10.0f);

		public Transform _colliderPosition;

		public Transform _hitBox_LB;
		public Transform _hitBox_RT;
		public Transform _hitBox_LB_Wide;
		public Transform _hitBox_RT_Wide;

		private Vector2 _pos = Vector2.zero;//<<계산 후 X 좌표는 리셋된다. (맵 이동에 사용됨)
		private Vector2 _velocity = Vector2.zero;

		private float _startPosY = 0.0f;
		private float _jumpPosY = 0.0f;

		private bool _isGround = false;
		private float _runSpeedCount = 0.0f;


		private enum MoveStatus
		{
			Idle,
			Run,
			Air,
			Stop
		}
		private MoveStatus _moveStatus = MoveStatus.Idle;
		private int _jumpCount = 0;



		// Initialize
		//---------------------------------------
		void Start()
		{
			_moveStatus = MoveStatus.Idle;
			_isGround = true;

			_pos = Vector2.zero;//<<계산 후 X 좌표는 리셋된다. (맵 이동에 사용됨)
			_velocity = Vector2.zero;
			_jumpCount = 0;
		}


		// Update
		//---------------------------------------
		void Update()
		{

		}

		public void AdaptPosition(bool isGround, float posWorldY)
		{
			_pos.x = 0;
			_pos.y = posWorldY - _startPosY;
			if (_isGround != isGround)
			{
				if (isGround)
				{
					//착지했다.
					if (_moveStatus == MoveStatus.Air)
					{
						_velocity.y = 0;
						Land();
					}
				}
				else
				{
					//착지하지 않은 상태 (낙하중)
					if (_moveStatus != MoveStatus.Air)
					{
						Fall();
					}
				}

				_isGround = isGround;
			}
			if (_isGround)
			{
				_velocity.y = 0;
			}
			transform.position = new Vector3(transform.position.x, posWorldY, transform.position.z);
		}
		public Vector2 UpdateVelocity(float deltaTime)
		{

			switch (_moveStatus)
			{
				case MoveStatus.Idle:
				case MoveStatus.Stop:
					_velocity.x = 0;
					break;

				case MoveStatus.Run:
				case MoveStatus.Air:
					_runSpeedCount += deltaTime;
					if (_runSpeedCount > _maxVelocityCount)
					{
						_runSpeedCount = _maxVelocityCount;
						_velocity.x = _runMaxVelocity;
					}
					else
					{
						float lerp = _runSpeedCount / _maxVelocityCount;
						_velocity.x = _runVelocity * (1.0f - lerp) + _runMaxVelocity * lerp;
					}

					break;
			}

			_velocity += _gravity * deltaTime;
			_pos += _velocity * deltaTime;

			return _pos;
		}


		public void SetStartPosY(float startPosY)
		{
			_startPosY = startPosY;
		}

		// Functions
		//---------------------------------------
		public void SetIdle(Vector3 readyWorldPos)
		{
			transform.position = readyWorldPos;
			_portrait.Play("Idle");

			_moveStatus = MoveStatus.Idle;

			_isGround = true;
			_jumpCount = 0;
		}

		public void ResetRunVelocity()
		{
			_runSpeedCount = 0.0f;
		}

		public void StartRun()
		{
			_portrait.CrossFade("Run");

			_moveStatus = MoveStatus.Run;

			_isGround = true;
			_jumpCount = 0;
		}

		public void Land()
		{
			if (transform.position.y < _jumpPosY - 5)
			{
				//높은데서 떨어졌을때
				_portrait.Play("Land");
				_portrait.CrossFadeQueued("Run", 0.1f);
			}
			else
			{
				_portrait.CrossFade("Run", 0.1f);
			}

			_moveStatus = MoveStatus.Run;

			_jumpPosY = transform.position.y;

			_isGround = true;
			_jumpCount = 0;
		}

		public void Jump()
		{
			if (_moveStatus != MoveStatus.Air)
			{
				_velocity.y = _jumpVelocity;

				//Debug.Log(_moveStatus + " > Jumpb");
				//_portrait.StopAll();
				_portrait.Play("Jump");
				_portrait.CrossFadeQueued("Air");

				_moveStatus = MoveStatus.Air;

				_isGround = false;

				_jumpPosY = transform.position.y;
				_jumpCount = 1;
			}
			else if (_jumpCount == 1)
			{
				_velocity.y = _doubleJumpVelocity;

				//_portrait.StopAll();
				_portrait.Play("DoubleJump");
				_portrait.CrossFadeQueued("Air");

				_moveStatus = MoveStatus.Air;

				_isGround = false;

				_jumpCount = 2;
			}
		}

		public void Fall()
		{
			if (_moveStatus != MoveStatus.Air)
			{
				//Debug.Log(_moveStatus + " > Fall");
				_portrait.CrossFade("Air");

				_moveStatus = MoveStatus.Air;

				_isGround = false;

				_jumpPosY = transform.position.y;

				_jumpCount = 1;
			}
		}

		public void Die()
		{
			//_portrait.StopAll();
			_portrait.Play("Die");
			_moveStatus = MoveStatus.Stop;

			_isGround = false;
			_jumpCount = 1;

		}

		//---------------------------------------------------------
		public bool IsHit(Vector3 worldPos)
		{
			if (_hitBox_LB_Wide.position.x < worldPos.x && worldPos.x < _hitBox_RT_Wide.position.x
					&& _hitBox_LB_Wide.position.y < worldPos.y && worldPos.y < _hitBox_RT_Wide.position.y)
			{
				return true;
			}
			return false;
		}

		public bool IsHit(Vector3 worldPosLB, Vector3 worldPosRT)
		{
			if (_hitBox_RT.position.x < worldPosLB.x ||
				worldPosRT.x < _hitBox_LB.position.x ||
				_hitBox_RT.position.y < worldPosLB.y ||
				worldPosRT.y < _hitBox_LB.position.y)
			{
				return false;
			}
			return true;
		}


	}
}