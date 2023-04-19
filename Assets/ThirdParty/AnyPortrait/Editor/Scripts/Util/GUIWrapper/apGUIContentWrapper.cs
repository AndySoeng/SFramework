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
	//19.11.16 : GUI Content대신 이걸 쓰자
	//+ 임시로 쓰지 말고 항상 멤버로 만들 것
	//리스트인 경우 Pool 이용할 것
	public class apGUIContentWrapper
	{
		// Member
		//---------------------------------------------
		private GUIContent _guiContent = null;
		private StringBuilder _sb_Text = null;
		private StringBuilder _sb_ToolTip = null;//툴팁용. 이건 기본적으로 False
		private bool _isLongText = false;
		private const int TEXT_CAPACITY_DEFAULT = 64;
		private const int TEXT_CAPACITY_LONG = 128;

		private const int TOOLTIP_CAPACITY = 256;

		private bool _isVisible = false;
		private const string WHITE_SPACE_1 = " ";
		private const string WHITE_SPACE_2 = "  ";
		private const string WHITE_SPACE_3 = "   ";
		private const string WHITE_SPACE_4 = "    ";
		private const string WHITE_SPACE_5 = "     ";

		// Get
		//---------------------------------------------
		public GUIContent Content { get {  return _guiContent; } }
		public bool IsVisible { get { return _isVisible; } }

		// Init
		//---------------------------------------------
		public apGUIContentWrapper(bool isLongText = false)
		{
			_guiContent = new GUIContent();

			_isLongText = isLongText;
			_sb_Text = null;//<<일단 텍스트도 null
			_sb_ToolTip = null;
			_isVisible = true;//기본적으로는 보임 (굳이 생성했는데 안보인다고 하는 경우는 적으니)

			_guiContent.text = null;
			_guiContent.image = null;
			_guiContent.tooltip = null;
		}

		public void ClearAll()
		{
			if (_sb_Text != null)
			{
				ClearText(true);
			}

			_guiContent.image = null;
			if(_sb_ToolTip != null)
			{
				ClearToolTip(true);
			}
		}

		public void SetVisible(bool isVisible)
		{
			_isVisible = isVisible;
		}



		// Make
		//---------------------------------------------
		public static apGUIContentWrapper Make(string text, bool isLongText)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(isLongText);
			newGUIContent.SetText(text);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(string text, bool isLongText, Texture2D image)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(isLongText);
			newGUIContent.SetText(text);
			newGUIContent.SetImage(image);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(int space, string text)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetText(space, text);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(int space, string text, Texture2D image)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetText(space, text);
			newGUIContent.SetImage(image);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(int space, string text, string tooltip)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetText(space, text);
			newGUIContent.SetToolTip(tooltip);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(string text, bool isLongText, Texture2D image, string tooltip)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(isLongText);
			newGUIContent.SetText(text);
			newGUIContent.SetImage(image);
			newGUIContent.SetToolTip(tooltip);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(string text, Texture2D image, string tooltip)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetText(text);
			newGUIContent.SetImage(image);
			newGUIContent.SetToolTip(tooltip);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(int space, string text, Texture2D image, string tooltip)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetText(space, text);
			newGUIContent.SetImage(image);
			newGUIContent.SetToolTip(tooltip);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(Texture2D image, string tooltip)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetImage(image);
			newGUIContent.SetToolTip(tooltip);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(Texture2D image)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(false);
			newGUIContent.SetImage(image);
			return newGUIContent;
		}

		public static apGUIContentWrapper Make(string text, bool isLongText, string tooltip)
		{
			apGUIContentWrapper newGUIContent = new apGUIContentWrapper(isLongText);
			newGUIContent.SetText(text);
			newGUIContent.SetToolTip(tooltip);
			return newGUIContent;
		}

		// 공통
		//---------------------------------------------
		private static apGUIContentWrapper _sEmptyText = null;
		public static apGUIContentWrapper Empty
		{
			get
			{
				if(_sEmptyText == null)
				{
					_sEmptyText = Make("", false);
				}
				return _sEmptyText;
			}
		}

		// Functions
		//---------------------------------------------
		

		//1. Text
		//-------------------------------------------------
		public void ClearText(bool applyToContent)
		{
			if (_sb_Text == null)
			{
				_sb_Text = new StringBuilder(_isLongText ? TEXT_CAPACITY_LONG : TEXT_CAPACITY_DEFAULT);
			}

			_sb_Text.Length = 0;
			_sb_Text.Capacity = 1;

			if (_isLongText)
			{
				_sb_Text.Capacity = TEXT_CAPACITY_LONG;
			}
			else
			{
				_sb_Text.Capacity = TEXT_CAPACITY_DEFAULT;
			}

			if(applyToContent)
			{
				_guiContent.text = _sb_Text.ToString();
			}
		}

		/// <summary>
		/// 텍스트 직접 입력 (Clear 포함)
		/// GUIContent에 바로 적용된다.
		/// </summary>
		/// <param name="text"></param>
		public void SetText(string text)
		{
			ClearText(false);
			_sb_Text.Append(text);

			_guiContent.text = _sb_Text.ToString();
			_isVisible = true;
		}

		public void SetText(int space, string text)
		{
			ClearText(false);
			AppendSpaceText(space, false);
			AppendText(text, true);
		}

		/// <summary>
		/// Text에 공백을 넣는다. 최대 5개
		/// </summary>
		/// <param name="nSpace"></param>
		public void AppendSpaceText(int nSpace, bool applyToContent)
		{
			if (_sb_Text == null)
			{
				_sb_Text = new StringBuilder(_isLongText ? TEXT_CAPACITY_LONG : TEXT_CAPACITY_DEFAULT);
			}

			switch (nSpace)
			{
				case 1: _sb_Text.Append(WHITE_SPACE_1); break;
				case 2: _sb_Text.Append(WHITE_SPACE_2); break;
				case 3: _sb_Text.Append(WHITE_SPACE_3); break;
				case 4: _sb_Text.Append(WHITE_SPACE_4); break;
				case 5: _sb_Text.Append(WHITE_SPACE_5); break;
				default:
					_sb_Text.Append(WHITE_SPACE_5);
					break;
			}

			if(applyToContent)
			{
				_guiContent.text = _sb_Text.ToString();
			}
			_isVisible = true;
		}

		/// <summary>
		/// 텍스트 추가 (Clear 없음)
		/// </summary>
		/// <param name="text"></param>
		public void AppendText(string text, bool applyToContent)
		{
			if (_sb_Text == null)
			{
				_sb_Text = new StringBuilder(_isLongText ? TEXT_CAPACITY_LONG : TEXT_CAPACITY_DEFAULT);
			}

			_sb_Text.Append(text);

			if(applyToContent)
			{
				_guiContent.text = _sb_Text.ToString();
			}
			_isVisible = true;
		}

		public void AppendText(int intValue, bool applyToContent)
		{
			if (_sb_Text == null)
			{
				_sb_Text = new StringBuilder(_isLongText ? TEXT_CAPACITY_LONG : TEXT_CAPACITY_DEFAULT);
			}

			_sb_Text.Append(intValue);

			if(applyToContent)
			{
				_guiContent.text = _sb_Text.ToString();
			}
			_isVisible = true;
		}

		public void AppendText(float floatValue, bool applyToContent)
		{
			if (_sb_Text == null)
			{
				_sb_Text = new StringBuilder(_isLongText ? TEXT_CAPACITY_LONG : TEXT_CAPACITY_DEFAULT);
			}

			_sb_Text.Append(floatValue);

			if(applyToContent)
			{
				_guiContent.text = _sb_Text.ToString();
			}
			_isVisible = true;
		}

		public void AppendText(Vector2 vec2Value, bool applyToContent)
		{
			if (_sb_Text == null)
			{
				_sb_Text = new StringBuilder(_isLongText ? TEXT_CAPACITY_LONG : TEXT_CAPACITY_DEFAULT);
			}

			_sb_Text.Append('(');
			_sb_Text.Append(vec2Value.x);
			_sb_Text.Append(',');
			_sb_Text.Append(' ');
			_sb_Text.Append(vec2Value.y);
			_sb_Text.Append(')');

			if(applyToContent)
			{
				_guiContent.text = _sb_Text.ToString();
			}
			_isVisible = true;
		}


		//2. Image
		//-------------------------------------------------
		public void SetImage(Texture2D image)
		{
			_guiContent.image = image;
			if(image != null)
			{
				_isVisible = true;
			}
			
		}


		//3. Tooltip
		//-------------------------------------------------
		public void ClearToolTip(bool applyToContent)
		{
			if(_sb_ToolTip == null)
			{
				_sb_ToolTip = new StringBuilder(TOOLTIP_CAPACITY);
			}
			_sb_ToolTip.Length = 0;
			_sb_ToolTip.Capacity = 1;
			_sb_ToolTip.Capacity = TOOLTIP_CAPACITY;

			

			if(applyToContent)
			{
				//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
#if UNITY_2019_3_OR_NEWER
				_guiContent.tooltip = null;
#else
				_guiContent.tooltip = _sb_ToolTip.ToString();
#endif		
			}
		}

		/// <summary>
		/// 텍스트 직접 입력 (Clear 포함)
		/// </summary>
		/// <param name="text"></param>
		public void SetToolTip(string tooltip)
		{	
#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
#else
			ClearToolTip(false);
			_sb_ToolTip.Append(tooltip);

			_guiContent.tooltip = _sb_ToolTip.ToString();
#endif	
			_isVisible = true;
		}

		/// <summary>
		/// 텍스트 추가 (Clear 없음)
		/// </summary>
		/// <param name="text"></param>
		public void AppendToolTip(string text, bool applyToContent)
		{
#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
#else
			if(_sb_ToolTip == null)
			{
				_sb_ToolTip = new StringBuilder(TOOLTIP_CAPACITY);
			}
			_sb_ToolTip.Append(text);

			if(applyToContent)
			{
				_guiContent.tooltip = _sb_ToolTip.ToString();
			}
#endif	
			_isVisible = true;
		}

		public void AppendToolTip(int intValue, bool applyToContent)
		{
#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
#else
			if(_sb_ToolTip == null)
			{
				_sb_ToolTip = new StringBuilder(TOOLTIP_CAPACITY);
			}
			_sb_ToolTip.Append(intValue);

			if(applyToContent)
			{
				_guiContent.tooltip = _sb_ToolTip.ToString();
			}
#endif
			_isVisible = true;
		}

		public void AppendToolTip(float floatValue, bool applyToContent)
		{
#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
#else
			if(_sb_ToolTip == null)
			{
				_sb_ToolTip = new StringBuilder(TOOLTIP_CAPACITY);
			}
			_sb_ToolTip.Append(floatValue);

			if(applyToContent)
			{
				_guiContent.tooltip = _sb_ToolTip.ToString();
			}
#endif
			_isVisible = true;
		}

		public void AppendToolTip(Vector2 vec2Value, bool applyToContent)
		{
#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
#else
			if(_sb_ToolTip == null)
			{
				_sb_ToolTip = new StringBuilder(TOOLTIP_CAPACITY);
			}
			_sb_ToolTip.Append('(');
			_sb_ToolTip.Append(vec2Value.x);
			_sb_ToolTip.Append(',');
			_sb_ToolTip.Append(' ');
			_sb_ToolTip.Append(vec2Value.y);
			_sb_ToolTip.Append(')');

			if(applyToContent)
			{
				_guiContent.tooltip = _sb_ToolTip.ToString();
			}
#endif
			_isVisible = true;
		}

		//4. 조합 (Overload)
		//-------------------------------------------------
		/// <summary>
		/// 한번에 Text, Image, ToolTip을 지정할 수 있다.
		/// 필요 없는 경우 null을 넣으면 된다. (다른 값은 넣지 말것)
		/// </summary>
		/// <param name="text"></param>
		/// <param name="image"></param>
		/// <param name="tooltip"></param>
		public void SetTextImageToolTip(string text, Texture2D image, string tooltip)
		{
			if(text != null)
			{	
				SetText(text);
			}
			else
			{	
				_guiContent.text = null;
			}

			if(image != null)
			{
				SetImage(image);
			}
			else
			{
				_guiContent.image = null;
			}

#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
			_guiContent.tooltip = null;
#else
			if(tooltip != null)
			{
				SetToolTip(tooltip);
			}
			else
			{
				_guiContent.tooltip = null;
			}
#endif
		}


		public void SetTextImageToolTip(int nSpace, string text, Texture2D image, string tooltip)
		{
			if (text != null)
			{
				SetText(nSpace, text);
			}
			else
			{
				_guiContent.text = null;
			}

			if(image != null)
			{
				SetImage(image);
			}
			else
			{
				_guiContent.image = null;
			}
#if UNITY_2019_3_OR_NEWER
			//Unity 2019.3에서 툴팁이 이상하게 보인다. 이 버전에서는 툴팁을 삭제하자
			//툴팁 로직 삭제
			_guiContent.tooltip = null;
#else
			if(tooltip != null)
			{
				SetToolTip(tooltip);
			}
			else
			{
				_guiContent.tooltip = null;
			}
#endif
		}

		public override string ToString()
		{
			return _guiContent.text;
		}
	}
}