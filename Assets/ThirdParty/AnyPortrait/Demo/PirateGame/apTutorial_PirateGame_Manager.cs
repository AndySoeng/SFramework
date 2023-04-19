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
	/// <summary>
	/// A manager class that manages the state of the game
	/// </summary>
	public class apTutorial_PirateGame_Manager : MonoBehaviour
	{
		// Members
		//----------------------------------------------------------------------------------
		public apTutorial_PirateGame_Actor actor;
		public apTutorial_PirateGame_Bomb bomb;
		public apTutorial_PirateGame_Bullet bullet;
		public apTutorial_PirateGame_Enemy dragon;
		public apTutorial_PirateGame_Enemy slime;

		public Transform spawnRange_LT;
		public Transform spawnRange_RB;

		public Transform leftBound;
		public Transform rightBound;

		public Camera gameCamera;
		public Transform cameraLeftBound;
		public Transform cameraRightBound;

		public SpriteRenderer startUI;
		public SpriteRenderer howToPlayUI;
		public SpriteRenderer[] scoreNumber = new SpriteRenderer[6];
		public Sprite[] scoreSprites = new Sprite[10];
		public SpriteRenderer gameOverUI;
		public SpriteRenderer IKStatusUI;
		public Sprite IKSprite_ON;
		public Sprite IKSprite_OFF;

		public Transform startUIGroup;
		public Transform howToPlayUIGroup;
		public Transform scoreUIGroup;
		public Transform gameOverUIGroup;

		private List<apTutorial_PirateGame_Bomb> bombList = new List<apTutorial_PirateGame_Bomb>();
		private int lastBomeIndex = 0;

		private List<apTutorial_PirateGame_Bullet> bulletList = new List<apTutorial_PirateGame_Bullet>();
		private int lastBulletIndex = 0;

		private List<apTutorial_PirateGame_Enemy> dragonList = new List<apTutorial_PirateGame_Enemy>();
		private List<apTutorial_PirateGame_Enemy> slimeList = new List<apTutorial_PirateGame_Enemy>();

		private List<apTutorial_PirateGame_Enemy> liveDragonList = new List<apTutorial_PirateGame_Enemy>();
		private List<apTutorial_PirateGame_Enemy> liveSlimeList = new List<apTutorial_PirateGame_Enemy>();

		private float spawnCount = 0.0f;
		private int spawnOrder = 0;

		private enum GAME_STATE
		{
			Ready, Live, GameOver
		}
		private GAME_STATE _gameState = GAME_STATE.Ready;
		private bool _isFirstFrame = false;
		private int _score = 0;

		private float _tFx = 0.0f;
		private float _tDelay = 0.0f;
		private float _tFx2 = 0.0f;
		private bool _isAnyKeyDown = false;
		private bool _isIKCalculated = false;


		// Initialize
		//----------------------------------------------------------------------------------
		// Use this for initialization
		void Start()
		{
			bombList.Clear();
			for (int i = 0; i < 10; i++)
			{
				GameObject duplicatedBombGameObject = Instantiate<GameObject>(bomb.gameObject);
				apTutorial_PirateGame_Bomb duplicatedBomb = duplicatedBombGameObject.GetComponent<apTutorial_PirateGame_Bomb>();
				duplicatedBomb.Hide();
				bombList.Add(duplicatedBomb);
			}

			bulletList.Clear();
			for (int i = 0; i < 20; i++)
			{
				GameObject duplicatedBulletGameObject = Instantiate<GameObject>(bullet.gameObject);
				apTutorial_PirateGame_Bullet duplicatedBullet = duplicatedBulletGameObject.GetComponent<apTutorial_PirateGame_Bullet>();
				duplicatedBullet.Hide();
				bulletList.Add(duplicatedBullet);
			}

			dragonList.Clear();
			slimeList.Clear();
			for (int i = 0; i < 10; i++)
			{
				GameObject duplicatedDragonGameObject = Instantiate<GameObject>(dragon.gameObject);
				apTutorial_PirateGame_Enemy duplicatedDragon = duplicatedDragonGameObject.GetComponent<apTutorial_PirateGame_Enemy>();
				duplicatedDragon.Initialize();//<<
				dragonList.Add(duplicatedDragon);

				GameObject duplicatedSlimeGameObject = Instantiate<GameObject>(slime.gameObject);
				apTutorial_PirateGame_Enemy duplicatedSlime = duplicatedSlimeGameObject.GetComponent<apTutorial_PirateGame_Enemy>();
				duplicatedSlime.Initialize();//<<
				slimeList.Add(duplicatedSlime);
			}

			spawnCount = 0.0f;
			liveDragonList.Clear();
			liveSlimeList.Clear();

			_gameState = GAME_STATE.Ready;
			_isFirstFrame = true;

			Vector3 leftPos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
			Vector3 rightPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
			scoreUIGroup.position = new Vector3(leftPos.x, scoreUIGroup.position.y, scoreUIGroup.position.z);
			howToPlayUIGroup.position = new Vector3(rightPos.x, howToPlayUIGroup.position.y, howToPlayUIGroup.position.z);

		}

		// Update
		//----------------------------------------------------------------------------------
		// Update is called once per frame
		void Update()
		{
			switch (_gameState)
			{
				case GAME_STATE.Ready:
					{
						//Status before game start
						if (_isFirstFrame)
						{

							startUI.enabled = true;
							howToPlayUI.enabled = false;
							gameOverUI.enabled = false;
							SetScore(0);

							_tFx = 0.0f;
							_isAnyKeyDown = false;

							_isFirstFrame = false;

							IKStatusUI.enabled = false;
						}

						_tFx += Time.deltaTime;
						if (_tFx > 1.2f)
						{
							_tFx -= 1.2f;
						}
						//-1 ~ 1 => 0.9 ~ 1.1
						float scaleLerp = ((Mathf.Sin((_tFx / 1.2f) * Mathf.PI * 2.0f)) * 0.1f) + 1.0f;
						startUIGroup.localScale = Vector3.one * scaleLerp;

						if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
						{
							_isFirstFrame = true;
							_gameState = GAME_STATE.Live;
						}

						Cursor.visible = true;
					}
					break;

				case GAME_STATE.Live:
					{
						//State in which the game is running
						if (_isFirstFrame)
						{
							startUI.enabled = false;
							howToPlayUI.enabled = true;
							gameOverUI.enabled = false;
							howToPlayUI.color = new Color(1, 1, 1, 1);
							SetScore(0);

							_isFirstFrame = false;

							_tFx = 0.0f;
							_isAnyKeyDown = false;
							actor.Reset();

							IKStatusUI.enabled = true;
							_isIKCalculated = actor.IsIKCalculate;
							if (_isIKCalculated)
							{
								IKStatusUI.sprite = IKSprite_ON;
							}
							else
							{
								IKStatusUI.sprite = IKSprite_OFF;
							}
						}

						if (!_isAnyKeyDown)
						{
							if (Input.GetKey(KeyCode.A)
								|| Input.GetKey(KeyCode.S)
								|| Input.GetKey(KeyCode.LeftArrow)
								|| Input.GetKey(KeyCode.RightArrow)
								|| Input.GetMouseButtonDown(0)
								|| Input.GetMouseButtonDown(1))
							{
								_isAnyKeyDown = true;
							}
						}

						if (_isAnyKeyDown && howToPlayUI.enabled)
						{
							_tFx += Time.deltaTime;
							if (_tFx < 5.0f)
							{
								howToPlayUI.color = new Color(1, 1, 1, 1);
							}
							else if (_tFx < 8.0f)
							{

								float alpha = 1.0f - ((_tFx - 5.0f) / 3.0f);
								howToPlayUI.color = new Color(1, 1, 1, alpha);
							}
							else
							{
								howToPlayUI.enabled = false;
							}
						}

						if (Input.GetKeyDown(KeyCode.I))
						{
							actor.SetIKCalculateToggle();
						}

						spawnCount += Time.deltaTime;
						if (spawnCount > 3.0f)
						{
							spawnCount = 0.0f;
							SpawnEnemies();
						}

						Vector3 nextCamPos = actor.transform.position;
						nextCamPos.z = gameCamera.transform.position.z;
						nextCamPos.y = gameCamera.transform.position.y;
						nextCamPos.x = Mathf.Clamp(nextCamPos.x, cameraLeftBound.position.x, cameraRightBound.position.x);

						gameCamera.transform.position = gameCamera.transform.position * 0.9f + nextCamPos * 0.1f;

						if (_isIKCalculated != actor.IsIKCalculate)
						{
							_isIKCalculated = actor.IsIKCalculate;
							if (_isIKCalculated)
							{
								IKStatusUI.sprite = IKSprite_ON;
							}
							else
							{
								IKStatusUI.sprite = IKSprite_OFF;
							}
						}

						Cursor.visible = false;
					}
					break;

				case GAME_STATE.GameOver:
					{
						//Game has ended
						if (_isFirstFrame)
						{
							startUI.enabled = true;
							gameOverUI.enabled = true;
							howToPlayUI.enabled = false;

							gameOverUI.color = new Color(1, 1, 1, 0);
							startUI.color = new Color(1, 1, 1, 0);

							IKStatusUI.enabled = false;

							_tFx = 0.0f;
							_tFx2 = 0.0f;
							_tDelay = 0.0f;
							_isFirstFrame = false;
						}


						_tFx2 += Time.deltaTime;
						_tDelay += Time.deltaTime;
						if (_tFx2 < 0.8f)
						{
							float alphaLerp = 1.0f - Mathf.Pow(1.0f - _tFx2 / 0.8f, 2f);
							gameOverUI.color = new Color(1, 1, 1, alphaLerp);
							gameOverUIGroup.transform.localScale = Vector3.one * (1.2f * (1.0f - alphaLerp) + 1.0f * alphaLerp);
						}
						else
						{
							gameOverUI.color = new Color(1, 1, 1, 1);
							gameOverUIGroup.transform.localScale = Vector3.one;
							_tFx2 = 1.0f;
						}
						if (_tDelay < 2.5f)
						{
							_tFx = 0.0f;
							startUI.color = new Color(1, 1, 1, 0);
						}
						else
						{
							_tDelay = 10.0f;
							_tFx += Time.deltaTime;
							if (_tFx > 1.2f)
							{
								_tFx -= 1.2f;
							}

							//-1 ~ 1 => 0.9 ~ 1.1
							float scaleLerp = ((Mathf.Sin((_tFx / 1.2f) * Mathf.PI * 2.0f)) * 0.1f) + 1.0f;
							startUI.color = new Color(1, 1, 1, 1);
							startUIGroup.localScale = Vector3.one * scaleLerp;

							if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
							{
								_isFirstFrame = true;
								_gameState = GAME_STATE.Live;
							}
						}

						Cursor.visible = true;
					}
					break;
			}

		}

		//---------------------------------------------
		//Spanw the enemies.
		private void SpawnEnemies()
		{
			if (liveDragonList.Count < 5)
			{
				for (int i = 0; i < dragonList.Count; i++)
				{
					if (!dragonList[i].IsInitialized() || dragonList[i].IsLive() || liveDragonList.Contains(dragonList[i]))
					{
						continue;
					}

					Vector3 spawnPos = new Vector3(
						UnityEngine.Random.Range(spawnRange_LT.position.x, spawnRange_RB.position.x),
						UnityEngine.Random.Range(spawnRange_LT.position.y, spawnRange_RB.position.y),
						0.0f);
					dragonList[i].Spawn(spawnPos, spawnOrder++);
					liveDragonList.Add(dragonList[i]);
					break;
				}
			}


			if (liveSlimeList.Count < 5)
			{
				for (int i = 0; i < slimeList.Count; i++)
				{
					if (!slimeList[i].IsInitialized() || slimeList[i].IsLive() || liveSlimeList.Contains(slimeList[i]))
					{
						continue;
					}

					Vector3 spawnPos = new Vector3(
						UnityEngine.Random.Range(spawnRange_LT.position.x, spawnRange_RB.position.x),
						UnityEngine.Random.Range(spawnRange_LT.position.y, spawnRange_RB.position.y),
						0.0f);
					slimeList[i].Spawn(spawnPos, spawnOrder++);
					liveSlimeList.Add(slimeList[i]);
					break;
				}
			}

			if (spawnOrder > 10)
			{
				spawnOrder = 0;
			}


		}
		//---------------------------------------------
		//When a player shoots, a bullet is created.
		public void Shoot(Vector3 position, Vector3 aimPosition)
		{
			Vector3 direction = aimPosition - position;
			//shootFx.transform.position = position;
			//shootFx.Play();
			for (int i = 0; i < bulletList.Count; i++)
			{
				int bulletIndex = (lastBulletIndex + i) % bulletList.Count;
				if (bulletList[bulletIndex].IsLive())
				{
					continue;
				}

				bulletList[bulletIndex].Shoot(position, direction);
				lastBulletIndex = bulletIndex;
				return;
			}
		}

		//The robot fires the bomb.
		public void FireBomb(Vector3 position, Vector3 aimPosition)
		{
			Vector3 direction = aimPosition - position;
			for (int i = 0; i < bombList.Count; i++)
			{
				int bombIndex = (lastBomeIndex + i) % bombList.Count;
				if (bombList[bombIndex].IsLive())
				{
					continue;
				}

				bombList[bombIndex].Fire(position, direction);
				lastBomeIndex = bombIndex;
				return;
			}
		}

		//------------------------------------------------------------------------------------------------
		//Check if the enemies are hit.
		public bool HitTestToEnemy(Vector3 position)
		{
			int addScore = 0;
			bool isAnyHitEnemy = false;
			for (int i = 0; i < liveSlimeList.Count; i++)
			{
				if (liveSlimeList[i].HitTestByPoint(position))
				{
					isAnyHitEnemy = true;
					addScore++;
				}
			}

			for (int i = 0; i < liveDragonList.Count; i++)
			{
				if (liveDragonList[i].HitTestByPoint(position))
				{
					isAnyHitEnemy = true;
					addScore++;
				}
			}

			SetScore(_score + addScore);
			return isAnyHitEnemy;
		}

		//Check if enemies are hit by bombs.
		public bool HitTestBombToEnemy(Vector3 position, float radius)
		{
			int addScore = 0;
			bool isAnyHitEnemy = false;
			for (int i = 0; i < liveSlimeList.Count; i++)
			{
				if (liveSlimeList[i].HitTestByBox(position, radius))
				{
					isAnyHitEnemy = true;
					addScore++;
				}
			}

			for (int i = 0; i < liveDragonList.Count; i++)
			{
				if (liveDragonList[i].HitTestByBox(position, radius))
				{
					isAnyHitEnemy = true;
					addScore++;
				}
			}

			SetScore(_score + addScore);
			return isAnyHitEnemy;
		}

		//The enemy was killed with damage.
		public void OnDiedEnemy(apTutorial_PirateGame_Enemy enemy)
		{
			if (enemy.enemyType == apTutorial_PirateGame_Enemy.ENEMY_TYPE.Dragon)
			{
				liveDragonList.Remove(enemy);
			}
			else if (enemy.enemyType == apTutorial_PirateGame_Enemy.ENEMY_TYPE.Slime)
			{
				liveSlimeList.Remove(enemy);
			}
		}

		//Check if the attack of the enemy damages the player.
		public void HitTestToActor(Vector3 position)
		{
			bool isHitTest = actor.HitTestByPoint(position);
			if (isHitTest)
			{
				_isFirstFrame = true;
				_gameState = GAME_STATE.GameOver;
			}
		}

		//Whether the game is running
		public bool IsLive()
		{
			return _gameState == GAME_STATE.Live;
		}
		//-------------------------------------------------------------
		private void SetScore(int score)
		{
			_score = score;
			if (_score > 999999)
			{
				_score = 999999;
			}
			scoreNumber[0].sprite = scoreSprites[_score % 10];
			scoreNumber[1].sprite = scoreSprites[(_score / 10) % 10];
			scoreNumber[2].sprite = scoreSprites[(_score / 100) % 10];
			scoreNumber[3].sprite = scoreSprites[(_score / 1000) % 10];
			scoreNumber[4].sprite = scoreSprites[(_score / 10000) % 10];
			scoreNumber[5].sprite = scoreSprites[(_score / 100000) % 10];
		}
	}
}