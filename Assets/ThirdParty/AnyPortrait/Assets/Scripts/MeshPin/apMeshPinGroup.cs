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
	/// v1.4.0 : 메시의 Pin들의 그룹. Pin들의 처리를 관리한다.
	/// </summary>
	[Serializable]
	public class apMeshPinGroup
	{
		// Members
		//-------------------------------------------
		//핀들
		[SerializeField]
		public List<apMeshPin> _pins_All = new List<apMeshPin>();

		//Pin 그룹에 해당하는 라인들
		//[NonSerialized]
		//public List<apMeshPinLine> _lines_All = new List<apMeshPinLine>();
		//[NonSerialized]
		//public Dictionary<apMeshPin, apMeshPinLine> _pin2Line = new Dictionary<apMeshPin, apMeshPinLine>();

		//핀과 핀 사이의 커브들
		[NonSerialized]
		public List<apMeshPinCurve> _curves_All = new List<apMeshPinCurve>();

		//연결
		[NonSerialized]
		public apMesh _parentMesh;

		//버텍스와의 연결. Vertex Data 리스트와 동기화되어야 한다.
		//만약 핀이 없다면, 이 리스트는 삭제한다.
		[SerializeField]
		public List<apMeshPinVertWeight> _vertWeights = new List<apMeshPinVertWeight>();

		
		//Pin, Curve 개수
		public int NumPins { get { return _pins_All != null ? _pins_All.Count : 0; } }
		public int NumCurves { get { return _curves_All != null ? _curves_All.Count : 0; } }

		// Init
		//-------------------------------------------
		public apMeshPinGroup()
		{

		}

		

		//초기화한다.
		public void Clear()
		{
			if(_pins_All == null) { _pins_All = new List<apMeshPin>(); }
			_pins_All.Clear();

			//if (_lines_All == null) { _lines_All = new List<apMeshPinLine>(); }
			//if(_pin2Line == null) { _pin2Line = new Dictionary<apMeshPin, apMeshPinLine>(); }
			//_lines_All.Clear();
			//_pin2Line.Clear();

			if(_curves_All == null) { _curves_All = new List<apMeshPinCurve>(); }
			_curves_All.Clear();

			if(_vertWeights == null) { _vertWeights = new List<apMeshPinVertWeight>(); }
			_vertWeights.Clear();
		}

		// Link
		//-------------------------------------------
		public void Link(apPortrait portrait, apMesh parentMesh)
		{
			if(_pins_All == null)
			{
				_pins_All = new List<apMeshPin>();
			}
			
			_parentMesh = parentMesh;


			//Pin들 간의 연결을 갱신한다.
			Refresh(REFRESH_TYPE.LinkAll);
		}


		// Refresh : 변경 사항이 있다면 무조건 Refresh를 하자.
		//-------------------------------------------
		public enum REFRESH_TYPE
		{
			/// <summary>
			/// 시작 단계인 "핀들을 서로 연결하기"부터 완전히 갱신한다. 기존의 Weight가 있다면 그대로 활용한다.
			/// 단순한 Link 과정에서의 초기화다.
			/// </summary>
			LinkAll,

			/// <summary>
			/// LinkAll보다 더 처음부터 초기화한다. VertWeight의 기존 데이터를 삭제하고 다시 계산한다.
			/// (핀을 추가/삭제한 경우, 직접 재계산을 하는 경우, 옵션에 따라 Pin의 위치가 바뀌었거나 버텍스가 변경된 경우)
			/// </summary>
			RecalculateAll,

			/// <summary>
			/// Default 단계에서 Vertex Weight를 갱신하지 않는 상태에서 Pin, Curve만 갱신하는 경우. 초기화는 끝난 상태여야 한다.
			/// (Default 단계에서 Pin을 움직인 상태에서 Weight를 수정하지 않는 경우)
			/// </summary>
			Update_Default,

			/// <summary>
			/// Test 단계에서 업데이트를 한다.
			/// </summary>
			Update_Test,

			//-----------------------------------------------------------------
			// 여기서부터는 메시 그룹의 동작이므로 순차적으로 동작하지 않는다.
			//-----------------------------------------------------------------
			/// <summary>
			/// 모디파이어 처리 도중 다른 값과 보간되지 않은 상태의 커브의 형태를 갱신한다.
			/// </summary>
			Update_ModMid,

			///// <summary>
			///// 모디파이어의 처리 결과가 모두 포함되어 보간된 상태의 커브의 형태를 갱신한다.
			///// </summary>
			//Update_ModFinal,

		}

		/// <summary>
		/// 연결이나 좌표를 갱신한다. 요청 타입에 따라서 어느 단계부터 다시 갱신할지 판단한다.
		/// </summary>
		public void Refresh(REFRESH_TYPE refreshType)
		{
			switch (refreshType)
			{
				case REFRESH_TYPE.LinkAll:
					{
						//기존의 데이터를 가진 상태로 링크를 점검하고 커브를 생성하여 계산한다.

						Sub_LinkPins();//핀들간의 링크를 다시 체크한다.
						Sub_MakeCurves();//커브를 처음부터 다시 생성한다.

						Default_UpdateCurves();//Default 상태에서의 커브를 다시 생성한다.

						bool isNeedToResetVertWeight = Sub_LinkVertWeights();//커브를 바탕으로 VertWeight를 다시 만든다.
						if(isNeedToResetVertWeight)
						{
							Sub_ResetVertWeights();//VertWeight를 아예 새로 계산한다.
						}

						//테스트 커브도 새로 생성한다.
						Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE.MeshTest);
					}
					break;

				case REFRESH_TYPE.RecalculateAll:
					{
						//기존의 데이터(Weight)를 삭제하고 처음부터 다시 계산한다.

						Sub_LinkPins();//핀들간의 링크를 다시 체크한다.
						Sub_MakeCurves();//커브를 처음부터 다시 생성한다.

						Default_UpdateCurves();//Default 상태에서의 커브를 다시 생성한다.

						Sub_ResetVertWeights();//VertWeight를 아예 새로 계산한다.

						//테스트 커브도 새로 생성한다.
						Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE.MeshTest);
					}
					break;

				case REFRESH_TYPE.Update_Default:
					{
						//초기 단계가 지난 상태에서 커브만 업데이트 하고자 할 때
						Default_UpdateCurves();
						//Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE.MeshTest);
					}
					break;

				case REFRESH_TYPE.Update_Test:
					{
						Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE.MeshTest);
					}
					break;

				case REFRESH_TYPE.Update_ModMid:
					{
						Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE.ModMid);
					}
					break;

				//case REFRESH_TYPE.Update_ModFinal:
				//	{
				//		Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE.ModFinal);
				//	}
				//	break;
			}
		}




		/// <summary>
		/// 핀들의 상호간 Link를 갱신한다.
		/// Line들도 다시 만든다.
		/// </summary>
		private void Sub_LinkPins()
		{
			int nPins = NumPins;
			if(nPins == 0)
			{
				return;
			}

			//Pin들을 하나씩 돌면서 연결하자
			apMeshPin curPin = null;
			for (int i = 0; i < nPins; i++)
			{
				curPin = _pins_All[i];

				//저장된 ID를 이용하여 연결하자
				//1. Prev 연결
				if(curPin._prevPin != null && curPin._prevPinID != curPin._prevPin._uniqueID)
				{
					//유효하지 않은 연결
					curPin._prevPin = null;
					curPin._prevPinID = -1;
				}

				if(curPin._prevPin == null && curPin._prevPinID >= 0)
				{
					curPin._prevPin = GetPin(curPin._prevPinID);
				}
				else if(curPin._prevPinID < 0)
				{
					curPin._prevPin = null;
				}
				
				//찾지 못했다면
				if(curPin._prevPin == null)
				{
					curPin._prevPinID = -1;
				}

				//2. Next 연결
				if(curPin._nextPin != null && curPin._nextPinID != curPin._nextPin._uniqueID)
				{
					//유효하지 않은 연결
					curPin._nextPin = null;
					curPin._nextPinID = -1;
				}
				

				if(curPin._nextPin == null && curPin._nextPinID >= 0)
				{
					curPin._nextPin = GetPin(curPin._nextPinID);
				}
				else if(curPin._nextPinID < 0)
				{
					curPin._nextPin = null;
				}

				//찾지 못했다면
				if(curPin._nextPin == null)
				{
					curPin._nextPinID = -1;
				}
			}
		}



		// Sub 갱신 함수들
		//--------------------------------------------------

		/// <summary>
		/// 모든 버텍스 가중치를 링크한다. (새로 계산하진 않는다.)
		/// 개수가 맞지 않거나 순서가 다르다면 리스트를 완전히 리셋한다. (그 경우 true 리턴)
		/// 추가로, 커브 연결 여부와 실제 커브 존재 여부가 달라도 true 리턴
		/// </summary>
		private bool Sub_LinkVertWeights()
		{
			if(_vertWeights == null)
			{
				_vertWeights = new List<apMeshPinVertWeight>();
			}

			if(NumPins == 0)
			{
				//핀이 없다면 버텍스 가중치가 필요없다.
				_vertWeights.Clear();
				return false;
			}

			//Pin들을 하나씩 돌면서 연결하자
			int nSrcVerts = _parentMesh._vertexData != null ? _parentMesh._vertexData.Count : 0;
			if(nSrcVerts == 0)
			{
				_vertWeights.Clear();
				return false;
			}

			int nVertWeights = _vertWeights.Count;

			bool isNeedSync = false;
			
			if(nVertWeights != nSrcVerts)
			{
				//1. 길이가 다르면 리셋
				isNeedSync = true;
			}
			else
			{
				//2. 길이가 같다면 하나씩 확인하자 (바로 링크도 하면서)
				apMeshPinVertWeight curVertWeight = null;
				apVertex curSrcVert = null;
				for (int iVert = 0; iVert < nVertWeights; iVert++)
				{
					curSrcVert = _parentMesh._vertexData[iVert];
					curVertWeight = _vertWeights[iVert];

					if(curSrcVert._uniqueID != curVertWeight._vertID)
					{
						//ID가 맞지 않는다.
						isNeedSync = true;
						break;
					}

					//링크하자
					if(curVertWeight.Link(curSrcVert, this))
					{
						//싱크가 필요하다. [v1.4.1]
						isNeedSync = true;
						break;
					}
				}
			}

			if(isNeedSync)
			{
				//처음부터 다시 연결하자
				//CalculateVertWeightAll();

				return true;
			}

			return false;
		}


		// Functions
		//---------------------------------------------
		
		
		
		//---------------------------------------------------------
		// 핀 추가 / 삭제
		//---------------------------------------------------------
		/// <summary>
		/// 핀을 추가한다. 핀 추가 즉시 전체적으로 다시 계산한다.
		/// </summary>
		public apMeshPin AddMeshPin(int uniqueID, Vector2 posW, apMeshPin prevPin, int initRange, int initFade)
		{
			if(_pins_All == null)
			{
				_pins_All = new List<apMeshPin>();
			}


			
			apMeshPin newPin = new apMeshPin();
			newPin.Init(uniqueID, posW, this, initRange, initFade);

			_pins_All.Add(newPin);

			if(prevPin == null)
			{
				//이전에 선택된게 없다면
				//apMeshPinLine newLine = new apMeshPinLine(newPin);
				
			}
			else
			{
				//이전에 선택된게 있다면
				
				if(prevPin._nextPin == null)
				{
					//1. Prev의 Next가 비어있다.
					//- Prev의 Next로서 연결
					prevPin.LinkPinAsNext(newPin);//Prev > New 순서
				}
				else if(prevPin._prevPin == null)
				{
					//2. Prev의 Prev가 비어있다.
					//- Prev의 Prev로서 연결
					//- Start에는 새로운 Pin이 들어가고, PrevPin은 제거한다.
					newPin.LinkPinAsNext(prevPin);//New > Prev 순서
				}
				else
				{
					//3. Prev가 꽉 차있다.
					//- 연결 불가
					//_pins_Start.Add(newPin);
				}
			}

			//전체 다시 계산
			Refresh(REFRESH_TYPE.RecalculateAll);

			return newPin;
		}


		/// <summary>
		/// 핀을 삭제한다. 전체적으로 다시 계산한다.
		/// </summary>
		/// <param name="pin"></param>
		public void RemovePin(apMeshPin pin)
		{
			if(pin == null)
			{
				return;
			}

			if(_pins_All == null)
			{
				_pins_All = new List<apMeshPin>();
			}


			//연결을 끊는다.
			apMeshPin nextPin = pin._nextPin;
			apMeshPin prevPin = pin._prevPin;
			if(nextPin != null)
			{
				nextPin._prevPin = null;
				nextPin._prevPinID = -1;
				nextPin._prevCurve = null;
			}
			if(prevPin != null)
			{
				prevPin._nextPin = null;
				prevPin._nextPinID = -1;
				prevPin._nextCurve = null;
			}

			pin._prevCurve = null;
			pin._nextCurve = null;

			//리스트에서 삭제한다.
			_pins_All.Remove(pin);
			
			//전체 다시 계산
			Refresh(REFRESH_TYPE.RecalculateAll);

			//CheckAndMakeCurves();
			//Default_UpdateCurves();
		}


		//---------------------------------------------------------
		// 핀 연결 함수
		//---------------------------------------------------------
		/// <summary>
		/// 두개의 핀이 연결 가능한가 (v1.4.1)
		/// </summary>
		/// <param name="pinFrom"></param>
		/// <param name="pinTo"></param>
		/// <returns></returns>
		public bool IsPinLinkable(apMeshPin pinFrom, apMeshPin pinTo)
		{
			//서로 같다면 불가
			if(pinFrom == null || pinTo == null) { return false; }
			if(pinFrom == pinTo) { return false; }

			//서로 Prev, Next가 비어있지 않다면 실패
			if((pinFrom._prevPin != null && pinFrom._nextPin != null)
				|| (pinTo._prevPin != null && pinTo._nextPin != null))
			{
				return false;
			}

			//서로 이미 연결되었다면 실패
			if(pinFrom._nextPin == pinTo || pinFrom._prevPin == pinTo
				|| pinTo._nextPin == pinFrom || pinTo._prevPin == pinFrom)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 요청한 핀 두개를 연결한다.
		/// 서로간에 연결이 비어있어야 한다.
		/// From의 조건을 보고, To는 거기에 맞게 연결을 설정한다.
		/// 연결 가능시 True 리턴. Refresh가 포함된다.
		/// </summary>
		public bool LinkPins(apMeshPin pinFrom, apMeshPin pinTo)
		{
			//서로 같다면 실패
			if(pinFrom == pinTo)
			{
				return false;
			}
			
			//서로 Prev, Next가 비어있지 않다면 실패
			if((pinFrom._prevPin != null && pinFrom._nextPin != null)
				|| (pinTo._prevPin != null && pinTo._nextPin != null))
			{
				//연결 불가
				return false;
			}

			//서로 이미 연결되었다면 실패
			if(pinFrom._nextPin == pinTo || pinFrom._prevPin == pinTo
				|| pinTo._nextPin == pinFrom || pinTo._prevPin == pinFrom)
			{
				return false;
			}
			
			//가장 우선시 되는 연결
			//Next > Prev간 비어있는지 확인
			//1. From > To
			if(pinFrom._nextPin == null && pinTo._prevPin == null)
			{
				//From > To 순서로 연결하자
				pinFrom._nextPin = pinTo;
				pinFrom._nextPinID = pinTo._uniqueID;

				pinTo._prevPin = pinFrom;
				pinTo._prevPinID = pinFrom._uniqueID;

				//연결 다시 체크 후 리턴
				Refresh(REFRESH_TYPE.RecalculateAll);

				return true;
			}
			//2. To > From
			if(pinTo._nextPin == null && pinFrom._prevPin == null)
			{
				//To > From 순서로 연결하자
				pinTo._nextPin = pinFrom;
				pinTo._nextPinID = pinFrom._uniqueID;

				pinFrom._prevPin = pinTo;
				pinFrom._prevPinID = pinTo._uniqueID;

				//연결 다시 체크 후 리턴
				Refresh(REFRESH_TYPE.RecalculateAll);

				return true;
			}

			//Prev > Next간 연결이 안되는 경우
			//Next, Next끼리 비어있을 때, Prev, Prev끼리 비어있다면 하나를 완전히 Turn한다.
			//단, 이 경우에 서로 루프로 연결된게 아니어야 한다.

			//일단 서로 직간접적으로 연결된 것인지 체크한다.
			bool isConnectedAlready = IsPinInConnection(pinFrom, pinTo);
			if(isConnectedAlready)
			{
				//이미 직간접적으로 연결되었다.
				return false;
			}

			//연결 하나를 완전히 Reverse하자
			Reverse(pinFrom);

			if(pinFrom._nextPin == null && pinTo._prevPin == null)
			{
				//From > To 순서로 연결하자
				pinFrom._nextPin = pinTo;
				pinFrom._nextPinID = pinTo._uniqueID;

				pinTo._prevPin = pinFrom;
				pinTo._prevPinID = pinFrom._uniqueID;

				//전체 갱신
				Refresh(REFRESH_TYPE.RecalculateAll);
				return true;
			}
			if(pinTo._nextPin == null && pinFrom._prevPin == null)
			{
				//To > From 순서로 연결하자
				pinTo._nextPin = pinFrom;
				pinTo._nextPinID = pinFrom._uniqueID;

				pinFrom._prevPin = pinTo;
				pinFrom._prevPinID = pinTo._uniqueID;

				//전체 갱신
				Refresh(REFRESH_TYPE.RecalculateAll);
				return true;
			}


			//실패했지만 연결 유효성 테스트하고 커브 생성해야한다 (중간에 변경 내역이 있었다.)
			Refresh(REFRESH_TYPE.RecalculateAll);


			return false;
		}

		/// <summary>
		/// 타겟 핀이 다른 핀의 연결 중에 포함되어 있는지 체크한다. True인 경우, 이 핀들은 직-간접적으로 연결된 셈이다.
		/// </summary>
		/// <param name="targetPin"></param>
		/// <param name="pinOfConnection"></param>
		/// <returns></returns>
		private bool IsPinInConnection(apMeshPin targetPin, apMeshPin pinOfConnection)
		{
			if(targetPin == pinOfConnection)
			{
				return true;
			}

			if(targetPin == null || pinOfConnection == null)
			{
				return false;
			}

			apMeshPin curPin = null;
			List<apMeshPin> checkedPins = new List<apMeshPin>();
			
			checkedPins.Add(pinOfConnection);
			
			//Prev, Next 방향으로 각각 순회하며 리스트에 저장한다.
			
			//1. Prev 방향으로 순회
			if(pinOfConnection._prevPin != null)
			{
				curPin = pinOfConnection._prevPin;
				while(true)
				{
					if(curPin == targetPin)
					{
						//타겟이 루프 내에 포함되어 있다.
						return true;
					}

					//루프 종료 조건들
					if(curPin._prevPin == null) { break; }
					if(curPin == pinOfConnection) { break; }
					if(checkedPins.Contains(curPin)) { break; }

					//다음 루프
					checkedPins.Add(curPin);
					curPin = curPin._prevPin;
				}
			}

			//2. Next 방향으로 순회
			if(pinOfConnection._nextPin != null)
			{
				curPin = pinOfConnection._nextPin;
				while(true)
				{
					if(curPin == targetPin)
					{
						//타겟이 루프 내에 포함되어 있다.
						return true;
					}

					//루프 종료 조건들
					if(curPin._nextPin == null) { break; }
					if(curPin == pinOfConnection) { break; }
					if(checkedPins.Contains(curPin)) { break; }

					//다음 루프
					checkedPins.Add(curPin);
					curPin = curPin._nextPin;
				}
			}

			return false;
		}

		/// <summary>
		/// 하나의 핀으로 부터 연결된 Prev-Next 순서들을 모두 뒤집는다.
		/// </summary>
		/// <param name="targetPin"></param>
		private void Reverse(apMeshPin targetPin)
		{
			//1. Start를 찾는다. (만약 루프라면, 
			//2. End방향으로 이동하면서 리스트에 저장한다.
			//3. 리스트를 돌면서 하나씩 순서를 거꾸로 만든다. (Curve 포함)
			apMeshPin startPin = targetPin;
			
			
			if(startPin._prevPin != null)
			{
				startPin = startPin._prevPin;
				while(true)
				{
					//계속 찾자
					if(startPin._prevPin == null)
					{
						//끝. 여기거 Start 핀이다.
						break;
					}
					if(startPin == targetPin)
					{
						//루프다 (더 찾을 수가 없다)
						break;
					}

					//이전으로 하나 더 이동
					startPin = startPin._prevPin;
				}
			}

			List<apMeshPin> connectedPins = new List<apMeshPin>();

			apMeshPin curPin = startPin;

			//Start부터 End까지의 핀들을 리스트에 정리한다.
			while(true)
			{
				if(connectedPins.Contains(curPin))
				{
					//리스트에 이미 있다면 문제가 있다. 종료
					break;
				}

				//리스트에 넣고
				connectedPins.Add(curPin);

				if(curPin._nextPin == null
					|| curPin._nextPin == startPin
					|| curPin._nextPin == curPin)
				{
					//순회 종료
					break;
				}

				//넥스트로 커서 이동
				curPin = curPin._nextPin;
			}


			//여기의 모든 핀들을 순서대로 돌면서 Next와 관계를 바꾼다.
			//커브는 생략한다. 꼭 리프레시할 것
			int nConnPins = connectedPins.Count;

			apMeshPin nextPin = null;
			apMeshPin prevPin = null;
			for (int i = 0; i < nConnPins; i++)
			{
				curPin = connectedPins[i];
				nextPin = curPin._nextPin;
				prevPin = curPin._prevPin;
				
				//Next, Prev의 순서를 뒤집는다.
				curPin._nextPin = prevPin;
				curPin._nextPinID = prevPin != null ? prevPin._uniqueID : -1;

				curPin._prevPin = nextPin;
				curPin._prevPinID = nextPin != null ? nextPin._uniqueID : -1;
			}
			
			
		}




		//----------------------------------------------------------
		// 핀 연결 유효성 체크 및 커브 생성 함수
		//----------------------------------------------------------
		

		/// <summary>
		/// Start 목록/Curve 리스트를 다시 갱신한다.
		/// Start Pin들은 Prev가 없는 모든 핀이다.
		/// </summary>
		private void Sub_MakeCurves()
		{
			//if (_lines_All == null) { _lines_All = new List<apMeshPinLine>(); }
			//if(_pin2Line == null) { _pin2Line = new Dictionary<apMeshPin, apMeshPinLine>(); }
			//_lines_All.Clear();
			//_pin2Line.Clear();

			//커브들도 초기화
			_curves_All.Clear();

			//연결이 유효한지 테스트한다. (유효하지 않은 모든 경우를 해제한다.)
			//Prev가 없는 "시작 Pin"들을 별도로 리스트에 넣는다.

			apMeshPin curPin = null;
			apMeshPin prevPin = null;
			apMeshPin nextPin = null;

			int nPins = NumPins;

			for (int i = 0; i < nPins; i++)
			{
				curPin = _pins_All[i];

				//Default Matrix를 먼저 게산한다.
				curPin.RefreshDefaultMatrix();

				prevPin = curPin._prevPin;
				nextPin = curPin._nextPin;

				if(prevPin != null)
				{	
					if(prevPin._nextPin != curPin
						|| prevPin == curPin)
					{
						//유효하지 않은 연결인 경우
						//- 이전핀의 Next가 자기 자신이 아니라면
						//- 이전 핀이 자기 자신인 경우
						//서로 연결을 끊자
						curPin._prevPin = null;
						curPin._prevPinID = -1;

						prevPin._nextPin = null;
						prevPin._nextPinID = -1;
					}
				}

				if(nextPin != null)
				{
					if(nextPin._prevPin != curPin
						|| nextPin == curPin)
					{
						//유효하지 않은 연결인 경우
						//- 다음 핀의 Prev가 자기 자신이 아니라면
						//- 다음 핀이 자기 자신이라면
						//서로 연결을 끊자
						curPin._nextPin = null;
						curPin._nextPinID = -1;

						nextPin._prevPin = null;
						nextPin._prevPinID = -1;
					}
				}

				//커브 체크
				//- 기존에 생성된 커브가 있다면 유효성 체크 후 재사용.
				//- 없다면 Prev의 Next 커브가 자신을 향하는지 체크후 재사용
				//- 그렇지 않다면 양쪽에서 해당 커브를 모두 없애고 새로 생성
				//- Next도 마찬가지

				//삭제는 Prev, Next 양쪽에서 실행
				//유효성 체크및 연결은 Next로만 실행
				
				// 삭제 체크
				//1. Pre <- PrevCurve -> Cur 체크
				if(curPin._prevPin == null)
				{
					//Prev가 없다면
					curPin._prevCurve = null;//Prev 커브 삭제
				}
				if(curPin._nextPin == null)
				{
					//Next가 없다면
					curPin._nextCurve = null;//Next 커브 삭제
				}

				//추가는 Cur > Next로만 체크
				//Cur -> NextCurve -> NextPin
				if (curPin._nextPin != null)
				{
					if (curPin._nextCurve != null)
					{
						//이미 NextCurve가 있다면 연결 갱신 후 리스트에 넣자
						curPin._nextCurve._prevPin = curPin;            //커브의 Prev는 Cur (본인)
						curPin._nextCurve._nextPin = curPin._nextPin;   //커브의 Next는 Next Pin
						curPin._nextPin._prevCurve = curPin._nextCurve; //Next Pin의 Prev Curve도 이 커브여야 한다.

						_curves_All.Add(curPin._nextCurve);
					}
					else
					{
						//없다면 추가
						apMeshPinCurve newCurve = new apMeshPinCurve(curPin, curPin._nextPin);
						curPin._nextCurve = newCurve;
						curPin._nextPin._prevCurve = newCurve;

						_curves_All.Add(newCurve);
					}
				}
			}
		}





		/// <summary>
		/// 핀들의 위치를 바탕으로 컨트롤 포인트와 곡선을 계산한다.
		/// ValidateAndMakeCurves 함수 이후에 호출할 것
		/// </summary>
		public void Default_UpdateCurves()
		{
			//1. 모든 점들을 돌면서, Normal (+Y)를 계산하자 (이웃한 점 이용)
			if(NumPins == 0)
			{
				return;
			}

			//1. 양쪽의 점이 연결된 점들을 대상으로 World Matrix + Control Point를 계산한다. (아무것도 없다면 0도로 바로 계산)
			//2. 한쪽만 점이 연결된 점들은 이웃한 점의 위치가 아닌 Control Point를 기준으로 각도를 계산한다.

			int nPins = NumPins;

			apMeshPin curPin = null;
			apMeshPin prevPin = null;
			apMeshPin nextPin = null;
			
			//1. 양쪽에 핀이 있거나 아예 없는 경우만 계산
			for (int i = 0; i < nPins; i++)
			{
				curPin = _pins_All[i];
				prevPin = curPin._prevPin;
				nextPin = curPin._nextPin;

				
				if(prevPin == null && nextPin == null)
				{
					//둘다 없다면 0도
					curPin._defaultAngle = 0.0f;
				}
				else if(prevPin != null && nextPin == null)
				{
					//Prev만 있다면
					//이번 루틴에서는 패스
					continue;
				}
				else if(prevPin == null && nextPin != null)
				{
					//Next만 있다면
					//이번 루틴에서는 패스
					continue;
				}
				else
				{
					//Prev, Next 둘다 있다면
					//두개의 각도의 평균을 낸다.
					//거리의 영향을 없애기 위해 Normalized 거리를 잰다.
					Vector2 normalizedNextDefPos = ((nextPin._defaultPos - curPin._defaultPos).normalized * 10.0f) + curPin._defaultPos;
					Vector2 normalizedPrevDefPos = ((prevPin._defaultPos - curPin._defaultPos).normalized * 10.0f) + curPin._defaultPos;

					//Vector2 dir2Next = nextPin._defaultPos - prevPin._defaultPos;
					Vector2 dir2Next = normalizedNextDefPos - normalizedPrevDefPos;
					float angleToNext = 0.0f;
					if(dir2Next.sqrMagnitude > 0.0f)
					{
						angleToNext = Mathf.Atan2(dir2Next.y, dir2Next.x) * Mathf.Rad2Deg;
					}

					curPin._defaultAngle = apUtil.AngleTo180(angleToNext);

					//Debug.Log("[" + i + "] 둘다 존재 | Prev 2 Next Dir (" + dir2Next + " / " + angleToNext + ") >> 결과 : " + curPin._defaultAngle);
				}

				curPin.RefreshDefaultMatrix();
				curPin.RefreshDefaultControlPoints();
			}


			//2. 한쪽에 핀이 있는 경우만 계산
			//- 연결된 핀이 "두개의 핀이 연결된 핀"이라면 : Prev의 Next Control Point 또는 Next의 Prev Control Point를 대상으로 벡터를 긋고 각도 계산
			//- 연결된 핀이 "한개의 핀만 연결된 핀"이라면 : 
			for (int i = 0; i < nPins; i++)
			{
				curPin = _pins_All[i];
				prevPin = curPin._prevPin;
				nextPin = curPin._nextPin;

				if((prevPin == null && nextPin == null)
					|| (prevPin != null && nextPin != null))
				{
					//이번 루틴에서는 둘다 없거나 둘다 연결되어 있다면 패스
					continue;
				}

				if(prevPin != null)
				{
					//Prev만 있다면
					//Prev의 상태에 따라서 컨트롤 포인트를 기준으로 각도를 설정할지, 핀 위치를 기준으로 각도를 설정할지 정한다.
					Vector2 dir2Prev = Vector2.zero;
					if (prevPin._prevPin != null && prevPin._nextPin != null)
					{
						// 이 Pin은 양쪽이 연결된 상태다 = 루틴 1에서 Control Point가 생성되었을 것
						dir2Prev = prevPin._controlPointPos_Def_Next - curPin._defaultPos;
					}
					else
					{
						// 이 Pin은 한쪽(현재 핀)만 연결되어 있다 = 루틴 1을 거치지 않아서 Control Point가 생성되지 않았다.
						dir2Prev = prevPin._defaultPos - curPin._defaultPos;
					}
					
					//Y는 Dir > Prev의 +90
					float angleToPrev = 0.0f;
					if(dir2Prev.sqrMagnitude > 0.0f)
					{
						angleToPrev = Mathf.Atan2(dir2Prev.y, dir2Prev.x) * Mathf.Rad2Deg;
					}
					angleToPrev += 180.0f;
					curPin._defaultAngle = apUtil.AngleTo180(angleToPrev);

					//Debug.Log("[" + i + "] Prev 존재 | Dir (" + dir2Prev + " / " + angleToPrev + ") >> 결과 : " + curPin._defaultAngle);
				}
				else//if(prevPin == null && nextPin != null)
				{
					//Next만 있다면
					//Y는 Dir > Next의 -90

					Vector2 dir2Next = Vector2.zero;

					//Next 상태에 따라서 컨트롤 포인트를 기준으로 각도를 설정할지, 핀 위치를 기준으로 각도를 설정할지 정한다.
					if (nextPin._prevPin != null && nextPin._nextPin != null)
					{
						// 이 Pin은 양쪽이 연결된 상태다 = 루틴 1에서 Control Point가 생성되었을 것
						dir2Next = nextPin._controlPointPos_Def_Prev - curPin._defaultPos;
					}
					else
					{
						dir2Next = nextPin._defaultPos - curPin._defaultPos;
					}
					

					float angleToNext = 0.0f;
					if(dir2Next.sqrMagnitude > 0.0f)
					{
						angleToNext = Mathf.Atan2(dir2Next.y, dir2Next.x) * Mathf.Rad2Deg;
					}
					curPin._defaultAngle = apUtil.AngleTo180(angleToNext);

					//Debug.Log("[" + i + "] Next 존재 | Dir (" + dir2Next + " / " + angleToNext + ") >> 결과 : " + curPin._defaultAngle);
				}

				curPin.RefreshDefaultMatrix();
				curPin.RefreshDefaultControlPoints();
			}

		}


		/// <summary>
		/// 모든 버텍스들을 대상으로 가중치를 새로 생성한다.
		/// 커브 데이터나 위치를 모두 갱신한 이후에 마지막에 호출해야한다.
		/// </summary>
		private void Sub_ResetVertWeights()
		{
			//가중치 리스트는 일단 리셋
			if(_vertWeights == null)
			{
				_vertWeights = new List<apMeshPinVertWeight>();
			}
			_vertWeights.Clear();


			if(NumPins == 0)
			{
				return;
			}
			

			List<apVertex> srcVerts = _parentMesh._vertexData;
			int nVert = srcVerts != null ? srcVerts.Count : 0;
			if(nVert == 0)
			{
				return;
			}

			//버텍스 순서대로 만든다.


			apMeshPin curPin;			
			apVertex curVert = null;

			int nPins = NumPins;

			for (int iVert = 0; iVert < nVert; iVert++)
			{
				curVert = srcVerts[iVert];

				//새로운 Vert > Pin Weight를 생성
				apMeshPinVertWeight newVertPinWeight = new apMeshPinVertWeight();
				newVertPinWeight.Init(curVert);
				_vertWeights.Add(newVertPinWeight);

				//핀들을 돌면서 검사하자
				for (int iPin = 0; iPin < nPins; iPin++)
				{
					curPin = _pins_All[iPin];

					if (curPin._prevCurve == null
						&& curPin._nextCurve == null)
					{
						//다른 Pin과 연결되어 있지 않다면 단일 가중치 계산
						float weight_Single = curPin.GetWeight_Default(curVert._pos);
						float distToPin = Vector2.Distance(curVert._pos, curPin._defaultPos);

						if (weight_Single > 0.0f)
						{
							newVertPinWeight.AddWeight_Single(curPin, weight_Single, distToPin);
						}
					}
					else if (curPin._nextCurve != null && curPin._nextPin != null)
					{
						//Next Pin과 연결되어 있다면,
						//Lerp를 적당히 옮겨가면서 가장 가까운 곳을 구하고,
						//그곳의 가중치를 적용한다.
						apMeshPin nextPin = curPin._nextPin;
						
						float nearestLerp = -1.0f;
						float nearestDist = -1.0f;

						//커브를 20등분 한다.
						int nSplit = 20;
						for (int iSplit = 0; iSplit <= nSplit; iSplit++)
						{
							float curLerp = Mathf.Clamp01((float)iSplit / (float)nSplit);
							Vector2 splitPos = curPin._nextCurve.GetCurvePos_Default(curLerp);
							float curDist = Vector2.Distance(splitPos, curVert._pos);
							if(nearestDist < 0.0f || curDist < nearestDist)
							{
								//가장 가까운 Lerp를 찾자
								nearestLerp = curLerp;
								nearestDist = curDist;
							}
						}

						//가장 가까운 위치에서의 Range 체크
						float range = ((float)curPin._range * (1.0f - nearestLerp)) + ((float)nextPin._range * nearestLerp);
						float fade = ((float)curPin._fade * (1.0f - nearestLerp)) + ((float)nextPin._fade * nearestLerp);

						float weight = -1.0f;
						if(nearestDist < range)
						{
							weight = 1.0f;
						}
						else if(fade > 0.0f && nearestDist < (range + fade))
						{
							weight = Mathf.Clamp01(1.0f - ((nearestDist - range) / fade));
						}

						//커브 내의 Weight를 입력하자
						if(weight > 0.0f)
						{
							apMatrix3x3 curveDefaultMatrix = curPin._nextCurve.GetCurveMatrix_Default(nearestLerp);
							newVertPinWeight.AddWeight_Curve(curPin,  weight, nearestDist, nearestLerp, ref curveDefaultMatrix);
						}

					}
				}

				//다 완료했다면 Pair 정보를 마무리
				newVertPinWeight.CompletePairs();


			}

			//Debug.Log("< Reset Vert Weights >");
		}


		// 테스트 모드용 함수
		//----------------------------------------------------
		/// <summary>
		/// [테스트 모드] 테스트 모드시 기본 위치와 Matrix를 초기화한다.
		/// </summary>
		public void Test_ResetMatrixAll(apMeshPin.TMP_VAR_TYPE tmpVarType)
		{

			if(NumPins == 0)
			{
				return;
			}
			int nPins = NumPins;

			//1. Pin들의 위치를 초기화한다.
			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = _pins_All[iPin];
				curPin.Tmp_ResetMatrix(tmpVarType);
			}

			//2. 커브들을 다시 계산한다. (각 종류에 따라
			if(tmpVarType == apMeshPin.TMP_VAR_TYPE.MeshTest)
			{
				//메시 테스트인 경우
				Refresh(REFRESH_TYPE.Update_Test);
			}
			//TODO : 다른 타입도 업데이트
			
		}


		/// <summary>
		/// 테스트 모드에서, Pin들의 위치를 비교하여 사이드 컨트롤 포인트를 계산한다.
		/// </summary>
		private void Tmp_UpdateCurves(apMeshPin.TMP_VAR_TYPE targetVarType)
		{	
			if(NumPins == 0)
			{
				return;
			}

			//1. 양쪽의 점이 연결된 점들을 대상으로 World Matrix + Control Point를 계산한다. (아무것도 없다면 0도로 바로 계산)
			//2. 한쪽만 점이 연결된 점들은 이웃한 점의 위치가 아닌 Control Point를 기준으로 각도를 계산한다.

			int nPins = NumPins;

			
			apMeshPin curPin = null;
			apMeshPin prevPin = null;
			apMeshPin nextPin = null;

			//1. 양쪽에 핀이 있거나 아예 없는 경우만 계산
			for (int i = 0; i < nPins; i++)
			{
				curPin = _pins_All[i];
				prevPin = curPin._prevPin;
				nextPin = curPin._nextPin;

				if(prevPin == null && nextPin == null)
				{
					//둘다 없다면 0도
					curPin.Tmp_CalculateWorldMatrix(targetVarType, 0.0f);
				}
				else if(prevPin != null && nextPin == null)
				{
					//Prev만 있다면
					//이번 루틴에서는 한쪽 핀만 존재하는 경우는 제외합니다.
					continue;
				}
				else if(prevPin == null && nextPin != null)
				{
					//Next만 있다면
					//이번 루틴에서는 한쪽 핀만 존재하는 경우는 제외합니다.
					continue;
				}
				else
				{
					//Prev, Next 둘다 있다면
					//두개의 각도의 평균을 낸다.
					//거리에 관계없이 내고자 하므로,
					//일정 거리로 통일을 해야한다.
					Vector2 dir2Next = Vector2.zero;
					Vector2 normalizedNextPos = Vector2.zero;
					Vector2 normalizedPrevPos = Vector2.zero;

					switch (targetVarType)
					{
						case apMeshPin.TMP_VAR_TYPE.MeshTest:
							normalizedNextPos = ((nextPin.TmpPos_MeshTest - curPin.TmpPos_MeshTest).normalized * 10.0f) + curPin.TmpPos_MeshTest;
							normalizedPrevPos = ((prevPin.TmpPos_MeshTest - curPin.TmpPos_MeshTest).normalized * 10.0f) + curPin.TmpPos_MeshTest;
							break;

						case apMeshPin.TMP_VAR_TYPE.ModMid:
							//dir2Next = nextPin.TmpPos_ModMid - prevPin.TmpPos_ModMid;
							normalizedNextPos = ((nextPin.TmpPos_ModMid - curPin.TmpPos_ModMid).normalized * 10.0f) + curPin.TmpPos_ModMid;
							normalizedPrevPos = ((prevPin.TmpPos_ModMid - curPin.TmpPos_ModMid).normalized * 10.0f) + curPin.TmpPos_ModMid;
							break;

						//case apMeshPin.TMP_VAR_TYPE.ModFinal:
						//	//dir2Next = nextPin.TmpPos_ModFinal - prevPin.TmpPos_ModFinal;
						//	normalizedNextPos = ((nextPin.TmpPos_ModFinal - curPin.TmpPos_ModFinal).normalized * 10.0f) + curPin.TmpPos_ModFinal;
						//	normalizedPrevPos = ((prevPin.TmpPos_ModFinal - curPin.TmpPos_ModFinal).normalized * 10.0f) + curPin.TmpPos_ModFinal;
						//	break;
					}

					dir2Next = normalizedNextPos - normalizedPrevPos;
					
					float angleToNext = 0.0f;
					if(dir2Next.sqrMagnitude > 0.0f)
					{
						angleToNext = Mathf.Atan2(dir2Next.y, dir2Next.x) * Mathf.Rad2Deg;
					}

					curPin.Tmp_CalculateWorldMatrix(targetVarType, apUtil.AngleTo180(angleToNext));
				}

				//사이드 컨트롤 포인트를 업데이트 한다.
				curPin.Tmp_RefreshControlPoints(targetVarType);
			}



			//2. 한쪽에 핀이 있는 경우만 계산
			//- 연결된 핀이 "두개의 핀이 연결된 핀"이라면 : Prev의 Next Control Point 또는 Next의 Prev Control Point를 대상으로 벡터를 긋고 각도 계산
			//- 연결된 핀이 "한개의 핀만 연결된 핀"이라면 : 
			for (int i = 0; i < nPins; i++)
			{
				curPin = _pins_All[i];
				prevPin = curPin._prevPin;
				nextPin = curPin._nextPin;

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
					if(prevPin._prevPin != null && prevPin._nextPin != null)
					{
						// 이 Pin은 양쪽이 연결된 상태다 = 루틴 1에서 Control Point가 생성되었을 것
						switch (targetVarType)
						{
							case apMeshPin.TMP_VAR_TYPE.MeshTest:	dir2Prev = prevPin.TmpControlPos_Next_MeshTest - curPin.TmpPos_MeshTest; break;
							case apMeshPin.TMP_VAR_TYPE.ModMid:		dir2Prev = prevPin.TmpControlPos_Next_ModMid - curPin.TmpPos_ModMid; break;
							//case apMeshPin.TMP_VAR_TYPE.ModFinal:	dir2Prev = prevPin.TmpControlPos_Next_ModFinal - curPin.TmpPos_ModFinal; break;
						}
					}
					else
					{
						// 이 Pin은 한쪽(현재 핀)만 연결되어 있다 = 루틴 1을 거치지 않아서 Control Point가 생성되지 않았다.
						switch (targetVarType)
						{
							case apMeshPin.TMP_VAR_TYPE.MeshTest:	dir2Prev = prevPin.TmpPos_MeshTest - curPin.TmpPos_MeshTest; break;
							case apMeshPin.TMP_VAR_TYPE.ModMid:		dir2Prev = prevPin.TmpPos_ModMid - curPin.TmpPos_ModMid; break;
							//case apMeshPin.TMP_VAR_TYPE.ModFinal:	dir2Prev = prevPin.TmpPos_ModFinal - curPin.TmpPos_ModFinal; break;
						}
					}
					
					//Y는 Dir > Prev의 +90
					float angleToPrev = 0.0f;
					if(dir2Prev.sqrMagnitude > 0.0f)
					{
						angleToPrev = Mathf.Atan2(dir2Prev.y, dir2Prev.x) * Mathf.Rad2Deg;
					}
					angleToPrev += 180.0f;
					curPin.Tmp_CalculateWorldMatrix(targetVarType, apUtil.AngleTo180(angleToPrev));
				}
				else//if(prevPin == null && nextPin != null)
				{
					//Next만 있다면

					Vector2 dir2Next = Vector2.zero;

					//Next 상태에 따라서 컨트롤 포인트를 기준으로 각도를 설정할지, 핀 위치를 기준으로 각도를 설정할지 정한다.
					if(nextPin._prevPin != null && nextPin._nextPin != null)
					{
						// 이 Pin은 양쪽이 연결된 상태다 = 루틴 1에서 Control Point가 생성되었을 것
						switch (targetVarType)
						{
							case apMeshPin.TMP_VAR_TYPE.MeshTest:	dir2Next = nextPin.TmpControlPos_Prev_MeshTest - curPin.TmpPos_MeshTest; break;
							case apMeshPin.TMP_VAR_TYPE.ModMid:		dir2Next = nextPin.TmpControlPos_Prev_ModMid - curPin.TmpPos_ModMid; break;
							//case apMeshPin.TMP_VAR_TYPE.ModFinal:	dir2Next = nextPin.TmpControlPos_Prev_ModFinal - curPin.TmpPos_ModFinal; break;
						}
					}
					else
					{
						// 이 Pin은 한쪽(현재 핀)만 연결되어 있다 = 루틴 1을 거치지 않아서 Control Point가 생성되지 않았다.
						switch (targetVarType)
						{
							case apMeshPin.TMP_VAR_TYPE.MeshTest:	dir2Next = nextPin.TmpPos_MeshTest - curPin.TmpPos_MeshTest; break;
							case apMeshPin.TMP_VAR_TYPE.ModMid:		dir2Next = nextPin.TmpPos_ModMid - curPin.TmpPos_ModMid; break;
							//case apMeshPin.TMP_VAR_TYPE.ModFinal:	dir2Next = nextPin.TmpPos_ModFinal - curPin.TmpPos_ModFinal; break;
						}
					}


					//Y는 Dir > Next의 -90
					
					float angleToNext = 0.0f;
					if(dir2Next.sqrMagnitude > 0.0f)
					{
						angleToNext = Mathf.Atan2(dir2Next.y, dir2Next.x) * Mathf.Rad2Deg;
					}
					curPin.Tmp_CalculateWorldMatrix(targetVarType, apUtil.AngleTo180(angleToNext));
				}

				//사이드 컨트롤 포인트를 업데이트 한다.
				curPin.Tmp_RefreshControlPoints(targetVarType);
			}
		}



		// Get / Set
		//---------------------------------------------
		public List<apMeshPin> Pins
		{
			get { return _pins_All; }
		}

		public apMeshPin GetPin(int uniqueID)
		{
			return _pins_All.Find(delegate(apMeshPin a)
			{
				return a._uniqueID == uniqueID;
			});
		}

	}
}