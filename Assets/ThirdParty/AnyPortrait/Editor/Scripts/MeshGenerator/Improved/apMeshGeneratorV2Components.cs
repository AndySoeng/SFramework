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
	public partial class apMeshGeneratorV2
	{
		// Sub Classes
		//----------------------------------------------
		//< 텍스쳐 정보>
		//텍스쳐 정보는 겹칠 수 있으므로, 따로 모아서 중복처리를 줄이자
		//텍스쳐의 Read-Write Enabled 설정을 켜거나 꺼야하므로, Request를 실행하기 전의 상태로 복구할 수 있어야 한다.
		//처리는 밖에서 하고 정보만 저장한다.
		private class GenTextureData
		{
			public apTextureData _textureData = null;
			public Texture2D _image = null;
			public TextureImporter _texImporter = null;
			public bool _isPrevReadable = false;//처리 이전에 Readable이었는가

			//이전
			//private int _halfSize_X = 0;
			//private int _halfSize_Y = 0;

			//변경 21.9.11 : Image 좌표계와 Asset 좌표계가 다르다.
			private int _imageProp_halfSize_X = 0;
			private int _imageProp_halfSize_Y = 0;
			//private int _texAsset_halfSize_X = 0;
			//private int _texAsset_halfSize_Y = 0;

			private Color[] _colors = null;
			
			//이전
			//private int _imageWidth = 0;
			//private int _imageHeight = 0;

			//변경 21.9.11 : 에셋의 크기와 툴 이미지에서의 크기가 다를 수 있다.
			//좌표계는 ImageProp크기, 색상 샘플은 texAsset 크기
			private int _imagePropWidth = 0;
			private int _imagePropHeight = 0;
			private int _texAssetWidth = 0;
			private int _texAssetHeight = 0;

			private bool _isSameSize = false;
			private float _image2AssetCoordRatio_X = 1.0f;
			private float _image2AssetCoordRatio_Y = 1.0f;

			public GenTextureData(apTextureData textureData,
									Texture2D image,
									TextureImporter texImporter)
			{
				_textureData = textureData;
				_image = image;				
				_texImporter = texImporter;
				_isPrevReadable = _texImporter.isReadable;

				//이전
				//_halfSize_X = _image.width / 2;
				//_halfSize_Y = _image.height / 2;

				//변경 21.9.11 : TextureData와 TextureAsset의 크기와 좌표계가 다를 수 있다.
				_imagePropWidth = textureData._width;
				_imagePropHeight = textureData._height;
				_texAssetWidth = image.width;
				_texAssetHeight = image.height;
				
				_imageProp_halfSize_X = _imagePropWidth / 2;
				_imageProp_halfSize_Y = _imagePropHeight / 2;
				//_texAsset_halfSize_X = _texAssetWidth / 2;
				//_texAsset_halfSize_Y = _texAssetHeight / 2;

				_isSameSize = true;
				_image2AssetCoordRatio_X = 1.0f;
				_image2AssetCoordRatio_Y = 1.0f;

				if(_imagePropWidth != _texAssetWidth
					|| _imagePropHeight != _texAssetHeight)
				{
					//이미지 크기가 다르다.
					_isSameSize = false;

					_image2AssetCoordRatio_X = (float)_texAssetWidth / (float)_imagePropWidth;
					_image2AssetCoordRatio_Y = (float)_texAssetHeight / (float)_imagePropHeight;
				}

			}

			public void ReadColors()
			{
				_colors = _image.GetPixels();
				//_imageWidth = _image.width;
				//_imageHeight = _image.height;

				//Debug.Log("_colors의 크기 : " + _colors.Length + " / 이미지 크기곱 : " + (_imageWidth * _imageHeight));
			}

			/// <summary>
			/// 이미지의 투명도를 체크한다.
			/// 입력된 위치는 텍스쳐 좌표계(중심이 0.5)가 아닌 메시 좌표계이다. (중심이 0)
			/// </summary>
			public bool ImageHitTest(float posX, float posY)
			{	
				//return _image.GetPixel((int)(posX + _halfSize_X), (int)(posY + _halfSize_Y)).a > 0.005f;

				//Clamp 방식
				//이전
				//int iPosX = Mathf.Clamp((int)(posX + _halfSize_X), 0, _imageWidth - 1);
				//int iPosY = Mathf.Clamp((int)(posY + _halfSize_Y), 0, _imageHeight - 1);
				//return _colors[iPosY * _imageWidth + iPosX].a > 0.005f;


				//변환을 해야한다.
				//입력값 : ImageProp 좌표계
				//샘플값 : TexAsset 좌표계
				if(_isSameSize)
				{
					int iPosX = Mathf.Clamp((int)(posX + _imageProp_halfSize_X), 0, _texAssetWidth - 1);
					int iPosY = Mathf.Clamp((int)(posY + _imageProp_halfSize_Y), 0, _texAssetHeight - 1);
					return _colors[iPosY * _texAssetWidth + iPosX].a > 0.005f;
				}
				else
				{
					int iPosX = Mathf.Clamp((int)((posX + _imageProp_halfSize_X) * _image2AssetCoordRatio_X), 0, _texAssetWidth - 1);
					int iPosY = Mathf.Clamp((int)((posY + _imageProp_halfSize_Y) * _image2AssetCoordRatio_Y), 0, _texAssetHeight - 1);
					return _colors[iPosY * _texAssetWidth + iPosX].a > 0.005f;
				}
				
			}
		}



		//< 생성 리퀘스트 >
		//요청을 미리 받아서 서서히 처리할 수 있다.
		private class GenRequest
		{
			public int _index = 0;
			
			public GenResult _result = null;
			public bool _isFailed = false;

			public apMesh _mesh = null;
			public GenTextureData _genTexData = null;

			public Vector2 _area_Min = Vector2.zero;
			public Vector2 _area_Max = Vector2.zero;
			public float _areaWidth;
			public float _areaHeight;


			//변경 21.10.5 : 퀄리티 옵션을 공통이 아닌 Request 각각에 넣는다.
			public int _option_Inner_Density = 2;//이 값이 커질수록 내부의 점이 많이 생성된다. 기본값 4
			public int _option_OuterMargin = 10;//외부로의 여백
			public int _option_InnerMargin = 5;//내부로의 여백
			public bool _option_IsInnerMargin = true;//내부 여백 필요

			//빠른 Normal 체크를 위한 각도 배열
			private const int NUM_NORMAL_VECTOR = 24;
			private Vector2[] _normalDirs = null;
			//private NormalDirResult _normalCheckResult = null;
			private float[] _checkRadius = null;
			private float[] _checkRadiusWeights = null;
			private const int NUM_CHECK_STEP = 4;
			private const float CHECK_RADIUS_MIN = 2.0f;
			private const float CHECK_RADIUS_MAX = 15.0f;
			private const float CHECK_RADIUSWEIGHT_MIN = 1.0f;
			private const float CHECK_RADIUSWEIGHT_MAX = 0.2f;

			private const int NUM_VECTORS_FOR_BIAS = 8;
			private const int NUM_BIAS_VALUE = 6;//이것보다 많으면 그 칸은 비어있지 않다고 판단한다.
			private const float BIAS_DIST = 3.0f;
			private Vector2[] _dirs_forBias = null;




			private ImageHitResult _tmpHitResult = null;



			public class NormalDirResult
			{
				public Vector2 resultDir = Vector2.zero;
				public bool isSuccess = false;
				public bool isCenterChecked = false;
				public NormalDirResult() { }
				public void Init()
				{
					resultDir = Vector2.zero;
					isSuccess = false;
					isCenterChecked = false;
				}
			}


			



			public GenRequest(int index,
								apMesh mesh,
								GenResult result,
								int option_Inner_Density,
								int option_OuterMargin,
								int option_InnerMargin,
								bool option_IsInnerMargin)
			{
				_index = index;
				_result = result;
				_isFailed = false;
				_mesh = mesh;

				//미리 알파 체크를 위한 변수를 만들자
				_normalDirs = new Vector2[NUM_NORMAL_VECTOR];
				for (int i = 0; i < NUM_NORMAL_VECTOR; i++)
				{
					float angle = ((float)i / (float)NUM_NORMAL_VECTOR) * 360.0f;
					angle *= Mathf.Deg2Rad;
					_normalDirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				}
				//_normalCheckResult = new NormalDirResult();

				_checkRadius = new float[NUM_CHECK_STEP];
				_checkRadiusWeights = new float[NUM_CHECK_STEP];
				
				for (int i = 0; i < NUM_CHECK_STEP; i++)
				{
					float lerp = (float)i / (float)(NUM_CHECK_STEP - 1);
					_checkRadius[i] = CHECK_RADIUS_MIN * (1.0f - lerp) + CHECK_RADIUS_MAX * lerp;
					_checkRadiusWeights[i] = CHECK_RADIUSWEIGHT_MIN * (1.0f - lerp) + CHECK_RADIUSWEIGHT_MAX * lerp;
				}

				_dirs_forBias = new Vector2[NUM_VECTORS_FOR_BIAS];
				for (int i = 0; i < NUM_VECTORS_FOR_BIAS; i++)
				{
					float angle = ((float)i / (float)NUM_VECTORS_FOR_BIAS) * 360.0f;
					angle *= Mathf.Deg2Rad;
					_dirs_forBias[i] = new Vector2(Mathf.Cos(angle) * BIAS_DIST, Mathf.Sin(angle) * BIAS_DIST);
				}

				_tmpHitResult = new ImageHitResult();


				//추가 21.10.5 : 옵션을 여기에 넣자
				_option_Inner_Density = option_Inner_Density;
				_option_OuterMargin = option_OuterMargin;
				_option_InnerMargin = option_InnerMargin;
				_option_IsInnerMargin = option_IsInnerMargin;
				
			}

			public void SetGenTextureData(GenTextureData genTexData)
			{
				_genTexData = genTexData;
			}

			public void SetFailed(string errorMsg)
			{
				_isFailed = true;//이 Request는 더이상 처리가 불가능하다.
				_result.SetFailed(errorMsg);
			}
			
			public void CalculateArea()
			{
				Vector2 scanLT = _mesh._atlasFromPSD_LT;
				Vector2 scanRB = _mesh._atlasFromPSD_RB;

				_area_Min = new Vector2(Mathf.Min(scanLT.x, scanRB.x), Mathf.Min(scanLT.y, scanRB.y));
				_area_Max = new Vector2(Mathf.Max(scanLT.x, scanRB.x), Mathf.Max(scanLT.y, scanRB.y));
				_areaWidth = _area_Max.x - _area_Min.x;
				_areaHeight = _area_Max.y - _area_Min.y;
			}

			/// <summary>
			/// 이미지의 투명도를 체크한다.
			/// 입력된 위치는 텍스쳐 좌표계(중심이 0.5)가 아닌 메시 좌표계이다. (중심이 0)
			/// </summary>
			public bool ImageHitTest(float posX, float posY)
			{
				return _genTexData.ImageHitTest(posX, posY);
			}

			public bool ImageHitTest(Vector2 pos)
			{
				return _genTexData.ImageHitTest(pos.x, pos.y);
			}


			#region [미사용 코드] 이 함수는 사용하지 않는다.
			///// <summary>
			///// CheckAlpha의 Bias가 들어갔다. 너무 작은 빈 공간에 대해서는 체크하지 않는다.
			///// 비어있지만 주변은 거의 꽉차있는 경우, 비어있지 않은 것으로 간주한다.
			///// </summary>
			///// <param name="posX"></param>
			///// <param name="posY"></param>
			///// <returns></returns>
			//public bool ImageHitTestWithBias(float posX, float posY)
			//{
			//	bool isChecked = _genTexData.ImageHitTest(posX, posY);
			//	if(isChecked)
			//	{
			//		return true;//빈공간이 아니다.
			//	}

			//	int nCheckedBias = 0;
			//	for (int i = 0; i < NUM_VECTORS_FOR_BIAS; i++)
			//	{
			//		if (_genTexData.ImageHitTest(posX + _dirs_forBias[i].x, posY + _dirs_forBias[i].y))
			//		{
			//			//주변은 비어있지 않다.
			//			nCheckedBias++;
			//		}
			//	}
			//	if(nCheckedBias > NUM_BIAS_VALUE)
			//	{
			//		//주변이 꽉차있어서 이건 무시한다.
			//		return true;
			//	}
			//	return false;
			//} 
			#endregion

			#region [미사용 코드] 이 함수는 사용하지 않는다.
			///// <summary>
			///// 선분을 이용하여 이미지를 체크한다. 모든 포인트가 Filled일 때에만 True
			///// </summary>
			//public bool ImageHitTestLine_AllHitTrue(Vector2 posA, Vector2 posB, int nPoints)
			//{
			//	Vector2 curPos = Vector2.zero;
			//	for (int i = 1; i < nPoints + 1; i++)
			//	{
			//		float lerp = (float)i / (float)(nPoints + 1);
			//		curPos = (posA * lerp) + (posB * (1.0f - lerp));

			//		if (!_genTexData.ImageHitTest(curPos.x, curPos.y))
			//		{
			//			//비어있는 부분을 찾았다.
			//			return false;
			//		}
			//	}
			//	return true;
			//} 
			#endregion



			/// 선분을 이용하여 이미지를 체크한다. 하나라도 Filled일 때 True
			/// </summary>
			public bool ImageHitTestLine_AnyHitTrue(GenWrapGroup wrapGroup, Vector2 posA, Vector2 posB, int nPoints)
			{
				Vector2 curPos = Vector2.zero;

				for (int i = 1; i < nPoints + 1; i++)
				{
					float lerp = (float)i / (float)(nPoints + 1);
					curPos = (posA * lerp) + (posB * (1.0f - lerp));

					if(curPos.x < wrapGroup._rectArea._minX || wrapGroup._rectArea._maxX < curPos.x
						|| curPos.y < wrapGroup._rectArea._minY || wrapGroup._rectArea._maxY < curPos.y)
					{
						//영역 밖이다.
						//Hit 안한 것으로 인식한다.
						continue;
					}

					if (_genTexData.ImageHitTest(curPos.x, curPos.y))
					{
						//차있는 부분을 찾았다.
						return true;
					}
				}
				return false;
			}


			public bool ImageHitTestLine_AnyHitEveryPixels(GenWrapGroup wrapGroup, Vector2 posA, Vector2 posB)
			{
				int nPoints = (int)(Vector2.Distance(posA, posB) * 0.75f);
				Vector2 curPos = Vector2.zero;
				for (int i = 1; i < nPoints + 1; i++)
				{
					float lerp = (float)i / (float)(nPoints + 1);
					curPos = (posA * lerp) + (posB * (1.0f - lerp));

					if(curPos.x < wrapGroup._rectArea._minX || wrapGroup._rectArea._maxX < curPos.x
						|| curPos.y < wrapGroup._rectArea._minY || wrapGroup._rectArea._maxY < curPos.y)
					{
						//영역 밖이다.
						//Hit 안한 것으로 인식한다.
						continue;
					}

					if (_genTexData.ImageHitTest(curPos.x, curPos.y))
					{
						//차있는 부분을 찾았다.
						return true;
					}
				}
				return false;
			}



			#region [미사용 코드] 사용되지 않는 함수당.
			//public NormalDirResult ImageHitTestAndNormalVector(Vector2 pos)
			//{
			//	Vector2 sumDir = Vector2.zero;

			//	_normalCheckResult.Init();
			//	Vector2 curDir = Vector2.zero;
			//	bool isAllChecked = true;
			//	bool isAllEmpty = true;
			//	//체크하는 방법 : 짧게-길게 순으로 한바퀴 체크
			//	//같은 방향이라도 길이에 따라 벡터값이 계속 증가. (가중치 개념)

			//	bool isCheck = false;

			//	//각도별로 체크
			//	for (int iDir = 0; iDir < NUM_NORMAL_VECTOR; iDir++)
			//	{
			//		curDir = _normalDirs[iDir];

			//		//거리별로 체크
			//		for (int iRadius = 0; iRadius < NUM_CHECK_STEP; iRadius++)
			//		{
			//			isCheck = ImageHitTest(pos + (curDir * _checkRadius[iRadius]));

			//			if (!isCheck)
			//			{
			//				//빈 공간이라면 벡터 포함 (가중치 적용)
			//				sumDir += curDir * _checkRadiusWeights[iRadius];
			//				isAllChecked = false;
			//			}
			//			else
			//			{
			//				isAllEmpty = false;
			//			}
			//		}
			//	}

			//	//가운데도 체크
			//	_normalCheckResult.isCenterChecked = ImageHitTest(pos);

			//	if (isAllChecked || isAllEmpty)
			//	{
			//		//두번 검토해도 안되면 그냥 패스
			//		_normalCheckResult.isSuccess = false;
			//	}
			//	else
			//	{
			//		_normalCheckResult.isSuccess = true;
			//		_normalCheckResult.resultDir = sumDir.normalized;
			//	}

			//	return _normalCheckResult;
			//} 
			#endregion

			/// <summary>
			/// Margin거리 + Normal만큼 버텍스를 이동시킬 수 있는가
			/// </summary>
			/// <param name="pos"></param>
			/// <param name="dir"></param>
			/// <param name="margin"></param>
			/// <returns></returns>
			public bool IsVertMoveToMargin(Vector2 pos, Vector2 normal, float margin)
			{
				Vector2 targetPos = pos + normal * margin;
				if(targetPos.x < _area_Min.x || targetPos.x > _area_Max.x || targetPos.y < _area_Min.x || targetPos.y > _area_Max.y)
				{
					//밖으로 나가는 거라면 가능
					return true;
				}
				return !_genTexData.ImageHitTest(targetPos.x, targetPos.y);

			}

			/// <summary>
			/// 버텍스가 이미지로부터 Margin을 포함한 적절한 위치에 위치하는가. false인 경우는 바깥에 있는 경우
			/// </summary>
			/// <param name="vert"></param>
			/// <param name="margin"></param>
			/// <returns></returns>
			public bool IsVertIsInMargin(GenWrapGroup wrapGroup, GenVertex vert, float margin)
			{
				Vector2 posSrc = vert._pos;
				Vector2 posInner = vert._pos - (vert._dir * (margin + 3.0f));

				//안쪽에서부터 서서히 올라가면서 체크가 되면 오케이
				int nPoints = (int)((margin + 3.0f) * 0.7f);

				return ImageHitTestLine_AnyHitTrue(wrapGroup, posInner, posSrc, nPoints);
			}

			public bool IsVertIsInMargin(GenWrapGroup wrapGroup, Vector2 pos, Vector2 dir, float margin)
			{
				Vector2 posInner = pos - (dir * (margin + 3.0f));

				//안쪽에서부터 서서히 올라가면서 체크가 되면 오케이
				int nPoints = (int)((margin + 3.0f) * 0.7f) + 1;

				return ImageHitTestLine_AnyHitTrue(wrapGroup, posInner, pos, nPoints);
			}

			public class ImageHitResult
			{
				public bool isHit = false;
				public Vector2 pos = Vector2.zero;
				public float lerp = 0.0f;

				public ImageHitResult()
				{
					isHit = false;
					pos = Vector2.zero;
					lerp = 0.0f;
				}
			}

			/// <summary>
			/// 선을 긋고, 충돌한 지점을 가져온다.
			/// Mid에 가까운 지점이 리턴된다.
			/// </summary>
			/// <param name="posA"></param>
			/// <param name="posB"></param>
			/// <returns></returns>
			public ImageHitResult GetHitPosInLine(Vector2 posA, Vector2 posB)
			{
				int nPoints = (int)(Vector2.Distance(posA, posB) * 0.75f);
				if(nPoints < 6)
				{
					nPoints = 6;
				}
				Vector2 curPos = Vector2.zero;
				int iStart = 1;
				int iEnd = nPoints;
				int iMid = nPoints / 2;
				//1) Mid부터 End까지,
				
				for (int i = iMid; i <= iEnd; i++)
				{
					float lerp = (float)i / (float)(nPoints + 1);
					curPos = (posA * (1.0f - lerp)) + (posB * lerp);

					if (_genTexData.ImageHitTest(curPos.x, curPos.y))
					{
						//차있는 부분을 찾았다.
						_tmpHitResult.isHit = true;
						_tmpHitResult.pos = curPos;
						_tmpHitResult.lerp = lerp;
						return _tmpHitResult;
					}
				}

				//2) Mid부터 Start까지 역순서로 체크
				for (int i = iMid - 1; i >= iStart; i--)
				{
					float lerp = (float)i / (float)(nPoints + 1);
					curPos = (posA * (1.0f - lerp)) + (posB * lerp);

					if (_genTexData.ImageHitTest(curPos.x, curPos.y))
					{
						//차있는 부분을 찾았다.
						_tmpHitResult.isHit = true;
						_tmpHitResult.pos = curPos;
						_tmpHitResult.lerp = lerp;
						return _tmpHitResult;
					}
				}

				return null;
			}




			/// <summary>
			/// 역 Normal 방향으로 벡터를 그렸을때 가장 짧은 거리인 곳을 리턴
			/// 체크하는 간격은 아주 촘촘하지는 않다.
			/// </summary>
			/// <returns></returns>
			public ImageHitResult GetNearestNormalPosInLine(GenVertex vertA, GenVertex vertB, float maxLength, int nPoints)
			{
				Vector2 curPos = Vector2.zero;
				Vector2 curDir = Vector2.zero;
				Vector2 revNormalPos = Vector2.zero;

				int nLerp = nPoints + 2;

				int iNear = -1;
				float nearLerp = 0.0f;
				Vector2 nearPos = Vector2.zero;
				float minHitLength = 0.0f;

				for (int i = 1; i < nLerp + 1; i++)
				{
					float lerp = (float)i / (float)(nLerp + 1);
					curPos = (vertA._pos * (1.0f - lerp)) + (vertB._pos * lerp);
					curDir = ((vertA._dir * (1.0f - lerp)) + (vertB._dir * lerp)).normalized;

					float hitLength = 1.0f;
					while(true)
					{
						revNormalPos = curPos - (curDir * hitLength);
						if(_genTexData.ImageHitTest(revNormalPos.x, revNormalPos.y))
						{
							//Hit했다!
							if(iNear < 0 || hitLength < minHitLength)
							{
								//최소 거리 갱신
								iNear = i;
								nearLerp = lerp;
								nearPos = curPos;
								minHitLength = hitLength;
							}
							break;
						}

						//아니면
						hitLength += 1.5f;
					}
				}
				if(iNear < 0)
				{
					//성공한게 없다.
					return null;
				}

				_tmpHitResult.isHit = true;
				_tmpHitResult.lerp = nearLerp;
				_tmpHitResult.pos = nearPos;
				return _tmpHitResult;
				
			}

		}

		//처리 결과
		public class GenResult
		{
			//키값 (메시, WrapGroup별)
			public apMesh _mesh;
			public MESH_GEN_RESULT _resultType = MESH_GEN_RESULT.Processing;
			public string _errorMsg = null;

			public GenResult(apMesh mesh)
			{
				_mesh = mesh;
				_resultType = MESH_GEN_RESULT.Processing;
				_errorMsg = null;

				
			}

			public void SetComplete()
			{
				_resultType = MESH_GEN_RESULT.Success;
			}

			public void SetFailed(string errorMsg)
			{
				_resultType = MESH_GEN_RESULT.Failed;
				_errorMsg = errorMsg;
			}
		}
		

		//Group을 계산하기 위한 사각형 영역
		//Y축으로 파싱하면서 올라가기 때문에 비교는 X축 비교 위주로 한다.
		private class GenRectArea
		{
			public int _minX = 0;
			public int _minY = 0;
			public int _maxX = 0;
			public int _maxY = 0;

			public int _width = 0;
			public int _height = 0;
			public int _shortSize = 0;
			public int _longSize = 0;

			public GenRectArea(int minX, int maxX, int Y)
			{
				_minX = minX;
				_maxX = maxX;
				_minY = Y;
				_maxY = Y;

				_width = Mathf.Abs(_maxX - _minX);
				_height = Mathf.Abs(_maxY - _minY);
				_shortSize = Mathf.Min(_width, _height);
				_longSize = Mathf.Max(_width, _height);
			}

			public void UpdateLine(int lineMinX, int lineMaxX, int lineY)
			{
				_minX = Mathf.Min(_minX, lineMinX);
				_maxX = Mathf.Max(_maxX, lineMaxX);
				_maxY = lineY;

				_width = Mathf.Abs(_maxX - _minX);
				_height = Mathf.Abs(_maxY - _minY);
				_shortSize = Mathf.Min(_width, _height);
				_longSize = Mathf.Max(_width, _height);
			}

			public void Merge(GenRectArea rect)
			{
				_minX = Mathf.Min(_minX, rect._minX);
				_maxX = Mathf.Max(_maxX, rect._maxX);
				_minY = Mathf.Min(_minY, rect._minY);
				_maxY = Mathf.Max(_maxY, rect._maxY);

				_width = Mathf.Abs(_maxX - _minX);
				_height = Mathf.Abs(_maxY - _minY);
				_shortSize = Mathf.Min(_width, _height);
				_longSize = Mathf.Max(_width, _height);
			}


			//Rect간의 오버랩
			public bool IsOverapped(GenRectArea rect)
			{
				//겹치는지 여부
				//bias는 5픽셀
				if(_maxX < rect._minX - 5 || rect._maxX + 5 < _minX
					|| _maxY < rect._minY - 5 || rect._maxY + 5 < _minY)
				{
					//영역 밖이다.
					return false;
				}
				return true;

			}

			public bool IsAttachableLine(int lineMinX, int lineMaxX, int lineY)
			{
				//bias는 5픽셀
				if(_maxX < lineMinX - 5 || lineMaxX + 5 < _minX
					|| _maxY < lineY - 5 || lineY + 5 < _minY)
				{
					//너무 멀다
					return false;
				}
				return true;
			}
		}



		private class GenWrapGroup
		{
			public GenRectArea _rectArea = null;

			//외곽선과 버텍스
			public List<GenVertex> _vertices_Outline = null;
			public List<GenEdge> _edges_Outline = null;
			private GenEdgeGrid _grid_OutlineEdges = null;

			//내부 라인과 버텍스
			public List<GenOutVert2InnerLineVerts> _out2InLineVerts = null;
			public List<GenVertex> _vertices_Inline = null;
			public List<GenEdge> _edges_Out2Inline = null;
			public List<GenEdge> _edges_InterInlineVerts = null;

			public GenEdgeGrid _grid_lineEdgesForCrossCheck = null;

			//내부 가이드선
			public List<GenInnerGuidEdgeGroup> _innerGuides = null;
			public List<GenVertex> _vertices_Inner = null;
			public List<GenEdge> _edges_Inner = null;

			//크기 영역
			public float _areaSizeRatio = 1.0f;
			public float _areaShortSize = 1.0f;

			public GenWrapGroup(GenRectArea srcRectArea)
			{
				_rectArea = srcRectArea;
				_vertices_Outline = new List<GenVertex>();
				_edges_Outline = new List<GenEdge>();
				_out2InLineVerts = new List<GenOutVert2InnerLineVerts>();

				_grid_OutlineEdges = new GenEdgeGrid(30.0f);

				_vertices_Inline = new List<GenVertex>();
				_edges_Out2Inline = new List<GenEdge>();
				_edges_InterInlineVerts = new List<GenEdge>();
				_innerGuides = new List<GenInnerGuidEdgeGroup>();
				_vertices_Inner = new List<GenVertex>();
				_edges_Inner = new List<GenEdge>();

				
				

				int shortSize = _rectArea._shortSize;
				if (shortSize < 200)
				{
					_areaSizeRatio = 1.0f;
				}
				else
				{
					_areaSizeRatio = (float)shortSize / 200.0f;
				}
				//_areaSizeRatio = (float)shortSize / 200.0f;
				_areaShortSize = shortSize;

				//_grid_lineEdgesForCrossCheck = new GenEdgeGrid(30.0f);
				_grid_lineEdgesForCrossCheck = new GenEdgeGrid(Mathf.Max(_areaShortSize / 5.0f, 30.0f));
			}

			public void ClearVerticesAndEdges()
			{
				_vertices_Outline.Clear();
				_edges_Outline.Clear();
				_out2InLineVerts.Clear();

				_vertices_Inline.Clear();
				_edges_Out2Inline.Clear();
				_edges_InterInlineVerts.Clear();
				
				_innerGuides.Clear();
				_vertices_Inner.Clear();
				_edges_Inner.Clear();
			}


			


			public void CalculateNormals()
			{
				int nVerts = _vertices_Outline.Count;
				GenVertex vert = null;
				GenVertex prev = null;
				GenVertex next = null;
				
				Vector2 prev2Cur = Vector2.zero;
				Vector2 cur2Next = Vector2.zero;

				Vector2 vecSum = Vector2.zero;

				float resultAngle = 0.0f;

				for (int i = 0; i < nVerts; i++)
				{
					vert = _vertices_Outline[i];

					prev = vert._prev;
					next = vert._next;

					prev2Cur = (vert._pos - prev._pos).normalized;
					cur2Next = (next._pos - vert._pos).normalized;
					vecSum = prev2Cur + cur2Next;
				
					if(vecSum.sqrMagnitude == 0.0f)
					{
						resultAngle = 0.0f;
					}
					else
					{
						resultAngle = Mathf.Atan2(vecSum.y, vecSum.x) * Mathf.Rad2Deg;
						resultAngle -= 90.0f;
					}

					vert._angle = apUtil.AngleTo360(resultAngle);
					vert._dir = new Vector2(Mathf.Cos(resultAngle * Mathf.Deg2Rad), Mathf.Sin(resultAngle * Mathf.Deg2Rad));
				}
			}



			public void CalculateNormal(GenVertex targetVert)
			{
				GenVertex vert = null;
				GenVertex prev = null;
				GenVertex next = null;
				
				Vector2 prev2Cur = Vector2.zero;
				Vector2 cur2Next = Vector2.zero;

				Vector2 vecSum = Vector2.zero;

				float resultAngle = 0.0f;

				vert = targetVert;

				prev = vert._prev;
				next = vert._next;

				prev2Cur = (vert._pos - prev._pos).normalized;
				cur2Next = (next._pos - vert._pos).normalized;
				vecSum = prev2Cur + cur2Next;
				
				if(vecSum.sqrMagnitude == 0.0f)
				{
					resultAngle = 0.0f;
				}
				else
				{
					resultAngle = Mathf.Atan2(vecSum.y, vecSum.x) * Mathf.Rad2Deg;
					resultAngle -= 90.0f;
				}

				vert._angle = apUtil.AngleTo360(resultAngle);
				vert._dir = new Vector2(Mathf.Cos(resultAngle * Mathf.Deg2Rad), Mathf.Sin(resultAngle * Mathf.Deg2Rad));
			}

			//이건 정식 Normal은 아니고, 노멀이 기존에 비해서 너무 많이 바뀌는 것을 방지
			public void CalculateSmoothNormals()
			{
				Dictionary<GenVertex, float> prevNormals = new Dictionary<GenVertex, float>();
				Dictionary<GenVertex, float> nextNormals = new Dictionary<GenVertex, float>();

				int nVerts = _vertices_Outline.Count;
				GenVertex vert = null;
				GenVertex prev = null;
				GenVertex next = null;
				
				Vector2 prev2Cur = Vector2.zero;
				Vector2 cur2Next = Vector2.zero;

				Vector2 vecSum = Vector2.zero;

				float resultAngle = 0.0f;

				for (int i = 0; i < nVerts; i++)
				{
					vert = _vertices_Outline[i];
					prevNormals.Add(vert, vert._angle);

					prev = vert._prev;
					next = vert._next;

					prev2Cur = vert._pos - prev._pos;
					cur2Next = next._pos - vert._pos;
					vecSum = prev2Cur + cur2Next;
				
					if(vecSum.sqrMagnitude == 0.0f)
					{
						resultAngle = 0.0f;
					}
					else
					{
						resultAngle = Mathf.Atan2(vecSum.y, vecSum.x) * Mathf.Rad2Deg;
						resultAngle -= 90.0f;
					}

					nextNormals.Add(vert, apUtil.AngleTo360(resultAngle));
				}

				//이제 주변의 예상 각도를 반영하여 절충된 각도를 만들자
				for (int i = 0; i < nVerts; i++)
				{
					vert = _vertices_Outline[i];

					float curAngle = apUtil.AngleSlerp(prevNormals[vert], nextNormals[vert], 0.7f);
					float prevVert_Angle = apUtil.AngleSlerp(prevNormals[vert._prev], nextNormals[vert._prev], 0.5f);
					float nextVert_Angle = apUtil.AngleSlerp(prevNormals[vert._next], nextNormals[vert._next], 0.5f);
					float sideAngle = apUtil.AngleSlerp(prevVert_Angle, nextVert_Angle, 0.5f);
					float sumAngle = apUtil.AngleSlerp(curAngle, sideAngle, 0.2f);

					vert._angle = sumAngle;
					vert._dir = new Vector2(Mathf.Cos(sumAngle * Mathf.Deg2Rad), Mathf.Sin(sumAngle * Mathf.Deg2Rad));
				}
				
			}

			/// <summary>
			/// Normal의 역방향으로 이동하여 이미지에 Check할 때 까지 이동한다. margin을 계산한다. 1픽셀씩 이동
			/// </summary>
			public GenMoveResult MoveShrink_Parallel(GenRequest request, GenVertex vert, float margin, bool isCheckWithEdges)
			{
				Vector2 curPos = vert._pos;
				float curMoveLength = 0.0f;
				bool isChecked = false;
				while(true)
				{
					if(curMoveLength > _rectArea._shortSize * 0.5f)
					{
						//너무 많이 움직였다.
						//일단 절반만 가자
						float resultLength = _rectArea._shortSize * 0.5f;
						curPos = vert._pos + (-1 * resultLength * vert._dir);//축소이므로, 음수

						return GenMoveResult.I.SetFailed(curPos, resultLength);
					}
					curPos = vert._pos + (-1 * curMoveLength * vert._dir);//축소이므로, 음수

					//영역 바깥쪽이면 check가 안된거랑 동일하다.
					if(curPos.x < _rectArea._minX || _rectArea._maxX < curPos.x
						|| curPos.y < _rectArea._minY || _rectArea._maxY < curPos.y)
					{
						isChecked = false;
					}
					else
					{
						isChecked = request.ImageHitTest(curPos.x, curPos.y);
						if(!isChecked && isCheckWithEdges)
						{
							//사이드 선분도 계산하자
							if(vert._prev != null)
							{
								Vector2 sidePrevPos = vert._prev._pos + (-1 * curMoveLength * vert._dir);
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sidePrevPos))
								{
									isChecked = true;
								}
							}
							if(!isChecked && vert._next != null)
							{
								Vector2 sideNextPos = vert._next._pos + (-1 * curMoveLength * vert._dir);
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sideNextPos))
								{
									isChecked = true;
								}
							}
						}
					}

					if(isChecked)
					{
						//이미지를 발견했다.
						//margin만큼 뒤로 이동한다.
						float resultLength = Mathf.Max(curMoveLength - margin, 0.0f);
						curPos = vert._pos + (-1 * resultLength * vert._dir);//축소이므로, 음수
						return GenMoveResult.I.SetSuccess(curPos, resultLength);
					}
					else
					{
						//이미지를 발견하지 못했다.
						//더 이동하자
						curMoveLength += 0.8f;
					}
				}
				//return null;
			}


			public GenMoveResult MoveShrink(GenRequest request, GenVertex vert, float margin)
			{
				Vector2 curPos = vert._pos;
				float curMoveLength = 0.0f;
				bool isChecked = false;
				while(true)
				{
					if(curMoveLength > _rectArea._longSize * 0.5f)
					{
						//너무 많이 움직였다.
						//일단 절반만 가자
						float resultLength = _rectArea._longSize * 0.5f;
						curPos = vert._pos + (-1 * resultLength * vert._dir);//축소이므로, 음수

						return GenMoveResult.I.SetFailed(curPos, resultLength);
					}
					curPos = vert._pos + (-1 * curMoveLength * vert._dir);//축소이므로, 음수

					//영역 바깥쪽이면 check가 안된거랑 동일하다.
					if(curPos.x < _rectArea._minX || _rectArea._maxX < curPos.x
						|| curPos.y < _rectArea._minY || _rectArea._maxY < curPos.y)
					{
						isChecked = false;
					}
					else
					{
						isChecked = request.ImageHitTest(curPos.x, curPos.y);
						if(!isChecked)
						{
							//사이드 선분도 계산하자
							if(vert._prev != null)
							{
								//평행이 아니면 현재 위치와 비교
								Vector2 sidePrevPos = vert._prev._pos;
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sidePrevPos))
								{
									isChecked = true;
								}
							}
							if(!isChecked && vert._next != null)
							{
								//평행이 아니면 현재 위치와 비교
								Vector2 sideNextPos = vert._next._pos;
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sideNextPos))
								{
									isChecked = true;
								}
							}
						}
						if(!isChecked)
						{
							//추가 21.2.19 : 이미지 발견을 하지 않았는데, 다른 선분과 겹친 경우
							//다른 선분..이 생성되기 전이므로,
							GenVertex otherVert = null;
							for (int iOtherVert = 0; iOtherVert < _vertices_Outline.Count; iOtherVert++)
							{
								otherVert = _vertices_Outline[iOtherVert];
								if(otherVert == vert
									|| otherVert == vert._prev
									|| otherVert == vert._next)
								{
									//이웃한 버텍스면 패스
									continue;
								}

								//Vert-Prev간의 체크
								if(otherVert._prev != vert && otherVert._prev != vert._prev)
								{
									if(GenEdge.CheckCross(vert, vert._prev, otherVert, otherVert._prev)._result == GenEdgeCrossResult.RESULT_TYPE.Cross)
									{
										isChecked = true;
									}
								}
								if(!isChecked)
								{
									if (otherVert._next != vert && otherVert._next != vert._prev)
									{
										if (GenEdge.CheckCross(vert, vert._prev, otherVert, otherVert._next)._result == GenEdgeCrossResult.RESULT_TYPE.Cross)
										{
											isChecked = true;
										}
									}
								}
								if (!isChecked)
								{
									if (otherVert._prev != vert && otherVert._prev != vert._next)
									{
										if (GenEdge.CheckCross(vert, vert._next, otherVert, otherVert._prev)._result == GenEdgeCrossResult.RESULT_TYPE.Cross)
										{
											isChecked = true;
										}
									}
								}
								if(!isChecked)
								{
									if (otherVert._next != vert && otherVert._next != vert._next)
									{
										if (GenEdge.CheckCross(vert, vert._next, otherVert, otherVert._next)._result == GenEdgeCrossResult.RESULT_TYPE.Cross)
										{
											isChecked = true;
										}
									}
								}
							}
							
						}
					}

					if(isChecked)
					{
						//이미지를 발견했다.
						//margin만큼 뒤로 이동한다.
						float resultLength = Mathf.Max(curMoveLength - margin, 0.0f);
						curPos = vert._pos + (-1 * resultLength * vert._dir);//축소이므로, 음수
						return GenMoveResult.I.SetSuccess(curPos, resultLength);
					}
					else
					{
						//이미지를 발견하지 못했다.
						//더 이동하자
						curMoveLength += 1.5f;
					}
				}
				//return null;
			}


			/// <summary>
			/// Normal의 역방향으로 이동하여 이미지에 Check할 때 까지 이동한다. margin을 계산한다. 1픽셀씩 이동
			/// </summary>
			public GenMoveResult MoveShrink_VirtualMidPos(GenRequest request, GenVertex vert, float margin)
			{
				Vector2 curPos = vert._pos;
				float curMoveLength = 0.0f;
				bool isChecked = false;

				Vector2 midPosPrev = vert._prev._pos + curPos;
				Vector2 midPosNext = vert._next._pos + curPos;

				while(true)
				{
					if(curMoveLength > _rectArea._longSize * 0.5f)
					{
						//너무 많이 움직였다.
						//일단 절반만 가자
						float resultLength = _rectArea._longSize * 0.5f;
						curPos = vert._pos + (-1 * resultLength * vert._dir);//축소이므로, 음수

						return GenMoveResult.I.SetFailed(curPos, resultLength);
					}
					curPos = vert._pos + (-1 * curMoveLength * vert._dir);//축소이므로, 음수

					//영역 바깥쪽이면 check가 안된거랑 동일하다.
					if(curPos.x < _rectArea._minX || _rectArea._maxX < curPos.x
						|| curPos.y < _rectArea._minY || _rectArea._maxY < curPos.y)
					{
						isChecked = false;
					}
					else
					{
						isChecked = request.ImageHitTest(curPos.x, curPos.y);
						if(!isChecked)
						{
							//사이드 선분도 계산하자
							if(vert._prev != null)
							{
								//가상의 중점을 기준으로 이동하자
								Vector2 sidePrevPos = midPosPrev + (-1 * curMoveLength * vert._dir);
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sidePrevPos))
								{
									isChecked = true;
								}
							}
							if(!isChecked && vert._next != null)
							{
								Vector2 sideNextPos = midPosNext + (-1 * curMoveLength * vert._dir);
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sideNextPos))
								{
									isChecked = true;
								}
							}
						}
					}

					if(isChecked)
					{
						//이미지를 발견했다.
						//margin만큼 뒤로 이동한다.
						float resultLength = Mathf.Max(curMoveLength - margin, 0.0f);
						curPos = vert._pos + (-1 * resultLength * vert._dir);//축소이므로, 음수
						return GenMoveResult.I.SetSuccess(curPos, resultLength);
					}
					else
					{
						//이미지를 발견하지 못했다.
						//더 이동하자
						curMoveLength += 1.5f;
					}
				}
				//return null;
			}


			public GenMoveResult MoveExpand(GenRequest request, GenVertex vert, float margin)
			{
				Vector2 curPos = vert._pos;
				float curMoveLength = 0.0f;
				bool isChecked = false;
				while(true)
				{
					curPos = vert._pos + (curMoveLength * vert._dir);//확장
					//바깥으로 나갔다면 거기서 정지
					if(curMoveLength > _rectArea._longSize)
					{
						//너무 많이 움직였다.
						//취소
						break;
					}
					

					//영역 바깥쪽이면 check가 안된거랑 동일하다.
					if(curPos.x < _rectArea._minX || _rectArea._maxX < curPos.x
						|| curPos.y < _rectArea._minY || _rectArea._maxY < curPos.y)
					{
						float resultLength = curMoveLength + margin;
						curPos = vert._pos + (resultLength * vert._dir);//확장하여 저장
						return GenMoveResult.I.SetSuccess(curPos, resultLength);
					}
					else
					{
						isChecked = request.ImageHitTest(curPos.x, curPos.y);
						if(!isChecked)
						{
							//사이드 선분도 계산하자
							if(vert._prev != null)
							{
								//평행이 아니므로 이전 위치와 확인
								Vector2 sidePrevPos = vert._prev._pos;
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sidePrevPos))
								{
									isChecked = true;
								}
							}
							if(!isChecked && vert._next != null)
							{
								Vector2 sideNextPos = vert._next._pos;
								//(이동한) Prev - Cur 충돌 체크
								if(request.ImageHitTestLine_AnyHitEveryPixels(this, curPos, sideNextPos))
								{
									isChecked = true;
								}
							}
						}
					}

					if(!isChecked)
					{
						//빈 공간을 발견했다.
						//margin만큼 더 이동한다.
						float resultLength = curMoveLength + margin;
						curPos = vert._pos + (resultLength * vert._dir);//확장
						return GenMoveResult.I.SetSuccess(curPos, resultLength);
					}
					else
					{
						//이미지와 아직 겹쳐있다.
						//더 이동하자
						curMoveLength += 1.5f;
					}
				}
				return null;
			}

			//Link 정보를 토대로 위치를 다시 정렬한다.
			public void Sort()
			{
				if(_vertices_Outline.Count == 0)
				{
					return;
				}
				GenVertex startVert = _vertices_Outline[0];
				GenVertex curVert = startVert;
				int nVerts = _vertices_Outline.Count;

				_vertices_Outline.Clear();

				//버텍스를 순서대로 돌면서 추가
				int iVert = 0;
				while(true)
				{
					if(!_vertices_Outline.Contains(curVert))
					{
						_vertices_Outline.Add(curVert);
					}
					if(iVert >= nVerts)
					{
						break;
					}

					if(curVert._next == null ||
						curVert._next == startVert)
					{
						break;
					}

					curVert = curVert._next;
					iVert++;
				}
			}

			/// <summary>
			/// 빠른 외곽선 충돌 처리를 위해 외곽선을 Grid에 넣자
			/// </summary>
			public void StoreOutlineEdgesToGrid()
			{	
				for (int i = 0; i < _edges_Outline.Count; i++)
				{
					_grid_OutlineEdges.AddEdge(_edges_Outline[i]);
				}
			}


			//Out > Inner Vert를 위한 데이터들을 서로 연결한다. (아직 내부 버텍스는 만들기 직전)
			public void LinkOut2InnerVertData()
			{
				if(_out2InLineVerts == null || _out2InLineVerts.Count < 2)
				{
					return;
				}

				GenOutVert2InnerLineVerts curData = null;
				GenOutVert2InnerLineVerts nextData = null;
				GenVertex nextVert = null;

				//Cur > Next로 연결
				for (int iData = 0; iData < _out2InLineVerts.Count; iData++)
				{
					curData = _out2InLineVerts[iData];
					nextData = null;

					nextVert = curData.GetNextVert();

					if(nextVert != null)
					{
						nextData = _out2InLineVerts.Find(delegate(GenOutVert2InnerLineVerts a)
						{
							return a.IsOutVertContain(nextVert);
						});
					}
					if(nextData != null)
					{
						curData._nextData = nextData;
						nextData._prevData = curData;
					}

				}
			}
			public void StoreOutlineData()
			{
				_grid_lineEdgesForCrossCheck.Clear();
				//_edges_Outline > _grid_lineEdgesForCrossCheck도 추가
				for (int iOutLine = 0; iOutLine < _edges_Outline.Count; iOutLine++)
				{
					_grid_lineEdgesForCrossCheck.AddEdge(_edges_Outline[iOutLine]);
				}
			}


			//Out > InLine Data를 가져와서 일반 데이터로 저장한다.
			public void StoreVertexAndEdgeOfInlineData()
			{
				_vertices_Inline.Clear();
				_edges_Out2Inline.Clear();
				_edges_InterInlineVerts.Clear();
				

				GenOutVert2InnerLineVerts curData = null;
				GenEdge curEdge = null;
				for (int iData = 0; iData < _out2InLineVerts.Count; iData++)
				{
					curData = _out2InLineVerts[iData];

					if(!_vertices_Inline.Contains(curData._innerLineVert))
					{
						_vertices_Inline.Add(curData._innerLineVert);
					}
					
					for (int iEdge = 0; iEdge < curData._outVerts2InnerLineVertEdges.Count; iEdge++)
					{
						curEdge = curData._outVerts2InnerLineVertEdges[iEdge];
						_edges_Out2Inline.Add(curEdge);
						
						_grid_lineEdgesForCrossCheck.AddEdge(curEdge);
					}
					
					if(curData._innerLineEdge_ToNext != null)
					{
						_edges_InterInlineVerts.Add(curData._innerLineEdge_ToNext);
						_grid_lineEdgesForCrossCheck.AddEdge(curData._innerLineEdge_ToNext);
					}
				}

				
			}

			public void StoreVertexAndEdgeOfInnerGroup()
			{	
				_vertices_Inner.Clear();
				_edges_Inner.Clear();

				GenInnerGuidEdgeGroup curGuide = null;
				GenVertex curVert = null;
				GenEdge curEdge = null;
				for (int i = 0; i < _innerGuides.Count; i++)
				{
					curGuide = _innerGuides[i];
					for (int iVert = 0; iVert < curGuide._innerVerts.Count; iVert++)
					{
						curVert = curGuide._innerVerts[iVert];
						if(!_vertices_Inner.Contains(curVert))
						{
							_vertices_Inner.Add(curVert);
						}
					}
					for (int iEdge = 0; iEdge < curGuide._innerEdges.Count; iEdge++)
					{
						curEdge = curGuide._innerEdges[iEdge];
						_edges_Inner.Add(curEdge);
					}
				}
			}


			
			/// <summary>
			/// 해당 점을 잇는 선분이 Outline과 교차하는가
			/// </summary>
			public bool IsOutlineCross(Vector2 posA, Vector2 posB)
			{	
				if(Vector2.SqrMagnitude(posA - posB) < 1.0f)
				{
					return false;
				}
				List<GenEdge> outlineEdges = _grid_OutlineEdges.GetEdges(posA, posB);
				if(outlineEdges == null || outlineEdges.Count == 0)
				{
					//근처에 선분이 없다.
					//Debug.Log("IsOutlineCross > No Edges");
					return false;
				}

				GenEdge curEdge = null;
				int nEdges = outlineEdges.Count;

				for (int i = 0; i < nEdges; i++)
				{
					curEdge = outlineEdges[i];
					if(GenEdge.CheckCross(posA, posB, curEdge._vert_A._pos, curEdge._vert_B._pos)._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
					{
						//충돌했다.
						//Debug.Log("IsOutlineCross > Hit");
						return true;
					}
				}

				//Debug.Log("IsOutlineCross > No-Hit");
				return false;
			}


			public bool IsCrossWithInLines(GenVertex startVert, Vector2 posA, Vector2 posB)
			{	
				List<GenEdge> crossableEdges = _grid_lineEdgesForCrossCheck.GetEdges(posA, posB);
				int nInLineEdges = crossableEdges.Count;

				GenEdge checkEdge = null;

				for (int iEdge = 0; iEdge < nInLineEdges; iEdge++)
				{
					checkEdge = crossableEdges[iEdge];

					//현재 버텍스와 연결된거면 각도만 확인한다.
					if(checkEdge._vert_A == startVert
						|| checkEdge._vert_B == startVert)
					{
						Vector2 edgeDir = Vector2.zero;
						Vector2 edgeEndPos = Vector2.zero;
						if(checkEdge._vert_A == startVert)
						{
							edgeDir = checkEdge._vert_B._pos - checkEdge._vert_A._pos;
							edgeEndPos = checkEdge._vert_B._pos;
						}
						else
						{
							edgeDir = checkEdge._vert_A._pos - checkEdge._vert_B._pos;
							edgeEndPos = checkEdge._vert_A._pos;
						}
						if(Vector2.Angle(edgeDir, posB - posA) < 1.5f
							|| Vector2.Distance(posB, edgeEndPos) < 2.0f)
						{
							//아주 가까운 라인이다.
							//라인이 겹침
							return true;
						}
						//그렇지 않으면 패스
						continue;
					}

					GenEdgeCrossResult result = GenEdge.CheckCrossApprox(posA, posB, checkEdge._vert_A._pos, checkEdge._vert_B._pos);
					if(result._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
					{
						//교차됨
						return true;
					}
				}
				return false;
			}

			public bool IsCrossWithInLines(GenVertex vertA, GenVertex vertB, bool isAccurate = false)
			{	
				List<GenEdge> crossableEdges = _grid_lineEdgesForCrossCheck.GetEdges(vertA._pos, vertB._pos);
				int nInLineEdges = crossableEdges.Count;

				GenEdge checkEdge = null;

				float samelineAngleBias = isAccurate ? 1.0f : 1.5f;
				float samelinePointPosBias = isAccurate ? 1.0f : 2.0f;


				for (int iEdge = 0; iEdge < nInLineEdges; iEdge++)
				{
					checkEdge = crossableEdges[iEdge];

					//현재 버텍스와 연결된거면 라인이 겹치는지 확인해야한다.
					
					if(checkEdge._vert_A == vertA
						|| checkEdge._vert_A == vertB
						|| checkEdge._vert_B == vertA
						|| checkEdge._vert_B == vertB
						)
					{
						//vertA, B와 대응되는 Pos를 발견하자
						Vector2 edgePosA = Vector2.zero;
						Vector2 edgePosB = Vector2.zero;
						Vector2 edgeDir = Vector2.zero;

						if(checkEdge._vert_A == vertA
							|| checkEdge._vert_B == vertB)
						{
							//정방향
							edgePosA = checkEdge._vert_A._pos;
							edgePosB = checkEdge._vert_B._pos;
						}
						else
						{
							//역방향
							edgePosA = checkEdge._vert_B._pos;
							edgePosB = checkEdge._vert_A._pos;
						}
						edgeDir = edgePosB - edgePosA;

						//방향이 유사하면 겹침
						if(Vector2.Angle(edgeDir, vertB._pos - vertA._pos) < samelineAngleBias)
						{
							return true;
						}
						//EndPos가 거의 유사하면 겹침
						if(checkEdge._vert_A == vertA
							|| checkEdge._vert_B == vertA)
						{
							//PosA와 겹칠때
							if(Vector2.Distance(edgePosB, vertB._pos) < samelinePointPosBias)
							{
								return true;
							}
						}
						else
						{
							//PosB와 겹칠때
							if(Vector2.Distance(edgePosA, vertA._pos) < samelinePointPosBias)
							{
								return true;
							}
						}

						//그렇지 않으면 패스
						continue;	
					}

					GenEdgeCrossResult result = GenEdge.CheckCrossApprox(
													vertA._pos, 
													vertB._pos, 
													checkEdge._vert_A._pos, 
													checkEdge._vert_B._pos,
													isAccurate);
					if(result._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
					{
						//교차됨
						return true;
					}
				}
				return false;
			}



			/// <summary>
			/// 점 하나와 연결된 선분들이 다른 선분들과 교차되는지 테스트.
			/// </summary>
			/// <param name="sharedVert"></param>
			/// <param name="simulatePos"></param>
			/// <param name="linkedEdges"></param>
			/// <returns></returns>
			public bool IsCrossWithInLines_MultiLineSimulate(GenVertex sharedVert, Vector2 simulatePos, List<GenEdge> linkedEdges)
			{	
				//일단 모든 점들의 Min-Max를 정하자
				Vector2 minPos = simulatePos;
				Vector2 maxPos = simulatePos;
				int nLinkedEdges = linkedEdges.Count;

				GenEdge linkedEdge = null;
				for (int iEdge = 0; iEdge < nLinkedEdges; iEdge++)
				{
					linkedEdge = linkedEdges[iEdge];
					minPos.x = Mathf.Min(minPos.x, linkedEdge._vert_A._pos.x);
					minPos.x = Mathf.Min(minPos.x, linkedEdge._vert_B._pos.x);
					minPos.y = Mathf.Min(minPos.y, linkedEdge._vert_A._pos.y);
					minPos.y = Mathf.Min(minPos.y, linkedEdge._vert_B._pos.y);
					
					maxPos.x = Mathf.Max(maxPos.x, linkedEdge._vert_A._pos.x);
					maxPos.x = Mathf.Max(maxPos.x, linkedEdge._vert_B._pos.x);
					maxPos.y = Mathf.Max(maxPos.y, linkedEdge._vert_A._pos.y);
					maxPos.y = Mathf.Max(maxPos.y, linkedEdge._vert_B._pos.y);
				}

				//전체 범위로 Edge를 가져오자
				List<GenEdge> crossableEdges = _grid_lineEdgesForCrossCheck.GetEdges(minPos, maxPos);
				int nInLineEdges = crossableEdges.Count;

				GenEdge checkEdge = null;

				GenVertex linkedVert = null;
				
				for (int iLinkedEdge = 0; iLinkedEdge < nLinkedEdges; iLinkedEdge++)
				{
					linkedEdge = linkedEdges[iLinkedEdge];
					if (linkedEdge._vert_A == sharedVert)
					{
						linkedVert = linkedEdge._vert_B;
					}
					else if (linkedEdge._vert_B == sharedVert)
					{
						linkedVert = linkedEdge._vert_A;
					}
					else
					{
						//??
						continue;
					}

					//Edge별로 하나씩 모두 검사하자
					for (int iEdge = 0; iEdge < nInLineEdges; iEdge++)
					{
						checkEdge = crossableEdges[iEdge];
						if(linkedEdges.Contains(checkEdge))
						{
							//동일한거면 패스
							continue;
						}

						//현재 버텍스와 연결된거면 라인이 겹치는지 확인해야한다.

						if (checkEdge._vert_A == sharedVert
							|| checkEdge._vert_A == linkedVert
							|| checkEdge._vert_B == sharedVert
							|| checkEdge._vert_B == linkedVert
							)
						{
							//vertA, B와 대응되는 Pos를 발견하자
							Vector2 edgePosA = Vector2.zero;
							Vector2 edgePosB = Vector2.zero;
							Vector2 edgeDir = Vector2.zero;

							if (checkEdge._vert_A == sharedVert
								|| checkEdge._vert_B == linkedVert)
							{
								//정방향
								edgePosA = checkEdge._vert_A._pos;
								edgePosB = checkEdge._vert_B._pos;
							}
							else
							{
								//역방향
								edgePosA = checkEdge._vert_B._pos;
								edgePosB = checkEdge._vert_A._pos;
							}
							edgeDir = edgePosB - edgePosA;

							//방향이 유사하면 겹침
							if (Vector2.Angle(edgeDir, linkedVert._pos - simulatePos) < 1.5f)
							{
								return true;
							}
							//EndPos가 거의 유사하면 겹침
							if (checkEdge._vert_A == sharedVert
								|| checkEdge._vert_B == sharedVert)
							{
								//PosA와 겹칠때
								if (Vector2.Distance(edgePosB, linkedVert._pos) < 2.0f)
								{
									return true;
								}
							}
							else
							{
								//PosB와 겹칠때
								if (Vector2.Distance(edgePosA, simulatePos) < 2.0f)
								{
									return true;
								}
							}

							//그렇지 않으면 패스
							continue;
						}

						GenEdgeCrossResult result = GenEdge.CheckCrossApprox(
														simulatePos,
														linkedVert._pos,
														checkEdge._vert_A._pos,
														checkEdge._vert_B._pos,
														true);//정확도 옵션
						if (result._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
						{
							//교차됨
							return true;
						}
					}
				}
				
				return false;
			}



			public bool IsCrossWithSimulate(Vector2 posA, Vector2 posB)
			{	
				List<GenEdge> crossableEdges = _grid_lineEdgesForCrossCheck.GetEdges(posA, posB);
				int nInLineEdges = crossableEdges.Count;

				GenEdge checkEdge = null;

				for (int iEdge = 0; iEdge < nInLineEdges; iEdge++)
				{
					checkEdge = crossableEdges[iEdge];

					//현재 버텍스와 연결된거면 각도만 확인한다.
					GenEdgeCrossResult result = GenEdge.CheckCrossApprox(posA, posB, checkEdge._vert_A._pos, checkEdge._vert_B._pos);
					if(result._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
					{
						//교차됨
						return true;
					}
				}
				return false;
			}
			
		}



		
		

		private class GenMoveResult
		{
			public Vector2 _resultPos;
			public float _moveLength;
			public bool _isSuccess = false;

			private static GenMoveResult _instance = new GenMoveResult();
			public static GenMoveResult I { get { return _instance; } }
			public GenMoveResult()
			{

			}
			public void Clear()
			{
				_resultPos = Vector2.zero;
				_moveLength = 0.0f;
				_isSuccess = false;
			}
			public GenMoveResult SetSuccess(Vector2 resultPos, float moveLength)
			{
				_resultPos = resultPos;
				_moveLength = moveLength;
				_isSuccess = true;
				return this;
			}
			/// <summary>
			/// 실패했지만 약간 이동해야할 때
			/// </summary>
			/// <param name="resultPos"></param>
			/// <param name="moveLength"></param>
			/// <returns></returns>
			public GenMoveResult SetFailed(Vector2 resultPos, float moveLength)
			{
				_resultPos = resultPos;
				_moveLength = moveLength;
				_isSuccess = false;
				return this;
			}
		}









		/// <summary>
		/// WrapGroup에 속하는 메타 데이터
		/// </summary>
		private class GenOutVert2InnerLineVerts
		{
			public List<GenVertex> _outVerts = null;
			public List<GenEdge> _outVerts2InnerLineVertEdges = null;
			public GenVertex _innerLineVert = null;

			public GenOutVert2InnerLineVerts _prevData = null;
			public GenOutVert2InnerLineVerts _nextData = null;

			public GenEdge _innerLineEdge_ToPrev = null;
			public GenEdge _innerLineEdge_ToNext = null;

			//빠른 검색 위한 범위
			public Vector2 _areaMin = Vector2.zero;
			public Vector2 _areaMax = Vector2.zero;

			////병합이 되었다. 더이상 병합되지는 않는다.
			//public bool _isMerged = false;
			////축소가 되었다. 이건 더이상 축소되지 않는다. 다음엔 무조건 병합
			public bool _isContracted = false;
			public float _remainContractedLength = 0.0f;
			public Vector2 _orgInLinePos = Vector2.zero;

			//추가 21.2.19 : 만약 InnerMargin이 없어서 InlineVert가 없는 경우에 true
			public bool _isOnlyOutVerts = false;



			public GenOutVert2InnerLineVerts()
			{
				_outVerts = new List<GenVertex>();
				_outVerts2InnerLineVertEdges = new List<GenEdge>();
				_innerLineVert = null;

				_prevData = null;
				_nextData = null;

				_innerLineEdge_ToPrev = null;
				_innerLineEdge_ToNext = null;

				//_isMerged = false;
				_isContracted = false;
				_orgInLinePos = Vector2.zero;

				_isOnlyOutVerts = false;
			}

			public void SetOutVertices(List<GenVertex> outVerts)
			{
				for (int i = 0; i < outVerts.Count; i++)
				{
					_outVerts.Add(outVerts[i]);
				}
			}

			public GenVertex GetPrevVert()
			{
				if(_outVerts == null || _outVerts.Count == 0) { return null; }
				return _outVerts[0]._prev;
			}

			public GenVertex GetNextVert()
			{
				if(_outVerts == null || _outVerts.Count == 0) { return null; }
				return _outVerts[_outVerts.Count - 1]._next;
			}

			public bool IsOutVertContain(GenVertex vert)
			{
				return _outVerts.Contains(vert);
			}

			public void MakeMinMaxArea()
			{
				if(_innerLineVert == null || _outVerts == null)
				{
					return;
				}
				_areaMin = _innerLineVert._pos;
				_areaMax = _innerLineVert._pos;

				GenVertex curVert = null;
				for (int i = 0; i < _outVerts.Count; i++)
				{
					curVert = _outVerts[i];
					_areaMin.x = Mathf.Min(_areaMin.x, curVert._pos.x);
					_areaMin.y = Mathf.Min(_areaMin.y, curVert._pos.y);
					_areaMax.x = Mathf.Max(_areaMax.x, curVert._pos.x);
					_areaMax.y = Mathf.Max(_areaMax.y, curVert._pos.y);
				}

				if(_innerLineEdge_ToPrev != null)
				{
					GenVertex vertA = _innerLineEdge_ToPrev._vert_A;
					GenVertex vertB = _innerLineEdge_ToPrev._vert_B;
					
					_areaMin.x = Mathf.Min(_areaMin.x, vertA._pos.x);
					_areaMin.y = Mathf.Min(_areaMin.y, vertA._pos.y);
					_areaMax.x = Mathf.Max(_areaMax.x, vertA._pos.x);
					_areaMax.y = Mathf.Max(_areaMax.y, vertA._pos.y);

					_areaMin.x = Mathf.Min(_areaMin.x, vertB._pos.x);
					_areaMin.y = Mathf.Min(_areaMin.y, vertB._pos.y);
					_areaMax.x = Mathf.Max(_areaMax.x, vertB._pos.x);
					_areaMax.y = Mathf.Max(_areaMax.y, vertB._pos.y);
				}

				if(_innerLineEdge_ToNext != null)
				{
					GenVertex vertA = _innerLineEdge_ToNext._vert_A;
					GenVertex vertB = _innerLineEdge_ToNext._vert_B;
					
					_areaMin.x = Mathf.Min(_areaMin.x, vertA._pos.x);
					_areaMin.y = Mathf.Min(_areaMin.y, vertA._pos.y);
					_areaMax.x = Mathf.Max(_areaMax.x, vertA._pos.x);
					_areaMax.y = Mathf.Max(_areaMax.y, vertA._pos.y);

					_areaMin.x = Mathf.Min(_areaMin.x, vertB._pos.x);
					_areaMin.y = Mathf.Min(_areaMin.y, vertB._pos.y);
					_areaMax.x = Mathf.Max(_areaMax.x, vertB._pos.x);
					_areaMax.y = Mathf.Max(_areaMax.y, vertB._pos.y);
				}
			}

			public bool IsCross(GenOutVert2InnerLineVerts otherData)
			{
				if(_innerLineVert == null || otherData._innerLineVert == null)
				{
					return false;
				}
				//Edge들 간에 비교를 한다.
				//만약 이웃한 otherData라면, Prev-Next 라인은 비교하지 않는다.
				//bool isNearData = _nextData == otherData || _prevData == otherData;

				//모든 Edge를 체크할 필요는 없고
				//1. Out [0] -> InnerVert
				//2. Out [Last] -> InnerVert
				//3. Prev
				//4. Next
				//를 비교한다.

				//일단 영역이 겹쳐야 한다.
				if (otherData._areaMax.x < _areaMin.x || _areaMax.x < otherData._areaMin.x
					|| otherData._areaMax.y < _areaMin.y || _areaMax.y < otherData._areaMin.y)
				{
					//영역이 겹치지 않는다.
					return false;
				}


				//Debug.LogWarning("--- 교차 체크 ---");
				

				GenEdge curEdge_1 = _outVerts2InnerLineVertEdges[0];
				GenEdge curEdge_2 = _outVerts2InnerLineVertEdges[_outVerts2InnerLineVertEdges.Count - 1];
				GenEdge curEdge_3 = _innerLineEdge_ToPrev;
				GenEdge curEdge_4 = _innerLineEdge_ToNext;

				
				
				//체크 가능한 Edge와, 이 Edge들에 포함된 Vert를 모으자
				List<GenVertex> curCheckEdgeVerts = new List<GenVertex>();
				List<GenEdge> curEdges = new List<GenEdge>();

				curEdges.Add(curEdge_1);
				if (!curEdges.Contains(curEdge_2)) { curEdges.Add(curEdge_2); }
				if (!curEdges.Contains(curEdge_3)) { curEdges.Add(curEdge_3); }
				if (!curEdges.Contains(curEdge_4)) { curEdges.Add(curEdge_4); }

				GenEdge edge = null;
				for (int i = 0; i < curEdges.Count; i++)
				{
					edge = curEdges[i];
					if(!curCheckEdgeVerts.Contains(edge._vert_A))
					{
						curCheckEdgeVerts.Add(edge._vert_A);
					}
					if(!curCheckEdgeVerts.Contains(edge._vert_B))
					{
						curCheckEdgeVerts.Add(edge._vert_B);
					}
				}

				GenEdge otherEdge_1 = otherData._outVerts2InnerLineVertEdges[0];
				GenEdge otherEdge_2 = otherData._outVerts2InnerLineVertEdges[otherData._outVerts2InnerLineVertEdges.Count - 1];
				GenEdge otherEdge_3 = otherData._innerLineEdge_ToPrev;
				GenEdge otherEdge_4 = otherData._innerLineEdge_ToNext;

				//이제 체크 가능한 상대 Edge를 찾자
				//버텍스를 공유하면 안된다.
				List<GenEdge> otherEdges = new List<GenEdge>();
				if(!curCheckEdgeVerts.Contains(otherEdge_1._vert_A) 
					&& !curCheckEdgeVerts.Contains(otherEdge_1._vert_B))
				{
					otherEdges.Add(otherEdge_1);
				}
				if(!otherEdges.Contains(otherEdge_2)
					&&!curCheckEdgeVerts.Contains(otherEdge_2._vert_A) 
					&& !curCheckEdgeVerts.Contains(otherEdge_2._vert_B))
				{
					otherEdges.Add(otherEdge_2);
				}
				if(!otherEdges.Contains(otherEdge_3)
					&&!curCheckEdgeVerts.Contains(otherEdge_3._vert_A) 
					&& !curCheckEdgeVerts.Contains(otherEdge_3._vert_B))
				{
					otherEdges.Add(otherEdge_3);
				}
				if(!otherEdges.Contains(otherEdge_4)
					&&!curCheckEdgeVerts.Contains(otherEdge_4._vert_A) 
					&& !curCheckEdgeVerts.Contains(otherEdge_4._vert_B))
				{
					otherEdges.Add(otherEdge_4);
				}
				

				if(curEdges.Count == 0 || otherEdges.Count == 0)
				{
					//체크할게 없다.
					return false;
				}

				//체크하자
				GenEdge otherEdge = null;
				for (int iCur = 0; iCur < curEdges.Count; iCur++)
				{
					edge = curEdges[iCur];
					for (int iOther = 0; iOther < otherEdges.Count; iOther++)
					{
						otherEdge = otherEdges[iOther];
						if(GenEdge.CheckCrossApprox(
							edge._vert_A._pos, 
							edge._vert_B._pos, 
							otherEdge._vert_A._pos,
							otherEdge._vert_B._pos)._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
						{
							//겹치넹
							return true;
						}
					}
				}
				return false;				
			}
		}

		private class GenInnerGuidEdgeGroup
		{
			public GenVertex _startVert;
			public GenVertex _endVert;

			public List<GenVertex> _innerVerts = null;
			public List<GenEdge> _innerEdges = null;

			public GenInnerGuidEdgeGroup(GenVertex vertStart, GenVertex vertEnd)
			{
				_startVert = vertStart;
				_endVert = vertEnd;

				_innerVerts = new List<GenVertex>();
				_innerEdges = new List<GenEdge>();

				////바로 Edge하나 추가한다.
				//GenEdge edge = new GenEdge(_startVert, _endVert);
				//_innerEdges.Add(edge);
			}

			public void AddInnerVertex(GenVertex vert)
			{
				if(!_innerVerts.Contains(vert))
				{
					_innerVerts.Add(vert);
				}
			}
			public void AddEdge(GenEdge edge)
			{
				if(!_innerEdges.Contains(edge))
				{
					_innerEdges.Add(edge);
				}
			}
			
			public void RemoveEdge(GenEdge edge)
			{
				_innerEdges.Remove(edge);
			}
		}


		#region [미사용 코드]

		//private class GenInLineRayGrid
		//{
		//	public Dictionary<int, Dictionary<int, Dictionary<int, GenVertex>>> _grid = null;
		//	public List<GenVertex> _vertices = null;
		//	public float _gridSize = 0.0f;
		//	public float _angleSize = 0.0f;

		//	public GenInLineRayGrid(float gridSize, float ignoreAngle)
		//	{
		//		_gridSize = gridSize;
		//		_angleSize = ignoreAngle;

		//		_grid = new Dictionary<int, Dictionary<int, Dictionary<int, GenVertex>>>();
		//		_vertices = new List<GenVertex>();
		//	}

		//	public void AddInLineVert(GenVertex inLineVert)
		//	{
		//		int iX = (int)(inLineVert._pos.x / _gridSize);
		//		int iY = (int)(inLineVert._pos.y / _gridSize);
		//		int iAngle = (int)apUtil.AngleTo360(inLineVert._angle / _angleSize);

		//		//이미 있으면 추가하지 않는다.
		//		if(_grid.ContainsKey(iX))
		//		{
		//			if(_grid[iX].ContainsKey(iY))
		//			{
		//				if(_grid[iX][iY].ContainsKey(iAngle))
		//				{
		//					return;
		//				}
		//			}
		//		}

		//		if(!_grid.ContainsKey(iX))
		//		{
		//			_grid.Add(iX, new Dictionary<int, Dictionary<int, GenVertex>>());
		//		}
		//		if(!_grid[iX].ContainsKey(iY))
		//		{
		//			_grid[iX].Add(iY, new Dictionary<int, GenVertex>());
		//		}
		//		_grid[iX][iY].Add(iAngle, inLineVert);
		//		_vertices.Add(inLineVert);
		//	}

		//}

		#endregion



		#region [미사용 코드] 버텍스 타입은 의미가 없었당 ㅎ

		//public enum VERT_TYPE
		//{
		//	/// <summary>Ray의 결과로 생성된 것으로, 여백에 위치한다.</summary>
		//	Outline,
		//	/// <summary>Ray의 결과로 생성된 것으로, 내부에 위치한다. 외곽선 바로 밑에 존재한다.</summary>
		//	Inline,

		//	/// <summary>내부에 생성되었다.</summary>
		//	Inner,
		//	/// <summary>메시 생성을 위한 무한 위치의 버텍스</summary>
		//	Infinity
		//} 
		#endregion

		/// <summary>
		/// 버텍스로 생성되기 전의 메타 버텍스 객체
		/// GenRay에서 생성된 것도 있고, 내부에 생성된 것도 있다. 타입을 확인하자.
		/// </summary>
		private class GenVertex
		{
			//public GenCircleArea _parentCircle = null;

			public Vector2 _pos;
			//public VERT_TYPE _vertType = VERT_TYPE.Inner;

			//연결 정보, 방향 등을 더 정의하자
			//Ray로 부터 생성되었다면
			//public GenRay _parentRay = null;
			public float _angle = 0.0f;//생성 당시의 방향
			public Vector2 _dir = Vector2.zero;

			public int _groupID = -1;

			//public GenVertex _rayPairVert = null;//Ray로 부터 생성될 때의 짝이된 버텍스

			//Outer일때 Normal 방향으로 확장해야하는 경우, 그걸 바로 하지말고 딜레이를 주자
			//public Vector2 _expandVector = Vector2.zero;//확장 벡터(단위 벡터 아니다)
			//public Vector2 _pos_BeforeExpand = Vector2.zero;
			//public bool _isExpandable = false;

			//Outline인 경우
			//public int _nLinkedEdges = 0;
			//public GenVertex _outline_Left = null;
			//public GenVertex _outline_Right = null;

			//public GenLinkedVertGroup _linkedVertGroup = null;
			public GenVertex _prev = null;//Linked Group에서의 연결 정보
			public GenVertex _next = null;

			public int _ID = 0;
			public static int s_uniqueID = 0;


			public bool _isSuccess = false;
			
			//public GenVertex(	GenCircleArea parentCircle,
			//					Vector2 pos,
			//					VERT_TYPE vertType,
			//					int groupID)
			//{
			//	_parentCircle = parentCircle;
			//	_pos = pos;
			//	_vertType = vertType;
			//	//_nLinkedEdges = 0;
			//	//_outline_Left = null;
			//	//_outline_Right = null;

			//	_linkedVertGroup = null;
			//	_prev = null;
			//	_next = null;

			//	_groupID = groupID;

			//	_ID = s_uniqueID;
			//	s_uniqueID++;
			//}
			public List<GenEdge> _linkedEdges = null;


			//그냥 생성할 때
			public GenVertex(Vector2 pos
				//, VERT_TYPE vertType
				)
			{
				//_parentCircle = null;
				_pos = pos;
				//_vertType = vertType;
				//_nLinkedEdges = 0;
				//_outline_Left = null;
				//_outline_Right = null;
				_groupID = -1;

				_ID = s_uniqueID;
				s_uniqueID++;

				_isSuccess = false;

				_linkedEdges = new List<GenEdge>();
			}


			//public void SetRayVert(GenRay ray
			//	//, GenVertex pairVert
			//	)
			//{
			//	_parentRay = ray;

			//	//이건 Ray에 대한 Dir
			//	//Circle 자체의 누적 Dir도 감안하자


			//	//_angle = ray._angle;
			//	//_dir = ray._dir;

			//	if (ray._circle._parent == null)
			//	{
			//		//루트라면 > Ray만 이용
			//		_angle = ray._angle;
			//		_dir = ray._dir;
			//	}
			//	else
			//	{
			//		//Child라면 누적 방향도 보간
			//		_angle = (ray._angle * 0.5f) + (ray._circle._angleFromRoot * 0.5f);
			//		_dir = (ray._dir + ray._circle._direction_FromRoot).normalized;
			//	}


			//	//_rayPairVert = pairVert;
			//}
		}

		#region [미사용 코드]

		///// <summary>
		///// 버텍스간의 연결을 위한 메타 정보
		///// </summary>
		//private class GenVertLinkInfo
		//{
		//	public GenVertex _srcVert = null;
		//	public GenVertex _dstVert = null;
		//	public float _dist = 0.0f;
		//	public float _dist_Weighted = 0.0f;
		//	public int _sortedIndex =  0;
		//	public bool _isImageChecked = false;//이게 True면 이미지가 충돌된 상태이다.

		//	public GenVertLinkInfo(GenVertex srcVert, GenVertex dstVert, float dist, float dist_Weighted, bool isImageChecked)
		//	{
		//		_srcVert = srcVert;
		//		_dstVert = dstVert;
		//		_dist = dist;
		//		_dist_Weighted = dist_Weighted;
		//		_sortedIndex = 0;
		//		_isImageChecked = isImageChecked;
		//	}
		//} 
		#endregion

		#region [미사용 코드] Circle/Linear 방식

		///// <summary>
		///// Edge를 만들기 위한 버텍스 그룹.
		///// 시작과 끝이 있으며, 마지막에 시작과 끝이 연결되어야 루프가 완성된다.
		///// </summary>		
		//private class GenLinkedVertGroup
		//{
		//	public List<GenVertex> _vertices = null;//순서와는 별개이다.
		//	public GenVertex _startVert = null;
		//	public GenVertex _endVert = null;
		//	public bool _isCompleted = false;

		//	public int NumList
		//	{
		//		get
		//		{
		//			return _vertices.Count;
		//		}
		//	}

		//	public GenLinkedVertGroup(GenVertex startVert)
		//	{
		//		_vertices = new List<GenVertex>();

		//		startVert._linkedVertGroup = this;
		//		startVert._prev = null;
		//		startVert._next = null;


		//		_startVert = startVert;
		//		_endVert = startVert;
		//		_vertices.Add(startVert);

		//		//Debug.Log("GenLinkedVertGroup 생성 [" + startVert._ID + "]");

		//		_isCompleted = false;
		//	}

		//	public GenLinkedVertGroup(GenVertex startVert, GenVertex endVert)
		//	{
		//		_vertices = new List<GenVertex>();

		//		startVert._linkedVertGroup = this;
		//		startVert._prev = null;
		//		startVert._next = endVert;

		//		endVert._linkedVertGroup = this;
		//		endVert._prev = startVert;
		//		endVert._next = null;


		//		_startVert = startVert;
		//		_endVert = endVert;
		//		_vertices.Add(startVert);
		//		_vertices.Add(endVert);

		//		//Debug.Log("GenLinkedVertGroup 생성 [" + startVert._ID + "]");

		//		_isCompleted = false;
		//	}

		//	/// <summary>
		//	/// 버텍스를 기존의 버텍스 A, B 사이에 넣는다.
		//	/// 만약 시작 또는 끝 옆에 붙인다면 B를 Null로 한다.
		//	/// </summary>
		//	/// <param name="newVert"></param>
		//	/// <param name="vertA"></param>
		//	/// <param name="vertB"></param>
		//	public void AddVert(GenVertex newVert, GenVertex vertA, GenVertex vertB = null)
		//	{
		//		//버텍스를 사이에 넣는다.
		//		_vertices.Add(newVert);

		//		newVert._linkedVertGroup = this;

		//		string strDebug_Prev = null;
		//		string strDebug_Next = null;


		//		if(vertB == null)
		//		{
		//			vertB = vertA._next;
		//		}

		//		//앞뒤로 연결하자
		//		if(vertB != null)
		//		{	
		//			strDebug_Prev = "> 이전 : A :" + vertA._ID + " (" 
		//				+ (vertA._prev != null ? vertA._prev._ID : -1) + "/" 
		//				+ (vertA._next != null ? vertA._next._ID : -1) 
		//				+ ") / B : " + vertB._ID + " (" 
		//				+ (vertB._prev != null ? vertB._prev._ID : -1) + "/" 
		//				+ (vertB._next != null ? vertB._next._ID : -1) + ")";

		//			//둘다 연결한다면
		//			//A - New - B로 연결한다면 (Prev/Next인지는 확인할 것)
		//			//A와 B의 전후 관계를 확인하자
		//			bool isA2B = true;
		//			if(vertA._next == vertB)
		//			{
		//				//A가 앞
		//				newVert._prev = vertA;
		//				newVert._next = vertB;
		//				isA2B = true;
		//			}
		//			else
		//			{
		//				//A가 뒤
		//				newVert._prev = vertB;
		//				newVert._next = vertA;
		//				isA2B = false;
		//			}

		//			//A, B의 prev에 넣을지 next에 넣을지 확인해야한다.
		//			if(isA2B)
		//			{
		//				//A > New > B
		//				vertA._next = newVert;
		//				vertB._prev = newVert;
		//			}
		//			else
		//			{
		//				//B > New > A
		//				vertB._next = newVert;
		//				vertA._prev = newVert;
		//			}

		//			strDebug_Next = "> 변경 : A :" + vertA._ID + " (" 
		//				+ (vertA._prev != null ? vertA._prev._ID : -1) + "/" 
		//				+ (vertA._next != null ? vertA._next._ID : -1) 
		//				+ ") / B : " + vertB._ID + " (" 
		//				+ (vertB._prev != null ? vertB._prev._ID : -1) + "/" 
		//				+ (vertB._next != null ? vertB._next._ID : -1) 
		//				+ ") / New : " + newVert._ID + " (" 
		//				+ (newVert._prev != null ? newVert._prev._ID : -1) + "/" 
		//				+ (newVert._next != null ? newVert._next._ID : -1) + ")";
		//		}
		//		else
		//		{
		//			strDebug_Prev = "> 이전 : A :" + vertA._ID + " (" 
		//				+ (vertA._prev != null ? vertA._prev._ID : -1) + "/" 
		//				+ (vertA._next != null ? vertA._next._ID : -1) + ") / B : Null";

		//			//A - New로만 연결하자


		//			//A의 prev에 넣을지 next에 넣을지 확인해야한다.
		//			if(vertA._next == null)
		//			{
		//				//A > Null를 A > New > Null로 변경
		//				vertA._next = newVert;
		//				newVert._prev = vertA;
		//				newVert._next = null;
		//			}
		//			else if(vertA._prev == null)
		//			{
		//				//Null > A를 Null > New > A로 변경
		//				vertA._prev = newVert;
		//				newVert._prev = null;
		//				newVert._next = vertA;
		//				_startVert = newVert;//Start 변경
		//				Debug.Log("Start 변경 : " + newVert._ID);
		//			}
		//			else
		//			{
		//				//둘다 비어있지 않으면 에러
		//				Debug.LogError("에러 : 추가 불가..");
		//			}

		//			//뒤에 붙은거니까 End로 취급한다.
		//			_endVert = newVert;

		//			strDebug_Next = "> 변경 : A :" + vertA._ID + " (" 
		//				+ (vertA._prev != null ? vertA._prev._ID : -1) + "/" 
		//				+ (vertA._next != null ? vertA._next._ID : -1) 
		//				+ ") / New : " + newVert._ID + " (" 
		//				+ (newVert._prev != null ? newVert._prev._ID : -1) + "/" 
		//				+ (newVert._next != null ? newVert._next._ID : -1) + ")";
		//		}

		//		//Debug.Log(strDebug_Prev + "\n" + strDebug_Next);
		//	}


		//	//Sort를 한다.
		//	//항상 Prev>Next로만 이어지도록
		//	public void Sort()
		//	{
		//		GenVertex curVert = null;
		//		GenVertex nextVert = null;
		//		GenVertex prevVert = null;

		//		//일단 Start의 Next가 없다면 교환
		//		if(_startVert._next == null && _startVert._prev == null)
		//		{
		//			return;
		//		}
		//		if(_startVert._next == null && _startVert._prev != null)
		//		{
		//			//Prev가 있고 Next가 없다면 > 교환
		//			_startVert._next = _startVert._prev;
		//			_startVert._prev = null;
		//		}

		//		curVert = _startVert;
		//		prevVert = _startVert._prev;
		//		nextVert = _startVert._next;


		//		//Debug.Log("< Sort >");

		//		while (true)
		//		{
		//			//현재 방향의 prev > cur > next와 변수가 다르다면 교체한다.
		//			if (curVert == null)
		//			{
		//				//더 움직일게 없다.
		//				break;
		//			}

		//			//Debug.Log((curVert._prev != null ? curVert._prev._ID : -1) 
		//			//	+ " >> ["  + curVert._ID + "] >> "
		//			//	+ (curVert._next != null ? curVert._next._ID : -1));

		//			//현재 위치에 대해서 멤버 변수 설정
		//			//진행 방향대로 값을 다시 설정한다.
		//			curVert._prev = prevVert;
		//			curVert._next = nextVert;

		//			//이제 다음으로 넘어간다.
		//			//Prev -- Cur -- Next
		//			//       Prev -- Cur -- Next

		//			if(nextVert == null)
		//			{
		//				break;
		//			}

		//			//진행방향과 다르면 안된다.
		//			if(nextVert._next != curVert)
		//			{
		//				nextVert = nextVert._next;
		//			}
		//			else
		//			{
		//				nextVert = nextVert._prev;
		//			}

		//			prevVert = curVert;
		//			curVert = curVert._next;

		//		}

		//		//이제 순서대로 리스트를 정리한다.
		//		Debug.Log("Sort 이전 : " + _vertices.Count);

		//		_vertices.Clear();

		//		curVert = _startVert;
		//		prevVert = null;
		//		nextVert = _startVert._next;

		//		while (true)
		//		{
		//			if (curVert == null)
		//			{
		//				break;
		//			}
		//			if (_vertices.Contains(curVert))
		//			{
		//				Debug.Log("Sort 에러");
		//				break;
		//			}
		//			_vertices.Add(curVert);

		//			prevVert = curVert;
		//			curVert = curVert._next;
		//		}

		//		Debug.Log("Sort 결과 : " + _vertices.Count);
		//	}


		//	public void AddToNewEnd(GenVertex newEndVert)
		//	{	
		//		_vertices.Add(newEndVert);

		//		newEndVert._linkedVertGroup = this;				
		//		newEndVert._next = null;

		//		//서로 연결
		//		newEndVert._prev = _endVert;
		//		_endVert._next = newEndVert;

		//		//End 변경
		//		_endVert = newEndVert;
		//	}

		//	public void AddToNewStart(GenVertex newStartVert)
		//	{	
		//		_vertices.Add(newStartVert);

		//		newStartVert._linkedVertGroup = this;				
		//		newStartVert._prev = null;


		//		//서로 연결
		//		newStartVert._next = _startVert;
		//		_startVert._prev = newStartVert;

		//		//Start 변경
		//		_startVert = newStartVert;
		//	}


		//	public void ConnectEndToStart()
		//	{
		//		_isCompleted = true;
		//	}



		//	public void Reverse()
		//	{
		//		//	public List<GenVertex> _vertices = null;//순서와는 별개이다.
		//		//public GenVertex _startVert = null;
		//		//public GenVertex _endVert = null;
		//		//public bool _isCompleted = false;

		//		//모든 버텍스들의 Prev/Next를 전환하고, Start와 End를 전환한다.
		//		GenVertex vert = null;
		//		for (int i = 0; i < _vertices.Count; i++)
		//		{
		//			vert = _vertices[i];
		//			GenVertex prev = vert._prev;
		//			GenVertex next = vert._next;

		//			//방향 전환
		//			vert._prev = next;
		//			vert._next = prev;
		//		}

		//		//Start와 End도 전환
		//		GenVertex end = _endVert;
		//		GenVertex start = _startVert;
		//		_startVert = end;
		//		_endVert = start;
		//	}

		//	public void MergeToEnd(GenLinkedVertGroup dstGroup)
		//	{
		//		//dstGroup의 Start를 End에 연결한다.
		//		_endVert._next = dstGroup._startVert;
		//		dstGroup._startVert._prev = _endVert;

		//		//End를 dstGroup의 End로 전환하여 Start -> (End) >>>> End로 확장
		//		_endVert = dstGroup._endVert;

		//		//dstGroup의 버텍스들을 리스트에 넣자
		//		GenVertex vert = null;

		//		for (int iDst = 0; iDst < dstGroup._vertices.Count; iDst++)
		//		{
		//			vert = dstGroup._vertices[iDst];
		//			vert._linkedVertGroup = this;//소속 Group 변경
		//			_vertices.Add(vert);
		//		}
		//	}

		//	public void MergeToStart(GenLinkedVertGroup dstGroup)
		//	{
		//		//dstGroup의 End를 Start에 연결한다.
		//		_startVert._prev = dstGroup._endVert;
		//		dstGroup._endVert._next = _startVert;

		//		//Start를 dstGroup의 Start로 전환하여 Start <<<< (Start) <- End로 확장
		//		_startVert = dstGroup._startVert;

		//		//dstGroup의 버텍스들을 리스트에 넣자
		//		GenVertex vert = null;

		//		for (int iDst = 0; iDst < dstGroup._vertices.Count; iDst++)
		//		{
		//			vert = dstGroup._vertices[iDst];
		//			vert._linkedVertGroup = this;//소속 Group 변경
		//			_vertices.Add(vert);
		//		}
		//	}
		//}

		#endregion





		private class GenEdge
		{
			public GenVertex _vert_A;
			public GenVertex _vert_B;

			public bool _isError = false;//에러가 발생했지만 일단 생성되었다.

			//내부 연결시 필요한 weight 데이터
			public float _weight = 1.0f;

			public GenEdge(GenVertex vert_A, GenVertex vert_B)
			{
				_vert_A = vert_A;
				_vert_B = vert_B;

				//_vert_A._nLinkedEdges++;
				//_vert_B._nLinkedEdges++;
				_vert_A._linkedEdges.Add(this);
				_vert_B._linkedEdges.Add(this);
				_isError = false;
			}

			public bool IsSame(GenVertex vertA, GenVertex vertB)
			{
				return (_vert_A == vertA && _vert_B == vertB)
					|| (_vert_A == vertB && _vert_B == vertA);
			}


			public static GenEdgeCrossResult CheckCross(GenEdge edge1, GenEdge edge2)
			{
				if(edge1 == null)
				{
					Debug.LogError("에러 : Null Edge 1");
					//GenEdgeCrossResult.I.Init();
					//return GenEdgeCrossResult.I;
					return null;
				}
				if(edge2 == null)
				{
					Debug.LogError("에러 : Null Edge 2");
					//GenEdgeCrossResult.I.Init();
					//return GenEdgeCrossResult.I;
					return null;
				}
				return CheckCross(edge1._vert_A, edge1._vert_B, edge2._vert_A, edge2._vert_B);
			}

			public static GenEdgeCrossResult CheckCross(GenVertex edge1_VertA, GenVertex edge1_VertB, 
														GenVertex edge2_VertA, GenVertex edge2_VertB)
			{
				GenEdgeCrossResult.I.Init();

				//일단 버텍스가 겹친다면 교차된 것이다.
				//하나만 공유하는지, 아니면 두개의 버텍스를 공유하는지에 따라서 결과를 다르게 입력해야한다.
				if(		(edge1_VertA == edge2_VertA && edge1_VertB == edge2_VertB)
						||	(edge1_VertA == edge2_VertB && edge1_VertB == edge2_VertA))
				{
					//포인트 둘다 공유한다면 > 같은 선분.. 이게 왜 들어왔지
					//GenEdgeCrossResult.I.SetResult_SameLine();

					//Debug.LogError("라인 충돌 1 : SameLine > 무시");
					return GenEdgeCrossResult.I;
				}
				else if(edge1_VertA == edge2_VertA || edge1_VertA == edge2_VertB)
				{
					//기존 버텍스 A를 공유한다.
					//GenEdgeCrossResult.I.SetResult_PointShared(edge1_VertA);
					//Debug.LogError("라인 충돌 2 : A 공유 > 무시");
					return GenEdgeCrossResult.I;
				}
				else if(edge1_VertB == edge2_VertA || edge1_VertB == edge2_VertB)
				{
					//기존 버텍스 B를 공유한다.
					//GenEdgeCrossResult.I.SetResult_PointShared(edge1_VertB);
					//Debug.LogError("라인 충돌 3 : B 공유 > 무시");
					return GenEdgeCrossResult.I;
				}

				//이제부터 시작
				Vector2 v1_A = edge1_VertA._pos;
				Vector2 v1_B = edge1_VertB._pos;
				Vector2 v2_A = edge2_VertA._pos;
				Vector2 v2_B = edge2_VertB._pos;

				//Debug.Log("라인 체크 : Edge 1 : " + v1_A + " - " + v1_B + " // Edge 2 : " + v2_A + " - " + v2_B);

				float zeroBias = 0.2f;

				//만약 어떤 점이 겹친 상태라면 일단 겹친 점에 대한 정보를 넣어준다.
				if (Vector2.Distance(v1_A, v2_A) < zeroBias || Vector2.Distance(v1_A, v2_B) < zeroBias)
				{
					//Vert1 에 겹친다.
					GenEdgeCrossResult.I.SetResult_PointShared(edge1_VertA);
					//Debug.LogError("라인 충돌 4 : A 위치 유사");
					return GenEdgeCrossResult.I;
				}
				else if (Vector2.Distance(v1_B, v2_A) < zeroBias || Vector2.Distance(v1_B, v2_B) < zeroBias)
				{
					//Vert2 에 겹친다.
					GenEdgeCrossResult.I.SetResult_PointShared(edge1_VertB);
					//Debug.LogError("라인 충돌 5 : B 위치 유사");
					return GenEdgeCrossResult.I;
				}

				#region [이 알고리즘 뭔가 이상하다]
				//float dX_1 = edge1B.x - edge1A.x;
				//float dY_1 = edge1B.y - edge1A.y;
				//float dX_2 = edge2B.x - edge2A.x;
				//float dY_2 = edge2B.y - edge2A.y;

				//float a1 = 0.0f;
				//float a2 = 0.0f;
				//float b1 = 0.0f;
				//float b2 = 0.0f;

				//float x1_Min = Mathf.Min(edge1A.x, edge1B.x);
				//float x1_Max = Mathf.Max(edge1A.x, edge1B.x);
				//float y1_Min = Mathf.Min(edge1A.y, edge1B.y);
				//float y1_Max = Mathf.Max(edge1A.y, edge1B.y);

				//float x2_Min = Mathf.Min(edge2A.x, edge2B.x);
				//float x2_Max = Mathf.Max(edge2A.x, edge2B.x);
				//float y2_Min = Mathf.Min(edge2A.y, edge2B.y);
				//float y2_Max = Mathf.Max(edge2A.y, edge2B.y);

				////수직/수평에 따라 다르게 처리
				////if (Mathf.Abs(dX_1) < zeroBias * 0.01f)
				//if(Mathf.Approximately(dX_1, 0.0f))
				//{
				//	//Line 1이 수직일 때
				//	float X1 = (edge1A.x + edge1B.x) * 0.5f;

				//	//if (Mathf.Abs(dX_2) < zeroBias * 0.01f)
				//	if(Mathf.Approximately(dX_2, 0.0f))
				//	{
				//		//Line 2도 같이 수직일 때
				//		//수직 + 수직
				//		//x가 같으면 [겹침] (y범위 비교)
				//		//그 외에는 [평행]

				//		float X2 = (edge2A.x + edge2B.x) * 0.5f;

				//		//if (Mathf.Abs(X1 - X2) < zeroBias * 0.01f)
				//		if (Mathf.Approximately(X1, X2))
				//		{

				//			//Y 영역이 겹치는가 [Y영역이 겹치면 겹침]
				//			if (IsAreaIntersection(y1_Min, y1_Max, y2_Min, y2_Max))
				//			{
				//				//[겹침] : 수직1 + 수직2
				//				GenEdgeCrossResult.I.SetResult_SameLine();
				//				Debug.LogError("라인 충돌 6 : 유사 라인");
				//				return GenEdgeCrossResult.I;
				//			}
				//		}
				//	}
				//	else if(Mathf.Approximately(dY_2, 0.0f))
				//	{
				//		//Line2가 수평일 때
				//		float Y2 = (edge2A.y + edge2B.y) * 0.5f;

				//		//서로가 범위 안에 들어가야 한다.
				//		if(y1_Min <= Y2 && Y2 <= y1_Max
				//			&& x2_Min <= X1 && X1 <= x2_Max)
				//		{
				//			//[교차] : 수직1 + 수평2
				//			GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(X1, Y2));
				//			Debug.LogError("라인 충돌 7 : 교차");
				//			return GenEdgeCrossResult.I;
				//		}
				//	}
				//	else
				//	{
				//		//Line 2는 수평이나 기울기가 있을 때
				//		//Line1의 x 범위에서 y 안에 들면 [교차]
				//		//Line1의 x 범위 밖이거나 y 범위 밖에 있으면 [교차하지 않음]
				//		if (x2_Min <= X1 && X1 <= x2_Max)
				//		{
				//			a2 = dY_2 / dX_2;
				//			b2 = edge2A.y - edge2A.x * a2;

				//			float Yresult = a2 * X1 + b2;
				//			if (y1_Min <= Yresult && Yresult <= y1_Max)
				//			{
				//				//[교차] : 수직1 + 기울기2
				//				GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(X1, Yresult));
				//				Debug.LogError("라인 충돌 8 : 교차");
				//				return GenEdgeCrossResult.I;
				//			}
				//		}
				//	}
				//}
				//else if(Mathf.Approximately(dY_1, 0.0f))
				//{
				//	//Line 1이 수평일 때
				//	float Y1 = (edge1A.y + edge1B.y) * 0.5f;
				//	if (Mathf.Approximately(dX_2, 0.0f))
				//	{
				//		//Line 2가 수직일 때
				//		//수평 + 수직
				//		//교차점 비교
				//		float X2 = (edge2A.x + edge2B.x) * 0.5f;

				//		if(y2_Min <= Y1 && Y1 <= y2_Max
				//			&& x1_Min <= X2 && X2 <= x1_Max)
				//		{
				//			//[교차] : 수평1 + 수직2
				//			GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(X2, Y1));
				//			Debug.LogError("라인 충돌 9 : 교차");
				//			return GenEdgeCrossResult.I;
				//		}
				//	}
				//	else if (Mathf.Approximately(dY_2, 0.0f))
				//	{
				//		//Line 2가 수평일 때
				//		//수평 + 수평
				//		//Y가 같고 X 범위가 겹쳐야 함 Same
				//		float Y2 = (edge2A.y + edge2B.y) * 0.5f;

				//		if(Mathf.Approximately(Y1, Y2))
				//		{
				//			if (IsAreaIntersection(x1_Min, x1_Max, x2_Min, x2_Max))
				//			{
				//				//[겹침] : 수평1 + 수평2
				//				GenEdgeCrossResult.I.SetResult_SameLine();
				//				Debug.LogError("라인 충돌 10 : 유사 라인");
				//				return GenEdgeCrossResult.I;
				//			}
				//		}
				//	}
				//	else
				//	{
				//		//Line 2가 기울기가 있을 때
				//		//범위와 교점 체크
				//		//수평 + 기울기
				//		a1 = 0.0f;
				//		b1 = edge1A.y;

				//		a2 = dY_2 / dX_2;
				//		b2 = edge2A.y - edge2A.x * a2;

				//		float XResult = (b1 - b2) / a2;

				//		if(x1_Min <= XResult && XResult <= x1_Max
				//			&& x2_Min <= XResult && XResult <= x2_Max)
				//		{
				//			//Line 1, 2의 X 범위 안에 들어간다.
				//			GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(XResult, b1));
				//			Debug.LogError("라인 충돌 11 : 교차");
				//			return GenEdgeCrossResult.I;
				//		}
				//	}
				//}
				//else
				//{
				//	//Line 1이 기울기가 있을 때
				//	//if (Mathf.Abs(dX_2) < zeroBias * 0.01f)
				//	a1 = dY_1 / dX_1;
				//	b1 = edge1A.y - edge1A.x * a1;

				//	if(Mathf.Approximately(dX_2, 0.0f))
				//	{
				//		//Line 2가 수직일 때
				//		//기울기 + 수직
				//		//Line2를 기준으로 x, y범위 비교후 Y 체크 [교차]
				//		//범위 밖이면 [교차하지 않음]

				//		float X2 = (edge2A.x + edge2B.x) * 0.5f;

				//		if (x1_Min <= X2 && X2 <= x1_Max)
				//		{
				//			float Yresult = a1 * X2 + b1;
				//			if (y2_Min <= Yresult && Yresult <= y2_Max)
				//			{
				//				//[교차]
				//				GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(X2, Yresult));
				//				Debug.LogError("라인 충돌 12 : 교차");
				//				return GenEdgeCrossResult.I;
				//			}
				//		}
				//	}
				//	else if (Mathf.Approximately(dY_2, 0.0f))
				//	{
				//		//Line 2가 수평일 때
				//		//기울기 + 수평
				//		a2 = 0.0f;
				//		b2 = edge2A.y;

				//		float XResult = (b2 - b1) / a1;

				//		if(x1_Min <= XResult && XResult <= x1_Max
				//			&& x2_Min <= XResult && XResult <= x2_Max)
				//		{
				//			//Line 1, 2의 X 범위 안에 들어간다.
				//			GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(XResult, b2));
				//			Debug.LogError("라인 충돌 13 : 교차");
				//			return GenEdgeCrossResult.I;
				//		}
				//	}
				//	else
				//	{
				//		//Line 2는 수평이나 기울기가 있을 때
				//		//X 범위 비교후
				//		//대입법 이용하여 체크하면 [교차]

				//		if (IsAreaIntersection(x1_Min, x1_Max, x2_Min, x2_Max))
				//		{	

				//			a2 = dY_2 / dX_2;
				//			b2 = edge2A.y - edge2A.x * a2;

				//			float Yparam1 = a2 - a1;
				//			float Yparam2 = (a2 * b1) - (a1 * b2);

				//			//if (Mathf.Abs(Yparam1) < zeroBias * 0.01f)
				//			if(Mathf.Approximately(Yparam1, 0.0f))
				//			{
				//				//기울기가 같을때
				//				//b도 같아야한다.
				//				//if (Mathf.Abs(Yparam2) < zeroBias * 0.01f)
				//				if (Mathf.Approximately(b1, b2))
				//				{
				//					//[일치]
				//					GenEdgeCrossResult.I.SetResult_SameLine();
				//					Debug.LogError("라인 충돌 14 : 유사 라인");
				//					return GenEdgeCrossResult.I;
				//				}
				//			}
				//			else
				//			{
				//				//기울기가 다를때
				//				float Yresult = Yparam2 / Yparam1;

				//				//교차점의 위치를 확인한다.
				//				if (y1_Min <= Yresult && Yresult <= y1_Max &&
				//					y2_Min <= Yresult && Yresult <= y2_Max)
				//				{

				//					float Xresult = (Yresult - b1) / a1;

				//					GenEdgeCrossResult.I.SetResult_InnerCross(new Vector2(Xresult, Yresult));
				//					Debug.LogError("라인 충돌 15 : 교차");
				//					return GenEdgeCrossResult.I;
				//				}
				//			}
				//		}
				//	}
				//} 
				#endregion



				
				float A1 = v1_B.y - v1_A.y;
				float B1 = v1_A.x - v1_B.x;
				float C1 = A1 * v1_A.x + B1 * v1_A.y;

				// Get A,B,C of second line - points : ps2 to pe2
				float A2 = v2_B.y - v2_A.y;
				float B2 = v2_A.x - v2_B.x;
				float C2 = A2 * v2_A.x + B2 * v2_A.y;

				// Get delta and check if the lines are parallel
				float delta = A1 * B2 - A2 * B1;
				if (delta == 0)
				{
					//그냥 리턴
					return GenEdgeCrossResult.I;
				}

				Vector2 intersection = new Vector2((B2 * C1 - B1 * C2) / delta,
													(A1 * C2 - A2 * C1) / delta
												  );

				float p1 = (intersection - v1_A).magnitude / (v1_B - v1_A).magnitude;
				float p2 = (intersection - v2_A).magnitude / (v2_B - v2_A).magnitude;
				float bias = 0.3f;

				bool bHit_1 = false;
				bool bHit_2 = false;
				if (p1 < bias * 0.5f)
				{
					bHit_1 = false;
				}
				else if (p1 < bias)
				{
					//충돌
					bHit_1 = true;
				}
				else if (p1 < 1.0f - bias)
				{
					//충돌
					bHit_1 = true;
				}
				else if (p1 < 1.0f - bias * 0.5f)
				{
					//충돌
					bHit_1 = true;
				}
				else
				{
					bHit_1 = false;
				}


				if (p2 < bias * 0.5f)
				{
					bHit_2 = false;
				}
				else if (p2 < bias)
				{
					// 충돌
					bHit_2 = true;

				}
				else if (p2 < 1.0f - bias)
				{
					bHit_2 = true;
				}
				else if (p2 < 1.0f - bias * 0.5f)
				{
					//충돌함
					bHit_2 = true;
				}
				else
				{
					bHit_2 = false;
				}

				if (bHit_1 && bHit_2)
				{
					//충돌함
					//Debug.LogError("라인 충돌 6 : 충돌함");
					GenEdgeCrossResult.I.SetResult_InnerCross(intersection);
				}
				

				return GenEdgeCrossResult.I;
			}



			public static GenEdgeCrossResult CheckCross(Vector2 v1_A, Vector2 v1_B, Vector2 v2_A, Vector2 v2_B)
			{
				GenEdgeCrossResult.I.Init();

				float zeroBias = 0.2f;

				//만약 어떤 점이 겹친 상태라면 일단 겹친 점에 대한 정보를 넣어준다.
				if (Vector2.Distance(v1_A, v2_A) < zeroBias || Vector2.Distance(v1_A, v2_B) < zeroBias)
				{
					//Vert1 에 겹친다.
					GenEdgeCrossResult.I.SetResult_InnerCross(v1_A);
					return GenEdgeCrossResult.I;
				}
				else if (Vector2.Distance(v1_B, v2_A) < zeroBias || Vector2.Distance(v1_B, v2_B) < zeroBias)
				{
					//Vert2 에 겹친다.
					GenEdgeCrossResult.I.SetResult_InnerCross(v1_B);
					return GenEdgeCrossResult.I;
				}


				float A1 = v1_B.y - v1_A.y;
				float B1 = v1_A.x - v1_B.x;
				float C1 = A1 * v1_A.x + B1 * v1_A.y;

				// Get A,B,C of second line - points : ps2 to pe2
				float A2 = v2_B.y - v2_A.y;
				float B2 = v2_A.x - v2_B.x;
				float C2 = A2 * v2_A.x + B2 * v2_A.y;

				// Get delta and check if the lines are parallel
				float delta = A1 * B2 - A2 * B1;
				if (delta == 0)
				{
					//그냥 리턴
					return GenEdgeCrossResult.I;
				}

				Vector2 intersection = new Vector2((B2 * C1 - B1 * C2) / delta,
													(A1 * C2 - A2 * C1) / delta
												  );

				float p1 = (intersection - v1_A).magnitude / (v1_B - v1_A).magnitude;
				float p2 = (intersection - v2_A).magnitude / (v2_B - v2_A).magnitude;
				float bias = 0.3f;

				bool bHit_1 = false;
				bool bHit_2 = false;
				if (p1 < bias * 0.5f)
				{
					bHit_1 = false;
				}
				else if (p1 < bias)
				{
					//충돌
					bHit_1 = true;
				}
				else if (p1 < 1.0f - bias)
				{
					//충돌
					bHit_1 = true;
				}
				else if (p1 < 1.0f - bias * 0.5f)
				{
					//충돌
					bHit_1 = true;
				}
				else
				{
					bHit_1 = false;
				}


				if (p2 < bias * 0.5f)
				{
					bHit_2 = false;
				}
				else if (p2 < bias)
				{
					// 충돌
					bHit_2 = true;

				}
				else if (p2 < 1.0f - bias)
				{
					bHit_2 = true;
				}
				else if (p2 < 1.0f - bias * 0.5f)
				{
					//충돌함
					bHit_2 = true;
				}
				else
				{
					bHit_2 = false;
				}

				if (bHit_1 && bHit_2)
				{
					//충돌함
					GenEdgeCrossResult.I.SetResult_InnerCross(intersection);
				}

				return GenEdgeCrossResult.I;
			}




			public static GenEdgeCrossResult CheckCrossApprox(Vector2 v1_A, Vector2 v1_B, Vector2 v2_A, Vector2 v2_B, bool isAccurate = false)
			{	
				GenEdgeCrossResult.I.Init();

				//라인의 범위를 비교한다.
				float edge1_MinX = Mathf.Min(v1_A.x, v1_B.x);
				float edge1_MaxX = Mathf.Max(v1_A.x, v1_B.x);
				float edge1_MinY = Mathf.Min(v1_A.y, v1_B.y);
				float edge1_MaxY = Mathf.Max(v1_A.y, v1_B.y);

				float edge2_MinX = Mathf.Min(v2_A.x, v2_B.x);
				float edge2_MaxX = Mathf.Max(v2_A.x, v2_B.x);
				float edge2_MinY = Mathf.Min(v2_A.y, v2_B.y);
				float edge2_MaxY = Mathf.Max(v2_A.y, v2_B.y);
				//X축이나 Y축으로 벗어났다면 비교하지 않는다.
				if(edge1_MaxX < edge2_MinX || edge2_MaxX < edge1_MinX
					|| edge1_MaxY < edge2_MinY || edge2_MaxY < edge1_MinY)
				{
					return GenEdgeCrossResult.I;
				}

				float pointPosBias = isAccurate ? 1.0f : 1.5f;

				//두개의 포인트가 비슷한지 보자
				if(Vector2.Distance(v1_A, v2_A) < pointPosBias ||
					Vector2.Distance(v1_A, v2_B) < pointPosBias)
				{
					//Hit함
					GenEdgeCrossResult.I.SetResult_InnerCross(v1_A);
					return GenEdgeCrossResult.I;
				}

				if( Vector2.Distance(v1_B, v2_A) < pointPosBias ||
					Vector2.Distance(v1_B, v2_B) < pointPosBias)
				{
					//Hit함
					GenEdgeCrossResult.I.SetResult_InnerCross(v1_B);
					return GenEdgeCrossResult.I;
				}

				Vector2 dir1 = v1_B - v1_A;
				Vector2 dir2 = v2_B - v2_A;
				if(dir1.magnitude < 0.2f)
				{
					//선분으로 보기에 너무 짧다
					return GenEdgeCrossResult.I;
				}
				if(dir2.magnitude < 0.2f)
				{
					//선분으로 보기에 너무 짧다
					return GenEdgeCrossResult.I;
				}

				float length1 = dir1.magnitude;
				float length2 = dir2.magnitude;

				dir1.Normalize();
				dir2.Normalize();

				float t1 = 0.0f;
				bool isT2Valid_X = false;
				bool isT2Valid_Y = false;
				float t2_X = 0.0f;
				float t2_Y = 0.0f;


				Vector2 pos1 = Vector2.zero;
				Vector2 pos2 = Vector2.zero;

				//모든 t1, t2를 체크하지 말고, 겹치는 영역만 확인하자
				float common_MinX = Mathf.Max(edge1_MinX, edge2_MinX);
				float common_MaxX = Mathf.Min(edge1_MaxX, edge2_MaxX);
				float common_MinY = Mathf.Max(edge1_MinY, edge2_MinY);
				float common_MaxY = Mathf.Min(edge1_MaxY, edge2_MaxY);

				if(common_MaxX - common_MinX < 0.05f)
				{
					float centerX = (common_MaxX * 0.5f) + (common_MinX * 0.5f);
					common_MaxX = centerX + 2.0f;
					common_MinX = centerX - 2.0f;
				}
				if(common_MaxY - common_MinY < 0.05f)
				{
					float centerY = (common_MaxY * 0.5f) + (common_MinY * 0.5f);
					common_MaxY = centerY + 2.0f;
					common_MinY = centerY - 2.0f;
				}
				//if(common_MaxX - common_MinX < 0.05f && 
				//	common_MaxY - common_MinY < 0.05f)
				//{
				//	//범위가 너무 좁다
				//	return GenEdgeCrossResult.I;
				//}

				//T1의 범위를 정하자
				//minX = Ax + dirX * t
				//(minX - Ax) / dirX = t
				float t1_Min_dX = 0.0f;
				float t1_Max_dX = length1;
				float t1_Min_dY = 0.0f;
				float t1_Max_dY = length1;
				
				float dirBias = 0.001f;
				
				if(Mathf.Abs(dir1.x) > dirBias)
				{
					t1_Min_dX = (common_MinX - v1_A.x) / dir1.x;
					t1_Max_dX = (common_MaxX - v1_A.x) / dir1.x;
					if(t1_Min_dX > t1_Max_dX)
					{
						float tmp = t1_Min_dX;
						t1_Min_dX = t1_Max_dX;
						t1_Max_dX = tmp;
					}
				}
				if(Mathf.Abs(dir1.y) > dirBias)
				{
					t1_Min_dY = (common_MinY - v1_A.y) / dir1.y;
					t1_Max_dY = (common_MaxY - v1_A.y) / dir1.y;
					if(t1_Min_dY > t1_Max_dY)
					{
						float tmp = t1_Min_dY;
						t1_Min_dY = t1_Max_dY;
						t1_Max_dY = tmp;
					}
				}

				float t1_Min = Mathf.Clamp(Mathf.Max(t1_Min_dX, t1_Min_dY), 0.0f, length1);
				float t1_Max = Mathf.Clamp(Mathf.Min(t1_Max_dX, t1_Max_dY), 0.0f, length1);
				
				if(t1_Min > t1_Max)
				{
					float tmpT = t1_Min;
					t1_Min = t1_Max;
					t1_Max = tmpT;
				}
				
				//최종 공통 간격 계산
				//if(Mathf.Abs(t1_Max - t1_Min) < 0.2f)
				//{
				//	//너무 좁다
				//	Debug.LogError("좁은 영역 취소 : " + t1_Min + " ~ " + t1_Max);
				//	Debug.Log("common_X : " + common_MinX + " ~ " + common_MaxX);
				//	Debug.Log("common_Y : " + common_MinY + " ~ " + common_MaxY);
				//	Debug.Log("t1_Min_dX : " + t1_Min_dX + " / t1_Max_dX : " + t1_Max_dX + " (dirX : " + dir1.x + ")");
				//	Debug.Log("t1_Min_dY : " + t1_Min_dY + " / t1_Max_dY : " + t1_Max_dY + " (dirY : " + dir1.y + ")");
				//	return GenEdgeCrossResult.I;
				//}

				t1 = t1_Min;


				float midNearBias = isAccurate ? 1.5f : 3.0f;
				float endNearBias = 0.5f;
				float endCheckBias = 2.0f;//t가 이 범위 안에 들어오면 midNearBias가 아닌 endNearBias를 이용해야한다.

				float moveBias = isAccurate ? 0.4f : 0.7f;

				bool isLastCheck = false;

				while(true)
				{
					pos1 = v1_A + dir1 * t1;

					t2_X = 0.0f;
					t2_Y = 0.0f;
					isT2Valid_X = false;
					isT2Valid_Y = false;

					//현재 위치에 해당하는 t2를 구하자
					//기울기에 따라서 오차가 있을 수 있으니, X축으로 체크, Y축으로도 체크를 한다.
					if(Mathf.Abs(dir2.x) > dirBias)
					{
						t2_X = (pos1.x - v2_A.x) / dir2.x;
						isT2Valid_X = true;
					}

					if(Mathf.Abs(dir2.y) > dirBias)
					{
						t2_Y = (pos1.y - v2_A.y) / dir2.y;
						isT2Valid_Y = true;
					}

					if(isT2Valid_X)
					{
						//X에 의한 것이므로 Y를 체크
						if(t2_X >= 0.0f && t2_X <= length2)
						{
							pos2 = v2_A + dir2 * t2_X;
							float accBias = midNearBias;
							
							if(isAccurate)
							{
								//정확 모드에서는 더 세분화한 거리 계산
								if(t1 < endCheckBias || t1 > length1 - endCheckBias
									|| t2_X < endCheckBias || t2_X > length2 - endCheckBias)
								{
									accBias = endNearBias;//끝점 근처에서는 체크 범위가 줄어든다.
								}
							}

							if (Mathf.Abs(pos1.y - pos2.y) <= accBias)
							{
								//Hit함
								GenEdgeCrossResult.I.SetResult_InnerCross(pos1 * 0.5f + pos2 * 0.5f);
								return GenEdgeCrossResult.I;
							}
							
						}
					}
					if(isT2Valid_Y)
					{
						//Y에 의한 것이므로 X를 체크
						if(t2_Y >= 0.0f && t2_Y <= length2)
						{
							pos2 = v2_A + dir2 * t2_Y;

							float accBias = midNearBias;

							if(isAccurate)
							{
								//정확 모드에서는 더 세분화한 거리 계산
								if(t1 < endCheckBias || t1 > length1 - endCheckBias
									|| t2_Y < endCheckBias || t2_Y > length2 - endCheckBias)
								{
									accBias = endNearBias;//끝점 근처에서는 체크 범위가 줄어든다.
								}
							}

							if (Mathf.Abs(pos1.x - pos2.x) <= accBias)
							{
								//Hit함
								GenEdgeCrossResult.I.SetResult_InnerCross(pos1 * 0.5f + pos2 * 0.5f);
								return GenEdgeCrossResult.I;
							}
						}
					}
					
					//이건 너무 노가다고..
					//while(true)
					//{
					//	pos2 = v2_A + dir2 * t2;

					//	if(Vector2.Distance(pos1, pos2) <= bias)
					//	{
					//		//Hit함
					//		GenEdgeCrossResult.I.SetResult_InnerCross(pos1 * 0.5f + pos2 * 0.5f);
					//		return GenEdgeCrossResult.I;
					//	}
					//	//Hit가 안되었다면
					//	t2 += 1.0f;//1픽셀 이동
					//	if(t2 > length2)
					//	{
					//		break;
					//	}
					//}

					//Hit가 안되었다면
					if(isLastCheck)
					{
						//마지막 체크 > 종료
						break;
					}

					t1 += moveBias;//이동
					//if(t1 > length1)
					if(t1 > t1_Max || t1 > length1)
					{
						isLastCheck = true;
						t1 = t1_Max;//마지막 지점에서 한번 더 체크
					}
				}

				return GenEdgeCrossResult.I;

				
			}


			/// <summary>
			/// 여러개의 선분과 하나의 선분을 비교한다.
			/// </summary>
			public static bool CheckCrossApprox(Vector2 v1_A, Vector2 v1_B, List<Vector2> v2s_A, List<Vector2> v2s_B)
			{
				int nV2s = v2s_A.Count;
				for (int i = 0; i < nV2s; i++)
				{
					if(CheckCrossApprox(v1_A, v1_B, v2s_A[i], v2s_B[i])._result != GenEdgeCrossResult.RESULT_TYPE.NoCross)
					{
						return true;
					}
				}
				return false;

			}

			private static bool IsAreaIntersection(float area1Min, float area1Max, float area2Min, float area2Max)
			{
				//[ 1 ] .. [ 2 ] 이거나 [ 2 ] .. [ 1 ]으로 서로 겹쳐지지 않을 때
				if (area1Max < area2Min || area2Max < area1Min)
				{
					return false;
				}
				return true;
			}

		}
		





		private class GenEdgeCrossResult
		{
			//Static
			private static GenEdgeCrossResult _instance = new GenEdgeCrossResult();
			public static GenEdgeCrossResult I { get { return _instance; } }

			public enum RESULT_TYPE
			{
				/// <summary>교차되지 않느다.</summary>
				NoCross,
				/// <summary>같은 선이다. (점을 두개 공유한다.)</summary>
				SameLine,
				/// <summary>점을 하나 공유한다.</summary>
				PointShared,
				/// <summary>교차된다.</summary>
				Cross,
			}

			public RESULT_TYPE _result = RESULT_TYPE.NoCross;
			public Vector2 _pos = Vector2.zero;
			public GenVertex _sharedPoint = null;

			private GenEdgeCrossResult()
			{
				Init();
			}
			public void Init()
			{
				_result = RESULT_TYPE.NoCross;
				_pos = Vector2.zero;
				_sharedPoint = null;
			}
			public void SetResult_SameLine()
			{
				_result = RESULT_TYPE.SameLine;
			}

			public void SetResult_PointShared(GenVertex sharedPoint)
			{
				_result = RESULT_TYPE.PointShared;
				_sharedPoint = sharedPoint;
			}

			public void SetResult_InnerCross(Vector2 pos)
			{
				_result = RESULT_TYPE.Cross;
				_pos = pos;
			}
		}





		private class GenEdgeGrid
		{
			private float _gridSize = 10.0f;
			private Dictionary<int, Dictionary<int, List<GenEdge>>> _edges = null;//X, Y, List 순

			public GenEdgeGrid(float gridSize)
			{
				_gridSize = gridSize;
				_edges = new Dictionary<int, Dictionary<int, List<GenEdge>>>();
			}

			public void Clear()
			{
				_edges.Clear();
			}

			public void AddEdge(GenEdge edge)
			{
				//VertA / B에 대한 Index
				int iPosA_X = (int)(edge._vert_A._pos.x / _gridSize);
				int iPosA_Y = (int)(edge._vert_A._pos.y / _gridSize);
				int iPosB_X = (int)(edge._vert_B._pos.x / _gridSize);
				int iPosB_Y = (int)(edge._vert_B._pos.y / _gridSize);

				int iMinX = Mathf.Min(iPosA_X, iPosB_X);
				int iMaxX = Mathf.Max(iPosA_X, iPosB_X);
				int iMinY = Mathf.Min(iPosA_Y, iPosB_Y);
				int iMaxY = Mathf.Max(iPosA_Y, iPosB_Y);

				iMinX -= 1;
				iMaxX += 1;
				iMinY -= 1;
				iMaxY += 1;

				Dictionary<int, List<GenEdge>> curYEdges = null;

				//Edge는 해당되는 모든 그리드 영역에 Edge를 추가한다.
				for (int iX = iMinX; iX <= iMaxX; iX++)
				{
					if(!_edges.ContainsKey(iX))
					{
						_edges.Add(iX, new Dictionary<int, List<GenEdge>>());
					}
					curYEdges = _edges[iX];
					for (int iY = iMinY; iY <= iMaxY; iY++)
					{
						if(!curYEdges.ContainsKey(iY))
						{
							curYEdges.Add(iY, new List<GenEdge>());
						}
						curYEdges[iY].Add(edge);
					}
				}
			}

			public List<GenEdge> GetEdges(Vector2 posA, Vector2 posB)
			{
				List<GenEdge> result = new List<GenEdge>();

				int iPosA_X = (int)(posA.x / _gridSize);
				int iPosA_Y = (int)(posA.y / _gridSize);
				int iPosB_X = (int)(posB.x / _gridSize);
				int iPosB_Y = (int)(posB.y / _gridSize);

				int iMinX = Mathf.Min(iPosA_X, iPosB_X);
				int iMaxX = Mathf.Max(iPosA_X, iPosB_X);
				int iMinY = Mathf.Min(iPosA_Y, iPosB_Y);
				int iMaxY = Mathf.Max(iPosA_Y, iPosB_Y);

				Dictionary<int, List<GenEdge>> curYEdges = null;
				List<GenEdge> edges = null;
				GenEdge curEdge = null;

				for (int iX = iMinX; iX <= iMaxX; iX++)
				{
					if(!_edges.ContainsKey(iX))
					{
						continue;
					}
					curYEdges = _edges[iX];
					for (int iY = iMinY; iY <= iMaxY; iY++)
					{
						if(!curYEdges.ContainsKey(iY))
						{
							continue;
						}
						edges = curYEdges[iY];

						for (int iEdge = 0; iEdge < edges.Count; iEdge++)
						{
							curEdge = edges[iEdge];
							if(result.Contains(curEdge))
							{
								continue;
							}
							result.Add(curEdge);
						}
					}
				}

				return result;//결과를 리턴한다.
			}

			
		}


		//이미 해당 Vertex를 잇는 Edge가 생성되었는지 확인하는 캐시
		private class GenEdgeCache
		{
			private Dictionary<GenVertex, Dictionary<GenVertex, GenEdge>> _edges = null;

			public GenEdgeCache()
			{
				_edges = new Dictionary<GenVertex, Dictionary<GenVertex, GenEdge>>();
				_edges.Clear();
			}
			public void Clear()
			{
				_edges.Clear();
			}

			public bool IsContain(GenVertex vertA, GenVertex vertB)
			{
				return GetEdge(vertA, vertB) != null;
			}

			public GenEdge GetEdge(GenVertex vertA, GenVertex vertB)
			{
				if(_edges.ContainsKey(vertA))
				{
					if(_edges[vertA].ContainsKey(vertB))
					{
						return (_edges[vertA])[vertB];
					}
				}
				return null;
			}

			public void AddEdge(GenEdge edge)
			{
				//A, B기준으로 각각 저장하자
				if(!_edges.ContainsKey(edge._vert_A))
				{
					_edges.Add(edge._vert_A, new Dictionary<GenVertex, GenEdge>());
				}
				if(!_edges.ContainsKey(edge._vert_B))
				{
					_edges.Add(edge._vert_B, new Dictionary<GenVertex, GenEdge>());
				}
				_edges[edge._vert_A].Add(edge._vert_B, edge);
				_edges[edge._vert_B].Add(edge._vert_A, edge);
			}

			public void RemoveEdge(GenEdge edge)
			{
				if (_edges.ContainsKey(edge._vert_A))
				{
					if(_edges[edge._vert_A].ContainsKey(edge._vert_B))
					{
						_edges[edge._vert_A].Remove(edge._vert_B);
						if(_edges[edge._vert_A].Count == 0)
						{
							_edges.Remove(edge._vert_A);
						}
					}
				}

				if (_edges.ContainsKey(edge._vert_B))
				{
					if(_edges[edge._vert_B].ContainsKey(edge._vert_A))
					{
						_edges[edge._vert_B].Remove(edge._vert_A);
						if(_edges[edge._vert_B].Count == 0)
						{
							_edges.Remove(edge._vert_B);
						}
					}
				}
			}
		}

		



		#region [미사용 코드] Circle Area 방식은 사용하지 않는다.

		////<원형 영역>
		////버텍스를 생성하기 위한 메타 정보로, 원형의 영역이 배치된다.
		////Root가 존재하며, Root는 한개가 아닐 수 있다. (단, 분할 과정에서 합쳐질 수 있다.)
		////각각의 방향에 대해서 자식 원형이 존재하며, 부모 방향으로는 자식이 생성되지 않는다.
		//private class GenCircleArea
		//{
		//	//위치와 크기/방향
		//	public Vector2 _pos = Vector2.zero;

		//	//Merge 처리를 위한 변수
		//	public float _radius_ForMerge_Max = 0.0f;//모든 방향으로 했을때 가장 큰 크기 (메시를 모두 커버함)
		//	public float _radius_ForMerge_Min = 0.0f;

		//	public enum CALCULATE_STATUS
		//	{
		//		NotCalculated,
		//		Calculated,
		//		Failed,

		//	}
		//	public CALCULATE_STATUS _calculateResult = CALCULATE_STATUS.NotCalculated;
		//	public float _radius_Min_Outer = 0.0f;//여백을 발견한 크기 중 최소
		//	public float _radius_Min_Inner = 0.0f;//여백 직전의 크기 (약간 작음)

		//	//부모가 있는 경우, 이것이 어떤 방향으로 인하여 생성되었는지 알 수 있다.
		//	public Vector2 _direction = Vector2.zero;
		//	public float _angleFromParent = 0.0f;//부모와의 각도

		//	//누적된 각도
		//	public Vector2 _direction_FromRoot = Vector2.zero;
		//	public float _angleFromRoot = 0.0f;

		//	//계층
		//	public GenCircleArea _parent = null;//null이면 Root. 병합도 가능하다.
		//	public List<GenCircleArea> _childs = null;

		//	//Ray들
		//	public List<GenRay> _rays = new List<GenRay>();
		//	public int _nRays = 0;

		//	public bool _isEmptyCenter = false;//만약 시작점이 비어있다면, 체크를 다르게 해야한다.

		//	public int _groupID = -1;

		//	public GenCircleArea(Vector2 pos, int groupID)
		//	{
		//		_pos = pos;

		//		_parent = null;
		//		if(_childs == null)
		//		{
		//			_childs = new List<GenCircleArea>();
		//		}
		//		_childs.Clear();

		//		_calculateResult = CALCULATE_STATUS.NotCalculated;
		//		_radius_Min_Outer = 0.0f;
		//		_radius_Min_Inner = 0.0f;

		//		_direction = Vector2.zero;
		//		_angleFromParent = 0.0f;//부모와의 각도

		//		_isEmptyCenter = false;
		//		_groupID = groupID;
		//	}

		//	public void SetParent(GenCircleArea parent, float angle, Vector2 dir)
		//	{
		//		_parent = parent;

		//		//각도까지 자동 설정 (이 단계에서 크기는 모름)
		//		if(_parent != null)
		//		{
		//			//_direction = new Vector2(_pos.x - parent._pos.x, _pos.y - _parent._pos.y);
		//			//_angleFromParent = Mathf.Atan2(_direction.y, _direction.x);

		//			_direction = dir;
		//			_angleFromParent = angle;

		//			_direction_FromRoot = (_parent._direction_FromRoot + dir).normalized;
		//			_angleFromRoot = apUtil.AngleTo360(_parent._angleFromRoot + angle);

		//			//부모에도 추가
		//			if(_parent._childs == null)
		//			{
		//				_parent._childs = new List<GenCircleArea>();
		//			}
		//			_parent._childs.Add(this);//정렬은 언제..
		//		}
		//		else
		//		{
		//			_direction = Vector2.zero;
		//			_angleFromParent = 0.0f;

		//			_direction_FromRoot = Vector2.zero;
		//			_angleFromRoot = 0.0f;
		//		}
		//	}

		//	public void SortChilds()
		//	{
		//		if(_parent._childs == null || _parent._childs.Count == 0)
		//		{
		//			return;
		//		}

		//		_parent._childs.Sort(delegate(GenCircleArea a, GenCircleArea b)
		//		{
		//			return (int)((a._angleFromParent - b._angleFromParent) * 1000);
		//		});
		//	}

		//	//두개의 원의 가장 큰 반경을 기준으로 겹치는지 체크
		//	public bool IsOverlap(GenCircleArea target)
		//	{
		//		float distToCenter = Vector2.Distance(_pos, target._pos);
		//		return distToCenter < (_radius_ForMerge_Max + target._radius_ForMerge_Max);
		//	}

		//	public void MakeRays(int nRays)
		//	{
		//		//요청에 맞게 Ray들을 만들자
		//		if(_rays == null)
		//		{
		//			_rays = new List<GenRay>();
		//		}
		//		_rays.Clear();

		//		//Ray 체크시 Center의 Filled 여부가 중요하다
		//		RAY_STATUS startStatus = _isEmptyCenter ? RAY_STATUS.Processing_NonChecked : RAY_STATUS.Processing_Checked;

		//		if (_parent == null)
		//		{
		//			//루트라면
		//			//Debug.LogError("Root Rays");

		//			for (int i = 0; i < nRays; i++)
		//			{
		//				//float angle = (((endAngle - startAngle) / (float)(nRays)) * (float)i) + startAngle;
		//				float angle = apUtil.AngleTo360(((float)i / (float)nRays) * 360.0f);
		//				_rays.Add(new GenRay(this, angle, startStatus));
		//			}
		//		}
		//		else
		//		{
		//			//자식이라면
		//			float startAngle = _angleFromParent - 90.0f;
		//			float endAngle = _angleFromParent + 90.0f;

		//			for (int i = 0; i <= nRays; i++)//자식인 경우에는 nRay + 1만큼 만든다.
		//			{
		//				//float angle = (((endAngle - startAngle) / (float)(nRays)) * (float)i) + startAngle;
		//				float angle = (((float)i / (float)nRays) * (endAngle - startAngle)) + startAngle;

		//				angle = apUtil.AngleTo360(angle);

		//				_rays.Add(new GenRay(this, angle, startStatus));
		//			}
		//		}

		//		_nRays = _rays.Count;

		//	}

		//	public void SetTerminated(float dist_Outer, float dist_Inner)
		//	{
		//		_radius_Min_Outer = dist_Outer;
		//		_radius_Min_Inner = dist_Inner;
		//	}

		//	public void SetStatus(CALCULATE_STATUS calculateStatus)
		//	{
		//		_calculateResult = calculateStatus;
		//	}

		//	/// <summary>
		//	/// 현재 Circle의 부모부터 재귀적으로 올라가면서 원 안에 해당 점이 포함되는지 체크한다.
		//	/// 비교는 Inner Radius로 한다.
		//	/// </summary>
		//	/// <param name="pos"></param>
		//	/// <returns></returns>
		//	public bool IsPointInParentCircles(Vector2 pos)
		//	{
		//		if(_parent == null)
		//		{
		//			return false;
		//		}

		//		GenCircleArea curParent = _parent;
		//		while(true)
		//		{
		//			float distToCenter = Vector2.Distance(pos, curParent._pos);
		//			//if(distToCenter < curParent._radius_Min_Inner)
		//			if(distToCenter < curParent._radius_Min_Outer)
		//			{
		//				//점이 안에 포함되었다.
		//				return true;
		//			}

		//			if(curParent._parent == null)
		//			{
		//				//더이상 부모가 없다.
		//				break;
		//			}

		//			//위로 올라간다.
		//			curParent = curParent._parent;
		//		}

		//		return false;
		//	}


		//	public bool IsPointIn(Vector2 pos)
		//	{
		//		float distToCenter = Vector2.Distance(pos, _pos);
		//		//return (distToCenter < _radius_Min_Inner);
		//		//return (distToCenter < _radius_Min_Outer);
		//		if (_parent == null)
		//		{
		//			return (distToCenter < _radius_Min_Outer);
		//		}
		//		else
		//		{
		//			//루트가 아니라면
		//			float angleToPos = Vector2.Angle(pos - _pos, _direction);
		//			return (distToCenter < _radius_Min_Outer) && angleToPos < 90.0f;
		//		}

		//	}
		//}


		//public enum RAY_STATUS
		//{
		//	None,
		//	/// <summary>처리 중 : 시작시 Empty인 경우</summary>
		//	Processing_NonChecked,
		//	/// <summary>처리 중 : 시작시 Checked인 경우</summary>
		//	Processing_Checked,
		//	/// <summary>끝점을 찾아서 종료되었다. > GenVert가 생성된다.</summary>
		//	Terminated,
		//	/// <summary>끝점은 아니며, > ChildCircle이 생성된다.</summary>
		//	MakeChildCircle,
		//	/// <summary>이 Ray는 유효하지 않다. 다른 Circle나 Vertex가 생성되지 않는다.</summary>
		//	Failed,
		//}




		///// <summary>
		///// 원형으로의 방사형 체크를 위한 클래스
		///// </summary>
		//private class GenRay
		//{
		//	public GenCircleArea _circle = null;
		//	public float _angle = 0.0f;
		//	public Vector2 _dir = Vector2.zero;

		//	public RAY_STATUS _status = RAY_STATUS.None;

		//	//결과에 따른 생성물
		//	//Terminated라면
		//	public GenVertex _vert_Outer = null;
		//	//public GenVertex _vert_Inner = null;

		//	//MakeChildCircle라면
		//	public GenCircleArea _childCircle = null;

		//	//Failed라면.. 쩝

		//	public GenRay(GenCircleArea circle, float angle, RAY_STATUS startProcess)
		//	{
		//		_circle = circle;
		//		_angle = angle;
		//		_dir = new Vector2(Mathf.Cos(_angle * Mathf.Deg2Rad), Mathf.Sin(_angle * Mathf.Deg2Rad));

		//		_status = startProcess;

		//		_vert_Outer = null;
		//		//_vert_Inner = null;
		//		_childCircle = null;
		//	}

		//	public void SetTerminated(float dist_Outer
		//		//, float dist_Inner
		//		)
		//	{
		//		_status = RAY_STATUS.Terminated;

		//		//Vertex도 만들어야 한다.
		//		_vert_Outer = new GenVertex(_circle, _circle._pos + (dist_Outer * _dir), VERT_TYPE.Outline, _circle._groupID);
		//		//_vert_Inner = new GenVertex(_circle, _circle._pos + dist_Inner * _dir, VERT_TYPE.Ray_Inner);

		//		_vert_Outer.SetRayVert(this);
		//		//_vert_Outer.SetRayVert(this, _vert_Inner);
		//		//_vert_Inner.SetRayVert(this, _vert_Outer);
		//	}

		//	public void SetFailed()
		//	{
		//		_status = RAY_STATUS.Failed;
		//	}

		//	public GenCircleArea SetMakeChildCircle(float dist, bool isCenterFilled)
		//	{
		//		_status = RAY_STATUS.MakeChildCircle;

		//		_childCircle = new GenCircleArea(_circle._pos + (dist * _dir), _circle._groupID);
		//		_childCircle.SetParent(_circle, _angle, _dir);//부모-자식 연결
		//		_childCircle._isEmptyCenter = !isCenterFilled;//중심이 비어있는가

		//		return _childCircle;
		//	}
		//}




		///// <summary>
		///// Root Circle을 병합할 때 사용되는 클래스
		///// </summary>
		//private class GenRootCircleGroup
		//{
		//	public List<GenCircleArea> _rootCircles = null;
		//	public GenRootCircleGroup()
		//	{
		//		_rootCircles = new List<GenCircleArea>();
		//	}

		//	public void AddCircle(GenCircleArea circle)
		//	{
		//		_rootCircles.Add(circle);
		//	}

		//	public void MergeGroup(GenRootCircleGroup otherGroup)
		//	{
		//		//다른 그룹을 여기에 병합한다.
		//		List<GenCircleArea> otherCircles = otherGroup._rootCircles;
		//		int nOtherCircles = otherCircles.Count;

		//		for (int i = 0; i < otherCircles.Count; i++)
		//		{
		//			_rootCircles.Add(otherCircles[i]);
		//		}
		//	}

		//	//가장 중심에 있는 Circle을 구하자
		//	public GenCircleArea GetCenterCircle()
		//	{
		//		GenCircleArea curCircle = null;
		//		int nCircles = _rootCircles.Count;

		//		//일단 중점을 구하자
		//		Vector2 centerPos = Vector2.zero;
		//		for (int i = 0; i < nCircles; i++)
		//		{
		//			centerPos += _rootCircles[i]._pos;
		//		}
		//		centerPos /= nCircles;

		//		//이제 중점에 가장 가까운 점을 가져오자
		//		GenCircleArea nearestCircle = null;
		//		float minDist = 0.0f;
		//		float dist = 0.0f;
		//		for (int i = 0; i < nCircles; i++)
		//		{
		//			curCircle = _rootCircles[i];
		//			dist = Vector2.SqrMagnitude(curCircle._pos - centerPos);
		//			if(nearestCircle == null || dist < minDist)
		//			{
		//				nearestCircle = curCircle;
		//				minDist = dist;
		//			}
		//		}

		//		return nearestCircle;
		//	}
		//} 
		#endregion

		#region [미사용 코드] 이 그리드 방식은 복잡해서 사용하지 않음 (Circle / Linear 방식에 적합)

		///// <summary>
		///// 그리드 방식으로 정의된 버텍스 리스트
		///// 설정에 따라서 그리드의 크기, 저장 방식 등이 바뀐다.		
		///// </summary>
		//private class GenGridVerts
		//{
		//	//Members
		//	//키값 순서 : GridX, GrixY, GroupID > Vertices
		//	private Dictionary<int, Dictionary<int, Dictionary<int, List<GenVertex>>>> _vertGrid = null;
		//	private float _gridSize = 0.0f;
		//	private int _iInitGridX_Min = 0;
		//	private int _iInitGridY_Min = 0;
		//	private int _iInitGridX_Max = 0;
		//	private int _iInitGridY_Max = 0;
		//	private int _initMaxGroupID = 0;

		//	public enum STORE_TYPE
		//	{
		//		StoreToOneGrid,//1개의 버텍스가 한칸에만 저장된다.
		//		StoreToAdjacentGrid,//1개의 버텍스가 인접한 칸에도 같이 저장된다.
		//	}
		//	private STORE_TYPE _storeType = STORE_TYPE.StoreToOneGrid;


		//	public Dictionary<int, Dictionary<int, Dictionary<int, List<GenVertex>>>> Grid { get { return _vertGrid; } }

		//	// Init
		//	public GenGridVerts()
		//	{
		//		Clear();
		//	}

		//	public void Clear()
		//	{
		//		if(_vertGrid != null)
		//		{
		//			_vertGrid.Clear();
		//		}
		//		_vertGrid = null;
		//		_gridSize = 0.0f;
		//		_iInitGridX_Min = 0;
		//		_iInitGridY_Min = 0;
		//		_iInitGridX_Max = 0;
		//		_iInitGridY_Max = 0;
		//		_storeType = STORE_TYPE.StoreToOneGrid;
		//	}

		//	public void Init(float gridSize, STORE_TYPE storeType, Vector2 initMinPos, Vector2 initMaxPos, int maxGroupID)
		//	{
		//		Clear();

		//		_gridSize = gridSize;
		//		_storeType = storeType;

		//		//초기값의 그리드를 정의한다. 예상되는 범위를 먼저 지정하면, 조회를 빠르게 할 수 있다.
		//		_iInitGridX_Min = (int)(initMinPos.x / gridSize) - 1;
		//		_iInitGridX_Max = (int)(initMaxPos.x / gridSize) + 1;

		//		_iInitGridY_Min = (int)(initMinPos.y / gridSize) - 1;				
		//		_iInitGridY_Max = (int)(initMaxPos.y / gridSize) + 1;

		//		_initMaxGroupID = maxGroupID;
		//		if(_initMaxGroupID < 0)
		//		{
		//			_initMaxGroupID = 0;
		//		}

		//		_vertGrid = new Dictionary<int, Dictionary<int, Dictionary<int, List<GenVertex>>>>();

		//		//초기 사각형 그리드를 생성한다.
		//		//이 범위 내에서는 배열처럼 사용하자
		//		Dictionary<int, Dictionary<int, List<GenVertex>>> curGridYs = null;
		//		Dictionary<int, List<GenVertex>> curGridGroupIDs = null;

		//		for (int iGX = _iInitGridX_Min; iGX <= _iInitGridX_Max; iGX++)
		//		{
		//			curGridYs = new Dictionary<int, Dictionary<int, List<GenVertex>>>();
		//			_vertGrid.Add(iGX, curGridYs);

		//			for (int iGY = _iInitGridY_Min; iGY <= _iInitGridY_Max; iGY++)
		//			{
		//				curGridGroupIDs = new Dictionary<int, List<GenVertex>>();
		//				curGridYs.Add(iGY, curGridGroupIDs);

		//				for (int iID = 0; iID <= _initMaxGroupID; iID++)
		//				{
		//					curGridGroupIDs.Add(iID, new List<GenVertex>());
		//				}
		//			}
		//		}
		//	}

		//	// Add
		//	public void AddVertex(GenVertex vert)
		//	{
		//		int iGX = (int)(vert._pos.x / _gridSize);
		//		int iGY = (int)(vert._pos.y / _gridSize);
		//		int iGID = vert._groupID;

		//		Dictionary<int, Dictionary<int, List<GenVertex>>> curGYs = null;
		//		Dictionary<int, List<GenVertex>> curGIDs = null;
		//		if (_storeType == STORE_TYPE.StoreToOneGrid)
		//		{
		//			//밖에 있다면 직접 만들자
		//			if (iGX < _iInitGridX_Min
		//				|| iGX > _iInitGridX_Max
		//				|| iGY < _iInitGridY_Min
		//				|| iGY > _iInitGridY_Max
		//				|| iGID > _initMaxGroupID)
		//			{
		//				if (!_vertGrid.ContainsKey(iGX))
		//				{
		//					_vertGrid.Add(iGX, new Dictionary<int, Dictionary<int, List<GenVertex>>>());
		//				}
		//				curGYs = _vertGrid[iGX];

		//				if (!curGYs.ContainsKey(iGY))
		//				{
		//					curGYs.Add(iGY, new Dictionary<int, List<GenVertex>>());
		//				}
		//				curGIDs = curGYs[iGY];

		//				if (!curGIDs.ContainsKey(iGID))
		//				{
		//					curGIDs.Add(iGID, new List<GenVertex>());
		//				}

		//				curGIDs[iGID].Add(vert);
		//			}
		//			else
		//			{
		//				((_vertGrid[iGX])[iGY])[iGID].Add(vert);
		//			}

		//		}
		//		else
		//		{
		//			//인접한 그리드에도 저장해야한다.
		//			for (int iGXadj = iGX - 1; iGXadj <= iGX + 1; iGXadj++)
		//			{
		//				for (int iGYadj = iGY - 1; iGYadj <= iGY + 1; iGYadj++)
		//				{
		//					if (iGXadj < _iInitGridX_Min
		//						|| iGXadj > _iInitGridX_Max
		//						|| iGYadj < _iInitGridY_Min
		//						|| iGYadj > _iInitGridY_Max
		//						|| iGID > _initMaxGroupID)
		//					{
		//						if (!_vertGrid.ContainsKey(iGXadj))
		//						{
		//							_vertGrid.Add(iGXadj, new Dictionary<int, Dictionary<int, List<GenVertex>>>());
		//						}
		//						curGYs = _vertGrid[iGXadj];

		//						if (!curGYs.ContainsKey(iGYadj))
		//						{
		//							curGYs.Add(iGYadj, new Dictionary<int, List<GenVertex>>());
		//						}

		//						curGIDs = curGYs[iGYadj];

		//						if (!curGIDs.ContainsKey(iGID))
		//						{
		//							curGIDs.Add(iGID, new List<GenVertex>());
		//						}
		//						curGIDs[iGID].Add(vert);
		//					}
		//					else
		//					{
		//						((_vertGrid[iGXadj])[iGYadj])[iGID].Add(vert);
		//					}
		//				}
		//			}
		//		}
		//	}



		//	// Get
		//	public Dictionary<int, List<GenVertex>> GetVertices(Vector2 targetPos)
		//	{
		//		int iGX = (int)(targetPos.x / _gridSize);
		//		int iGY = (int)(targetPos.y / _gridSize);

		//		if (_iInitGridX_Min <= iGX && iGX <= _iInitGridX_Max
		//			&& _iInitGridY_Min <= iGY && iGY <= _iInitGridY_Max)
		//		{
		//			//초기 범위면 바로 호출
		//			return (_vertGrid[iGX])[iGY];
		//		}

		//		//그 외에는 해당 그리드가 있는지 체크
		//		if(!_vertGrid.ContainsKey(iGX))
		//		{
		//			return null;
		//		}
		//		if(!_vertGrid[iGX].ContainsKey(iGY))
		//		{
		//			return null;
		//		}

		//		return (_vertGrid[iGX])[iGY];
		//	}


		//	public List<GenVertex> GetVerticesPerGroupID(Vector2 targetPos, int groupID)
		//	{
		//		int iGX = (int)(targetPos.x / _gridSize);
		//		int iGY = (int)(targetPos.y / _gridSize);

		//		if (_iInitGridX_Min <= iGX && iGX <= _iInitGridX_Max
		//			&& _iInitGridY_Min <= iGY && iGY <= _iInitGridY_Max
		//			&& groupID <= _initMaxGroupID)
		//		{
		//			//초기 범위면 바로 호출
		//			return ((_vertGrid[iGX])[iGY])[groupID];
		//		}

		//		//그 외에는 해당 그리드가 있는지 체크
		//		if(!_vertGrid.ContainsKey(iGX))
		//		{
		//			return null;
		//		}
		//		if(!_vertGrid[iGX].ContainsKey(iGY))
		//		{
		//			return null;
		//		}

		//		if(!(_vertGrid[iGX])[iGY].ContainsKey(groupID))
		//		{
		//			return null;
		//		}

		//		return ((_vertGrid[iGX])[iGY])[groupID];
		//	}


		//	/// <summary>
		//	/// 그리드에서 동일한 타입+가장 가까운 버텍스를 가져온다. 없으면 null
		//	/// </summary>
		//	public GenVertex FindNearestSimilarVertex(GenVertex vertex, float mergeRange)
		//	{
		//		Dictionary<int, List<GenVertex>> groupID2vertices = GetVertices(vertex._pos);

		//		if (groupID2vertices == null)
		//		{
		//			return null;
		//		}

		//		int groupID = vertex._groupID;

		//		if(!groupID2vertices.ContainsKey(groupID))
		//		{
		//			return null;
		//		}

		//		List<GenVertex> vertices = groupID2vertices[groupID];
		//		if(vertices == null || vertices.Count == 0)
		//		{
		//			return null;
		//		}

		//		//가깝고, 같은 타입의 버텍스여야 한다.
		//		//각도도 서로 같은 방향.. 최소한 예각이어야 한다.
		//		GenVertex curVert = null;
		//		float sqrtDist = 0.0f;
		//		float sqrtRange = mergeRange * mergeRange;
		//		float dotVector = 0.0f;

		//		for (int i = 0; i < vertices.Count; i++)
		//		{
		//			curVert = vertices[i];
		//			if(curVert == vertex)
		//			{
		//				continue;
		//			}

		//			if (curVert._vertType != vertex._vertType)
		//			{
		//				//타입이 다르다.
		//				continue;
		//			}

		//			sqrtDist = ((curVert._pos.x - vertex._pos.x) * (curVert._pos.x - vertex._pos.x))
		//				+ ((curVert._pos.y - vertex._pos.y) * (curVert._pos.y - vertex._pos.y));

		//			dotVector = (curVert._dir.x * vertex._dir.x) + (curVert._dir.y * vertex._dir.y);
		//			if (sqrtDist < sqrtRange && dotVector > 0.5f)
		//			{
		//				//병합할 수 있다.
		//				return curVert;
		//			}
		//		}
		//		//병합할 수 없다.
		//		return null;
		//	}

		//} 
		#endregion

		private class GenSimpleVertGrid
		{
			public Dictionary<int, Dictionary<int, List<GenVertex>>> _grid = null;
			
			public float _offsetX = 0.0f;
			public float _offsetY = 0.0f;
			public float _gridSize = 0.0f;
			public int _nVerts = 0;

			public GenSimpleVertGrid(float gridSize, float offsetX, float offsetY)
			{
				_gridSize = gridSize;
				_offsetX = offsetX;
				_offsetY = offsetY;
				_grid = new Dictionary<int, Dictionary<int, List<GenVertex>>>();
				_nVerts = 0;
			}

			public void AddVert(GenVertex vert)
			{
				int iX = (int)((vert._pos.x - _offsetX) / _gridSize);
				int iY = (int)((vert._pos.y - _offsetY) / _gridSize);

				if(!_grid.ContainsKey(iX))
				{
					_grid.Add(iX, new Dictionary<int, List<GenVertex>>());
				}
				if(!_grid[iX].ContainsKey(iY))
				{
					(_grid[iX]).Add(iY, new List<GenVertex>());
				}
				(_grid[iX])[iY].Add(vert);
				_nVerts++;
			}

			
		}
	}
}