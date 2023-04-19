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
	//추가 v1.4.2 : 버텍스의 렌더 설정들을 저장하는 클래스.
	//옵션에 따라 자동으로 갱신되는 값인데, apBone의 RenderSettings와 같은 역할을 한다.
	//apEditor에 속해있다.
	public class apGUIRenderSettings
	{
		private float _scaleRatio = 1.0f;
		private float _vertexRenderSize = 1.0f;
		private float _vertexRenderSize_Half = 1.0f;
		private float _pinRenderSize = 1.0f;
		private float _pinRenderSize_Half = 1.0f;
		private float _pinLineThickness = 1.0f;

		//선택 범위도 설정한다.
		//선택의 경우, 정확한 클릭(Normal)과 적당히 가까운 클릭(Wide) 버전 두가지를 준비한다.
		private float _vertexSelectionRange_Normal = 1.0f;
		private float _vertexSelectionRange_Wide = 1.0f;

		private float _pinSelectionRange_Normal = 1.0f;
		private float _pinSelectionRange_Wide = 1.0f;

		//Edge 선택시 사용되는 거리 기준값
		//이건 해상도가 특별히 증가할때마다 올라간다. (저해상도일때 줄어들지는 않는다. 오직 고해상도일때 증가만 함)
		private float _edgeSelectBaseRange = 5.0f;
		private float _boneV1OutlineSelectBaseRange = 4.0f;
		
		//해상도에 따른 클릭 범위 보정값
		private float _clickRangeCorrectionByResolution = 1.0f;

		//기본 크기 (기존에 사용하던 값을 여기에 모았다.)
		private const float DEFAULT_VERT_RENDER_SIZE = 20.0f;
		private const float DEFAULT_PIN_RENDER_SIZE = 26.0f;
		private const float DEFAULT_PIN_LINE_THICKNESS = 2.5f;//라인 두께는 완전히 비례하지 않고 비율을 적게 적용한다.

		//선택 기본값은 렌더링 크기의 몇%인지로 결정. (절반을 기준으로 한다.)
		//Wide는 Normal 대비로 고정 길이 Offset을 가진다. (비율 약간만 적용)
		private const float DEFAULT_VERT_SELECT_RANGE_RATIO = 0.6f;//기본 렌더 크기 절반(10)의 60%인 6픽셀이 원래 클릭 범위
		private const float DEFAULT_VERT_SELECT_WIDE_OFFSET = 4.0f;//기본 클릭 범위(6)에 4픽셀을 더한 값이 원래 Wide 클릭 범위

		private const float DEFAULT_PIN_SELECT_RANGE_RATIO = 0.92f;//기본 렌더 크기 절반(13)의 92%인 12픽셀이 원래 클릭 범위
		private const float DEFAULT_PIN_SELECT_WIDE_OFFSET = 3.0f;//기본 클릭 범위(12)에 3픽셀을 더한 값이 원래 Wide 클릭 범위

		private const float DEFAULT_LINE_CLICK_RANGE = 5.0f;//Edge를 클릭해서 선택하는 기본 범위. 이건 오직 해상도에 의해 조금씩 늘어난다. (줄진 않는다.)
		private const float DEFAULT_BONE_OUTLINE_V1_CLICK_RANGE = 4.0f;//본 외곽선 클릭 범위 (V1)




		public apGUIRenderSettings()
		{
			Init();
		}

		public void Init()
		{
			_scaleRatio = 1.0f;
			
			_vertexRenderSize = DEFAULT_VERT_RENDER_SIZE;
			_vertexRenderSize_Half = _vertexRenderSize * 0.5f;
			
			_pinRenderSize = DEFAULT_PIN_RENDER_SIZE;
			_pinRenderSize_Half = _pinRenderSize * 0.5f;
			
			_pinLineThickness = DEFAULT_PIN_LINE_THICKNESS;

			_vertexSelectionRange_Normal = _vertexRenderSize_Half * DEFAULT_VERT_SELECT_RANGE_RATIO;
			_vertexSelectionRange_Wide = _vertexSelectionRange_Normal + DEFAULT_VERT_SELECT_WIDE_OFFSET;

			_pinSelectionRange_Normal = _pinRenderSize_Half * DEFAULT_PIN_SELECT_RANGE_RATIO;
			_pinSelectionRange_Wide = _pinSelectionRange_Normal + DEFAULT_PIN_SELECT_WIDE_OFFSET;

			_edgeSelectBaseRange = DEFAULT_LINE_CLICK_RANGE;
			_boneV1OutlineSelectBaseRange = DEFAULT_BONE_OUTLINE_V1_CLICK_RANGE;

			_clickRangeCorrectionByResolution = 1.0f;//보정비율은 1
		}



		// Functions
		//---------------------------------------------------------------------------
		/// <summary>현재 옵션 및 렌더 상태를 입력한다.</summary>
		public void UpdateRenderSettings(int vertexScaleRatio_x100, int workspaceHeight)
		{
			

			_scaleRatio = Mathf.Clamp((float)vertexScaleRatio_x100 * 0.01f, 0.1f, 8.0f);

			//버텍스 렌더 크기
			_vertexRenderSize = DEFAULT_VERT_RENDER_SIZE * _scaleRatio;
			_vertexRenderSize_Half = _vertexRenderSize * 0.5f;
			
			//핀 렌더 크기
			_pinRenderSize = DEFAULT_PIN_RENDER_SIZE * _scaleRatio;
			_pinRenderSize_Half = _pinRenderSize * 0.5f;
			
			//핀 선분 두께.
			//완전히 ScaleRatio를 다 반영하진 않고, 50%만 반영한다.
			float thickRatio = (_scaleRatio * 0.5f) + (1.0f * 0.5f);

			_pinLineThickness = DEFAULT_PIN_LINE_THICKNESS * thickRatio;

			//핀, 버텍스 선택 범위
			//Wide의 Offset이 ScaleRatio의 30%를 반영하면서 증가한다. 나머지는 렌더 크기를 기반으로 하므로 그대로 유지			
			//단, Wide Offset이 줄어들지는 않는다.
			//해상도에 따른 보정비율은 Wide에 들어간다.
			float offsetRatio = Mathf.Max((_scaleRatio * 0.3f) + (1.0f * 0.7f), 1.0f);

			//1080p (FHD)를 기준으로 조금씩 증가한다.
			//변화량은 QHD의 기준인 1440p의 직전인 1200를 시작점으로 삼는다.
			//2배인 2160 (4K)의 경우 클릭 범위는 1.8배 정도가 되도록 만든다.
			
			//테스트
			//workspaceHeight = 2160;
			
			_clickRangeCorrectionByResolution = 1.0f;
			if(workspaceHeight > 1200)
			{
				//1200일때 1배 > 2000일때 1.8배 (4K에선 약 1.9배)
				//800마다 x0.8배 추가 (더 큰 해상도에선 더 증가)				
				_clickRangeCorrectionByResolution = 1.0f + Mathf.Max((((float)(workspaceHeight - 1200) * 0.8f) / 800.0f), 0.0f);
			}
			

			_vertexSelectionRange_Normal = _vertexRenderSize_Half * DEFAULT_VERT_SELECT_RANGE_RATIO;
			_vertexSelectionRange_Wide = _vertexSelectionRange_Normal + (DEFAULT_VERT_SELECT_WIDE_OFFSET * offsetRatio * _clickRangeCorrectionByResolution);

			_pinSelectionRange_Normal = _pinRenderSize_Half * DEFAULT_PIN_SELECT_RANGE_RATIO;
			_pinSelectionRange_Wide = _pinSelectionRange_Normal + (DEFAULT_PIN_SELECT_WIDE_OFFSET * offsetRatio * _clickRangeCorrectionByResolution);


			//Edge 클릭 범위			
			_edgeSelectBaseRange = DEFAULT_LINE_CLICK_RANGE * _clickRangeCorrectionByResolution;

			//본 외곽선(V1) 클릭 범위
			_boneV1OutlineSelectBaseRange = DEFAULT_BONE_OUTLINE_V1_CLICK_RANGE * _clickRangeCorrectionByResolution;
		}


		// Get
		//---------------------------------------------------------------------
		/// <summary>버텍스의 렌더링 크기 (Full)</summary>
		public float VertesRenderSize			{ get { return _vertexRenderSize; } }

		/// <summary>버텍스의 렌더링 크기의 절반 (Half)</summary>
		public float VertexRenderSize_Half		{ get { return _vertexRenderSize_Half; } }

		/// <summary>핀의 렌더링 크기 (Full)</summary>
		public float PinRenderSize				{ get { return _pinRenderSize; } }

		/// <summary>핀의 렌더링 크기의 절반 (Half)</summary>
		public float PinRenderSize_Half			{ get { return _pinRenderSize_Half; } }

		/// <summary>핀 사이에 그려지는 라인의 굵기</summary>
		public float PinLineThickness			{ get { return _pinLineThickness; } }

		/// <summary>버텍스의 클릭 선택 범위 거리값 (정밀 클릭용)</summary>
		public float VertexSelectionRange_Normal	{ get { return _vertexSelectionRange_Normal; } }

		/// <summary>버텍스의 클릭 선택 범위 거리값 중 Wide 값 (넉넉한 판정 후 가장 가까운 버텍스 선택)</summary>
		public float VertexSelectionRange_Wide		{ get { return _vertexSelectionRange_Wide; } }

		/// <summary>핀 선택 범위 (정밀 클릭용)</summary>
		public float PinSelectionRange_Normal	{ get { return _pinSelectionRange_Normal; } }

		/// <summary>핀 선택 범위 (Wide)</summary>
		public float PinSelectionRange_Wide		{ get { return _pinSelectionRange_Wide; } }

		/// <summary>Edge 클릭 범위</summary>
		public float EdgeSelectBaseRange		{ get { return _edgeSelectBaseRange; } }

		/// <summary>본 외곽선(V1) 클릭 범위</summary>
		public float BoneV1OutlineSelectBaseRange { get { return _boneV1OutlineSelectBaseRange; } }

		/// <summary>해상도에 따른 클릭 범위 보정</summary>
		public float ClickRangeCorrectionByResolution { get { return _clickRangeCorrectionByResolution; } }
	}
}