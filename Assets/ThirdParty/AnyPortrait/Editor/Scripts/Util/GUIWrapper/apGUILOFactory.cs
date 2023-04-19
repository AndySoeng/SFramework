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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	public class apGUILOFactory
	{
		// Members
		//-------------------------------------------
		private bool _isInitialized = false;
		private static apGUILOFactory s_instance = null;

		//크기별로 미리 GUILayoutOption을 다 만들고(Width / Height)
		//기본 범위 값을 만들고, 이것 이상으로 넘어가면 추가 생성한다.
		private const int SIZE_DEFAULT_WIDTH = 2500;//기본 1980 해상도
		private const int SIZE_DEFAULT_HEIGHT = 1600;//기본 1080 해상도

		private const int MARGIN_SIZE = 500;//여유 크기
		private const int INCREMENTAL_SIZE = 100;//증가시 크기 단위

		private int _curSizeMax_Width = 0;
		private int _curSizeMax_Height = 0;

		//빠른 접근을 위해서 배열로 만든다. (크기 변동시 다시 설정)
		private GUILayoutOption[] _LO_Width = null;
		private GUILayoutOption[] _LO_Height = null;

		// Init
		//-------------------------------------------
		public apGUILOFactory()
		{
			_isInitialized = false;
			s_instance = this;
		}

		public void Init()
		{
			if(_isInitialized)
			{
				return;
			}

			_isInitialized = true;
			s_instance = this;

			//GUILayoutOption을 여기서 만들자
			_curSizeMax_Width = SIZE_DEFAULT_WIDTH;
			_curSizeMax_Height = SIZE_DEFAULT_HEIGHT;

			_LO_Width = new GUILayoutOption[_curSizeMax_Width + 1];
			_LO_Height = new GUILayoutOption[_curSizeMax_Height + 1];

			
			//Width / Height 크기별로 Layout Option을 미리 만들자.
			for (int i = 0; i <= _curSizeMax_Width; i++)
			{
				_LO_Width[i] = GUILayout.Width(i);
			}

			for (int i = 0; i <= _curSizeMax_Height; i++)
			{
				_LO_Height[i] = GUILayout.Height(i);
			}
		}
		
		/// <summary>
		/// 큰 값의 Width가 들어올 것이 예상된다면 미리 함수를 호출해서 크기를 더 늘려야 할 지 체크해야한다.
		/// 속도를 위해서 매번 체크하지 않음
		/// </summary>
		/// <param name="nextWidth"></param>
		public void CheckSize_Width(int nextWidth)
		{
			nextWidth += MARGIN_SIZE;
			if(nextWidth <= _curSizeMax_Width)
			{
				return;
			}

			//크기를 늘려서 배열을 만들자
			int prevSize = _curSizeMax_Width;
			int nextSize = GetNextSize(_curSizeMax_Width, nextWidth);

			GUILayoutOption[] prevLO = _LO_Width;
			_LO_Width = new GUILayoutOption[nextSize + 1];

			//이전 데이터 복사
			for (int i = 0; i <= prevSize; i++)
			{
				_LO_Width[i] = prevLO[i];
			}

			//새로운 데이터 생성
			for (int i = prevSize + 1; i <= nextSize; i++)
			{
				_LO_Width[i] = GUILayout.Width(i);
			}

			_curSizeMax_Width = nextSize;
		}


		/// <summary>
		/// 큰 값의 Height가 들어올 것이 예상된다면 미리 함수를 호출해서 크기를 더 늘려야 할 지 체크해야한다.
		/// 속도를 위해서 매번 체크하지 않음
		/// </summary>
		/// <param name="nextHeight"></param>
		public void CheckSize_Height(int nextHeight)
		{
			nextHeight += MARGIN_SIZE;
			if(nextHeight <= _curSizeMax_Height)
			{
				return;
			}

			//크기를 늘려서 배열을 만들자
			int prevSize = _curSizeMax_Height;
			int nextSize = GetNextSize(_curSizeMax_Height, nextHeight);

			GUILayoutOption[] prevLO = _LO_Height;
			_LO_Height = new GUILayoutOption[nextSize + 1];

			//이전 데이터 복사
			for (int i = 0; i <= prevSize; i++)
			{
				_LO_Height[i] = prevLO[i];
			}

			//새로운 데이터 생성
			for (int i = prevSize + 1; i <= nextSize; i++)
			{
				_LO_Height[i] = GUILayout.Height(i);
			}

			_curSizeMax_Height = nextSize;
		}


		public void CheckSize(int nextWidth, int nextHeight)
		{
			CheckSize_Width(nextWidth);
			CheckSize_Height(nextHeight);
		}

		private int GetNextSize(int prevSize, int nextSize)
		{
			if(nextSize <= prevSize)
			{
				return prevSize;
			}
			int diff = nextSize - prevSize;
			float diffPerInc = (float)diff / (float)INCREMENTAL_SIZE;
			diffPerInc += 1.5f;
			int iDiffPerInc = (int)diffPerInc;
			int incSize = iDiffPerInc * INCREMENTAL_SIZE;
			return prevSize + incSize;
		}

		// Get
		//-------------------------------------------
		public static apGUILOFactory I {  get {  return s_instance; } }
		public bool IsInitialize() {  return _isInitialized; }

		//GUILayotOption 가져오기
		public GUILayoutOption Width(int width)
		{
			return _LO_Width[Mathf.Clamp(width, 0, _curSizeMax_Width)];
		}

		public GUILayoutOption Height(int height)
		{
			return _LO_Height[Mathf.Clamp(height, 0, _curSizeMax_Height)];
		}
	}
}