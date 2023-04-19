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
using System.Runtime.InteropServices;

namespace AnyPortrait
{

	public static class apAnimCurveGL
	{
		// Members
		//------------------------------------------------------------
		private static int _layoutPosX = 0;
		private static int _layoutPosY = 0;
		private static int _layoutWidth = 0;
		private static int _layoutHeight = 0;
		private static int _scrollOffsetY = 0;
		private static int _clipPosY = 0;
		
		private static Vector4 _glScreenClippingSize = Vector4.zero;

		private static apGL.MaterialBatch _matBatch = new apGL.MaterialBatch();


		private static bool _isMouseEvent = false;
		private static bool _isMouseEventUsed = false;
		private static apMouse.MouseBtnStatus _leftBtnStatus = apMouse.MouseBtnStatus.Released;
		private static Vector2 _mousePos = Vector2.zero;
		//private static Vector2 _mousePos_Down = Vector2.zero;

		//private static EventType _curEventType;
		private static apSelection _selection;

		private static GUIStyle _textStyle = null;
		private static bool _isMouseInputIgnored = false;

		private static Texture2D _img_ControlPoint = null;

		private static Color _color_Grid = new Color(0.3f, 0.3f, 0.3f, 1.0f);
		private static Color _color_Text = new Color(0.7f, 0.7f, 0.7f, 1.0f);

		private static Color _color_Gray = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		// Init
		//------------------------------------------------------------
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

			if(_textStyle == null)
			{
				_textStyle = new GUIStyle(GUIStyle.none);
			}
		}


		public static void SetTexture(Texture2D img_ControlPoint)
		{
			_img_ControlPoint = img_ControlPoint;

			if(_textStyle == null)
			{
				_textStyle = new GUIStyle(GUIStyle.none);
			}
		}

		// GUI/Event Setting
		//------------------------------------------------------------
		public static void SetLayoutSize(int layoutWidth, int layoutHeight,
											int posX, int posY,
											int scrollOffsetY,
											int clipPosY,
											int totalEditorWidth, int totalEditorHeight)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			_layoutPosX = posX;
			_layoutPosY = posY;
			_scrollOffsetY = scrollOffsetY;
			_clipPosY = clipPosY;
			_layoutWidth = layoutWidth;
			_layoutHeight = layoutHeight;

			//원래는 30 / Timeline은 28
			totalEditorHeight += 30;
			posY += 30;

			posX += 5;
			//layoutWidth -= 25;
			//layoutWidth -= 17;//<<

			layoutWidth -= 12;

			//layoutHeight -= 4;
			//layoutHeight -= 20; //?

			_glScreenClippingSize.x = (float)(posX) / (float)totalEditorWidth;
			_glScreenClippingSize.y = (float)(posY - _clipPosY) / (float)totalEditorHeight;
			_glScreenClippingSize.z = (float)(posX + layoutWidth) / (float)totalEditorWidth;
			_glScreenClippingSize.w = (float)(posY + _layoutHeight - _clipPosY) / (float)totalEditorHeight;

			_isMouseEvent = false;
			_isMouseEventUsed = false;

