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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// AnimClip을 저장하고 로드하는 클래스
	/// 본격적으로 Retarget을 해야한다.
	/// AnimClip의 정보와 해당 MeshGroup의 기본 정보를 모두 가지고 있다.
	/// </summary>
	public class apRetargetAnimFile
	{
		// Members
		//-----------------------------------------------
		//1. AnimClip 정보
		public apAnimClip _linkedAnimClip = null;
		
		public int _animUniqueID = -1;
		public string _animName = "";
		
		public int _FPS = 0;
		public int _startFrame = 0;
		public int _endFrame = 0;
		public bool _isLoop = false;

		public List<apRetargetTimelineUnit> _timelineUnits = new List<apRetargetTimelineUnit>();

		//2. MeshGroup 정보
		public int _animTargetMeshGroupID = -1;
		public apMeshGroup _targetMeshGroup = null;
		public string _meshGroupName = "";

		//2-1. Transform / Bone 정보
		// 이름, ID, 기본 Matrix와 Color, 계층 정보를 저장한다.
		public List<apRetargetSubUnit> _transforms_Total = new List<apRetargetSubUnit>();
		public List<apRetargetSubUnit> _bones_Total = new List<apRetargetSubUnit>();
		public List<apRetargetSubUnit> _transforms_Root = new List<apRetargetSubUnit>();
		public List<apRetargetSubUnit> _bones_Root = new List<apRetargetSubUnit>();

		//2-2. AnimEvent 정보
		public List<apRetargetAnimEvent> _animEvents = new List<apRetargetAnimEvent>();


		//3. Control Param 정보
		public List<apRetargetControlParam> _controlParams = new List<apRetargetControlParam>();

		//로딩 정보
		private bool _isLoaded = false;
		private string _filePath = "";

		// Init
		//-----------------------------------------------
		public apRetargetAnimFile()
		{
			Clear();
		}

		public void Clear()
		{
			_linkedAnimClip = null;
		
			_animUniqueID = -1;
			_animName = "";
		
			_FPS = 0;
			_startFrame = 0;
			_endFrame = 0;
			_isLoop = false;

			_timelineUnits.Clear();

			//2. MeshGroup 정보
			_animTargetMeshGroupID = -1;
			_targetMeshGroup = null;
			_meshGroupName = "";

			//2-1. Transform / Bone 정보
			// 이름, ID, 기본 Matrix와 Color, 계층 정보를 저장한다.
			_transforms_Total.Clear();
			_bones_Total.Clear();
			_transforms_Root.Clear();
			_bones_Root.Clear();

			//2-2. AnimEvent 정보
			_animEvents.Clear();

			//3. ControlParam 정보
			_controlParams.Clear();

			//로딩 정보
			_isLoaded = false;
			_filePath = "";
		}

		// Functions
		//-----------------------------------------------
		public static bool SaveAnimFile(apAnimClip animClip, string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return false;
			}
			if (animClip == null ||
				animClip._targetMeshGroup == null)
			{
				return false;
			}

			//데이터를 넣자

			int animUniqueID = animClip._uniqueID;
			string animName = animClip._name;

			int FPS = animClip.FPS;
			int startFrame = animClip.StartFrame;
			int endFrame = animClip.EndFrame;
			bool isLoop = animClip.IsLoop;

			List<apRetargetTimelineUnit> timelineUnits = new List<apRetargetTimelineUnit>();

			int animTargetMeshGroupID = animClip._targetMeshGroup._uniqueID;
			apMeshGroup targetMeshGroup = animClip._targetMeshGroup;
			string meshGroupName = animClip._targetMeshGroup._name;

			//1. Timeline을 설정하자
			int curTimelineUnitID = 0;
			for (int i = 0; i < animClip._timelines.Count; i++)
			{
				apRetargetTimelineUnit timelineUnit = new apRetargetTimelineUnit();
				timelineUnit.SetTimeline(curTimelineUnitID, animClip._timelines[i]);

				timelineUnits.Add(timelineUnit);
				curTimelineUnitID++;
			}
			

			//2. Transform/Bone 을 설정
			List<apRetargetSubUnit> transforms_All = new List<apRetargetSubUnit>();
			List<apRetargetSubUnit> bones_All = new List<apRetargetSubUnit>();
			List<apRetargetSubUnit> transforms_Root = new List<apRetargetSubUnit>();
			List<apRetargetSubUnit> bones_Root = new List<apRetargetSubUnit>();

			MakeSubUnits(targetMeshGroup, transforms_All, transforms_Root, bones_All, bones_Root);

			//2-2. AnimEvent 설정
			List<apRetargetAnimEvent> animEvents = new List<apRetargetAnimEvent>();
			if(animClip._animEvents.Count > 0)
			{
				for (int i = 0; i < animClip._animEvents.Count; i++)
				{
					apRetargetAnimEvent newEvent = new apRetargetAnimEvent();
					newEvent.SetAnimationEvent(animClip._animEvents[i]);

					animEvents.Add(newEvent);
				}
				
			}

			//3. Control Param 설정
			List<apRetargetControlParam> controlParams = new List<apRetargetControlParam>();
			int nControlParams = targetMeshGroup._parentPortrait._controller._controlParams.Count;
			int curContolParamUnitID = 0;
			for (int i = 0; i < nControlParams; i++)
			{
				apControlParam controlParam = targetMeshGroup._parentPortrait._controller._controlParams[i];
				apRetargetControlParam cpUnit = new apRetargetControlParam();
				cpUnit.SetControlParam(curContolParamUnitID, controlParam);

				curContolParamUnitID++;

				controlParams.Add(cpUnit);
			}


			FileStream fs = null;
			StreamWriter sw = null;
			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine("Animation Clip");
				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine(animUniqueID);
				sw.WriteLine(animName);
				sw.WriteLine(FPS);
				sw.WriteLine(startFrame);
				sw.WriteLine(endFrame);
				sw.WriteLine((isLoop ? "1" : "0"));
				sw.WriteLine(animTargetMeshGroupID);
				sw.WriteLine(meshGroupName);

				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine("SubTransforms");
				sw.WriteLine(transforms_All.Count);
				sw.WriteLine("-----------------------------------------------------------------------------------");
				for (int i = 0; i < transforms_All.Count; i++)
				{
					sw.WriteLine(transforms_All[i].GetEncodingData());
				}

				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine("SubBones");
				sw.WriteLine(bones_All.Count);
				sw.WriteLine("-----------------------------------------------------------------------------------");
				for (int i = 0; i < bones_All.Count; i++)
				{
					sw.WriteLine(bones_All[i].GetEncodingData());
				}

				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine("Animation Timelines");
				sw.WriteLine(timelineUnits.Count);
				sw.WriteLine("-----------------------------------------------------------------------------------");
				for (int i = 0; i < timelineUnits.Count; i++)
				{
					timelineUnits[i].EncodeToFile(sw);
				}

				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine("Animation Events");
				sw.WriteLine(animEvents.Count);
				sw.WriteLine("-----------------------------------------------------------------------------------");
				if(animEvents.Count > 0)
				{
					for (int i = 0; i < animEvents.Count; i++)
					{
						animEvents[i].EncodeToFile(sw);
					}
				}

				sw.WriteLine("-----------------------------------------------------------------------------------");
				sw.WriteLine(" Controller Parameters ");
				sw.WriteLine(controlParams.Count);
				sw.WriteLine("-----------------------------------------------------------------------------------");
				if(controlParams.Count > 0)
				{
					for (int i = 0; i < controlParams.Count; i++)
					{
						sw.WriteLine(controlParams[i].GetEncodingData());
					}
				}

				sw.Flush();
				sw.Close();
				fs.Close();
				sw = null;
				fs = null;
			}
			catch(Exception ex)
			{
				Debug.LogError("SaveAnimFile Exception : " + ex);

				if(sw != null)
				{
					sw.Close();
					sw = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}

				return false;
			}

			return true;
		}

		private static void MakeSubUnits(apMeshGroup targetMeshGroup, 
											List<apRetargetSubUnit> transforms_All,
											List<apRetargetSubUnit> transforms_Root,
											List<apRetargetSubUnit> bones_All,
											List<apRetargetSubUnit> bones_Root)
		{
			int unitID = 0;
			

			if(targetMeshGroup._childMeshTransforms != null)
			{
				for (int i = 0; i < targetMeshGroup._childMeshTransforms.Count; i++)
				{
					apTransform_Mesh meshTransform = targetMeshGroup._childMeshTransforms[i];

					apRetargetSubUnit newSubUnit = new apRetargetSubUnit();
					newSubUnit.SetSubData(unitID, meshTransform, null, null, null);
					unitID++;

					transforms_All.Add(newSubUnit);
					transforms_Root.Add(newSubUnit);
				}
				
			}


			if(targetMeshGroup._childMeshGroupTransforms != null)
			{
				for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
				{
					apTransform_MeshGroup meshGroupTransform = targetMeshGroup._childMeshGroupTransforms[i];

					//재귀 호출로 ChildMeshGroup의 SubUnit을 만들어주자
					unitID = MakeSubUnitsFromMeshGroupTransformRecursive(	targetMeshGroup, meshGroupTransform,
																			unitID,
																			transforms_All, transforms_Root,
																			null);
				}
			}

			//Sort를 다시 하자
			for (int i = 0; i < transforms_All.Count; i++)
			{
				apRetargetSubUnit subUnit = transforms_All[i];
				int sortIndex = -1;
				if(subUnit._type == apRetargetSubUnit.TYPE.MeshTransform)
				{
					sortIndex = targetMeshGroup._renderUnits_All.FindIndex(delegate (apRenderUnit a)
					{
						if(a._meshTransform == null)
						{
							return false;
						}
						return a._meshTransform._transformUniqueID == subUnit._uniqueID;
					});
				}
				else if(subUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform)
				{
					sortIndex = targetMeshGroup._renderUnits_All.FindIndex(delegate (apRenderUnit a)
					{
						if(a._meshGroupTransform == null)
						{
							return false;
						}
						return a._meshGroupTransform._transformUniqueID == subUnit._uniqueID;
					});
				}
				subUnit._sortIndex = sortIndex;
			}
			transforms_All.Sort(delegate (apRetargetSubUnit a, apRetargetSubUnit b)
			{
				return b._sortIndex - a._sortIndex;
			});




			//Bone도 넣자
			//<BONE_EDIT>
			//if(targetMeshGroup._boneList_Root.Count > 0)
			//{
			//	for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			//	{
			//		unitID = MakeSubUnitsFromBonesRecursive(targetMeshGroup,
			//												targetMeshGroup._boneList_Root[i],
			//												unitID, bones_All, bones_Root, null);
			//	}
			//}

			//>> Bone Set으로 변경
			if(targetMeshGroup._boneListSets.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
				{
					boneSet = targetMeshGroup._boneListSets[iSet];
					for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
					{
						unitID = MakeSubUnitsFromBonesRecursive(
															boneSet._bones_Root[iRoot],
															unitID, bones_All, bones_Root, null);
					}
				}
			}


			//Sort를 다시 하자
			for (int i = 0; i < bones_All.Count; i++)
			{
				apRetargetSubUnit subUnit = bones_All[i];
				//<BONE_EDIT>

				//subUnit._sortIndex = targetMeshGroup._boneList_All.FindIndex(delegate (apBone a)
				//{
				//	return a._uniqueID == subUnit._uniqueID;
				//});

				//>>Bone Set을 이용
				subUnit._sortIndex = -1;

				apMeshGroup.BoneListSet boneSet = null;
				int startIndex = 0;
				for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
				{
					boneSet = targetMeshGroup._boneListSets[iSet];

					//현재의 Bone List에 있는지 확인
					int resultIndex = boneSet._bones_All.FindIndex(delegate(apBone a)
					{
						return a._uniqueID == subUnit._uniqueID;
					});

					if(resultIndex >= 0 && resultIndex < boneSet._bones_All.Count)
					{
						//찾았다.
						subUnit._sortIndex = startIndex + resultIndex;
						break;
					}
					else
					{
						//업다면 기본 인덱스 증가
						startIndex += boneSet._bones_All.Count;
					}
				}

			}
			bones_All.Sort(delegate (apRetargetSubUnit a, apRetargetSubUnit b)
			{
				return a._sortIndex - b._sortIndex;
			});
			
			
		}

		private static int MakeSubUnitsFromMeshGroupTransformRecursive(
												apMeshGroup rootMeshGroup,
												apTransform_MeshGroup meshGroupTransform,
												int startUnitID,
												List<apRetargetSubUnit> transforms_All,
												List<apRetargetSubUnit> transforms_Root,
												apRetargetSubUnit parentSubUnit
												)
		{
			//MeshGroup Transform을 추가한다.
			apRetargetSubUnit newGroupSubUnit = new apRetargetSubUnit();
			newGroupSubUnit.SetSubData(startUnitID, null, meshGroupTransform, null, parentSubUnit);

			transforms_All.Add(newGroupSubUnit);
			if (parentSubUnit == null)
			{
				transforms_Root.Add(newGroupSubUnit);
			}

			startUnitID++;

			if(meshGroupTransform._meshGroup != null && meshGroupTransform._meshGroup != rootMeshGroup)
			{
				if(meshGroupTransform._meshGroup._childMeshTransforms != null)
				{
					for (int i = 0; i < meshGroupTransform._meshGroup._childMeshTransforms.Count; i++)
					{
						//MeshTransform을 추가한다.
						apTransform_Mesh meshTransform = meshGroupTransform._meshGroup._childMeshTransforms[i];

						apRetargetSubUnit newSubUnit = new apRetargetSubUnit();
						newSubUnit.SetSubData(startUnitID, meshTransform, null, null, newGroupSubUnit);
						startUnitID++;

						transforms_All.Add(newSubUnit);
					}
				}

				//하위에 다른 MeshGroup Transform이 있는 경우
				if(meshGroupTransform._meshGroup._childMeshGroupTransforms != null)
				{
					//재귀 호출을 한다
					for (int i = 0; i < meshGroupTransform._meshGroup._childMeshGroupTransforms.Count; i++)
					{
						apTransform_MeshGroup childMeshGroup = meshGroupTransform._meshGroup._childMeshGroupTransforms[i];

						startUnitID = MakeSubUnitsFromMeshGroupTransformRecursive(
												rootMeshGroup,
												childMeshGroup,
												startUnitID,
												transforms_All,
												transforms_Root,
												newGroupSubUnit
												);
					}
				}
			}
			return startUnitID;
		}

		private static int MakeSubUnitsFromBonesRecursive(
												apBone targetBone,
												int startUnitID,
												List<apRetargetSubUnit> bones_All,
												List<apRetargetSubUnit> bones_Root,
												apRetargetSubUnit parentSubUnit
												)
		{
			apRetargetSubUnit newUnit = new apRetargetSubUnit();
			newUnit.SetSubData(startUnitID, null, null, targetBone, parentSubUnit);

			startUnitID++;
			bones_All.Add(newUnit);
			if(parentSubUnit == null)
			{
				bones_Root.Add(newUnit);
			}

			if(targetBone._childBones.Count > 0)
			{
				for (int i = 0; i < targetBone._childBones.Count; i++)
				{
					startUnitID = MakeSubUnitsFromBonesRecursive(
												targetBone._childBones[i],
												startUnitID,
												bones_All,
												bones_Root,
												newUnit
												);
				}
			}

			return startUnitID;
		}



		public bool LoadAnimClip(string filePath)
		{
			FileStream fs = null;
			StreamReader sr = null;

			Clear();

			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제와 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				sr.ReadLine();//"---"
				sr.ReadLine();//"Animation Clip"
				sr.ReadLine();//"---"

				_animUniqueID = int.Parse(sr.ReadLine());
				_animName = sr.ReadLine();
				_FPS = int.Parse(sr.ReadLine());
				_startFrame = int.Parse(sr.ReadLine());
				_endFrame = int.Parse(sr.ReadLine());
				_isLoop = int.Parse(sr.ReadLine()) == 1 ? true : false;
				_animTargetMeshGroupID = int.Parse(sr.ReadLine());
				_meshGroupName = sr.ReadLine();

				sr.ReadLine();//"---"
				sr.ReadLine();//"SubTransforms"
				int nTransforms = int.Parse(sr.ReadLine());
				sr.ReadLine();//"---"
				for (int i = 0; i < nTransforms; i++)
				{
					apRetargetSubUnit transformUnit = new apRetargetSubUnit();
					transformUnit.DecodeData(sr.ReadLine());
					_transforms_Total.Add(transformUnit);//<<링크는 나중에
				}


				sr.ReadLine();//"---"
				sr.ReadLine();//"SubBones"
				int nBones = int.Parse(sr.ReadLine());
				sr.ReadLine();//"---"
				for (int i = 0; i < nBones; i++)
				{
					apRetargetSubUnit boneUnit = new apRetargetSubUnit();
					boneUnit.DecodeData(sr.ReadLine());

					_bones_Total.Add(boneUnit);
				}

				sr.ReadLine();//"---"
				sr.ReadLine();//"Animation Timelines"
				int nTimelines = int.Parse(sr.ReadLine());
				sr.ReadLine();//"---"
				for (int i = 0; i < nTimelines; i++)
				{
					apRetargetTimelineUnit timelineUnit = new apRetargetTimelineUnit();
					bool parseResult = timelineUnit.DecodeData(sr);
					if(!parseResult)
					{
						Debug.LogError("Parse Failed");
					}

					_timelineUnits.Add(timelineUnit);

				}


				sr.ReadLine();//"---"
				sr.ReadLine();//"Animation Event"
				int nEvnet = int.Parse(sr.ReadLine());
				sr.ReadLine();//"---"
				for (int i = 0; i < nEvnet; i++)
				{
					apRetargetAnimEvent animEvent = new apRetargetAnimEvent();
					bool parseResult = animEvent.DecodeData(sr);
					if(!parseResult)
					{
						Debug.LogError("Parse Failed");
					}

					_animEvents.Add(animEvent);
				}

				sr.ReadLine();//"---"
				sr.ReadLine();//"Controller Parameters"
				int nControllerParams = int.Parse(sr.ReadLine());
				sr.ReadLine();//"---"
				for (int i = 0; i < nControllerParams; i++)
				{
					apRetargetControlParam controlParam = new apRetargetControlParam();
					bool parseResult = controlParam.DecodeData(sr.ReadLine());
					if(!parseResult)
					{
						Debug.LogError("Parse Error");
					}

					_controlParams.Add(controlParam);
				}


				sr.Close();
				fs.Close();

				sr = null;
				fs = null;

				_isLoaded = true;
				_filePath = filePath;
			}
			catch (Exception ex)
			{
				Debug.LogError("LoadAnimClip Exception : " + ex);
				return false;
			}
			return true;
		}

		

		// Get / Set
		//-----------------------------------------------
		public bool IsLoaded {  get { return _isLoaded; } }
		public string LoadFilePath { get { return _filePath; } }
	}
}