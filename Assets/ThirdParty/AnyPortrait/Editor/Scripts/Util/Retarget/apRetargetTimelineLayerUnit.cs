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
	public class apRetargetTimelineLayerUnit
	{
		// Members
		//------------------------------------------------------
		// TimelineLayer를 저장한다.
		public int _unitID = -1;

		public string _displayName = "";

		public int _timelineLayerUniqueID = -1;
		public apAnimTimelineLayer _linkedTimelineLayer = null;

		public apAnimTimelineLayer.LINK_MOD_TYPE _linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.None;
		public int _transformID = -1;
		public int _boneID = -1;
		public string _targetTransformBoneName = "";
		

		public Color _guiColor = Color.black;
		public int _controlParamID = -1;
		public apControlParam.TYPE _controlValueType = apControlParam.TYPE.Int;

		public apAnimClip.LINK_TYPE _linkType = apAnimClip.LINK_TYPE.AnimatedModifier;

		public List<apRetargetKeyframeUnit> _keyframeUnits = new List<apRetargetKeyframeUnit>();

		
		public bool _isImported = false;
		//Control Param 타입에 한해서..
		public apAnimTimelineLayer _targetTimelineLayer_ControlParam = null;

		// Init
		//------------------------------------------------------
		public apRetargetTimelineLayerUnit()
		{

		}


		// Functions
		//------------------------------------------------------
		// AnimTimelineLayer -> File
		public void SetTimelineLayer(int unitID, apAnimTimelineLayer timelineLayer)
		{
			_unitID = unitID;

			_displayName = timelineLayer.DisplayName;

			_timelineLayerUniqueID = timelineLayer._uniqueID;
			_linkedTimelineLayer = timelineLayer;

			_linkModType = timelineLayer._linkModType;
			_transformID = timelineLayer._transformID;
			_boneID = timelineLayer._boneID;

		
			_guiColor = timelineLayer._guiColor;
			_controlParamID = timelineLayer._controlParamID;

			_controlValueType = apControlParam.TYPE.Int;
			if(timelineLayer._linkType == apAnimClip.LINK_TYPE.ControlParam
				&& timelineLayer._linkedControlParam != null)
			{
				_controlValueType = timelineLayer._linkedControlParam._valueType;
			}
			
			 

			_linkType = timelineLayer._linkType;

			_keyframeUnits.Clear();

			int curKeyframeUnitID = 0;
			for (int i = 0; i < timelineLayer._keyframes.Count; i++)
			{
				apRetargetKeyframeUnit newKeyUnit = new apRetargetKeyframeUnit();
				newKeyUnit.SetAnimKeyframe(curKeyframeUnitID, timelineLayer._keyframes[i]);

				_keyframeUnits.Add(newKeyUnit);
				curKeyframeUnitID++;
			}
		}

		public bool EncodeToFile(StreamWriter sw)
		{
			try
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				if(_displayName.Length < 10)
				{
					sb.Append("00"); sb.Append(_displayName.Length);
				}
				else if(_displayName.Length < 100)
				{
					sb.Append("0"); sb.Append(_displayName.Length);
				}
				else if(_displayName.Length < 100)
				{
					sb.Append(_displayName.Length);
				}

				if(_displayName.Length > 0)
				{
					sb.Append(_displayName);

				}
				

				sb.Append(_unitID); sb.Append("/");
				sb.Append(_timelineLayerUniqueID); sb.Append("/");

				sb.Append((int)_linkModType); sb.Append("/");
				sb.Append(_transformID); sb.Append("/");
				sb.Append(_boneID); sb.Append("/");

				sb.Append(_guiColor.r); sb.Append("/");
				sb.Append(_guiColor.g); sb.Append("/");
				sb.Append(_guiColor.b); sb.Append("/");
				sb.Append(_guiColor.a); sb.Append("/");

				sb.Append(_controlParamID); sb.Append("/");
				sb.Append((int)_controlValueType); sb.Append("/");
				sb.Append((int)_linkType); sb.Append("/");

				sb.Append(_keyframeUnits.Count); sb.Append("/");

				sw.WriteLine(sb.ToString());

				//Keyframe을 출력하자
				for (int i = 0; i < _keyframeUnits.Count; i++)
				{
					sw.WriteLine(_keyframeUnits[i].GetEncodingData());
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("EncodeToFile Exception : " + ex);
				return false;
			}

			return true;
		}


		// File -> Retarget TimelineLayer
		public bool DecodeData(StreamReader sr)
		{
			try
			{
				//일단 TimelineLayer의 기본 속성을 로드한다.
				string strHeader = sr.ReadLine();

				int nName = int.Parse(strHeader.Substring(0, 3));
				if(nName > 0)
				{
					_displayName = strHeader.Substring(3, nName);
				}
				else
				{
					_displayName = "<None>";
				}

				strHeader = strHeader.Substring(3 + nName);

				string[] strParse = strHeader.Split(new string[] { "/" }, StringSplitOptions.None);

				_unitID = int.Parse(strParse[0]);
				_timelineLayerUniqueID = int.Parse(strParse[1]);

				_linkModType = (apAnimTimelineLayer.LINK_MOD_TYPE)int.Parse(strParse[2]);
				_transformID = int.Parse(strParse[3]);
				_boneID = int.Parse(strParse[4]);

				_guiColor.r = apUtil.ParseFloat(strParse[5]);
				_guiColor.g = apUtil.ParseFloat(strParse[6]);
				_guiColor.b = apUtil.ParseFloat(strParse[7]);
				_guiColor.a = apUtil.ParseFloat(strParse[8]);

				_controlParamID = int.Parse(strParse[9]);
				_controlValueType = (apControlParam.TYPE)int.Parse(strParse[10]); 
				_linkType = (apAnimClip.LINK_TYPE)int.Parse(strParse[11]);

				int nKeyframes = int.Parse(strParse[12]);

				_keyframeUnits.Clear();
				for (int i = 0; i < nKeyframes; i++)
				{
					//Keyframe을 하나씩 파싱해서 넣어주자
					apRetargetKeyframeUnit keyframe = new apRetargetKeyframeUnit();
					keyframe.DecodeData(sr.ReadLine());

					_keyframeUnits.Add(keyframe);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Decode Data Exception : " + ex);
				return false;
			}
			return true;
		}

		// Get / Set
		//------------------------------------------------------
		public string LinkedControlParamName
		{
			get
			{
				if(!_isImported)
				{
					return "[ Not Imported ]";
				}
				if(_targetTimelineLayer_ControlParam != null)
				{
					return _targetTimelineLayer_ControlParam.DisplayName;
				}
				return "[ Not Selected ]";
			}
		}

		public bool IsLinked
		{
			get
			{
				return _targetTimelineLayer_ControlParam != null;
			}

		}
	}
}
