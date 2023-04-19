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
	/// 메시의 데이터 중 핀을 복사하고 붙여넣는 클래스
	/// </summary>
	public class apSnapShot_MeshPin : apSnapShotBase
	{
		// Sub Class
		//-----------------------------------------------------
		public class PinData
		{
			//위치, 연결 정보, 모서리 타입을 저장한다. (Pin은 연결 정보를 자신이 갖는다)
			public Vector2 _pos = Vector2.zero;
			public apMeshPin.TANGENT_TYPE _tangentType = apMeshPin.TANGENT_TYPE.Sharp;

			//연결 정보
			public PinData _prevPinData = null;
			public PinData _nextPinData = null;

			//Weight 정보
			public int _range = 200;
			public int _fade = 100;

			public PinData()
			{
				//연결 정보는 초기화 다시
				_prevPinData = null;
				_nextPinData = null;
			}

			public void SetSrcPin(apMeshPin srcPin)
			{
				_pos = srcPin._defaultPos;
				_tangentType = srcPin._tangentType;

				_range = srcPin._range;
				_fade = srcPin._fade;
			}
		}


		// Members
		//-----------------------------------------------------
		private int _nPinData = 0;
		private List<PinData> _pinData = null;

		//원본 메시의 OffsetPos를 저장해야한다.
		private Vector2 _srcMeshOffsetPos = Vector2.zero;


		// Init
		//-----------------------------------------------------
		public apSnapShot_MeshPin()
		{
			Clear();
		}

		public override void Clear()
		{
			if(_pinData == null)
			{
				_pinData = new List<PinData>();
			}
			_pinData.Clear();
			_nPinData = 0;

			_srcMeshOffsetPos = Vector2.zero;
		}

		// Functions
		//-----------------------------------------------------
		public bool IsPastable()
		{
			//저장된 값이 있어야 한다.
			if(_pinData == null || _nPinData == 0)
			{
				return false;
			}
			return _pinData.Count > 0;
		}


		public bool Copy(List<apMeshPin> selectedPins, apMesh srcMesh)
		{
			int nSelectedPins = selectedPins != null ? selectedPins.Count : 0;
			if(srcMesh == null
				|| nSelectedPins == 0)
			{
				return false;
			}

			Clear();

			//1. 연결 정보를 제외하고 데이터를 모두 변환한다. (매핑에도 넣자)
			//2. 매핑을 참고하여 연결을 시키자
			Dictionary<apMeshPin, PinData> src2DstPins = new Dictionary<apMeshPin, PinData>();

			apMeshPin curSrcPin = null;

			for (int iSrcPin = 0; iSrcPin < nSelectedPins; iSrcPin++)
			{
				curSrcPin = selectedPins[iSrcPin];
				if(src2DstPins.ContainsKey(curSrcPin))
				{
					continue;
				}

				PinData newPinData = new PinData();
				newPinData.SetSrcPin(curSrcPin);

				_pinData.Add(newPinData);

				src2DstPins.Add(curSrcPin, newPinData);//매핑에도 추가
			}

			_nPinData = _pinData.Count;


			//2. 매핑을 참고하여 연결을 시키자
			foreach (KeyValuePair<apMeshPin, PinData> pinPair in src2DstPins)
			{
				apMeshPin srcPin = pinPair.Key;
				PinData dstData = pinPair.Value;

				//연결 정보가 있는지 확인하자
				//Prev
				if(srcPin._prevPin != null)
				{
					if(src2DstPins.ContainsKey(srcPin._prevPin))
					{
						PinData prevPinData = src2DstPins[srcPin._prevPin];
						if(prevPinData != null && prevPinData != dstData)
						{
							dstData._prevPinData = prevPinData;//데이터를 연결하자
						}
					}
				}

				//Next
				if(srcPin._nextPin != null)
				{
					if(src2DstPins.ContainsKey(srcPin._nextPin))
					{
						PinData nextPinData = src2DstPins[srcPin._nextPin];
						if(nextPinData != null && nextPinData != dstData)
						{
							dstData._nextPinData = nextPinData;//데이터를 연결하자
						}
					}
				}
			}

			//3. 피벗 위치 저장
			_srcMeshOffsetPos = srcMesh._offsetPos;

			return true;
		}


		//붙여넣기
		public List<apMeshPin> Paste(apMesh targetMesh, apDialog_CopyMeshVertPin.POSITION_SPACE posSpace)
		{
			_nPinData = _pinData != null ? _pinData.Count : 0;

			if(_nPinData == 0
				|| _pinData == null
				|| targetMesh == null)
			{
				return null;
			}

			


			//붙여넣자

			//일단 해단 메시에 PinGroup이 없다면 생성
			if(targetMesh._pinGroup == null)
			{
				targetMesh._pinGroup = new apMeshPinGroup();
				targetMesh._pinGroup._parentMesh = targetMesh;

			}

			
			List<apMeshPin> newPins = new List<apMeshPin>();
			Dictionary<PinData, apMeshPin> data2CopiedPin = new Dictionary<PinData, apMeshPin>();
			Dictionary<apMeshPin, PinData> copied2PinData = new Dictionary<apMeshPin, PinData>();



			//피벗에 대한 오프셋
			Vector2 deltaPivot = targetMesh._offsetPos - _srcMeshOffsetPos;

			//일단 생성을 하고
			PinData curPinData = null;
			for (int i = 0; i < _nPinData; i++)
			{
				curPinData = _pinData[i];

				int nextUniqueID = targetMesh._portrait.MakeUniqueID(apIDManager.TARGET.MeshPin);
				if(nextUniqueID >= 0)
				{
					Vector2 posW = curPinData._pos;

					//옵션에 따라 Pivot을 고려하여 붙여넣어야 할 수 있다.
					if(posSpace == apDialog_CopyMeshVertPin.POSITION_SPACE.RelativeToPivot)
					{
						posW += deltaPivot;
					}

					apMeshPin newPin = targetMesh._pinGroup.AddMeshPin(nextUniqueID, posW, null, curPinData._range, curPinData._fade);
					newPin._tangentType = curPinData._tangentType;

					if(newPin != null)
					{
						//매핑
						data2CopiedPin.Add(curPinData, newPin);
						copied2PinData.Add(newPin, curPinData);

						//리턴 리스트에 추가
						newPins.Add(newPin);
					}
				}
			}

			//연결을 하자
			int nNewPins = newPins.Count;
			apMeshPin curPin = null;
			for (int i = 0; i < nNewPins; i++)
			{
				curPin = newPins[i];
				PinData pinData = null;

				if(curPin._prevPin != null
					&& curPin._nextPin != null)
				{
					//양쪽다 이미 연결되었다.
					continue;
				}

				if(!copied2PinData.TryGetValue(curPin, out pinData))
				{
					continue;
				}
				if(pinData == null)
				{
					continue;
				}

				//이전/이후 연결 정보 체크
				//연결은 Prev > Next로 붙인다.

				//Prev가 아직 연결 안된 경우
				if(curPin._prevPin == null && pinData._prevPinData != null)
				{
					apMeshPin prevPin = null;
					data2CopiedPin.TryGetValue(pinData._prevPinData, out prevPin);

					if(prevPin != null
						&& prevPin._nextPin == null)
					{
						//Prev에서 현재 Pin으로 연결 가능할 때
						prevPin.LinkPinAsNext(curPin);
					}
				}

				//Next가 아직 연결 안된 경우
				if(curPin._nextPin == null && pinData._nextPinData != null)
				{
					apMeshPin nextPin = null;
					data2CopiedPin.TryGetValue(pinData._nextPinData, out nextPin);

					if(nextPin != null
						&& nextPin._prevPin == null)
					{
						//Cur에서 Next로 연결 가능할 때
						curPin.LinkPinAsNext(nextPin);
					}
				}
			}

			//마무리로 커브 업데이트
			targetMesh._pinGroup.Default_UpdateCurves();

			return newPins;
		}

	}
}