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

using AnyPortrait;

namespace AnyPortrait
{

	public static class apControllerGL
	{
		// Members
		//------------------------------------------------------------------------
		private static int _layoutPosX = 0;
		private static int _layoutPosY = 0;
		private static int _layoutWidth = 0;
		private static int _layoutHeight = 0;
		private static Vector4 _glScreenClippingSize = Vector4.zero;
		private static Vector2 _scrollPos = Vector2.zero;

		private static apGL.MaterialBatch _matBatch = new apGL.MaterialBatch();

		//이전 버전
		//private static Texture2D _imgScrollBtn = null;
		//private static Texture2D _imgScrollBtn_Recorded = null;
		//private static Texture2D _imgSlotDeactive = null;

		//변경 22.6.9 : 하나의 Atlas로 관리한다.
		private static Texture2D _imgAtlas = null;


		//각 이미지별 UV를 설정하자
		//LT가 (0, 0)
		private static Vector2 UV_BUTTON_NORMAL = new Vector2(0.0f, 0.0f);
		private static Vector2 UV_BUTTON_CLICKED = new Vector2(0.25f, 0.0f);
		private static Vector2 UV_BUTTON_RECORDED = new Vector2(0.0f, 0.25f);
		private static Vector2 UV_SLOT_1D = new Vector2(0.0f, 0.75f);
		private static Vector2 UV_SLOT_2D = new Vector2(0.0f, 0.5f);
		private static Vector2 UV_LINE = new Vector2(0.75f, 0.0f);
		private const float UV_SIZE = 0.25f;
		private const float IMAGE_UNIT_PIXEL_SIZE = 32;//가변 크기의 Line을 제외한 모든 이미지는 32픽셀이다.
		

		

		private static float _barThickness = 4.0f;//6 > 4로 축소
		private static float _scrollBtnSize = 14.0f;
		private static float _slotSize = 16.0f;
		private static Color _grayColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private static Color _barColor = new Color(0.15f, 0.3f, 0.35f, 1.0f);
		private static Color _barColor_selected = new Color(0.4f, 0.2f, 0.2f, 1.0f);

		private static float _marginSize = 10.0f;

		//private static bool _isNeedPreRender = false;

		private static bool _isMouseEvent = false;
		private static bool _isMouseEventUsed = false;
		private static apControlParam _selectedControlParam = null;
		//private static int _selectSubParam = 0;//<<평소엔 0이지만, Vec3인 경우 1, 2로 나뉜다.
		
		//이전 : 외부의 마우스 이벤트를 그대로 사용한다.
		private static apMouse.MouseBtnStatus _leftBtnStatus = apMouse.MouseBtnStatus.Released;

		


		private static Vector2 _mousePos = Vector2.zero;

		private static bool _isSnapWhenReleased = false;

		// Init
		//------------------------------------------------------------------------
		//이전 버전
		//public static void SetTexture(Texture2D imgScrollBtn, Texture2D imgScrollBtn_Recorded, Texture2D imgSlotDeactive, Texture2D imgSlotActive)
		//{
		//	_imgScrollBtn = imgScrollBtn;
		//	_imgScrollBtn_Recorded = imgScrollBtn_Recorded;
		//	_imgSlotDeactive = imgSlotDeactive;
		//	//_imgSlotActive = imgSlotActive;
		//}

		//변경 22.6.9 : 하나의 Atlas로 관리한다.
		public static void SetTexture(Texture2D imgAtlas)
		{
			_imgAtlas = imgAtlas;
		}

		public static void SetShader(Shader shader_Color,
									Shader[] shader_Texture_Normal_Set,
									Shader[] shader_Texture_VColorAdd_Set,
									//Shader[] shader_MaskedTexture_Set,
									Shader shader_MaskOnly,
									Shader[] shader_Clipped_Set,
									Shader shader_GUITexture,
									Shader shader_ToneColor_Normal,
									Shader shader_ToneColor_Clipped,
									Shader shader_Alpha2White,
									Shader shader_BoneV2,
									Shader shader_TextureVColorMul,
									Shader shader_RigCircleV2,
									Shader shader_Gray_Normal, Shader shader_Gray_Clipped,
									Shader shader_VertPin)
		{
			//_matBatch.SetMaterial(mat_Color, mat_Texture, mat_MaskedTexture);
			//_matBatch = matBatch;

			_matBatch.SetShader(	shader_Color, 
									shader_Texture_Normal_Set, 
									shader_Texture_VColorAdd_Set, 
									/*shader_MaskedTexture_Set, */shader_MaskOnly, 
									shader_Clipped_Set, 
									shader_GUITexture, 
									shader_ToneColor_Normal, 
									shader_ToneColor_Clipped, 
									shader_Alpha2White,
									shader_BoneV2, null,
									shader_TextureVColorMul,
									shader_RigCircleV2, null,
									shader_Gray_Normal, shader_Gray_Clipped,
									shader_VertPin, null);
		}

		public static void SetLayoutSize(int layoutWidth, int layoutHeight,
											int posX, int posY,
											int totalEditorWidth, int totalEditorHeight,
											Vector2 scrollPos)
		{
			_layoutPosX = posX;
			_layoutPosY = posY;
			_layoutWidth = layoutWidth;
			_layoutHeight = layoutHeight;
			_scrollPos = scrollPos;

			totalEditorHeight += 30;
			posY += 30;
			posX += 5;
			layoutWidth -= 25;
			//layoutHeight -= 20; //?

			_glScreenClippingSize.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize.y = (float)(posY) / (float)totalEditorHeight;
			_glScreenClippingSize.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize.w = (float)(posY + layoutHeight) / (float)totalEditorHeight;

			//_isNeedPreRender = true;
			_isMouseEvent = false;
			_isMouseEventUsed = false;

			//변경 21.5.19
			_matBatch.SetClippingSizeToAllMaterial(_glScreenClippingSize);
		}

		// 기본적인 렌더링
		//------------------------------------------------------------------------
		//렌더링중인 Pass가 있다면 종료를 한다.
		public static void EndPass()
		{
			_matBatch.EndPass();
		}


		#region [미사용 코드] 아틀라스 렌더링으로 바뀌어서 더이상 사용하지 않는다. v1.4.0
		//public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		//{
		//	if (_matBatch.IsNotReady())
		//	{
		//		return;
		//	}

		//	if (Vector2.Equals(pos1, pos2))
		//	{
		//		return;
		//	}

		//	if (isNeedResetMat)
		//	{
		//		//변경 21.5.19
		//		_matBatch.BeginPass_Color(GL.LINES);
		//		//_matBatch.SetClippingSize(_glScreenClippingSize);
		//		//GL.Begin(GL.LINES);
		//	}

		//	GL.Color(color);
		//	GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
		//	GL.Vertex(new Vector3(pos2.x, pos2.y, 0.0f));

		//	//삭제
		//	if (isNeedResetMat)
		//	{
		//		//GL.End();//<전환 완료>
		//		_matBatch.EndPass();
		//	}
		//}