			//변경 21.5.19
			_matBatch.SetClippingSizeToAllMaterial(_glScreenClippingSize);
		}

		public static void SetMouseValue(bool isLeftBtnPressed,
											Vector2 mousePos,
											EventType curEventType,
											apSelection selection)
		{
			_isMouseEvent = true;
			_isMouseEventUsed = false;

			_mousePos = mousePos;

			_mousePos.x -= _layoutPosX;
			_mousePos.y -= (_layoutPosY - _scrollOffsetY);

			_mousePos.x -= 1;
			_mousePos.y -= 7;

			bool isMouseEvent = (Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp ||
				Event.current.rawType == EventType.MouseDrag);

			if (isMouseEvent)
			{
				if (isLeftBtnPressed)
				{
					if (_isMouseInputIgnored)
					{
						//무시..
					}
					else
					{
						//첫 클릭때 체크하자
						//bool isMouseDown = false;
						//if (isLeftBtnPressed && (_leftBtnStatus == apMouse.MouseBtnStatus.Up || _leftBtnStatus == apMouse.MouseBtnStatus.Released))
						//{
						//	isMouseDown = true;
						//}

						//막 눌리기 시작했을때
						//if (isMouseDown)
						//{
						//	if (IsMouseInLayout(_mousePos))
						//	{
						//		//패스
						//	}
						//}
					}
				}
				else
				{
					if (_isMouseInputIgnored)
					{
						_isMouseInputIgnored = false;//해제..
					}
				}
			}





			//_curEventType = curEventType;
			_selection = selection;

			if (curEventType != EventType.MouseDown &&
				curEventType != EventType.MouseDrag &&
				curEventType != EventType.MouseMove &&
				curEventType != EventType.MouseUp)
			{
				_isMouseEvent = false;
			}

			if (_selection.SelectionType != apSelection.SELECTION_TYPE.Animation)
			{
				_isMouseEvent = false;
			}

			if (isMouseEvent)
			{
				if (isLeftBtnPressed)
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Pressed;
					}
					else
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Down;
					}
				}
				else
				{
					if (_leftBtnStatus == apMouse.MouseBtnStatus.Down || _leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Up;
					}
					else
					{
						_leftBtnStatus = apMouse.MouseBtnStatus.Released;
					}
				}
			}

			if (_isMouseInputIgnored)
			{
				_isMouseEvent = false;
				_isMouseEventUsed = true;
			}
		}


		public static void SetMouseUse()
		{
			_isMouseEventUsed = true;
		}

		// GUI
		//------------------------------------------------------------
		//렌더링중인 Pass를 종료한다.
		public static void EndPass()
		{
			_matBatch.EndPass();
		}

		// Draw Line
		//-------------------------------------------------------------------------------------------------------
		public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch == null)
			{ return; }
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{
				return;
			}

			if (isNeedResetMat)
			{
				//변경 21.5.19
				_matBatch.BeginPass_Color(GL.LINES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			GL.Color(color);
			GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
			GL.Vertex(new Vector3(pos2.x, pos2.y, 0.0f));

			//삭제 21.5.19
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				_matBatch.EndPass();
			}
		}


		// Draw Box
		//---------------------------------------------------------------------------------------------------------
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isNeedResetMat)
		{
			if (isNeedResetMat)
			{
				if (_matBatch == null)
				{ return; }
				if (_matBatch.IsNotReady())
				{ return; }
			}

			float halfWidth = width * 0.5f;
			float halfHeight = height * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector3 pos_0 = new Vector3(pos.x - halfWidth, pos.y - halfHeight, 0);
			Vector3 pos_1 = new Vector3(pos.x + halfWidth, pos.y - halfHeight, 0);
			Vector3 pos_2 = new Vector3(pos.x + halfWidth, pos.y + halfHeight, 0);
			Vector3 pos_3 = new Vector3(pos.x - halfWidth, pos.y + halfHeight, 0);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2

			if (isNeedResetMat)
			{
				//변경 21.5.19
				_matBatch.BeginPass_Color(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				//GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_0); // 0

			//삭제
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				_matBatch.EndPass();
			}
		}

		// Draw Texture
		//--------------------------------------------------------------------------------------------------
		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, bool isNeedResetMat)
		{
			if (isNeedResetMat)
			{
				if (_matBatch == null)
				{ return; }
				if (_matBatch.IsNotReady())
				{ return; }
			}

			float realWidth = width;
			float realHeight = height;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);

			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}

			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			if (isNeedResetMat)
			{
				//변경 21.5.19
				_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				//GL.Begin(GL.TRIANGLES);
			}

			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0
			GL.TexCoord(uv_1);
			GL.Vertex(new Vector3(pos_1.x, pos_1.y, 0)); // 1
			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2

			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, 0)); // 2
			GL.TexCoord(uv_3);
			GL.Vertex(new Vector3(pos_3.x, pos_3.y, 0)); // 3
			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, 0)); // 0

			//삭제 21.5.19
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				_matBatch.EndPass();
			}

			//GL.Flush();
		}


		public static void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
		{
			if (isNeedResetMat)
			{
				if (_matBatch == null)
				{ return; }
				if (_matBatch.IsNotReady())
				{ return; }
			}

			if (pos1 == pos2)
			{
				return;
			}

			//float halfWidth = width * 0.5f / _zoom;
			float halfWidth = width * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			// -------->
			// |    1
			// | 0     2
			// | 
			// | 
			// | 
			// | 5     3
			// |    4

			Vector2 dir = (pos1 - pos2).normalized;
			Vector2 dirRev = new Vector2(-dir.y, dir.x);

			Vector2 pos_0 = pos1 - dirRev * halfWidth;
			Vector2 pos_1 = pos1 + dir * halfWidth;
			//Vector2 pos_1 = pos1;
			Vector2 pos_2 = pos1 + dirRev * halfWidth;

			Vector2 pos_3 = pos2 + dirRev * halfWidth;
			Vector2 pos_4 = pos2 - dir * halfWidth;
			//Vector2 pos_4 = pos2;
			Vector2 pos_5 = pos2 - dirRev * halfWidth;

			if (isNeedResetMat)
			{
				//변경 21.5.19
				_matBatch.BeginPass_Color(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				//GL.Begin(GL.TRIANGLES);
			}

			GL.Color(color);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_3); // 3

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_0); // 0

			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_3); // 3

			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_5); // 5

			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_3); // 3

			//삭제 21.5.19
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				_matBatch.EndPass();
			}
		}


		public static void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color colorA, Color colorB, bool isNeedResetMat)
		{
			if (isNeedResetMat)
			{
				if (_matBatch == null)
				{ return; }
				if (_matBatch.IsNotReady())
				{ return; }
			}

			if (pos1 == pos2)
			{
				return;
			}

			//float halfWidth = width * 0.5f / _zoom;
			float halfWidth = width * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			// -------->
			// |    1
			// | 0     2
			// | 
			// | 
			// | 
			// | 5     3
			// |    4

			Vector2 dir = (pos1 - pos2).normalized;
			Vector2 dirRev = new Vector2(-dir.y, dir.x);

			Vector2 pos_0 = pos1 - dirRev * halfWidth;
			Vector2 pos_1 = pos1 + dir * halfWidth;
			//Vector2 pos_1 = pos1;
			Vector2 pos_2 = pos1 + dirRev * halfWidth;

			Vector2 pos_3 = pos2 + dirRev * halfWidth;
			Vector2 pos_4 = pos2 - dir * halfWidth;
			//Vector2 pos_4 = pos2;
			Vector2 pos_5 = pos2 - dirRev * halfWidth;

			if (isNeedResetMat)
			{
				//변경 21.5.19
				_matBatch.BeginPass_Color(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);
			}

			GL.Color(colorA);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_2); // 2

			GL.Color(colorA);
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_1); // 1
			GL.Vertex(pos_0); // 0

			GL.Color(colorA);
			GL.Vertex(pos_0); // 0
			GL.Vertex(pos_2); // 2

			GL.Color(colorB);
			GL.Vertex(pos_3); // 3

			GL.Color(colorB);
			GL.Vertex(pos_3); // 3

			GL.Color(colorA);
			GL.Vertex(pos_2); // 2
			GL.Vertex(pos_0); // 0

			GL.Color(colorB);
			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_5); // 5

			GL.Color(colorA);
			GL.Vertex(pos_0); // 0

			GL.Color(colorA);
			GL.Vertex(pos_0); // 0

			GL.Color(colorB);
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_3); // 3

			GL.Color(colorB);
			GL.Vertex(pos_3); // 3
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_5); // 5

			GL.Color(colorB);
			GL.Vertex(pos_5); // 5
			GL.Vertex(pos_4); // 4
			GL.Vertex(pos_3); // 3

			//삭제 21.5.19
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				_matBatch.EndPass();
			}
		}


		//----------------------------------------------------------------------------------------
		// Draw Text
		//----------------------------------------------------------------------------------------
		public static void DrawText(string text, Vector2 pos, float width, Color color)
		{
			if (pos.x < 0)
			{ return; }
			if (pos.x + (width) > _layoutWidth)
			{ return; }

			_textStyle.normal.textColor = color;
			//GUI.Label(new Rect(pos.x, pos.y, width, 30), text, _textStyle);

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = color;
			//GUI.Label(new Rect(pos.x, pos.y, width, 30), text);
			GUI.Label(new Rect(pos.x, pos.y, width, 30), text, _textStyle);

			GUI.backgroundColor = prevColor;
			
		}

		public static void DrawNumber(string number, Vector2 pos, Color color)
		{
			//float width = GetIntWidth(number);
			float width = number.Length * 7;
			DrawText(number, pos - new Vector2(width / 2, 0), width, color);
		}

		public static void DrawNumberRightAlign(string number, Vector2 pos, Color color)
		{
			//float width = GetIntWidth(number);
			float width = number.Length * 7;
			DrawText(number, pos - new Vector2(width, 0), width, color);
		}



		//----------------------------------------------------------------------------------------
		// Draw Curve And Update
		//----------------------------------------------------------------------------------------

		public static bool DrawCurve(apAnimCurve curveA, apAnimCurve curveB, apAnimCurveResult curveResult, Color graphColorA, Color graphColorB)
		{
			if (_matBatch == null)
			{ return false; }
			if (_matBatch.IsNotReady())
			{ return false; }

			//int leftAxisSize = 40;
			//int bottomAxisSize = 5;
			//int margin = 5;



			int leftAxisSize = 30;
			int bottomAxisSize = 0;
			int margin = 15;

			int gridWidth = _layoutWidth - (leftAxisSize + 2 * margin + 4);
			int gridHeight = _layoutHeight - (bottomAxisSize + 2 * margin + 4);

			bool isCurveChanged = false;

			//TODO
			//1. Grid를 그린다. + Grid의 축에 글씨를 쓴다. (왼쪽에 Percent)
			DrawGrid(leftAxisSize, bottomAxisSize, margin, gridWidth, gridHeight);

			Vector2 posA = new Vector2(leftAxisSize + margin, margin + gridHeight);
			Vector2 posB = new Vector2(leftAxisSize + margin + gridWidth, margin);

			//3. 계산되는 그래프를 BoldLine으로 그린다.
			switch (curveResult.CurveTangentType)
			{
				case apAnimCurve.TANGENT_TYPE.Linear:
					{
						DrawBoldLine(posA, posB, 3, graphColorA, graphColorB, true);
					}
					break;

				case apAnimCurve.TANGENT_TYPE.Smooth:
					{
						DrawSmoothLine(curveA, curveB, graphColorA, graphColorB, posA, posB);
						isCurveChanged = DrawSmoothLineControlPoints(curveA, curveB, posA, posB);//<<커브를 수정할 수 있다.
					}
					break;

				case apAnimCurve.TANGENT_TYPE.Constant:
					{
						//수정
						//변경전
						//0.5를 기준으로 ITP가 0->1로 변경

						//변경 후
						//1을 기준으로 ITP가 0->1로 변경
						//Vector2 posC_Bottom = new Vector2((posA.x + posB.x) * 0.5f, posA.y);
						//Vector2 posC_Top = new Vector2((posA.x + posB.x) * 0.5f, posB.y);

						//DrawBoldLine(posA, posC_Bottom, 3, graphColorA, true);
						//DrawBoldLine(posC_Bottom, posC_Top, 3, graphColorA, graphColorB, true);
						//DrawBoldLine(posC_Top, posB, 3, graphColorB, true);

						Vector2 posB_Bottom = new Vector2(posB.x, posA.y);
						DrawBoldLine(posA, posB_Bottom, 3, graphColorA, true);
						DrawBoldLine(posB_Bottom, posB, 3, graphColorA, true);
					}
					break;
			}

			//4. 컨트롤 포인트를 그리고 업데이트를 한다.
			DrawBox(posA, 10, 10, Color.white, true);
			DrawBox(posB, 10, 10, Color.white, true);

			if (_isLockMouse && _isMouseEvent)
			{
				if (_leftBtnStatus == apMouse.MouseBtnStatus.Up ||
					_leftBtnStatus == apMouse.MouseBtnStatus.Released)
				{
					_isLockMouse = false;
					_selectedCurveKey = null;
				}
			}

			return isCurveChanged;
		}

		private const string STR_NUM_100 = "100";
		private const string STR_NUM_50 = "50";
		private const string STR_NUM_0 = "0";

		private static void DrawGrid(int leftAxisSize, int bottomAxisSize, int margin, int gridWidth, int gridHeight)
		{
			//변경 21.5.19
			_matBatch.BeginPass_Color(GL.LINES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.LINES);

			//세로줄 한번, 가로줄 한번
			for (int i = 0; i <= 4; i++)
			{
				float posX = leftAxisSize + margin + ((float)(i * gridWidth) / 4.0f);
				DrawLine(new Vector2(posX, margin), new Vector2(posX, margin + gridHeight), _color_Grid, false);
			}

			for (int i = 0; i <= 4; i++)
			{
				float posY = margin + ((float)(i * gridHeight) / 4.0f);
				DrawLine(new Vector2(leftAxisSize + margin, posY), new Vector2(leftAxisSize + margin + gridWidth, posY), _color_Grid, false);
			}

			//삭제 21.5.19
			//GL.End();//<전환 완료>
			_matBatch.EndPass();

			DrawNumberRightAlign(STR_NUM_100, new Vector2(leftAxisSize, margin), _color_Text);
			DrawNumberRightAlign(STR_NUM_50, new Vector2(leftAxisSize, gridHeight * 0.5f), _color_Text);
			DrawNumberRightAlign(STR_NUM_0, new Vector2(leftAxisSize, gridHeight - margin), _color_Text);
		}

		private static void DrawSmoothLine(apAnimCurve curveA, apAnimCurve curveB, Color graphColorA, Color graphColorB, Vector2 posA, Vector2 posB)
		{
			//변경 21.5.19
			_matBatch.BeginPass_Color(GL.TRIANGLES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);

			Vector2 pos_Prev = Vector2.zero;
			Vector2 pos_Cur = Vector2.zero;
			float itp = 0.0f;
			float itpKey = 0.0f;
			float itpY = 0.0f;
			for (int i = 0; i <= 20; i++)
			{
				itp = ((float)i) / 20.0f;
				itpKey = curveA._keyIndex + (itp * (float)(curveA._nextIndex - curveA._keyIndex));

				//itpY = 1.0f - apAnimCurve.GetCurvedRelativeInterpolation(curveA, curveB, itpKey, false);

				itpY = 1.0f - curveA.GetItp_Float(itpKey, false, (int)(itpKey + 0.5f));

				pos_Cur = new Vector2(
					(posA.x * (1.0f - itp)) + (posB.x * itp),
					(posA.y * (1.0f - itpY)) + (posB.y * itpY)
					);

				if (i > 0)
				{
					//DrawBoldLine(pos_Prev, pos_Cur, 3, graphColor, false);
					Color grdColor = graphColorA * (1.0f - itp) + graphColorB * itp;
					DrawBoldLine(pos_Prev, pos_Cur, 3, grdColor, false);
				}

				pos_Prev = pos_Cur;
			}

			//삭제 21.5.19
			//GL.End();//<전환 완료>
			_matBatch.EndPass();
		}

		private static apAnimCurve _selectedCurveKey = null;
		private static bool _isLockMouse = false;
		private static Vector2 _prevMousePos = Vector2.zero;

		private static bool DrawSmoothLineControlPoints(apAnimCurve curveA, apAnimCurve curveB, Vector2 posA, Vector2 posB)
		{
			Vector2 smoothA_Ratio = new Vector2(Mathf.Clamp01(curveA._nextSmoothX), Mathf.Clamp01(curveA._nextSmoothY));
			Vector2 smoothB_Ratio = new Vector2(Mathf.Clamp01(curveB._prevSmoothX), Mathf.Clamp01(curveB._prevSmoothY));

			Vector2 smoothA_Pos = new Vector2(
				posA.x * (1.0f - smoothA_Ratio.x) + posB.x * smoothA_Ratio.x,
				posA.y * (1.0f - smoothA_Ratio.y) + posB.y * smoothA_Ratio.y);

			Vector2 smoothB_Pos = new Vector2(
				posB.x * (1.0f - smoothB_Ratio.x) + posA.x * smoothB_Ratio.x,
				posB.y * (1.0f - smoothB_Ratio.y) + posA.y * smoothB_Ratio.y);

			int pointSize = 16;
			DrawBoldLine(posA, smoothA_Pos, 2, Color.red, true);
			DrawBoldLine(posB, smoothB_Pos, 2, Color.red, true);

			DrawTexture(_img_ControlPoint, smoothA_Pos, pointSize, pointSize, _color_Gray, true);
			DrawTexture(_img_ControlPoint, smoothB_Pos, pointSize, pointSize, _color_Gray, true);

			AddCursorRect(smoothA_Pos, pointSize, pointSize, MouseCursor.MoveArrow);
			AddCursorRect(smoothB_Pos, pointSize, pointSize, MouseCursor.MoveArrow);

			bool isCurveChanged = false;

			if (_isMouseEvent && !_isMouseEventUsed)
			{
				switch (_leftBtnStatus)
				{
					case apMouse.MouseBtnStatus.Down:
					case apMouse.MouseBtnStatus.Pressed:
						if (_leftBtnStatus == apMouse.MouseBtnStatus.Down)
						{
							if (IsMouseInLayout(_mousePos))
							{
								apEditorUtil.SetEditorDirty();
								//이전 : 포인트 영역을 클릭해야만 선택한다.
								//근데 영역이 좀 좁다.
								//if (IsMouseInButton(_mousePos, smoothA_Pos, pointSize, pointSize))
								//{
								//	//Debug.Log("Select A");
								//	_selectedCurveKey = curveA;
								//	_isLockMouse = false;
								//	_prevMousePos = _mousePos;
								//}
								//else if (IsMouseInButton(_mousePos, smoothB_Pos, pointSize, pointSize))
								//{
								//	//Debug.Log("Select B");
								//	_selectedCurveKey = curveB;
								//	_isLockMouse = false;
								//	_prevMousePos = _mousePos;
								//}
								//else
								//{
								//	_selectedCurveKey = null;
								//	_isLockMouse = true;
								//}

								//변경 20.7.6 : 거리 비교로 꽤 잡기 쉽게 만든다.
								//최대 한계 범위를 크게 잡자
								float dstA = Vector2.Distance(_mousePos, smoothA_Pos);
								float dstB = Vector2.Distance(_mousePos, smoothB_Pos);
								float clickLimitDst = pointSize * 4;

								if(dstA < dstB)
								{
									//A 포인트에 더 가깝게 클릭했다.
									if (dstA < clickLimitDst)
									{
										_selectedCurveKey = curveA;
										_isLockMouse = false;
										_prevMousePos = _mousePos;
									}
									else
									{
										_selectedCurveKey = null;
										_isLockMouse = true;
									}
								}
								else
								{
									//B 포인트에 더 가깝게 클릭했다.
									if (dstB < clickLimitDst)
									{
										_selectedCurveKey = curveB;
										_isLockMouse = false;
										_prevMousePos = _mousePos;
									}
									else
									{
										_selectedCurveKey = null;
										_isLockMouse = true;
									}
								}
							}
							else
							{
								_selectedCurveKey = null;
								_isLockMouse = true;
							}
						}

						if (!_isLockMouse)
						{
							apEditorUtil.SetEditorDirty();

							if (_selectedCurveKey == curveA)
							{
								Vector2 nextPos_Ratio = new Vector2(
									(_mousePos.x - posA.x) / (posB.x - posA.x),
									(_mousePos.y - posA.y) / (posB.y - posA.y));

								curveA._nextSmoothX = Mathf.Clamp01(nextPos_Ratio.x);
								curveA._nextSmoothY = Mathf.Clamp01(nextPos_Ratio.y);
							}
							else if (_selectedCurveKey == curveB)
							{
								Vector2 nextPos_Ratio = new Vector2(
									(_mousePos.x - posA.x) / (posB.x - posA.x),
									(_mousePos.y - posA.y) / (posB.y - posA.y));

								curveB._prevSmoothX = 1.0f - Mathf.Clamp01(nextPos_Ratio.x);
								curveB._prevSmoothY = 1.0f - Mathf.Clamp01(nextPos_Ratio.y);
							}

							if (Vector2.Distance(_prevMousePos, _mousePos) > 0.001f)
							{
								curveA.Refresh();
								curveB.Refresh();
								//Debug.LogError("Make Curve");
								_prevMousePos = _mousePos;

								isCurveChanged = true;
							}
						}

						break;


					case apMouse.MouseBtnStatus.Released:
					case apMouse.MouseBtnStatus.Up:
						break;
				}
			}

			return isCurveChanged;
		}

		// Functions
		//---------------------------------------------------------------------
		public static void AddCursorRect(Vector2 pos, float width, float height, MouseCursor cursorType)
		{
			if (pos.x < 0 || pos.x > _layoutWidth || pos.y < 0 || pos.y > _layoutHeight)
			{
				return;
			}
			//pos.x += _layoutPosX;
			//pos.y += _layoutPosY_Header;
			pos.x -= width / 2;
			pos.y -= height / 2;

			//Debug.Log("AddCursorRect [ " + pos + " ]");
			//EditorGUI.DrawRect(new Rect(pos.x, pos.y, width, height), Color.yellow);
			EditorGUIUtility.AddCursorRect(new Rect(pos.x, pos.y, width, height), cursorType);
		}


		private static bool IsMouseInLayout(Vector2 mousePos)
		{
			if (mousePos.x < 0.0f || mousePos.x > _layoutWidth ||
				mousePos.y < (_scrollOffsetY - _clipPosY) || mousePos.y > _layoutHeight)
			{
				//Debug.LogError("Timeline GL : Mouse Over [Mouse Y : " + mousePos.y + ", Layout Y : " + _layoutPosY + ", Height : " + _layoutHeight + ", Clip : " +  _clipPosY + ", Scroll : " + _scrollOffsetY + "]");
				return false;
			}

			//Debug.Log("Timeline GL : Mouse In [Mouse Y : " + mousePos.y + ", Layout Y : " + _layoutPosY + ", Height : " + _layoutHeight + ", Clip : " +  _clipPosY + ", Scroll : " + _scrollOffsetY + "]");
			return true;
		}

		private static bool IsMouseInButton(Vector2 mousePos, Vector2 btnPos, float sizeW, float sizeH)
		{
			if (mousePos.x < btnPos.x - (sizeW * 0.5f) ||
				mousePos.x > btnPos.x + (sizeW * 0.5f) ||
				mousePos.y < btnPos.y - (sizeH * 0.5f) ||
				mousePos.y > btnPos.y + (sizeH * 0.5f))
			{
				return false;
			}
			return true;

		}
	}

}