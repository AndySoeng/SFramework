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

	//Portrait를 통해서 힘을 가할 수 있는 기능을 관리한다.
	// Editor/Opt 모두 사용 가능하며, 힘 자체는 World 좌표계로 존재한다.
	// 힘은 복합적으로 제공되며, Physics가 적용된 모든 객체에 포함된다.
	// 직선형, 방사형이 있고, 충격과 지속형이 있다.
	// 터치에 의한 당김도 구현한다. (터치는 별도로 시작->지속->끝의 과정을 거친다)
	
	/// <summary>
	/// A manager class that adds a force to give a physical effect.
	/// </summary>
	public class apForceManager
	{
		//Members
		//----------------------------------------------------------
		private List<apForceUnit> _forceUnits = new List<apForceUnit>();
		private bool _isAnyForceUnit = false;


		public const int MAX_TOUCH_UNIT = 16;//최대 16개의 터치가 가능하다.
		private apPullTouch[] _touchUnits = new apPullTouch[MAX_TOUCH_UNIT];
		private bool _isAnyTouchUnit = false;
		private int _nTouchUnit = 0;
		private Int32 _touchProcessCode = 0;//처리중인 TouchID를 bit로 넣어서 저장하자. (유효성 검사위함)




		private Vector2 _tmpF_Sum = Vector2.zero;

		// Init
		//----------------------------------------------------------
		public apForceManager()
		{
			if(_touchUnits == null)
			{
				_touchUnits = new apPullTouch[MAX_TOUCH_UNIT];
			}

			for (int i = 0; i < MAX_TOUCH_UNIT; i++)
			{
				_touchUnits[i] = new apPullTouch(i);
			}

			if(_forceUnits == null)
			{
				_forceUnits = new List<apForceUnit>();
			}

			ClearAll();
		}


		public void ClearAll()
		{
			ClearForce();

			for (int i = 0; i < MAX_TOUCH_UNIT; i++)
			{
				if (_touchUnits[i] == null)
				{
					_touchUnits[i] = new apPullTouch(i);
				}
				_touchUnits[i].SetDisable();
			}
			_isAnyTouchUnit = false;
			_nTouchUnit = 0;
		}


		public void ClearForce()
		{	
			_forceUnits.Clear();
			_isAnyForceUnit = false;

		}

		//추가 21.7.8 : 일부만 삭제할 수 있다.
		public void RemoveForce(apForceUnit forceUnit)
		{
			if(forceUnit == null)
			{
				return;
			}

			if(_forceUnits.Contains(forceUnit))
			{
				_forceUnits.Remove(forceUnit);
			}
			_isAnyForceUnit = _forceUnits.Count > 0;
		}

		




		// Update
		//----------------------------------------------------------
		public void Update(float tDelta)
		{
			if (!_isAnyForceUnit)
			{
				return;
			}
			int nForce = _forceUnits.Count;
			apForceUnit curForce = null;
			bool isAnyRemoved = false;
			for (int i = 0; i < nForce; i++)
			{
				curForce = _forceUnits[i];
				if (curForce.Update(tDelta))
				{
					isAnyRemoved = true;//뭔가 없애야 할게 있다.
				}
			}
			if (isAnyRemoved)
			{
				//업데이트가 종료되어 Live가 아닌 것들을 삭제하자
				_forceUnits.RemoveAll(delegate (apForceUnit a)
				{
					return !a.IsLive;
				});
			}

			//PulledTouch는 업데이트하지 않아요
		}



		// Functions
		//----------------------------------------------------------
		// Make Force
		//----------------------------------------------------------
		public apForceUnit AddForce_Point(Vector2 pointPosW, float radius)
		{
			apForceUnit newForce = apForceUnit.Make().SetShape(pointPosW, radius);
			_forceUnits.Add(newForce);
			_isAnyForceUnit = true;
			return newForce;
		}

		public apForceUnit AddForce_Direction(Vector2 directionW)
		{
			apForceUnit newForce = apForceUnit.Make().SetShape(directionW);
			_forceUnits.Add(newForce);
			_isAnyForceUnit = true;
			return newForce;
		}

		public apForceUnit AddForce_Direction(Vector2 directionW, Vector2 waveSize, Vector2 waveTime)
		{
			apForceUnit newForce = apForceUnit.Make().SetShape(directionW, waveSize, waveTime);
			_forceUnits.Add(newForce);
			_isAnyForceUnit = true;
			return newForce;
		}

		// Make PulledTouch
		//----------------------------------------------------------
		public apPullTouch AddTouch(Vector2 posW, float radius)
		{
			int newTouchID = -1;
			//Disabled된 것을 찾자
			for (int i = 0; i < MAX_TOUCH_UNIT; i++)
			{
				if (!_touchUnits[i].IsLive)
				{
					newTouchID = i;
					break;
				}
			}
			if (newTouchID < 0)
			{
				Debug.LogError("AddTouch Failed : Too Many TouchEvents [" + MAX_TOUCH_UNIT + "]");
				return null;
			}

			_touchUnits[newTouchID].SetEnable(posW, radius);
			CalculateTouchCount();

			return _touchUnits[newTouchID];
		}

		public void RemoveTouch(int touchID)
		{
			if (!_isAnyTouchUnit)
			{
				return;
			}
			if (touchID < 0 || touchID >= MAX_TOUCH_UNIT)
			{
				Debug.LogError("RemoveTouch Failed : Wrong ID [" + touchID + "]");
				return;
			}
			_touchUnits[touchID].SetDisable();

			CalculateTouchCount();
		}


		public void RemoveTouch(apPullTouch touch)
		{
			if(touch == null)
			{
				return;
			}
			
			RemoveTouch(touch.TouchID);
		}


		public void ClearTouch()
		{
			for (int i = 0; i < MAX_TOUCH_UNIT; i++)
			{
				_touchUnits[i].SetDisable();
			}

			_isAnyTouchUnit = false;
			_nTouchUnit = 0;
			_touchProcessCode = 0;
		}

		private void CalculateTouchCount()
		{
			_nTouchUnit = 0;
			_touchProcessCode = 0;
			for (int i = 0; i < MAX_TOUCH_UNIT; i++)
			{
				_touchProcessCode = (_touchProcessCode << 1);
				if (_touchUnits[i].IsLive)
				{
					_nTouchUnit++;
					_touchProcessCode += 1;
				}
			}
			if (_nTouchUnit == 0)
			{
				_isAnyTouchUnit = false;
				_touchProcessCode = 0;
			}
			else
			{
				_isAnyTouchUnit = true;
			}
		}

		public apPullTouch GetTouch(int touchID)
		{
			if (touchID < 0 || touchID >= MAX_TOUCH_UNIT)
			{
				Debug.LogError("GetTouch Failed : Wrong ID [" + touchID + "]");
				return null;
			}

			return _touchUnits[touchID];
		}

		public void SetTouchPosition(int touchID, Vector2 posW)
		{
			if (touchID < 0 || touchID >= MAX_TOUCH_UNIT)
			{
				Debug.LogError("SetTouchPosition Failed : Wrong ID [" + touchID + "]");
				return;
			}
			if (!_touchUnits[touchID].IsLive)
			{
				return;
			}

			_touchUnits[touchID].SetPos(posW);
		}

		public void SetTouchPosition(apPullTouch touch, Vector2 posW)
		{
			if (touch == null)
			{
				return;
			}

			if (!touch.IsLive)
			{
				return;
			}
			touch.SetPos(posW);
		}



		// Get / Set
		//----------------------------------------------------------
		public bool IsAnyForceEvent
		{
			get { return _isAnyForceUnit; }
		}

		public Vector2 GetForce(Vector2 targetPosW)
		{
			if (!_isAnyForceUnit)
			{
				return Vector2.zero;
			}

			_tmpF_Sum = Vector2.zero;
			int nForce = _forceUnits.Count;
			apForceUnit curForce = null;
			Vector2 vecPower2Target = Vector2.zero;
			for (int i = 0; i < nForce; i++)
			{
				curForce = _forceUnits[i];
				if (!curForce.IsLive)
				{
					continue;
				}

				if (curForce.ShapeType == apForceUnit.SHAPE_TYPE.Point)
				{
					//Point 타입이면
					//방향을 직접 계산해서 힘을 가해야한다.
					//거리에 따라 멀어지면 힘이 선형으로 줄어든다..
					if (curForce.PointRadius < 0.001f)
					{
						continue;
					}
					vecPower2Target = (targetPosW - curForce.PointPos);
					float distItp = Mathf.Clamp01(1.0f - (vecPower2Target.magnitude / curForce.PointRadius));
					float power = curForce.Power * distItp;
					_tmpF_Sum += vecPower2Target.normalized * power;
				}
				else
				{
					//Direction 타입이면
					//그냥 그 자체로 사용하면 된다.
					_tmpF_Sum += curForce.Direction * curForce.Power;
				}
			}

			return _tmpF_Sum;
		}

		public bool IsAnyTouchEvent { get { return _isAnyTouchUnit; } }
		public int TouchCount { get { return _nTouchUnit; } }
		public int TouchProcessCode { get { return _touchProcessCode; } }
	}

}