		//public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isNeedResetMat)
		//{
		//	if (_matBatch.IsNotReady())
		//	{
		//		Debug.LogError("Controll GL Error");
		//		return;
		//	}

		//	float halfWidth = width * 0.5f;
		//	float halfHeight = height * 0.5f;

		//	//CW
		//	// -------->
		//	// | 0(--) 1
		//	// | 		
		//	// | 3   2 (++)
		//	Vector3 pos_0 = new Vector3(pos.x - halfWidth, pos.y - halfHeight, 0);
		//	Vector3 pos_1 = new Vector3(pos.x + halfWidth, pos.y - halfHeight, 0);
		//	Vector3 pos_2 = new Vector3(pos.x + halfWidth, pos.y + halfHeight, 0);
		//	Vector3 pos_3 = new Vector3(pos.x - halfWidth, pos.y + halfHeight, 0);

		//	//CW
		//	// -------->
		//	// | 0   1
		//	// | 		
		//	// | 3   2
		//	if (isNeedResetMat)
		//	{
		//		//변경 21.5.19
		//		_matBatch.BeginPass_Color(GL.TRIANGLES);
		//		//_matBatch.SetClippingSize(_glScreenClippingSize);
		//		//GL.Begin(GL.TRIANGLES);
		//	}
		//	GL.Color(color);
		//	GL.Vertex(pos_0); // 0
		//	GL.Vertex(pos_1); // 1
		//	GL.Vertex(pos_2); // 2

		//	GL.Vertex(pos_2); // 2
		//	GL.Vertex(pos_3); // 3
		//	GL.Vertex(pos_0); // 0

		//	//삭제 21.5.19
		//	if (isNeedResetMat)
		//	{
		//		//GL.End();//<전환 완료>
		//		_matBatch.EndPass();
		//	}
		//}


		//public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, bool isNeedResetMat)
		//{
		//	if (_matBatch.IsNotReady())
		//	{
		//		return;
		//	}

		//	float realWidth = width;
		//	float realHeight = height;

		//	float realWidth_Half = realWidth * 0.5f;
		//	float realHeight_Half = realHeight * 0.5f;

		//	//CW
		//	// -------->
		//	// | 0(--) 1
		//	// | 		
		//	// | 3   2 (++)
		//	Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
		//	Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
		//	Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
		//	Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);

		//	float widthResize = (pos_1.x - pos_0.x);
		//	float heightResize = (pos_3.y - pos_0.y);

		//	if (widthResize < 1.0f || heightResize < 1.0f)
		//	{
		//		return;
		//	}

		//	float u_left = 0.0f;
		//	float u_right = 1.0f;

		//	float v_top = 0.0f;
		//	float v_bottom = 1.0f;

		//	Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
		//	Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
		//	Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
		//	Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

		//	//CW
		//	// -------->
		//	// | 0   1
		//	// | 		
		//	// | 3   2
		//	if (isNeedResetMat)
		//	{
		//		//변경 21.5.19
		//		_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
		//		//_matBatch.SetClippingSize(_glScreenClippingSize);
		//		//GL.Begin(GL.TRIANGLES);
		//	}

		//	GL.TexCoord(uv_0); GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
		//	GL.TexCoord(uv_1); GL.Vertex(new Vector3(pos_1.x, pos_1.y, 0)); // 1
		//	GL.TexCoord(uv_2); GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2

		//	GL.TexCoord(uv_2); GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2
		//	GL.TexCoord(uv_3); GL.Vertex(new Vector3(pos_3.x, pos_3.y, 0)); // 3
		//	GL.TexCoord(uv_0); GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0

		//	//삭제 21.5.19
		//	if (isNeedResetMat)
		//	{
		//		//GL.End();//<전환 완료>
		//		_matBatch.EndPass();
		//	}

		//	//GL.Flush();
		//} 
		#endregion



		//추가 22.6.9 [v1.4.0] : Atlas 그리기. 이미 BeginBatch에서 Image를 입력했으므로, 여기서는 Quad Mesh만 그리면 된다.
		public static void DrawTexture_Atlas(	ref Vector2 centerPos, ref Vector2 baseUV, 
												float width, float height,
												ref Color color)
		{
			float realWidth_Half = width * 0.5f;
			float realHeight_Half = height * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(centerPos.x - realWidth_Half, centerPos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(centerPos.x + realWidth_Half, centerPos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(centerPos.x + realWidth_Half, centerPos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(centerPos.x - realWidth_Half, centerPos.y + realHeight_Half);

			float u_left = baseUV.x;
			float u_right = u_left + UV_SIZE;

			float v_top = baseUV.y;
			float v_bottom = v_top + UV_SIZE;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			
			GL.Color(color);
			GL.TexCoord(uv_0); GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
			GL.TexCoord(uv_1); GL.Vertex(new Vector3(pos_1.x, pos_1.y, 0)); // 1
			GL.TexCoord(uv_2); GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2

			GL.TexCoord(uv_2); GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2
			GL.TexCoord(uv_3); GL.Vertex(new Vector3(pos_3.x, pos_3.y, 0)); // 3
			GL.TexCoord(uv_0); GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
		}



		public static void DrawTexture_Atlas_PixelPerfect(	ref Vector2 centerPos, ref Vector2 baseUV, 
															float width, float height,
															ref Color color)
		{
			int realWidth_Half = (int)(width * 0.5f);
			int realHeight_Half = (int)(height * 0.5f);

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2((int)(centerPos.x) - realWidth_Half, (int)(centerPos.y) - realHeight_Half);
			Vector2 pos_1 = new Vector2((int)(centerPos.x) + realWidth_Half, (int)(centerPos.y) - realHeight_Half);
			Vector2 pos_2 = new Vector2((int)(centerPos.x) + realWidth_Half, (int)(centerPos.y) + realHeight_Half);
			Vector2 pos_3 = new Vector2((int)(centerPos.x) - realWidth_Half, (int)(centerPos.y) + realHeight_Half);

			float u_left = baseUV.x;
			float u_right = u_left + UV_SIZE;

			float v_top = baseUV.y;
			float v_bottom = v_top + UV_SIZE;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			
			GL.Color(color);
			GL.TexCoord(uv_0); GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
			GL.TexCoord(uv_1); GL.Vertex(new Vector3(pos_1.x, pos_1.y, 0)); // 1
			GL.TexCoord(uv_2); GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2

			GL.TexCoord(uv_2); GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2
			GL.TexCoord(uv_3); GL.Vertex(new Vector3(pos_3.x, pos_3.y, 0)); // 3
			GL.TexCoord(uv_0); GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
		}


		// Update Mouse
		//------------------------------------------------------------------------
		public static void SetSnapWhenReleased(bool isSnapWhenReleased)
		{
			_isSnapWhenReleased = isSnapWhenReleased;
		}

		//변경 21.2.9 : 함수명 변경 + 외부 마우스 입력 가져오는걸 다르게 처리한다.
		public static void ReadyToUpdate(apMouse.MouseBtnStatus leftBtnStatus, Vector2 mousePos, bool isMouseEvent)
		{

			//위치 보정
			_mousePos = mousePos - new Vector2(_layoutPosX, _layoutPosY + 6) + _scrollPos;

			bool isMousePressed = leftBtnStatus == apMouse.MouseBtnStatus.Down || leftBtnStatus == apMouse.MouseBtnStatus.Pressed;
			bool isMouseRealDown = leftBtnStatus == apMouse.MouseBtnStatus.Down;

			//if (isMouseEvent)
			{
				//밖에서 클릭(Down)을 하는 경우나 유효하지 않는 입력인 경우
				//Down > False
				if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					if (mousePos.x < _layoutPosX || mousePos.x > _layoutPosX + _layoutWidth
						|| mousePos.y < _layoutPosY || mousePos.y > _layoutPosY + _layoutHeight
						|| Event.current.type == EventType.Used)
					{
						//Debug.LogError("밖에서 눌렸거나 유효하지 않은 이벤트다.");
						isMousePressed = false;
						isMouseRealDown = false;
					}
				}
			}

			//마우스 입력 상태에 따라서 입력 상태를 결정하자
			switch (_leftBtnStatus)
			{
				case apMouse.MouseBtnStatus.Down:
				case apMouse.MouseBtnStatus.Pressed:
					if (isMousePressed)
					{
						if(_leftBtnStatus == apMouse.MouseBtnStatus.Pressed && isMouseRealDown)
						{
							//Debug.Log("Pressed 상태에서 Down으로 변경");
							_leftBtnStatus = apMouse.MouseBtnStatus.Down;
						}
						else
						{
							_leftBtnStatus = apMouse.MouseBtnStatus.Pressed;
						}
						
					}
					else
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Up;
					}
					break;

				case apMouse.MouseBtnStatus.Up:
				case apMouse.MouseBtnStatus.Released:
					if (isMousePressed)
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Down;
					}
					else
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Released;
					}
					break;
			}

