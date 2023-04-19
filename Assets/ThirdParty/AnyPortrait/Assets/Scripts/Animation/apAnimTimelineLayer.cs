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

	[Serializable]
	public class apAnimTimelineLayer
	{
		// Members
		//--------------------------------------------------
		[SerializeField]
		public int _uniqueID = -1;

		[NonSerialized]
		public apAnimClip _parentAnimClip = null;

		[NonSerialized]
		public apAnimTimeline _parentTimeline = null;

		[SerializeField]
		public List<apAnimKeyframe> _keyframes = new List<apAnimKeyframe>();

		//실시간 참조를 위한 시작/끝 프레임
		//해당 프레임이 StartFrame, EndFrame이 아닐 수 있으며, 두 프레임은 같을 수도 있다.
		[NonSerialized]
		public apAnimKeyframe _firstKeyFrame = null;

		[NonSerialized]
		public apAnimKeyframe _lastKeyFrame = null;

		[NonSerialized]
		public apAnimKeyframe _underStartKeyframe = null;//Start 이전에 연결되는 프레임 (Loop에서 일정 조건하에 할당된다.)

		[NonSerialized]
		public apAnimKeyframe _overEndKeyframe = null;//End 이후에 연결되는 프레임 (Loop에서 일정 조건하에 할당된다.)

		[NonSerialized]
		private List<apAnimKeyframe> _activeKeyframes = new List<apAnimKeyframe>();//<<유효한 키프레임만 저장한다.

		//Modifier 타입일때
		public enum LINK_MOD_TYPE
		{
			None = 0,
			MeshTransform = 1,
			MeshGroupTransform = 2,
			Bone = 3
		}
		//public bool _isMeshTransform = true;
		public LINK_MOD_TYPE _linkModType = LINK_MOD_TYPE.None;
		public int _transformID = -1;
		public int _boneID = -1;


		//에디터
		[NonSerialized]
		public apTransform_Mesh _linkedMeshTransform = null;

		[NonSerialized]
		public apTransform_MeshGroup _linkedMeshGroupTransform = null;

		[NonSerialized]
		public apBone _linkedBone = null;

		//런타임
		[NonSerialized]
		public apOptTransform _linkedOptTransform = null;

		//TODO:
		[NonSerialized]
		public apOptBone _linkedOptBone = null;




		[SerializeField]
		public Color _guiColor = Color.black;//<<에디터에서 사용되지만 이 값을 저장해둔다. (GUI에서 보이는 색상값이다)


		[SerializeField]
		public bool _guiLayerVisible = true;//<<에디터에서 사용되며 저장되는 값. GUI에 출력되는가

		//1) Modifier 타입일 때
		[NonSerialized]
		public apModifierParamSetGroup _targetParamSetGroup = null;//<<해당 


		//2) Control Param 타입일때
		public int _controlParamID = -1;


		[NonSerialized]
		public apControlParam _linkedControlParam = null;


		public apAnimClip.LINK_TYPE _linkType = apAnimClip.LINK_TYPE.AnimatedModifier;

		[NonSerialized]
		public apAnimControlParamResult _linkedControlParamResult = null;

		//TODO :
		//만약 여기서 멤버를 수정했다면 -> EditorController의 Duplicate Anim Clip 부분 수정할 것



		// Init
		//--------------------------------------------------
		public apAnimTimelineLayer()
		{

		}


		public void Init_TransformOfModifier(apAnimTimeline timeline, int uniqueID, int transformID, bool isMeshTransform)
		{
			_uniqueID = uniqueID;

			_parentTimeline = timeline;
			_transformID = transformID;
			_boneID = -1;
			if (isMeshTransform)
			{
				_linkModType = LINK_MOD_TYPE.MeshTransform;
			}
			else
			{
				_linkModType = LINK_MOD_TYPE.MeshGroupTransform;
			}
			//_isMeshTransform = isMeshTransform;
			_linkType = apAnimClip.LINK_TYPE.AnimatedModifier;

			_guiColor = GetRandomColor();
		}

		public void Init_ControlParam(apAnimTimeline timeline, int uniqueID, int controlParamID)
		{
			_uniqueID = uniqueID;

			_parentTimeline = timeline;
			_controlParamID = controlParamID;
			_linkType = apAnimClip.LINK_TYPE.ControlParam;
			_linkModType = LINK_MOD_TYPE.None;

			_guiColor = GetRandomColor();
		}

		public void Init_Bone(apAnimTimeline timeline, int uniqueID, int boneID)
		{
			_uniqueID = uniqueID;

			_parentTimeline = timeline;
			_boneID = boneID;
			//_linkType = apAnimClip.LINK_TYPE.Bone;
			_linkType = apAnimClip.LINK_TYPE.AnimatedModifier;//Bone 타입이 AnimatedModifier에 통합되었다.
			_linkModType = LINK_MOD_TYPE.Bone;

			_guiColor = GetRandomColor();
		}

		private Color GetRandomColor()
		{
			float randColor_R = UnityEngine.Random.Range(0.4f, 0.6f);
			float randColor_G = UnityEngine.Random.Range(0.4f, 0.6f);
			float randColor_B = UnityEngine.Random.Range(0.4f, 0.6f);
			return new Color(randColor_R, randColor_G, randColor_B, 1.0f);
		}




		public void Link(apAnimClip animClip, apAnimTimeline timeline)
		{
			_parentAnimClip = animClip;
			_parentTimeline = timeline;

			_linkType = _parentTimeline._linkType;

			_parentAnimClip._portrait.RegistUniqueID(apIDManager.TARGET.AnimTimelineLayer, _uniqueID);

			
			//[v1.4.2] TargetMeshGroup이 없는 경우에 대한 체크 코드 추가
			//단, Target MeshGroup이 없는 경우에 바로 데이터를 -1로 초기화하지 않는다.
			//Undo 같은 경우에 제대로 링크가 되지 않을 수 있기 때문

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						switch (_linkModType)
						{
							case LINK_MOD_TYPE.MeshTransform:
								//수정 : 재귀적으로 링크를 수행한다.
								
								if(_parentAnimClip._targetMeshGroup != null)
								{
									_linkedMeshTransform = _parentAnimClip._targetMeshGroup.GetMeshTransformRecursive(_transformID);

									if (_linkedMeshTransform == null)
									{
										_transformID = -1;
									}
								}
								else
								{	
									_linkedMeshTransform = null;
								}
								
								
								break;

							case LINK_MOD_TYPE.MeshGroupTransform:
								//수정 : 재귀적으로 링크를 수행한다.
								if(_parentAnimClip._targetMeshGroup != null)
								{
									_linkedMeshGroupTransform = _parentAnimClip._targetMeshGroup.GetMeshGroupTransformRecursive(_transformID);
									
									if (_linkedMeshGroupTransform == null)
									{
										_transformID = -1;
									}
								}
								else
								{
									_linkedMeshGroupTransform = null;
								}
								
								
								break;

							case LINK_MOD_TYPE.Bone:
								if(_parentAnimClip._targetMeshGroup != null)
								{
									_linkedBone = _parentAnimClip._targetMeshGroup.GetBoneRecursive(_boneID);//Recursive 방식으로 검색한다.
									
									if (_linkedBone == null)
									{
										_boneID = -1;
									}
								}
								else
								{
									_linkedBone = null;
								}
								
								
								break;

							case LINK_MOD_TYPE.None:
								//?? 이 타입이 들어올리가 없는뎅..
								_linkedMeshTransform = null;
								_linkedMeshGroupTransform = null;
								_linkedBone = null;

								_transformID = -1;
								_boneID = -1;
								break;
						}
					}
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					{
						_linkedControlParam = _parentAnimClip._portrait.GetControlParam(_controlParamID);
						if (_linkedControlParam == null)
						{
							_controlParamID = -1;
						}
					}
					break;


			}


			for (int i = 0; i < _keyframes.Count; i++)
			{
				_keyframes[i].Link(this);
			}
		}




		public void LinkParamSetGroup(apModifierParamSetGroup paramSetGroup)
		{
			_targetParamSetGroup = paramSetGroup;
		}


		//[1.4.2] Link를 해야할지 여부를 체크하는 Validate 함수.
		//Link되어야 하는 데이터가 Null이면 false를 리턴한다.
		public bool ValidateForLinkEditor()
		{
			if(_parentAnimClip == null
				|| _parentTimeline == null)
			{
				return false;
			}

			if(_parentAnimClip._targetMeshGroup == null)
			{
				return false;
			}

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						switch (_linkModType)
						{
							case LINK_MOD_TYPE.MeshTransform:
								if(_linkedMeshTransform == null)
								{
									return false;
								}
								break;

							case LINK_MOD_TYPE.MeshGroupTransform:
								if(_linkedMeshGroupTransform == null)
								{
									return false;
								}
								break;

							case LINK_MOD_TYPE.Bone:
								if (_linkedBone == null)
								{
									return false;
								}
								break;

							case LINK_MOD_TYPE.None:
								break;
						}
					}
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					if(_linkedControlParam == null)
					{
						return false;
					}
					break;
			}

			//키프레임은 체크하지 않는다.

			//끝. 정상적이다.
			return true;
		}






		public void LinkOpt(apAnimClip animClip, apAnimTimeline timeline)
		{
			_parentAnimClip = animClip;
			_parentTimeline = timeline;

			_linkType = _parentTimeline._linkType;

			if (_parentAnimClip == null)
			{
				Debug.Log("AnyPortrait Error : Parent Anim Clip is Null");
			}
			else if (_parentAnimClip._targetOptTranform == null)
			{
				Debug.LogError("AnyPortrait Error : Parent Anim Clip TargetOptTranform is Null");
			}

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						switch (_linkModType)
						{
							case LINK_MOD_TYPE.MeshTransform:
								//수정 : 재귀적으로 링크를 수행한다.
								_linkedOptTransform = _parentAnimClip._targetOptTranform.GetMeshTransformRecursive(_transformID);
								if (_linkedOptTransform == null)
								{
									Debug.LogError("AnyPortrait Error : Opt TimelineLayer MeshTransform Linking Failed : " + _transformID);
									_transformID = -1;
								}
								break;

							case LINK_MOD_TYPE.MeshGroupTransform:
								//수정 : 재귀적으로 링크를 수행한다.
								_linkedOptTransform = _parentAnimClip._targetOptTranform.GetMeshGroupTransformRecursive(_transformID);
								if (_linkedOptTransform == null)
								{
									Debug.LogError("AnyPortrait Error : Opt TimelineLayer MeshGroupTransform Linking Failed : " + _transformID);
									_transformID = -1;
								}
								break;

							case LINK_MOD_TYPE.Bone:
								//TODO : Bone 타입 연결을 해야한다.
								_linkedOptBone = _parentAnimClip._targetOptTranform.GetBoneRecursive(_boneID);
								break;

							case LINK_MOD_TYPE.None:
								_linkedOptTransform = null;
								_transformID = -1;
								_boneID = -1;
								break;
						}
					}
					break;

				case apAnimClip.LINK_TYPE.ControlParam:
					{
						_linkedControlParam = _parentAnimClip._portrait.GetControlParam(_controlParamID);
						if (_linkedControlParam == null)
						{
							_controlParamID = -1;
						}
					}
					break;

					//case apAnimClip.LINK_TYPE.Bone:
					//	{
					//		Debug.LogError("TODO : TimelineLayer의 Bone 타입 연동하기");
					//	}
					//	break;
			}


			for (int i = 0; i < _keyframes.Count; i++)
			{
				_keyframes[i].Link(this);
			}

			SortAndRefreshKeyframes();
			
		}



		// Functions
		//--------------------------------------------------
		//이전
		//public int SortAndRefreshKeyframes(bool isPrintLog = false, bool isMakeCurveForce = false)
		//변경 19.5.20 : 최적화 작업 겸 불필요한 인자 제거 (성능상에 Curve 만들고 안만들고는 큰 영향이 없다.. 이 함수 전체가 무거움)
		public int SortAndRefreshKeyframes()
		{
			_keyframes.Sort(delegate (apAnimKeyframe a, apAnimKeyframe b)
			{
				return a._frameIndex - b._frameIndex;
			});

			int startFrame = _parentAnimClip.StartFrame;
			int endFrame = _parentAnimClip.EndFrame;
			int lengthFrame = endFrame - startFrame;
			bool isLoop = _parentAnimClip.IsLoop;

			_firstKeyFrame = null;
			_lastKeyFrame = null;
			_underStartKeyframe = null;
			_overEndKeyframe = null;

			apAnimKeyframe prevKey = null;
			apAnimKeyframe nextKey = null;
			apAnimKeyframe curKey = null;


			int nRefreshed = 0;

			if (_activeKeyframes == null)
			{
				_activeKeyframes = new List<apAnimKeyframe>();
			}
			else
			{
				_activeKeyframes.Clear();
			}


			for (int i = 0; i < _keyframes.Count; i++)
			{
				curKey = _keyframes[i];
				//curKey._curveKey.SetLoopSize(lengthFrame);

				curKey._isActive = true;

				if (curKey._frameIndex < startFrame ||
					curKey._frameIndex > endFrame)
				{
					//범위를 벗어났다.
					curKey.SetInactive();
					continue;
				}
				curKey.SetDummyDisable();//일단 더미 끈 상태로 처리
				_activeKeyframes.Add(curKey);
			}

			
			if (_activeKeyframes.Count > 0)
			{
				//1. 일단 루프 체크 및 더미 키 여부를 판단하자
				if (isLoop)
				{
					//수정 : F/L은 무조건 0, n 에서 지정된다.
					//1)의 경우 (Start, End가 모두 지정된 경우)를 제외하고는 F, L은 모두 더미를 갖는다.

					//		-----[Start]-------------------------[End]-----------
					//1)	     <First>                         <Last>         (F/L 다르다 + 더미가 없다)
					//2)	 <<- <First>      ~       [L:n]  ->> (dummy) <<-	(F가 L이 된다 (L이 더미))
					//3)	 ->> (dummy) <<-  [F:0]     ~        <Last>    ->>  (L이 F가 된다 (F가 더미))
					//4)	 <<->>      [0:F:Dummy] ~ [n:L:Dummy]		<<->>   (F와 L이 다르다 (서로가 서로의 더미 프레임을 가진다)


					_firstKeyFrame = _activeKeyframes[0];
					_lastKeyFrame = _activeKeyframes[_activeKeyframes.Count - 1];
					if (_firstKeyFrame._frameIndex == startFrame &&
						_lastKeyFrame._frameIndex == endFrame)
					{
						//딱히 뭐 없네용
					}
					else
					{
						_firstKeyFrame.SetLoopFrame(true, _firstKeyFrame._frameIndex + lengthFrame);
						_lastKeyFrame.SetLoopFrame(false, _lastKeyFrame._frameIndex - lengthFrame);

						_underStartKeyframe = _lastKeyFrame;
						_overEndKeyframe = _firstKeyFrame;
					}
				}
				else
				{
					//루프가 아니라면
					//마지막 프레임이 마지막이다.
					_firstKeyFrame = _activeKeyframes[0];
					_lastKeyFrame = _activeKeyframes[_activeKeyframes.Count - 1];
				}


				//2. 활성화된 프레임들끼리 연결을 해주자
				int iPrev = 0;//<<여기서 인덱스는 Frame값이 아니라 리스트의 인덱스이다.
				int iNext = 0;
				int iLast = _activeKeyframes.Count - 1;
				int prevFrameIndex = 0;
				int nextFrameIndex = 0;

				for (int i = 0; i < _activeKeyframes.Count; i++)
				{

					curKey = _activeKeyframes[i];
					prevKey = null;
					nextKey = null;

					prevFrameIndex = curKey._frameIndex;
					nextFrameIndex = curKey._frameIndex;

					iPrev = i - 1;
					iNext = i + 1;


					if (iPrev < 0 && isLoop)
					{
						if (_underStartKeyframe != null)
						{
							iPrev = iLast;
						}
					}

					if (iPrev >= 0 && iPrev <= iLast)
					{
						prevKey = _activeKeyframes[iPrev];

						////예외가 하나 있다.
						////[현재 = StartFrame] 일때
						////[Prev = EndFrame]이라면 두 프레임은 연결되지 않는다. (Loop에서 두 프레임은 같은걸로 친다)
						//if (curKey._frameIndex == startFrame &&
						//	prevKey._frameIndex == endFrame)
						//{
						//	prevKey = null;
						//}
					}

					if (iNext > iLast && isLoop)
					{
						if (_overEndKeyframe != null)
						{
							iNext = 0;
						}
					}

					if (iNext >= 0 && iNext <= iLast)
					{
						nextKey = _activeKeyframes[iNext];

						////예외가 하나 있다.
						////[현재 = EndFrame] 일때
						////[Next = StartFrame]이라면 두 프레임은 연결되지 않는다. (Loop에서 두 프레임은 같은걸로 친다)
						//if (curKey._frameIndex == endFrame &&
						//	nextKey._frameIndex == startFrame)
						//{
						//	nextKey = null;
						//}
					}


					if (prevKey != null)
					{
						prevFrameIndex = prevKey._frameIndex;
						if (prevFrameIndex > curKey._frameIndex)
						{
							prevFrameIndex -= lengthFrame;
						}
					}

					if (nextKey != null)
					{
						nextFrameIndex = nextKey._frameIndex;
						if (nextFrameIndex < curKey._frameIndex)
						{
							nextFrameIndex += lengthFrame;
						}
					}

					//curKey.SetLinkedKeyframes(prevKey, nextKey, isPrevDummy, isNextDummy);

					//자기 자신으로 연결되지는 않는다.

					if (prevKey == curKey)
					{
						prevKey = null;
					}

					if (nextKey == curKey)
					{
						nextKey = null;
					}

					//변경 19.5.20
					curKey.SetLinkedKeyframes(prevKey, nextKey, prevFrameIndex, nextFrameIndex);
				}



				//마지막으로 전체 CurveKeyCalculate를 하자
				for (int i = 0; i < _activeKeyframes.Count; i++)
				{
					//+ frame영역을 정하자
					curKey = _activeKeyframes[i];
					if (curKey._prevLinkedKeyframe != null)
					{
						if (curKey._isLoopAsStart)
						{
							curKey._activeFrameIndexMin = curKey._prevLinkedKeyframe._loopFrameIndex;
							curKey._activeFrameIndexMin_Dummy = curKey._activeFrameIndexMin + lengthFrame;
							curKey._activeFrameIndexMax_Dummy = curKey._frameIndex + lengthFrame;
						}
						else
						{
							curKey._activeFrameIndexMin = curKey._prevLinkedKeyframe._frameIndex;
						}

						//if(curKey._activeFrameIndexMin > curKey._frameIndex)
						//{
						//	//Loop인가..
						//	curKey._activeFrameIndexMin -= lengthFrame;
						//}
					}
					else
					{
						if (curKey == _firstKeyFrame)
						{
							curKey._activeFrameIndexMin = startFrame - 10;
						}
						else
						{
							//? 연결이 안되어있다니?
							curKey._activeFrameIndexMin = curKey._frameIndex;
						}
					}

					if (curKey._nextLinkedKeyframe != null)
					{
						if (curKey._isLoopAsEnd)
						{
							curKey._activeFrameIndexMax = curKey._nextLinkedKeyframe._loopFrameIndex;
							curKey._activeFrameIndexMin_Dummy = curKey._frameIndex - lengthFrame;
							curKey._activeFrameIndexMax_Dummy = curKey._activeFrameIndexMax - lengthFrame;
						}
						else
						{
							curKey._activeFrameIndexMax = curKey._nextLinkedKeyframe._frameIndex;
						}

						//if(curKey._activeFrameIndexMax < curKey._frameIndex)
						//{
						//	//Loop인가..
						//	curKey._activeFrameIndexMax += lengthFrame;
						//}
					}
					else
					{
						if (curKey == _lastKeyFrame)
						{
							curKey._activeFrameIndexMax = endFrame + 10;
						}
						else
						{
							curKey._activeFrameIndexMax = curKey._frameIndex;
						}
					}

					_activeKeyframes[i].RefreshCurveKey();

					nRefreshed++;
					//Debug.Log("Frame [" + curKey._frameIndex + "] ( " + curKey._activeFrameIndexMin + " ~ " + curKey._activeFrameIndexMax + " )");
				}
			}

			//if (isPrintLog)
			//{
			//	Debug.Log("---------------------------------------------");
			//}

			return nRefreshed;
		}


		// Get / Set
		//--------------------------------------------------
		public bool IsKeyframeContain(apAnimKeyframe keyframe)
		{
			return _keyframes.Contains(keyframe);
		}

		public string DisplayName
		{
			get
			{
				switch (_linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						switch (_linkModType)
						{
							case LINK_MOD_TYPE.MeshTransform:
								if (_linkedMeshTransform != null)
								{
									return _linkedMeshTransform._nickName;
								}
								break;

							case LINK_MOD_TYPE.MeshGroupTransform:
								if (_linkedMeshGroupTransform != null)
								{
									return _linkedMeshGroupTransform._nickName;
								}
								break;

							case LINK_MOD_TYPE.Bone:
								if (_linkedBone != null)
								{
									return _linkedBone._name;
								}
								break;

							case LINK_MOD_TYPE.None:
								break;
						}

						return "Unknown Modifier Unit";



					case apAnimClip.LINK_TYPE.ControlParam:
						if (_linkedControlParam != null)
						{
							return _linkedControlParam._keyName;
						}
						return "Unknown Control Param";


					default:
						return "Unknown Type";
				}
			}
		}

		public bool IsContainTargetObject(object targetObject)
		{
			if (targetObject == null)
			{
				return false;
			}

			switch (_linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					switch (_linkModType)
					{
						case LINK_MOD_TYPE.MeshTransform:
							if (_linkedMeshTransform == targetObject)
							{
								return true;
							}
							break;

						case LINK_MOD_TYPE.MeshGroupTransform:
							if (_linkedMeshGroupTransform == targetObject)
							{
								return true;
							}
							break;

						case LINK_MOD_TYPE.Bone:
							if (_linkedBone == targetObject)
							{
								return true;
							}
							break;

						case LINK_MOD_TYPE.None:
							//값이 지정 안되었다.
							return false;
					}
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					if (_linkedControlParam == targetObject)
					{
						return true;
					}
					break;


				default:
					Debug.LogError(" Error : Unknown Type");
					break;
			}

			return false;
		}


		public apAnimKeyframe GetKeyframeByID(int keyframeUniqueID)
		{
			return _keyframes.Find(delegate (apAnimKeyframe a)
			{
				return a._uniqueID == keyframeUniqueID;
			});
		}

		public apAnimKeyframe GetKeyframeByFrameIndex(int frame)
		{
			return _keyframes.Find(delegate (apAnimKeyframe a)
			{
				return a._frameIndex == frame;
			});
		}

		//-----------------------------------------------------------------------------------
		// Copy For Bake
		//-----------------------------------------------------------------------------------
		public void CopyFromTimelineLayer(apAnimTimelineLayer srcTimelineLayer, apAnimClip parentAnimClip, apAnimTimeline parentTimeline)
		{
			_uniqueID = srcTimelineLayer._uniqueID;
			_parentTimeline = parentTimeline;
			_parentAnimClip = parentAnimClip;

			_keyframes.Clear();
			for (int iKey = 0; iKey < srcTimelineLayer._keyframes.Count; iKey++)
			{
				apAnimKeyframe srcKeyframe = srcTimelineLayer._keyframes[iKey];

				//Keyframe을 복사하자
				apAnimKeyframe newKeyframe = new apAnimKeyframe();
				newKeyframe.CopyFromKeyframe(srcKeyframe);

				_keyframes.Add(newKeyframe);
			}

			_linkModType = srcTimelineLayer._linkModType;
			_transformID = srcTimelineLayer._transformID;
			_boneID = srcTimelineLayer._boneID;

			_guiColor = srcTimelineLayer._guiColor;
			_guiLayerVisible = srcTimelineLayer._guiLayerVisible;
			_controlParamID = srcTimelineLayer._controlParamID;

			_linkType = srcTimelineLayer._linkType;

			//SortAndRefreshKeyframes(false, true);//<<추가 3.31 : 커브 복사 버그 수정
			SortAndRefreshKeyframes();//변경 19.5.20

		}
	}

}