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
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apTutorial_RunnerGame : MonoBehaviour
	{
		// Members
		//--------------------------------------------------

		// Character
		public Transform _characterBasePos = null;
		public apTutorial_RunnerCharacter _character = null;

		public Transform _characterDeadZone = null;

		// UI Groups
		public Transform _scoreUIGroup = null;
		public SpriteRenderer[] _scoreNumbers = new SpriteRenderer[7];
		public Sprite[] _scoreNumberSprite = new Sprite[10];

		//public SpriteRenderer _gameOverUI = null;
		public apPortrait _gameOver = null;
		public SpriteRenderer _guidePressToStart = null;
		public SpriteRenderer _guidePressToJump = null;

		//Map Group
		//맵 타일이 생성되는 시작점과 끝 점 + 각 타일의 생성 범위

		public Transform _tileGenPos_Left = null;
		public Transform _tileGenPos_Right = null;
		public Transform _tileGenPos_Top = null;
		public Transform _tileGenPos_Bottom = null;
		public Transform _tileGenPos_Base = null;

		public float _tileSize = 5.0f;
		public float _coinPosSize = 2;

		public Camera _gameCamera = null;
		public Camera _uiCamera = null;

		public Transform _camFocus = null;
		public Transform _camFocus_UpperLimit = null;
		public Transform _camFocus_LowerLimit = null;

		public Transform _camPos_LowerLimit = null;

		public Transform _camPos_Sight;

		//public AudioSource[] _audioSources;



		public Transform _mapTilePoolGroup;
		public apTutorial_RunnerMapTile _mapTileSrc_Ground;
		public apTutorial_RunnerMapTile _mapTileSrc_Soil;
		public apTutorial_RunnerMapTile _mapTileSrc_Upper;
		public apTutorial_RunnerMapTile _mapTileSrc_UpperLeft;
		public apTutorial_RunnerMapTile _mapTileSrc_UpperRight;

		public Transform _objectPoolGroup;
		public apTutorial_RunnerObject _objectSrc_Coin;
		public apTutorial_RunnerObject _objectSrc_Obstacle;
		public apTutorial_RunnerObject _objectSrc_CoinFx;

		public Transform _fxPos;

		//TODO
		//1. 화면 스테이트
		//2. 캐릭터 제어
		//3. 캐릭터 위치 및 이동 (좌표계만)
		//4. 캐릭터 이동에 따른 맵 높이 정보 생성 (동적)
		//5. 코인, 맵타일, 장애물 풀
		//6. 맵 높이 정보에 맞게 타일 생성
		//7. 

		private enum PlayState
		{
			Ready,
			Play,
			GameOver
		}
		private PlayState _playState = PlayState.Ready;
		private PlayState _nextState = PlayState.Ready;
		private bool _isFirstFrame = false;
		private bool _isChangeFrame = false;


		private int _score = 0;
		private const int MAX_SCORE = 9999999;

		private float _tState = 0.0f;
		private float _tStateDelay = 0.0f;
		private bool _isBeforeJump = false;

		//타일을 배열로 만들고 계속 밀자
		private int _tileSize_Left = 0;
		private int _tileSize_Right = 0;
		private int _tileSize_Top = 0;
		private int _tileSize_Bottom = 0;

		private int _tileSize_Width = 0;
		private int _tileSize_Height = 0;

		private int _coinSize_Width = 0;
		private int _coinSize_Height = 0;

		private Vector2 _tilePosLB = Vector2.zero;
		//private Vector2 _tilePosRT = Vector2.zero;
		private float _tilePosOffsetX = 0;
		private float _coinPosOffsetX = 0;

		private byte[,] _curTileMapTypes = null;
		private apTutorial_RunnerMapTile[,] _curTileMapRefs = null;
		private const byte TILE_EMPTY = 0;
		private const byte TILE_GROUND = 1;
		private const byte TILE_SOIL = 2;
		private const byte TILE_UPPER = 3;

		private byte[,] _curObstacleTypes = null;
		private apTutorial_RunnerObject[,] _curObstacleRefs = null;

		private byte[,] _curCoinTypes = null;
		private apTutorial_RunnerObject[,] _curCoinRefs = null;

		private const byte OBJ_EMPTY = 0;
		private const byte OBJ_COIN = 1;
		private const byte OBJ_OBSTACLE_LOWER = 1;
		private const byte OBJ_OBSTACLE_UPPER = 2;


		private const float GROUND_HEIGHT = 0.5f;

		private float _chaMapPosIndexX_Float = 0.0f;
		private float _chaMapPosIndexY_Float = 0.0f;

		private int _chaMapPosIndexX = 0;
		private int _chaMapPosIndexY = 0;

		private float _predictSightPosY = 0;

		//private int _audioIndex = 0;
		//private float _audioDelay = 0.0f;

		public enum MapTileType
		{
			Ground,
			Soil,
			Upper,
			UpperLeft,
			UpperRight
		}

		public enum ObjectType
		{
			Coin,
			Obstacle,
			CoinFx,
		}

		private Dictionary<MapTileType, TilePool> _tilePools = new Dictionary<MapTileType, TilePool>();
		private Dictionary<ObjectType, ObjectPool> _objectPools = new Dictionary<ObjectType, ObjectPool>();
		private bool _isFirstLoad = false;

		private bool _isCoinGenerating = false;
		private int _coinNextGenCount = 5;
		private int _coinGenLength = 10;
		private int _coinGenLine = 1;

		// Initialize
		//--------------------------------------------------
		void Start()
		{
			_playState = PlayState.Ready;
			_isFirstFrame = true;

			_nextState = PlayState.Ready;
			_isChangeFrame = false;
			_isFirstLoad = true;
		}

		// Update
		//--------------------------------------------------
		void Update()
		{
			float deltaTime = Time.deltaTime;
			switch (_playState)
			{
				case PlayState.Ready:
					UpdateReady(deltaTime);
					break;
				case PlayState.Play:
					UpdatePlay(deltaTime);
					break;
				case PlayState.GameOver:
					UpdateGameOver(deltaTime);
					break;
			}

			if (_isChangeFrame)
			{
				_isFirstFrame = true;
				_playState = _nextState;
				_isChangeFrame = false;
			}
			else if (_isFirstFrame)
			{
				_isFirstFrame = false;
			}
		}


		private void UpdateReady(float deltaTime)
		{
			if (_isFirstFrame)
			{
				float leftScreenPos = _uiCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
				float rightScreenPos = _uiCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
				_scoreUIGroup.position = new Vector3(leftScreenPos, _scoreUIGroup.position.y, _scoreUIGroup.position.z);

				//타일 생성 기준점 체크
				_tileGenPos_Left.position = new Vector3(leftScreenPos - _tileSize * 2, _tileGenPos_Left.position.y, _tileGenPos_Left.position.z);
				_tileGenPos_Right.position = new Vector3(rightScreenPos + _tileSize * 2, _tileGenPos_Right.position.y, _tileGenPos_Right.position.z);


				_character.transform.position = _characterBasePos.transform.position;


				if (_isFirstLoad)
				{
					_isFirstLoad = false;

					_tilePools.Clear();

					_tilePools.Add(MapTileType.Ground, new TilePool(MapTileType.Ground, 50, _mapTilePoolGroup));
					_tilePools.Add(MapTileType.Soil, new TilePool(MapTileType.Soil, 200, _mapTilePoolGroup));
					_tilePools.Add(MapTileType.Upper, new TilePool(MapTileType.Upper, 50, _mapTilePoolGroup));
					_tilePools.Add(MapTileType.UpperLeft, new TilePool(MapTileType.UpperLeft, 50, _mapTilePoolGroup));
					_tilePools.Add(MapTileType.UpperRight, new TilePool(MapTileType.UpperRight, 50, _mapTilePoolGroup));

					_tilePools[MapTileType.Ground].MakePool(_mapTileSrc_Ground);
					_tilePools[MapTileType.Soil].MakePool(_mapTileSrc_Soil);
					_tilePools[MapTileType.Upper].MakePool(_mapTileSrc_Upper);
					_tilePools[MapTileType.UpperLeft].MakePool(_mapTileSrc_UpperLeft);
					_tilePools[MapTileType.UpperRight].MakePool(_mapTileSrc_UpperRight);

					_objectPools.Clear();
					_objectPools.Add(ObjectType.Coin, new ObjectPool(ObjectType.Coin, 100, _objectPoolGroup));
					_objectPools.Add(ObjectType.Obstacle, new ObjectPool(ObjectType.Coin, 30, _objectPoolGroup));
					_objectPools.Add(ObjectType.CoinFx, new ObjectPool(ObjectType.Coin, 30, _objectPoolGroup));

					_objectPools[ObjectType.Coin].MakePool(_objectSrc_Coin);
					_objectPools[ObjectType.Obstacle].MakePool(_objectSrc_Obstacle);
					_objectPools[ObjectType.CoinFx].MakePool(_objectSrc_CoinFx);
				}

				foreach (KeyValuePair<MapTileType, TilePool> tilePair in _tilePools)
				{
					tilePair.Value.PushAll();
				}
				foreach (KeyValuePair<ObjectType, ObjectPool> objPair in _objectPools)
				{
					objPair.Value.PushAll();
				}

				InitMapTiles();

				SetScore(0);

				_gameOver.Hide();
				_guidePressToJump.enabled = false;
				_guidePressToStart.enabled = true;

				_character.SetIdle(_characterBasePos.position);
				_character.SetStartPosY(_characterBasePos.position.y);
				_character.AdaptPosition(true, _characterBasePos.position.y);
				_character.ResetRunVelocity();

				_tState = 0.0f;
				_tStateDelay = 0.0f;

				UpdateCamera(true, 0.0f);
			}
			_tState += deltaTime;
			_tStateDelay += deltaTime;
			if (_tState > 0.7f)
			{
				_guidePressToStart.enabled = !_guidePressToStart.enabled;
				_tState -= 0.7f;
			}

			if (_tStateDelay > 0.5f)
			{
				if (Input.anyKeyDown || Input.touchCount > 0 || Input.GetMouseButtonDown(0))
				{
					ChangePlayState(PlayState.Play);
				}

				_tStateDelay = 0.5f;
			}

		}


		private void UpdatePlay(float deltaTime)
		{
			if (_isFirstFrame)
			{
				_gameOver.Hide();
				_guidePressToJump.enabled = true;
				_guidePressToStart.enabled = false;

				_tState = 0.0f;
				_isBeforeJump = true;
				//_audioDelay = 0.2f;

				_character.StartRun();
			}

			if (_isBeforeJump)
			{
				_tState += deltaTime;
				if (_tState > 0.7f)
				{
					_guidePressToJump.enabled = !_guidePressToJump.enabled;
					_tState -= 0.7f;
				}
			}

			if (Input.anyKeyDown || Input.touchCount > 0 || Input.GetMouseButtonDown(0))
			{
				//TODO : 점프가 가능한지 체크
				//점프!
				_isBeforeJump = false;
				_guidePressToJump.enabled = false;

				_character.Jump();
			}

			//캐릭터 업데이트를 하자
			Vector2 nextPos = _character.UpdateVelocity(deltaTime);

			//X 이동이 가능한가.
			float expectedNextPosX = _character._colliderPosition.position.x + nextPos.x;
			//int expectedNextPosIndexX = GetTileIndexX(expectedNextPosX);

			//다음 타일을 체크
			float expectedGroundHeight = GetGroundHeightWorldPos(expectedNextPosX);
			//bool isXCollided = false;
			if (expectedGroundHeight > _character.transform.position.y)
			{
				//앞으로 갈 수가 없다.
				//float tileLeftPos = GetTileLeftWorldPos(expectedNextPosX);
				nextPos.x = 0;

				//isXCollided = true;
			}

			//타일을 이동시키자
			_tilePosOffsetX += nextPos.x / _tileSize;
			_coinPosOffsetX += nextPos.x / _coinPosSize;

			bool isTileIndexChanged = false;
			while (_tilePosOffsetX > 1.0f)
			{
				//타일 인덱스를 하나씩 전진시킨다.
				_tilePosOffsetX -= 1.0f;

				IncreseMapTileX();
				isTileIndexChanged = true;
			}


			bool isCoinIndexChanged = false;
			while (_coinPosOffsetX > 1.0f)
			{
				_coinPosOffsetX -= 1.0f;

				IncreseCoinX();
				isCoinIndexChanged = true;
			}

			RefreshMapTileRefs(isTileIndexChanged);
			RefreshCoinRefs(isCoinIndexChanged);

			//현재 위치 파악
			bool isGround = false;

			float curGroundY = GetGroundHeightWorldPos(_character.transform.position.x);
			float characterWorldPosY = nextPos.y + _characterBasePos.position.y;
			if (characterWorldPosY < curGroundY)
			{
				isGround = true;
			}

			if (isGround)
			{
				characterWorldPosY = curGroundY;
			}

			_character.AdaptPosition(isGround, characterWorldPosY);

			UpdateCamera(false, deltaTime);

			// 코인을 먹자
			for (int iX = 0; iX < _coinSize_Width; iX++)
			{
				for (int iY = 0; iY < _coinSize_Height; iY++)
				{
					if (_curCoinRefs[iX, iY] != null)
					{
						if (_character.IsHit(_curCoinRefs[iX, iY].transform.position))
						{
							Vector3 hitPos = _curCoinRefs[iX, iY].transform.position;
							hitPos.z = _fxPos.position.z;
							EmitCoinFx(hitPos);
							SetScore(_score + 5);

							_objectPools[ObjectType.Coin].Push(_curCoinRefs[iX, iY]);
							_curCoinRefs[iX, iY] = null;
							_curCoinTypes[iX, iY] = OBJ_EMPTY;
						}
					}
				}
			}


			//선인장에 부딪히면 죽는다.
			apTutorial_RunnerObject curObstacle = null;
			for (int iX = 0; iX < _tileSize_Width; iX++)
			{
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					if (_curObstacleRefs[iX, iY] != null)
					{
						curObstacle = _curObstacleRefs[iX, iY];
						if (_character.IsHit(curObstacle._colliderLB.position, curObstacle._colliderRT.position))
						{
							_character.Die();
							ChangePlayState(PlayState.GameOver);
						}
					}
				}
			}

			// 떨어지면 죽는다.
			if (_character.transform.position.y < _characterDeadZone.position.y)
			{
				ChangePlayState(PlayState.GameOver);
			}


			//코인 사운드 리셋
			//if(_audioDelay > 0.0f)
			//{
			//	_audioDelay -= deltaTime;
			//}

		}

		private void UpdateGameOver(float deltaTime)
		{
			if (_isFirstFrame)
			{
				_gameOver.Show();
				_gameOver.Play("Show");
				_guidePressToJump.enabled = false;
				_guidePressToStart.enabled = false;

				_tState = -1.0f;
				_tStateDelay = 0.0f;
			}

			_tState += deltaTime;
			_tStateDelay += deltaTime;
			if (_tState > 0.7f)
			{
				_guidePressToStart.enabled = !_guidePressToStart.enabled;
				_tState -= 0.7f;
			}

			//캐릭터 업데이트를 하자
			Vector2 nextPos = _character.UpdateVelocity(deltaTime);

			//현재 위치 파악
			bool isGround = false;

			float curGroundY = GetGroundHeightWorldPos(_character.transform.position.x);
			float characterWorldPosY = nextPos.y + _characterBasePos.position.y;
			if (characterWorldPosY < curGroundY)
			{
				isGround = true;
			}

			if (isGround)
			{
				characterWorldPosY = curGroundY;
			}

			_character.AdaptPosition(isGround, characterWorldPosY);

			if (_tStateDelay > 0.7f)
			{
				_tStateDelay = 1.0f;
				if (Input.anyKeyDown || Input.touchCount > 0 || Input.GetMouseButtonDown(0))
				{
					ChangePlayState(PlayState.Ready);
				}
			}

		}


		private void ChangePlayState(PlayState nextPlayState)
		{
			_isChangeFrame = true;
			_nextState = nextPlayState;
		}

		// Functions
		//--------------------------------------------------

		private void SetScore(int score)
		{
			_score = score;
			if (_score < 0)
			{
				_score = 0;
			}
			else if (_score > MAX_SCORE)
			{
				_score = MAX_SCORE;
			}

			RefreshScoreUI();
		}

		private void RefreshScoreUI()
		{
			_scoreNumbers[6].sprite = _scoreNumberSprite[_score % 10];
			_scoreNumbers[5].sprite = _scoreNumberSprite[(_score / 10) % 10];
			_scoreNumbers[4].sprite = _scoreNumberSprite[(_score / 100) % 10];
			_scoreNumbers[3].sprite = _scoreNumberSprite[(_score / 1000) % 10];
			_scoreNumbers[2].sprite = _scoreNumberSprite[(_score / 10000) % 10];
			_scoreNumbers[1].sprite = _scoreNumberSprite[(_score / 100000) % 10];
			_scoreNumbers[0].sprite = _scoreNumberSprite[(_score / 1000000) % 10];
		}

		private void EmitCoinFx(Vector3 worldPos)
		{
			apTutorial_RunnerObject coinFx = _objectPools[ObjectType.CoinFx].Pop();
			if (coinFx != null)
			{
				coinFx.SetWorldPos(worldPos);
				coinFx.Emit(OnFxEnded);
			}

			//if (_audioDelay <= 0.0f)
			//{
			//	_audioSources[_audioIndex].Play();
			//	_audioIndex++;
			//	if(_audioIndex >= _audioSources.Length)
			//	{
			//		_audioIndex = 0;
			//	}
			//	_audioDelay = 0.05f;
			//}
		}
		private void OnFxEnded(apTutorial_RunnerObject targetFx)
		{
			_objectPools[targetFx._objectType].Push(targetFx);
		}

		// Make Tile
		//--------------------------------------------------
		private void InitMapTiles()
		{
			_tileSize_Left = Mathf.Abs((int)((_tileGenPos_Base.position.x - _tileGenPos_Left.position.x) / _tileSize)) + 2;
			_tileSize_Right = Mathf.Abs((int)((_tileGenPos_Base.position.x - _tileGenPos_Right.position.x) / _tileSize)) + 3;//<<타일은 미리 만든다.
			_tileSize_Top = Mathf.Abs((int)((_tileGenPos_Base.position.y - _tileGenPos_Top.position.y) / _tileSize)) + 1;
			_tileSize_Bottom = Mathf.Abs((int)((_tileGenPos_Base.position.y - _tileGenPos_Bottom.position.y) / _tileSize)) + 2;

			_tileSize_Width = _tileSize_Left + _tileSize_Right;
			_tileSize_Height = _tileSize_Top + _tileSize_Bottom;

			int coinSize_Left = Mathf.Abs((int)((_tileGenPos_Base.position.x - _tileGenPos_Left.position.x) / _coinPosSize)) + 2;
			int coinSize_Right = Mathf.Abs((int)((_tileGenPos_Base.position.x - _tileGenPos_Right.position.x) / _coinPosSize)) + 1;
			int coinSize_Top = Mathf.Abs((int)((_tileGenPos_Base.position.y - _tileGenPos_Top.position.y) / _coinPosSize)) + 3;
			int coinSize_Bottom = Mathf.Abs((int)((_tileGenPos_Base.position.y - _tileGenPos_Bottom.position.y) / _coinPosSize)) + 2;

			_coinSize_Width = coinSize_Left + coinSize_Right;
			_coinSize_Height = coinSize_Top + coinSize_Bottom;

			//타일맵 생성 후 클리어
			_curTileMapTypes = new byte[_tileSize_Width, _tileSize_Height];
			_curTileMapRefs = new apTutorial_RunnerMapTile[_tileSize_Width, _tileSize_Height];

			_curCoinTypes = new byte[_coinSize_Width, _coinSize_Height];
			_curCoinRefs = new apTutorial_RunnerObject[_coinSize_Width, _coinSize_Height];

			_curObstacleTypes = new byte[_tileSize_Width, _tileSize_Height];
			_curObstacleRefs = new apTutorial_RunnerObject[_tileSize_Width, _tileSize_Height];


			for (int iX = 0; iX < _tileSize_Width; iX++)
			{
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					_curTileMapTypes[iX, iY] = TILE_EMPTY;
					_curTileMapRefs[iX, iY] = null;

					_curObstacleTypes[iX, iY] = OBJ_EMPTY;
					_curObstacleRefs[iX, iY] = null;
				}
			}

			for (int iX = 0; iX < _coinSize_Width; iX++)
			{
				for (int iY = 0; iY < _coinSize_Height; iY++)
				{
					_curCoinTypes[iX, iY] = OBJ_EMPTY;
					_curCoinRefs[iX, iY] = null;
				}
			}

			_tilePosLB = new Vector2(_tileGenPos_Base.position.x - _tileSize_Left * _tileSize,
										_tileGenPos_Base.position.y - _tileSize_Bottom * _tileSize);

			//타일맵 위치
			//LB가 0, 0
			//RT가 W, H
			//이미지의 좌표계도 LB를 중점으로 둔다.

			float chaMapPosX = _character.transform.position.x - _tilePosLB.x;
			float chaMapPosY = _character.transform.position.y - _tilePosLB.y;

			//기준 위치를 잡자
			_chaMapPosIndexX_Float = chaMapPosX / _tileSize;
			_chaMapPosIndexY_Float = chaMapPosY / _tileSize;

			//이 값은 바뀌지 않는다.
			_chaMapPosIndexX = (int)_chaMapPosIndexX_Float;
			_chaMapPosIndexY = (int)_chaMapPosIndexY_Float;

			//기준 맵을 생성하자
			for (int iX = 0; iX < _tileSize_Width; iX++)
			{
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					if (iY == _chaMapPosIndexY)
					{
						_curTileMapTypes[iX, iY] = TILE_UPPER;
					}
					else if (iY == _chaMapPosIndexY - 1)
					{
						_curTileMapTypes[iX, iY] = TILE_GROUND;
					}
					else if (iY < _chaMapPosIndexY - 1)
					{
						_curTileMapTypes[iX, iY] = TILE_SOIL;
					}

				}
			}

			float chaCoinPosIndexX_Float = chaMapPosX / _coinPosSize;

			_tilePosOffsetX = _chaMapPosIndexX_Float - _chaMapPosIndexX;
			_coinPosOffsetX = chaCoinPosIndexX_Float - (int)chaCoinPosIndexX_Float;

			Vector3 chaStartPos = _characterBasePos.position;
			chaStartPos.y = TileY2RealPosY(_chaMapPosIndexY) + GROUND_HEIGHT;

			//TODO . 코인도 만들어야 한다.
			_characterBasePos.position = chaStartPos;

			//DebugMapTiles();

			RefreshMapTileRefs(true);
		}

		private void IncreseMapTileX()
		{
			//X = 0일때는 Push하고 초기화
			for (int iY = 0; iY < _tileSize_Height; iY++)
			{
				if (_curTileMapRefs[0, iY] != null)
				{
					_tilePools[_curTileMapRefs[0, iY]._tileType].Push(_curTileMapRefs[0, iY]);
					_curTileMapRefs[0, iY] = null;
				}
				if (_curObstacleRefs[0, iY] != null)
				{
					_curObstacleRefs[0, iY].Hide();
					_objectPools[ObjectType.Obstacle].Push(_curObstacleRefs[0, iY]);
				}
				_curTileMapTypes[0, iY] = TILE_EMPTY;
				_curObstacleTypes[0, iY] = OBJ_EMPTY;
			}

			//X + 1의 값을 가져온다.
			for (int iX = 0; iX < _tileSize_Width - 1; iX++)
			{
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					_curTileMapTypes[iX, iY] = _curTileMapTypes[iX + 1, iY];
					_curTileMapRefs[iX, iY] = _curTileMapRefs[iX + 1, iY];

					_curObstacleTypes[iX, iY] = _curObstacleTypes[iX + 1, iY];
					_curObstacleRefs[iX, iY] = _curObstacleRefs[iX + 1, iY];
				}
			}

			//마지막 인덱스에서 위치를 새로 만든다.
			//일단 X-1, X-2, X-3의 위치를 비교한다.
			int lastX = _tileSize_Width - 1;
			int heightIndex_X1 = GetHeightIndexY(lastX - 1);
			int heightIndex_X2 = GetHeightIndexY(lastX - 2);
			int heightIndex_X3 = GetHeightIndexY(lastX - 3);

			int nextHeightIndex = -1;

			//1. X1이 -1이라면 (낭떠러지)
			//	1-1. X2도 -1이라면 => X3의 H (80%), X3 H-1 (10%), X3 H-2 (10%) (높이 반전 없음)
			//	1-2. X2가 값을 가진다면 => -1 (20%), 동일 높이 (40%), H+-1 (30%), H-2 (10%)
			//		> 단, 높이가 증감하는 경우, 경계에 도달하면 대체 높이로 처리해야한다.

			//2. X1의 높이가 존재한다면
			//	2-1. X2와 높이가 다르다면 100% 같은 높이로 유지한다.
			//	2-2. X2와 높이가 같다면
			//		2-2-1. X3와 높이가 같다면 "계속 이어지는 평지" => H0 (60%), H+-1 (20%), H+-2 (10%), -1 (10%)
			//		2-2-2. X3와 높이가 다르다면 "계단" => H0 (80%), H+-1 (10%), H+-2 (5%), -1 (5%)
			//경계에선 반전한다.
			float rand = UnityEngine.Random.Range(0.0f, 100.0f);
			bool isRandPlus = UnityEngine.Random.Range(0.0f, 100.0f) < 50;

			if (heightIndex_X1 < 0)
			{
				//	1-1. X2도 -1이라면 => X3의 H (80%), X3 H-1 (10%), X3 H-2 (10%) (높이 반전 없음)
				//	1-2. X2가 값을 가진다면 => -1 (20%), 동일 높이 (40%), H+-1 (30%), H-2 (10%)
				if (heightIndex_X2 < 0)
				{
					if (rand < 80.0f)
					{
						nextHeightIndex = heightIndex_X3;
					}
					else if (rand < 80.0f + 10.0f)
					{
						nextHeightIndex = heightIndex_X3 - 1;
					}
					else
					{
						nextHeightIndex = heightIndex_X3 - 2;
					}
					if (nextHeightIndex < 2)
					{
						nextHeightIndex = 2;
					}
				}
				else
				{
					if (rand < 20.0f)
					{
						nextHeightIndex = -1;
					}
					else if (rand < 20 + 40)
					{
						nextHeightIndex = heightIndex_X2;
					}
					else if (rand < 20 + 40 + 30)
					{
						if (isRandPlus)
						{
							nextHeightIndex = heightIndex_X2 + 1;
							if (nextHeightIndex > _tileSize_Height - 1)
							{
								//반전
								nextHeightIndex = heightIndex_X2 - 1;
							}
						}
						else
						{
							nextHeightIndex = heightIndex_X2 - 1;
							if (nextHeightIndex < 2)
							{
								//반전
								nextHeightIndex = heightIndex_X2 + 1;
							}
						}
					}
					else
					{
						nextHeightIndex = heightIndex_X2 - 2;
						if (nextHeightIndex < 2)
						{
							//반전
							nextHeightIndex = heightIndex_X2 + 1;
						}

					}
				}
			}
			else
			{
				//	2-1. X2와 높이가 다르다면 100% 같은 높이로 유지한다.
				//	2-2. X2와 높이가 같다면
				//		2-2-1. X3와 높이가 같다면 "계속 이어지는 평지" => H0 (60%), H+-1 (20%), H+-2 (10%), -1 (10%)
				//		2-2-2. X3와 높이가 다르다면 "계단" => H0 (80%), H+-1 (10%), H+- (5%), -1 (5%)
				if (heightIndex_X1 != heightIndex_X2)
				{
					nextHeightIndex = heightIndex_X1;
				}
				else
				{
					if (heightIndex_X1 == heightIndex_X3)
					{
						//2-2-1
						if (rand < 60.0f)
						{
							nextHeightIndex = heightIndex_X1;
						}
						else if (rand < 60.0f + 20.0f)
						{
							if (isRandPlus)
							{
								nextHeightIndex = heightIndex_X1 + 1;
								if (nextHeightIndex > _tileSize_Height - 1)
								{
									//반전
									nextHeightIndex = heightIndex_X1 - 1;
								}
							}
							else
							{
								nextHeightIndex = heightIndex_X1 - 1;
								if (nextHeightIndex < 2)
								{
									//반전
									nextHeightIndex = heightIndex_X1 + 1;
								}
							}
						}
						else if (rand < 60.0f + 20.0f + 10.0f)
						{
							if (isRandPlus)
							{
								nextHeightIndex = heightIndex_X1 + 2;
								if (nextHeightIndex > _tileSize_Height - 1)
								{
									//반전 (1)
									nextHeightIndex = heightIndex_X1 - 1;
								}
							}
							else
							{
								nextHeightIndex = heightIndex_X1 - 2;
								if (nextHeightIndex < 2)
								{
									//반전 (1)
									nextHeightIndex = heightIndex_X1 + 1;
								}
							}
						}
						else
						{
							nextHeightIndex = -1;
						}
					}
					else
					{
						//2-2-2
						//		2-2-2. X3와 높이가 다르다면 "계단" => H0 (80%), H+-1 (10%), H+-2 (5%), -1 (5%)
						if (rand < 80.0f)
						{
							nextHeightIndex = heightIndex_X1;
						}
						else if (rand < 80.0f + 10.0f)
						{
							if (isRandPlus)
							{
								nextHeightIndex = heightIndex_X1 + 1;
								if (nextHeightIndex > _tileSize_Height - 1)
								{
									//반전
									nextHeightIndex = heightIndex_X1 - 1;
								}
							}
							else
							{
								nextHeightIndex = heightIndex_X1 - 1;
								if (nextHeightIndex < 2)
								{
									//반전
									nextHeightIndex = heightIndex_X1 + 1;
								}
							}
						}
						else if (rand < 80.0f + 10.0f + 5.0f)
						{
							if (isRandPlus)
							{
								nextHeightIndex = heightIndex_X1 + 2;
								if (nextHeightIndex > _tileSize_Height - 1)
								{
									//반전 (1)
									nextHeightIndex = heightIndex_X1 - 1;
								}
							}
							else
							{
								nextHeightIndex = heightIndex_X1 - 2;
								if (nextHeightIndex < 2)
								{
									//반전 (1)
									nextHeightIndex = heightIndex_X1 + 1;
								}
							}
						}
						else
						{
							nextHeightIndex = -1;
						}
					}
				}
			}

			//마지막 인덱스를 만들자
			for (int iY = 0; iY < _tileSize_Height; iY++)
			{
				_curTileMapRefs[lastX, iY] = null;
				if (iY < nextHeightIndex - 1)
				{
					_curTileMapTypes[lastX, iY] = TILE_SOIL;
				}
				else if (iY < nextHeightIndex)
				{
					_curTileMapTypes[lastX, iY] = TILE_GROUND;
				}
				else if (iY == nextHeightIndex)
				{
					_curTileMapTypes[lastX, iY] = TILE_UPPER;
				}
				else
				{
					_curTileMapTypes[lastX, iY] = TILE_EMPTY;
				}
			}

			//낭떠러지가 아닐때 -> 장애물을 만들자
			bool isNextObstacle = false;
			if (nextHeightIndex > 0)
			{
				//- X-1이 낭떠러지일때는 안나온다.
				//- X-1, X-2에서 장애물이 나왔다면 안나온다.
				//- next, X-1, X-2의 높이가 같아야 나온다.

				isNextObstacle = true;
				if (nextHeightIndex != heightIndex_X1 ||
					nextHeightIndex != heightIndex_X2 ||
					IsObstacleExist(lastX - 1) ||
					IsObstacleExist(lastX - 2))
				{
					isNextObstacle = false;
				}

				if (isNextObstacle)
				{
					rand = UnityEngine.Random.Range(0.0f, 100.0f);
					if (rand < 30.0f)
					{
						isNextObstacle = true;
					}
					else
					{
						isNextObstacle = false;
					}
				}
			}

			if (isNextObstacle)
			{
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					_curObstacleRefs[lastX, iY] = null;
					if (iY == nextHeightIndex)
					{
						_curObstacleTypes[lastX, iY] = OBJ_OBSTACLE_LOWER;
					}
					else if (iY == nextHeightIndex + 1)
					{
						_curObstacleTypes[lastX, iY] = OBJ_OBSTACLE_UPPER;
					}
					else
					{
						_curObstacleTypes[lastX, iY] = OBJ_EMPTY;
					}
				}
			}
			else
			{
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					_curObstacleRefs[lastX, iY] = null;
					_curObstacleTypes[lastX, iY] = OBJ_EMPTY;
				}
			}
		}




		public float TileY2RealPosY(int tileIndexY)
		{
			return _tilePosLB.y + tileIndexY * _tileSize;
		}

		public int GetHeightIndexY(int indexX)
		{
			for (int iY = 0; iY < _tileSize_Height; iY++)
			{
				if (_curTileMapTypes[indexX, iY] == TILE_UPPER)
				{
					return iY;
				}
			}
			return -1;
		}

		private float GetGroundHeightWorldPos(float worldPosX)
		{
			int indexX = (int)((worldPosX - _tilePosLB.x) / _tileSize + _tilePosOffsetX);
			int indexY = GetHeightIndexY(indexX);
			if (indexY >= 0)
			{
				return _tilePosLB.y + (indexY * _tileSize) + GROUND_HEIGHT;
			}
			else
			{
				return _tilePosLB.y + (indexY * _tileSize) - 100;
			}
		}

		private float GetGroundHeightWorldPosWithObstacle(float worldPosX)
		{
			int indexX = (int)((worldPosX - _tilePosLB.x) / _tileSize + _tilePosOffsetX);
			int indexY = GetHeightIndexY(indexX);

			if (indexY >= 0)
			{
				//Obstacle이 있는가
				if (_curObstacleTypes[indexX, indexY] != OBJ_EMPTY)
				{
					return _tilePosLB.y + ((indexY + 1) * _tileSize) + GROUND_HEIGHT;
				}
				else
				{
					return _tilePosLB.y + (indexY * _tileSize) + GROUND_HEIGHT;
				}
			}
			else
			{
				return _tilePosLB.y + (indexY * _tileSize) - 100;
			}

		}

		private float GetTileLeftWorldPos(float worldPosX)
		{
			int indexX = (int)((worldPosX - _tilePosLB.x) / _tileSize + _tilePosOffsetX);
			return _tilePosLB.x + (indexX * _tileSize) - _tilePosOffsetX;
		}

		public int GetTileIndexX(float worldPosX)
		{
			return (int)((worldPosX - _tilePosLB.x) / _tileSize + _tilePosOffsetX);
		}

		private void DebugMapTiles()
		{
			if (_curTileMapTypes == null)
			{
				return;
			}

			string strDebug = "TileMap [" + _tileSize_Width + "x" + _tileSize_Height + "]\n";
			for (int iY = _tileSize_Height - 1; iY >= 0; iY--)
			{
				strDebug += "( ";
				for (int iX = 0; iX < _tileSize_Width; iX++)
				{
					strDebug += _curTileMapTypes[iX, iY] + " ";
				}
				strDebug += ")\n";
			}
			Debug.Log(strDebug);

		}

		private bool IsObstacleExist(int indexX)
		{
			for (int iY = 0; iY < _tileSize_Height; iY++)
			{
				if (_curObstacleTypes[indexX, iY] != OBJ_EMPTY)
				{
					return true;
				}
			}
			return false;
		}

		private int GetCoinIndexY(int indexX)
		{
			for (int iY = 0; iY < _coinSize_Height; iY++)
			{
				if (_curCoinTypes[indexX, iY] != OBJ_EMPTY)
				{
					return iY;
				}
			}
			return -1;
		}

		/// <summary>
		/// MapTile을 배치하고, 위치를 지정한다.
		/// </summary>
		private void RefreshMapTileRefs(bool isIndexXChanged)
		{
			if (isIndexXChanged)
			{
				for (int iX = 0; iX < _tileSize_Width; iX++)
				{
					for (int iY = 0; iY < _tileSize_Height; iY++)
					{
						//_curTileMapTypes[iX, iY] = TILE_EMPTY;
						//_curTileMapRefs[iX, iY] = null;
						if (_curTileMapRefs[iX, iY] == null)
						{
							//아직 지정 안한경우
							switch (_curTileMapTypes[iX, iY])
							{
								case TILE_EMPTY:
									//냅두자
									break;

								case TILE_UPPER:
									//맨 마지막 iX만 제외하고 좌우를 비교하여 만든다.
									if (iX == 0)
									{
										//무조건 Upper	
										_curTileMapRefs[iX, iY] = _tilePools[MapTileType.Upper].Pop();
									}
									else if (iX == _tileSize_Width - 1)
									{
										//다음에 뭐가 올지 모르므로 생략한다.
									}
									else
									{
										if (_curTileMapTypes[iX - 1, iY] != TILE_UPPER)
										{
											//왼쪽이 끊어졌다.
											_curTileMapRefs[iX, iY] = _tilePools[MapTileType.UpperLeft].Pop();
										}
										else if (_curTileMapTypes[iX + 1, iY] != TILE_UPPER)
										{
											//오른쪽이 끊어졌다.
											_curTileMapRefs[iX, iY] = _tilePools[MapTileType.UpperRight].Pop();
										}
										else
										{
											//이어짐
											_curTileMapRefs[iX, iY] = _tilePools[MapTileType.Upper].Pop();
										}
									}
									break;

								case TILE_GROUND:
									//지상
									_curTileMapRefs[iX, iY] = _tilePools[MapTileType.Ground].Pop();
									break;

								case TILE_SOIL:
									//땅
									_curTileMapRefs[iX, iY] = _tilePools[MapTileType.Soil].Pop();
									break;
							}

						}

						if (_curObstacleRefs[iX, iY] == null)
						{
							if (_curObstacleTypes[iX, iY] == OBJ_OBSTACLE_LOWER)
							{
								_curObstacleRefs[iX, iY] = _objectPools[ObjectType.Obstacle].Pop();
								_curObstacleRefs[iX, iY].Show();
							}
						}
					}
				}
			}

			float tilePosX = 0.0f;
			float tilePosY = 0.0f;
			for (int iX = 0; iX < _tileSize_Width; iX++)
			{
				tilePosX = _tilePosLB.x + (iX - _tilePosOffsetX) * _tileSize;
				for (int iY = 0; iY < _tileSize_Height; iY++)
				{
					tilePosY = _tilePosLB.y + iY * _tileSize;

					if (_curTileMapRefs[iX, iY] != null)
					{
						_curTileMapRefs[iX, iY].SetWorldPos(tilePosX, tilePosY);
					}

					if (_curObstacleRefs[iX, iY] != null)
					{
						_curObstacleRefs[iX, iY].SetWorldPos(tilePosX, tilePosY);
					}
				}
			}
		}



		private void IncreseCoinX()
		{
			//X = 0일때는 Push하고 초기화
			for (int iY = 0; iY < _coinSize_Height; iY++)
			{
				if (_curCoinRefs[0, iY] != null)
				{
					_objectPools[ObjectType.Coin].Push(_curCoinRefs[0, iY]);
					_curCoinRefs[0, iY] = null;


				}
				_curCoinTypes[0, iY] = OBJ_EMPTY;
			}

			//X + 1의 값을 가져온다.
			for (int iX = 0; iX < _coinSize_Width - 1; iX++)
			{
				for (int iY = 0; iY < _coinSize_Height; iY++)
				{
					_curCoinTypes[iX, iY] = _curCoinTypes[iX + 1, iY];
					_curCoinRefs[iX, iY] = _curCoinRefs[iX + 1, iY];
				}
			}

			int lastX = _coinSize_Width - 1;

			for (int iY = 0; iY < _coinSize_Height; iY++)
			{
				_curCoinRefs[lastX, iY] = null;
				_curCoinTypes[lastX, iY] = OBJ_EMPTY;
			}

			//코인 생성 조건
			//1. 이전 생성 횟수 이후 "Increase 카운터"가 다 된 경우 생성 가능
			//2. 길이, 1~3줄인지는 생성시 결정하고, 그 값이 유지된다. (생성 중인지 변수로 저장한다)
			//3. 높이는 Ground Height/Obstacle에 따라 맞춘다.
			//	3-1. 코인 위치를 기준으로 높이를 샘플링해서 Min Index Y를 구한다.
			//	3-2. X-0, X-1, X-2과 X+1 (예상)의 높이를 구한다.
			//	X-0이 -1이라면 구간 예상 높이 중 가장 높은 높이를 유지한다.
			//  그 외에는 예상 높이에 맞게 설정

			if (_isCoinGenerating)
			{
				//생성 중이다.
				_coinGenLength--;
				if (_coinGenLength <= 0)
				{
					_isCoinGenerating = false;
					_coinNextGenCount = UnityEngine.Random.Range(8, 20);
				}
			}
			else
			{
				//생성 대기 중
				_coinNextGenCount--;
				if (_coinNextGenCount <= 0)
				{
					_isCoinGenerating = true;
					_coinGenLength = UnityEngine.Random.Range(3, 11);
					_coinGenLine = UnityEngine.Random.Range(1, 4);


				}
			}

			if (_isCoinGenerating)
			{
				//Debug.Log("Coin Gen");
				int nextCoinIndexY = -1;
				//생성 중이다.
				float coinPos_X0 = _tilePosLB.x + (lastX - _coinPosOffsetX) * _coinPosSize;
				float coinPos_Xp1 = _tilePosLB.x + ((lastX + 1) - _coinPosOffsetX) * _coinPosSize;

				float coinHeight_X0 = Mathf.Max(GetGroundHeightWorldPosWithObstacle(coinPos_X0), GetGroundHeightWorldPosWithObstacle(coinPos_X0 + _coinPosSize))
									+ _coinPosSize * 2.0f;
				float coinHeight_Xp1 = Mathf.Max(GetGroundHeightWorldPosWithObstacle(coinPos_Xp1), GetGroundHeightWorldPosWithObstacle(coinPos_Xp1 + _coinPosSize))
									+ _coinPosSize * 2.0f;

				int iCoinHeight_X0 = Mathf.Max((int)((coinHeight_X0 - _tilePosLB.y) / _coinPosSize), -1);
				int iCoinHeight_Xm1 = GetCoinIndexY(lastX - 1);
				//int iCoinHeight_Xm2 = GetCoinIndexY(lastX - 2);
				int iCoinHeight_Xp1 = Mathf.Max((int)((coinHeight_Xp1 - _tilePosLB.y) / _coinPosSize), -1);

				//Debug.Log("[" + iCoinHeight_X0 + " / " + iCoinHeight_Xm1 + " / " + iCoinHeight_Xm2 + " / " + iCoinHeight_Xp1 + "]");

				//형태
				//만약 이전 코인이 없다면.. (높이 예상이 안됨)
				if (iCoinHeight_Xm1 < 0 && iCoinHeight_X0 < 0 && iCoinHeight_Xp1 < 0)
				{
					//현재, 이전, 다음 코인 높이를 예상할 수 없다.
					//생성 불가능한 상태
					nextCoinIndexY = -1;
				}
				else
				{
					//일단 생성은 가능하다.
					//1. 높이 값이 있다면
					//	1-1. m1, p1 중 하나라도 유효한 값이 있을 때
					//		1-1-1. x0이 max라면 => 높이 값을 이용한다.
					//		1-1-2. x0이 max가 아니라면 => max - 1의 값을 지정한다.
					//	1-2. m1, p1 중 유효한 값이 없을 때 => 높이 값을 이용한다.
					//2. 높이 값이 없다면
					//	2-1. m1, p1 둘다 유효한 값이 있을 때
					//		2-1-1. 두 값이 같다면 그 값을 적용
					//		2-1-2. 두 값이 다르다면 max 값의 -1의 값을 적용한다.
					//	2-2. m1, p1 중 하나만 유효할 때 => 유효한 값을 그대로 따른다.

					if (iCoinHeight_X0 >= 0)
					{
						//1. 높이 값이 있다면
						if (iCoinHeight_Xm1 >= 0 || iCoinHeight_Xp1 >= 0)
						{
							//	1-1. m1, p1 중 하나라도 유효한 값이 있을 때
							int maxHeight = Mathf.Max(iCoinHeight_X0, iCoinHeight_Xm1, iCoinHeight_Xp1, 1);
							if (maxHeight == iCoinHeight_X0)
							{
								//1-1-1. x0이 max라면 => 높이 값을 이용한다.
								nextCoinIndexY = iCoinHeight_X0;
							}
							else
							{
								//1-1-2. x0이 max가 아니라면 => max - 1의 값을 지정한다.
								nextCoinIndexY = maxHeight - 1;
							}
						}
						else
						{
							//	1-2. m1, p1 중 유효한 값이 없을 때 => 높이 값을 이용한다.
							nextCoinIndexY = iCoinHeight_X0;
						}
					}
					else
					{
						//2. 높이 값이 없다면
						if (iCoinHeight_Xm1 >= 0 && iCoinHeight_Xp1 >= 0)
						{
							//2-1. m1, p1 둘다 유효한 값이 있을 때
							if (iCoinHeight_Xm1 == iCoinHeight_Xp1)
							{
								//2-1-1. 두 값이 같다면 그 값을 적용
								nextCoinIndexY = iCoinHeight_Xm1;
							}
							else
							{
								//2-1-2. 두 값이 다르다면 max 값의 -1의 값을 적용한다.
								nextCoinIndexY = Mathf.Max(iCoinHeight_Xm1, iCoinHeight_Xp1) - 1;
							}
						}
						else
						{
							//2-2. m1, p1 중 하나만 유효할 때 => 유효한 값을 그대로 따른다.
							nextCoinIndexY = Mathf.Max(iCoinHeight_Xm1, iCoinHeight_Xp1);
						}
					}

				}





				if (nextCoinIndexY >= 0)
				{

					for (int iY = 0; iY < _coinSize_Height; iY++)
					{
						_curCoinRefs[lastX, iY] = null;
						if (iY >= nextCoinIndexY && iY < nextCoinIndexY + _coinGenLine)
						//if (iY == nextCoinIndexY)
						{
							_curCoinTypes[lastX, iY] = OBJ_COIN;
						}
						else
						{
							_curCoinTypes[lastX, iY] = OBJ_EMPTY;
						}
					}
				}
			}
		}

		private void RefreshCoinRefs(bool isCoinIndexChanged)
		{
			if (isCoinIndexChanged)
			{
				for (int iX = 0; iX < _coinSize_Width; iX++)
				{
					for (int iY = 0; iY < _coinSize_Height; iY++)
					{
						if (_curCoinRefs[iX, iY] == null)
						{
							if (_curCoinTypes[iX, iY] == OBJ_COIN)
							{
								_curCoinRefs[iX, iY] = _objectPools[ObjectType.Coin].Pop();
								if (_curCoinRefs[iX, iY] != null)
								{
									_curCoinRefs[iX, iY].Show();
								}
							}
						}
					}
				}
			}

			float cosPosX = 0.0f;
			float cosPosY = 0.0f;
			for (int iX = 0; iX < _coinSize_Width; iX++)
			{
				cosPosX = _tilePosLB.x + (iX - _coinPosOffsetX) * _coinPosSize;
				for (int iY = 0; iY < _coinSize_Height; iY++)
				{
					cosPosY = _tilePosLB.y + iY * _coinPosSize;

					if (_curCoinRefs[iX, iY] != null)
					{
						_curCoinRefs[iX, iY].SetWorldPos(cosPosX, cosPosY);
					}
				}
			}
		}

		//-----------------------------------------------------------------------------------------------
		private void UpdateCamera(bool isForceToFocus, float deltaTime)
		{
			float focusY = _camFocus.position.y;
			float focusY_UpperLimit = _camFocus_UpperLimit.position.y;
			float focusY_LowerLimit = _camFocus_LowerLimit.position.y;

			float chaY = _character.transform.position.y;
			float deltaY = 0.0f;

			float sightX = _camPos_Sight.position.x;

			float groundY = GetGroundHeightWorldPos(sightX);
			if (groundY > _tilePosLB.y)
			{
				_predictSightPosY = groundY - _tileSize * 0.5f;
			}


			if (isForceToFocus)
			{
				deltaY = chaY - focusY;
			}
			else
			{
				if (chaY < focusY_LowerLimit)
				{
					deltaY = chaY - focusY_LowerLimit;
				}
				else if (chaY > focusY_UpperLimit)
				{
					deltaY = chaY - focusY_UpperLimit;
				}
				else
				{
					//if (Mathf.Abs(chaY - focusY) < 0.5f)
					//{
					//	deltaY = chaY - focusY;
					//}
					//else
					//{
					//	deltaY = (chaY * 0.1f + focusY * 0.9f) - focusY;
					//}

					float focusWeight = 0.6f * deltaTime;
					deltaY = ((chaY * 0.3f + _predictSightPosY * 0.7f) * focusWeight + focusY * (1.0f - focusWeight)) - focusY;
				}


			}

			float nextPosY = _gameCamera.transform.position.y + deltaY;
			if (nextPosY < _camPos_LowerLimit.position.y)
			{
				nextPosY = _camPos_LowerLimit.position.y;
			}

			_gameCamera.transform.position = new Vector3(_gameCamera.transform.position.x,
															nextPosY,
															_gameCamera.transform.position.z);


		}

		//-----------------------------------------------------------------------------------------------
		public class TilePool
		{
			//private MapTileType _mapTileType;
			private List<apTutorial_RunnerMapTile> _total = new List<apTutorial_RunnerMapTile>();
			private List<apTutorial_RunnerMapTile> _live = new List<apTutorial_RunnerMapTile>();
			private List<apTutorial_RunnerMapTile> _remain = new List<apTutorial_RunnerMapTile>();
			private int _poolSize = 0;

			private Transform _poolGroup;

			public TilePool(MapTileType mapTileType, int poolSize, Transform poolGroup)
			{
				//_mapTileType = mapTileType;
				_poolSize = poolSize;
				_poolGroup = poolGroup;
			}

			public void MakePool(apTutorial_RunnerMapTile srcMapTile)
			{
				_total.Clear();
				_live.Clear();
				_remain.Clear();

				GameObject dupMapTileGameObject = null;
				apTutorial_RunnerMapTile dupMapTile = null;
				for (int i = 0; i < _poolSize; i++)
				{
					dupMapTileGameObject = Instantiate<GameObject>(srcMapTile.gameObject);
					dupMapTileGameObject.transform.parent = _poolGroup;
					dupMapTileGameObject.transform.localPosition = Vector3.zero;

					dupMapTile = dupMapTileGameObject.GetComponent<apTutorial_RunnerMapTile>();
					dupMapTile.Hide();

					_total.Add(dupMapTile);
					_remain.Add(dupMapTile);
				}
			}

			public void PushAll()
			{
				apTutorial_RunnerMapTile tile = null;

				_live.Clear();
				_remain.Clear();
				for (int i = 0; i < _total.Count; i++)
				{
					tile = _total[i];
					tile.Hide();
					tile.SetLocalPos(Vector3.zero);

					_remain.Add(tile);
				}
			}

			public apTutorial_RunnerMapTile Pop()
			{
				if (_remain.Count == 0)
				{
					return null;
				}

				apTutorial_RunnerMapTile popTile = _remain[0];
				_remain.RemoveAt(0);
				_live.Add(popTile);

				popTile.Show();

				return popTile;
			}

			public void Push(apTutorial_RunnerMapTile pushTile)
			{
				if (!_remain.Contains(pushTile))
				{
					_remain.Add(pushTile);
				}

				_live.Remove(pushTile);

				pushTile.Hide();
				pushTile.SetLocalPos(Vector3.zero);
			}
		}


		public class ObjectPool
		{
			//private ObjectType _objectType;
			private List<apTutorial_RunnerObject> _total = new List<apTutorial_RunnerObject>();
			private List<apTutorial_RunnerObject> _live = new List<apTutorial_RunnerObject>();
			private List<apTutorial_RunnerObject> _remain = new List<apTutorial_RunnerObject>();
			private int _poolSize = 0;

			private Transform _poolGroup;

			public ObjectPool(ObjectType objectType, int poolSize, Transform poolGroup)
			{
				//_objectType = objectType;
				_poolSize = poolSize;
				_poolGroup = poolGroup;
			}

			public void MakePool(apTutorial_RunnerObject srcObject)
			{
				_total.Clear();
				_live.Clear();
				_remain.Clear();

				GameObject dupMapTileGameObject = null;
				apTutorial_RunnerObject dupObject = null;
				for (int i = 0; i < _poolSize; i++)
				{
					dupMapTileGameObject = Instantiate<GameObject>(srcObject.gameObject);
					dupMapTileGameObject.transform.parent = _poolGroup;
					dupMapTileGameObject.transform.localPosition = Vector3.zero;

					dupObject = dupMapTileGameObject.GetComponent<apTutorial_RunnerObject>();
					dupObject.Hide();

					_total.Add(dupObject);
					_remain.Add(dupObject);
				}
			}

			public void PushAll()
			{
				apTutorial_RunnerObject pushObject = null;

				_live.Clear();
				_remain.Clear();
				for (int i = 0; i < _total.Count; i++)
				{
					pushObject = _total[i];
					pushObject.Hide();
					pushObject.SetLocalPos(Vector3.zero);

					_remain.Add(pushObject);
				}
			}

			public apTutorial_RunnerObject Pop()
			{
				if (_remain.Count == 0)
				{
					return null;
				}

				apTutorial_RunnerObject popObject = _remain[0];
				_remain.RemoveAt(0);
				_live.Add(popObject);

				//popTile.Show();

				return popObject;
			}

			public void Push(apTutorial_RunnerObject pushObject)
			{
				if (!_remain.Contains(pushObject))
				{
					_remain.Add(pushObject);
				}

				_live.Remove(pushObject);

				pushObject.Hide();
				pushObject.SetLocalPos(Vector3.zero);
			}
		}


	}
}