			_isMouseEvent = isMouseEvent;

			//마우스 이벤트에서
			//눌려지지 않은 상태에서
			//선택된 Param이 있다면
			//> 해제
			if (_selectedControlParam != null
				&& (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released))
			{
				//if(!_isMouseEvent)
				//{
				//	Debug.LogError("Mouse 이벤트가 아닌데 Up 이벤트가 발생했다");
				//}
				//Debug.LogError("Released");
				ReleaseSelectedParam();
				_selectedControlParam = null;
				//_selectSubParam = -1;
			}


			_isMouseEventUsed = false;
		}

		/// <summary>
		/// 마우스 이벤트 후 Release 했을때 Snap을 하여 ControlParam값을 설정한다.
		/// SetSnapWhenReleased 함수에 의해 Snap 여부가 결정된다.
		/// </summary>
		/// <param name="isAnimEditing"></param>
		private static void ReleaseSelectedParam()
		{
			//Debug.LogWarning("ReleaseSelectedParam");
			if (_selectedControlParam == null)
			{
				return;
			}

			//Debug.LogError("Snap 하기 : " + _leftBtnStatus);

			//if(!_selectedParam._isRange)
			//{
			//	return;
			//}

			switch (_selectedControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					//Int는 자동이므로 교정할 필요가 없다.
					break;

				case apControlParam.TYPE.Float:
					{
						if (_selectedControlParam._float_Cur < _selectedControlParam._float_Min)
						{
							_selectedControlParam._float_Cur = _selectedControlParam._float_Min;
						}
						else if (_selectedControlParam._float_Cur > _selectedControlParam._float_Max)
						{
							_selectedControlParam._float_Cur = _selectedControlParam._float_Max;
						}
						else
						{	
							if (_isSnapWhenReleased)
							{
								//값을 적절히 스냅핑하자
								_selectedControlParam._float_Cur = SnapValue(_selectedControlParam._float_Cur, _selectedControlParam._float_Min, _selectedControlParam._float_Max, _selectedControlParam._snapSize);
							}
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						_selectedControlParam._vec2_Cur.x = Mathf.Clamp(_selectedControlParam._vec2_Cur.x, _selectedControlParam._vec2_Min.x, _selectedControlParam._vec2_Max.x);
						_selectedControlParam._vec2_Cur.y = Mathf.Clamp(_selectedControlParam._vec2_Cur.y, _selectedControlParam._vec2_Min.y, _selectedControlParam._vec2_Max.y);

						if (_isSnapWhenReleased)
						{
							//값을 적절히 스냅핑하자
							_selectedControlParam._vec2_Cur.x = SnapValue(_selectedControlParam._vec2_Cur.x, _selectedControlParam._vec2_Min.x, _selectedControlParam._vec2_Max.x, _selectedControlParam._snapSize);
							_selectedControlParam._vec2_Cur.y = SnapValue(_selectedControlParam._vec2_Cur.y, _selectedControlParam._vec2_Min.y, _selectedControlParam._vec2_Max.y, _selectedControlParam._snapSize);
						}
					}
					break;

					//case apControlParam.TYPE.Vector3:
					//	{
					//		_selectedParam._vec3_Cur.x = Mathf.Clamp(_selectedParam._vec3_Cur.x, _selectedParam._vec3_Min.x, _selectedParam._vec3_Max.x);
					//		_selectedParam._vec3_Cur.y = Mathf.Clamp(_selectedParam._vec3_Cur.y, _selectedParam._vec3_Min.y, _selectedParam._vec3_Max.y);
					//		_selectedParam._vec3_Cur.z = Mathf.Clamp(_selectedParam._vec3_Cur.z, _selectedParam._vec3_Min.z, _selectedParam._vec3_Max.z);

					//		//값을 적절히 스냅핑하자
					//		_selectedParam._vec3_Cur.x = SnapValue(_selectedParam._vec3_Cur.x, _selectedParam._vec3_Min.x, _selectedParam._vec3_Max.x);
					//		_selectedParam._vec3_Cur.y = SnapValue(_selectedParam._vec3_Cur.y, _selectedParam._vec3_Min.y, _selectedParam._vec3_Max.y);
					//		_selectedParam._vec3_Cur.z = SnapValue(_selectedParam._vec3_Cur.z, _selectedParam._vec3_Min.z, _selectedParam._vec3_Max.z);
					//	}
					//	break;
			}
		}

		private static float SnapValue(float curValue, float minValue, float maxValue, int snapSize)
		{
			if (maxValue - minValue <= 0.0f)
			{
				return curValue;
			}
			float unitSize = 0.1f;
			float length = Mathf.Abs(maxValue - minValue);
			unitSize = length / snapSize;


			//float curCheckValue = minValue - unitSize;
			float curCheckValue = minValue;

			float minDist = float.MaxValue;
			float resultValue = curValue;
			while (true)
			{
				if (curCheckValue > maxValue + unitSize)
				{
					break;
				}

				float dist = Mathf.Abs(curValue - curCheckValue);
				if (dist < minDist)
				{
					minDist = dist;
					resultValue = curCheckValue;
				}

				curCheckValue += unitSize;
			}

			//int iResult = 0;
			//if (resultValue > 0.0f)
			//{
			//	iResult = (int)(Mathf.Abs(resultValue * 100.0f) + 0.5f);
			//	resultValue = (float)(iResult * 0.01);
			//}
			//else
			//{
			//	iResult = -(int)(Mathf.Abs(resultValue * 100.0f) + 0.5f);
			//	resultValue = (float)(iResult * 0.01);
			//}

			return Mathf.Clamp(resultValue, minValue, maxValue);
		}

		public static void EndUpdate()
		{
			if (!_isMouseEvent)
			{
				return;
			}
			if (!_isMouseEventUsed)
			{
				if (_selectedControlParam != null)
				{
					//Debug.LogError("Released - EndUpdate");
					ReleaseSelectedParam();
					_selectedControlParam = null;
					//_selectSubParam = -1;
				}

				//마우스 입력인데, 마우스 입력 처리가 없었다면
				if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					//Debug.Log(">> 입력 없음 : " + _leftBtnStatus + " > Up");
					_leftBtnStatus = apMouse.MouseBtnStatus.Up;
				}
				else if(_leftBtnStatus == apMouse.MouseBtnStatus.Up)
				{
					//Debug.Log(">> 입력 없음 : Up > Released");
					_leftBtnStatus = apMouse.MouseBtnStatus.Released;
				}

			}
		}

