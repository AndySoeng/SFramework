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
//using UnityEditor;
//using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.6.3 : 작업 공간에 기준이 되는 선을 추가할 수 있다.
	/// </summary>
	[Serializable]
	public class apGuideLines
	{
		// Sub Class
		//-----------------------------------------
		[Serializable]
		public enum LINE_DIRECTION : int
		{
			Vertical = 0,
			Horizontal = 1,
		}

		[Serializable]
		public enum LINE_THICKNESS : int
		{
			Thin = 0,//가는 선
			Thick = 1,//두꺼운 선
		}

		[Serializable]
		public class LineInfo
		{
			[SerializeField]
			public bool _isEnabled = true;//작업 공간에 보여지는가

			[SerializeField]
			public LINE_DIRECTION _direction = LINE_DIRECTION.Vertical;//어느 방향으로 보여지는가

			[SerializeField]
			public int _position = 0;//위치 (기준점으로부터)

			[SerializeField]
			public Color _color = new Color(0.0f, 1.0f, 0.5f, 0.9f);//색상

			[SerializeField]
			public LINE_THICKNESS _thickness = LINE_THICKNESS.Thick;//선의 두께

			public LineInfo()
			{
				
			}

			public void CopyFromSrc(LineInfo src)
			{
				_isEnabled = src._isEnabled;
				_direction = src._direction;
				_position = src._position;
				_color = src._color;
				_thickness = src._thickness;
			}
		}

		// Members
		//----------------------------------------------------------
		[SerializeField]
		public List<LineInfo> _lineInfos = new List<LineInfo>();

		// Init
		//----------------------------------------------------------
		public apGuideLines()
		{

		}

		// Functions
		//----------------------------------------------------------
		/// <summary>
		/// 새로운 라인 추가
		/// </summary>
		/// <returns></returns>
		public LineInfo AddNewLine()
		{
			LineInfo newLine = new LineInfo();
			if(_lineInfos == null)
			{
				_lineInfos = new List<LineInfo>();
			}

			//마지막 라인을 복사하자
			if(_lineInfos.Count > 0)
			{
				LineInfo lastLine = _lineInfos[_lineInfos.Count - 1];
				if(lastLine != null)
				{
					newLine.CopyFromSrc(lastLine);
				}
				
			}
			_lineInfos.Add(newLine);
			return newLine;
		}

		/// <summary>
		/// 기존의 라인을 선택해서 복사하기
		/// </summary>
		/// <param name="srcLine"></param>
		/// <returns></returns>
		public LineInfo DuplicateLine(LineInfo srcLine)
		{
			if(srcLine == null)
			{
				return null;
			}

			LineInfo newLine = new LineInfo();
			newLine.CopyFromSrc(srcLine);
			if(_lineInfos == null)
			{
				_lineInfos = new List<LineInfo>();
			}
			_lineInfos.Add(newLine);
			return newLine;
		}

		/// <summary>
		/// 라인을 삭제하기
		/// </summary>
		/// <param name="targetLine"></param>
		public void RemoveLine(LineInfo targetLine)
		{
			if(_lineInfos == null)
			{
				return;
			}
			_lineInfos.Remove(targetLine);
		}

		/// <summary>
		/// 모든 라인 삭제하기
		/// </summary>
		public void RemoveAllLines()
		{
			if(_lineInfos == null)
			{
				return;
			}
			_lineInfos.Clear();
		}

		// Get
		//----------------------------------------------------------
		public int NumLines
		{
			get
			{
				return (_lineInfos != null) ? _lineInfos.Count : 0;
			}
		}

		public List<LineInfo> Lines { get { return _lineInfos; } }

	}
}