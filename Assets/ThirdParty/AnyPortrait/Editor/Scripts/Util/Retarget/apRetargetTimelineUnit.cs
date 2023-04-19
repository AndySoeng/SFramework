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
	public class apRetargetTimelineUnit
	{
		// Members
		//-----------------------------------------------
		public int _unitID = -1;

		public int _timelineUniqueID = -1;
		public apAnimTimeline _linkedTimeline = null;

		public Color _guiColor = Color.black;

		public apAnimClip.LINK_TYPE _linkType = apAnimClip.LINK_TYPE.AnimatedModifier;
		public int _modifierUniqueID = -1;
		public apModifierBase.MODIFIER_TYPE _linkedModifierType = apModifierBase.MODIFIER_TYPE.Base;

		public List<apRetargetTimelineLayerUnit> _layerUnits = new List<apRetargetTimelineLayerUnit>();


		// Import 설정
		public bool _isImported = false;
		public apAnimTimeline _targetTimeline = null;
		public bool _isFold = true;

		// Init
		//-----------------------------------------------
		public apRetargetTimelineUnit()
		{

		}
		
		// Functions
		//-----------------------------------------------
		// Timeline -> File
		//---------------------------------------------
		public void SetTimeline(int unitID, apAnimTimeline timeline)
		{
			_unitID = unitID;

			_timelineUniqueID = timeline._uniqueID;
			_linkedTimeline = timeline;

			_guiColor = timeline._guiColor;

			_linkType = timeline._linkType;
			_modifierUniqueID = timeline._modifierUniqueID;

			//연결된 Modifier의 Type을 넣자.
			//같은걸 대입하기 위함 (UniqueID를 찾지 못했다면..)
			_linkedModifierType = apModifierBase.MODIFIER_TYPE.Base;
			if(timeline._linkedModifier != null && _linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				_linkedModifierType = timeline._linkedModifier.ModifierType;
			}

			_layerUnits.Clear();
			int curUnitID = 0;
			for (int i = 0; i < timeline._layers.Count; i++)
			{
				//Layer도 넣자
				apRetargetTimelineLayerUnit layerUnit = new apRetargetTimelineLayerUnit();
				layerUnit.SetTimelineLayer(curUnitID, timeline._layers[i]);

				curUnitID++;
				_layerUnits.Add(layerUnit);
			}
		}

		public bool EncodeToFile(StreamWriter sw)
		{
			try
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.Append(_unitID); sb.Append("/");
				sb.Append(_timelineUniqueID); sb.Append("/");

				sb.Append(_guiColor.r); sb.Append("/");
				sb.Append(_guiColor.g); sb.Append("/");
				sb.Append(_guiColor.b); sb.Append("/");
				sb.Append(_guiColor.a); sb.Append("/");

				sb.Append((int)_linkType); sb.Append("/");
				sb.Append(_modifierUniqueID); sb.Append("/");
				sb.Append((int)_linkedModifierType); sb.Append("/");

				sb.Append(_layerUnits.Count); sb.Append("/");

				sw.WriteLine(sb.ToString());

				//다음 줄부터 TimelineLayer 정보를 넣자
				for (int i = 0; i < _layerUnits.Count; i++)
				{
					_layerUnits[i].EncodeToFile(sw);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("EncodeToFile Exception : " + ex);
				return false;
			}
			return true;
		}


		public bool DecodeData(StreamReader sr)
		{
			try
			{
				string strHeader = sr.ReadLine();
				string[] strParse = strHeader.Split(new string[] { "/" }, StringSplitOptions.None);

				//Timeline Header 정보를 먼저 파싱하자
				_unitID = int.Parse(strParse[0]);
				_timelineUniqueID = int.Parse(strParse[1]);

				_guiColor.r = apUtil.ParseFloat(strParse[2]);
				_guiColor.g = apUtil.ParseFloat(strParse[3]);
				_guiColor.b = apUtil.ParseFloat(strParse[4]);
				_guiColor.a = apUtil.ParseFloat(strParse[5]);

				_linkType = (apAnimClip.LINK_TYPE)int.Parse(strParse[6]);
				_modifierUniqueID = int.Parse(strParse[7]);
				_linkedModifierType = (apModifierBase.MODIFIER_TYPE)int.Parse(strParse[8]);
				
				int nLayers = int.Parse(strParse[9]);
				_layerUnits.Clear();

				for (int i = 0; i < nLayers; i++)
				{
					//Layer를 하나씩 Decode한다.
					apRetargetTimelineLayerUnit layerUnit = new apRetargetTimelineLayerUnit();
					layerUnit.DecodeData(sr);
					_layerUnits.Add(layerUnit);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("DecodeData Exception : " + ex);
				return false;
			}

			return true;
		}



		// Get / Set
		//-----------------------------------------------
		public string LinkedName
		{
			get
			{
				if(!_isImported)
				{
					return "[ Not Imported ]";
				}
				if(_targetTimeline != null)
				{
					return _targetTimeline.DisplayName;
				}
				return "[ Not Selected ]";
			}
		}

		public bool IsLinked
		{
			get
			{
				return _targetTimeline != null;
			}
		}
	}
}