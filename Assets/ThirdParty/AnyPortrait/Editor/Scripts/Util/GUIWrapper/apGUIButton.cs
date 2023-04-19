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
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.1.18 : 작업 공간에 추가되는 버튼.
	/// 
	/// </summary>
	public class apGUIButton
	{
		// Members
		//-----------------------------------
		public Vector2 _pos;
		public int _width = 0;
		public int _height = 0;
		public Texture2D _img_Normal = null;
		public Texture2D _img_RollOver = null;

		//토글 방식인 경우
		public Texture2D _img_Diabled_Normal = null;
		public Texture2D _img_Diabled_RollOver = null;

		public enum STATUS
		{
			Released,
			Pressed,//Up 이벤트가 아닌 Down 이벤트에서 메뉴가 등장한다. Up은 없다.
		}
		private STATUS _status = STATUS.Released;
		private bool _isRollOver = false;//Released 상태 + 누르지 않음 + 마우스가 올려져있음 > 롤오버
		private bool _isVisible = false;

		//토글 방식인가
		private bool _isToggleButton = false;

		//토글 방식인 경우
		private bool _isEnabled = false;


		// Init
		//-----------------------------------
		/// <summary>
		/// 일반 버튼의 생성
		/// </summary>
		public apGUIButton(Texture2D imgNormal, Texture2D imgRollOver, int width, int height)
		{
			_width = width;
			_height = height;
			_img_Normal = imgNormal;
			_img_RollOver = imgRollOver;

			_img_Diabled_Normal = imgNormal;
			_img_Diabled_RollOver = imgRollOver;

			_status = STATUS.Released;
			_isRollOver = false;
			_isVisible = true;

			//토글 버튼이 아니다.
			_isToggleButton = false;
			_isEnabled = true;//항상 Enabled
		}

		/// <summary>
		/// 토글 방식인 경우의 버튼 생성
		/// </summary>
		public apGUIButton(	Texture2D imgNormal_Enabled,
							Texture2D imgRollOver_Enabled,
							Texture2D imgNormal_Diabled,
							Texture2D imgRollOver_Diabled,
							int width, int height)
		{
			_width = width;
			_height = height;

			_img_Normal = imgNormal_Enabled;
			_img_RollOver = imgRollOver_Enabled;

			_img_Diabled_Normal = imgNormal_Diabled;
			_img_Diabled_RollOver = imgRollOver_Diabled;

			_status = STATUS.Released;
			_isRollOver = false;
			_isVisible = true;

			//토글 버튼 켜기
			_isToggleButton = true;
			_isEnabled = false;;//일단 Diabled
		}


		// Function - Update / Draw
		//-----------------------------------
		/// <summary>
		/// 마우스 입력을 넣는다. 버튼을 클릭하면 true로 리턴한다.
		/// 안눌렀거나 미리 눌렀거나 다른데서 눌러서 왔으면 false로 리턴한다.
		/// </summary>
		/// <param name="isVisible"></param>
		/// <param name="pos"></param>
		/// <param name="isMouseEvent"></param>
		/// <param name="mousePos"></param>
		/// <param name="mouseStatus"></param>
		/// <returns></returns>
		public bool Update(Vector2 pos, Vector2 mousePos, apMouse.MouseBtnStatus mouseStatus, bool isEnabled = true)
		{
			if(!_isVisible)
			{
				//없었다가 나타나면 상태 초기화
				_status = STATUS.Released;
				_isRollOver = false;
			}
			_isVisible = true;
			
			_isEnabled = isEnabled;

			_pos = pos;
			_isRollOver = false;
			bool isMouseInButton = false;
			if(_pos.x - (_width / 2) < mousePos.x && mousePos.x < _pos.x + (_width / 2)
				&& _pos.y - (_height / 2) < mousePos.y && mousePos.y < _pos.y + (_height / 2))
			{
				isMouseInButton = true;

			}

			bool isClick = false;
			switch (_status)
			{
				case STATUS.Released:
					//기본
					if(isMouseInButton
						&& mouseStatus == apMouse.MouseBtnStatus.Down)
					{
						//눌렀다!
						isClick = true;
						_status = STATUS.Pressed;
					}
					break;

				case STATUS.Pressed:
					//누른 이후 프레임
					if(mouseStatus == apMouse.MouseBtnStatus.Up || mouseStatus == apMouse.MouseBtnStatus.Released)
					{
						//마우스 입력이고 마우스를 클릭하지 않은 상태일 때
						_status = STATUS.Released;
					}
					break;
			}
			
			if(_status == STATUS.Released && isMouseInButton)
			{
				_isRollOver = true;
			}

			return isClick;
		}

		public void Hide()
		{
			//안보이면 처리 불가
			_isVisible = false;
			_status = STATUS.Released;
			_isRollOver = false;
		}

		/// <summary>
		/// 버튼을 그린다.
		/// </summary>
		public void Draw()
		{
			if(!_isVisible)
			{
				return;
			}
			
			if(!_isToggleButton)
			{
				//일반 버튼일 때
				apGL.DrawTextureGL(	(_isRollOver ? _img_RollOver : _img_Normal),
								_pos,
								_width / apGL.Zoom, _height / apGL.Zoom,
								Color.gray,
								0.0f);
			}
			else
			{
				//토글 버튼일 때
				if(_isEnabled)
				{
					//버튼이 켜진 상태
					apGL.DrawTextureGL(	(_isRollOver ? _img_RollOver : _img_Normal),
								_pos,
								_width / apGL.Zoom, _height / apGL.Zoom,
								Color.gray,
								0.0f);
				}
				else
				{
					//버튼이 꺼진 상태
					apGL.DrawTextureGL(	(_isRollOver ? _img_Diabled_RollOver : _img_Diabled_Normal),
								_pos,
								_width / apGL.Zoom, _height / apGL.Zoom,
								Color.gray,
								0.0f);
				}
				
			}
			
		}


	}
}