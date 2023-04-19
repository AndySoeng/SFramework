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
	/// 특정 MeshGroup이 리타겟을 적용하면서 매핑했던 기록을 파일로 저장한다.
	/// 리타겟 했던 정보를 빠르게 저장할 수 있다.
	/// 일단 열린 정보를 이 인스턴스를 계속 가지고 있으면 반복해서 리타겟을 할 수 있다.
	/// > 변경 : 멤버는 최소화. 일종의 메타 데이터만 가진다.
	/// </summary>
	public class apRetargetMapping
	{
		// Members
		//---------------------------------------------------
		public class Mapping
		{
			public enum TYPE
			{
				MeshTransform = 0,
				MeshGroupTransform = 1,
				Bone = 2,
				Timeline = 3,
				ControlParam = 4,
				Event = 5,

			}

			public int _srcUnitID = -1;
			public int _srcUniqueID = -1;//원래의 Src의 UniqueID. Mapping의 유효성을 검사하는 용도
			public string _srcName = "";
			public int _subParam1 = -1;
			public int _subParam2 = -1;
			public TYPE _type = TYPE.Timeline;
			public int _dstUniqueID = -1;
			public string _dstName = "";
			public bool _isImported = false;

			public Mapping()
			{

			}
			public Mapping(int srcUnitID, int srcUniqueID, string srcName, TYPE type, int dstUniqueID, string dstName, bool isImported, int subParam1 = -1, int subParam2 = -1)
			{
				_srcUnitID = srcUnitID;
				_srcUniqueID = srcUniqueID;
				_srcName = srcName;
				_type = type;
				_dstUniqueID = dstUniqueID;
				_dstName = dstName;
				_isImported = isImported;
				_subParam1 = subParam1;
				_subParam2 = subParam2;
			}

			public string GetEncodingData()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				//Src Name
				if(_srcName.Length < 10)
				{
					sb.Append("00"); sb.Append(_srcName.Length);
				}
				else if(_srcName.Length < 100)
				{
					sb.Append("0"); sb.Append(_srcName.Length);
				}
				else
				{
					sb.Append(_srcName.Length);
				}
				if (_srcName.Length > 0)
				{
					sb.Append(_srcName);
				}

				//Dst Name
				if(_dstName.Length < 10)
				{
					sb.Append("00"); sb.Append(_dstName.Length);
				}
				else if(_dstName.Length < 100)
				{
					sb.Append("0"); sb.Append(_dstName.Length);
				}
				else
				{
					sb.Append(_dstName.Length);
				}
				if (_dstName.Length > 0)
				{
					sb.Append(_dstName);
				}


				sb.Append((int)_type);	sb.Append("/");
				sb.Append(_srcUnitID);	sb.Append("/");
				sb.Append(_srcUniqueID);	sb.Append("/");
				sb.Append(_dstUniqueID);	sb.Append("/");
				sb.Append(_isImported ? "1" : "0");	sb.Append("/");
				sb.Append(_subParam1);	sb.Append("/");
				sb.Append(_subParam2);	sb.Append("/");


				return sb.ToString();
			}

			public bool DecodeData(string strSrc)
			{
				try
				{
					_srcUnitID = -1;
					_srcName = "";
					_type = TYPE.Timeline;
					_dstUniqueID = -1;
					_dstName = "";
					_isImported = false;
					_subParam1 = -1;
					_subParam2 = -1;

					int nSrcNameLength = int.Parse(strSrc.Substring(0, 3));
					if(nSrcNameLength > 0)
					{
						_srcName = strSrc.Substring(3, nSrcNameLength);
					}
					strSrc = strSrc.Substring(3 + nSrcNameLength);

					int nDstNameLength = int.Parse(strSrc.Substring(0, 3));
					if(nDstNameLength > 0)
					{
						_dstName = strSrc.Substring(3, nDstNameLength);
					}
					strSrc = strSrc.Substring(3 + nDstNameLength);


					string[] strParse = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);

					_type = (TYPE)int.Parse(strParse[0]);
					_srcUnitID = int.Parse(strParse[1]);
					_srcUniqueID = int.Parse(strParse[2]);
					_dstUniqueID = int.Parse(strParse[3]);

					_isImported = int.Parse(strParse[4]) == 1 ? true : false;

					_subParam1 = int.Parse(strParse[5]);
					_subParam2 = int.Parse(strParse[6]);
				}
				catch(Exception ex)
				{
					Debug.LogError("DecodeData Exception : " + ex);
					return false;
				}
				return true;
			}
		}



		


		public class SimilarNameSet
		{	
			public object _srcObj = null;
			public apDialog_RetargetPose.TargetUnit _targetUnit = null;
			public int _weight = 0;

			public SimilarNameSet(object srcObj, apDialog_RetargetPose.TargetUnit targetUnit, string[] srcNames, string[] targetNames)
			{
				_srcObj = srcObj;
				_targetUnit = targetUnit;
				_weight = 0;
				//Weight를 계산하자
				for (int iSrc = 0; iSrc < srcNames.Length; iSrc++)
				{
					for (int iTar = 0; iTar < targetNames.Length; iTar++)
					{
						if(string.Equals(srcNames[iSrc], targetNames[iTar]))
						{
							//글자가 같다면
							if(srcNames[iSrc].Length < 2)
							{
								_weight += 10;
							}
							else
							{
								//글자가 길수록 Weight가 증가한다.
								_weight += 10 + (srcNames[iSrc].Length - 2);
							}
							 
							break;
						}
					}
				}
			}
		}



		// Init
		//---------------------------------------------------
		public apRetargetMapping()
		{

		}




		// Functions
		//---------------------------------------------------
		//Save
		public void SaveMapping(	string filePath,
									apRetargetAnimFile srcAnimFile)
		{
			FileStream fs = null;
			StreamWriter sw = null;

			try
			{
				//저장 데이터는
				//타입 + SrcUnit의 ID와 이름 + Import여부 + 대상의 UniqueID와 이름
				List<Mapping> mappings = new List<Mapping>();

				apRetargetSubUnit subUnit = null;
				for (int i = 0; i < srcAnimFile._transforms_Total.Count; i++)
				{
					subUnit = srcAnimFile._transforms_Total[i];

					if(subUnit._type == apRetargetSubUnit.TYPE.MeshTransform)
					{
						mappings.Add(
							new Mapping(
								subUnit._unitID,
								subUnit._uniqueID,
								subUnit._name,
								Mapping.TYPE.MeshTransform,
								(subUnit._targetMeshTransform != null ? subUnit._targetMeshTransform._transformUniqueID : -1),
								(subUnit._targetMeshTransform != null ? subUnit._targetMeshTransform._nickName : ""),
								subUnit._isImported
							));
					}
					else if(subUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform)
					{
						mappings.Add(
							new Mapping(
								subUnit._unitID,
								subUnit._uniqueID,
								subUnit._name,
								Mapping.TYPE.MeshGroupTransform,
								(subUnit._targetMeshGroupTransform != null ? subUnit._targetMeshGroupTransform._transformUniqueID : -1),
								(subUnit._targetMeshGroupTransform != null ? subUnit._targetMeshGroupTransform._nickName : ""),
								subUnit._isImported
							));
					}
				}

				for (int i = 0; i < srcAnimFile._bones_Total.Count; i++)
				{
					subUnit = srcAnimFile._bones_Total[i];
					mappings.Add(
							new Mapping(
								subUnit._unitID,
								subUnit._uniqueID,
								subUnit._name,
								Mapping.TYPE.Bone,
								(subUnit._targetBone != null ? subUnit._targetBone._uniqueID : -1),
								(subUnit._targetBone != null ? subUnit._targetBone._name : "<None>"),
								subUnit._isImported
							));
				}

				apRetargetControlParam cpUnit = null;
				for (int i = 0; i < srcAnimFile._controlParams.Count; i++)
				{
					cpUnit = srcAnimFile._controlParams[i];
					mappings.Add(
							new Mapping(
								cpUnit._unitID,
								cpUnit._controlParamUniqueID,
								cpUnit._keyName,
								Mapping.TYPE.ControlParam,
								(cpUnit._targetControlParam != null ? cpUnit._targetControlParam._uniqueID : -1),
								(cpUnit._targetControlParam != null ? cpUnit._targetControlParam._keyName : "<None>"),
								cpUnit._isImported,
								(int)cpUnit._valueType//<<ValueType이 추가된다.
							));
				}

				apRetargetTimelineUnit timelineUnit = null;
				for (int i = 0; i < srcAnimFile._timelineUnits.Count; i++)
				{
					timelineUnit = srcAnimFile._timelineUnits[i];
					mappings.Add(
							new Mapping(
								timelineUnit._unitID,
								timelineUnit._timelineUniqueID,
								"<Noname>",
								Mapping.TYPE.Timeline,
								(timelineUnit._targetTimeline != null ? timelineUnit._targetTimeline._uniqueID : -1),
								(timelineUnit._targetTimeline != null ? timelineUnit._targetTimeline.DisplayName : "<None>"),
								timelineUnit._isImported,
								(int)timelineUnit._linkType,
								(timelineUnit._linkType == apAnimClip.LINK_TYPE.AnimatedModifier ? (int)timelineUnit._linkedModifierType : -1)
							));
				}

				apRetargetAnimEvent eventUnit = null;
				for (int i = 0; i < srcAnimFile._animEvents.Count; i++)
				{
					eventUnit = srcAnimFile._animEvents[i];
					mappings.Add(
						new Mapping(
							eventUnit._frameIndex,
							eventUnit._frameIndex,
							eventUnit._eventName,
							Mapping.TYPE.Event,
							-1, "", eventUnit._isImported));
				}


				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				sw.WriteLine("---------------------------------------");
				sw.WriteLine(" Mapping Data");
				sw.WriteLine(mappings.Count);
				sw.WriteLine("---------------------------------------");

				for (int i = 0; i < mappings.Count; i++)
				{
					sw.WriteLine(mappings[i].GetEncodingData());
				}

				sw.Flush();

				sw.Close();
				fs.Close();

				sw = null;
				fs = null;
			}
			catch (Exception ex)
			{
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
				
				Debug.LogError("Save Mapping Exception : " + ex);
			}
		}



		// Load
		public void LoadMapping(	string filePath,
									apRetargetAnimFile srcAnimFile, 
									List<apDialog_RetargetPose.TargetUnit> targetTransforms,
									List<apDialog_RetargetPose.TargetUnit> targetBones,
									List<apDialog_RetargetPose.TargetUnit> targetControlParams,
									List<apDialog_RetargetPose.TargetUnit> targetTimelines
									)
		{
			FileStream fs = null;
			StreamReader sr = null;

			try
			{
				List<Mapping> map_Transform = new List<Mapping>();
				List<Mapping> map_Bone = new List<Mapping>();
				List<Mapping> map_ControlParam = new List<Mapping>();
				List<Mapping> map_Timeline = new List<Mapping>();
				List<Mapping> map_Event = new List<Mapping>();

				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제와 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);


				sr.ReadLine();//"----------"
				sr.ReadLine();//"Mapping Data"
				int nMapping = int.Parse(sr.ReadLine());
				sr.ReadLine();

				//하나씩 읽어서 디코드
				for (int i = 0; i < nMapping; i++)
				{
					Mapping newMap = new Mapping();
					if (newMap.DecodeData(sr.ReadLine()))
					{
						switch (newMap._type)
						{
							case Mapping.TYPE.MeshTransform:
							case Mapping.TYPE.MeshGroupTransform:
								map_Transform.Add(newMap);
								break;

							case Mapping.TYPE.Bone:
								map_Bone.Add(newMap);
								break;

							case Mapping.TYPE.ControlParam:
								map_ControlParam.Add(newMap);
								break;

							case Mapping.TYPE.Timeline:
								map_Timeline.Add(newMap);
								break;

							case Mapping.TYPE.Event:
								map_Event.Add(newMap);
								break;
						}
					}
				}



				sr.Close();
				fs.Close();

				sr = null;
				fs = null;

				//-------------------------------------------------------------
				//다 읽었다. 끝!... 이 아니라
				//TargetList를 확인해서 연결을 해준다.
				//1. Src 데이터를 읽는다 -> Mapping 데이터가 있는지 확인
				//2. Mapping 데이터가 있다면 Dst 데이터를 확인한다.
				//3. TargetList를 확인해서 연결을 해준다.
				//4. 중복 방지를 위한 처리도 해준다.
				//5. 마지막에 전체 Refresh

				//<1> Mesh / MeshGroup Transform
				apRetargetSubUnit tfUnit = null;
				apRetargetSubUnit boneUnit = null;
				apRetargetControlParam cpUnit = null;
				apRetargetTimelineUnit tlUnit = null;
				apRetargetAnimEvent eventUnit = null;

				Mapping linkMap = null;
				apDialog_RetargetPose.TargetUnit linkedTargetUnit = null;

				//서로 매핑하기 위한 Dictionary
				Dictionary<apRetargetSubUnit, apDialog_RetargetPose.TargetUnit> srcSubUnit2Target = new Dictionary<apRetargetSubUnit, apDialog_RetargetPose.TargetUnit>();
				Dictionary<apDialog_RetargetPose.TargetUnit, apRetargetSubUnit> target2SrcSubUnit = new Dictionary<apDialog_RetargetPose.TargetUnit, apRetargetSubUnit>();

				Dictionary<apRetargetControlParam, apDialog_RetargetPose.TargetUnit> srcCpUnit2Target = new Dictionary<apRetargetControlParam, apDialog_RetargetPose.TargetUnit>();
				Dictionary<apDialog_RetargetPose.TargetUnit, apRetargetControlParam> target2SrcCpUnit = new Dictionary<apDialog_RetargetPose.TargetUnit, apRetargetControlParam>();

				Dictionary<apRetargetTimelineUnit, apDialog_RetargetPose.TargetUnit> srcTlUnit2Target = new Dictionary<apRetargetTimelineUnit, apDialog_RetargetPose.TargetUnit>();
				Dictionary<apDialog_RetargetPose.TargetUnit, apRetargetTimelineUnit> target2SrcTlUnit = new Dictionary<apDialog_RetargetPose.TargetUnit, apRetargetTimelineUnit>();

				//<1> Mesh / MeshGroup Transforms 
				for (int i = 0; i < srcAnimFile._transforms_Total.Count; i++)
				{
					tfUnit = srcAnimFile._transforms_Total[i];
					linkMap = map_Transform.Find(delegate (Mapping a)
					{
						if(tfUnit._type == apRetargetSubUnit.TYPE.MeshTransform
							&& a._type == Mapping.TYPE.MeshTransform 
							&& a._srcUniqueID == tfUnit._uniqueID)
						{
							return true;
						}
						else if(tfUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform
							&& a._type == Mapping.TYPE.MeshGroupTransform
							&& a._srcUniqueID == tfUnit._uniqueID)
						{
							return true;
						}

						return false;
					});

					if(linkMap != null)
					{
						//연결정보가 존재한다.
						tfUnit._isImported = linkMap._isImported;

						//타겟이 존재하는지 찾자
						if (linkMap._dstUniqueID < 0)
						{
							//연결이 안되었다.
							linkedTargetUnit = null;
						}
						else
						{
							linkedTargetUnit = targetTransforms.Find(delegate (apDialog_RetargetPose.TargetUnit a)
							{
								if (tfUnit._type == apRetargetSubUnit.TYPE.MeshTransform)
								{
									if (a._type == apDialog_RetargetPose.TargetUnit.TYPE.MeshTransform
										&& a._meshTransform != null && a._meshTransform._transformUniqueID == linkMap._dstUniqueID)
									{
										return true;
									}
								}
								else if (tfUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform)
								{
									if (a._type == apDialog_RetargetPose.TargetUnit.TYPE.MeshGroupTransform
										&& a._meshGroupTransform != null && a._meshGroupTransform._transformUniqueID == linkMap._dstUniqueID)
									{
										return true;
									}
								}
								return false;
							});
						}

						//TODO. 여기서부터 작성
						if(linkedTargetUnit == null)
						{
							//연결을 지운다.
							tfUnit._targetMeshTransform = null;
							tfUnit._targetMeshGroupTransform = null;
						}
						else
						{
							//새로 연결을 만들고 다른 중복이 있다면 막는다.
							if (tfUnit._type == apRetargetSubUnit.TYPE.MeshTransform)
							{
								tfUnit._targetMeshTransform = linkedTargetUnit._meshTransform;
							}
							else
							{
								tfUnit._targetMeshGroupTransform = linkedTargetUnit._meshGroupTransform;
							}
							
							//연결 정보를 추가한다.
							srcSubUnit2Target.Add(tfUnit, linkedTargetUnit);
							target2SrcSubUnit.Add(linkedTargetUnit, tfUnit);
						}
						
					}
				}
				//이제 일괄적으로 src <-> target간의 연결을 다시 정리하자.
				for (int i = 0; i < srcAnimFile._transforms_Total.Count; i++)
				{
					tfUnit = srcAnimFile._transforms_Total[i];
					if(!srcSubUnit2Target.ContainsKey(tfUnit))
					{
						tfUnit._targetMeshTransform = null;
						tfUnit._targetMeshGroupTransform = null;
					}
				}

				for (int i = 0; i < targetTransforms.Count; i++)
				{
					apDialog_RetargetPose.TargetUnit targetUnit = targetTransforms[i];
					if(target2SrcSubUnit.ContainsKey(targetUnit))
					{
						targetUnit._linkedSubUnit = target2SrcSubUnit[targetUnit];
					}
					else
					{
						targetUnit._linkedSubUnit = null;
					}
				}
				srcSubUnit2Target.Clear();
				target2SrcSubUnit.Clear();


				//<2> Bone
				for (int i = 0; i < srcAnimFile._bones_Total.Count; i++)
				{
					boneUnit = srcAnimFile._bones_Total[i];
					linkMap = map_Bone.Find(delegate (Mapping a)
					{
						if(a._srcUniqueID == boneUnit._uniqueID)
						{
							return true;
						}

						return false;
					});

					if(linkMap != null)
					{
						//연결정보가 존재한다.
						boneUnit._isImported = linkMap._isImported;

						//타겟이 존재하는지 찾자
						if (linkMap._dstUniqueID < 0)
						{
							//연결이 안되었다.
							linkedTargetUnit = null;
						}
						else
						{
							linkedTargetUnit = targetBones.Find(delegate (apDialog_RetargetPose.TargetUnit a)
							{
								if(a._bone != null && a._bone._uniqueID == linkMap._dstUniqueID)
								{
									return true;
								}
								return false;
							});
						}

						if(linkedTargetUnit == null)
						{
							//연결을 지운다.
							boneUnit._targetBone = null;
						}
						else
						{
							//새로 연결을 만들고 다른 중복이 있다면 막는다.
							boneUnit._targetBone = linkedTargetUnit._bone;

							//연결 정보를 추가한다.
							srcSubUnit2Target.Add(boneUnit, linkedTargetUnit);
							target2SrcSubUnit.Add(linkedTargetUnit, boneUnit);
						}
						
					}
				}
				//이제 일괄적으로 src <-> target간의 연결을 다시 정리하자.
				for (int i = 0; i < srcAnimFile._bones_Total.Count; i++)
				{
					boneUnit = srcAnimFile._bones_Total[i];
					if(!srcSubUnit2Target.ContainsKey(boneUnit))
					{
						boneUnit._targetBone = null;
					}
				}

				for (int i = 0; i < targetBones.Count; i++)
				{
					apDialog_RetargetPose.TargetUnit targetUnit = targetBones[i];
					if(target2SrcSubUnit.ContainsKey(targetUnit))
					{
						targetUnit._linkedSubUnit = target2SrcSubUnit[targetUnit];
					}
					else
					{
						targetUnit._linkedSubUnit = null;
					}
				}
				srcSubUnit2Target.Clear();
				target2SrcSubUnit.Clear();




				//<3> Control Param
				for (int i = 0; i < srcAnimFile._controlParams.Count; i++)
				{
					cpUnit = srcAnimFile._controlParams[i];
					linkMap = map_ControlParam.Find(delegate (Mapping a)
					{
						if(a._srcUniqueID == cpUnit._controlParamUniqueID
						&& (apControlParam.TYPE)a._subParam1 == cpUnit._valueType)
						{
							return true;
						}

						return false;
					});

					if(linkMap != null)
					{
						//연결정보가 존재한다.
						cpUnit._isImported = linkMap._isImported;

						//타겟이 존재하는지 찾자
						if (linkMap._dstUniqueID < 0)
						{
							//연결이 안되었다.
							linkedTargetUnit = null;
						}
						else
						{
							linkedTargetUnit = targetControlParams.Find(delegate (apDialog_RetargetPose.TargetUnit a)
							{
								if(a._controlParam != null && a._controlParam._uniqueID == linkMap._dstUniqueID)
								{
									return true;
								}
								return false;
							});
						}

						if(linkedTargetUnit == null)
						{
							//연결을 지운다.
							cpUnit._targetControlParam = null;
						}
						else
						{
							//새로 연결을 만들고 다른 중복이 있다면 막는다.
							cpUnit._targetControlParam = linkedTargetUnit._controlParam;

							srcCpUnit2Target.Add(cpUnit, linkedTargetUnit);
							target2SrcCpUnit.Add(linkedTargetUnit, cpUnit);
						}
					}
				}
				//이제 일괄적으로 src <-> target간의 연결을 다시 정리하자.
				for (int i = 0; i < srcAnimFile._controlParams.Count; i++)
				{
					cpUnit = srcAnimFile._controlParams[i];
					if(!srcCpUnit2Target.ContainsKey(cpUnit))
					{
						cpUnit._targetControlParam = null;
					}
				}

				for (int i = 0; i < targetControlParams.Count; i++)
				{
					apDialog_RetargetPose.TargetUnit targetUnit = targetControlParams[i];
					if(target2SrcCpUnit.ContainsKey(targetUnit))
					{
						targetUnit._linkedControlParam = target2SrcCpUnit[targetUnit];
					}
					else
					{
						targetUnit._linkedControlParam = null;
					}
				}
				srcCpUnit2Target.Clear();
				target2SrcCpUnit.Clear();




				//<4> Timeline
				for (int i = 0; i < srcAnimFile._timelineUnits.Count; i++)
				{
					tlUnit = srcAnimFile._timelineUnits[i];
					linkMap = map_Timeline.Find(delegate (Mapping a)
					{
						if(a._srcUniqueID == tlUnit._timelineUniqueID
						&& a._subParam1 == (int)tlUnit._linkType)
						{
							if (tlUnit._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
							{
								if(a._subParam2 == (int)tlUnit._linkedModifierType)
								{
									return true;
								}
							}
							else
							{
								return true;
							}
						}

						return false;
					});

					if(linkMap != null)
					{
						//연결정보가 존재한다.
						//Debug.Log("Timeline Find : " + tlUnit._timelineUniqueID + " / " + linkMap._isImported);
						tlUnit._isImported = linkMap._isImported;

						//타겟이 존재하는지 찾자
						if (linkMap._dstUniqueID < 0)
						{
							//연결이 안되었다.
							linkedTargetUnit = null;
						}
						else
						{
							linkedTargetUnit = targetTimelines.Find(delegate (apDialog_RetargetPose.TargetUnit a)
							{
								if(a._timeline != null && a._timeline._uniqueID == linkMap._dstUniqueID
								&& a._timeline._linkType == tlUnit._linkType)
								{
									if(tlUnit._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
									{
										if(a._timeline._linkedModifier != null
										&& a._timeline._linkedModifier.ModifierType == tlUnit._linkedModifierType)
										{
											return true;
										}
									}
									else
									{
										return true;
									}
								}
								return false;
							});
						}

						if(linkedTargetUnit == null)
						{
							//연결을 지운다.
							tlUnit._targetTimeline = null;
						}
						else
						{
							//새로 연결을 만들고 다른 중복이 있다면 막는다.
							tlUnit._targetTimeline = linkedTargetUnit._timeline;

							//연결 정보를 추가한다.
							srcTlUnit2Target.Add(tlUnit, linkedTargetUnit);
							target2SrcTlUnit.Add(linkedTargetUnit, tlUnit);
						}
					}
				}

				//이제 일괄적으로 src <-> target간의 연결을 다시 정리하자.
				for (int i = 0; i < srcAnimFile._timelineUnits.Count; i++)
				{
					tlUnit = srcAnimFile._timelineUnits[i];
					if(!srcTlUnit2Target.ContainsKey(tlUnit))
					{
						tlUnit._targetTimeline = null;
					}
				}

				for (int i = 0; i < targetTimelines.Count; i++)
				{
					apDialog_RetargetPose.TargetUnit targetUnit = targetTimelines[i];
					if(target2SrcTlUnit.ContainsKey(targetUnit))
					{
						targetUnit._linkedTimelineUnit = target2SrcTlUnit[targetUnit];
					}
					else
					{
						targetUnit._linkedTimelineUnit = null;
					}
				}
				srcSubUnit2Target.Clear();
				target2SrcTlUnit.Clear();


				//5. Event
				for (int i = 0; i < srcAnimFile._animEvents.Count; i++)
				{
					eventUnit = srcAnimFile._animEvents[i];

					linkMap = map_Event.Find(delegate (Mapping a)
					{
						//이름이 같고 프레임이 같아야 한다.
						return string.Equals(a._srcName, eventUnit._eventName)
									&& a._srcUnitID == eventUnit._frameIndex;
					});

					if (linkMap != null)
					{
						//연결정보가 존재한다.
						eventUnit._isImported = linkMap._isImported;
					}

				}

			}
			catch (Exception ex)
			{
				Debug.LogError("Save Mapping Exception : " + ex);

				if(sr != null)
				{
					sr.Close();
					sr = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}
			}
		}


		// 자동 매핑
		//--------------------------------------------------------------------------------------
		public void AutoMapping_Transform(apRetargetAnimFile srcAnimFile, List<apDialog_RetargetPose.TargetUnit> targetTransforms)
		{
			//(1) Import가 되고 (2) 연결이 안된거 기준으로 타입 맞는 이름 비교를 한다.
			//1. " "나 "_", "-"로 구분된 단어들의 테이블을 만들고 숫자 인덱스로 만든다.
			//2. Src와 Target의 이름에 대한 <단어 인덱스 배열>을 만든다.
			//인덱스 배열의 가중치를 설정한다. (여러번 나올수록 점수가 낮다)

			//3. Src -> Target 로의 가중치가 포함된 유사도 점수를 List로 저장하여 Sort를 한다.
			//4. 점수 순으로 계산한다.
			// -> 대상을 정하지 말고, 가장 높은 점수 순으로 짝을 가져온다.
			// -> 일단 짝을 가져 온다면 전체 리스트에서 해당 결과의 Src와 Target을 모두 제외한다.
			
			List<apRetargetSubUnit> subUnits_Mesh = new List<apRetargetSubUnit>();
			List<apRetargetSubUnit> subUnits_MeshGroup = new List<apRetargetSubUnit>();

			List<apDialog_RetargetPose.TargetUnit> targetUnits_Mesh = new List<apDialog_RetargetPose.TargetUnit>();
			List<apDialog_RetargetPose.TargetUnit> targetUnits_MeshGroup = new List<apDialog_RetargetPose.TargetUnit>();

			apRetargetSubUnit subUnit = null;
			for (int i = 0; i < srcAnimFile._transforms_Total.Count; i++)
			{
				subUnit = srcAnimFile._transforms_Total[i];

				if (!subUnit._isImported)	{ continue; }
				if(subUnit.IsLinked)		{ continue; }

				if(subUnit._type == apRetargetSubUnit.TYPE.MeshTransform)
				{
					subUnits_Mesh.Add(subUnit);
				}
				else if(subUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform)
				{
					subUnits_MeshGroup.Add(subUnit);
				}
			}

			apDialog_RetargetPose.TargetUnit targetUnit = null;
			for (int i = 0; i < targetTransforms.Count; i++)
			{
				targetUnit = targetTransforms[i];
				if(targetUnit._linkedSubUnit != null
					&& targetUnit._linkedSubUnit._isImported)//<<Import 옵션까지 켜져있어야 함
				{
					continue;
				}

				if(targetUnit._type == apDialog_RetargetPose.TargetUnit.TYPE.MeshTransform)
				{
					targetUnits_Mesh.Add(targetUnit);
				}
				else if(targetUnit._type == apDialog_RetargetPose.TargetUnit.TYPE.MeshGroupTransform)
				{
					targetUnits_MeshGroup.Add(targetUnit);
				}
			}

			List<SimilarNameSet> simNameSets_Mesh = new List<SimilarNameSet>();
			List<SimilarNameSet> simNameSets_MeshGroup = new List<SimilarNameSet>();

			//Src-Target의 이름을 비교하여 Weight를 저장한다.
			string[] delimeter = new string[] { " ", "/", "_", "-" };

			
			for (int iSrc = 0; iSrc < subUnits_Mesh.Count; iSrc++)
			{
				subUnit = subUnits_Mesh[iSrc];

				for (int iTarget = 0; iTarget < targetUnits_Mesh.Count; iTarget++)
				{
					targetUnit = targetUnits_Mesh[iTarget];

					simNameSets_Mesh.Add(
						new SimilarNameSet(
							subUnit, targetUnit,
							subUnit._name.Split(delimeter, StringSplitOptions.None),
							targetUnit._name.Split(delimeter, StringSplitOptions.None)
							)
							);
				}
			}

			for (int iSrc = 0; iSrc < subUnits_MeshGroup.Count; iSrc++)
			{
				subUnit = subUnits_MeshGroup[iSrc];

				for (int iTarget = 0; iTarget < targetUnits_MeshGroup.Count; iTarget++)
				{
					targetUnit = targetUnits_MeshGroup[iTarget];

					simNameSets_MeshGroup.Add(
						new SimilarNameSet(
							subUnit, targetUnit,
							subUnit._name.Split(delimeter, StringSplitOptions.None),
							targetUnit._name.Split(delimeter, StringSplitOptions.None)
							)
							);
				}
			}

			
			//Weight 기준으로 정렬을 한다. (내림차순)
			simNameSets_Mesh.Sort(delegate (SimilarNameSet a, SimilarNameSet b)
			{
				return b._weight - a._weight;
			});

			simNameSets_MeshGroup.Sort(delegate (SimilarNameSet a, SimilarNameSet b)
			{
				return b._weight - a._weight;
			});

			//Weight 높은거 (앞에서부터) 하나씩 Result로 옮기고, 등장했던 객체는 List에 넣는다.
			List<object> usedObjects_Src = new List<object>();
			List<object> usedObjects_Target = new List<object>();

			SimilarNameSet simSet = null;
			int nResult_Mesh = 0;
			int nResult_MeshGroup = 0;

			List<SimilarNameSet> simNameSetResults_Mesh = new List<SimilarNameSet>();
			List<SimilarNameSet> simNameSetResults_MeshGroup = new List<SimilarNameSet>();


			for (int i = 0; i < simNameSets_Mesh.Count; i++)
			{
				simSet = simNameSets_Mesh[i];
				if(usedObjects_Src.Contains(simSet._srcObj))
				{
					//이미 처리된 Src
					continue;
				}
				if(usedObjects_Target.Contains(simSet._targetUnit))
				{
					//이미 처리된 Target
					continue;
				}

				simNameSetResults_Mesh.Add(simSet);
				nResult_Mesh++;

				usedObjects_Src.Add(simSet._srcObj);
				usedObjects_Target.Add(simSet._targetUnit);

				//이후 처리는 의미가 없다. 다 들어감
				//if(simNameSetResults_Mesh.Count >= subUnits_Mesh.Count)
				//{
				//	break;
				//}
				if (nResult_Mesh >= subUnits_Mesh.Count)
				{
					break;
				}
			}


			for (int i = 0; i < simNameSets_MeshGroup.Count; i++)
			{
				simSet = simNameSets_MeshGroup[i];
				if(usedObjects_Src.Contains(simSet._srcObj))
				{
					//이미 처리된 Src
					continue;
				}
				if(usedObjects_Target.Contains(simSet._targetUnit))
				{
					//이미 처리된 Target
					continue;
				}

				simNameSetResults_MeshGroup.Add(simSet);
				nResult_MeshGroup++;
				
				usedObjects_Src.Add(simSet._srcObj);
				usedObjects_Target.Add(simSet._targetUnit);

				//이후 처리는 의미가 없다. 다 들어감
				//if(simNameSetResults_MeshGroup.Count >= subUnits_MeshGroup.Count)
				//{
				//	break;
				//}
				if (nResult_MeshGroup >= subUnits_MeshGroup.Count)
				{
					break;
				}
			}

			//Result가 완료되었다. Result를 바탕으로 하나씩 연결하자.

			apRetargetSubUnit tfUnit = null;

			for (int i = 0; i < simNameSetResults_Mesh.Count; i++)
			{
				simSet = simNameSetResults_Mesh[i];
				tfUnit = simSet._srcObj as apRetargetSubUnit;

				tfUnit._targetMeshTransform = simSet._targetUnit._meshTransform;
				simSet._targetUnit._linkedSubUnit = tfUnit;
			}

			for (int i = 0; i < simNameSetResults_MeshGroup.Count; i++)
			{
				simSet = simNameSetResults_MeshGroup[i];
				tfUnit = simSet._srcObj as apRetargetSubUnit;

				tfUnit._targetMeshGroupTransform = simSet._targetUnit._meshGroupTransform;
				simSet._targetUnit._linkedSubUnit = tfUnit;
			}

			//연결인 끊긴 Src를 확실하게 끊어주자
			List<apRetargetSubUnit> _validSubUnits = new List<apRetargetSubUnit>();
			for (int i = 0; i < targetTransforms.Count; i++)
			{
				targetUnit = targetTransforms[i];
				if(targetUnit._linkedSubUnit != null && targetUnit._linkedSubUnit._isImported)
				{
					_validSubUnits.Add(targetUnit._linkedSubUnit);
				}
			}

			for (int i = 0; i < srcAnimFile._transforms_Total.Count; i++)
			{
				tfUnit = srcAnimFile._transforms_Total[i];
				
				if(!_validSubUnits.Contains(tfUnit))
				{
					//연결이 끊겼네요..
					tfUnit._targetMeshTransform = null;
					tfUnit._targetMeshGroupTransform = null;
				}
			}
		}






		public void AutoMapping_Bone(apRetargetAnimFile srcAnimFile, List<apDialog_RetargetPose.TargetUnit> targetBones)
		{
			//(1) Import가 되고 (2) 연결이 안된거 기준으로 타입 맞는 이름 비교를 한다.
			//1. " "나 "_", "-"로 구분된 단어들의 테이블을 만들고 숫자 인덱스로 만든다.
			//2. Src와 Target의 이름에 대한 <단어 인덱스 배열>을 만든다.
			//인덱스 배열의 가중치를 설정한다. (여러번 나올수록 점수가 낮다)

			//3. Src -> Target 로의 가중치가 포함된 유사도 점수를 List로 저장하여 Sort를 한다.
			//4. 점수 순으로 계산한다.
			// -> 대상을 정하지 말고, 가장 높은 점수 순으로 짝을 가져온다.
			// -> 일단 짝을 가져 온다면 전체 리스트에서 해당 결과의 Src와 Target을 모두 제외한다.
			
			List<apRetargetSubUnit> subUnits_Bone = new List<apRetargetSubUnit>();

			List<apDialog_RetargetPose.TargetUnit> targetUnits_Bone = new List<apDialog_RetargetPose.TargetUnit>();

			apRetargetSubUnit subUnit = null;
			for (int i = 0; i < srcAnimFile._bones_Total.Count; i++)
			{
				subUnit = srcAnimFile._bones_Total[i];

				if (!subUnit._isImported)	{ continue; }
				if(subUnit.IsLinked)		{ continue; }

				subUnits_Bone.Add(subUnit);
			}

			apDialog_RetargetPose.TargetUnit targetUnit = null;
			for (int i = 0; i < targetBones.Count; i++)
			{
				targetUnit = targetBones[i];
				if(targetUnit._linkedSubUnit != null
					&& targetUnit._linkedSubUnit._isImported)//Import 옵션까지 켜져있어야 함
				{
					continue;
				}

				targetUnits_Bone.Add(targetUnit);
			}

			List<SimilarNameSet> simNameSets_Bone = new List<SimilarNameSet>();

			//Src-Target의 이름을 비교하여 Weight를 저장한다.
			string[] delimeter = new string[] { " ", "/", "_", "-" };

			
			for (int iSrc = 0; iSrc < subUnits_Bone.Count; iSrc++)
			{
				subUnit = subUnits_Bone[iSrc];

				for (int iTarget = 0; iTarget < targetUnits_Bone.Count; iTarget++)
				{
					targetUnit = targetUnits_Bone[iTarget];

					simNameSets_Bone.Add(
						new SimilarNameSet(
							subUnit, targetUnit,
							subUnit._name.Split(delimeter, StringSplitOptions.None),
							targetUnit._name.Split(delimeter, StringSplitOptions.None)
							)
							);
				}
			}
			
			//Weight 기준으로 정렬을 한다. (내림차순)
			simNameSets_Bone.Sort(delegate (SimilarNameSet a, SimilarNameSet b)
			{
				return b._weight - a._weight;
			});

			
			//Weight 높은거 (앞에서부터) 하나씩 Result로 옮기고, 등장했던 객체는 List에 넣는다.
			List<object> usedObjects_Src = new List<object>();
			List<object> usedObjects_Target = new List<object>();

			SimilarNameSet simSet = null;
			int nResult_Bone = 0;

			List<SimilarNameSet> simNameSetResults_Bone = new List<SimilarNameSet>();


			for (int i = 0; i < simNameSets_Bone.Count; i++)
			{
				simSet = simNameSets_Bone[i];
				if(usedObjects_Src.Contains(simSet._srcObj))
				{
					//이미 처리된 Src
					continue;
				}
				if(usedObjects_Target.Contains(simSet._targetUnit))
				{
					//이미 처리된 Target
					continue;
				}

				simNameSetResults_Bone.Add(simSet);
				nResult_Bone++;

				usedObjects_Src.Add(simSet._srcObj);
				usedObjects_Target.Add(simSet._targetUnit);

				//이후 처리는 의미가 없다. 다 들어감
				if (nResult_Bone >= subUnits_Bone.Count)
				{
					break;
				}
			}
			

			//Result가 완료되었다. Result를 바탕으로 하나씩 연결하자.

			apRetargetSubUnit boneUnit = null;

			for (int i = 0; i < simNameSetResults_Bone.Count; i++)
			{
				simSet = simNameSetResults_Bone[i];
				boneUnit = simSet._srcObj as apRetargetSubUnit;

				boneUnit._targetBone = simSet._targetUnit._bone;
				simSet._targetUnit._linkedSubUnit = boneUnit;
			}
			

			//연결인 끊긴 Src를 확실하게 끊어주자
			List<apRetargetSubUnit> _validSubUnits = new List<apRetargetSubUnit>();
			for (int i = 0; i < targetBones.Count; i++)
			{
				targetUnit = targetBones[i];
				if(targetUnit._linkedSubUnit != null && targetUnit._linkedSubUnit._isImported)
				{
					_validSubUnits.Add(targetUnit._linkedSubUnit);
				}
			}

			for (int i = 0; i < srcAnimFile._bones_Total.Count; i++)
			{
				boneUnit = srcAnimFile._bones_Total[i];
				
				if(!_validSubUnits.Contains(boneUnit))
				{
					//연결이 끊겼네요..
					boneUnit._targetBone = null;
				}
			}
		}




		public void AutoMapping_ControlParam(apRetargetAnimFile srcAnimFile, List<apDialog_RetargetPose.TargetUnit> targetControlParams)
		{
			//(1) Import가 되고 (2) 연결이 안된거 기준으로 타입 맞는 이름 비교를 한다.
			//1. " "나 "_", "-"로 구분된 단어들의 테이블을 만들고 숫자 인덱스로 만든다.
			//2. Src와 Target의 이름에 대한 <단어 인덱스 배열>을 만든다.
			//인덱스 배열의 가중치를 설정한다. (여러번 나올수록 점수가 낮다)

			//3. Src -> Target 로의 가중치가 포함된 유사도 점수를 List로 저장하여 Sort를 한다.
			//4. 점수 순으로 계산한다.
			// -> 대상을 정하지 말고, 가장 높은 점수 순으로 짝을 가져온다.
			// -> 일단 짝을 가져 온다면 전체 리스트에서 해당 결과의 Src와 Target을 모두 제외한다.
			
			List<apRetargetControlParam> cpUnits_ControlParam = new List<apRetargetControlParam>();

			List<apDialog_RetargetPose.TargetUnit> targetUnits_ControlParam = new List<apDialog_RetargetPose.TargetUnit>();

			apRetargetControlParam cpUnit = null;
			for (int i = 0; i < srcAnimFile._controlParams.Count; i++)
			{
				cpUnit = srcAnimFile._controlParams[i];

				if (!cpUnit._isImported)	{ continue; }
				if(cpUnit.IsLinked)		{ continue; }

				cpUnits_ControlParam.Add(cpUnit);
			}

			apDialog_RetargetPose.TargetUnit targetUnit = null;
			for (int i = 0; i < targetControlParams.Count; i++)
			{
				targetUnit = targetControlParams[i];
				if(targetUnit._linkedControlParam != null
					&& targetUnit._linkedControlParam._isImported)//Import 옵션까지 켜져있어야 함
				{
					continue;
				}

				targetUnits_ControlParam.Add(targetUnit);
			}

			List<SimilarNameSet> simNameSets_ContronParam = new List<SimilarNameSet>();

			//Src-Target의 이름을 비교하여 Weight를 저장한다.
			string[] delimeter = new string[] { " ", "/", "_", "-" };

			
			for (int iSrc = 0; iSrc < cpUnits_ControlParam.Count; iSrc++)
			{
				cpUnit = cpUnits_ControlParam[iSrc];

				for (int iTarget = 0; iTarget < targetUnits_ControlParam.Count; iTarget++)
				{
					targetUnit = targetUnits_ControlParam[iTarget];

					//추가 : 컨트롤러 타입이 같아야 한다.
					if (cpUnit._valueType == targetUnit._controlParam._valueType)
					{
						simNameSets_ContronParam.Add(
							new SimilarNameSet(
								cpUnit, targetUnit,
								cpUnit._keyName.Split(delimeter, StringSplitOptions.None),
								targetUnit._name.Split(delimeter, StringSplitOptions.None)
								)
								);
					}
				}
			}
			
			//Weight 기준으로 정렬을 한다. (내림차순)
			simNameSets_ContronParam.Sort(delegate (SimilarNameSet a, SimilarNameSet b)
			{
				return b._weight - a._weight;
			});

			
			//Weight 높은거 (앞에서부터) 하나씩 Result로 옮기고, 등장했던 객체는 List에 넣는다.
			List<object> usedObjects_Src = new List<object>();
			List<object> usedObjects_Target = new List<object>();

			SimilarNameSet simSet = null;
			int nResult_ControlParam = 0;

			List<SimilarNameSet> simNameSetResults_ControlParam = new List<SimilarNameSet>();


			for (int i = 0; i < simNameSets_ContronParam.Count; i++)
			{
				simSet = simNameSets_ContronParam[i];
				if(usedObjects_Src.Contains(simSet._srcObj))
				{
					//이미 처리된 Src
					continue;
				}
				if(usedObjects_Target.Contains(simSet._targetUnit))
				{
					//이미 처리된 Target
					continue;
				}

				simNameSetResults_ControlParam.Add(simSet);
				nResult_ControlParam++;

				usedObjects_Src.Add(simSet._srcObj);
				usedObjects_Target.Add(simSet._targetUnit);

				//이후 처리는 의미가 없다. 다 들어감
				if (nResult_ControlParam >= cpUnits_ControlParam.Count)
				{
					break;
				}
			}
			

			//Result가 완료되었다. Result를 바탕으로 하나씩 연결하자.
			for (int i = 0; i < simNameSetResults_ControlParam.Count; i++)
			{
				simSet = simNameSetResults_ControlParam[i];
				cpUnit = simSet._srcObj as apRetargetControlParam;

				cpUnit._targetControlParam = simSet._targetUnit._controlParam;
				simSet._targetUnit._linkedControlParam = cpUnit;
			}
			

			//연결인 끊긴 Src를 확실하게 끊어주자
			List<apRetargetControlParam> _validCpUnits = new List<apRetargetControlParam>();
			for (int i = 0; i < targetControlParams.Count; i++)
			{
				targetUnit = targetControlParams[i];
				if(targetUnit._linkedControlParam != null && targetUnit._linkedControlParam._isImported)
				{
					_validCpUnits.Add(targetUnit._linkedControlParam);
				}
			}

			for (int i = 0; i < srcAnimFile._controlParams.Count; i++)
			{
				cpUnit = srcAnimFile._controlParams[i];
				
				if(!_validCpUnits.Contains(cpUnit))
				{
					//연결이 끊겼네요..
					cpUnit._targetControlParam = null;
				}
			}
		}


		public void AutoMapping_Timeline(apRetargetAnimFile srcAnimFile, List<apDialog_RetargetPose.TargetUnit> targetTimelines)
		{
			//(1) Import가 되고 (2) 연결이 안된거 기준으로 타입 맞는 이름 비교를 한다.
			// 타임라인을 자동으로 연결한다.
			// 타임라인은 이름이 아니라 그냥 타입이 맞는 것을 하나 찾자. 그 이상 처리 안함
			
			List<apRetargetTimelineUnit> tlUnits_Timeline = new List<apRetargetTimelineUnit>();

			List<apDialog_RetargetPose.TargetUnit> targetUnits_Timeline = new List<apDialog_RetargetPose.TargetUnit>();

			apRetargetTimelineUnit tlUnit = null;
			for (int i = 0; i < srcAnimFile._timelineUnits.Count; i++)
			{
				tlUnit = srcAnimFile._timelineUnits[i];

				if (!tlUnit._isImported)	{ continue; }
				if(tlUnit.IsLinked)		{ continue; }

				tlUnits_Timeline.Add(tlUnit);
			}

			apDialog_RetargetPose.TargetUnit targetUnit = null;
			for (int i = 0; i < targetTimelines.Count; i++)
			{
				targetUnit = targetTimelines[i];
				if(targetUnit._linkedTimelineUnit != null
					&& targetUnit._linkedTimelineUnit._isImported)//Import 옵션까지 켜져있어야 함
				{
					continue;
				}

				targetUnits_Timeline.Add(targetUnit);
			}

			//타입 맞는 것을 찾아서 연결하자
			for (int iSrc = 0; iSrc < tlUnits_Timeline.Count; iSrc++)
			{
				tlUnit = tlUnits_Timeline[iSrc];

				for (int iTarget = 0; iTarget < targetUnits_Timeline.Count; iTarget++)
				{
					targetUnit = targetUnits_Timeline[iTarget];

					if(targetUnit._timeline._linkType != tlUnit._linkType)
					{
						//타입이 안맞네용
						continue;
					}

					if (tlUnit._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						//더 볼것 없이 연결
						tlUnit._targetTimeline = targetUnit._timeline;

						targetUnit._isLinked = true;
						targetUnit._linkedTimelineUnit = tlUnit;
					}
					else if (tlUnit._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if(targetUnit._timeline._linkedModifier != null)
						{
							if (tlUnit._linkedModifierType == targetUnit._timeline._linkedModifier.ModifierType)
							{
								//Modifier 타입까지 같다면 연결
								tlUnit._targetTimeline = targetUnit._timeline;

								targetUnit._isLinked = true;
								targetUnit._linkedTimelineUnit = tlUnit;
							}
						}
						
					}
				}
			}

			

			//연결인 끊긴 Src를 확실하게 끊어주자
			List<apRetargetTimelineUnit> _validTlUnits = new List<apRetargetTimelineUnit>();
			for (int i = 0; i < targetTimelines.Count; i++)
			{
				targetUnit = targetTimelines[i];
				if(targetUnit._linkedTimelineUnit != null && targetUnit._linkedTimelineUnit._isImported)
				{
					_validTlUnits.Add(targetUnit._linkedTimelineUnit);
				}
			}

			for (int i = 0; i < srcAnimFile._timelineUnits.Count; i++)
			{
				tlUnit = srcAnimFile._timelineUnits[i];
				
				if(!_validTlUnits.Contains(tlUnit))
				{
					//연결이 끊겼네요..
					tlUnit._targetTimeline = null;
				}
			}
		}


		

		// Get / Set
		//---------------------------------------------------
	}
}
