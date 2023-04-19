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

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 20.12.8
	/// 기존의 메시 생성기가 과정이 너무 번거롭고 메모리 문제와 결과물의 품질에 문제가 있어서 리뉴얼한다.
	/// 마법사의 절차는 없어지지만, 동기적으로 만들수 없으므로 로딩 시간을 부여한다.
	/// </summary>
	public partial class apMeshGeneratorV2
	{
		// Members
		//----------------------------------------------
		public delegate void FUNC_ON_MESH_GENERATE(PROCESS_STATE curState, SUB_STEP subState, List<GenResult> results, int iCurRequest, int nRequests);

		//전체 처리 과정
		public enum PROCESS_STATE
		{
			None,
			/// <summary>준비 단계. 텍스쳐 임포트 설정하기</summary>
			Step1_Ready,
			/// <summary>각각의 메시들에 버텍스들을 추가하는 단계</summary>
			Step2_Processing,
			/// <summary>마지막 정리 단계. 텍스쳐 설정들을 복구하자</summary>
			//Step3_AlmostComplete,//>>이건 그냥 마지막 단계로 넘긴다.
			/// <summary>완료 : 모두 성공</summary>
			Step4_Complete_Success,
			/// <summary>완료 : 일부 실패. 결과 확인하자</summary>
			Step4_Complete_SomeFailed,
			/// <summary>완료 : 모두 실패..</summary>
			Step4_Complete_AllFailed,
		}

		//처리 결과만 정의
		//각 메시별로 처리 결과를 정의하자
		public enum MESH_GEN_RESULT
		{
			Processing,
			Success,
			Failed,
		}

		//빠른 테스트를 위해서, 이 타입을 변경하여 메시에 반영되는 내용을 정하자
		//public enum APPLY_TO_MESH
		//{
		//	Result,//실제 결과
		//	ResultWithNormals,//Normal을 포함한 결과
		//	Result_StartAndEndNormal,
		//	Step2_FindRoots,
		//	Step3_MergeRoots,
		//	Step4_MakeCircleTree,
		//	Step5_MakeOuterVertices,
		//	Step7_MakeInnerVertices,
		//}

		//private APPLY_TO_MESH ApplyToMesh
		//{
		//	get
		//	{
		//		return APPLY_TO_MESH.Result;
		//	}
		//}


		public apEditor _editor;



		private List<GenRequest> _requests = null;
		private Dictionary<apTextureData, GenTextureData> _texData2GenData = new Dictionary<apTextureData, GenTextureData>();

		private GenRequest _curRequest = null;

		private bool _isProcessing = false;
		private PROCESS_STATE _processState = PROCESS_STATE.None;
		private PROCESS_STATE _nextState = PROCESS_STATE.None;
		private bool _isFirstFrame = false;
		private int _iCur = 0;

		private FUNC_ON_MESH_GENERATE _func_MeshGenerated = null;
		private List<GenResult> _results = null;

		


		//서브 스텝
		// - Request를 하나 꺼낸다. (에러가 발생한 요청은 패스)
		// - 기본 시작점을 찾는다. (중심부터)
		// - 원 / 반원을 Tree 형식으로 그려가면서 영역을 감싼다. (그 원에서의 외곽과 그 직전의 버텍스를 체크한다.)
		// - 점들을 알고리즘을 이용해서 모두 연결하자.
		// - 단 외곽 Vertex만 이루어진 삼각형은 제외한다.
		// - 완성된 데이터들을 메시에 반영한다.
		// - 서브루틴 끗!
		public enum SUB_STEP
		{
			None,
			Step1_PopRequest,
			Step2_FindRoots,
			//Step3_MergeRoots,
			//Step4_MakeCircleTree,
			//Step5_MergeOuterVertices,
			Step6_MakeOuterEdges,
			Step7_MakeInnerVertices,
			Step7_MakeOutVertWOInnerMargin,//추가. 내부 여백 없이 GenOutVert2InnerLineVert를 생성하자
			Step8_MakeTriangles,
			Step9_ApplyToMesh,
			
		}

		private SUB_STEP _subStep = SUB_STEP.None;
		private SUB_STEP _subNextStep = SUB_STEP.None;
		private bool _isSubFirstFrame = true;

		//중간 처리 과정
		private Dictionary<GenVertex, Dictionary<GenVertex, GenEdge>> _vert2Edge = null;//이건 Vertex 2개 > Edge로 가는 매핑 변수이다.

		//Wrap방식
		private List<GenWrapGroup> _wrapGroups = new List<GenWrapGroup>();

		//밀도, 병합 범위를 절대값으로 하지 않고 영역 해상도에 따라서 비례하자
		//기본 200을 기준으로 값을 비례하여 증가 > 영역마다 다르게.. 영역의 크기 차이가 크다면 에러가 발생한다.. 흐밍
		//public float _areaSizeRatio = 1.0f;
		//public float _maxShortSizeOfGroups = 1.0f;//모든 그룹의 ShortSize의 최대값
		//>> 이건 그룹별로하자
			
			


		//옵션들을 받자
		//Edge 분할
		//이전 : 21.10.5 > 옵션은 Request에 넣는다.
		//private int _option_Inner_Density = 2;//이 값이 커질수록 내부의 점이 많이 생성된다. 기본값 4
		//private int _option_OuterMargin = 10;//외부로의 여백
		//private int _option_InnerMargin = 5;//내부로의 여백
		//private bool _option_IsInnerMargin = true;//내부 여백 필요


		private const long MAX_TIMEOUT_MSEC = 60000;//1분(60 * 1000)
		//private const long MAX_TIMEOUT_MSEC = 10000;//1분(60 * 1000)
		private System.Diagnostics.Stopwatch _timeoutCounter = null;





		// Init
		//----------------------------------------------
		public apMeshGeneratorV2(apEditor editor)
		{
			_editor = editor;


			Clear();
		}

		private void Clear()
		{
			if (_requests == null)
			{
				_requests = new List<GenRequest>();
			}
			_requests.Clear();

			if (_texData2GenData == null)
			{
				_texData2GenData = new Dictionary<apTextureData, GenTextureData>();
			}
			_texData2GenData.Clear();

			_curRequest = null;
			_isProcessing = false;
			_processState = PROCESS_STATE.None;
			_nextState = PROCESS_STATE.None;
			_isFirstFrame = true;
			_iCur = 0;
			_func_MeshGenerated = null;

			if (_results == null)
			{
				_results = new List<GenResult>();
			}
			_results.Clear();

			_vert2Edge = null;//이건 Vertex 2개 > Edge로 가는 매핑 변수이다.

			_wrapGroups = null;
			//_areaSizeRatio = 1.0f;
			//_maxShortSizeOfGroups = 1.0f;

			_timeoutCounter = null;
		}

		// Functions
		//----------------------------------------------
		//이 함수를 먼저 호출한다.		
		public void ReadyToRequest(FUNC_ON_MESH_GENERATE func_MeshGenerated
										//,
										//int option_Inner_Density,
										//int option_OuterMargin,
										//int option_InnerMargin,
										//bool option_IsInnerMargin
									)
		{
			Clear();

			_isProcessing = false;
			_processState = PROCESS_STATE.None;
			_func_MeshGenerated = func_MeshGenerated;

			//옵션 설정 > Request로 이동 (21.10.5)
			//_option_Inner_Density = option_Inner_Density;
			//_option_OuterMargin = option_OuterMargin;
			//_option_InnerMargin = option_InnerMargin;
			//_option_IsInnerMargin = option_IsInnerMargin;
		}


		//public void AddRequest(apMesh mesh)//이전

		//변경 21.10.5 : 퀄리티 옵션을 Request에 넣는다.
		public void AddRequest(	apMesh mesh,
								int option_Inner_Density,
								int option_OuterMargin,
								int option_InnerMargin,
								bool option_IsInnerMargin)
		{
			if (mesh == null)
			{
				return;
			}
			//요청을 일단 추가한다.
			int iNext = _requests.Count;
			GenResult newResult = new GenResult(mesh);

			GenRequest newRequest = new GenRequest(	iNext,
													mesh,
													newResult,
													option_Inner_Density,
													option_OuterMargin,
													option_InnerMargin,
													option_IsInnerMargin);
			
			_requests.Add(newRequest);
			_results.Add(newResult);
		}



		public void StartGenerate()
		{
			_isProcessing = true;
			_processState = PROCESS_STATE.Step1_Ready;
			_nextState = PROCESS_STATE.Step1_Ready;
			_isFirstFrame = true;

			if(_timeoutCounter == null)
			{
				_timeoutCounter = new System.Diagnostics.Stopwatch();
			}
			_timeoutCounter.Stop();
			_timeoutCounter.Reset();
			_timeoutCounter.Start();
		}

		//처리를 종료한다. (강제로)
		public void EndGenerate()
		{
			Clear();
		}


		// Update Functions
		//----------------------------------------------
		public void Update()
		{
			try
			{
				if (_func_MeshGenerated == null || _processState == PROCESS_STATE.None)
				{
					//Debug.LogError("이벤트가 없다");
					//이벤트 받을게 없다면 종료
					EndGenerate();
					return;
				}
				switch (_processState)
				{
					case PROCESS_STATE.Step1_Ready:
						SubUpdate_Step1_Ready(_isFirstFrame);
						break;

					case PROCESS_STATE.Step2_Processing:
						SubUpdate_Step2_Processing(_isFirstFrame);
						break;

					//case PROCESS_STATE.Step3_AlmostComplete:
					//	SubUpdate_Step3_AlmostComplete(_isFirstFrame);
					//	break;

					case PROCESS_STATE.Step4_Complete_Success:
					case PROCESS_STATE.Step4_Complete_SomeFailed:
					case PROCESS_STATE.Step4_Complete_AllFailed:
						if(_isFirstFrame)
						{
							//텍스쳐 설정 복구하기
							RecoverTextureSettings();
						}
						//결과를 보고한다.
						//끝났다!
						_func_MeshGenerated(_processState, SUB_STEP.None, _results, 0, 0);
						EndGenerate();
						return;
				}

				//그 외에는 처리하지 않는다.

				//스테이트를 전환한다.
				if (_nextState != _processState)
				{
					_processState = _nextState;
					_isFirstFrame = true;
				}
				else if (_isFirstFrame)
				{
					_isFirstFrame = false;
				}
			}
			catch (Exception)
			{
				//Debug.LogError("에러 발생 : " + ex);


				//에러가 발생했다. > 종료
				if (_func_MeshGenerated != null)
				{
					_func_MeshGenerated(PROCESS_STATE.Step4_Complete_AllFailed, SUB_STEP.None, null, 0, 0);
				}

				EndGenerate();
			}
		}


		// < Step1 >
		// Request를 돌면서 TexGenData를 만든다.
		// TexGen을 만들면서 isReadable을 모두 활성화한다.
		// 처리가 끝나면 이벤트를 호출한다.
		//-------------------------------------------------------------------
		private void SubUpdate_Step1_Ready(bool isFirstFrame)
		{
			GenRequest curReq = null;
			apTextureData linkedTexData = null;
			Texture2D linkedImage = null;
			TextureImporter texImporter = null;

			GenTextureData genTexData = null;

			bool isAnyTexImportChanged = false;

			bool isAnySuccessRequest = false;
			for (int iReq = 0; iReq < _requests.Count; iReq++)
			{
				curReq = _requests[iReq];
				linkedTexData = null;
				linkedImage = null;
				texImporter = null;
				genTexData = null;

				if (!IsValidAtlasArea(curReq._mesh))
				{
					//에러 : Area가 없거나 너무 작다.
					curReq.SetFailed("Area is disabled or too small.");
					continue;
				}


				linkedTexData = curReq._mesh._textureData_Linked;
				if (linkedTexData == null)
				{
					//에러 : 텍스쳐 데이터가 연결되지 않았다.
					//curReq.SetResult(MESH_GEN_RESULT.Failed_NoTextureData);
					curReq.SetFailed("No Image is assigned to this mesh.");
					continue;
				}


				//이미 만들어져 있는지 체크
				if (_texData2GenData.ContainsKey(linkedTexData))
				{
					genTexData = _texData2GenData[linkedTexData];
				}
				else
				{
					linkedImage = linkedTexData._image;
					if (linkedImage == null)
					{
						//에러 : 텍스쳐 에셋이 없다.
						//curReq.SetResult(MESH_GEN_RESULT.Failed_NoImage);
						curReq.SetFailed("No Texture asset is assigned to the linked image.");
						continue;
					}

					string path = AssetDatabase.GetAssetPath(linkedImage);
					texImporter = TextureImporter.GetAtPath(path) as TextureImporter;

					//GenTexData를 추가한다.
					genTexData = new GenTextureData(linkedTexData, linkedImage, texImporter);
					_texData2GenData.Add(linkedTexData, genTexData);

					//중요! : 여기서 바로 isReadable을 켜자
					if (!texImporter.isReadable)
					{
						texImporter.isReadable = true;
						texImporter.SaveAndReimport();
						isAnyTexImportChanged = true;
					}

					//이제 색상을 가져오자
					genTexData.ReadColors();
				}

				//GenTexData를 연결한다.
				curReq.SetGenTextureData(genTexData);
				isAnySuccessRequest = true;//하나라도 성공한게 있다.
			}


			//하나라도 TextureImporter에 변동이 있다면
			if (isAnyTexImportChanged)
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			}


			//현재 상황을 리포트하고
			_func_MeshGenerated(PROCESS_STATE.Step1_Ready, SUB_STEP.None, _results, 0, 0);


			
			//스테이트 전환
			if(isAnySuccessRequest)
			{
				//하나라도 성공시
				ChangeState(PROCESS_STATE.Step2_Processing);
			}
			else
			{
				//모두 실패시
				ChangeState(PROCESS_STATE.Step4_Complete_AllFailed);
			}
			
		}


		/// <summary>
		/// 너무 시간이 오래 걸린다면..
		/// </summary>
		/// <returns></returns>
		private bool IsTimeout()
		{	
			return (_timeoutCounter.ElapsedMilliseconds > MAX_TIMEOUT_MSEC);
		}


		// < Step2 >
		// 서브 스테이트에 따라 동작한다.
		// - Request를 하나 꺼낸다. (에러가 발생한 요청은 패스)
		// - 기본 시작점을 찾는다. (중심부터)
		// - 원 / 반원을 Tree 형식으로 그려가면서 영역을 감싼다. (그 원에서의 외곽과 그 직전의 버텍스를 체크한다.)
		// - 점들을 알고리즘을 이용해서 모두 연결하자.
		// - 단 외곽 Vertex만 이루어진 삼각형은 제외한다.
		// - 완성된 데이터들을 메시에 반영한다.
		// - 서브루틴 끗!
		// 다음 요청이 없으면 Step3로 넘어간다.
		//-------------------------------------------------------------------
		private void SubUpdate_Step2_Processing(bool isFirstFrame)
		{
			if (isFirstFrame)
			{
				_subStep = SUB_STEP.Step1_PopRequest;
				_subNextStep = SUB_STEP.Step1_PopRequest;
				_isSubFirstFrame = true;
				_iCur = 0;
			}

			try
			{
				switch (_subStep)
				{
					case SUB_STEP.Step1_PopRequest:
						Processing_Step1_PopRequest(_isSubFirstFrame);
						break;

					case SUB_STEP.Step2_FindRoots:
						//Wrap 방식만 사용한다.
						Processing_Step2_FindRoots_Wrap(_isSubFirstFrame);
						break;

					case SUB_STEP.Step6_MakeOuterEdges:
						Processing_Step6_MakeOuterEdges_Wrap(_isSubFirstFrame);
						break;

					case SUB_STEP.Step7_MakeInnerVertices:
						Processing_Step7_MakeInnerVertices(_isSubFirstFrame);
						break;

					case SUB_STEP.Step7_MakeOutVertWOInnerMargin:
						Processing_Step7_MakeOutVertWOInnerMargin(_isSubFirstFrame);
						break;

					case SUB_STEP.Step8_MakeTriangles:
						Processing_Step8_MakeTriangles_V2(_isSubFirstFrame);
						break;

					case SUB_STEP.Step9_ApplyToMesh:
						Processing_Step9_ApplyToMesh(_isSubFirstFrame);
						break;
				}

				//현재 서브 상태와 관계없이 콜백 호출
				_func_MeshGenerated(PROCESS_STATE.Step2_Processing, _subStep, _results, _iCur, _requests.Count);

				//서브 스테이트 교체
				if (_subNextStep != _subStep)
				{
					_subStep = _subNextStep;
					_isSubFirstFrame = true;
				}
				else if (_isSubFirstFrame)
				{
					_isSubFirstFrame = false;
				}
			}
			catch(Exception ex)
			{
				//정말 에러 발생
				//현재 에러를 저장하고 스테이트 전환
				if(_curRequest != null)
				{
					_curRequest.SetFailed("Unknown Error : " + ex.ToString());
				}
				ChangeState(PROCESS_STATE.Step4_Complete_AllFailed);
			}
		}

		//Sub Step 1 : Request를 하나 꺼낸다. (에러가 발생한 요청은 패스)
		private void Processing_Step1_PopRequest(bool isFirstFrame)
		{
			//Debug.LogWarning("Processing_Step1_PopRequest");

			//request를 가져오자
			//범위와 유효성을 체크하자
			while (true)
			{
				if (_iCur >= _requests.Count)
				{
					//이미 다 가져왔다.
					ChangeState(PROCESS_STATE.Step4_Complete_Success);
					return;
				}
				//다음에 가져올 request가 유효한가				
				_curRequest = _requests[_iCur];

				if (!_curRequest._isFailed)
				{
					//유효한거 발견
					//처리해야한다.
					break;
				}

				//이미 에러가 발생했다면 다음으로 넘어간다.
				_iCur++;
			}

			//끝났으면 다음으로 이동
			_subNextStep = SUB_STEP.Step2_FindRoots;
		}


		//추가 : Wrap 방식으로 사각형 영역들=그룹을 만들자
		private void Processing_Step2_FindRoots_Wrap(bool isFirstFrame)
		{
			//Debug.LogWarning("Processing_Step2_FindRoots_Wrap");

			_curRequest.CalculateArea();//영역을 계산하자

			List<GenRectArea> rectAreas = new List<GenRectArea>();//전체 사각 영역

			
			//인식 범위와 거리 단위를 체크하자
			float areaMinX = _curRequest._area_Min.x;
			float areaMaxX = _curRequest._area_Max.x;
			float areaMinY = _curRequest._area_Min.y;
			float areaMaxY = _curRequest._area_Max.y;
			//float areaMinSize = Mathf.Min(_curRequest._areaWidth, _curRequest._areaHeight);

			int iAreaMinX = (int)areaMinX;
			int iAreaMaxX = (int)(areaMaxX + 0.5f);
			int iAreaMinY = (int)areaMinY;
			int iAreaMaxY = (int)(areaMaxY + 0.5f);




			//몇픽셀당 하나씩 볼까
			//- 1픽셀씩 검사하자!
			//(1) 연속적인 줄을 먼저 찾고, 줄이 끝날때 연결 가능한 기존의 Rect가 있는지 찾는다. (bias가 있음)
			//(2-1) 연속적인 X라인에서 "연결 가능한 Rect"가 여러개 있는 경우, 이 Rect들을 먼저 병합한다.
			//      병합시에는 루프를 이용해서 모든 병합이 끝날때까지 계속해서 병합한다.
			//      결과적으로 병합되는 Rect는 하나여야 한다.
			//(3) 대상 Rect에 현재의 줄을 추가한다.
			//(4) Y축으로 계속해서 이동한다.
			//(5) 완성된 Rect를 기준으로 다시 병합을 검사한다.

			bool isChecking = false;//현재 라인이 생성 중이다.
			int curLine_StartX = 0;
			int curLine_EndX = 0;

			List<GenRectArea> attachableRectAreas = new List<GenRectArea>();//전체 사각 영역
			GenRectArea curRectArea = null;

			for (int iY = iAreaMinY; iY <= iAreaMaxY; iY++)
			{
				isChecking = false;
				curLine_StartX = 0;
				curLine_EndX = 0;
				bool isNeedToCalculate = false;
				for (int iX = iAreaMinX; iX <= iAreaMaxX; iX++)
				{
					isNeedToCalculate = false;

					if(_curRequest.ImageHitTest(iX, iY))
					{
						//현재 픽셀이 있다면,
						if(!isChecking)
						{
							//빈칸 > 이미지
							//시작
							isChecking = true;
							curLine_StartX = iX;
							curLine_EndX = iX;
						}
						else
						{
							//이미지 > 이미지
							//계속 갱신
							curLine_EndX = iX;
							//만약 이게 마지막 X라면 계산 필요
							if(iX == iAreaMaxX)
							{
								isNeedToCalculate = true;
							}
						}
					}
					else
					{
						//현재 픽셀이 없다면
						if(!isChecking)
						{
							//빈칸 > 빈칸
						}
						else
						{
							//이미지 > 빈칸
							//있었는데 없습니다.
							//계산 필요
							isChecking = false;
							isNeedToCalculate = true;
						}
					}

					if(!isNeedToCalculate)
					{
						//더 처리할 필요가 없다.
						continue;
					}

					//사각 영역을 찾아서 추가하자
					attachableRectAreas.Clear();

					//추가 가능한 Rect가 있는가
					for (int iRect = 0; iRect < rectAreas.Count; iRect++)
					{
						curRectArea = rectAreas[iRect];
						if(curRectArea.IsAttachableLine(curLine_StartX, curLine_EndX, iY))
						{
							//추가 가능하다
							attachableRectAreas.Add(curRectArea);
						}
					}

					//이제 이 라인을 어떻게 추가할지 여부를 결정
					if(attachableRectAreas.Count == 0)
					{
						//<1> 추가될 수 있는 영역이 없는 경우
						//새로운 영역을 만든다.
						GenRectArea newRectArea = new GenRectArea(curLine_StartX, curLine_EndX, iY);
						rectAreas.Add(newRectArea);
					}
					else if(attachableRectAreas.Count == 1)
					{
						//<2> 추가될 수 있는 영역이 1개인 경우
						//해당 영역을 갱신하여 확장한다.
						attachableRectAreas[0].UpdateLine(curLine_StartX, curLine_EndX, iY);
					}
					else
					{
						//<3> 추가될 수 있는 영역이 여러개인 경우
						//맨 앞의 것만 남기고 병합한다.
						GenRectArea targetRectArea = attachableRectAreas[0];
						for (int iArea = 1; iArea < attachableRectAreas.Count; iArea++)
						{
							curRectArea = attachableRectAreas[iArea];
							targetRectArea.Merge(curRectArea);//0번째에 병합
							rectAreas.Remove(curRectArea);//리스트에서는 삭제
						}

						//해당 RectArea는 확장
						targetRectArea.UpdateLine(curLine_StartX, curLine_EndX, iY);
					}
				}
			}

			////테스트
			//_curRequest.SetFailed("테스트 실패");
			//ChangeState(PROCESS_STATE.Step4_Complete_AllFailed);
			//return;


			//마지막으로 RectArea의 병합을 체크하자
			if(rectAreas.Count > 1)
			{
				List<GenRectArea> removableAreas = new List<GenRectArea>();
				GenRectArea otherRectArea = null;
				//병합할 때는, "첫번째로 돌때는 안겹쳤는데, 다른거랑 합쳐서 오면 겹쳐지는 경우"가 있다.
				//따라서 변경내역이 없어질때까지 반복해야한다.
				while (true)
				{
					bool isAnyMerged = false;

					int prevNumArea = rectAreas.Count;

					removableAreas.Clear();
					for (int iArea = 0; iArea < rectAreas.Count; iArea++)
					{
						curRectArea = rectAreas[iArea];

						//이미 삭제될 거라면 생략
						if (removableAreas.Contains(curRectArea))
						{
							continue;
						}

						for (int iOther = 0; iOther < rectAreas.Count; iOther++)
						{
							otherRectArea = rectAreas[iOther];

							if (otherRectArea == curRectArea)
							{
								continue;
							}
							//이미 삭제될 거라면 생략
							if (removableAreas.Contains(otherRectArea))
							{
								continue;
							}

							if (curRectArea.IsOverapped(otherRectArea))
							{
								//겹쳐져있다.
								//otherRectArea > curRectArea로 병합후 삭제
								curRectArea.Merge(otherRectArea);
								removableAreas.Add(otherRectArea);
								isAnyMerged = true;
							}
						}
					}

					if (removableAreas.Count > 0)
					{
						//겹쳐져있는건 삭제하자
						for (int iRmv = 0; iRmv < removableAreas.Count; iRmv++)
						{
							rectAreas.Remove(removableAreas[iRmv]);
						}
					}
					

					if(!isAnyMerged || rectAreas.Count == 1 || prevNumArea == rectAreas.Count)
					{
						//더이상 병합할게 없다
						break;
					}

					if(IsTimeout())
					{
						//타임 아웃이라면
						_curRequest.SetFailed("Processing was terminated due to timeout.");
						ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
						return;
					}

					
					
				}
				
			}
			
			if(_wrapGroups == null)
			{
				_wrapGroups = new List<GenWrapGroup>();
			}
			_wrapGroups.Clear();
			//_areaSizeRatio = 1.0f;
			//_maxShortSizeOfGroups = 100.0f;

			//Rect를 WrapGroup으로 만든다.
			GenWrapGroup newWarapGroup = null;
			for (int i = 0; i < rectAreas.Count; i++)
			{
				newWarapGroup = new GenWrapGroup(rectAreas[i]);
				_wrapGroups.Add(newWarapGroup);

				//int shortSize = newWarapGroup._rectArea._shortSize;
				////크기 비율을 여기서 정하자
				//if(shortSize < 200)
				//{
				//	_areaSizeRatio = Mathf.Max(_areaSizeRatio, 1.0f);
				//}
				//else
				//{
				//	//200보다 크다면 크기 비율이 증가한다.
				//	_areaSizeRatio = Mathf.Max(_areaSizeRatio, (float)shortSize / 200.0f);
				//}
				//_maxShortSizeOfGroups = Mathf.Max(_maxShortSizeOfGroups, shortSize);
			}

			//--------------------------------------------------------------
			

			//테스트로 메시에 반영
			//if (ApplyToMesh == APPLY_TO_MESH.Step2_FindRoots)
			//{
			//	GenVertex vert = null;
			//	for (int iDebug = 0; iDebug < _vertices_Step1_Outer.Count; iDebug++)
			//	{
			//		vert = _vertices_Step1_Outer[iDebug];
			//		_curRequest._mesh.AddVertexAutoUV(vert._pos);
					
			//	}
			//}


			//------------------------------
			//다끝나면 Step6로
			_subNextStep = SUB_STEP.Step6_MakeOuterEdges;
		}


		private void Processing_Step6_MakeOuterEdges_Wrap(bool isFirstFrame)
		{
			//Debug.LogWarning("Processing_Step6_MakeOuterEdges_Wrap");

			//외곽 Edge들와 빠른 접근을 위한 매핑 리스트
			if (_vert2Edge == null) { _vert2Edge = new Dictionary<GenVertex, Dictionary<GenVertex, GenEdge>>(); }
			_vert2Edge.Clear();


			//WrapGroup을 이용하여 이미지를 인식해서 처리하는 방법
			//<Step1>
			// 1. 먼저 RectArea를 기준으로 8방향으로 기본 버텍스를 만들자 (원형)
			// 2. 주변 선분에 따라서 Normal을 계산하자
			// 3. Normal에 따라 증감을 하자. 처음엔 감소만 한다. (감소까지 완료해야 Step1 종료)

			//<Step2>
			// 루프를 돈다
			// - 루프 종료 조건 : 
			//  (1) 모든 선분에 충돌이 발생하지 않았으며, 해당 중점과 이미지의 거리가 margin 이내였다. > 바로 완료
			//  (2) 최대 분할 횟수를 넘어갔다. > 해당 선분의 각각의 충돌점을 바깥쪽으로 일일이 옮기고 종료
			// 루프의 특성
			// - 기본 분할 횟수 이후에는 Adaptive로 변경한다. (선분에 따라서 분할이 될 수도 있고 안될 수도 있다.)

			//루프 과정
			// 1. 모든 버텍스의 Normal을 구한다.
			// 2. 각 선분별로 중점을 생성한다. (Adaptive인 경우 선분의 내부 점들의 충돌과 Margin 범위를 체크하여 결정)
			// 3-1. 선분이 (중점 아님) Checked 상태면 중점에서 확장. 구멍이 있어서 우연히 중점이 비어 있어도, 무조건 Checkd > Empty로 상태 변화가 있어야 함
			// 3-2. 반대로 선분이 Checked 상태면 Margin 영역 이내까지 축소한다.
			// 반복


			//------------------------------------------------------------------
			////여백
			////float outerMargin = 10.0f;//하드코딩
			//float outerMargin = _option_OuterMargin;//옵션

			////중점 생성 무시 거리
			////이 값보다 작으면 조건에 상관없이 Edge
			//float minEdgeLengthIgnoreCond = 3.0f;

			////하드 코딩
			
			//////이 값보다 작으면 Edge를 축소 목적으로는 분할하지 않는다
			////float minEdgeLengthForShrink = 30.0f;

			//////이 값보다 크면 조건에 상관없이 무조건 분할한다.
			////float maxEdgeLengthForDivide = Mathf.Max(Mathf.Max(_curRequest._areaWidth, _curRequest._areaHeight) / 10.0f, minEdgeLengthForShrink * 3.0f);

			////옵션
			////float maxEdgeLengthForDivide = Mathf.Max(_curRequest._areaWidth, _curRequest._areaHeight) / Mathf.Max(_option_OutlineDivide_Density, 1.0f);

			////영역의 해상도에 비례한다.
			////float maxEdgeLengthForDivide = _maxShortSizeOfGroups / (float)_option_OutlineDivide_Density;
			
			////if(maxEdgeLengthForDivide < 90.0f * _areaSizeRatio)
			////{
			////	maxEdgeLengthForDivide = 90.0f * _areaSizeRatio;
			////}
			////조건 상관없이 중점 생성하는 거리
			//float maxEdgeLengthForDivide = 90.0f * _areaSizeRatio;

			////축소시에는 중점 생성 안되는 거리
			//float minEdgeLengthForShrink = 30.0f * _areaSizeRatio;


			//int optioninnerDensity = _option_Inner_Density;//이전
			int optioninnerDensity = _curRequest._option_Inner_Density;//변경 21.10.5
			
			//OuterMargin으로 벌어진 이후엔, 약간의 병합을 한다.

			//최대 루프 카운트
			int maxLoopCount = 20;

			
			GenWrapGroup curWrapGroup = null;
			for (int iWrap = 0; iWrap < _wrapGroups.Count; iWrap++)
			{
				curWrapGroup = _wrapGroups[iWrap];
				curWrapGroup.ClearVerticesAndEdges();




				//여백
				//float outerMargin = _option_OuterMargin;//이전
				float outerMargin = _curRequest._option_OuterMargin;//옵션

				//중점 생성 무시 거리
				//이 값보다 작으면 조건에 상관없이 Edge
				float minEdgeLengthIgnoreCond = 3.0f;

				//조건 상관없이 중점 생성하는 거리 (밀집도 옵션도 체크)
				float maxEdgeLengthForDivide = Mathf.Min(90.0f * curWrapGroup._areaSizeRatio, curWrapGroup._areaShortSize / (float)optioninnerDensity);
				if(maxEdgeLengthForDivide < 60.0f)
				{
					maxEdgeLengthForDivide = 60.0f;
				}

				//축소시에는 중점 생성 안되는 거리
				//float minEdgeLengthForShrink = 30.0f * curWrapGroup._areaSizeRatio;
				float minEdgeLengthForShrink = maxEdgeLengthForDivide / 3.0f;






				//<Step1>
				// 1. 먼저 RectArea를 기준으로 8방향으로 기본 버텍스를 만들자 (원형)
				// 2. 주변 선분에 따라서 Normal을 계산하자
				// 3. Normal에 따라 증감을 하자. 처음엔 감소만 한다. (감소까지 완료해야 Step1 종료)

				// 1. 먼저 RectArea를 기준으로 8방향으로 기본 버텍스를 만들자 (원형)
				//중점과 반지름을 만들어서 원형으로 만들 수 있게 준비
				Vector2 centerPos = new Vector2(	curWrapGroup._rectArea._minX * 0.5f + curWrapGroup._rectArea._maxX * 0.5f,
													curWrapGroup._rectArea._minY * 0.5f + curWrapGroup._rectArea._maxY * 0.5f);

				float areaWidth = Mathf.Abs(curWrapGroup._rectArea._maxX - curWrapGroup._rectArea._minX);
				float areaHeight = Mathf.Abs(curWrapGroup._rectArea._maxY - curWrapGroup._rectArea._minY);
				
				//반지름은 offset + 대각선의 절반
				float radius = Mathf.Sqrt((areaWidth * areaWidth) + (areaHeight * areaHeight)) * 0.5f;
				radius += 10.0f;//Offset


				int nStartVerts = 8;

				//8방향으로 점을 만들고 연결한다.
				for (int iAngle = 0; iAngle < nStartVerts; iAngle++)
				{
					float angleRad = ((360.0f / (float)nStartVerts) * iAngle) * Mathf.Deg2Rad;

					Vector2 vertPos = centerPos + new Vector2(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius);

					curWrapGroup._vertices_Outline.Add(new GenVertex(vertPos /*, VERT_TYPE.Outline*/));
				}

				//생성된 버텍스들을 Prev-Next 관계로 연결
				GenVertex curVert = null;
				GenVertex nextVert = null;

				

				for (int iVert = 0; iVert < nStartVerts; iVert++)
				{
					int iNextVert = (iVert + 1) % nStartVerts;

					curVert = curWrapGroup._vertices_Outline[iVert];
					nextVert = curWrapGroup._vertices_Outline[iNextVert];

					//서로 연결
					curVert._next = nextVert;
					nextVert._prev = curVert;
				}

				// 2. 주변 선분에 따라서 Normal을 계산하자
				curWrapGroup.CalculateNormals();


				// 3. Normal에 따라 증감을 하자. 처음엔 감소만 한다. (감소까지 완료해야 Step1 종료)
				//단, 한번에 감소가 안될 수 있으므로, 하나라도 실패하면, 조금씩 이동하여 몇차례 시도한다.
				int nVerts = curWrapGroup._vertices_Outline.Count;
				int maxTry = 20;
				for (int iTry = 1; iTry <= maxTry; iTry++)
				{
					for (int iVert = 0; iVert < nVerts; iVert++)
					{
						curVert = curWrapGroup._vertices_Outline[iVert];

						GenMoveResult result = curWrapGroup.MoveShrink_Parallel(_curRequest, curVert, outerMargin, iTry != maxTry);

						if (result != null)
						{
							if(iTry == maxTry)
							{
								curVert._pos = result._resultPos;
							}
							else
							{
								curVert._pos = curVert._pos * 0.7f + result._resultPos * 0.3f;
							}
							
							curVert._isSuccess = result._isSuccess;
						}
					}
					curWrapGroup.CalculateSmoothNormals();
				}


				//curWrapGroup.CalculateSmoothNormals();


				//<Step2>
				// 루프를 돈다
				// - 루프 종료 조건 : 
				//  (1) 모든 선분에 충돌이 발생하지 않았으며, 해당 중점과 이미지의 거리가 margin 이내였다. > 바로 완료
				//  (2) 최대 분할 횟수를 넘어갔다. > 해당 선분의 각각의 충돌점을 바깥쪽으로 일일이 옮기고 종료

				//모두 Adaptive
				//루프는 두개의 스텝이 번갈아가면서 이루어진다.
				//(1) 중점 생성 및 확장/축소 루프
				// - Edge 단위로 계산을 한다.
				// - Edge가 충돌했거나, Edge의 좌우 버텍스가 Margin 범위 밖에 있다면 > 중점 생성
				// - 중점이 생성된 경우, 
				//   > Edge가 충돌했다면 확장하는 방향으로 이동. (선분 상관없이 Normal 방향으로만)
				//   > Edge가 충돌한게 아니라면 일단 정지

				// 버텍스 Normal 계산

				//(2) 버텍스 축소
				// - 버텍스를 Cur > Next 순서로 체크한다. 중점을 더 만들지는 않는다.
				// - 버텍스가 Margin 범위 밖에 있다면 연결된 선분을 포함하여 축소를 시도한다. (약간 이동 후 몇차례 반복)
				// - (추가 21.2.19) 연결되지 않은 다른 Wrap의 선분과 충돌하면 축소 종료. (말리는걸 막기 위함)
				// - 이동 후 Normal 계산



				int loopCount = 1;//1부터 시작
				
				//리스트의 크기가 계속 바뀌므로, 계산될 Vertex, 
				List<GenVertex> processVerts_Prev = new List<GenVertex>();
				List<GenVertex> processVerts_Cur = new List<GenVertex>();
				List<GenVertex> processVerts_Next = new List<GenVertex>();
				//maxLoopCount = 0;
				while(true)
				{
					//일단 종료 조건을 체크한다.
					//  (1) 최대 분할 횟수를 넘어갔다. > 해당 선분의 각각의 충돌점을 바깥쪽으로 일일이 옮겨야 한다.
					//  (2) 모든 선분에 충돌이 발생하지 않았으며 해당 중점과 이미지의 거리가 margin 이내였다.
					
					if(loopCount > maxLoopCount)
					{
						//루프 카운트를 오버했다.
						//종료
						break;
					}

					
					// 1. 모든 버텍스의 Normal을 구한다.
					curWrapGroup.CalculateNormals();

					nVerts = curWrapGroup._vertices_Outline.Count;

					//체크를 해야할 버텍스를 리스트로 미리 만들자
					processVerts_Prev.Clear();
					processVerts_Cur.Clear();
					processVerts_Next.Clear();
					

					for (int iVert = 0; iVert < nVerts; iVert++)
					{
						curVert = curWrapGroup._vertices_Outline[iVert];
						//현재의 데이터를 구한다.
						processVerts_Cur.Add(curVert);
						processVerts_Prev.Add(curVert._prev);
						processVerts_Next.Add(curVert._next);
					}

					int nProcess = processVerts_Cur.Count;

					//Debug.Log("Num Process Vert : " + processVerts_Cur.Count);
					if (nProcess > 50000)
					{
						//------------------------------------------------
						_curRequest.SetFailed("Image recognition fails, resulting in too many vertices.");
						ChangeState(PROCESS_STATE.Step4_Complete_AllFailed);
						return;
						//------------------------------------------------
					}

					bool isAnyCheckedEdge = false;//선분이 이미지와 하나라도 충돌했는가
					bool isAnyOverMargin = false;//선분이 충돌하지 않았다는 가정하에, 중점이 margin보다 너무 멀리 떨어져 있는가
					
					for (int iVert = 0; iVert < nVerts; iVert++)
					{
						curVert = curWrapGroup._vertices_Outline[iVert];
						nextVert = curVert._next;

						//이 선분이 라인과 겹치나
						
						if(_curRequest.ImageHitTestLine_AnyHitEveryPixels(curWrapGroup, curVert._pos, nextVert._pos))
						{
							//선분이 충돌을 한다.
							isAnyCheckedEdge = true;
							break;
						}
						//충돌이 없다면 + Adaptive가 아닐 때
						Vector2 midPos = curVert._pos * 0.5f + nextVert._pos * 0.5f;
						Vector2 midDir = (curVert._dir + nextVert._dir).normalized;
						//margin + Offset만큼 뒤로 넣었을 때, 이미지를 발견 못하면 너무 많이 벌어진 것이다.
						if(!_curRequest.IsVertIsInMargin(curWrapGroup, midPos, midDir, outerMargin))
						{
							//Margin을 오버했다.
							isAnyOverMargin = true;
							break;
						}
					}
					if(!isAnyCheckedEdge && !isAnyOverMargin)
					{
						//모든 선분이 이미지와 충돌되지 않고, 중점이 너무 많이 떨어지지도 않았다.
						//종료
						break;
					}


					//-------------------------------

					// Step 1

					//(1) 중점 생성 및 확장/축소 루프
					// - Edge 단위로 계산을 한다.
					// - Edge가 충돌했거나, Edge의 좌우 버텍스가 Margin 범위 밖에 있다면 > 중점 생성
					// - 중점이 생성된 경우, 
					//   > Edge가 충돌했다면 확장하는 방향으로 이동. (선분 상관없이 Normal 방향으로만)
					//   > Edge가 충돌한게 아니라면 일단 정지
					GenVertex vertA = null;
					GenVertex vertB = null;
					for (int iProcess = 0; iProcess < nProcess; iProcess++)
					{
						vertA = processVerts_Cur[iProcess];
						vertB = processVerts_Next[iProcess];

						float distA2B = Vector2.Distance(vertA._pos, vertB._pos);
						if(distA2B < minEdgeLengthIgnoreCond)
						{
							//너무 가까우면 조건에 상관없이 패스
							continue;
						}

						//Edge가 이미지와 충돌했거나
						bool isAnyCheckedLine = _curRequest.ImageHitTestLine_AnyHitEveryPixels(curWrapGroup, vertA._pos, vertB._pos);

						//A, B가 Margin보다 안쪽에 있지 않다면
						bool isVertA_OutMargin = !_curRequest.IsVertIsInMargin(curWrapGroup, vertA, outerMargin * 3.0f);
						bool isVertB_OutMargin = !_curRequest.IsVertIsInMargin(curWrapGroup, vertB, outerMargin * 3.0f);

						//중간 점들도 너무 안쪽에 있을 때

						
						//중점이 너무 떨어진 경우 (중점은 조금 더 넉넉하게 준다)
						Vector2 midPos = vertA._pos * 0.5f + vertB._pos * 0.5f;
						Vector2 midDir = (vertA._dir + vertB._dir).normalized;
						bool isVertMid_OutMargin = !_curRequest.IsVertIsInMargin(curWrapGroup, midPos, midDir, outerMargin * 4.0f);
						
						

						//강제 중점 분할 카운트를 넘어간다면
						bool isNeedToDivide = false;
						if(distA2B > maxEdgeLengthForDivide)
						{
							//너무 길이가 길면 무조건 분할한다.
							isNeedToDivide = true;
						}
						else
						{
							bool isNeedToExpand = isAnyCheckedLine;
							bool isNeedToShrink = isVertA_OutMargin || isVertB_OutMargin || isVertMid_OutMargin;

							if(isNeedToExpand)
							{
								//확장해야 한다면 분할한다.
								isNeedToDivide = true;
							}
							else if(distA2B > minEdgeLengthForShrink && isNeedToShrink)
							{
								//축소해야한다면 분할한다.
								isNeedToDivide = true;
							}
						}
						
						if(!isNeedToDivide)
						{
							//중점을 만들 필요가 없다.
							continue;
						}
						

						//새로운 중점 버텍스를 만들자
						
						//위치를 꼭 중점으로 하지 말고, 조건에 따라서는 다른 곳을 찾아야 한다.					
						
						if(isAnyCheckedLine)
						{
							//- 충돌시에는 > 중점이 충돌하지 않는 경우에만 충돌점을 찾는다.
							if(!_curRequest.ImageHitTest(midPos))
							{
								// 중점이 충돌점이 아니다..충돌점을 찾자
								GenRequest.ImageHitResult hitResult = _curRequest.GetHitPosInLine(vertA._pos, vertB._pos);
								if(hitResult != null && hitResult.isHit)
								{
									//중점의 위치를 다른 곳에 두자
									midPos = hitResult.pos;
									midDir = (vertA._dir * (1.0f - hitResult.lerp)) + (vertB._dir * hitResult.lerp);
								}
							}
						}
						//else if(isVertAInMargin || isVertBInMargin)
						//{
						//	//- 좌우 점의 거리에 의한 축소시에는 > 예상 축소 길이가 가장 짧은 곳에 중점을 둔다. (거기가 모서리가 된다.)
						//	//GenRequest.ImageHitResult hitResult = _curRequest.GetNearestNormalPosInLine(vertA, vertB, distA2B * 2.0f, 5);
						//	//if(hitResult != null && hitResult.isHit)
						//	//{
						//	//	//중점의 위치를 다른 곳에 두자
						//	//	midPos = hitResult.pos;
						//	//	midDir = (vertA._dir * (1.0f - hitResult.lerp)) + (vertB._dir * hitResult.lerp);
						//	//}
						//}

						GenVertex newMidVert = new GenVertex(midPos/*, VERT_TYPE.Outline*/);
						newMidVert._dir = midDir;//일단 각도 벡터만 계산하자

						//리스트에 넣고
						curWrapGroup._vertices_Outline.Add(newMidVert);

						//A, B와 연결
						newMidVert._prev = vertA;
						newMidVert._next = vertB;
						vertA._next = newMidVert;
						vertB._prev = newMidVert;

						//중점을 이동하자
						if(isAnyCheckedLine)
						{
							//충돌했으면 바깥으로 이동
							GenMoveResult result = curWrapGroup.MoveExpand(_curRequest, newMidVert, outerMargin);

							if (result != null)
							{
								newMidVert._pos = result._resultPos;
								curVert._isSuccess = result._isSuccess;

							}

							//변경된 버텍스의 노멀 변경
							curWrapGroup.CalculateNormal(curVert._prev);
							curWrapGroup.CalculateNormal(curVert._next);
							curWrapGroup.CalculateNormal(curVert);
						}
					}

					// 버텍스 Normal 계산
					curWrapGroup.CalculateNormals();

					// Step 2

					//(2) 버텍스 축소
					// - 버텍스를 Cur > Next 순서로 체크한다. 중점을 더 만들지는 않는다.
					// - 버텍스가 Margin 범위 밖에 있다면 연결된 선분을 포함하여 축소를 시도한다. (약간 이동 후 몇차례 반복)
					// - (추가 21.2.19) 연결되지 않은 다른 Wrap의 선분과 충돌하면 축소 종료. (말리는걸 막기 위함)
					// - 이동 후 Normal 계산
					//for (int iProcess = 0; iProcess < nProcess; iProcess++)
					//{
					//	curVert = processVerts_Cur[iProcess];

					for (int iVert = 0; iVert < curWrapGroup._vertices_Outline.Count; iVert++)
					{
						curVert = curWrapGroup._vertices_Outline[iVert];

						//margin + Offset만큼 뒤로 넣었을 때, 이미지를 발견 못하면 너무 많이 벌어진 것이다.
						if(_curRequest.IsVertIsInMargin(curWrapGroup, curVert, outerMargin))
						{
							//Margin 이내라면
							continue;
						}

						//축소를 시도하자
						GenMoveResult result = curWrapGroup.MoveShrink(_curRequest, curVert, outerMargin);
						if (result != null && result._isSuccess)
						{
							if (loopCount < maxLoopCount)
							{
								curVert._pos = curVert._pos * 0.7f + result._resultPos * 0.3f;
							}
							else
							{
								curVert._pos = result._resultPos;
							}

							//변경된 버텍스의 노멀 변경
							curWrapGroup.CalculateNormal(curVert._prev);
							curWrapGroup.CalculateNormal(curVert._next);
							curWrapGroup.CalculateNormal(curVert);
						}
							
						curVert._isSuccess = result._isSuccess;
					}
					
					// 버텍스 Normal 계산
					curWrapGroup.CalculateNormals();

					//-------------------------------
					//루프 카운트 증가
					loopCount++;

					

					if(IsTimeout())
					{
						//타임 아웃이라면
						_curRequest.SetFailed("Processing was terminated due to timeout.");
						ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
						return;
					}
				}
			}

			//------------------------------------------------------------------

			//완료되면 Vert와 Edge를 모두 만들자

			//if(_vertices_Step2_Optimized == null)
			//{
			//	_vertices_Step2_Optimized = new List<GenVertex>();
			//}
			//_vertices_Step2_Optimized.Clear();


			//외곽 점들이 아래의 조건에 해당하고, 이미지에 충돌되지 않는다면 병합을 할 수 있다.
			// 병합은 Start > Last및 Last의 Next와 비교한다.
			//(1) 일정 Range 이내 (Start ~ Last)
			//(2) Normal이 비슷한건 의미 없고, Start의 next로의 벡터와 비교
			//(3) Start > Cur로의 선분 내에 이미지 충돌이 없어야 (Start ~ Last-Next)
			//(4) 병합하려면 Start 이후에 1개 이상 (Start > 1, 2.. n(Last))
			//병합시에는 Start -> Last-Next를 병합하고, Start 다음부터 Last까지 삭제한다.
			//Last-Next가 Start면 더이상 체크를 중지하고 병합을 시도하고 종료

			//Start - (1 - 2 ... Last) - Last-Next

			//옵션 값은 Step7의 옵션 값과 유사하다.
			//하드 코딩
			float outerMerge_Radius = 60.0f;
			float outerMerge_NextAngle = 10.0f;

			//옵션
			//float outerMerge_Radius = _option_OutlineVertMerge_Radius;
			//float outerMerge_NextAngle = _option_OutlineVertMerge_Angle;

			
			
			for (int iWrapGroup = 0; iWrapGroup < _wrapGroups.Count; iWrapGroup++)
			{
				curWrapGroup = _wrapGroups[iWrapGroup];

				curWrapGroup.Sort();
				curWrapGroup.CalculateNormals();

				if(curWrapGroup._vertices_Outline.Count == 0)
				{
					continue;
				}

				//시작점을 먼저 찾고, 병합할 수 있는건 병합하자
				//시작점은 Range이내, Normal 비슷 Start < Cur로의 선분 체크만 한다.
				GenVertex step1StartVert = curWrapGroup._vertices_Outline[0];
				GenVertex resultStartVert = null;
				GenVertex curVert = step1StartVert._prev;

				for (int iTry = 0; iTry < 20; iTry++)//최대 20개의 버텍스를 역으로 이동하여 찾자
				{
					if(curVert == step1StartVert)
					{
						break;
					}

					//조건 1 : (1) 일정 Range 이내 / (2) Normal이 비슷한 경우
					Vector2 prev2Start = step1StartVert._pos - curVert._pos;
					Vector2 prev2NextDir = curVert._next._pos - curVert._pos;
					float dist = prev2Start.magnitude;
					float deltaNextAngle = Vector2.Angle(prev2Start, prev2NextDir);

					//거리, 각도, 충돌 여부 체크후 병합 가능하면 이전으로 이동
					if(dist < outerMerge_Radius
						&& deltaNextAngle < outerMerge_NextAngle
						&& !_curRequest.ImageHitTestLine_AnyHitEveryPixels(curWrapGroup, curVert._pos, step1StartVert._pos))
					{
						resultStartVert = curVert;
						curVert = curVert._prev;//이전으로 이동
						
						continue;
					}

					//다 찾은 것 같다.
					break;
				}

				if(resultStartVert != null)
				{
					step1StartVert = resultStartVert;
				}

				//이제 병합체크를 하자
				//외곽선을 돌면서 합칠 수 있는건 합치자
				GenVertex mergeStartVert = null;
				List<GenVertex> mergedOutVerts = new List<GenVertex>();
				

				curVert = step1StartVert;
				int nOutlineVerts = curWrapGroup._vertices_Outline.Count;
				int iMerge = 0;
				int nRemoved = 0;

				while(true)
				{
					bool isLoopEnd = false;
					if(mergeStartVert == null)
					{
						//병합 시작
						mergeStartVert = curVert;
						mergedOutVerts.Clear();
						mergedOutVerts.Add(curVert);
					}
					else
					{

						//병합 중이면, 현재꺼를 추가할 수 있는지 확인
						// 병합은 Start > Last및 Last의 Next와 비교한다.
						//(1) 일정 Range 이내 (Start ~ Last)
						//(2) Normal이 비슷 (Start ~ Last)
						//(3) Start > Cur로의 선분 내에 이미지 충돌이 없어야 (Start ~ Last-Next)
						//(4) 병합하려면 Start 이후에 1개 이상 (Start > 1, 2.. n(Last))
						
						//Last-Next가 Start면 더이상 체크를 중지하고 병합을 시도하고 종료

						

						//이미 종료된 루프다. 
						isLoopEnd = curVert == step1StartVert;

						bool isAddToMerge = false;

						if (!isLoopEnd)
						{
							//끝나지 않았다면 검사를 하자
							Vector2 start2Cur = curVert._pos - mergeStartVert._pos;
							Vector2 start2NextDir = mergeStartVert._next._pos - mergeStartVert._pos;
							float dist = start2Cur.magnitude;
							float deltaNextAngle = Vector2.Angle(start2Cur, start2NextDir);

							if (dist < outerMerge_Radius
							&& deltaNextAngle < outerMerge_NextAngle
							&& !_curRequest.ImageHitTestLine_AnyHitEveryPixels(curWrapGroup, curVert._pos, mergeStartVert._pos)
							)
							{
								//더 병합할 거리가 있다.
								isAddToMerge = true;
							}

						}

						if(isAddToMerge)
						{
							//이어진다.
							mergedOutVerts.Add(curVert);
						}
						else
						{
							//이어지지 않는 버텍스다.
							//병합을 하자
							if(mergedOutVerts.Count > 2)
							{
								//시작, 마지막을 제외한 가운데만 삭제하고 연결해야하므로
								GenVertex vertA = mergedOutVerts[0];
								GenVertex vertB = mergedOutVerts[mergedOutVerts.Count - 1];

								vertA._next = vertB;
								vertB._prev = vertA;

								//가운데는 삭제
								for (int iRV = 1; iRV < mergedOutVerts.Count - 1; iRV++)
								{
									curWrapGroup._vertices_Outline.Remove(mergedOutVerts[iRV]);
									nRemoved++;
								}
							}
							
							mergeStartVert = curVert;
							mergedOutVerts.Clear();
							mergedOutVerts.Add(curVert);
						}
					}

					curVert = curVert._next;
					iMerge++;

					if(curVert == step1StartVert
						|| iMerge >= nOutlineVerts
						|| isLoopEnd)
					{
						break;
					}

					if(IsTimeout())
					{
						//타임 아웃이라면
						_curRequest.SetFailed("Processing was terminated due to timeout.");
						ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
						return;
					}
				}

				//Debug.Log("[" + nRemoved + "] 개의 외곽 버텍스들이 삭제되었다.");
				if(nRemoved > 0)
				{
					curWrapGroup.Sort();
					curWrapGroup.CalculateNormals();
				}
				
			}

			

			//완성된 데이터를 이용하여 Edge를 만든다.
			for (int iWrap = 0; iWrap < _wrapGroups.Count; iWrap++)
			{
				curWrapGroup = _wrapGroups[iWrap];

				int nVerts = curWrapGroup._vertices_Outline.Count;
				GenVertex vertA = null;
				GenVertex vertB = null;
				//일단 넣고
				//for (int iVert = 0; iVert < nVerts; iVert++)
				//{
				//	_vertices_Step2_Optimized.Add(curWrapGroup._vertices_Outline[iVert]);
				//}

				//Edge를 연결하자
				for (int iVert = 0; iVert < nVerts; iVert++)
				{
					vertA = curWrapGroup._vertices_Outline[iVert];
					vertB = vertA._next;

					if (_vert2Edge.ContainsKey(vertA))
					{
						if (_vert2Edge[vertA].ContainsKey(vertB))
						{
							continue;
						}
					}

					//연결선 생성
					GenEdge newEdge = new GenEdge(vertA, vertB);
					//_edges_Outer.Add(newEdge);


					//if (!_vert2OuterEdges.ContainsKey(vertA))
					//{
					//	_vert2OuterEdges.Add(vertA, new List<GenEdge>());
					//}
					//if (!_vert2OuterEdges.ContainsKey(vertB))
					//{
					//	_vert2OuterEdges.Add(vertB, new List<GenEdge>());
					//}
					//_vert2OuterEdges[vertA].Add(newEdge);
					//_vert2OuterEdges[vertB].Add(newEdge);

					if (!_vert2Edge.ContainsKey(vertA))
					{
						_vert2Edge.Add(vertA, new Dictionary<GenVertex, GenEdge>());
					}
					if (!_vert2Edge.ContainsKey(vertB))
					{
						_vert2Edge.Add(vertB, new Dictionary<GenVertex, GenEdge>());
					}
					_vert2Edge[vertA][vertB] = newEdge;
					_vert2Edge[vertB][vertA] = newEdge;

					//Wrap Group에도 Edge 추가
					curWrapGroup._edges_Outline.Add(newEdge);
				}

				//Outline 데이터를 EdgeGrid에 반영한다.
				curWrapGroup.StoreOutlineData();
			}



			//------------------------------------------------------------------
			//끝. 다음 단계로 넘어간다.
			//Inner Margin 옵션에 따라서 스텝이 다르다
			//if(_option_IsInnerMargin)
			if(_curRequest._option_IsInnerMargin)//변경 21.10.5
			{
				//Inner Margin이 있다.
				_subNextStep = SUB_STEP.Step7_MakeInnerVertices;
			}
			else
			{
				//Inner Margin이 없다.
				_subNextStep = SUB_STEP.Step7_MakeOutVertWOInnerMargin;
			}
			
		}








		//Sub Step 7 : 내부에 점들을 채운다.
		private void Processing_Step7_MakeInnerVertices(bool isFirstFrame)
		{
			//내부에 점들을 채운다.

			//-------------------------------------------------
			// 생성 과정

			//모든 처리는 WrapGroup 단위로 한다.


			// Step 1 : 내부 라인을 생성한다.
			// 1. 버텍스를 옆으로 이동하면서 (1) 일정 Range 이내 / (2) Normal이 비슷한 경우의 버텍스들을 묶는다.
			// 2. 버텍스들의 Inner Margin만큼의 -Normal의 평균에 "내부 라인 버텍스"를 만든다. 이건 데이터가 특이하다.
			// 3. 버텍스들의 Prev-Next를 이용해서 "내부 라인 버텍스"들을 서로 연결한다.
			// 4. 버텍스 그룹들과 "내부 라인 버텍스"들을 연결한다.
			// 예외1) 만약 "내부 라인 버텍스"간의 선분이 외곽선을 침범한다면, 충돌점에 중점을 만들어서 안쪽으로 이동시키자. 충돌 안된 만큼만.
			// 예외2) 만약 "내부 라인 버텍스"들이 너무 가깝다면 병합을 한다.
			// 예외3) 만약 "내부 라인 버텍스"를 만들 수 없을 정도로 영역이 좁다면 (-Normal이 다른 외곽선을 침범한다면) 생성하지 않는대신, 생성한게 없다는 정보만 남긴다. (연결을 위해서)


			// Step 2 : 내부 버텍스를 생성한다.
			// 1. "내부 라인 버텍스"들에서 -Normal 방향으로 선을 그어서 반대편 외곽선 또는 "내부 라인"과의 충돌점을 찾는다.
			// 2. 충돌 점과 가장 가까운 "내부 라인 버텍스"를 찾아서 잇는다. 이건 가이드 선분이다.
			// 3. 선분을 이었다 하더라도, 맞은편 "내부 라인 버텍스"에서 선분을 만들지 않는게 아니다. Normal 방향이 다를 수 있으니 마찬가지로 -Normal 방향으로 선분 생성
			//   > 양쪽 버텍스에 의한 가이드 선분이 같다면 하나는 삭제
			// 4. 가이드 선분들을 일단 다 만든다.
			// 5. 1) 가이드 선분들을 하나씩 비교하면서 교차점을 찾는다. 교차점은 별도의 데이터로 만들며, 교차점 생성시 가이드라인이 분리가 된다. (특정 정보는 유지)
			// 6. 2) 분리된 가이드 선분들 중에서 "최대 거리"를 넘어간 선분들은 2개 혹은 n개로로 분할한다.
			// 7. 특정 거리이내의 교차점들을 합친다. 데이터도 합칠것
			// 8. 가이드 정보를 활용해서 선분들을 생성한다.
			// 9. 다시 모든 "교차점" ~> "다른 교차점" / "내부 라인 버텍스"로의 연결 가능한 선분들을 찾고, 가까운 거리 순서대로 추가한다. 다른 선분과 교차시 제거

			
			GenWrapGroup curWrapGroup = null;
			GenVertex curVert = null;

			////외곽 버텍스를 내부 라인 버텍스로 복제할 때 병합하는 거리와 각도 차이
			//float outer2InnerMerge_Radius = 20.0f * _areaSizeRatio;//하드코딩 x 비율
			////float outer2InnerMerge_Radius = _option_Out2Inline_Radius;//옵션
			//float outer2InnerMerge_DeltaNormal = 5.0f;
			//float outer2InnerMerge_InLinePosRadius = 20.0f * _areaSizeRatio;//하드코딩 x 비율

			//OuterMargin보다 같거나 살짝 커야 한다. (많이 크면 안된다.

			//하드 코딩
			//float outerMargin = 10.0f;
			//float innerMargin = 5.0f;//실제로는 (10 + 5) 위에서 outerMargin이 10이더라. 나중에 변수로 바꾸자)

			//옵션
			//float outerMargin = _option_OuterMargin;
			//float innerMargin = _option_InnerMargin;//실제로는 (10 + 5) 위에서 outerMargin이 10이더라. 나중에 변수로 바꾸자)
			
			//변경 21.10.5 : Request에 따라 다름
			float outerMargin = _curRequest._option_OuterMargin;
			float innerMargin = _curRequest._option_InnerMargin;



			for (int iWrapGroup = 0; iWrapGroup < _wrapGroups.Count; iWrapGroup++)
			{
				curWrapGroup = _wrapGroups[iWrapGroup];

				//외곽 버텍스를 내부 라인 버텍스로 복제할 때 병합하는 거리와 각도 차이
				float outer2InnerMerge_Radius = 20.0f * curWrapGroup._areaSizeRatio;//하드코딩 x 비율
				float outer2InnerMerge_DeltaNormal = 5.0f;
				float outer2InnerMerge_InLinePosRadius = 20.0f * curWrapGroup._areaSizeRatio;//하드코딩 x 비율




				// Step 1 : 내부 라인을 생성한다.
				// 1. 버텍스를 옆으로 이동하면서 (1) 일정 Range 이내 / (2) Normal이 비슷한 경우 / 또는 (1) -Normal로 이동한 점이 특정 Range 이내인 버텍스들을 묶는다.
				// 2. 버텍스들의 Inner Margin만큼의 -Normal의 평균에 "내부 라인 버텍스"를 만든다. 이건 데이터가 특이하다.
				// 3. 버텍스들의 Prev-Next를 이용해서 "내부 라인 버텍스"들을 서로 연결한다.
				// 4. 버텍스 그룹들과 "내부 라인 버텍스"들을 연결한다.
				// 예외1) 만약 "내부 라인 버텍스"간의 선분이 외곽선을 침범한다면, 충돌점에 중점을 만들어서 안쪽으로 이동시키자. 충돌 안된 만큼만.
				// 예외2) 만약 "내부 라인 버텍스"들이 너무 가깝다면 병합을 한다.
				// 예외3) 만약 "내부 라인 버텍스"를 만들 수 없을 정도로 영역이 좁다면 (-Normal이 다른 외곽선을 침범한다면) 생성하지 않는대신, 생성한게 없다는 정보만 남긴다. (연결을 위해서)

				curWrapGroup.Sort();
				curWrapGroup.CalculateNormals();

				if(curWrapGroup._vertices_Outline.Count == 0)
				{
					continue;
				}

				// 1. 버텍스를 옆으로 이동하면서 (1) 일정 Range 이내 / (2) Normal이 비슷한 경우의 버텍스들을 묶는다.
				//병합을 시작할 버텍스를 찾는다. 처음엔 Prev쪽으로 찾는다.
				GenVertex step1StartVert = curWrapGroup._vertices_Outline[0];
				GenVertex resultStartVert = null;
				curVert = step1StartVert._prev;

				Vector2 startInLinePos = step1StartVert._pos - (step1StartVert._dir * (innerMargin + outerMargin));

				//리스트에 넣어서 체크
				List<Vector2> checkPosList_From = new List<Vector2>();
				List<Vector2> checkPosList_To = new List<Vector2>();

				checkPosList_From.Add(step1StartVert._pos);
				checkPosList_To.Add(startInLinePos);

				for (int iTry = 0; iTry < 20; iTry++)//최대 20개의 버텍스를 역으로 이동하여 찾자
				{
					if(curVert == step1StartVert)
					{
						break;
					}

					//조건 1 : (1) 일정 Range 이내 / (2) Normal이 비슷한 경우
					float dist = Vector2.Distance(curVert._pos, step1StartVert._pos);
					float deltaNormal = Mathf.Abs(apUtil.AngleTo180(curVert._angle - step1StartVert._angle));

					Vector2 curInLinePos = curVert._pos - (curVert._dir * (innerMargin + outerMargin));
					float distInLinePos = Vector2.Distance(startInLinePos, curInLinePos);

					if(dist < outer2InnerMerge_Radius
						&& deltaNormal < outer2InnerMerge_DeltaNormal)
					{
						//더 이동 가능하다.
						checkPosList_From.Add(curVert._pos);
						checkPosList_To.Add(curInLinePos);

						resultStartVert = curVert;
						curVert = curVert._prev;//이전으로 이동

						
						continue;
					}

					//조건 2 : (1) -Normal로 이동한 점이 특정 Range 이내 또는 내부로의 선이 교차한 경우
					
					if(distInLinePos < outer2InnerMerge_InLinePosRadius
						|| GenEdge.CheckCrossApprox(curInLinePos, curVert._pos, checkPosList_From, checkPosList_To))
					{
						//더 이동 가능하다.
						checkPosList_From.Add(curVert._pos);
						checkPosList_To.Add(curInLinePos);

						resultStartVert = curVert;
						curVert = curVert._prev;//이전으로 이동

						
						continue;
					}



					//다 찾은 것 같다.
					break;
				}

				if(resultStartVert != null)
				{
					step1StartVert = resultStartVert;
					startInLinePos = step1StartVert._pos - (step1StartVert._dir * (innerMargin + outerMargin));
				}

				//외곽선을 돌면서 합칠 수 있는건 합치자
				GenVertex mergeStartVert = null;
				List<GenVertex> mergedOutVerts = new List<GenVertex>();
				
				curVert = step1StartVert;
				int nOutlineVerts = curWrapGroup._vertices_Outline.Count;
				int iMerge = 0;

				checkPosList_From.Clear();
				checkPosList_To.Clear();

				////Outline Vert에서 Data로 변환되는 정보를 가져오자
				//Dictionary<GenVertex, GenOutVert2InnerLineVerts> outVert2Data = new Dictionary<GenVertex, GenOutVert2InnerLineVerts>();

				//int iLoop = 0;
				GenVertex virtualMergedVert = null;

				while(true)
				{
					if(mergeStartVert == null)
					{
						//병합 시작
						mergeStartVert = curVert;
						startInLinePos = mergeStartVert._pos - (mergeStartVert._dir * (innerMargin + outerMargin));
						mergedOutVerts.Clear();
						mergedOutVerts.Add(curVert);

						checkPosList_From.Clear();
						checkPosList_To.Clear();
						checkPosList_From.Add(mergeStartVert._pos);
						checkPosList_To.Add(startInLinePos);
					}
					else
					{
						//병합 중이면, 현재꺼를 추가할 수 있는지 확인
						float dist = Vector2.Distance(curVert._pos, mergeStartVert._pos);
						float deltaNormal = Mathf.Abs(apUtil.AngleTo180(curVert._angle - mergeStartVert._angle));
						
						Vector2 curInLinePos = curVert._pos - (curVert._dir * (innerMargin + outerMargin));
						float distInLinePos = Vector2.Distance(startInLinePos, curInLinePos);

						//선 연결시 Outline과 교차하지 않아야 한다. (추가)
						//병합을 가정해서, 병합된 InLineVert이 이전(이미 있는) + 다음(예정)간의 연결선이 외곽선을 침범하지 않는 경우에만
						
						//가상의 중점 계산
						Vector2 virtualAvgInLinePos = Vector2.zero;

						for (int iMergedOut = 0; iMergedOut < mergedOutVerts.Count; iMergedOut++)
						{
							virtualMergedVert = mergedOutVerts[iMergedOut];
							virtualAvgInLinePos += virtualMergedVert._pos - (virtualMergedVert._dir * (innerMargin + outerMargin));
						}
						//추가할 버텍스도 확인
						virtualAvgInLinePos += curInLinePos;
						virtualAvgInLinePos /= (mergedOutVerts.Count + 1);

						//이전, 이후의 InLinePos
						Vector2 prevInLinePos = mergeStartVert._prev._pos - (mergeStartVert._prev._dir * (innerMargin + outerMargin));
						Vector2 nextInLinePos = curVert._next._pos - (curVert._next._dir * (innerMargin + outerMargin));

						bool isCross_ToPrev = curWrapGroup.IsCrossWithSimulate(virtualAvgInLinePos, prevInLinePos);
						bool isCross_ToNext = curWrapGroup.IsCrossWithSimulate(virtualAvgInLinePos, nextInLinePos);

						//if(isCross_ToPrev)
						//{
						//	Debug.LogError("Prev와 충돌 > Merge 중단.");
						//}
						//if(isCross_ToNext)
						//{
						//	Debug.LogError("Next와 충돌 > Merge 중단.");
						//}

						//bool isCross = curWrapGroup.IsCrossWithInLines(null, startInLinePos, curInLinePos);
						bool isCross = isCross_ToPrev | isCross_ToNext;
						//isCross = false;
						
						//(6) Cur 또는 Cur-Next가 "바깥쪽으로 꺾었다면" 중단
						Vector2 start2Cur = curVert._pos - mergeStartVert._pos;
						Vector2 start2Next = curVert._next._pos - mergeStartVert._pos;
						bool isOuterCorner = false;
						if(Vector2.Angle(start2Cur, start2Next) > 10.0f && Vector2.Angle(mergeStartVert._dir, start2Next) < 80.0f)
						{
							//바깥쪽 코너 만남
							//Debug.Log("바깥쪽 코너 만남 [" + mergeStartVert._pos + " >> " + curVert._pos + "]");
							isOuterCorner = true;
						}
						
						bool isMerged = false;

						if(!isCross && !isOuterCorner)
						{
							if(dist < outer2InnerMerge_Radius
							&& deltaNormal < outer2InnerMerge_DeltaNormal)
							{
								//이어진다.
								isMerged = true;
							}
							else if(distInLinePos < outer2InnerMerge_InLinePosRadius 
								|| GenEdge.CheckCrossApprox(curInLinePos, curVert._pos, checkPosList_From, checkPosList_To))
							{
								//조건 2 : (1) -Normal로 이동한 점이 특정 Range 이내 또는 내부로의 선이 교차한 경우
								//더 이동 가능하다.		
								isMerged = true;
							}
						}

						if (isMerged)
						{
							//병합 후 다음으로 이동
							mergedOutVerts.Add(curVert);

							checkPosList_From.Add(curVert._pos);
							checkPosList_To.Add(curInLinePos);
						}						
						else
						{
							//이어지지 않는 버텍스다.
							//지금까지의 버텍스들을 묶자
							GenOutVert2InnerLineVerts newOut2InlineVertData = new GenOutVert2InnerLineVerts();
							newOut2InlineVertData.SetOutVertices(mergedOutVerts);

							curWrapGroup._out2InLineVerts.Add(newOut2InlineVertData);

							//변환 정보 저장
							//for (int iOutVert = 0; iOutVert < mergedOutVerts.Count; iOutVert++)
							//{
							//	outVert2Data.Add(mergedOutVerts[iOutVert], newOut2InlineVertData);
							//}

							//다시 시작
							mergeStartVert = curVert;
							startInLinePos = mergeStartVert._pos - (mergeStartVert._dir * (innerMargin + outerMargin));
							mergedOutVerts.Clear();
							mergedOutVerts.Add(curVert);

							checkPosList_From.Clear();
							checkPosList_To.Clear();
							checkPosList_From.Add(mergeStartVert._pos);
							checkPosList_To.Add(startInLinePos);
						}
					}

					curVert = curVert._next;
					iMerge++;

					if(curVert == step1StartVert
						|| iMerge >= nOutlineVerts)
					{
						if(mergedOutVerts.Count > 0)
						{
							//남은게 있다.
							GenOutVert2InnerLineVerts newOut2InlineVertData = new GenOutVert2InnerLineVerts();
							newOut2InlineVertData.SetOutVertices(mergedOutVerts);
							curWrapGroup._out2InLineVerts.Add(newOut2InlineVertData);
						}
						break;
					}

					if(IsTimeout())
					{
						//타임 아웃이라면
						_curRequest.SetFailed("Processing was terminated due to timeout.");
						ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
						return;
					}
				}

				//내부 라인 버텍스 데이터들을 서로 연결한다.
				curWrapGroup.LinkOut2InnerVertData();

				//외곽선 데이터를 그리드에 넣어서 아래의 처리를 가속화하자
				curWrapGroup.StoreOutlineEdgesToGrid();

				// 2. 버텍스들의 Inner Margin만큼의 -Normal의 평균에 "내부 라인 버텍스"를 만든다. 이건 데이터가 특이하다.
				// 예외1) 만약 "내부 라인 버텍스"간의 선분이 외곽선을 침범한다면, 충돌점에 중점을 만들어서 안쪽으로 이동시키자. 충돌 안된 만큼만.
				// 예외2) 만약 "내부 라인 버텍스"들이 너무 가깝다면 병합을 한다.
				// 예외3) 만약 "내부 라인 버텍스"를 만들 수 없을 정도로 영역이 좁다면 (-Normal이 다른 외곽선을 침범한다면) 생성하지 않는대신, 생성한게 없다는 정보만 남긴다. (연결을 위해서)

				// 다른 내부 선분에 충돌했다면, 두가지 선택지가 있다.
				// (1) 병합. Inner Line Vertex를 병합한다.
				// (2) 이동 범위 축소.
				// 이동 범위를 축소(2)하려면 다음의 조건을 만족해야한다.
				// - 충돌된 Edge의 Inner Line Vertex들 Normal과의 각도 차이가 모두 60도 미만
				// - 축소 후 Margin이 기존보다 50% 미만인 경우 (너무 많이 움직여야 하는 경우)
				// 위 조건을 만족하지 않는다면 병합(1)을 한다.
				// 위의 처리 결과에 상관없이 이 Inner Line Vertex에서는 가이드를 생성하지 않는다. (플래그 걸어둘것)


				int nOut2InVertData = curWrapGroup._out2InLineVerts.Count;
				GenOutVert2InnerLineVerts curOut2InVertData = null;

				for (int iData = 0; iData < nOut2InVertData; iData++)
				{
					curOut2InVertData = curWrapGroup._out2InLineVerts[iData];

					int nOutVerts = curOut2InVertData._outVerts.Count;
					if(nOutVerts == 0)
					{
						continue;
					}
					Vector2 avgInnerPos = Vector2.zero;
					Vector2 sumInnerDir = Vector2.zero;

					for (int iOutVert = 0; iOutVert < nOutVerts; iOutVert++)
					{
						curVert = curOut2InVertData._outVerts[iOutVert];

						Vector2 innerPos = curVert._pos - (curVert._dir * (innerMargin + outerMargin));
						avgInnerPos += innerPos;
						sumInnerDir += curVert._dir;
					}

					avgInnerPos /= nOutVerts;
					sumInnerDir.Normalize();

					//이제 버텍스를 추가하자
					GenVertex newInLineVert = new GenVertex(avgInnerPos/*, VERT_TYPE.Inline*/);
					newInLineVert._dir = sumInnerDir;
					newInLineVert._angle = Mathf.Atan2(sumInnerDir.y, sumInnerDir.x) * Mathf.Rad2Deg;

					curOut2InVertData._innerLineVert = newInLineVert;
					curOut2InVertData._orgInLinePos = avgInnerPos;//초기 위치
					curOut2InVertData._remainContractedLength = innerMargin + (outerMargin * 0.5f);//축소 가능한 여백의 길이

					//버텍스들을 연결하자
					for (int iOutVert = 0; iOutVert < nOutVerts; iOutVert++)
					{
						curVert = curOut2InVertData._outVerts[iOutVert];
						GenEdge newOut2InlineEdge = new GenEdge(curVert, curOut2InVertData._innerLineVert);

						curOut2InVertData._outVerts2InnerLineVertEdges.Add(newOut2InlineEdge);
					}
				}


				// 3. 버텍스들의 Prev-Next를 이용해서 "내부 라인 버텍스"들을 서로 연결한다.
				// 4. 버텍스 그룹들과 "내부 라인 버텍스"들을 연결한다.

				//다시 돌면서, 서로 연결한다.
				//Cur > Next로 돈다.
				GenOutVert2InnerLineVerts nextOut2InVertData = null;
				for (int iData = 0; iData < nOut2InVertData; iData++)
				{
					curOut2InVertData = curWrapGroup._out2InLineVerts[iData];
					if(curOut2InVertData._nextData != null)
					{
						nextOut2InVertData = curOut2InVertData._nextData;
						GenVertex inlineVert_A = curOut2InVertData._innerLineVert;
						GenVertex inlineVert_B = nextOut2InVertData._innerLineVert;
						GenEdge newEdge = new GenEdge(inlineVert_A, inlineVert_B);

						curOut2InVertData._innerLineEdge_ToNext = newEdge;
						nextOut2InVertData._innerLineEdge_ToPrev = newEdge;
					}

					
				}

				

				for (int iData = 0; iData < nOut2InVertData; iData++)
				{
					curOut2InVertData = curWrapGroup._out2InLineVerts[iData];
					//다음의 처리를 위해 여기서 Min-Max Area를 생성하자
					curOut2InVertData.MakeMinMaxArea();

				}

				//충돌 여부를 검사하고 Normal Line의 이동 거리를 줄여야 한다.

				// 다른 내부 선분에 충돌했다면, 두가지 선택지가 있다.
				// (1) 병합. Inner Line Vertex를 병합한다.
				// (2) 이동 범위 축소.
				// 이동 범위를 축소(2)하려면 다음의 조건을 만족해야한다.
				// - 충돌된 Edge의 Inner Line Vertex들 Normal과의 각도 차이가 모두 60도 미만
				// - 축소 후 Margin이 기존보다 50% 미만인 경우 (너무 많이 움직여야 하는 경우)
				// 위 조건을 만족하지 않는다면 병합(1)을 한다.
				// 위의 처리 결과에 상관없이 이 Inner Line Vertex에서는 가이드를 생성하지 않는다. (플래그 걸어둘것)
				
				
				//병합은 했으니 축소만 한다.
				

				bool isNeedToMerge = true;

				if (isNeedToMerge)
				{
					GenOutVert2InnerLineVerts otherOut2InVertData = null;
					
					GenVertex curInlineVert = null;
					GenVertex otherInlineVert = null;

					int iLoop = 0;
					while (true)
					{
						bool isAnyChanged = false;
						for (int iData = 0; iData < nOut2InVertData; iData++)
						{
							curOut2InVertData = curWrapGroup._out2InLineVerts[iData];
							curInlineVert = curOut2InVertData._innerLineVert;

							//if (curOut2InVertData._isMerged)
							//{
							//	continue;
							//}

							for (int iOther = 0; iOther < nOut2InVertData; iOther++)
							{
								otherOut2InVertData = curWrapGroup._out2InLineVerts[iOther];
								otherInlineVert = otherOut2InVertData._innerLineVert;

								if (curOut2InVertData == otherOut2InVertData
									//|| otherOut2InVertData._isMerged
									|| curInlineVert == otherInlineVert
									)
								{
									continue;
								}


								if (!curOut2InVertData.IsCross(otherOut2InVertData))
								{
									//충돌하지 않는다.
									//Debug.Log("충돌하지 않음 : [" + iData + " > " + iOther + "] : " + curInlineVert._pos + " > " + otherInlineVert._pos);
									continue;
								}

								//Debug.Log("충돌한다 : [" + iData + " > " + iOther + "] : " + curInlineVert._pos + " > " + otherInlineVert._pos);

								//축소를 하자
								float curMoreContract = innerMargin * 0.3f;
								float otherMoreContract = innerMargin * 0.3f;
								if(curOut2InVertData._remainContractedLength - curMoreContract < 0.0f)
								{
									//남은 축소 거리가 얼마 안남았다.
									curMoreContract = curOut2InVertData._remainContractedLength;
								}
								if(otherOut2InVertData._remainContractedLength - otherMoreContract < 0.0f)
								{
									//남은 축소 거리가 얼마 안남았다.
									otherMoreContract = otherOut2InVertData._remainContractedLength;
								}

								curInlineVert._pos += curInlineVert._dir * curMoreContract;
								otherInlineVert._pos += otherInlineVert._dir * otherMoreContract;


								//축소 거리를 줄이자
								curOut2InVertData._remainContractedLength -= curMoreContract;
								otherOut2InVertData._remainContractedLength -= otherMoreContract;

								
								//Area 크기 갱신
								curOut2InVertData.MakeMinMaxArea();
								otherOut2InVertData.MakeMinMaxArea();

								//Debug.Log("- 축소가 되었다. ");

								isAnyChanged = true;
							}
						}

						if(!isAnyChanged)
						{
							break;
						}

						iLoop++;
						if (iLoop > 20)
						{
							//Debug.LogError("--- 강제로 루프 종료 ---");
							break;
						}

						if (IsTimeout())
						{
							//타임 아웃이라면
							_curRequest.SetFailed("Processing was terminated due to timeout.");
							ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
							return;
						}
					}
				}


				//이 상태에서 Outline과 교차되는 선들이 있을 수 있다.
				//이때는 다른 위치를 고려한다.
				//for (int iData = 0; iData < nOut2InVertData; iData++)
				//{
				//	curOut2InVertData = curWrapGroup._out2InLineVerts[iData];
				//	//Prev / Next만 고려한다.
				//	GenEdge edgePrev = curOut2InVertData._innerLineEdge_ToPrev;
				//	GenEdge edgeNext = curOut2InVertData._innerLineEdge_ToNext;
				//	bool isCross_ToPrev = (edgePrev != null) ? curWrapGroup.IsCrossWithSimulate(edgePrev._vert_A._pos, edgePrev._vert_B._pos) : false;
				//	bool isCross_ToNext = (edgeNext != null) ? curWrapGroup.IsCrossWithSimulate(edgeNext._vert_A._pos, edgeNext._vert_B._pos) : false;

				//	if (isCross_ToPrev || isCross_ToNext)
				//	{
				//		Debug.LogError("외곽선을 침범하는 InLine 발견 : " + curOut2InVertData._innerLineVert._pos);
				//	}
				//}

				//여기서 작성된 버텍스와 선분들을 WrapGroup의 멤버로도 저장하자
				curWrapGroup.StoreVertexAndEdgeOfInlineData();
			}

			


			//-------------------------------------------------


			_subNextStep = SUB_STEP.Step8_MakeTriangles;

			//_subNextStep = SUB_STEP.Step9_ApplyToMesh;//트라이 만들기는 일단 패스


		}



		private void Processing_Step7_MakeOutVertWOInnerMargin(bool isFirstFrame)
		{
			//내부 여백없이 GenOutVert2InnerLineVerts를 생성한다.
			//당연히 많은 변수가 비어있다.

			GenWrapGroup curWrapGroup = null;
			GenVertex curVert = null;

			//float outerMargin = _option_OuterMargin;

			//WrapGroup 하나씩 보자
			for (int iWrapGroup = 0; iWrapGroup < _wrapGroups.Count; iWrapGroup++)
			{
				curWrapGroup = _wrapGroups[iWrapGroup];

				curWrapGroup.Sort();
				curWrapGroup.CalculateNormals();

				if(curWrapGroup._vertices_Outline.Count == 0)
				{
					continue;
				}



				//각 버텍스들을 하나씩 Out2InLineVert로 만든다.
				List<GenVertex> outVerts = new List<GenVertex>();

				int nOutVerts = curWrapGroup._vertices_Outline.Count;
				for (int iOutVert = 0; iOutVert < nOutVerts; iOutVert++)
				{
					curVert = curWrapGroup._vertices_Outline[iOutVert];

					outVerts.Clear();
					outVerts.Add(curVert);

					GenOutVert2InnerLineVerts newOut2InlineVertData = new GenOutVert2InnerLineVerts();
					newOut2InlineVertData.SetOutVertices(outVerts);
					newOut2InlineVertData._isOnlyOutVerts = true;//이게 중요. 이게 true이면 inLineVert가 없는 셈이다.

					//OutVert가 InlineVert인 셈
					//newOut2InlineVertData._innerLineVert = null;
					newOut2InlineVertData._innerLineVert = curVert;

					curWrapGroup._out2InLineVerts.Add(newOut2InlineVertData);
				}
				
				//내부 라인 버텍스 데이터들을 서로 연결한다.
				curWrapGroup.LinkOut2InnerVertData();

				//외곽선 데이터를 그리드에 넣어서 아래의 처리를 가속화하자
				curWrapGroup.StoreOutlineEdgesToGrid();

				////원래는 여기서 내부라인 버텍스를 만들어야 하는데, 지금은 그러지 않아도 된다.
				////Cur > Next로 돈다.
				int nOut2InVertData = curWrapGroup._out2InLineVerts.Count;
				GenOutVert2InnerLineVerts curOut2InVertData = null;
				GenOutVert2InnerLineVerts nextOut2InVertData = null;
				for (int iData = 0; iData < nOut2InVertData; iData++)
				{
					curOut2InVertData = curWrapGroup._out2InLineVerts[iData];
					if (curOut2InVertData._nextData != null)
					{
						nextOut2InVertData = curOut2InVertData._nextData;
						GenVertex inlineVert_A = curOut2InVertData._innerLineVert;
						GenVertex inlineVert_B = nextOut2InVertData._innerLineVert;
						GenEdge newEdge = new GenEdge(inlineVert_A, inlineVert_B);

						curOut2InVertData._innerLineEdge_ToNext = newEdge;
						nextOut2InVertData._innerLineEdge_ToPrev = newEdge;
					}
				}

				for (int iData = 0; iData < nOut2InVertData; iData++)
				{
					curOut2InVertData = curWrapGroup._out2InLineVerts[iData];
					//다음의 처리를 위해 여기서 Min-Max Area를 생성하자
					curOut2InVertData.MakeMinMaxArea();
				}

				//여기서 작성된 버텍스와 선분들을 WrapGroup의 멤버로도 저장하자
				curWrapGroup.StoreVertexAndEdgeOfInlineData();
			}


			_subNextStep = SUB_STEP.Step8_MakeTriangles;
		}



		private void Processing_Step8_MakeTriangles_V2(bool isFirstFrame)
		{	
			//변경된 순서
			// Step 1 : 벡터 발사하고 내부로의 선분 만들기
			// 1. InLine Vertex에서 가이드 선분을 -Normal 방향으로 쏜다.
			// 2. 해당 InLine Vetex와 연결된 선분과 Vertex를 제외한 반대편 선분으로의 교차점을 찾는다.
			// 3. 해당 교차점에서 가장 가까운 Vertex를 찾아서 연결한다. 단, 해당 InLine Vertex와 직접 연결된 버텍스는 제외.
			// > 연결될 버텍스를 찾지 못했다면 이 가이드는 삭제한다.

			// Step 2 : 내부 선분에서 임시 내부점 생성하기
			// 1. 가이드 선분의 길이에 따라서 n분할을 한 뒤, 임시 내부 점들을 만든다.
			
			// Step 3 : 내부점 병합하여 실제 내부점 만들기
			// 1. 옵션으로 지정된 거리에 따라서 내부의 점들을 병합한다. 위치는 평균점
			// > 점들간에 선을 잇고, 이게 Inline을 침범해서는 안된다. 침범하는 경우는 별도로 분리
			// 2. 이 점들을 정식 내부 점(Inner Vert)들로 추가한다.
			
			// Step 4 : InLine Vert에서 선 연결하기
			// 1. InLine Vert에서 Normal 방향을 고려한 가장 가까운 다른 InLine Vert나 Inner Vert로 연결한다.
			// 2. InLine Vert간에 연결한 경우, 해당 InLine Vert에서는 추가로 연결하지 않는다.

			// Step 5 : 내부 점에서 선 연결하기
			// 1. Inner Vert들을 기준으로 연결 가능한 모든 선분을 찾는다. (대상은 다른 Inner Vert나 InLine Vert)
			// 2. 선분들을 거리 순서대로 정렬한다. (짧은게 앞으로)
			// 3. 조건에 맞으면 선분들을 계속 추가한다.
			// - 다른 Edge와 충돌하면 스킵
			// - 선택한 점에서 비슷한 방향으로 가는게 있다면 바로 처리하지 말고, "추가 선분 리스트"에 넣자.
			// 4. "추가 선분 리스트"가 있다면 추가한다. 정렬은 필요없다.
			// - 다른 Edge와 충돌하면 스킵

			// Step 6 : Relax를 한다.
			// 1. 내부 점들의 Edge들의 길이의 평균을 기준으로 -kx 공식을 적용하여 조금씩 Relax를 하자


			GenWrapGroup curGroup = null;
			float rayMaxSize = Mathf.Max(_curRequest._areaWidth, _curRequest._areaHeight) * 5.0f;

			//Debug.Log("RaMaxSize : " + rayMaxSize);

			bool isInnerMargin = _curRequest._option_IsInnerMargin;//내부 여백이 있는가 (변경 21.10.5 : Request에 넣자)


			//분할 정도는 각 그룹의 범위의 각 축의 합의 최대 길이를 밀집도로 나눈거
			//float maxAreaSide = 10.0f;
			float totalWidth = 0.0f;
			float totalHeight = 0.0f;
			for (int iGroup = 0; iGroup < _wrapGroups.Count; iGroup++)
			{
				curGroup = _wrapGroups[iGroup];
				totalWidth += curGroup._rectArea._width;
				totalHeight += curGroup._rectArea._height;
			}

			//maxAreaSide = Mathf.Max(totalWidth, totalHeight);

			//하드 코딩
			//float innerVertRange = maxAreaSide / 4.0f;//이거에 따라 밀집도가 다름.

			//옵션
			//float innerVertRange = maxAreaSide / Mathf.Max(_option_Inner_Density, 1.0f);//이거에 따라 밀집도가 다름.
			//float innerVertRange = _maxShortSizeOfGroups / (float)_option_Inner_Density;//이거에 따라 밀집도가 다름.
			//if(innerVertRange < 10.0f * _areaSizeRatio)
			//{
			//	innerVertRange = 10.0f * _areaSizeRatio;
			//}

			//float maxEdgeLengthForDivide = _maxShortSizeOfGroups / (float)_option_OutlineDivide_Density;

			
			//int optionInnerDensity = _option_Inner_Density;//이전
			int optionInnerDensity = _curRequest._option_Inner_Density;//변경 21.10.5

			for (int iGroup = 0; iGroup < _wrapGroups.Count; iGroup++)
			{
				curGroup = _wrapGroups[iGroup];

				float innerVertRange = curGroup._areaShortSize / (float)optionInnerDensity;//이거에 따라 밀집도가 다름.
				if (innerVertRange < 10.0f * curGroup._areaSizeRatio)
				{
					innerVertRange = 10.0f * curGroup._areaSizeRatio;
				}
				if (innerVertRange < 30.0f)
				{
					innerVertRange = 30.0f;
				}



				// Step 1 : 벡터 발사하고 내부로의 선분 만들기
				// 1. InLine Vertex에서 가이드 선분을 -Normal 방향으로 쏜다.
				// 2. 해당 InLine Vetex와 연결된 선분과 Vertex를 제외한 반대편 선분으로의 교차점을 찾는다.
				// 3. 해당 교차점에서 가장 가까운 Vertex를 찾아서 연결한다. 단, 해당 InLine Vertex와 직접 연결된 버텍스는 제외.
				// > 연결될 버텍스를 찾지 못했다면 이 가이드는 삭제한다.

				int nOut2InLineData = curGroup._out2InLineVerts.Count;


				GenOutVert2InnerLineVerts curData = null;
				GenVertex curInlineVert = null;
				GenEdge checkEdge = null;

				List<GenVertex> checkedInLineVerts = new List<GenVertex>();//이미 체크한 InLineVert는 제외하기 위함

				List<GenEdge> crossableEdges = null;


				//모든 InLineVert에서 Ray를 쏴버리면 너무 무거워진다.
				//위치 + 각도에 따라 그룹을 지어서, 이미 처리된 이후에는 Ray를 쏘지 않도록 하자
				//이미 연결한건 패스

				//Grid로 만들어서 적당한 지점의 Inline들만 계산하자
				//float areaMinSize = Mathf.Min(curGroup._rectArea._width, curGroup._rectArea._height);


				GenSimpleVertGrid grid_TmpVert = new GenSimpleVertGrid(innerVertRange, curGroup._rectArea._minX, curGroup._rectArea._minY);

				//완성된 내부 점들
				List<GenVertex> innerVerts = new List<GenVertex>();
				//내부점과 연결된 Edge의 개수


				try
				{
					for (int iData = 0; iData < nOut2InLineData; iData++)
					{
						//iLoop++;
						//if (iLoop > 100)
						//{
						//	Debug.LogError("Loop 제한 초과 1");
						//	break;
						//}

						curData = curGroup._out2InLineVerts[iData];
						curInlineVert = curData._innerLineVert;

						if (curInlineVert == null)
						{
							//Debug.LogError("에러 : InLine Vert가 없다.");
							continue;
						}
						if (!curGroup._vertices_Inline.Contains(curInlineVert))
						{
							//Debug.LogError("실패 : 유효하지 않은 InLine Vert");
							continue;
						}
						if (checkedInLineVerts.Contains(curInlineVert))
						{
							//Debug.LogError("실패 : 이미 사용한 InLine Vert");
							continue;
						}
						//중복 체크 안하도록
						checkedInLineVerts.Add(curInlineVert);

						//InLineVert의 위치와 Ray의 끝점을 찾는다.
						Vector2 curPosA = curInlineVert._pos;
						Vector2 curPosB = curInlineVert._pos - curInlineVert._dir * rayMaxSize;//dir의 - 방향으로 이동


						//이제 가장 가까운 교차점을 찾는다.
						bool isMinCross = false;
						Vector2 minCrossPos = Vector2.zero;
						float minCrossDist = 0.0f;
						//GenEdge minCrossEdge = null;

						//그리드로부터 교차 가능한 선들을 가져온다.
						crossableEdges = curGroup._grid_lineEdgesForCrossCheck.GetEdges(curPosA, curPosB);
						int nInLineEdges = crossableEdges.Count;

						for (int iEdge = 0; iEdge < nInLineEdges; iEdge++)
						{
							checkEdge = crossableEdges[iEdge];

							//현재 버텍스와 연결된거면 패스
							if (checkEdge._vert_A == curInlineVert
								|| checkEdge._vert_B == curInlineVert)
							{
								continue;
							}

							GenEdgeCrossResult result = GenEdge.CheckCrossApprox(curPosA, curPosB, checkEdge._vert_A._pos, checkEdge._vert_B._pos);
							if (result._result == GenEdgeCrossResult.RESULT_TYPE.NoCross)
							{
								//교차하지 않음
								continue;
							}

							Vector2 crossPos = result._pos;
							float crossDist = Vector2.Distance(curPosA, crossPos);

							//최단 거리를 찾는다.
							if (!isMinCross || crossDist < minCrossDist)
							{
								isMinCross = true;
								minCrossDist = crossDist;
								minCrossPos = crossPos;
								//minCrossEdge = checkEdge;
							}
						}

						if (!isMinCross)
						{
							//교차점이 없다. 이 InLineVert에서는 추가적인 처리를 하지 않는다.
							//Debug.LogError("에러 : 교차점이 없다");
							continue;
						}

						//그냥 Edge의 교차점을 연결

						Vector2 targetPos = minCrossPos;

						// Step 2 : 내부 선분에서 임시 내부점 생성하기
						// 1. 가이드 선분의 길이에 따라서 n분할을 한 뒤, 임시 내부 점들을 만든다.
						float edgeLength = Vector2.Distance(curInlineVert._pos, targetPos);
						if (edgeLength < innerVertRange)
						{
							//분할하기에 너무 짧다면 그냥 스킵
							continue;
						}
						//분할을 하자
						Vector2 posA = curInlineVert._pos;
						Vector2 posB = targetPos;
						Vector2 posSplit = Vector2.zero;
						int nSplit = (int)(edgeLength / innerVertRange) + 1;


						//float sqrMinNearRange = (_option_InnerMargin + _option_OuterMargin);//이전
						float sqrMinNearRange = (_curRequest._option_InnerMargin + _curRequest._option_OuterMargin);//변경 21.10.5
						sqrMinNearRange *= sqrMinNearRange;

						for (int iSplit = 1; iSplit < nSplit; iSplit++)
						{
							float lerp = (float)iSplit / (float)nSplit;
							posSplit = (posA * (1.0f - lerp)) + (posB * lerp);

							// 단, innerMargin + outerMargin 만큼의 거리 이내에 InLineVert나 OutVert가 있어서는 안된다.
							bool isNearInLineVert = curGroup._vertices_Inline.Exists(delegate (GenVertex a)
							{
								return Vector2.SqrMagnitude(a._pos - posSplit) < sqrMinNearRange;
							});

							//가까운 InLineVert가 있어서 생성 실패
							if (isNearInLineVert)
							{
								continue;
							}

							bool isNearOutlineVert = curGroup._vertices_Outline.Exists(delegate (GenVertex a)
							{
								return Vector2.SqrMagnitude(a._pos - posSplit) < sqrMinNearRange;
							});

							//가까운 Outline Vert가 있어서 실패
							if (isNearOutlineVert)
							{
								continue;
							}

							//바로 그리드에 버텍스를 넣자
							grid_TmpVert.AddVert(new GenVertex(posSplit/*, VERT_TYPE.Inner*/));
						}

						if (IsTimeout())
						{
							//타임 아웃이라면
							_curRequest.SetFailed("Processing was terminated due to timeout.");
							ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
							return;
						}
					}
				}
				catch (Exception ex)
				{
					//Debug.LogError("에러 : " + ex.ToString());
					_curRequest.SetFailed("Error is occured : " + ex.ToString());
					ChangeState(PROCESS_STATE.Step4_Complete_AllFailed);
					return;
				}

				// Step 3 : 내부점 병합하여 실제 내부점 만들기
				// 1. 옵션으로 지정된 거리에 따라서 내부의 점들을 병합한다. 위치는 평균점
				// > 점들간에 선을 잇고, 이게 Inline을 침범해서는 안된다. 침범하는 경우는 별도로 분리
				// 2. 이 점들을 정식 내부 점(Inner Vert)들로 추가한다.

				//Debug.Log("내부 점들 : " + grid_TmpVert._nVerts);

				GenVertex curTmpVert = null;
				List<GenVertex> curTmpVertList = null;
				List<GenVertex> remainVerts = new List<GenVertex>();
				List<GenVertex> notMergedVertices = new List<GenVertex>();

				foreach (KeyValuePair<int, Dictionary<int, List<GenVertex>>> gridItemPerX in grid_TmpVert._grid)
				{
					foreach (KeyValuePair<int, List<GenVertex>> gridItemPerY in gridItemPerX.Value)
					{
						curTmpVertList = gridItemPerY.Value;

						//병합이 하나로 안될 수 있으므로, 
						//루프를 돌면서 추가한다.

						//일단 루프에 넣기 위한 리스트를 작성
						remainVerts.Clear();
						notMergedVertices.Clear();
						for (int iVert = 0; iVert < curTmpVertList.Count; iVert++)
						{
							remainVerts.Add(curTmpVertList[iVert]);
							//innerVerts.Add(curTmpVertList[iVert]);//테스트 : 병합 전의 InnerVert를 보고자 할 때
						}

						while (true)
						{
							if (remainVerts.Count == 0)
							{
								break;
							}

							//첫번째 버텍스를 꺼낸다.
							GenVertex startVertex = remainVerts[0];
							remainVerts.RemoveAt(0);

							Vector2 avgPos = startVertex._pos;
							int nAvg = 1;

							if (remainVerts.Count > 0)
							{
								//하나씩 꺼내면서 병합한다.
								//StartVertex와의 연결시 외곽선과 교차한다면 패스
								notMergedVertices.Clear();

								for (int iRemainVert = 0; iRemainVert < remainVerts.Count; iRemainVert++)
								{
									curTmpVert = remainVerts[iRemainVert];

									bool isValid = false;

									float dist = Vector2.Distance(startVertex._pos, curTmpVert._pos);

									if (dist < 1.0f)
									{
										//적당히 가까운거는 그냥 무시하고 병합
										isValid = true;
									}
									else
									{
										//거리가 멀다면 체크를 하자
										//선을 긋고 교차점 여부를 체크한다.
										crossableEdges = curGroup._grid_lineEdgesForCrossCheck.GetEdges(startVertex._pos, curTmpVert._pos);
										int nInLineEdges = crossableEdges.Count;

										bool isAnyCross = false;
										for (int iEdge = 0; iEdge < nInLineEdges; iEdge++)
										{
											checkEdge = crossableEdges[iEdge];

											//현재 버텍스와 연결된거면 패스
											if (checkEdge._vert_A == curInlineVert
												|| checkEdge._vert_B == curInlineVert)
											{
												continue;
											}

											GenEdgeCrossResult result = GenEdge.CheckCrossApprox(startVertex._pos, curTmpVert._pos, checkEdge._vert_A._pos, checkEdge._vert_B._pos);
											if (result._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
											{
												//교차하는걸 찾았다.
												isAnyCross = true;
												break;
											}
										}

										if (isAnyCross)
										{
											isValid = false;
										}
										else
										{
											isValid = true;
										}
									}

									if (isValid)
									{
										//유효하다면 평균에 추가
										avgPos += curTmpVert._pos;
										nAvg += 1;
									}
									else
									{
										//유효하지 않다면, 이건 다른 평균에 추가
										//notMergedVertices.Add(curTmpVert);
										//Debug.Log("가깝지만 유효하지 않은 버텍스 발견 : " + curTmpVert._pos);
									}

								}

								//일단 RemainVert는 Clear
								//병합이 안된게 있다면 RemainVert에 넣는다.
								remainVerts.Clear();
								if (notMergedVertices.Count > 0)
								{
									for (int iNotMerge = 0; iNotMerge < notMergedVertices.Count; iNotMerge++)
									{
										remainVerts.Add(notMergedVertices[iNotMerge]);
									}
								}
							}

							//평균점을 계산하여 Inner Vert에 추가한다.
							if (nAvg > 1)
							{
								avgPos /= nAvg;
							}




							innerVerts.Add(new GenVertex(avgPos/*, VERT_TYPE.Inner*/));

							if (IsTimeout())
							{
								//타임 아웃이라면
								_curRequest.SetFailed("Processing was terminated due to timeout.");
								ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
								return;
							}
						}
					}
				}

				//내부에서 연결 가능한 모든 선분 리스트
				List<GenEdge> connectableEdges = new List<GenEdge>();
				//List<GenEdge> notConnectedEdges = new List<GenEdge>();//실패한 Edge들을 모아서 나중에 한번 더 시도한다.
				GenEdgeCache innerEdgeCache = new GenEdgeCache();


				// Step 4 : InLine Vert에서 선 연결하기
				// 1. InLine Vert에서 Normal 방향을 고려한 가장 가까운 다른 InLine Vert나 Inner Vert로 연결한다.
				// 2. InLine Vert간에 연결한 경우, 해당 InLine Vert에서는 추가로 연결하지 않는다.

				//만약 Inner Margin이 없는 경우, InLineVert간의 연결은 불가하다.

				int nInnerVerts = innerVerts.Count;
				int nInLineVerts = curGroup._vertices_Inline.Count;


				//중복 체크 리스트 초기화
				checkedInLineVerts.Clear();

				for (int iData = 0; iData < nOut2InLineData; iData++)
				{
					curData = curGroup._out2InLineVerts[iData];
					curInlineVert = curData._innerLineVert;

					if (curInlineVert == null
						|| !curGroup._vertices_Inline.Contains(curInlineVert)
						|| checkedInLineVerts.Contains(curInlineVert)
						)
					{
						continue;
					}

					//중복 체크 안하도록
					checkedInLineVerts.Add(curInlineVert);

					GenVertex linkedPrevVert = null;
					GenVertex linkedNextVert = null;
					if (curData._innerLineEdge_ToPrev != null)
					{
						if (curData._innerLineEdge_ToPrev._vert_A == curInlineVert) { linkedPrevVert = curData._innerLineEdge_ToPrev._vert_B; }
						else { linkedPrevVert = curData._innerLineEdge_ToPrev._vert_A; }
					}
					if (curData._innerLineEdge_ToNext != null)
					{
						if (curData._innerLineEdge_ToNext._vert_A == curInlineVert) { linkedNextVert = curData._innerLineEdge_ToNext._vert_B; }
						else { linkedNextVert = curData._innerLineEdge_ToNext._vert_A; }
					}

					//이 InLineVert에서 연결 가능한 가장 가까운 InLine / Inner Vert를 찾자
					//- 외곽선과 교차되면 안된다.
					//- 거리 가중치는 Cos(Angle)

					Vector2 curPosA = curInlineVert._pos;
					Vector2 curPosB = curInlineVert._pos - (curInlineVert._dir * rayMaxSize);
					Vector2 curLineDir = curPosB - curPosA;

					GenVertex otherVert = null;

					//InLineVert간 1차 연결은 내부 여백이 있는 경우에만 가능
					if (isInnerMargin)
					{
						//먼저 다른 InLine Vert와 비교하자
						for (int iOtherVert = 0; iOtherVert < nInLineVerts; iOtherVert++)
						{
							otherVert = curGroup._vertices_Inline[iOtherVert];

							if (otherVert == curInlineVert
								|| otherVert == linkedPrevVert
								|| otherVert == linkedNextVert)
							{
								//동일 / 인접 버텍스라면 제외
								continue;
							}

							if (innerEdgeCache.IsContain(curInlineVert, otherVert))
							{
								//이미 추가되었다면
								continue;
							}

							Vector2 cur2Other = otherVert._pos - curPosA;

							//거리와 각도, Cos 거리를 확인하자
							float dist = cur2Other.magnitude;
							float angle = Vector2.Angle(curLineDir, cur2Other);
							if (angle > 90.0f)
							{
								//너무 많이 벌어졌다.
								//continue;
								angle = 90.0f;
								//없애지는 말자

							}

							//충돌 여부 확인
							if (curGroup.IsCrossWithInLines(curInlineVert, otherVert))
							{
								//다른 선에 걸린다.
								////실패에 추가한다.
								//notConnectedEdges.Add(new GenEdge(curInlineVert, otherVert));
								continue;
							}

							//추가하자

							//가중치도 설정
							//가중치는 클수록 잘 연결이 안된다.
							//- InLineVert간의 가중치는 2.0배
							//- -Normal과 Edge의 각도가 좁으면 1.0, 각도가 90도에 가까우면 5.0 (이건 양쪽 Vert와 확인)

							float weight = 2.0f;//기본 2.0


							float angleOther = Mathf.Clamp(Vector2.Angle(-otherVert._dir, -cur2Other), 0.0f, 90.0f);
							float angleRatio_Cur = 1.0f - Mathf.Cos(Mathf.Abs(angle * Mathf.Deg2Rad));

							float angleRatio_Other = 1.0f - Mathf.Cos(Mathf.Abs(angleOther * Mathf.Deg2Rad));

							float weight_NormalCur = 1.0f * (1.0f - angleRatio_Cur) + (3.0f * angleRatio_Cur);
							float weight_NormalOther = 1.0f * (1.0f - angleRatio_Other) + (3.0f * angleRatio_Other);
							weight *= weight_NormalCur;
							weight *= weight_NormalOther;

							//Debug.Log("angle : " + angle + " > " + weight_NormalCur + " / Other : " + angleOther + " > " + weight_NormalOther);

							GenEdge newEdge = new GenEdge(curInlineVert, otherVert);
							newEdge._weight = weight;

							connectableEdges.Add(newEdge);//여기에 넣고 나중에 연결을 시도한다.
							innerEdgeCache.AddEdge(newEdge);
						}
					}
					


					//이번엔 내부 점들과의 연결 정보를 찾자
					otherVert = null;

					for (int iOtherVert = 0; iOtherVert < nInnerVerts; iOtherVert++)
					{
						otherVert = innerVerts[iOtherVert];

						if (innerEdgeCache.IsContain(curInlineVert, otherVert))
						{
							//이미 추가되었다면
							continue;
						}

						Vector2 cur2Other = otherVert._pos - curPosA;

						//거리와 각도, Cos 거리를 확인하자
						float dist = cur2Other.magnitude;
						float angle = Vector2.Angle(curLineDir, cur2Other);
						if (angle > 90.0f)
						{
							//너무 많이 벌어졌다.
							continue;
							//angle = 90.0f;
							//없애지는 말자
						}
						//float cosDist = dist * Mathf.Cos(angle * Mathf.Deg2Rad);

						//충돌 여부 확인
						if (curGroup.IsCrossWithInLines(curInlineVert, otherVert))
						{
							//다른 선에 걸린다.
							////실패에 추가한다.
							//notConnectedEdges.Add(new GenEdge(curInlineVert, otherVert));
							continue;
						}

						//가중치도 설정
						//가중치는 클수록 잘 연결이 안된다.
						//- InLineVert - Inner간의 가중치는 1.0배
						//- -Normal과 Edge의 각도가 좁으면 1.0, 각도가 90도에 가까우면 5.0
						float weight = 1.5f;//기본 1.0
						float angleRatio_Cur = 1.0f - Mathf.Cos(Mathf.Abs(angle * Mathf.Deg2Rad));
						float weight_NormalCur = 1.0f * (1.0f - angleRatio_Cur) + (3.0f * angleRatio_Cur);
						weight *= weight_NormalCur;


						GenEdge newEdge = new GenEdge(curInlineVert, otherVert);
						newEdge._weight = weight;

						connectableEdges.Add(newEdge);//여기에 넣고 나중에 연결을 시도한다.
						innerEdgeCache.AddEdge(newEdge);
					}



					//InLineVert간 2차 연결은 내부 여백이 있는 없는 경우에만 가능. 단 각도 비교해야한다. (Normal의 90이상 벌어지면 취소한다)
					if (!isInnerMargin)
					{
						//먼저 다른 InLine Vert와 비교하자
						for (int iOtherVert = 0; iOtherVert < nInLineVerts; iOtherVert++)
						{
							otherVert = curGroup._vertices_Inline[iOtherVert];

							if (otherVert == curInlineVert
								|| otherVert == linkedPrevVert
								|| otherVert == linkedNextVert)
							{
								//동일 / 인접 버텍스라면 제외
								continue;
							}

							if (innerEdgeCache.IsContain(curInlineVert, otherVert))
							{
								//이미 추가되었다면
								continue;
							}

							Vector2 cur2Other = otherVert._pos - curPosA;

							//거리와 각도, Cos 거리를 확인하자
							float dist = cur2Other.magnitude;

							//내부 여백이 있다면
							//float angle = Vector2.Angle(curLineDir, cur2Other);
							//if (angle > 90.0f)
							//{
							//	//너무 많이 벌어졌다.
							//	//continue;
							//	angle = 90.0f;
							//	//없애지는 말자
							//}

							//내부 여백이 없다면
							float angle = Vector2.Angle(curLineDir, cur2Other);
							if (angle > 90.0f || Vector2.Dot(curLineDir, cur2Other) < 0.0f)
							{
								//너무 많이 벌어졌다.
								continue;
							}

							//충돌 여부 확인
							if (curGroup.IsCrossWithInLines(curInlineVert, otherVert))
							{
								//다른 선에 걸린다.
								continue;
							}

							//이 조건에서는 InLineVert와 OutVert가 동일하기 때문에
							//"바깥으로의 연결"을 감지하기가 어렵다.
							//어쩔 수 없이 여기서는 다시 "색상 체크"를 해야한다.
							//4픽셀 단위로 한다.
							int nHitPoints = (int)(dist / 4.0f);
							if(nHitPoints < 4)
							{
								nHitPoints = 4;
							}
							if(!_curRequest.ImageHitTestLine_AnyHitTrue(curGroup, curPosA, otherVert._pos, nHitPoints))
							{
								//하나라도 Filled이면 내부
								//그 반대는 외부								
								continue;
							}


							//추가하자




							//가중치도 설정
							//가중치는 클수록 잘 연결이 안된다.
							//- InLineVert간의 가중치는 2.0배
							//- -Normal과 Edge의 각도가 좁으면 1.0, 각도가 90도에 가까우면 5.0 (이건 양쪽 Vert와 확인)

							float weight = 2.0f;//기본 2.0


							float angleOther = Mathf.Clamp(Vector2.Angle(-otherVert._dir, -cur2Other), 0.0f, 90.0f);
							float angleRatio_Cur = 1.0f - Mathf.Cos(Mathf.Abs(angle * Mathf.Deg2Rad));

							float angleRatio_Other = 1.0f - Mathf.Cos(Mathf.Abs(angleOther * Mathf.Deg2Rad));

							float weight_NormalCur = 1.0f * (1.0f - angleRatio_Cur) + (3.0f * angleRatio_Cur);
							float weight_NormalOther = 1.0f * (1.0f - angleRatio_Other) + (3.0f * angleRatio_Other);
							weight *= weight_NormalCur;
							weight *= weight_NormalOther;

							//Debug.Log("angle : " + angle + " > " + weight_NormalCur + " / Other : " + angleOther + " > " + weight_NormalOther);

							GenEdge newEdge = new GenEdge(curInlineVert, otherVert);
							newEdge._weight = weight;

							connectableEdges.Add(newEdge);//여기에 넣고 나중에 연결을 시도한다.
							innerEdgeCache.AddEdge(newEdge);
						}
					}
				}

				//이제 connectableEdges에는 각 InLine Vert당 2개의 최소 연결 정보가 포함되어있다.



				// Step 5 : 내부 점에서 선 연결하기
				// 1. Inner Vert들을 기준으로 연결 가능한 모든 선분을 찾는다. (대상은 다른 Inner Vert나 InLine Vert)

				GenVertex curInnerVert = null;

				//추가로, 다음의 Relax를 위해서 "초기 위치"를 저장하자
				Dictionary<GenVertex, Vector2> innerVert2OrgPos = new Dictionary<GenVertex, Vector2>();


				for (int iVert = 0; iVert < nInnerVerts; iVert++)
				{
					curInnerVert = innerVerts[iVert];

					//초기 위치 저장
					innerVert2OrgPos.Add(curInnerVert, curInnerVert._pos);

					//이건 모든 방향에 대해서 연결 가능한 모든 선분을 찾는다.
					GenVertex otherVert = null;

					//먼저 다른 InLine Vert와 비교하자
					for (int iOtherVert = 0; iOtherVert < nInLineVerts; iOtherVert++)
					{
						otherVert = curGroup._vertices_Inline[iOtherVert];
						if (innerEdgeCache.IsContain(curInnerVert, otherVert))
						{
							//이미 등록된거면 패스
							continue;
						}

						//충돌 여부 확인
						if (curGroup.IsCrossWithInLines(curInnerVert, curInnerVert._pos, otherVert._pos))
						{
							//다른 선에 걸린다.
							////실패에 추가한다.
							//notConnectedEdges.Add(new GenEdge(curInnerVert, otherVert));
							continue;
						}

						//가중치도 설정
						//가중치는 클수록 잘 연결이 안된다.
						//- InLineVert - Inner간의 가중치는 1.2배
						//- -Normal과 Edge의 각도가 좁으면 1.0, 각도가 90도에 가까우면 5.0 (이건 양쪽 Vert와 확인)

						float weight = 1.5f;//기본 1.2
						float angleOther = Mathf.Clamp(Vector2.Angle(-otherVert._dir, (curInnerVert._pos - otherVert._pos)), 0.0f, 90.0f);
						float angleRatio_Other = 1.0f - Mathf.Cos(Mathf.Abs(angleOther * Mathf.Deg2Rad));
						float weight_NormalOther = 1.0f * (1.0f - angleRatio_Other) + (3.0f * angleRatio_Other);
						weight *= weight_NormalOther;

						//Debug.Log("Other Angle : " + angleOther + "/(" + (-otherVert._dir) + ", " + (curInnerVert._pos - otherVert._pos) + ") >> Weight : " + weight_NormalOther);

						//추가하자
						GenEdge newEdge = new GenEdge(curInnerVert, otherVert);
						newEdge._weight = weight;

						connectableEdges.Add(newEdge);//여기에 넣고 나중에 연결을 시도한다.
						innerEdgeCache.AddEdge(newEdge);
					}

					//이번엔 내부 점들과의 연결 정보를 찾자
					otherVert = null;

					for (int iOtherVert = 0; iOtherVert < nInnerVerts; iOtherVert++)
					{
						otherVert = innerVerts[iOtherVert];
						if (curInnerVert == otherVert)
						{
							//같은거다
							continue;
						}
						if (innerEdgeCache.IsContain(curInnerVert, otherVert))
						{
							//이미 등록된거면 패스
							continue;
						}

						//충돌 여부 확인
						if (curGroup.IsCrossWithInLines(curInnerVert, curInnerVert._pos, otherVert._pos))
						{
							//다른 선에 걸린다.
							////실패에 추가한다.
							//notConnectedEdges.Add(new GenEdge(curInnerVert, otherVert));
							continue;
						}

						//추가하자
						GenEdge newEdge = new GenEdge(curInnerVert, otherVert);
						newEdge._weight = 1.0f;//Inner간의 연결은 1.0f


						connectableEdges.Add(newEdge);//여기에 넣고 나중에 연결을 시도한다.
						innerEdgeCache.AddEdge(newEdge);
					}
				}


				//이제 선분들을 정렬하고 연결한다.
				// 2. 선분들을 거리 순서대로 정렬한다. (짧은게 앞으로)
				connectableEdges.Sort(delegate (GenEdge a, GenEdge b)
				{
					float lengthA = Vector2.Distance(a._vert_A._pos, a._vert_B._pos) * a._weight;
					float lengthB = Vector2.Distance(b._vert_A._pos, b._vert_B._pos) * b._weight;

					return (int)((lengthA - lengthB) * 10.0f);
				});

				//결과 Edge
				List<GenEdge> innerEdges = new List<GenEdge>();
				Dictionary<GenVertex, List<GenEdge>> vert2Edges = new Dictionary<GenVertex, List<GenEdge>>();//Relax를 위한 매핑 리스트

				// 3. 조건에 맞으면 선분들을 계속 추가한다.
				// - 다른 Edge와 충돌하면 스킵
				// - 선택한 점에서 비슷한 방향으로 가는게 있다면 바로 처리하지 말고, "추가 선분 리스트"에 넣자.
				// 4. "추가 선분 리스트"가 있다면 추가한다. 정렬은 필요없다.
				// - 다른 Edge와 충돌하면 스킵
				GenEdge curEdge = null;
				int nConnEdges = connectableEdges.Count;
				for (int iEdge = 0; iEdge < nConnEdges; iEdge++)
				{
					curEdge = connectableEdges[iEdge];

					//연결 가능하면 계속 추가하면서 연결하자
					if (curGroup.IsCrossWithInLines(curEdge._vert_A, curEdge._vert_B, true))//정밀 체크 옵션을 켠다.
					{
						//무효한 Edge이므로 삭제
						curEdge._vert_A._linkedEdges.Remove(curEdge);
						curEdge._vert_B._linkedEdges.Remove(curEdge);
						continue;
					}

					//성공시 연결
					innerEdges.Add(curEdge);

					//더 연결 못하도록 리스트에 추가한다.
					curGroup._grid_lineEdgesForCrossCheck.AddEdge(curEdge);

					//매핑 데이터도 추가
					if (!vert2Edges.ContainsKey(curEdge._vert_A))
					{
						vert2Edges.Add(curEdge._vert_A, new List<GenEdge>());
					}
					if (!vert2Edges.ContainsKey(curEdge._vert_B))
					{
						vert2Edges.Add(curEdge._vert_B, new List<GenEdge>());
					}
					vert2Edges[curEdge._vert_A].Add(curEdge);
					vert2Edges[curEdge._vert_B].Add(curEdge);
				}
				connectableEdges.Clear();


				//Step 5.5 : 연결이 되다만 점들이 있다.
				//이건 미리 삭제하자
				//Edge가 1개 혹은 0개인 Vertex가 발견되면, 해당 Vert와 Edge를 삭제리스트에 넣고 삭제한다.
				//추가 : Edge가 2개인 Vertex이면서 해당 2개의 Edge들의 Prev, Next점들을 잇는 Edge가 존재할 때 삭제 (단 이 경우는 모든 Edge를 찾아야 한다.)
				//이 경우는 다른걸 삭제한 이후에 판단한다.
				//하나라도 삭제하면 해당 과정을 반복한다.
				List<GenVertex> misConnectedVerts = new List<GenVertex>();
				List<GenEdge> misConnectedEdges = new List<GenEdge>();

				List<GenVertex> skippableVerts = new List<GenVertex>();
				List<GenEdge> skippableEdges = new List<GenEdge>();

				int nRemoved = 0;
				int nSkipped = 0;
				while (true)
				{
					if (innerVerts.Count == 0)
					{
						break;
					}
					bool isAnyRemoved = false;
					bool isAnySkipped = false;

					misConnectedVerts.Clear();
					misConnectedEdges.Clear();
					skippableVerts.Clear();
					skippableEdges.Clear();

					for (int iVert = 0; iVert < innerVerts.Count; iVert++)
					{
						curInnerVert = innerVerts[iVert];
						if (curInnerVert._linkedEdges.Count > 2)
						{
							continue;
						}
						if (curInnerVert._linkedEdges.Count == 2)
						{
							GenEdge linkedEdge1 = curInnerVert._linkedEdges[0];
							GenEdge linkedEdge2 = curInnerVert._linkedEdges[1];
							GenVertex linkedVert1 = (linkedEdge1._vert_A == curInnerVert) ? linkedEdge1._vert_B : linkedEdge1._vert_A;
							GenVertex linkedVert2 = (linkedEdge2._vert_A == curInnerVert) ? linkedEdge2._vert_B : linkedEdge2._vert_A;

							//linkedVert1, 2로 구성된 InnerEdge 또는 InLine-Prev/Next Edge가 있는지 찾는다.
							bool isSkippable = innerEdgeCache.IsContain(linkedVert1, linkedVert2);//Inner에서 발견
							if (!isSkippable)
							{
								//스킵 가능한가
								isSkippable = curGroup._edges_InterInlineVerts.Exists(delegate (GenEdge a)
								{
									return a.IsSame(linkedVert1, linkedVert2);
								});
							}
							if (isSkippable)
							{
								isAnySkipped = true;
								skippableVerts.Add(curInnerVert);
								skippableEdges.Add(linkedEdge1);
								skippableEdges.Add(linkedEdge2);
							}
						}
						else
						{
							//유효하지 않는 버텍스이다. 삭제하자
							misConnectedVerts.Add(curInnerVert);
							if (curInnerVert._linkedEdges.Count == 1)
							{
								misConnectedEdges.Add(curInnerVert._linkedEdges[0]);//해당 Edge도 삭제
							}
							isAnyRemoved = true;
						}

					}

					if (!isAnyRemoved && !isAnySkipped)
					{
						break;
					}

					if (isAnyRemoved)
					{
						//삭제를 하자 (Edge부터)
						for (int iEdge = 0; iEdge < misConnectedEdges.Count; iEdge++)
						{
							curEdge = misConnectedEdges[iEdge];
							curEdge._vert_A._linkedEdges.Remove(curEdge);
							curEdge._vert_B._linkedEdges.Remove(curEdge);

							innerEdges.Remove(curEdge);
							innerEdgeCache.RemoveEdge(curEdge);
							if (vert2Edges.ContainsKey(curEdge._vert_A))
							{
								vert2Edges[curEdge._vert_A].Remove(curEdge);
							}
							if (vert2Edges.ContainsKey(curEdge._vert_B))
							{
								vert2Edges[curEdge._vert_B].Remove(curEdge);
							}
							nRemoved++;
						}

						for (int iVert = 0; iVert < misConnectedVerts.Count; iVert++)
						{
							curInnerVert = misConnectedVerts[iVert];
							innerVerts.Remove(curInnerVert);
							innerVert2OrgPos.Remove(curInnerVert);
							nRemoved++;
						}
					}
					else if (isAnySkipped)
					{
						//Skip은 Remove가 발생하지 않는 경우에만
						for (int iEdge = 0; iEdge < skippableEdges.Count; iEdge++)
						{
							curEdge = skippableEdges[iEdge];
							curEdge._vert_A._linkedEdges.Remove(curEdge);
							curEdge._vert_B._linkedEdges.Remove(curEdge);

							innerEdges.Remove(curEdge);
							innerEdgeCache.RemoveEdge(curEdge);
							if (vert2Edges.ContainsKey(curEdge._vert_A))
							{
								vert2Edges[curEdge._vert_A].Remove(curEdge);
							}
							if (vert2Edges.ContainsKey(curEdge._vert_B))
							{
								vert2Edges[curEdge._vert_B].Remove(curEdge);
							}
							nSkipped++;
						}

						for (int iVert = 0; iVert < skippableVerts.Count; iVert++)
						{
							curInnerVert = skippableVerts[iVert];
							innerVerts.Remove(curInnerVert);
							innerVert2OrgPos.Remove(curInnerVert);
							nSkipped++;
						}

					}
				}
				if (nRemoved > 0)
				{
					//Debug.LogError("유효하지 않은 버텍스/선분 삭제 : " + nRemoved);
				}
				if (nSkipped > 0)
				{
					//Debug.LogError("생략 가능한 버텍스/선분 삭제 : " + nSkipped);
				}

				

				// Step 6 : Relax를 한다. (옵션)
				// 1. 내부 점들의 Edge들의 길이의 평균을 기준으로 -kx 공식을 적용하여 조금씩 Relax를 하자

				//하드 코딩
				int nReleaxTry = 7;//3번 하자 <옵션>
				//nReleaxTry = 0;
				float relaxIntensity = 0.1f;//Relax 비율 <옵션>
				//많이 이동하는건 아니다.
				//초기 위치로부터 최대 이동 길이와 매 Try당 이동하는 길이(Max 힘에서의 이동 길이)를 정하자
				float maxMoveLength = curGroup._areaShortSize / 5.0f;
				float maxMoveLengthPerTry = maxMoveLength / 2.0f;

				//Debug.Log("Try당 이동 가능 거리 : " + maxMoveLengthPerTry);
				//Debug.Log("최대 이동 가능 거리 : " + maxMoveLength);
					
				List<GenEdge> linkedEdges = null;
				GenEdge linkedEdge = null;

				//int nRelax = 0;
				for (int iTry = 0; iTry < nReleaxTry; iTry++)
				{
					//Debug.LogWarning("Relax [" + iTry + "]");
					for (int iInVert = 0; iInVert < innerVerts.Count; iInVert++)
					{
						curInnerVert = innerVerts[iInVert];
						if (!vert2Edges.ContainsKey(curInnerVert))
						{
							continue;
						}
						linkedEdges = vert2Edges[curInnerVert];
						if (linkedEdges.Count <= 2)
						{
							continue;
						}

						//초기 위치
						Vector2 orgPos = innerVert2OrgPos[curInnerVert];

						int nLinkedEdges = linkedEdges.Count;

						//Relax를 위해서 일단 평균 길이를 구한다.
						float avgLength = 0.0f;
						for (int iLink = 0; iLink < nLinkedEdges; iLink++)
						{
							linkedEdge = linkedEdges[iLink];
							avgLength += Vector2.Distance(linkedEdge._vert_A._pos, linkedEdge._vert_B._pos);
						}
						avgLength /= nLinkedEdges;

						//힘의 합력으로 이동할 거리를 구하자
						//AvgLength보다 길면 당기고, 짧으면 민다.
						Vector2 sumRelax = Vector2.zero;
						float linkedLength = 0.0f;
						Vector2 linkedDir = Vector2.zero;

						for (int iLink = 0; iLink < nLinkedEdges; iLink++)
						{
							linkedEdge = linkedEdges[iLink];
							linkedLength = Vector2.Distance(linkedEdge._vert_A._pos, linkedEdge._vert_B._pos);

							if (linkedEdge._vert_A == curInnerVert)
							{
								linkedDir = linkedEdge._vert_B._pos - linkedEdge._vert_A._pos;
							}
							else if (linkedEdge._vert_B == curInnerVert)
							{
								linkedDir = linkedEdge._vert_A._pos - linkedEdge._vert_B._pos;
							}
							sumRelax += linkedDir.normalized * (linkedLength - avgLength);
						}

						//거리가 너무 먼건 아닌지 확인
						if(sumRelax.magnitude > maxMoveLengthPerTry)
						{
							sumRelax = sumRelax.normalized * maxMoveLengthPerTry;
						}

						//위치를 이동시킨다.
						//Debug.Log("Relax : " + curInnerVert._pos + " + " + (sumRelax * relaxIntensity));

						//움직일 해당 위치가 유효한지 판단한다.
						Vector2 simulatePos = curInnerVert._pos + sumRelax * relaxIntensity;

						//기존 위치에서 너무 많이 움직였다면
						if(Vector2.Distance(simulatePos, orgPos) > maxMoveLength)
						{
							simulatePos = orgPos + (simulatePos - orgPos).normalized * maxMoveLength;
						}

						if (curGroup.IsCrossWithInLines_MultiLineSimulate(curInnerVert, simulatePos, linkedEdges))
						{
							//충돌했는데용
							//Debug.Log("이동 불가 : " + curInnerVert._pos + " >> " + simulatePos);
							continue;
						}

						//이동 가능
						curInnerVert._pos = simulatePos;

					}
				}

				//------------------------------------------
				//다 끝나면 Mesh 생성을 위한 데이터로 전환
				//curGroup.StoreVertexAndEdgeOfInnerGroup();

				curGroup._vertices_Inner.Clear();
				curGroup._edges_Inner.Clear();

				for (int iInVert = 0; iInVert < innerVerts.Count; iInVert++)
				{
					curGroup._vertices_Inner.Add(innerVerts[iInVert]);
				}

				for (int iInEdge = 0; iInEdge < innerEdges.Count; iInEdge++)
				{
					curGroup._edges_Inner.Add(innerEdges[iInEdge]);
				}
			}


			//다음 단계로 넘어가자
			_subNextStep = SUB_STEP.Step9_ApplyToMesh;
		}





		//Sub Step 9 : 완성된 데이터들을 메시에 반영한다. (서브루틴 끗!)
		private void Processing_Step9_ApplyToMesh(bool isFirstFrame)
		{	
			//변환 정보를 저장하자
			Dictionary<GenVertex, apVertex> genVertToMeshVert = new Dictionary<GenVertex, apVertex>();


			//테스트로 이 메시에 버텍스들을 모두 심어보자
			apVertex meshVert = null;
			GenVertex genVert = null;

			//Edge도 연결한다.
			GenEdge genEdge = null;
			apVertex meshVert1 = null;
			apVertex meshVert2 = null;

			//bool isInnerMargin = _option_IsInnerMargin;//이전
			bool isInnerMargin = _curRequest._option_IsInnerMargin;//변경 21.10.5

			//테스트로 메시에 반영

			//그룹이 겹치거나 기존의 메시와 겹치면 폴리곤이 제대로 생성되지 않는다.
			//기존의 버텍스의 Max를 찾고, 각각의 그룹의 위치를 잠깐 이동시켰다가 와야한다.
			//기존 버텍스들의 MaxX를 찾자
			bool isAnyExistVert = false;
			float existMaxX = 0.0f;
			int nExistVerts = _curRequest._mesh._vertexData != null ? _curRequest._mesh._vertexData.Count : 0;
			if(nExistVerts > 0)
			{
				apVertex curExVert = null;
				curExVert = _curRequest._mesh._vertexData[0];
				
				existMaxX = curExVert._pos.x;

				isAnyExistVert = true;
				if (nExistVerts > 1)
				{
					for (int iExistVert = 1; iExistVert < nExistVerts; iExistVert++)
					{
						curExVert = _curRequest._mesh._vertexData[iExistVert];
						if (curExVert._pos.x > existMaxX)
						{
							existMaxX = curExVert._pos.x;
						}
					}
				}
			}

			//이제 그룹당 잠시 대피할 오프셋을 정하자
			//Group 순서대로 RectArea + 적당한 여백을 비교해서 이동 여부를 판단하고 마지막 Max X를 정하자
			bool isLastMaxX = false;
			float lastMaxX = 0.0f;

			float moveMargin = 30.0f;//여유는 30

			if(isAnyExistVert)
			{
				//이전 버텍스가 있었다면, 이전 버텍스의 마지막 X보다는 오른쪽에 위치해야한다.
				isLastMaxX = true;
				lastMaxX = existMaxX + moveMargin;
			}
			Dictionary<GenWrapGroup, float> wrapGroup2MoveOffset = new Dictionary<GenWrapGroup, float>();
			if (_wrapGroups != null && _wrapGroups.Count > 0)
			{
				GenWrapGroup curWrapGroup = null;

				//버텍스 먼저 추가하고
				for (int iGroup = 0; iGroup < _wrapGroups.Count; iGroup++)
				{
					curWrapGroup = _wrapGroups[iGroup];

					float moveOffset = 0.0f;
					float rectMinX = curWrapGroup._rectArea._minX - moveMargin;//여백 포함
					float rectMaxX = curWrapGroup._rectArea._maxX + moveMargin;//여백 포함
					if(isLastMaxX)
					{
						//움직여야 한다면 확인한다.
						if(rectMinX < lastMaxX)
						{
							//최종 위치보다 안쪽에 있다면, 겹친 만큼 이동해야한다.
							moveOffset = (lastMaxX - rectMinX) + 10.0f;//Bias는 10
						}
					}

					//이동 위치를 기록하자
					wrapGroup2MoveOffset.Add(curWrapGroup, moveOffset);

					//Debug.Log("[" + iGroup + "] Move Offset : " + moveOffset);

					//다음을 위해 겹치는 영역을 갱신
					isLastMaxX = true;
					lastMaxX = rectMaxX + moveOffset;
				}
			}
			
			Dictionary<GenWrapGroup, List<apVertex>> wrapGroup2VertDataList = new Dictionary<GenWrapGroup, List<apVertex>>();

			//내부의 선도 만들자
			//1. 내부의 점을 먼저 다 찍고, 다시 Edge를 추가
			if (_wrapGroups != null && _wrapGroups.Count > 0)
			{
				GenWrapGroup curWrapGroup = null;
					
				//버텍스 먼저 추가하고
				for (int iGroup = 0; iGroup < _wrapGroups.Count; iGroup++)
				{
					curWrapGroup = _wrapGroups[iGroup];

					//Group간에 겹치는 일을 막는 OffsetX.
					//폴리곤 생성후 돌려놔야한다.
					float moveOffsetX = wrapGroup2MoveOffset[curWrapGroup];
					Vector2 moveOffset = new Vector2(moveOffsetX, 0.0f);

					//그룹당 버텍스 리스트를 만들어서 저장하자 (Offset 복구용)
					List<apVertex> vertDataList = new List<apVertex>();
					wrapGroup2VertDataList.Add(curWrapGroup, vertDataList);


					//외곽선 버텍스
					for (int iOutVert = 0; iOutVert < curWrapGroup._vertices_Outline.Count; iOutVert++)
					{
						genVert = curWrapGroup._vertices_Outline[iOutVert];
						if(genVertToMeshVert.ContainsKey(genVert))
						{
							continue;
						}
						meshVert = _curRequest._mesh.AddVertexAutoUV(genVert._pos + moveOffset);//Offset 적용
						genVertToMeshVert.Add(genVert, meshVert);

						vertDataList.Add(meshVert);//리스트에 추가
					}

					if (isInnerMargin)
					{
						//외곽선-내부 여백 버텍스
						for (int iInlineVert = 0; iInlineVert < curWrapGroup._vertices_Inline.Count; iInlineVert++)
						{
							genVert = curWrapGroup._vertices_Inline[iInlineVert];
							if (genVertToMeshVert.ContainsKey(genVert))
							{
								continue;
							}
							meshVert = _curRequest._mesh.AddVertexAutoUV(genVert._pos + moveOffset);//Offset 적용
							genVertToMeshVert.Add(genVert, meshVert);

							vertDataList.Add(meshVert);//리스트에 추가
						}
					}


					//내부 점
					for (int iInner = 0; iInner < curWrapGroup._vertices_Inner.Count; iInner++)
					{
						genVert = curWrapGroup._vertices_Inner[iInner];
						if(genVertToMeshVert.ContainsKey(genVert))
						{
							continue;
						}
						meshVert = _curRequest._mesh.AddVertexAutoUV(genVert._pos + moveOffset);//Offset 적용
						genVertToMeshVert.Add(genVert, meshVert);

						vertDataList.Add(meshVert);//리스트에 추가
					}
				}
					
				//Edge도 넣자
				for (int iGroup = 0; iGroup < _wrapGroups.Count; iGroup++)
				{
					curWrapGroup = _wrapGroups[iGroup];

					//외곽선
					for (int iEdge = 0; iEdge < curWrapGroup._edges_Outline.Count; iEdge++)
					{
						genEdge = curWrapGroup._edges_Outline[iEdge];
						meshVert1 = genVertToMeshVert[genEdge._vert_A];
						meshVert2 = genVertToMeshVert[genEdge._vert_B];
						_curRequest._mesh.MakeNewEdge(meshVert1, meshVert2, false);
					}

					if (isInnerMargin)
					{
						//외곽-내부 여백
						for (int iEdge = 0; iEdge < curWrapGroup._edges_Out2Inline.Count; iEdge++)
						{
							genEdge = curWrapGroup._edges_Out2Inline[iEdge];
							meshVert1 = genVertToMeshVert[genEdge._vert_A];
							meshVert2 = genVertToMeshVert[genEdge._vert_B];
							_curRequest._mesh.MakeNewEdge(meshVert1, meshVert2, false);
						}

						//내부
						for (int iEdge = 0; iEdge < curWrapGroup._edges_InterInlineVerts.Count; iEdge++)
						{
							genEdge = curWrapGroup._edges_InterInlineVerts[iEdge];
							meshVert1 = genVertToMeshVert[genEdge._vert_A];
							meshVert2 = genVertToMeshVert[genEdge._vert_B];
							_curRequest._mesh.MakeNewEdge(meshVert1, meshVert2, false);
						}
					}

					for (int iEdge = 0; iEdge < curWrapGroup._edges_Inner.Count; iEdge++)
					{
						genEdge = curWrapGroup._edges_Inner[iEdge];
						meshVert1 = genVertToMeshVert[genEdge._vert_A];
						meshVert2 = genVertToMeshVert[genEdge._vert_B];
						_curRequest._mesh.MakeNewEdge(meshVert1, meshVert2, false);
					}
				}

			}

			_curRequest._mesh.MakeEdgesToPolygonAndIndexBuffer();
			_curRequest._mesh.RefreshPolygonsToIndexBuffer();

			//Pin이 있다면, 가중치를 재계산하자
			if(_curRequest._mesh._pinGroup != null)
			{
				_curRequest._mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
			}


			if(IsTimeout())
			{
				//타임 아웃이라면
				_curRequest.SetFailed("Processing was terminated due to timeout.");
				ChangeState(PROCESS_STATE.Step4_Complete_SomeFailed);
				return;
			}


			//Polygon 완성후에는 위치를 복구시킨다.
			if (_wrapGroups != null && _wrapGroups.Count > 0)
			{
				GenWrapGroup curWrapGroup = null;

				for (int iGroup = 0; iGroup < _wrapGroups.Count; iGroup++)
				{
					curWrapGroup = _wrapGroups[iGroup];

					//Group간에 겹치는 일을 막는 OffsetX.
					//폴리곤 생성후 돌려놔야한다.
					float moveOffsetX = wrapGroup2MoveOffset[curWrapGroup];
					//Vector2 moveOffset = new Vector2(moveOffsetX, 0.0f);

					//그룹당 버텍스 리스트를 만들어서 저장하자 (Offset 복구용)
					List<apVertex> vertDataList = wrapGroup2VertDataList[curWrapGroup];
					int nVerts = vertDataList.Count;

					apVertex curVert = null;
					for (int iVert = 0; iVert < nVerts; iVert++)
					{
						curVert = vertDataList[iVert];
						
						curVert._pos.x -= moveOffsetX;//위치를 거꾸로 복구

						//UV도 적절히 변경
						_curRequest._mesh.RefreshVertexAutoUV(curVert);
					}
				}
			}

			//RenderUnit에 Mesh 변경사항 반영
			_editor.Controller.ResetAllRenderUnitsVertexIndex();

			//다음 요청으로 넘어간다.
			_iCur++;//증가!
			_subNextStep = SUB_STEP.Step1_PopRequest;
		}



		private void RecoverTextureSettings()
		{
			//텍스쳐 Readable 속성을 복구하자
			if (_texData2GenData != null)
			{
				bool isAnyRecovered = false;
				foreach (KeyValuePair<apTextureData, GenTextureData> texData in _texData2GenData)
				{
					if (texData.Value._texImporter.isReadable && !texData.Value._isPrevReadable)
					{
						//이전에는 Readable이 아닌 경우에 false로 복구
						texData.Value._texImporter.isReadable = false;
						texData.Value._texImporter.SaveAndReimport();
						isAnyRecovered = true;
					}
				}

				if (isAnyRecovered)
				{
					AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
				}
			}
		}


		// Sub-Functions
		//----------------------------------------------
		private void ChangeState(PROCESS_STATE nextState)
		{
			_nextState = nextState;
		}


		//해당 메시가 유효한 Atlas Area를 가지고 있는가
		private bool IsValidAtlasArea(apMesh mesh)
		{
			if (mesh == null)
			{
				return false;
			}
			if (!mesh._isPSDParsed)
			{
				return false;
			}

			//없거나 너무 작다.
			int width = (int)(Mathf.Abs(mesh._atlasFromPSD_RB.x - mesh._atlasFromPSD_LT.x));
			int height = (int)(Mathf.Abs(mesh._atlasFromPSD_RB.y - mesh._atlasFromPSD_LT.y));
			if (width <= 20 || height <= 20)
			{
				return false;
			}
			return true;
		}

		


		// Get/Set
		//----------------------------------------------
		public bool IsProcessing
		{
			get
			{
				return _isProcessing;
			}
		}


		





		

		



		






		

		

		



		

	}
}