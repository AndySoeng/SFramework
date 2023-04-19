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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// A class that define information about forces added for physical effects
	/// </summary>
	public class apForceUnit
	{
		// Members
		//-------------------------------------------------
		public enum SHAPE_TYPE
		{
			// 특정 점에서 방사형으로 힘이 작동한다.
			/// <summary>The force is radially applied at a certain point.</summary>
			Point,
			// 위치는 관계없고, 방향만 가지는 힘
			/// <summary>
			/// There is no position information, only the direction of force is stored.
			/// </summary>
			Direction
		}

		public enum LIVE_TYPE
		{
			// 일정 시간 후에는 종료되는 일시적인 힘
			/// <summary>
			/// Temporary force that ends after a certain period of time
			/// </summary>
			Once,
			// 종료 명령이 나기까지 계속 지속되는 힘
			/// <summary>
			/// The force that lasts until the termination request occurs.
			/// </summary>
			Continuous,
		}


		//[1] 위치 / 방향
		private SHAPE_TYPE _shape_Type = SHAPE_TYPE.Point;

		//>> 1) 포인트 방식
		private Vector2 _pointPosW = Vector2.zero;
		private float _pointRadius = 0.0f;

		//>> 2) 직선 방식
		private Vector2 _directionW = Vector2.zero;
		private bool _isDirectionWave = false;
		private Vector2 _directionWaveSize = Vector2.zero;
		private Vector2 _directionWaveTime = Vector2.zero;


		//[2] 세기
		private float _power = 0.0f;
		private bool _isPowerWave = false;
		private float _powerWaveSize = 0.0f;
		private float _powerWaveTime = 0.0f;

		//[3] 시간
		private LIVE_TYPE _liveType = LIVE_TYPE.Once;
		private float _liveTime = 0.0f;//Once인 경우

		public bool _isInitShape = false;
		public bool _isInitPower = false;
		public bool _isInitLive = false;

		//처리 변수
		private float _tLive = 0.0f;
		private bool _isLive = false;


		//Wave의 영향을 받는 변수들은 따로 계산하자
		private Vector2 _curDirectionW = Vector2.zero;
		private float _curPower = 0.0f;

		private Vector2 _tWave_Direction = Vector2.zero;
		private float _tWave_Power = 0.0f;

		private Vector2 _itp_Direction = Vector2.zero;
		private float _itp_Power = 0.0f;



		// Get / Set
		//-------------------------------------------------------
		public bool IsLive { get { return _isLive; } }
		public SHAPE_TYPE ShapeType { get { return _shape_Type; } }
		public Vector2 PointPos { get { return _pointPosW; } }
		public float PointRadius { get { return _pointRadius; } }
		public Vector2 Direction { get { return _curDirectionW; } }
		public float Power { get { return _curPower; } }



		// Init
		//-------------------------------------------------
		private apForceUnit()
		{
			_isInitShape = false;
			_isInitPower = false;
			_isInitLive = false;
		}

		// Make
		//-------------------------------------------------
		public static apForceUnit Make()
		{
			return new apForceUnit();
		}

		public apForceUnit SetShape(Vector2 pointPosW, float radius)
		{
			_shape_Type = SHAPE_TYPE.Point;
			_pointPosW = pointPosW;
			_pointRadius = radius;
			_isInitShape = true;
			return this;
		}

		public apForceUnit SetShape(Vector2 directionW)
		{
			_shape_Type = SHAPE_TYPE.Direction;
			_directionW = directionW;
			_isDirectionWave = false;
			_directionWaveSize = Vector2.zero;
			_directionWaveTime = Vector2.zero;
			_isInitShape = true;

			_curDirectionW = _directionW.normalized;
			return this;
		}

		public apForceUnit SetShape(Vector2 directionW, Vector2 waveSize, Vector2 waveTime)
		{
			_shape_Type = SHAPE_TYPE.Direction;
			_directionW = directionW;
			_isDirectionWave = true;
			_directionWaveSize = waveSize;
			_directionWaveTime = waveTime;
			_isInitShape = true;

			_curDirectionW = _directionW.normalized;
			return this;
		}

		public apForceUnit SetPower(float power)
		{
			_power = power;
			_isPowerWave = false;
			_powerWaveSize = 0.0f;
			_powerWaveTime = 0.0f;
			_isInitPower = true;

			_curPower = _power;
			return this;
		}

		public apForceUnit SetPower(float power, float waveSize, float waveTime)
		{
			_power = power;
			_isPowerWave = true;
			_powerWaveSize = waveSize;
			_powerWaveTime = waveTime;
			_isInitPower = true;

			_curPower = _power;
			return this;
		}

		public void EmitLoop()
		{
			_liveType = LIVE_TYPE.Continuous;
			_liveTime = 0.0f;
			_isInitLive = true;
			_tLive = 0.0f;
			_isLive = true;
		}

		public void EmitOnce(float liveTime)
		{
			_liveType = LIVE_TYPE.Once;
			_liveTime = liveTime;
			_isInitLive = true;
			_tLive = 0.0f;
			_isLive = true;
		}


		// Update
		//------------------------------------------------------
		/// <summary>
		/// 업데이트를 한다. 
		/// 만약 종료가 된다면 true를 리턴한다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <returns></returns>
		public bool Update(float tDelta)
		{
			if (!_isLive)
			{
				//이미 죽었네요..
				return true;
			}

			//웨이브를 처리하자
			if (_shape_Type == SHAPE_TYPE.Direction)
			{
				if (_isDirectionWave)
				{
					_tWave_Direction.x += tDelta;
					_tWave_Direction.y += tDelta;
					if (_tWave_Direction.x > _directionWaveTime.x) { _tWave_Direction.x -= _directionWaveTime.x; }
					if (_tWave_Direction.y > _directionWaveTime.y) { _tWave_Direction.y -= _directionWaveTime.y; }

					if (_directionWaveTime.x < 0.001f)	{ _itp_Direction.x = 0.0f; }
					else								{ _itp_Direction.x = _tWave_Direction.x / _directionWaveTime.x; }

					if (_directionWaveTime.y < 0.001f)	{ _itp_Direction.y = 0.0f; }
					else								{ _itp_Direction.y = _tWave_Direction.y / _directionWaveTime.y; }

					_curDirectionW.x = _directionW.x + Mathf.Sin(_itp_Direction.x * Mathf.PI * 2.0f) * _directionWaveSize.x;
					_curDirectionW.y = _directionW.y + Mathf.Sin(_itp_Direction.y * Mathf.PI * 2.0f) * _directionWaveSize.y;

					_curDirectionW.Normalize();
				}
			}


			if (_isPowerWave)
			{
				_tWave_Power += tDelta;
				if (_tWave_Power > _powerWaveTime)
				{
					_tWave_Power -= _powerWaveTime;
				}

				if (_powerWaveTime < 0.001f)
				{
					_itp_Power = 0.0f;
				}
				else
				{
					_itp_Power = _tWave_Power / _powerWaveTime;
				}

				_curPower = _power + Mathf.Sign(_itp_Power * Mathf.PI * 2.0f) * _powerWaveSize;
			}

			//처리해야하는 것
			//웨이브, Once시 시간, 
			if (_liveType == LIVE_TYPE.Once)
			{
				_tLive += tDelta;
				if (_tLive > _liveTime)
				{
					//끝!
					_curPower = 0.0f;
					_isLive = false;
					return true;
				}
				else
				{
					_curPower *= Mathf.Clamp01(1.0f - (_tLive / _liveTime));
				}
			}
			return false;
		}
	}

}