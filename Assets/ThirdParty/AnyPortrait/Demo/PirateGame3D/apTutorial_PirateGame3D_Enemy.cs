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

	public class apTutorial_PirateGame3D_Enemy : MonoBehaviour
	{
		// Members
		//---------------------------------------------
		public enum ENEMY_TYPE
		{
			Slime,
			Dragon,
		}

		private const int STATUS_HIDE = 0;
		private const int STATUS_IDLE = 1;
		private const int STATUS_MOVE = 2;
		private const int STATUS_ATTACK = 3;
		private const int STATUS_DAMAGED = 4;

		public apPortrait portrait;
		public Animator animator;
		public ENEMY_TYPE enemyType;
		public apTutorial_PirateGame3D_Manager gameManager;

		public ParticleSystem spawnFx;
		public ParticleSystem diedFx;
		public ParticleSystem noticeFx;
		public ParticleSystem attackFx;
		public Transform footPos;
		public Transform hitBox_LT;
		public Transform hitBox_RB;
		public Transform attackPos;

		private bool _isInitialized = false;
		private int _status = STATUS_HIDE;
		private bool _isFirstFrame = false;
		private float _tLive = 0.0f;
		private float _tLiveLength = 0.0f;
		private float _tAnim = 0.0f;
		private Vector3 _nextMovePos;
		private Vector3 _prevMovePos;

		// Initialize
		//---------------------------------------------
		// Use this for initialization
		void Start()
		{
			_isInitialized = true;
			//portrait.Hide();//<<
		}

		public void Initialize()
		{
			_isInitialized = false;
			portrait.AsyncInitialize(OnAsyncLinkCompleted);
		}

		public void OnAsyncLinkCompleted(apPortrait portrait)
		{
			_isInitialized = true;
			Hide();//<<
		}

		// Update
		//---------------------------------------------
		// Update is called once per frame
		void Update()
		{
			if (_status == STATUS_HIDE)
			{
				return;
			}

			animator.SetInteger("Status", _status);
			if (enemyType == ENEMY_TYPE.Dragon)
			{
				UpdateDragon();
			}
			else
			{
				UpdateSlime();
			}
		}

		public void Hide()
		{
			_isFirstFrame = true;
			_status = STATUS_HIDE;
			portrait.Hide();
		}

		public bool IsInitialized()
		{
			return _isInitialized;
		}

		public bool IsLive()
		{
			return _status != STATUS_HIDE && _status != STATUS_DAMAGED;
		}

		public void Spawn(Vector3 position, int spawnOrder)
		{
			if (enemyType == ENEMY_TYPE.Slime)
			{
				//땅으로 나와서 스폰해야한다.
				RaycastHit hitResult;
				bool isHit = Physics.Raycast(position + new Vector3(0, 10, 0), new Vector3(0, -1, 0), out hitResult, 100);
				if (isHit && hitResult.collider != null)
				{
					position.y = hitResult.point.y - footPos.localPosition.y;
				}
			}

			transform.position = position;
			transform.localScale = new Vector3(1, 1, 1);
			portrait.SetSortingOrder(6 + spawnOrder);

			portrait.Show();
			//portrait.Play("Idle");


			if (UnityEngine.Random.Range(0, 10) < 5)
			{
				LookLeft();
			}
			else
			{
				LookRight();
			}

			_isFirstFrame = true;
			_status = STATUS_IDLE;
			animator.SetInteger("Status", _status);
			spawnFx.Play();
		}

		//------------------------------------------------------------------------------
		private void UpdateDragon()
		{
			switch (_status)
			{
				case STATUS_IDLE:
					{
						if (_isFirstFrame)
						{
							//portrait.Play("Idle");

							_tLive = 0.0f;
							_tAnim = 0.0f;
							_tLiveLength = UnityEngine.Random.Range(3.0f, 4.0f);

							_prevMovePos = transform.position;
							_nextMovePos = transform.position;
							if (IsLookLeft())
							{ _nextMovePos.x -= 3; }
							else
							{ _nextMovePos.x += 3; }

							if (_nextMovePos.y < gameManager.actor.transform.position.y + 8)
							{
								_nextMovePos.y = gameManager.actor.transform.position.y + UnityEngine.Random.Range(10.0f, 15.0f);
							}

							_isFirstFrame = false;
						}
						_tLive += Time.deltaTime;
						_tAnim += Time.deltaTime;
						if (_tLive > _tLiveLength)
						{
							_isFirstFrame = true;
							if ((gameManager.actor.transform.position - attackPos.position).magnitude < 5 && gameManager.actor.IsLive())
							{
								_status = STATUS_ATTACK;
							}
							else
							{
								_status = STATUS_MOVE;
							}
						}
						else
						{
							float lerp = _tLive / _tLiveLength;
							Vector3 curPos = _prevMovePos * (1.0f - lerp) + _nextMovePos * lerp;

							curPos.y += Mathf.Sin((_tAnim / 1.5f) * Mathf.PI * 2.0f) * 0.5f;
							transform.position = curPos;
						}
					}

					break;

				case STATUS_MOVE:
					{
						if (_isFirstFrame)
						{
							_tLive = 0.0f;
							_tAnim = 0.0f;
							//_tLiveLength = UnityEngine.Random.Range(2.0f, 3.0f);

							_prevMovePos = transform.position;
							_nextMovePos = gameManager.actor.transform.position;
							if (gameManager.actor.IsLive())
							{
								if (_prevMovePos.x < _nextMovePos.x)
								{
									_nextMovePos.x -= 3;
								}
								else
								{
									_nextMovePos.x += 3;
								}
								_nextMovePos.y += 2;
							}
							else
							{
								_nextMovePos.x = Mathf.Clamp(gameManager.actor.transform.position.x - UnityEngine.Random.Range(-20, 20), gameManager.leftBound.position.x, gameManager.rightBound.position.x);
								_nextMovePos.y += UnityEngine.Random.Range(0, 20);
							}

							if ((_nextMovePos - _prevMovePos).magnitude < 1)
							{
								_tLiveLength = 1.0f;
							}
							else
							{
								_tLiveLength = (_nextMovePos - _prevMovePos).magnitude / 15.0f;
							}

							if (_nextMovePos.x < _prevMovePos.x)
							{
								LookLeft();
							}
							else
							{
								LookRight();
							}



							_isFirstFrame = false;
						}

						_tLive += Time.deltaTime;
						if (_tLive > _tLiveLength)
						{
							_isFirstFrame = true;
							if ((gameManager.actor.transform.position - attackPos.position).magnitude < 5 && gameManager.actor.IsLive())
							{
								_status = STATUS_ATTACK;
							}
							else
							{
								_status = STATUS_IDLE;
							}

						}
						else
						{
							float lerp = _tLive / _tLiveLength;
							Vector3 curPos = _prevMovePos * (1.0f - lerp) + _nextMovePos * lerp;

							//curPos.y += Mathf.Sin((_tAnim / 1.5f) * Mathf.PI * 2.0f) * 0.5f;
							transform.position = curPos;
						}
					}
					break;

				case STATUS_ATTACK:
					if (_isFirstFrame)
					{
						_tLive = 0.0f;
						_tAnim = 0.0f;

						if (gameManager.actor.transform.position.x < transform.position.x)
						{
							LookLeft();
						}
						else
						{
							LookRight();
						}
						noticeFx.Play();

						_isFirstFrame = false;
					}
					break;

				case STATUS_DAMAGED:
					{
						if (_isFirstFrame)
						{
							//portrait.Play("Damaged");
							_tLive = 0.0f;
							_tLiveLength = 3.0f;

							_isFirstFrame = false;
						}
						_tLive += Time.deltaTime;
						if (_tLive > _tLiveLength)
						{
							gameManager.OnDiedEnemy(this);
							Hide();
						}
					}
					break;
			}
		}

		private void UpdateSlime()
		{
			switch (_status)
			{
				case STATUS_IDLE:
					{
						if (_isFirstFrame)
						{
							_tLive = 0.0f;
							_tAnim = 0.0f;
							_tLiveLength = UnityEngine.Random.Range(2.0f, 4.0f);

							_isFirstFrame = false;
						}
						_tLive += Time.deltaTime;
						if (_tLive > _tLiveLength)
						{
							_isFirstFrame = true;
							if (Mathf.Abs(gameManager.actor.transform.position.x - attackPos.position.x) < 5 && gameManager.actor.IsLive())
							{
								_status = STATUS_ATTACK;
							}
							else
							{
								_status = STATUS_MOVE;
							}

						}
					}
					break;

				case STATUS_MOVE:
					{
						if (_isFirstFrame)
						{
							_tLive = 0.0f;
							_tAnim = 0.0f;
							//_tLiveLength = UnityEngine.Random.Range(2.0f, 3.0f);

							_prevMovePos = transform.position;
							_nextMovePos = gameManager.actor.transform.position;
							if (gameManager.actor.IsLive())
							{
								if (_prevMovePos.x < _nextMovePos.x)
								{
									_nextMovePos.x -= 3;
								}
								else
								{
									_nextMovePos.x += 3;
								}
							}
							else
							{
								_nextMovePos.x = Mathf.Clamp(gameManager.actor.transform.position.x - UnityEngine.Random.Range(-20, 20), gameManager.leftBound.position.x, gameManager.rightBound.position.x);
							}


							if ((_nextMovePos - _prevMovePos).magnitude < 1)
							{
								_tLiveLength = 1.0f;
							}
							else
							{
								_tLiveLength = (_nextMovePos - _prevMovePos).magnitude / 10.0f;
								if (_tLiveLength > 3.0f)
								{
									_tLiveLength = 3.0f;
									_nextMovePos = (_nextMovePos - _prevMovePos).normalized * 10 * 3.0f + _prevMovePos;
								}
							}

							if (_nextMovePos.x < _prevMovePos.x)
							{
								LookLeft();
							}
							else
							{
								LookRight();
							}

							_isFirstFrame = false;
						}

						_tLive += Time.deltaTime;
						if (_tLive > _tLiveLength)
						{
							_isFirstFrame = true;
							if (Mathf.Abs(gameManager.actor.transform.position.x - attackPos.position.x) < 5 && gameManager.actor.IsLive())
							{
								_status = STATUS_ATTACK;
							}
							else
							{
								_status = STATUS_IDLE;
							}

						}
						else
						{
							float lerp = _tLive / _tLiveLength;
							Vector3 curPos = _prevMovePos * (1.0f - lerp) + _nextMovePos * lerp;

							RaycastHit hitResult;
							bool isHit = Physics.Raycast(curPos + new Vector3(0.0f, 20.0f, 0.0f), new Vector3(0, -1, 0), out hitResult, 50);
							if (isHit && hitResult.collider != null)
							{
								curPos.y = hitResult.point.y - footPos.localPosition.y;
							}
							transform.position = curPos;
						}
					}
					break;

				case STATUS_ATTACK:
					if (_isFirstFrame)
					{
						_tLive = 0.0f;
						_tAnim = 0.0f;

						if (gameManager.actor.transform.position.x < transform.position.x)
						{
							LookLeft();
						}
						else
						{
							LookRight();
						}
						noticeFx.Play();

						_isFirstFrame = false;
					}
					break;

				case STATUS_DAMAGED:
					{
						if (_isFirstFrame)
						{
							_tLive = 0.0f;
							_tLiveLength = 3.0f;

							_isFirstFrame = false;
						}
						_tLive += Time.deltaTime;
						if (_tLive > _tLiveLength)
						{
							gameManager.OnDiedEnemy(this);
							Hide();
						}
					}
					break;
			}
		}

		private void LookLeft()
		{
			if (transform.localScale.x > 0.0f)
			{
				transform.localScale = new Vector3(-1, 1, 1);
			}
		}

		private void LookRight()
		{
			if (transform.localScale.x < 0.0f)
			{
				transform.localScale = new Vector3(1, 1, 1);
			}
		}


		private bool IsLookLeft()
		{
			return transform.localScale.x < 0.0f;
		}

		//-------------------------------------------------------------
		public bool HitTestByPoint(Vector3 position)
		{
			if (!IsLive())
			{
				return false;
			}

			if (position.x > Mathf.Min(hitBox_LT.position.x, hitBox_RB.position.x)
				&& position.x < Mathf.Max(hitBox_LT.position.x, hitBox_RB.position.x)
				&& position.y > Mathf.Min(hitBox_RB.position.y, hitBox_LT.position.y)
				&& position.y < Mathf.Max(hitBox_RB.position.y, hitBox_LT.position.y))
			{
				if (position.x < transform.position.x)
				{
					LookLeft();
				}
				else
				{
					LookRight();
				}
				_isFirstFrame = true;
				_status = STATUS_DAMAGED;
				diedFx.Play();
				return true;
			}
			return false;
		}


		public bool HitTestByBox(Vector3 position, float radius)
		{
			if (!IsLive())
			{
				return false;
			}

			if (Mathf.Max(hitBox_LT.position.x, hitBox_RB.position.x) < position.x - radius
				|| position.x + radius < Mathf.Min(hitBox_LT.position.x, hitBox_RB.position.x)
				|| Mathf.Max(hitBox_LT.position.y, hitBox_RB.position.y) < position.y - radius
				|| position.y + radius < Mathf.Min(hitBox_LT.position.y, hitBox_RB.position.y)
				)
			{
				return false;
			}

			if (position.x < transform.position.x)
			{
				LookLeft();
			}
			else
			{
				LookRight();
			}

			_isFirstFrame = true;
			_status = STATUS_DAMAGED;
			diedFx.Play();
			return true;
		}
		//----------------------------------------------------
		void OnAttack()
		{
			if (IsLive())
			{
				attackFx.Play();
				gameManager.HitTestToActor(attackPos.position);
			}
		}

		void OnAttackEnded()
		{
			if (IsLive())
			{
				_isFirstFrame = true;
				_status = STATUS_IDLE;
			}

		}
	}
}