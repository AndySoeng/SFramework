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
	/// 메시 Pin의 Render Unit용 변수. Render Vertex들을 가지고 있다.
	/// Default의 Pin Group의 업데이트용 기능만 가지고 있으며, 매핑도 된다.
	/// </summary>
	public class apRenderPinGroup
	{
		// Members
		//----------------------------------------------
		public apRenderUnit _parentRenderUnit = null;
		public apMeshGroup _parentMeshGroup = null;
		public apMesh _parentMesh = null;


		public apRenderPin[] _pins = null;
		public Dictionary<apMeshPin, apRenderPin> _srcPin2RenderPin = new Dictionary<apMeshPin, apRenderPin>();

		public List<apRenderPinCurve> _curves = new List<apRenderPinCurve>();
		public Dictionary<apMeshPinCurve, apRenderPinCurve> _srcCurve2RenderCurve = new Dictionary<apMeshPinCurve, apRenderPinCurve>();

		private int _nPins = 0;
		public int NumPins { get { return _nPins; } }

		private int _nCurves = 0;
		public int NumCurves { get { return _nCurves; } }
		

		// Init
		//-----------------------------------------------
		public apRenderPinGroup(apRenderUnit parentRenderUnit, apMeshGroup parentMeshGroup, apMesh parentMesh)
		{
			_parentRenderUnit = parentRenderUnit;
			_parentMeshGroup = parentMeshGroup;
			_parentMesh = parentMesh;

			_pins = null;
			if (_srcPin2RenderPin == null)		{ _srcPin2RenderPin = new Dictionary<apMeshPin, apRenderPin>(); }
			if (_curves == null)				{ _curves = new List<apRenderPinCurve>(); }
			if (_srcCurve2RenderCurve == null)	{ _srcCurve2RenderCurve = new Dictionary<apMeshPinCurve, apRenderPinCurve>(); }
			
			
			_srcPin2RenderPin.Clear();

			_curves.Clear();
			_srcCurve2RenderCurve.Clear();

			_nPins = 0;
			_nCurves = 0;
		}

		/// <summary>
		/// 소스 메시를 이용해서 렌더 핀(커브 포함)을 만들자.
		/// </summary>
		public void MakeRenderPins()
		{
			_pins = null;
			if (_srcPin2RenderPin == null)		{ _srcPin2RenderPin = new Dictionary<apMeshPin, apRenderPin>(); }
			if (_curves == null)				{ _curves = new List<apRenderPinCurve>(); }
			if (_srcCurve2RenderCurve == null)	{ _srcCurve2RenderCurve = new Dictionary<apMeshPinCurve, apRenderPinCurve>(); }

			_srcPin2RenderPin.Clear();
			_curves.Clear();
			_srcCurve2RenderCurve.Clear();

			_nPins = 0;
			_nCurves = 0;

			apMeshPinGroup srcPinGroup = _parentMesh._pinGroup;
			if(srcPinGroup == null)
			{
				return;
			}

			int nSrcPins = srcPinGroup._pins_All != null ? srcPinGroup._pins_All.Count : 0;
			if(nSrcPins == 0)
			{
				return;
			}

			_pins = new apRenderPin[nSrcPins];
			
			apMeshPin srcPin = null;
			apRenderPin rPin = null;
			apRenderPinCurve rCurve = null;
			for (int iPin = 0; iPin < nSrcPins; iPin++)
			{
				//1. 하나씩 변환을 한다.
				srcPin = srcPinGroup._pins_All[iPin];
				rPin = new apRenderPin(srcPin, this, _parentRenderUnit, _parentMesh);

				_pins[iPin] = rPin;
				_srcPin2RenderPin.Add(srcPin, rPin);

				//2. 커브도 만든다.
				//Next만 체크하자 (Prev까지 체크하는건 불필요)
				if(srcPin._nextCurve != null)
				{
					if(!_srcCurve2RenderCurve.ContainsKey(srcPin._nextCurve))
					{
						rCurve = new apRenderPinCurve(srcPin._nextCurve, this);
						_curves.Add(rCurve);
						_srcCurve2RenderCurve.Add(srcPin._nextCurve, rCurve);
					}
					
				}
			}

			_nPins = _pins.Length;
			_nCurves = _curves.Count;

			//3. 매핑 데이터를 이용해서 연결을 한다.
			apRenderPinCurve linkedCurve_Prev = null;
			apRenderPinCurve linkedCurve_Next = null;
			apRenderPin linkedPin_Prev = null;
			apRenderPin linkedPin_Next = null;

			for (int iPin = 0; iPin < _nPins; iPin++)
			{
				rPin = _pins[iPin];

				linkedCurve_Prev = null;
				linkedCurve_Next = null;
				linkedPin_Prev = null;
				linkedPin_Next = null;

				if(rPin._srcPin._prevCurve != null)
				{
					if(_srcCurve2RenderCurve.ContainsKey(rPin._srcPin._prevCurve))
					{
						linkedCurve_Prev = _srcCurve2RenderCurve[rPin._srcPin._prevCurve];
					}	
				}

				if(rPin._srcPin._nextCurve != null)
				{
					if(_srcCurve2RenderCurve.ContainsKey(rPin._srcPin._nextCurve))
					{
						linkedCurve_Next = _srcCurve2RenderCurve[rPin._srcPin._nextCurve];
					}
				}

				if(rPin._srcPin._prevPin != null)
				{
					if(_srcPin2RenderPin.ContainsKey(rPin._srcPin._prevPin))
					{
						linkedPin_Prev = _srcPin2RenderPin[rPin._srcPin._prevPin];
					}
				}

				if(rPin._srcPin._nextPin != null)
				{
					if(_srcPin2RenderPin.ContainsKey(rPin._srcPin._nextPin))
					{
						linkedPin_Next = _srcPin2RenderPin[rPin._srcPin._nextPin];
					}
				}

				//연결 정보를 Pin에 입력하자
				rPin.LinkData(	linkedCurve_Prev,
								linkedCurve_Next,
								linkedPin_Prev,
								linkedPin_Next);
				
			}

			//커브의 양쪽도 연결한다.
			if(_nCurves > 0)
			{
				for (int iCurve = 0; iCurve < _nCurves; iCurve++)
				{
					rCurve = _curves[iCurve];

					linkedPin_Prev = null;
					linkedPin_Next = null;

					if(rCurve._srcCurve._prevPin != null)
					{
						if(_srcPin2RenderPin.ContainsKey(rCurve._srcCurve._prevPin))
						{
							linkedPin_Prev = _srcPin2RenderPin[rCurve._srcCurve._prevPin];
						}
					}

					if(rCurve._srcCurve._nextPin != null)
					{
						if(_srcPin2RenderPin.ContainsKey(rCurve._srcCurve._nextPin))
						{
							linkedPin_Next = _srcPin2RenderPin[rCurve._srcCurve._nextPin];
						}
					}

					rCurve.LinkData(linkedPin_Prev, linkedPin_Next);
				}
			}
		}



		// Functions (일괄 제어)
		//----------------------------------------------------------------------------------
		public void ResetData()
		{
			if(_nPins == 0)
			{
				return;
			}
			for (int i = 0; i < _nPins; i++)
			{
				_pins[i].ResetData();
			}
		}

		// Functions (Update)
		//----------------------------------------------------------------------------------
		public void Update()
		{
			if(_nPins == 0)
			{
				return;
			}

			apRenderPin curPin = null;
			apRenderPin prevPin = null;
			apRenderPin nextPin = null;

			//1. 양쪽의 점이 연결된 점들을 대상으로 World Matrix + Control Point를 계산한다. (아무것도 없다면 0도로 바로 계산)
			//2. 한쪽만 점이 연결된 점들은 이웃한 점의 위치가 아닌 Control Point를 기준으로 각도를 계산한다.

			for (int i = 0; i < _nPins; i++)
			{
				curPin = _pins[i];
				prevPin = curPin._prevRenderPin;
				nextPin = curPin._nextRenderPin;

				if(prevPin == null && nextPin == null)
				{
					//둘다 없다면 0도
					curPin.SetAngleByCurve(0.0f);
				}
				else if(prevPin != null && nextPin == null)
				{
					//Prev만 있다면 : 이번 루틴에서는 한쪽 핀만 존재하는 경우는 제외합니다.
					continue;
				}
				else if(prevPin == null && nextPin != null)
				{
					//Next만 있다면 : 이번 루틴에서는 한쪽 핀만 존재하는 경우는 제외합니다.
					continue;
				}
				else
				{
					//Prev, Next 둘다 있다면
					//두개의 각도의 평균을 낸다.
					Vector2 dir2Next = Vector2.zero;
					dir2Next = nextPin._pos_World - prevPin._pos_World;

					float angleToNext = 0.0f;
					if(dir2Next.sqrMagnitude > 0.0f)
					{
						angleToNext = Mathf.Atan2(dir2Next.y, dir2Next.x) * Mathf.Rad2Deg;
					}

					curPin.SetAngleByCurve(apUtil.AngleTo180(angleToNext));
				}

				//사이드 컨트롤 포인트를 업데이트 한다.
				curPin.CalculateControlPoints();
			}

			//2. 한쪽에 핀이 있는 경우만 계산
			//- 연결된 핀이 "두개의 핀이 연결된 핀"이라면 : Prev의 Next Control Point 또는 Next의 Prev Control Point를 대상으로 벡터를 긋고 각도 계산
			//- 연결된 핀이 "한개의 핀만 연결된 핀"이라면 : 
			for (int i = 0; i < _nPins; i++)
			{
				curPin = _pins[i];
				prevPin = curPin._prevRenderPin;
				nextPin = curPin._nextRenderPin;

				if((prevPin == null && nextPin == null)
					|| (prevPin != null && nextPin != null))
				{
					//이번 루틴에서는 둘다 없거나 둘다 연결되어 있다면 패스
					continue;
				}
				
				if(prevPin != null)
				{
					Vector2 dir2Prev = Vector2.zero;
					
					//Prev만 있다면
					//Prev의 상태에 따라서 컨트롤 포인트를 기준으로 각도를 설정할지, 핀 위치를 기준으로 각도를 설정할지 정한다.
					if(prevPin._prevRenderPin != null && prevPin._nextRenderPin != null)
					{
						// 이 Pin은 양쪽이 연결된 상태다 = 루틴 1에서 Control Point가 생성되었을 것
						dir2Prev = prevPin._controlPointPos_Next - curPin._pos_World;
					}
					else
					{
						// 이 Pin은 한쪽(현재 핀)만 연결되어 있다 = 루틴 1을 거치지 않아서 Control Point가 생성되지 않았다.
						dir2Prev = prevPin._pos_World - curPin._pos_World;
					}
					
					//Y는 Dir > Prev의 +90
					float angleToPrev = 0.0f;
					if(dir2Prev.sqrMagnitude > 0.0f)
					{
						angleToPrev = Mathf.Atan2(dir2Prev.y, dir2Prev.x) * Mathf.Rad2Deg;
					}
					angleToPrev += 180.0f;
					curPin.SetAngleByCurve(apUtil.AngleTo180(angleToPrev));
				}
				else//if(prevPin == null && nextPin != null)
				{
					//Next만 있다면

					Vector2 dir2Next = Vector2.zero;

					//Next 상태에 따라서 컨트롤 포인트를 기준으로 각도를 설정할지, 핀 위치를 기준으로 각도를 설정할지 정한다.
					if(nextPin._prevRenderPin != null && nextPin._nextRenderPin != null)
					{
						dir2Next = nextPin._controlPointPos_Prev - curPin._pos_World;
					}
					else
					{
						// 이 Pin은 한쪽(현재 핀)만 연결되어 있다 = 루틴 1을 거치지 않아서 Control Point가 생성되지 않았다.
						dir2Next = nextPin._pos_World - curPin._pos_World;
					}


					//Y는 Dir > Next의 -90
					
					float angleToNext = 0.0f;
					if(dir2Next.sqrMagnitude > 0.0f)
					{
						angleToNext = Mathf.Atan2(dir2Next.y, dir2Next.x) * Mathf.Rad2Deg;
					}
					curPin.SetAngleByCurve(apUtil.AngleTo180(angleToNext));
				}

				//사이드 컨트롤 포인트를 업데이트 한다.
				curPin.CalculateControlPoints();
			}
		}
	}
}