		public static void CancelMouseProcess()
		{
			_leftBtnStatus = apMouse.MouseBtnStatus.Up;
			ReleaseSelectedParam();
			_selectedControlParam = null;
			_isMouseEventUsed = false;
			_isMouseEvent = false;
			//_selectSubParam = -1;
		}

		//------------------------------------------------------------------------
		private static bool IsClipping(Vector2 pos, float height)
		{
			if (pos.y + height - _scrollPos.y < -50)
			{
				return true;
			}

			if (pos.y - _scrollPos.y > _layoutHeight + 50)
			{
				return true;
			}
			return false;
		}
		//------------------------------------------------------------------------

		// Draw Slider
		//------------------------------------------------------------------------

		public static float DrawFloatSlider(	Vector2 pos, 
												int width, int boxWidth, int boxHeight,
												apControlParam controlParam, List<apModifierParamSet> recordedParamSet,
												apModifierParamSet recordedKey, apAnimKeyframe editedKeyframe)
		{
			//중간에 이벤트가 날라갔다면
			if (Event.current.type == EventType.Used)
			{
				CancelMouseProcess();
			}


			//추가 22.7.2 : Repaint 단계가 아니라면 마우스 이벤트만 처리하고 렌더링 코드는 생략한다.
			bool isRepaint = Event.current.type == EventType.Repaint;

			bool isSelected = (controlParam == _selectedControlParam);

			Color barColor1 = _barColor;
			if (isSelected)
			{
				barColor1 = _barColor_selected;
			}

			float curValue = controlParam._float_Cur;
			float minValue = controlParam._float_Min;
			float maxValue = controlParam._float_Max;

			if (maxValue - minValue <= 0.0f)
			{
				return curValue;
			}

			//너무 밖으로 나가면 그냥 렌더링을 하지 말자
			if (IsClipping(pos, _scrollBtnSize * 2))
			{
				return curValue;
			}

			//1. 바
			Vector2 barPos = new Vector2(pos.x + width * 0.5f, pos.y + _scrollBtnSize * 0.5f);
			float barWidth = width - (_scrollBtnSize + _marginSize * 2);



			//< v1.4.0 렌더링 최적화 전략 >
			//이전에는 텍스쳐 따로, Color Shader 따로 그렸다.
			//이젠 모두 하나의 Atlas로 그리므로 Batch를 한번만 한다.


			//이전 : 따로 그리는 것은 제거한다.
			//_matBatch.BeginPass_Color(GL.TRIANGLES);
			//DrawBox(barPos, barWidth, _barThickness, barColor1, false);
			//DrawBox(barPos + new Vector2(0, -_barThickness / 4), barWidth, _barThickness / 2, barColor2, false);
			//_matBatch.EndPass();

			if (isRepaint)
			{
				//변경 22.6.9 [v1.4.0] Pass는 한번만 연다
				_matBatch.BeginPass_GUITexture(GL.TRIANGLES, _imgAtlas);

				//1. 바 그리기
				DrawTexture_Atlas(ref barPos, ref UV_LINE, barWidth, _barThickness, ref barColor1);
			}


			float minPosX = (pos.x + width * 0.5f) - (barWidth * 0.5f);
			float maxPosX = (pos.x + width * 0.5f) + (barWidth * 0.5f);
			float valueITP = Mathf.Clamp01((curValue - minValue) / (maxValue - minValue));

			float curPosX = (minPosX * (1.0f - valueITP)) + (maxPosX * valueITP);

			Vector2 btnPos = new Vector2(curPosX, pos.y + _scrollBtnSize * 0.5f);

			Dictionary<apModifierParamSet, Vector2> paramAndPosSet = null;
			if (recordedParamSet != null)
			{

				//삭제 22.6.9 : 개별로 그리지 않는다.
				//_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, _btnColor, _imgSlotDeactive, apPortrait.SHADER_TYPE.AlphaBlend);

				paramAndPosSet = new Dictionary<apModifierParamSet, Vector2>();
				for (int i = 0; i < recordedParamSet.Count; i++)
				{
					apModifierParamSet modParam = recordedParamSet[i];
					float pCurValue = modParam._conSyncValue_Float;
					if (pCurValue < minValue - 0.2f || pCurValue > maxValue + 0.2f)
					{
						continue;
					}
					float pItp = Mathf.Clamp01((pCurValue - minValue) / (maxValue - minValue));
					float pCurPosX = (minPosX * (1.0f - pItp)) + (maxPosX * pItp);
					Vector2 keyPos = new Vector2(pCurPosX, pos.y + _scrollBtnSize * 0.5f);

					paramAndPosSet.Add(modParam, keyPos);

					//이전
					//DrawTexture(_imgSlotDeactive, keyPos, _slotSize, _slotSize, _btnColor, false);

					if (isRepaint)
					{
						//변경 v1.4.0 : Atlas를 이용한다.
						DrawTexture_Atlas_PixelPerfect(ref keyPos, ref UV_SLOT_1D, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
					
				}

				//_matBatch.EndPass();//삭제 22.6.9 : 나중에 한번만 End한다.
			}


			if (isRepaint)
			{
				if (recordedKey == null && editedKeyframe == null)
				{
					//이전
					//DrawTexture(_imgScrollBtn, btnPos, _scrollBtnSize, _scrollBtnSize, _btnColor, true);

					//변경 22.6.9 [v1.4.0]
					if (isSelected)
					{
						//클릭했을때
						DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_CLICKED, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
					else
					{
						//일반 버튼
						DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_NORMAL, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
				}
				else
				{
					//이전
					//DrawTexture(_imgScrollBtn_Recorded, btnPos, _scrollBtnSize, _scrollBtnSize, _btnColor, true);

					//변경 22.6.9 [v1.4.0]
					DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_RECORDED, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
				}
			}
			

			//다만,
			//클릭을 했다면
			//값을 체크해주자
			if (_isMouseEvent)
			{
				if (_selectedControlParam == null)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						//없으면 새로 선택
						//1. 버튼을 선택하는가 (위치 이동 없음)
						//2. 슬롯을 선택하는가 (위치이동 있음)
						//3. 바를 선택하는가 (위치이동 있음)
						bool isSelect = false;
						if (IsMouseIn(_mousePos, btnPos, _scrollBtnSize * 2, _scrollBtnSize * 2))
						{
							_selectedControlParam = controlParam;
							isSelect = true;
						}
						else
						{
							if (paramAndPosSet != null)
							{
								foreach (KeyValuePair<apModifierParamSet, Vector2> paramPos in paramAndPosSet)
								{
									if (IsMouseIn(_mousePos, paramPos.Value, _slotSize * 1.5f, _slotSize * 1.5f))
									{
										//저장된 위치에서 클릭을 했다면
										_selectedControlParam = controlParam;
										
										curValue = paramPos.Key._conSyncValue_Float;

										isSelect = true;
										break;
									}
								}
							}
						}

						if (!isSelect)
						{
							//영역 안쪽을 클릭했는가
							Vector2 relativeMousePos = new Vector2(_mousePos.x - pos.x, _mousePos.y - (_scrollBtnSize * 0.5f + pos.y));
							bool isMouseInBox = relativeMousePos.x > 0.0f && relativeMousePos.x < boxWidth 
												&& relativeMousePos.y > -boxHeight / 2 && relativeMousePos.y < boxHeight / 2;

							//if (IsMouseIn(_mousePos, barPos, barWidth, _barThickness * 2))//바를 클릭했을 때
							if(isMouseInBox)//바를 포함한 박스를 클릭했을 때
							{
								_selectedControlParam = controlParam;

								//위치에 맞게 curValue를 바꾸자
								float mousePosX = _mousePos.x;
								if (mousePosX < minPosX)
								{
									curValue = minValue;
								}
								else if (mousePosX > maxPosX)
								{
									curValue = maxValue;
								}
								else
								{
									float itp = (mousePosX - minPosX) / (maxPosX - minPosX);
									curValue = minValue * (1.0f - itp) + maxValue * itp;
									curValue = Mathf.Clamp(curValue, minValue, maxValue);
								}

								//controlParam._float_Cur = curValue;
							}
						}

						if (_selectedControlParam != null)
						{
							_isMouseEventUsed = true;
							GUI.FocusControl(null);
						}
					}
				}
				else if (isSelected)
				{
					//선택한게 있을 때
					//나갔다 돌아오는건 허용해야한다.
					//새로 클릭하는건 영역 안쪽에서만 된다.
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down ||
						_leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
					{	
						//위치에 맞게 curValue를 바꾸자
						float mousePosX = _mousePos.x;
						if (mousePosX < minPosX)
						{
							//Debug.LogError("Min [" + mousePosX + " / " + minPosX + "]");
							curValue = minValue;
						}
						else if (mousePosX > maxPosX)
						{
							//Debug.LogError("Max [" + mousePosX + " / " + maxPosX + "]");
							curValue = maxValue;
						}
						else
						{
							float itp = (mousePosX - minPosX) / (maxPosX - minPosX);

							curValue = (minValue * (1.0f - itp)) + (maxValue * itp);
							curValue = Mathf.Clamp(curValue, minValue, maxValue);
						}

						//Debug.LogError("Value Changed " + controlParam._float_Cur + " -> " + curValue + "(" + minValue + " ~ " + maxValue + ")");
						//controlParam._float_Cur = curValue;

						_isMouseEventUsed = true;
					}
				}
			}

			if (isRepaint)
			{
				//렌더링을 여기서 종료한다.
				_matBatch.EndPass();
			}

			return curValue;
		}



		public static int DrawIntSlider(	Vector2 pos, 
											int width, int boxWidth, int boxHeight,
											apControlParam controlParam, List<apModifierParamSet> recordedParamSet,
											apModifierParamSet recordedKey, apAnimKeyframe editedKeyframe)
		{
			//중간에 이벤트가 날라갔다면
			if (Event.current.type == EventType.Used)
			{
				CancelMouseProcess();
			}

			//추가 22.7.2 : Repaint 단계가 아니라면 마우스 이벤트만 처리하고 렌더링 코드는 생략한다.
			bool isRepaint = Event.current.type == EventType.Repaint;


			bool isSelected = (controlParam == _selectedControlParam);

			Color barColor1 = _barColor;

			if (isSelected)
			{
				barColor1 = _barColor_selected;
			}

			int curValue = controlParam._int_Cur;
			int minValue = controlParam._int_Min;
			int maxValue = controlParam._int_Max;
			if (maxValue - minValue <= 0)
			{
				return curValue;
			}

			//너무 밖으로 나가면 그냥 렌더링을 하지 말자
			if (IsClipping(pos, _scrollBtnSize * 2))
			{
				return curValue;
			}


			//1. 바
			Vector2 barPos = new Vector2(pos.x + width * 0.5f, pos.y + _scrollBtnSize * 0.5f);
			float barWidth = width - (_scrollBtnSize + _marginSize * 2);


			//< v1.4.0 렌더링 최적화 전략 >
			//이전에는 텍스쳐 따로, Color Shader 따로 그렸다.
			//이젠 모두 하나의 Atlas로 그리므로 Batch를 한번만 한다.


			//이전
			//_matBatch.BeginPass_Color(GL.TRIANGLES);
			//DrawBox(barPos, barWidth, _barThickness, barColor1, false);
			//DrawBox(barPos + new Vector2(0, -_barThickness / 4), barWidth, _barThickness / 2, barColor2, false);
			//_matBatch.EndPass();

			if (isRepaint)
			{
				//변경 22.6.9 [v1.4.0]
				_matBatch.BeginPass_GUITexture(GL.TRIANGLES, _imgAtlas);

				//1. 바 그리기
				DrawTexture_Atlas(ref barPos, ref UV_LINE, barWidth, _barThickness, ref barColor1);
			}


			float minPosX = (pos.x + width * 0.5f) - (barWidth * 0.5f);
			float maxPosX = (pos.x + width * 0.5f) + (barWidth * 0.5f);
			float valueITP = Mathf.Clamp01((float)(curValue - minValue) / (float)(maxValue - minValue));


			float curPosX = (minPosX * (1.0f - valueITP)) + (maxPosX * valueITP);

			Vector2 btnPos = new Vector2(curPosX, pos.y + _scrollBtnSize * 0.5f);

			Dictionary<apModifierParamSet, Vector2> paramAndPosSet = null;



			if (recordedParamSet != null)
			{
				paramAndPosSet = new Dictionary<apModifierParamSet, Vector2>();

				//삭제 22.6.9 : 개별로 그리지 않는다.
				//_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, _btnColor, _imgSlotDeactive, apPortrait.SHADER_TYPE.AlphaBlend);
				
				for (int i = 0; i < recordedParamSet.Count; i++)
				{
					apModifierParamSet modParam = recordedParamSet[i];
					int pCurValue = modParam._conSyncValue_Int;
					if (pCurValue < minValue || pCurValue > maxValue)
					{
						continue;
					}
					float pItp = Mathf.Clamp01((float)(pCurValue - minValue) / (float)(maxValue - minValue));
					float pCurPosX = (minPosX * (1.0f - pItp)) + (maxPosX * pItp);
					Vector2 keyPos = new Vector2(pCurPosX, pos.y + _scrollBtnSize * 0.5f);

					paramAndPosSet.Add(modParam, keyPos);

					//삭제
					//DrawTexture(_imgSlotDeactive, keyPos, _slotSize, _slotSize, _btnColor, false);

					if (isRepaint)
					{
						//변경 v1.4.0 : Atlas를 이용한다.
						DrawTexture_Atlas_PixelPerfect(ref keyPos, ref UV_SLOT_1D, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
				}

				//_matBatch.EndPass();//삭제 22.6.9 : 나중에 한번만 End한다.
			}

			if (isRepaint)
			{
				if (recordedKey == null && editedKeyframe == null)
				{
					//이전
					//DrawTexture(_imgScrollBtn, btnPos, _scrollBtnSize, _scrollBtnSize, _btnColor, true);

					//변경 22.6.9 [v1.4.0]
					if (isSelected)
					{
						//클릭했을때
						DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_CLICKED, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
					else
					{
						//일반 버튼
						DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_NORMAL, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
				}
				else
				{
					//이전
					//DrawTexture(_imgScrollBtn_Recorded, btnPos, _scrollBtnSize, _scrollBtnSize, _btnColor, true);

					//변경 22.6.9 [v1.4.0]
					DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_RECORDED, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
				}
			}

			


			//다만,
			//클릭을 했다면
			//값을 체크해주자
			if (_isMouseEvent)
			{
				if (_selectedControlParam == null)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						//없으면 새로 선택
						//1. 버튼을 선택하는가 (위치 이동 없음)
						//2. 슬롯을 선택하는가 (위치이동 있음)
						//3. 바를 선택하는가 (위치이동 있음)
						bool isSelect = false;
						if (IsMouseIn(_mousePos, btnPos, _scrollBtnSize * 2, _scrollBtnSize * 2))
						{
							_selectedControlParam = controlParam;
							isSelect = true;
						}
						else
						{
							if (paramAndPosSet != null)
							{
								foreach (KeyValuePair<apModifierParamSet, Vector2> paramPos in paramAndPosSet)
								{
									if (IsMouseIn(_mousePos, paramPos.Value, _slotSize * 1.5f, _slotSize * 1.5f))
									{
										//저장된 위치에서 클릭을 했다면
										_selectedControlParam = controlParam;

										curValue = paramPos.Key._conSyncValue_Int;

										isSelect = true;
										break;
									}
								}
							}
						}
						if (!isSelect)
						{
							//영역 안쪽을 클릭했는가
							Vector2 relativeMousePos = new Vector2(_mousePos.x - pos.x, _mousePos.y - (_scrollBtnSize * 0.5f + pos.y));
							bool isMouseInBox = relativeMousePos.x > 0.0f && relativeMousePos.x < boxWidth 
												&& relativeMousePos.y > -boxHeight / 2 && relativeMousePos.y < boxHeight / 2;

							//if (IsMouseIn(_mousePos, barPos, barWidth, _barThickness * 2))
							if(isMouseInBox)//바를 포함한 박스를 클릭했을 때
							{
								_selectedControlParam = controlParam;

								//위치에 맞게 curValue를 바꾸자
								float mousePosX = _mousePos.x;
								if (mousePosX < minPosX)
								{
									curValue = minValue;
								}
								else if (mousePosX > maxPosX)
								{
									curValue = maxValue;
								}
								else
								{
									float itp = (mousePosX - minPosX) / (maxPosX - minPosX);
									float curValueF = (minValue * (1.0f - itp) + maxValue * itp);
									if (curValueF > 0.0f)
									{
										curValue = (int)(curValueF + 0.5f);
									}
									else
									{
										curValue = -(int)(Mathf.Abs(curValueF) + 0.5f);
									}
									curValue = Mathf.Clamp(curValue, minValue, maxValue);
								}

								//controlParam._float_Cur = curValue;
							}
						}

						if (_selectedControlParam != null)
						{
							_isMouseEventUsed = true;
							GUI.FocusControl(null);
						}
					}
				}
				else if (isSelected)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down ||
						_leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
					{
						//있는데... 같을 때만 처리
						//마우스 위치에 맞게 이동하자

						//위치에 맞게 curValue를 바꾸자
						float mousePosX = _mousePos.x;
						if (mousePosX < minPosX)
						{
							//Debug.LogError("Min [" + mousePosX + " / " + minPosX + "]");
							curValue = minValue;
						}
						else if (mousePosX > maxPosX)
						{
							//Debug.LogError("Max [" + mousePosX + " / " + maxPosX + "]");
							curValue = maxValue;
						}
						else
						{
							float itp = (mousePosX - minPosX) / (maxPosX - minPosX);

							float curValueF = (minValue * (1.0f - itp) + maxValue * itp);
							if (curValueF > 0.0f)
							{
								curValue = (int)(curValueF + 0.5f);
							}
							else
							{
								curValue = -(int)(Mathf.Abs(curValueF) + 0.5f);
							}

							curValue = Mathf.Clamp(curValue, minValue, maxValue);
						}

						_isMouseEventUsed = true;
					}
				}
			}

			if (isRepaint)
			{
				//렌더링을 여기서 종료한다.
				_matBatch.EndPass();
			}

			return curValue;
		}

		public static Vector2 DrawVector2Slider(Vector2 pos, 
												int width, int height, 
												int boxWidth, int boxHeight,
												apControlParam controlParam, List<apModifierParamSet> recordedParamSet,
												apModifierParamSet recordedKey, apAnimKeyframe editedKeyframe)
		{
			//중간에 이벤트가 날라갔다면
			if (Event.current.type == EventType.Used)
			{
				CancelMouseProcess();
			}

			height -= (int)_scrollBtnSize;

			//추가 22.7.2 : Repaint 단계가 아니라면 마우스 이벤트만 처리하고 렌더링 코드는 생략한다.
			bool isRepaint = Event.current.type == EventType.Repaint;

			bool isSelected = (controlParam == _selectedControlParam);

			Color barColor1 = _barColor;

			if (isSelected)
			{
				barColor1 = _barColor_selected;
			}

			Vector2 curValue = controlParam._vec2_Cur;
			Vector2 minValue = controlParam._vec2_Min;
			Vector2 maxValue = controlParam._vec2_Max;


			if (maxValue.x - minValue.x <= 0.0f || maxValue.y - minValue.y <= 0.0f)
			{
				return curValue;
			}

			//너무 밖으로 나가면 그냥 렌더링을 하지 말자
			if (IsClipping(pos, height + _scrollBtnSize * 2))
			{
				return curValue;
			}

			//1. 바
			// 굵게 사각형을 만들고
			// 라인으로 내부의 십자를 만든다.

			//라인 위쪽에 하나
			//아래쪽에도 하나

			float barWidth = width - (_scrollBtnSize + _marginSize * 2);
			float barHeight = height;

			
			Vector2 barPos_T = new Vector2(pos.x + width * 0.5f, pos.y + _scrollBtnSize * 0.5f);
			
			Vector2 barPos_B = barPos_T + new Vector2(0, height);


			Vector2 barPos_L = barPos_T + new Vector2(-barWidth * 0.5f, height * 0.5f);
			Vector2 barPos_R = barPos_T + new Vector2(barWidth * 0.5f, height * 0.5f);
			
			Vector2 barPos_C = new Vector2(barPos_T.x, barPos_L.y);

			//Vector2 centerPos = new Vector2(barPos_T.x, barPos_L.y);

			//T 렌더링
			//if(_isNeedPreRender)
			//{	
			//	DrawBox(barPos_T, barWidth, _barThickness, barColor1, true);
			//	_isNeedPreRender = false;
			//}



			//< v1.4.0 렌더링 최적화 전략 >
			//이전에는 텍스쳐 따로, Color Shader 따로 그렸다.
			//이젠 모두 하나의 Atlas로 그리므로 Batch를 한번만 한다.



			//이전
			//_matBatch.BeginPass_Color(GL.LINES);
			//DrawLine(barPos_T, barPos_B, barColor2, false);
			//DrawLine(barPos_L, barPos_R, barColor2, false);

			//_matBatch.EndPass();

			////변경 21.5.19
			//_matBatch.BeginPass_Color(GL.TRIANGLES);
			//DrawBox(barPos_T, barWidth, _barThickness, barColor1, false);

			//DrawBox(barPos_B, barWidth, _barThickness, barColor1, false);

			//DrawBox(barPos_L, _barThickness, barHeight, barColor1, false);
			//DrawBox(barPos_R, _barThickness, barHeight, barColor1, false);

			//DrawBox(barPos_T + new Vector2(0, -_barThickness / 4), barWidth + _barThickness, _barThickness / 2, barColor2, false);
			//DrawBox(barPos_B + new Vector2(0, +_barThickness / 4), barWidth + _barThickness, _barThickness / 2, barColor2, false);

			//DrawBox(barPos_L + new Vector2(-_barThickness / 4, 0), _barThickness / 2, barHeight + _barThickness, barColor2, false);
			//DrawBox(barPos_R + new Vector2(_barThickness / 4, 0), _barThickness / 2, barHeight + _barThickness, barColor2, false);

			//_matBatch.EndPass();

			if (isRepaint)
			{
				//변경 22.6.9 [v1.4.0] Pass는 한번만 연다
				_matBatch.BeginPass_GUITexture(GL.TRIANGLES, _imgAtlas);

				//1. 바 그리기
				//1-1. 내부 십자선
				float innerThickness = 2.0f;

				DrawTexture_Atlas(ref barPos_C, ref UV_LINE, innerThickness, height, ref barColor1);
				DrawTexture_Atlas(ref barPos_C, ref UV_LINE, barWidth, innerThickness, ref barColor1);

				//1-2. 외부 박스
				DrawTexture_Atlas(ref barPos_T, ref UV_LINE, barWidth + _barThickness, _barThickness, ref barColor1);
				DrawTexture_Atlas(ref barPos_B, ref UV_LINE, barWidth + _barThickness, _barThickness, ref barColor1);

				DrawTexture_Atlas(ref barPos_L, ref UV_LINE, _barThickness, barHeight + _barThickness, ref barColor1);
				DrawTexture_Atlas(ref barPos_R, ref UV_LINE, _barThickness, barHeight + _barThickness, ref barColor1);
			}

			float minPosX = barPos_L.x;
			float maxPosX = barPos_R.x;

			//주의 / Y좌표는 값과 좌표가 반전된다.
			float minPosY = barPos_T.y;
			float maxPosY = barPos_B.y;

			float valueITP_X = Mathf.Clamp01((curValue.x - minValue.x) / (maxValue.x - minValue.x));
			float valueITP_Y = Mathf.Clamp01((curValue.y - minValue.y) / (maxValue.y - minValue.y));


			Vector2 curPos = new Vector2(
				(minPosX * (1.0f - valueITP_X)) + (maxPosX * valueITP_X),
				(maxPosY * (1.0f - valueITP_Y)) + (minPosY * valueITP_Y)
				);

			Vector2 btnPos = curPos;

			Dictionary<apModifierParamSet, Vector2> paramAndPosSet = null;
			if (recordedParamSet != null)
			{
				paramAndPosSet = new Dictionary<apModifierParamSet, Vector2>();

				//삭제 22.6.9 : 개별로 그리지 않는다.
				//_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, _btnColor, _imgSlotDeactive, apPortrait.SHADER_TYPE.AlphaBlend);
				
				for (int i = 0; i < recordedParamSet.Count; i++)
				{
					apModifierParamSet modParam = recordedParamSet[i];
					float pCurValueX = modParam._conSyncValue_Vector2.x;
					float pCurValueY = modParam._conSyncValue_Vector2.y;
					
					if (pCurValueX < minValue.x - 0.2f || pCurValueX > maxValue.x + 0.2f)
					{
						continue;
					}
					if (pCurValueY < minValue.y - 0.2f || pCurValueY > maxValue.y + 0.2f)
					{
						continue;
					}
					float pItpX = Mathf.Clamp01((pCurValueX - minValue.x) / (maxValue.x - minValue.x));
					float pItpY = Mathf.Clamp01((pCurValueY - minValue.y) / (maxValue.y - minValue.y));

					float pCurPosX = (minPosX * (1.0f - pItpX)) + (maxPosX * pItpX);
					float pCurPosY = (maxPosY * (1.0f - pItpY)) + (minPosY * pItpY);

					Vector2 keyPos = new Vector2(pCurPosX, pCurPosY);

					paramAndPosSet.Add(modParam, keyPos);

					//이전
					//DrawTexture(_imgSlotDeactive, keyPos, _slotSize, _slotSize, _btnColor, false);

					if (isRepaint)
					{
						//변경 v1.4.0 : Atlas를 이용한다.
						DrawTexture_Atlas_PixelPerfect(ref keyPos, ref UV_SLOT_2D, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
				}

				//_matBatch.EndPass();삭제 22.6.9
			}

			if (isRepaint)
			{
				if (recordedKey == null && editedKeyframe == null)
				{
					//이전
					//DrawTexture(_imgScrollBtn, btnPos, _scrollBtnSize, _scrollBtnSize, _btnColor, true);

					//변경 22.6.9 [v1.4.0]
					if (isSelected)
					{
						//클릭했을때
						DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_CLICKED, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
					else
					{
						//일반 버튼
						DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_NORMAL, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
					}
				}
				else
				{
					//이전
					//DrawTexture(_imgScrollBtn_Recorded, btnPos, _scrollBtnSize, _scrollBtnSize, _btnColor, true);

					//변경 22.6.9 [v1.4.0]
					DrawTexture_Atlas_PixelPerfect(ref btnPos, ref UV_BUTTON_RECORDED, IMAGE_UNIT_PIXEL_SIZE, IMAGE_UNIT_PIXEL_SIZE, ref _grayColor);
				}
			}

			//다만,
			//클릭을 했다면
			//값을 체크해주자
			if (_isMouseEvent)
			{
				if (_selectedControlParam == null)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						//없으면 새로 선택
						//1. 버튼을 선택하는가 (위치 이동 없음)
						//2. 슬롯을 선택하는가 (위치이동 있음)
						//3. 바를 선택하는가 (위치이동 있음)
						bool isSelect = false;
						if (IsMouseIn(_mousePos, btnPos, _scrollBtnSize * 2, _scrollBtnSize * 2))
						{
							_selectedControlParam = controlParam;
							isSelect = true;
						}
						else
						{
							if (paramAndPosSet != null)
							{
								foreach (KeyValuePair<apModifierParamSet, Vector2> paramPos in paramAndPosSet)
								{
									if (IsMouseIn(_mousePos, paramPos.Value, _slotSize * 1.5f, _slotSize * 1.5f))
									{
										//저장된 위치에서 클릭을 했다면
										_selectedControlParam = controlParam;
										//if(isFromV3)
										//{
										//	//V3는 겹치는게 많아서 안좋을 수 있다.
										//	curValue.x = paramPos.Key._conSyncValue_Vector3.x;
										//	curValue.y = paramPos.Key._conSyncValue_Vector3.y;
										//}
										//else
										//{
										//	curValue = paramPos.Key._conSyncValue_Vector2;
										//}
										curValue = paramPos.Key._conSyncValue_Vector2;



										isSelect = true;
										break;
									}
								}
							}
						}
						if (!isSelect)
						{
							//영역 안쪽을 클릭했는가
							Vector2 relativeMousePos = new Vector2(_mousePos.x - pos.x, _mousePos.y - ((_scrollBtnSize + height) * 0.5f + pos.y));
							bool isMouseInBox = relativeMousePos.x > 0.0f && relativeMousePos.x < boxWidth 
												&& relativeMousePos.y > -boxHeight / 2 && relativeMousePos.y < boxHeight / 2;

							

							//if (IsMouseIn(_mousePos, centerPos, barWidth + _barThickness * 2, barHeight + _barThickness * 2))
							if(isMouseInBox)//바를 포함한 박스를 클릭했을 때
							{
								//Debug.Log("Mouse Pos In Box : " + relativeMousePos + " / Box : " + boxWidth + "x" + boxHeight);
								_selectedControlParam = controlParam;

								//위치에 맞게 curValue를 바꾸자
								float mousePosX = _mousePos.x;
								float mousePosY = _mousePos.y;

								//X 좌표 계산
								if (mousePosX < minPosX)
								{
									curValue.x = minValue.x;
								}
								else if (mousePosX > maxPosX)
								{
									curValue.x = maxValue.x;
								}
								else
								{
									float itp = (mousePosX - minPosX) / (maxPosX - minPosX);
									curValue.x = minValue.x * (1.0f - itp) + maxValue.x * itp;
									curValue.x = Mathf.Clamp(curValue.x, minValue.x, maxValue.x);
								}

								//Y 좌표 계산 (좌표와 값이 반대다)
								if (mousePosY < minPosY)
								{
									curValue.y = maxValue.y;
								}
								else if (mousePosY > maxPosY)
								{
									curValue.y = minValue.y;
								}
								else
								{
									float itp = (mousePosY - minPosY) / (maxPosY - minPosY);
									curValue.y = maxValue.y * (1.0f - itp) + minValue.y * itp;
									curValue.y = Mathf.Clamp(curValue.y, minValue.y, maxValue.y);
								}
							}
						}

						if (_selectedControlParam != null)
						{
							_isMouseEventUsed = true;
							GUI.FocusControl(null);
						}
					}
				}
				else if (isSelected)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down ||
						_leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
					{
						//있는데... 같을 때만 처리
						//마우스 위치에 맞게 이동하자

						//위치에 맞게 curValue를 바꾸자
						float mousePosX = _mousePos.x;
						float mousePosY = _mousePos.y;

						//X 좌표 계산
						if (mousePosX < minPosX)
						{
							curValue.x = minValue.x;
						}
						else if (mousePosX > maxPosX)
						{
							curValue.x = maxValue.x;
						}
						else
						{
							float itp = (mousePosX - minPosX) / (maxPosX - minPosX);
							curValue.x = minValue.x * (1.0f - itp) + maxValue.x * itp;
							curValue.x = Mathf.Clamp(curValue.x, minValue.x, maxValue.x);
						}

						//Y 좌표 계산 (좌표와 값이 반대다)
						if (mousePosY < minPosY)
						{
							curValue.y = maxValue.y;
						}
						else if (mousePosY > maxPosY)
						{
							curValue.y = minValue.y;
						}
						else
						{
							float itp = (mousePosY - minPosY) / (maxPosY - minPosY);
							curValue.y = maxValue.y * (1.0f - itp) + minValue.y * itp;
							curValue.y = Mathf.Clamp(curValue.y, minValue.y, maxValue.y);
						}

						_isMouseEventUsed = true;
					}
				}
			}

			if (isRepaint)
			{
				//렌더링을 여기서 종료한다.
				_matBatch.EndPass();
			}

			return curValue;
		}


		//--------------------------------------------------------------------------------------------------------------------
		private static bool IsMouseIn(Vector2 mousePos, Vector2 componentPos, float width, float height)
		{
			//_layoutPosX = posX;
			//_layoutPosY = posY;
			//_layoutWidth = layoutWidth;
			//_layoutHeight = layoutHeight;


			if (
				componentPos.x - (width * 0.5f) <= mousePos.x && mousePos.x <= componentPos.x + (width * 0.5f) &&
				componentPos.y - (height * 0.5f) <= mousePos.y && mousePos.y <= componentPos.y + (height * 0.5f)
				)
			{
				return true;
			}
			return false;
		}

	}


}