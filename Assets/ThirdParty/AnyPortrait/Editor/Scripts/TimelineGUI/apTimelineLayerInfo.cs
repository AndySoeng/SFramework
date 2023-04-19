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

	public class apTimelineLayerInfo
	{
		//Members
		//-----------------------------------------------------------------------------
		public bool _isTimeline = false;
		public apAnimTimeline _timeline = null;
		public apAnimTimelineLayer _layer = null;
		public apAnimTimeline _parentTimeline = null;
		public apTimelineLayerInfo _parentInfo = null;
		/// <summary>
		/// 선택된 상태인가
		/// Timeline : Layer 중 하나라도 선택이 되었으면 True
		/// Layer : 해당 객체(Transform/Bone/ControlParam)이 선택되거나 Frame이 선택되면 True
		/// </summary>
		public bool _isSelected = false;

		/// <summary>
		/// 활성화된 상태인가
		/// 기본적으로 True. Editing 상태일 때 선택된 Timeline을 제외하고는 모두 False가 된다.
		/// </summary>
		public bool _isAvailable = false;

		public bool IsVisibleLayer
		{
			get
			{
				if (_isTimeline || _layer == null)
				{
					return true;
				}
				return _layer._guiLayerVisible && !_parentInfo.IsTimelineFolded;
			}
		}

		public enum LAYER_TYPE
		{
			Transform,
			Bone,
			ControlParam
		}
		public LAYER_TYPE _layerType = LAYER_TYPE.Transform;

		public float _guiLayerPosY = 0;
		public bool _isRenderable = false;

		//public bool _isTimelineFold = false;
		public bool IsTimelineFolded
		{
			get
			{
				if (_timeline == null || !_isTimeline)
				{
					return false;
				}
				return _timeline._guiTimelineFolded;
			}
		}


		// Init
		//-----------------------------------------------------------------------------
		public apTimelineLayerInfo(apAnimTimeline timeline)
		{
			_isTimeline = true;
			_timeline = timeline;

			_isSelected = false;
			_isAvailable = true;
		}

		public apTimelineLayerInfo(apAnimTimelineLayer timelineLayer, apAnimTimeline parentTimeline, apTimelineLayerInfo parentInfo)
		{
			_isTimeline = false;
			_layer = timelineLayer;
			_parentTimeline = parentTimeline;
			_parentInfo = parentInfo;

			_isSelected = false;
			_isAvailable = true;

			if (_layer._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				switch (_layer._linkModType)
				{
					case apAnimTimelineLayer.LINK_MOD_TYPE.None:
						_layerType = LAYER_TYPE.Transform;
						break;

					case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
					case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
						_layerType = LAYER_TYPE.Transform;
						break;

					case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
						_layerType = LAYER_TYPE.Bone;
						break;


				}
			}
			else//if(_layer._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				_layerType = LAYER_TYPE.ControlParam;
			}
		}

		public void ShowLayer()
		{
			if (_layer != null)
			{
				_layer._guiLayerVisible = true;
			}

			if (_timeline != null)
			{
				_timeline._guiTimelineFolded = false;
			}
		}

		// Get / Set
		//-----------------------------------------------------------------------------
		public Color GUIColor
		{
			get
			{
				if (!_isAvailable)
				{ return new Color(0.2f, 0.2f, 0.2f); }

				Color resultColor = Color.black;
				if (_isTimeline)	{ resultColor = _timeline._guiColor; }
				else				{ resultColor = _layer._guiColor; }

				if (_isSelected)
				{
					float lum = (resultColor.r + resultColor.g + resultColor.b) / 3.0f;
					//lum = (lum * 1.2f) + 0.1f;
					resultColor.r += (resultColor.r - lum) * 0.2f;
					resultColor.g += (resultColor.g - lum) * 0.2f;
					resultColor.b += (resultColor.b - lum) * 0.2f;

					resultColor *= 1.4f;
					resultColor.a = 1.0f;
				}

				return resultColor;
			}
		}

		public void SetGUIColor(Color guiColor)
		{
			if (_isTimeline)
			{ _timeline._guiColor = guiColor; }
			else
			{ _layer._guiColor = guiColor; }
		}

		public Color TimelineColor
		{
			get
			{
				if (!_isAvailable || !_isSelected)
				{ return new Color(0.2f, 0.2f, 0.2f); }

				Color resultColor = Color.black;
				if (_isTimeline)
				{ resultColor = _timeline._guiColor; }
				else
				{ resultColor = _layer._guiColor; }

				float lum = (resultColor.r * 0.3f + resultColor.g * 0.6f + resultColor.b * 0.1f);
				//밝기를 보고, 0.25 근처가 되도록 만들자
				if (lum < 0.001f)
				{
					return new Color(0.27f, 0.27f, 0.27f);
				}

				float colorMul = 0.27f / lum;//어두우면 밝아지고, 너무 밝으면 줄어들도록

				resultColor.r *= colorMul;
				resultColor.g *= colorMul;
				resultColor.b *= colorMul;
				resultColor.a = 1.0f;

				return resultColor;
			}
		}

		public apImageSet.PRESET IconImgType
		{
			get
			{
				if (_isTimeline)
				{
					switch (_timeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							return apImageSet.PRESET.Anim_WithMod;
						//case apAnimClip.LINK_TYPE.Bone: return apImageSet.PRESET.Anim_WithBone;
						case apAnimClip.LINK_TYPE.ControlParam:
							return apImageSet.PRESET.Anim_WithControlParam;
					}
				}
				else
				{

					switch (_parentTimeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							if (_layer._linkedMeshTransform != null)
							{
								return apImageSet.PRESET.Hierarchy_Mesh;
							}
							if (_layer._linkedMeshGroupTransform != null)
							{
								return apImageSet.PRESET.Hierarchy_MeshGroup;
							}
							if (_layer._linkedBone != null)
							{
								return apImageSet.PRESET.Hierarchy_Bone;
							}
							return apImageSet.PRESET.Hierarchy_Modifier;
						//case apAnimClip.LINK_TYPE.Bone: return apImageSet.PRESET.Modifier_Rigging;
						case apAnimClip.LINK_TYPE.ControlParam:
							{
								if (_layer._linkedControlParam != null)
								{
									return apEditorUtil.GetControlParamPresetIconType(_layer._linkedControlParam._iconPreset);
								}
							}
							return apImageSet.PRESET.Hierarchy_Param;
					}
				}
				return apImageSet.PRESET.Edit_Record;
			}
		}

		public string DisplayName
		{
			get
			{
				if (_isTimeline)
				{ return _timeline.DisplayName; }
				else
				{ return _layer.DisplayName; }
			}
		}

		public int Depth
		{
			get
			{
				if (_layerType == LAYER_TYPE.ControlParam)
				{
					return 0;
				}
				else if (_layerType == LAYER_TYPE.Transform)
				{
					if (_layer._linkedMeshTransform != null &&
						_layer._linkedMeshTransform._linkedRenderUnit != null)
					{
						return _layer._linkedMeshTransform._linkedRenderUnit._guiIndex;
					}

					if (_layer._linkedMeshGroupTransform != null &&
						_layer._linkedMeshGroupTransform._linkedRenderUnit != null)
					{
						return _layer._linkedMeshGroupTransform._linkedRenderUnit._guiIndex;
					}
				}
				else
				{
					if (_layer._linkedBone != null)
					{
						return _layer._linkedBone._recursiveIndex;
					}
				}

				return 0;
			}
		}
	}

}