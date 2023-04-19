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

	public class apPSDGL
	{
		// Members
		//------------------------------------------------------------------------
		//private int _windowPosX = 0;
		//private int _windowPosY = 0;
		private int _windowWidth = 0;
		private int _windowHeight = 0;

		private Vector2 _windowScroll = Vector2.zero;

		private float _zoom = 1.0f;
		public float Zoom { get { return _zoom; } }

		//private GUIStyle _textStyle = GUIStyle.none;

		private Vector4 _glScreenClippingSize = Vector4.zero;
		private apGL.MaterialBatch _matBatch = new apGL.MaterialBatch();

		public Vector2 WindowSize { get { return new Vector2(_windowWidth, _windowHeight); } }
		public Vector2 WindowSizeHalf { get { return new Vector2(_windowWidth / 2, _windowHeight / 2); } }

		// Init
		//------------------------------------------------------------------------
		public apPSDGL()
		{

		}

		//public void SetMaterial(Material mat_Color, Material mat_Texture, Material mat_MaskedTexture)
		public void SetShader(Shader shader_Color,
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
			//_mat_Color = mat_Color;
			//_mat_Texture = mat_Texture;

			//_matBatch.SetMaterial(mat_Color, mat_Texture, mat_MaskedTexture);
			_matBatch.SetShader(shader_Color,
				shader_Texture_Normal_Set,
				shader_Texture_VColorAdd_Set,
				//shader_MaskedTexture_Set,
				shader_MaskOnly,
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

		public void SetWindowSize(int windowWidth, int windowHeight, Vector2 scroll, float zoom,
			int posX, int posY, int totalEditorWidth, int totalEditorHeight)
		{
			_windowWidth = windowWidth;
			_windowHeight = windowHeight;
			_windowScroll.x = scroll.x * _windowWidth * 0.1f;
			_windowScroll.y = scroll.y * windowHeight * 0.1f;
			_zoom = zoom;

			totalEditorHeight += 30;
			posY += 30;
			posX += 5;
			windowWidth -= 25;
			windowHeight -= 20;

			//_windowPosX = posX;
			//_windowPosY = posY;

			//float leftMargin = posX;
			//float rightMargin = totalEditorWidth - (posX + windowWidth);
			//float topMargin = posY;
			//float bottomMargin = totalEditorHeight - (posY + windowHeight);

			_glScreenClippingSize.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize.y = (float)(posY) / (float)totalEditorHeight;
			_glScreenClippingSize.z = (float)(posX + windowWidth) / (float)totalEditorWidth;
			_glScreenClippingSize.w = (float)(posY + windowHeight) / (float)totalEditorHeight;

			_matBatch.CheckMaskTexture(_windowWidth, _windowHeight);

			//추가 21.5.19 : 클리핑 사이즈를 바로 설정
			_matBatch.SetClippingSizeToAllMaterial(_glScreenClippingSize);
		}

		public Vector2 World2GL(Vector2 pos)
		{
			//(posX * Zoom) + (_windowWidth * 0.5f) - (ScrollX) = glX
			//(glX + ScrollX - (_windowWidth * 0.5f)) / Zoom
			return new Vector2(
				(pos.x * _zoom) + (_windowWidth * 0.5f)
				- _windowScroll.x,

				(_windowHeight - (pos.y * _zoom)) - (_windowHeight * 0.5f)
				- _windowScroll.y
				);
		}

		public Vector2 GL2World(Vector2 glPos)
		{
			return new Vector2(
				(glPos.x + (_windowScroll.x) - (_windowWidth * 0.5f)) / _zoom,
				(-1 * (glPos.y + _windowScroll.y + (_windowHeight * 0.5f) - (_windowHeight))) / _zoom
				);
		}

		// 최적화형
		//-------------------------------------------------------------------------------
		//public void BeginBatch_ColoredPolygon()
		//{
		//	_matBatch.SetPass_Color();
		//	_matBatch.SetClippingSize(_glScreenClippingSize);

		//	GL.Begin(GL.TRIANGLES);
		//}

		//public void BeginBatch_ColoredLine()
		//{
		//	_matBatch.SetPass_Color();
		//	_matBatch.SetClippingSize(_glScreenClippingSize);

		//	GL.Begin(GL.LINES);
		//}

		//public void EndBatch()
		//{
		//	GL.End();
		//	GL.Flush();
		//}


		public void EndPass()
		{
			//렌더링중인게 있다면 종료
			_matBatch.EndPass();
		}

		//-------------------------------------------------------------------------------
		// Draw Line
		//-------------------------------------------------------------------------------
		public void DrawLine(Vector2 pos1, Vector2 pos2, Color color)
		{
			DrawLine(pos1, pos2, color, true);
		}

		public void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (Vector2.Equals(pos1, pos2))
			{
				return;
			}

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			//Vector2 pos1_Real = pos1;
			//Vector2 pos2_Real = pos2;

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

			//변경 21.5.19
			if (isNeedResetMat)
			{
				//GL.End();//<변환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}

		public void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			if (pos1 == pos2)
			{
				return;
			}

			float halfWidth = width * 0.5f;

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

			//변경 21.5.19
			if (isNeedResetMat)
			{
				//GL.End();//<변환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}


		//-------------------------------------------------------------------------------
		// Draw Texture
		//-------------------------------------------------------------------------------
		private Color _lineColor_Outline = new Color(0.0f, 0.5f, 1.0f, 0.7f);
		public void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, bool isOutlineRender, bool isToneColor = false)
		{
			DrawTexture(image, pos, width, height, color2X, 0.0f, isOutlineRender, isToneColor);
		}

		public void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth, bool isOutlineRender, bool isToneColor = false)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);

			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0_Org = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1_Org = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2_Org = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3_Org = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);

			Vector2 pos_0 = pos_0_Org;
			Vector2 pos_1 = pos_1_Org;
			Vector2 pos_2 = pos_2_Org;
			Vector2 pos_3 = pos_3_Org;


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
			//변경 21.5.19
			if(isToneColor)
			{
				_matBatch.BeginPass_ToneColor_Normal(GL.TRIANGLES, color2X, image);
			}
			else
			{
				_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
			}
			
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);
			GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);
			GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);
			GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);
			GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			
			//삭제 21.5.19
			//GL.End();//<전환완료>


			if (isOutlineRender)
			{
				//변경 21.5.19
				_matBatch.BeginPass_Color(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);

				DrawBoldLine(GL2World(pos_0), GL2World(pos_1), 6.0f, _lineColor_Outline, false);
				DrawBoldLine(GL2World(pos_1), GL2World(pos_2), 6.0f, _lineColor_Outline, false);
				DrawBoldLine(GL2World(pos_2), GL2World(pos_3), 6.0f, _lineColor_Outline, false);
				DrawBoldLine(GL2World(pos_3), GL2World(pos_0), 6.0f, _lineColor_Outline, false);

				//삭제 21.5.19
				//GL.End();//<전환완료>
			}
			//GL.Flush();
			_matBatch.EndPass();

		}



		public void DrawTexture(Texture2D image, apMatrix3x3 matrix, float width, float height, Color color2X, float depth, bool isDrawToneOutline = false)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float width_Half = width * 0.5f;
			float height_Half = height * 0.5f;

			//Zero 대신 mesh Pivot 위치로 삼자
			Vector2 pos_0 = World2GL(matrix.MultiplyPoint(new Vector2(-width_Half, +height_Half)));
			Vector2 pos_1 = World2GL(matrix.MultiplyPoint(new Vector2(+width_Half, +height_Half)));
			Vector2 pos_2 = World2GL(matrix.MultiplyPoint(new Vector2(+width_Half, -height_Half)));
			Vector2 pos_3 = World2GL(matrix.MultiplyPoint(new Vector2(-width_Half, -height_Half)));

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
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
			//변경 21.5.19
			if (!isDrawToneOutline)
			{
				_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
			}
			else
			{

				_matBatch.BeginPass_ToneColor_Custom(GL.TRIANGLES, color2X, image, 0.0f, 0.0f);
			}
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0

			//삭제 21.5.19
			//GL.End();//<전환 완료>
			
			_matBatch.EndPass();
		}

		//------------------------------------------------------------------------------------------------
		// Draw Grid
		//------------------------------------------------------------------------------------------------
		public void DrawGrid()
		{
			int pixelSize = 50;

			Color lineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
			Color lineColor_Center = new Color(0.7f, 0.7f, 0.3f, 0.5f);

			if (_zoom < 0.2f + 0.05f)
			{
				pixelSize = 100;
				lineColor.a = 0.4f;
			}
			else if (_zoom < 0.5f + 0.05f)
			{
				pixelSize = 50;
				lineColor.a = 0.7f;
			}

			//Vector2 centerPos = World2GL(Vector2.zero);

			//Screen의 Width, Height에 해당하는 극점을 찾자
			//Vector2 pos_LT = GL2World(new Vector2(0, 0));
			//Vector2 pos_RB = GL2World(new Vector2(_windowWidth, _windowHeight));
			Vector2 pos_LT = GL2World(new Vector2(-500, -500));
			Vector2 pos_RB = GL2World(new Vector2(_windowWidth + 1500, _windowHeight + 1500));

			float yWorld_Max = Mathf.Max(pos_LT.y, pos_RB.y) + 100;
			float yWorld_Min = Mathf.Min(pos_LT.y, pos_RB.y) - 200;
			float xWorld_Max = Mathf.Max(pos_LT.x, pos_RB.x);
			float xWorld_Min = Mathf.Min(pos_LT.x, pos_RB.x);

			// 가로줄 먼저 (+- Y로 움직임)
			Vector2 curPos = Vector2.zero;
			//Vector2 curPosGL = Vector2.zero;
			Vector2 posA, posB;

			curPos.y = (int)(yWorld_Min / pixelSize) * pixelSize;

			// + Y 방향 (아래)
			while (true)
			{
				//curPosGL = World2GL(curPos);

				//if(curPosGL.y < 0 || curPosGL.y > _windowHeight)
				//{
				//	break;
				//}
				if (curPos.y > yWorld_Max)
				{
					break;
				}


				posA.x = pos_LT.x;
				posA.y = curPos.y;

				posB.x = pos_RB.x;
				posB.y = curPos.y;

				DrawLine(posA, posB, lineColor);

				curPos.y += pixelSize;
			}


			curPos = Vector2.zero;
			curPos.x = (int)(xWorld_Min / pixelSize) * pixelSize;

			// + X 방향 (오른쪽)
			while (true)
			{
				//curPosGL = World2GL(curPos);

				//if(curPosGL.x < 0 || curPosGL.x > _windowWidth)
				//{
				//	break;
				//}
				if (curPos.x > xWorld_Max)
				{
					break;
				}

				posA.y = pos_LT.y;
				posA.x = curPos.x;

				posB.y = pos_RB.y;
				posB.x = curPos.x;

				DrawLine(posA, posB, lineColor);

				curPos.x += pixelSize;
			}

			//중앙선

			curPos = Vector2.zero;

			posA.x = pos_LT.x;
			posA.y = curPos.y;

			posB.x = pos_RB.x;
			posB.y = curPos.y;

			DrawLine(posA, posB, lineColor_Center);


			posA.y = pos_LT.y;
			posA.x = curPos.x;

			posB.y = pos_RB.y;
			posB.x = curPos.x;

			DrawLine(posA, posB, lineColor_Center);

		}


		//--------------------------------------------------------------------------------------------------
		public void DrawRenderUnit(apRenderUnit renderUnit, apMatrix rootMatrix)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return; }

				//변경 22.3.23
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}

				Color textureColor = renderUnit._meshTransform._meshColor2X_Default;//<<Default값을 사용한다.

				
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._meshTransform._isVisible_Default;//<<Default값을 사용

				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				if (rootMatrix == null)
				{
					for (int i = 0; i < nRenderVerts; i++)
					{
						rVert = renderUnit._renderVerts[i];
						rVert._pos_GL = World2GL(rVert._pos_World_NoMod);//<<Mod 적용 안된걸로
					}
				}
				else
				{
					//추가 21.3.6
					for (int i = 0; i < nRenderVerts; i++)
					{
						rVert = renderUnit._renderVerts[i];						
						rVert._pos_GL = World2GL(rootMatrix.InvMulPoint2(rVert._pos_World_NoMod));//<<Mod 적용 안된걸로 + Root Matrix 역연산
					}
				}
				


				
				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.
				
				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3 && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// Debug.Log("Texture Color : " + textureColor);
					//Color color0 = Color.black, color1 = Color.black, color2 = Color.black;
					
					//변경 21.5.19
					_matBatch.BeginPass_Texture_VColor(GL.TRIANGLES, textureColor, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType, false, Vector4.zero);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);


					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;


					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;

					//색상은 한번만
					GL.Color(Color.black);

					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						
						////------------------------------------------

						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}

					//삭제 21.5.19
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}

				

				//DrawText("<-[" + renderUnit.Name + "_" + renderUnit._debugID + "]", renderUnit.WorldMatrixWrap._pos + new Vector2(10.0f, 0.0f), 100, Color.yellow);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}




		public void DrawRenderUnit_ClippingParent_Renew(apRenderUnit renderUnit,
																List<apTransform_Mesh.ClipMeshSet> childClippedSet,
																RenderTexture externalRenderTexture = null,
																apMatrix rootMatrix = null)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return; }

				//변경 22.3.23 [v1.4.0]
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}

				Color textureColor = renderUnit._meshTransform._meshColor2X_Default;//<<Default값
				apMesh mesh = renderUnit._meshTransform._mesh;


				//if (mesh._textureData == null)
				if (mesh.LinkedTextureData == null)
				{
					return;
				}

				int nClipMeshes = childClippedSet.Count;
				
				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				Color vertexChannelColor = Color.black;
				//Color vColor0 = Color.black, vColor1 = Color.black, vColor2 = Color.black;

				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;
		
				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				
				
				RenderTexture.active = null;

				for (int iPass = 0; iPass < 2; iPass++)
				{
					bool isRenderTexture = false;
					if (iPass == 1)
					{
						isRenderTexture = true;
					}
					
					//변경 21.5.19
					_matBatch.BeginPass_Mask(GL.TRIANGLES, textureColor, mesh.LinkedTextureData._image, 0.0f, renderUnit.ShaderType, isRenderTexture, false, Vector4.zero);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);


					//색상은 한번만
					GL.Color(Color.black);

					//------------------------------------------
					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{

						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						//vColor0 = Color.black;
						//vColor1 = Color.black;
						//vColor2 = Color.black;



						if (rootMatrix == null)
						{
							posGL_0 = World2GL(rVert0._pos_World_NoMod);//<<Mod가 적용 안된걸로
							posGL_1 = World2GL(rVert1._pos_World_NoMod);
							posGL_2 = World2GL(rVert2._pos_World_NoMod);
						}
						else
						{
							//추가 21.3.6 : RootMatrix는 빼야한다.
							posGL_0 = World2GL(rootMatrix.InvMulPoint2(rVert0._pos_World_NoMod));//<<Mod가 적용 안된걸로
							posGL_1 = World2GL(rootMatrix.InvMulPoint2(rVert1._pos_World_NoMod));
							posGL_2 = World2GL(rootMatrix.InvMulPoint2(rVert2._pos_World_NoMod));
						}
						

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;
						
						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						
						/*GL.Color(vColor0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						/*GL.Color(vColor1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(vColor2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						/*GL.Color(vColor2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						/*GL.Color(vColor1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(vColor0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					}



					//------------------------------------------
					//삭제 21.5.19
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}

				if (externalRenderTexture == null)
				{
					_matBatch.DeactiveRenderTexture();
				}
				else
				{
					RenderTexture.active = externalRenderTexture;
				}

				

				//3. Child를 렌더링하자
				//for (int iClip = 0; iClip < 3; iClip++)
				for (int iClip = 0; iClip < nClipMeshes; iClip++)
				{
					if(childClippedSet[iClip] == null || childClippedSet[iClip]._meshTransform == null)
					{
						continue;
					}
					apMesh clipMesh = childClippedSet[iClip]._meshTransform._mesh;
					apRenderUnit clipRenderUnit = childClippedSet[iClip]._renderUnit;

					if (clipMesh == null || clipRenderUnit == null) { continue; }
					if (clipRenderUnit._meshTransform == null) { continue; }
					if (!clipRenderUnit._isVisible) { continue; }

					if (clipMesh._indexBuffer.Count < 3)
					{
						continue;
					}

					//변경 21.5.19
					_matBatch.BeginPass_Clipped(GL.TRIANGLES, clipRenderUnit._meshTransform._meshColor2X_Default, 
												clipMesh.LinkedTextureData._image, 
												0.0f, 
												clipRenderUnit.ShaderType, 
												renderUnit._meshTransform._meshColor2X_Default);
					
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);

					//색상은 한번만
					GL.Color(Color.black);

					//------------------------------------------
					for (int i = 0; i < clipMesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= clipMesh._indexBuffer.Count)
						{ break; }

						if (clipMesh._indexBuffer[i + 0] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 1] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 2] >= clipMesh._vertexData.Count)
						{
							break;
						}

						rVert0 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 0]];
						rVert1 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 1]];
						rVert2 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 2]];


						//vColor0 = Color.black;
						//vColor1 = Color.black;
						//vColor2 = Color.black;



						if (rootMatrix == null)
						{
							posGL_0 = World2GL(rVert0._pos_World_NoMod);//Mod 없는 걸로
							posGL_1 = World2GL(rVert1._pos_World_NoMod);
							posGL_2 = World2GL(rVert2._pos_World_NoMod);
						}
						else
						{
							posGL_0 = World2GL(rootMatrix.InvMulPoint2(rVert0._pos_World_NoMod));//Mod 없는 걸로
							posGL_1 = World2GL(rootMatrix.InvMulPoint2(rVert1._pos_World_NoMod));
							posGL_2 = World2GL(rootMatrix.InvMulPoint2(rVert2._pos_World_NoMod));
						}

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;

						uv_0 = clipMesh._vertexData[clipMesh._indexBuffer[i + 0]]._uv;
						uv_1 = clipMesh._vertexData[clipMesh._indexBuffer[i + 1]]._uv;
						uv_2 = clipMesh._vertexData[clipMesh._indexBuffer[i + 2]]._uv;


						/*GL.Color(vColor0);*/	GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0
						/*GL.Color(vColor1);*/	GL.TexCoord(uv_1);	GL.Vertex(pos_1); // 1
						/*GL.Color(vColor2);*/	GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2

						//Back Side
						/*GL.Color(vColor2);*/	GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2
						/*GL.Color(vColor1);*/	GL.TexCoord(uv_1);	GL.Vertex(pos_1); // 1
						/*GL.Color(vColor0);*/	GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0


					}
					//------------------------------------------------
					//삭제 21.5.19
					//GL.End();//<전환 완료>
					_matBatch.EndPass();

				}

				//사용했던 RenderTexture를 해제한다.
				_matBatch.ReleaseRenderTexture();
				//_matBatch.DeactiveRenderTexture();

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public void DrawBox(Vector2 pos, float width, float height, Color color, bool isWireframe, bool isNeedResetMat = true)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			pos = World2GL(pos);

			float halfWidth = width * 0.5f * _zoom;
			float halfHeight = height * 0.5f * _zoom;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - halfWidth, pos.y - halfHeight);
			Vector2 pos_1 = new Vector2(pos.x + halfWidth, pos.y - halfHeight);
			Vector2 pos_2 = new Vector2(pos.x + halfWidth, pos.y + halfHeight);
			Vector2 pos_3 = new Vector2(pos.x - halfWidth, pos.y + halfHeight);

			if (isWireframe)
			{
				if (isNeedResetMat)
				{
					//변경 21.5.19
					_matBatch.BeginPass_Color(GL.LINES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);

					//GL.Begin(GL.LINES);
				}

				GL.Color(color);
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);

				GL.Vertex(pos_1);
				GL.Vertex(pos_2);

				GL.Vertex(pos_2);
				GL.Vertex(pos_3);

				GL.Vertex(pos_3);
				GL.Vertex(pos_0);

				//삭제 21.5.19
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}
			}
			else
			{
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

				//삭제 21.5.19
				if (isNeedResetMat)
				{
					//GL.End();//<변환 완료>
					_matBatch.EndPass();
				}
			}

			//GL.Flush();
		}

		public void DrawMesh(	apMesh mesh,
								apMatrix3x3 matrix,
								Color color2X,
								bool isShowAllTexture,
								bool isDrawOutline,
								bool isDrawEdge,
								bool isDrawToneOutline,
								Texture2D alternativeTexture = null)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)//이전 코드
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)//변경 코드
				{
					return;
				}

				//1. 모든 메시를 보여줄때 (또는 클리핑된 메시가 없을 때) => 
				Color textureColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//Color shadedTextureColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
				Color atlasBorderColor = new Color(0.0f, 1.0f, 1.0f, 0.5f);
				Color meshEdgeColor = new Color(1.0f, 0.5f, 0.0f, 0.9f);
				Color meshHiddenEdgeColor = new Color(1.0f, 1.0f, 0.0f, 0.7f);

				matrix *= mesh.Matrix_VertToLocal;


				Texture2D targetTexture = mesh.LinkedTextureData._image;
				if(alternativeTexture != null)
				{
					targetTexture = alternativeTexture;
				}

				if (isShowAllTexture)
				{
					DrawTexture(targetTexture, matrix, mesh.LinkedTextureData._width, mesh.LinkedTextureData._height, textureColor, -10);
				}
				
				Vector2 pos2_0 = Vector2.zero;
				Vector2 pos2_1 = Vector2.zero;
				Vector2 pos2_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				

				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3)
				{
					//변경 21.5.19
					if (!isDrawToneOutline)
					{
						
						_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, targetTexture, apPortrait.SHADER_TYPE.AlphaBlend);
					}
					else
					{
						_matBatch.BeginPass_ToneColor_Custom(GL.TRIANGLES, color2X, targetTexture, 0.0f, 0.0f);
					}
					
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);

					//------------------------------------------
					apVertex vert0, vert1, vert2;
					//Color color0 = Color.black, color1 = Color.black, color2 = Color.black;
					//Color color0 = Color.white, color1 = Color.white, color2 = Color.white;
					
					//색상은 한번만
					GL.Color(Color.white);

					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						vert0 = mesh._vertexData[mesh._indexBuffer[i + 0]];
						vert1 = mesh._vertexData[mesh._indexBuffer[i + 1]];
						vert2 = mesh._vertexData[mesh._indexBuffer[i + 2]];

						
						pos2_0 = World2GL(matrix.MultiplyPoint(vert0._pos));
						pos2_1 = World2GL(matrix.MultiplyPoint(vert1._pos));
						pos2_2 = World2GL(matrix.MultiplyPoint(vert2._pos));

						pos_0 = new Vector3(pos2_0.x, pos2_0.y, vert0._zDepth * 0.1f);
						pos_1 = new Vector3(pos2_1.x, pos2_1.y, vert1._zDepth * 0.5f);
						pos_2 = new Vector3(pos2_2.x, pos2_2.y, vert2._zDepth * 0.5f);//<<Z값이 반영되었다.

						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						
						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						//Back Side
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						

						////------------------------------------------
					}

					//삭제 21.5.19
					//GL.End();//<전환 완료>
					_matBatch.EndPass();

				}

				if (mesh._isPSDParsed && isDrawOutline)
				{
					Vector2 pos_LT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_LT.y));
					Vector2 pos_RT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_LT.y));
					Vector2 pos_LB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_RB.y));
					Vector2 pos_RB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_RB.y));

					//변경 21.5.19
					_matBatch.BeginPass_Color(GL.LINES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.LINES);

					DrawLine(pos_LT, pos_RT, atlasBorderColor, false);
					DrawLine(pos_RT, pos_RB, atlasBorderColor, false);
					DrawLine(pos_RB, pos_LB, atlasBorderColor, false);
					DrawLine(pos_LB, pos_LT, atlasBorderColor, false);
					
					//삭제 21.5.19
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}

				//외곽선을 그려주자
				//float imageWidthHalf = mesh._textureData._width * 0.5f;
				//float imageHeightHalf = mesh._textureData._height * 0.5f;

				float imageWidthHalf = mesh.LinkedTextureData._width * 0.5f;
				float imageHeightHalf = mesh.LinkedTextureData._height * 0.5f;

				Vector2 pos_TexOutline_LT = matrix.MultiplyPoint(new Vector2(-imageWidthHalf, -imageHeightHalf));
				Vector2 pos_TexOutline_RT = matrix.MultiplyPoint(new Vector2(imageWidthHalf, -imageHeightHalf));
				Vector2 pos_TexOutline_LB = matrix.MultiplyPoint(new Vector2(-imageWidthHalf, imageHeightHalf));
				Vector2 pos_TexOutline_RB = matrix.MultiplyPoint(new Vector2(imageWidthHalf, imageHeightHalf));

				//변경 21.5.19
				_matBatch.BeginPass_Color(GL.LINES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);

				DrawLine(pos_TexOutline_LT, pos_TexOutline_RT, atlasBorderColor, false);
				DrawLine(pos_TexOutline_RT, pos_TexOutline_RB, atlasBorderColor, false);
				DrawLine(pos_TexOutline_RB, pos_TexOutline_LB, atlasBorderColor, false);
				DrawLine(pos_TexOutline_LB, pos_TexOutline_LT, atlasBorderColor, false);

				//삭제 21.5.19
				//GL.End();//<전환 완료>
				_matBatch.EndPass();


				//3. Edge를 렌더링하자 (전체 / Ouline)
				if(isDrawEdge)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					if (mesh._edges.Count > 0)
					{
						//변경 21.5.19
						_matBatch.BeginPass_Color(GL.LINES);
						//_matBatch.SetClippingSize(_glScreenClippingSize);
						//GL.Begin(GL.LINES);


						for (int i = 0; i < mesh._edges.Count; i++)
						{
							pos0 = matrix.MultiplyPoint(mesh._edges[i]._vert1._pos);
							pos1 = matrix.MultiplyPoint(mesh._edges[i]._vert2._pos);

							DrawLine(pos0, pos1, meshEdgeColor, false);
						}

						for (int iPoly = 0; iPoly < mesh._polygons.Count; iPoly++)
						{
							for (int iHE = 0; iHE < mesh._polygons[iPoly]._hidddenEdges.Count; iHE++)
							{
								apMeshEdge hiddenEdge = mesh._polygons[iPoly]._hidddenEdges[iHE];

								pos0 = matrix.MultiplyPoint(hiddenEdge._vert1._pos);
								pos1 = matrix.MultiplyPoint(hiddenEdge._vert2._pos);

								DrawLine(pos0, pos1, meshHiddenEdgeColor, false);
							}

						}

						//삭제 21.5.19
						//GL.End();//<전환 완료>
						_matBatch.EndPass();
					}
				}
				



			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		public void DrawMeshEdgeOnly(apMesh mesh, apMatrix3x3 matrix)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)//이전 코드
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)//변경 코드
				{
					return;
				}

				//1. 모든 메시를 보여줄때 (또는 클리핑된 메시가 없을 때) => 
				Color meshEdgeColor = new Color(1.0f, 0.5f, 0.0f, 0.9f);
				Color meshHiddenEdgeColor = new Color(1.0f, 1.0f, 0.0f, 0.7f);

				matrix *= mesh.Matrix_VertToLocal;
				

				Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
				if (mesh._edges.Count > 0)
				{
					//변경 21.5.19
					_matBatch.BeginPass_Color(GL.LINES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.LINES);



					for (int i = 0; i < mesh._edges.Count; i++)
					{
						pos0 = matrix.MultiplyPoint(mesh._edges[i]._vert1._pos);
						pos1 = matrix.MultiplyPoint(mesh._edges[i]._vert2._pos);

						DrawLine(pos0, pos1, meshEdgeColor, false);
					}

					for (int iPoly = 0; iPoly < mesh._polygons.Count; iPoly++)
					{
						for (int iHE = 0; iHE < mesh._polygons[iPoly]._hidddenEdges.Count; iHE++)
						{
							apMeshEdge hiddenEdge = mesh._polygons[iPoly]._hidddenEdges[iHE];

							pos0 = matrix.MultiplyPoint(hiddenEdge._vert1._pos);
							pos1 = matrix.MultiplyPoint(hiddenEdge._vert2._pos);

							DrawLine(pos0, pos1, meshHiddenEdgeColor, false);
						}

					}

					//삭제 21.5.19
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}
				



			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
	}

}