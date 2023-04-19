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
	/// <summary>
	/// 일반적인 String을 개선하여 최적화하는 클래스. 
	/// StringBuilder를 이용하며, index를 이용하여 string 값을 미리 저장할 수 있다.
	/// </summary>
	public class apStringWrapper
	{
		// Members
		//-----------------------------------------------
		private StringBuilder _stringBuilder = null;
		private int _capacity = 1;
		private string _resultString = null;

		//기본 포맷을 지정할 수 있다.
		private List<string> _presetTexts = null;

		private const string TEXT_NONE = "";
		private const string TEXT_SPACE_1 = " ";
		private const string TEXT_SPACE_2 = "  ";
		private const string TEXT_SPACE_3 = "   ";
		private const string TEXT_SPACE_4 = "    ";
		private const string TEXT_SPACE_5 = "     ";

		// Make
		//-----------------------------------------------
		/// <summary>
		/// 고정된 길이의 텍스트를 생성합니다.
		/// Capacity는 입력된 텍스트 길이의 2배로 설정합니다.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static apStringWrapper MakeStaticText(string text)
		{
			apStringWrapper stringWrapper = new apStringWrapper(text.Length * 2);
			stringWrapper.Clear();
			stringWrapper.SetText(text);
			return stringWrapper;
		}
		
		// Init
		//-----------------------------------------------
		public apStringWrapper(int capacity)
		{
			_capacity = capacity;
			_stringBuilder = new StringBuilder(capacity);

			_presetTexts = null;

			_resultString = TEXT_NONE;
		}



		// Function
		//-----------------------------------------------
		public void Clear()
		{
			_stringBuilder.Length = 0;
			_stringBuilder.Capacity = 1;
			_stringBuilder.Capacity = _capacity;
		}


		public override string ToString()
		{
			//return _stringBuilder.ToString();
			return _resultString;
		}

		public void MakeString()
		{
			_resultString = _stringBuilder.ToString();
		}

		//미리 문자열 생성해서 저장하기
		public int SavePresetText(string strValue)
		{
			if(_presetTexts == null)
			{
				_presetTexts = new List<string>();
			}
			int curIndex = _presetTexts.Count;
			_presetTexts.Add(strValue);

			return curIndex;
		}

		public void ClearPresetText()
		{
			_presetTexts.Clear();
		}

		//Append/Set 함수
		/// <summary>
		/// 텍스트를 직접 입력한다.
		/// (Clear() 포함 / String 생성)
		/// </summary>
		/// <param name="strValue"></param>
		public void SetText(string strValue)
		{
			Clear();
			_stringBuilder.Append(strValue);

			_resultString = _stringBuilder.ToString();
		}

		/// <summary>
		/// 앞에 여백을 포함하여 텍스트를 직접 입력한다.
		/// (Clear() 포함 / 여백은 최대 5개까지 / String 생성)
		/// </summary>
		/// <param name="nSpace"></param>
		/// <param name="strValue"></param>
		public void SetText(int nSpace, string strValue)
		{
			Clear();
			switch (nSpace)
			{
				case 1: _stringBuilder.Append(TEXT_SPACE_1); break;
				case 2: _stringBuilder.Append(TEXT_SPACE_2); break;
				case 3: _stringBuilder.Append(TEXT_SPACE_3); break;
				case 4: _stringBuilder.Append(TEXT_SPACE_4); break;
				case 5:
				default:
					_stringBuilder.Append(TEXT_SPACE_5); break;
			}

			_stringBuilder.Append(strValue);

			_resultString = _stringBuilder.ToString();
		}

		/// <summary>
		/// 여백을 추가한다.
		/// </summary>
		/// <param name="nSpace"></param>
		/// <param name="isMakeString"></param>
		public void AppendSpace(int nSpace, bool isMakeString)
		{
			switch (nSpace)
			{
				case 1: _stringBuilder.Append(TEXT_SPACE_1); break;
				case 2: _stringBuilder.Append(TEXT_SPACE_2); break;
				case 3: _stringBuilder.Append(TEXT_SPACE_3); break;
				case 4: _stringBuilder.Append(TEXT_SPACE_4); break;
				case 5:
				default:
					_stringBuilder.Append(TEXT_SPACE_5); break;
			}

			if(isMakeString)
			{
				_resultString = _stringBuilder.ToString();
			}
		}


		/// <summary>
		/// string 값을 추가한다.
		/// </summary>
		public void Append(string value, bool isMakeString)
		{	
			_stringBuilder.Append(value);

			if(isMakeString)
			{
				_resultString = _stringBuilder.ToString();
			}
		}
		/// <summary>
		/// int 값을 추가한다.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isMakeString"></param>
		public void Append(int value, bool isMakeString)
		{	
			_stringBuilder.Append(value);

			if(isMakeString)
			{
				_resultString = _stringBuilder.ToString();
			}
		}

		/// <summary>
		/// float 값을 추가한다.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isMakeString"></param>
		public void Append(float value, bool isMakeString)
		{	
			_stringBuilder.Append(value);

			if(isMakeString)
			{
				_resultString = _stringBuilder.ToString();
			}
		}

		/// <summary>
		/// Vector2 값을 추가한다.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isMakeString"></param>
		public void Append(Vector2 value, bool isMakeString)
		{	
			_stringBuilder.Append(value);

			if(isMakeString)
			{
				_resultString = _stringBuilder.ToString();
			}
		}
		
		/// <summary>
		/// 저장해둔 프리셋을 추가한다.
		/// </summary>
		/// <param name="presetIndex"></param>
		/// <param name="isMakeString"></param>
		public void AppendPreset(int presetIndex, bool isMakeString)
		{
			if(_presetTexts == null)
			{
				return;
			}
			if(presetIndex < 0 || presetIndex >= _presetTexts.Count)
			{
				return;
			}
			_stringBuilder.Append(_presetTexts[presetIndex]);

			if(isMakeString)
			{
				_resultString = _stringBuilder.ToString();
			}

		}

	}
}