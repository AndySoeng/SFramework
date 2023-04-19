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
	public class apSnapShot_ModifiedMesh : apSnapShotBase
	{
		// Members
		//--------------------------------------------
		//키값은 바뀌지 않는다.
		private apMeshGroup _key_MeshGroupOfMod = null;
		private apMeshGroup _key_MeshGroupOfTransform = null;
		private apTransform_Mesh _key_MeshTransform = null;
		private apTransform_MeshGroup _key_MeshGroupTransform = null;
		private apRenderUnit _key_RenderUnit = null;

		//저장되는 멤버 데이터
		public class VertData
		{
			public apVertex _key_Vert = null;
			public Vector2 _deltaPos = Vector2.zero;

			public VertData(apVertex key_Vert, Vector2 deltaPos)
			{
				_key_Vert = key_Vert;
				_deltaPos = deltaPos;
			}
		}

		public class PinData
		{
			public apMeshPin _key_Pin = null;
			public Vector2 _deltaPos = Vector2.zero;

			public PinData(apMeshPin key_Pin, Vector2 deltaPos)
			{
				_key_Pin = key_Pin;
				_deltaPos = deltaPos;
			}
		}


		private List<VertData> _vertices = new List<VertData>();
		private List<PinData> _pins = new List<PinData>();

		private apMatrix _transformMatrix = new apMatrix();
		private Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private bool _isVisible = true;

		//추가 3.29 : ExtraOption도 저장하자
		public class ExtraDummyValue
		{
			public bool _isDepthChanged = false;
			public int _deltaDepth = 0;

			public bool _isTextureChanged = false;
			public apTextureData _linkedTextureData = null;

			public int _textureDataID = -1;

			public float _weightCutout = 0.5f;
			public float _weightCutout_AnimPrev = 0.5f;
			public float _weightCutout_AnimNext = 0.6f;

			public ExtraDummyValue(apModifiedMesh.ExtraValue srcValue)
			{
				_isDepthChanged = srcValue._isDepthChanged;
				_deltaDepth = srcValue._deltaDepth;

				_isTextureChanged = srcValue._isTextureChanged;
				_linkedTextureData = srcValue._linkedTextureData;

				_textureDataID = srcValue._textureDataID;

				_weightCutout = srcValue._weightCutout;
				_weightCutout_AnimPrev = srcValue._weightCutout_AnimPrev;
				_weightCutout_AnimNext = srcValue._weightCutout_AnimNext;
			}

			public ExtraDummyValue(ExtraDummyValue srcValue)
			{
				_isDepthChanged = srcValue._isDepthChanged;
				_deltaDepth = srcValue._deltaDepth;

				_isTextureChanged = srcValue._isTextureChanged;
				_linkedTextureData = srcValue._linkedTextureData;

				_textureDataID = srcValue._textureDataID;

				_weightCutout = srcValue._weightCutout;
				_weightCutout_AnimPrev = srcValue._weightCutout_AnimPrev;
				_weightCutout_AnimNext = srcValue._weightCutout_AnimNext;
			}
		}

		private bool _isExtraValueEnabled = false;
		private ExtraDummyValue _extraValue = null;

		// Init
		//--------------------------------------------
		public apSnapShot_ModifiedMesh() : base()
		{

		}



		// Functions
		//--------------------------------------------
		public override void Clear()
		{
			base.Clear();

			_key_MeshGroupOfMod = null;
			_key_MeshGroupOfTransform = null;
			_key_MeshTransform = null;
			_key_MeshGroupTransform = null;
			_key_RenderUnit = null;

			if(_vertices == null)
			{
				_vertices = new List<VertData>();
			}
			_vertices.Clear();

			if(_pins == null)
			{
				_pins = new List<PinData>();
			}
			_pins.Clear();

			if (_transformMatrix == null)
			{
				_transformMatrix = new apMatrix();
			}
			_transformMatrix.SetIdentity();
			_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_isVisible = true;

			_isExtraValueEnabled = false;
			_extraValue = null;
		}

		public override bool IsKeySyncable(object target)
		{
			//return base.IsKeySyncable(target);
			if (!(target is apModifiedMesh))
			{
				return false;
			}

			apModifiedMesh targetModMesh = target as apModifiedMesh;
			if (targetModMesh == null)
			{
				return false;
			}

			//Key들이 모두 같아야 한다.
			if (targetModMesh._meshGroupOfModifier != _key_MeshGroupOfMod)
			{
				return false;
			}

			if (targetModMesh._meshGroupOfTransform != _key_MeshGroupOfTransform)
			{
				return false;
			}

			if (targetModMesh._transform_MeshGroup != null)
			{
				if (targetModMesh._transform_MeshGroup != _key_MeshGroupTransform)
				{
					return false;
				}
			}
			if (targetModMesh._transform_Mesh != null)
			{
				if (targetModMesh._transform_Mesh != _key_MeshTransform)
				{
					return false;
				}
			}
			if (targetModMesh._renderUnit != _key_RenderUnit)
			{
				return false;
			}

			return true;
		}


		//추가 21.3.19 : Morph에서는 메시만 맞으면 된다.
		public override bool IsKeySyncable_MorphMod(object target)
		{
			if (!(target is apModifiedMesh))
			{
				return false;
			}

			apModifiedMesh targetModMesh = target as apModifiedMesh;
			if (targetModMesh == null)
			{
				return false;
			}

			//Key들이 모두 같아야 한다. > 메시만 맞으면 된다.
			//단, 메시가 있어야 한다.
			//메시 그룹 타입이면 그냥 복사 가능
			if(_key_MeshTransform != null && targetModMesh._transform_Mesh != null)
			{
				//메시인 경우, apMesh가 같아야 한다.
				if(_key_MeshTransform._mesh != null
					&& targetModMesh._transform_Mesh._mesh != null
					&& _key_MeshTransform._mesh == targetModMesh._transform_Mesh._mesh)
				{
					return true;
				}
			}
			else if(_key_MeshGroupTransform != null && targetModMesh._transform_MeshGroup != null)
			{
				//둘다 MeshGroupTF라면 복사 가능
				return true;
			}

			//그 외에는 복사 불가
			return false;
		}


		//추가 21.3.19 : TF에서는 그냥 메시 타입이면 아무렇게나 복사할 수 있다. 조건이 많이 완화됨
		public override bool IsKeySyncable_TFMod(object target)
		{
			//return base.IsKeySyncable(target);
			if (!(target is apModifiedMesh))
			{
				return false;
			}

			apModifiedMesh targetModMesh = target as apModifiedMesh;
			if (targetModMesh == null)
			{
				return false;
			}

			//키, 타겟 둘다 MeshTF / MeshGroupTF 중 하나라도 있으면 된다.
			if(
				(_key_MeshTransform != null || _key_MeshGroupTransform != null)
				&& (targetModMesh._transform_Mesh != null || targetModMesh._transform_MeshGroup != null)
				)
			{
				return true;
			}

			//그 외에는 안됨
			return false;
		}









		public override bool Save(object target, string strParam)
		{
			base.Save(target, strParam);

			apModifiedMesh modMesh = target as apModifiedMesh;
			if (modMesh == null)
			{
				return false;
			}

			_key_MeshGroupOfMod = modMesh._meshGroupOfModifier;
			_key_MeshGroupOfTransform = modMesh._meshGroupOfTransform;

			_key_MeshTransform = null;
			_key_MeshGroupTransform = null;
			_key_RenderUnit = null;

			if (modMesh._transform_Mesh != null)
			{
				_key_MeshTransform = modMesh._transform_Mesh;
			}
			if (modMesh._transform_MeshGroup != null)
			{
				_key_MeshGroupTransform = modMesh._transform_MeshGroup;
			}
			_key_RenderUnit = modMesh._renderUnit;

			_vertices.Clear();
			int nVert = modMesh._vertices.Count;

			if(nVert > 0)
			{
				apModifiedVertex modVert = null;
				for (int i = 0; i < nVert; i++)
				{
					modVert = modMesh._vertices[i];
					_vertices.Add(new VertData(modVert._vertex, modVert._deltaPos));
				}
			}
			

			//추가 22.5.4 [v1.4.0]
			_pins.Clear();
			int nPins = modMesh._pins != null ? modMesh._pins.Count : 0;
			if(nPins > 0)
			{
				apModifiedPin modPin = null;
				for (int i = 0; i < nPins; i++)
				{
					modPin = modMesh._pins[i];
					_pins.Add(new PinData(modPin._pin, modPin._deltaPos));
				}
			}


			_transformMatrix = new apMatrix(modMesh._transformMatrix);
			_meshColor = modMesh._meshColor;
			_isVisible = modMesh._isVisible;


			_isExtraValueEnabled = false;
			_extraValue = null;

			//추가 3.29 : ExtraValue도 복사
			if(modMesh._isExtraValueEnabled)
			{
				_isExtraValueEnabled = true;
				_extraValue = new ExtraDummyValue(modMesh._extraValue);
			}

			return true;
		}


		public override bool Load(object targetObj)
		{
			apModifiedMesh modMesh = targetObj as apModifiedMesh;
			if (modMesh == null)
			{
				return false;
			}

			int nVert = _vertices.Count;
			bool isDifferentVertAndPin = false;

			//만약 하나라도 Vert가 변경된게 있으면 좀 오래 걸리는 로직으로 바뀌어야 한다.
			//미리 체크해주자
			if (modMesh._vertices.Count != nVert)
			{
				isDifferentVertAndPin = true;
			}
			else
			{
				for (int i = 0; i < nVert; i++)
				{
					if (_vertices[i]._key_Vert != modMesh._vertices[i]._vertex)
					{
						isDifferentVertAndPin = true;
						break;
					}
				}
			}

			//추가 22.5.4 : 핀도 동일한 처리
			int nPins = _pins.Count;
			int nModPins = modMesh._pins != null ? modMesh._pins.Count : 0;

			if (!isDifferentVertAndPin && nModPins > 0 && nPins > 0)
			{
				if (modMesh._pins.Count != nPins)
				{
					isDifferentVertAndPin = true;
				}
				else
				{
					for (int i = 0; i < nPins; i++)
					{
						if (_pins[i]._key_Pin != modMesh._pins[i]._pin)
						{
							isDifferentVertAndPin = true;
							break;
						}
					}
				}
			}


			if (isDifferentVertAndPin)
			{
				//1. 만약 Vertex 구성이 다르면
				//매번 Find로 찾아서 매칭해야한다.
				VertData vertData = null;
				apModifiedVertex modVert = null;
				for (int i = 0; i < nVert; i++)
				{
					vertData = _vertices[i];
					modVert = modMesh._vertices.Find(delegate (apModifiedVertex a)
					{
						return a._vertex == vertData._key_Vert;
					});

					if (modVert != null)
					{
						modVert._deltaPos = vertData._deltaPos;
					}
				}

				//2. Pin도 마찬가지로 적용 [v1.4.0]
				if (nModPins > 0 && nPins > 0)
				{
					PinData pinData = null;
					apModifiedPin modPin = null;

					for (int i = 0; i < nPins; i++)
					{
						pinData = _pins[i];
						modPin = modMesh._pins.Find(delegate (apModifiedPin a)
						{
							return a._pin == pinData._key_Pin;
						});

						if(modPin != null)
						{
							modPin._deltaPos = pinData._deltaPos;
						}
					}
						
				}
				
			}
			else
			{
				//2. Vertex 구성이 같으면
				// 그냥 For 돌면서 넣어주자
				VertData vertData = null;
				apModifiedVertex modVert = null;
				for (int i = 0; i < nVert; i++)
				{
					vertData = _vertices[i];
					modVert = modMesh._vertices[i];

					modVert._deltaPos = vertData._deltaPos;
				}

				//2. Pin도 마찬가지로 적용 [v1.4.0]
				if (nModPins > 0 && nPins > 0)
				{
					PinData pinData = null;
					apModifiedPin modPin = null;

					for (int i = 0; i < nPins; i++)
					{
						pinData = _pins[i];
						modPin = modMesh._pins[i];
						modPin._deltaPos = pinData._deltaPos;
					}
						
				}
			}

			modMesh._transformMatrix = new apMatrix(_transformMatrix);
			modMesh._meshColor = _meshColor;
			modMesh._isVisible = _isVisible;


			//추가 3.29 : ExtraProperty도 복사
			modMesh._isExtraValueEnabled = _isExtraValueEnabled;
			if(modMesh._extraValue == null)
			{
				modMesh._extraValue = new apModifiedMesh.ExtraValue();
				modMesh._extraValue.Init();
			}
			if(_isExtraValueEnabled)
			{
				if(_extraValue != null)
				{
					modMesh._extraValue._isDepthChanged = _extraValue._isDepthChanged;
					modMesh._extraValue._deltaDepth = _extraValue._deltaDepth;
					modMesh._extraValue._isTextureChanged = _extraValue._isTextureChanged;
					modMesh._extraValue._linkedTextureData = _extraValue._linkedTextureData;
					modMesh._extraValue._textureDataID = _extraValue._textureDataID;
					modMesh._extraValue._weightCutout = _extraValue._weightCutout;
					modMesh._extraValue._weightCutout_AnimPrev = _extraValue._weightCutout_AnimPrev;
					modMesh._extraValue._weightCutout_AnimNext = _extraValue._weightCutout_AnimNext;
				}
			}
			else
			{
				modMesh._extraValue.Init();
			}


			return true;
		}




		//그냥 붙여넣는게 아니라, 특정 속성만 붙여넣는 경우
		public override bool LoadWithProperties(object targetObj,
													bool isVerts,
													bool isPins,
													bool isTransform,
													bool isVisibility,
													bool isColor,
													bool isExtra,
													bool isSelectedOnly,
													List<apModifiedVertex> selectedModVerts,
													List<apModifiedPin> selectedModPins)
		{
			apModifiedMesh modMesh = targetObj as apModifiedMesh;
			if (modMesh == null)
			{
				return false;
			}


			//버텍스나 핀을 붙여넣기 하는 경우
			if (isVerts || isPins)
			{

				int nVert = _vertices != null ? _vertices.Count : 0;
				bool isDifferentVertAndPin = false;

				//만약 하나라도 Vert가 변경된게 있으면 좀 오래 걸리는 로직으로 바뀌어야 한다.
				//미리 체크해주자
				if (modMesh._vertices.Count != nVert)
				{
					isDifferentVertAndPin = true;
				}
				else
				{
					for (int i = 0; i < nVert; i++)
					{
						if (_vertices[i]._key_Vert != modMesh._vertices[i]._vertex)
						{
							isDifferentVertAndPin = true;
							break;
						}
					}
				}

				//추가 22.5.4 : 핀도 동일한 처리
				int nPins = _pins != null ? _pins.Count : 0;
				int nModPins = modMesh._pins != null ? modMesh._pins.Count : 0;

				if (!isDifferentVertAndPin && nModPins > 0 && nPins > 0)
				{
					if (modMesh._pins.Count != nPins)
					{
						isDifferentVertAndPin = true;
					}
					else
					{
						for (int i = 0; i < nPins; i++)
						{
							if (_pins[i]._key_Pin != modMesh._pins[i]._pin)
							{
								isDifferentVertAndPin = true;
								break;
							}
						}
					}
				}


				if (isDifferentVertAndPin)
				{
					if (isVerts)
					{
						//1. 만약 Vertex 구성이 다르면
						//매번 Find로 찾아서 매칭해야한다.
						VertData vertData = null;
						apModifiedVertex modVert = null;
						for (int i = 0; i < nVert; i++)
						{
							vertData = _vertices[i];
							modVert = modMesh._vertices.Find(delegate (apModifiedVertex a)
							{
								return a._vertex == vertData._key_Vert;
							});

							if (modVert != null)
							{
								if (isSelectedOnly)
								{
									//선택된 버텍스만 붙여넣기를 하는 경우
									if (selectedModVerts != null && selectedModVerts.Contains(modVert))
									{
										modVert._deltaPos = vertData._deltaPos;
									}
								}
								else
								{
									//모든 버텍스들을 붙여넣기 하는 경우
									modVert._deltaPos = vertData._deltaPos;
								}
							}
						}
					}

					if (isPins)
					{
						//2. Pin도 마찬가지로 적용 [v1.4.0]
						if (nModPins > 0 && nPins > 0)
						{
							PinData pinData = null;
							apModifiedPin modPin = null;

							for (int i = 0; i < nPins; i++)
							{
								pinData = _pins[i];
								modPin = modMesh._pins.Find(delegate (apModifiedPin a)
								{
									return a._pin == pinData._key_Pin;
								});

								if (modPin != null)
								{
									if (isSelectedOnly)
									{
										//선택된 핀만 붙여넣기를 하는 경우
										if (selectedModPins != null && selectedModPins.Contains(modPin))
										{
											modPin._deltaPos = pinData._deltaPos;
										}
									}
									else
									{
										//모든 핀들을 붙여넣기 하는 경우
										modPin._deltaPos = pinData._deltaPos;
									}
								}
							}
						}
					}
				}
				else
				{
					if (isVerts)
					{
						//2. Vertex 구성이 같으면
						// 그냥 For 돌면서 넣어주자
						VertData vertData = null;
						apModifiedVertex modVert = null;
						for (int i = 0; i < nVert; i++)
						{
							vertData = _vertices[i];
							modVert = modMesh._vertices[i];

							if (isSelectedOnly)
							{
								//선택된 버텍스만 붙여넣기를 하는 경우
								if (selectedModVerts != null && selectedModVerts.Contains(modVert))
								{
									modVert._deltaPos = vertData._deltaPos;
								}
							}
							else
							{
								//모든 버텍스들을 붙여넣기 하는 경우
								modVert._deltaPos = vertData._deltaPos;
							}
						}
					}

					if (isPins)
					{
						//2. Pin도 마찬가지로 적용 [v1.4.0]
						if (nModPins > 0 && nPins > 0)
						{
							PinData pinData = null;
							apModifiedPin modPin = null;

							for (int i = 0; i < nPins; i++)
							{
								pinData = _pins[i];
								modPin = modMesh._pins[i];

								if (modPin != null)
								{
									if (isSelectedOnly)
									{
										//선택된 핀만 붙여넣기를 하는 경우
										if (selectedModPins != null && selectedModPins.Contains(modPin))
										{
											modPin._deltaPos = pinData._deltaPos;
										}
									}
									else
									{
										//모든 핀들을 붙여넣기 하는 경우
										modPin._deltaPos = pinData._deltaPos;
									}
								}
							}
						}
					}
				}
			}

			//각각의 속성에 맞게 붙여넣기를 하자
			if (isTransform)
			{
				modMesh._transformMatrix = new apMatrix(_transformMatrix);
			}

			if(isColor)
			{
				modMesh._meshColor = _meshColor;
			}

			if(isVisibility)
			{
				modMesh._isVisible = _isVisible;
			}

			if (isExtra)
			{
				//추가 3.29 : ExtraProperty도 복사
				modMesh._isExtraValueEnabled = _isExtraValueEnabled;
				if (modMesh._extraValue == null)
				{
					modMesh._extraValue = new apModifiedMesh.ExtraValue();
					modMesh._extraValue.Init();
				}
				if (_isExtraValueEnabled)
				{
					if (_extraValue != null)
					{
						modMesh._extraValue._isDepthChanged = _extraValue._isDepthChanged;
						modMesh._extraValue._deltaDepth = _extraValue._deltaDepth;
						modMesh._extraValue._isTextureChanged = _extraValue._isTextureChanged;
						modMesh._extraValue._linkedTextureData = _extraValue._linkedTextureData;
						modMesh._extraValue._textureDataID = _extraValue._textureDataID;
						modMesh._extraValue._weightCutout = _extraValue._weightCutout;
						modMesh._extraValue._weightCutout_AnimPrev = _extraValue._weightCutout_AnimPrev;
						modMesh._extraValue._weightCutout_AnimNext = _extraValue._weightCutout_AnimNext;
					}
				}
				else
				{
					modMesh._extraValue.Init();
				}
			}

			return true;
		}



		//다중 모드 메시 복사-붙여넣기용
		//-------------------------------------------------------
		/// <summary>
		/// 여러개의 스냅샷을 누적하기 전에 이 함수를 호출하자
		/// </summary>
		public void ReadyToAddMultipleSnapShots(bool isReadyToSum)
		{
			if(_vertices == null)
			{
				_vertices = new List<VertData>();
				_vertices.Clear();
			}

			if(_pins == null)
			{
				_pins = new List<PinData>();
				_pins.Clear();
			}

			if(_transformMatrix == null)
			{
				_transformMatrix = new apMatrix();
			}
			if(isReadyToSum)
			{
				//Sum 방식이라면 > Scale을 곱할 것이므로 Vector2.One이어야 한다.
				_transformMatrix.SetIdentity();
			}
			else
			{
				//Average 방식이라면 > 모두 더해서 나눌 것이므로 Vector2.Zero여야 한다.
				_transformMatrix.SetZero();//누적시켜야 하므로 Zero
			}
			
			_isVisible = false;

			_meshColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

			_isExtraValueEnabled = false;
			_extraValue = null;
		}

		/// <summary>
		/// 다른 SnapShot의 데이터를 누적시키자 (다중 복붙용)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="strParam"></param>
		/// <returns></returns>
		public void AddSnapShot(apSnapShot_ModifiedMesh otherSnapShot, float weight, bool isSumMethod)
		{	
			if(otherSnapShot == null)
			{
				return;
			}
			if(_vertices == null)
			{
				_vertices = new List<VertData>();
				_vertices.Clear();
			}
			
			if(_pins == null)
			{
				_pins = new List<PinData>();
				_pins.Clear();
			}

			//1. 버텍스 데이터
			int nVert = otherSnapShot._vertices != null ? otherSnapShot._vertices.Count : 0;

			if(nVert > 0)
			{
				//누적이기 때문에 순서가 맞지 않을 수도 있다.
				VertData srcVert = null;
				VertData dstVert = null;
				for (int i = 0; i < nVert; i++)
				{
					srcVert = otherSnapShot._vertices[i];
					//dst는 찾아야 한다. 없으면 만듬
					dstVert = _vertices.Find(delegate(VertData a)
					{
						return a._key_Vert == srcVert._key_Vert;
					});
					if(dstVert == null)
					{
						//없으면 추가
						dstVert = new VertData(srcVert._key_Vert, Vector2.zero);
						_vertices.Add(dstVert);
					}

					//값을 누적시키자
					dstVert._deltaPos += srcVert._deltaPos * weight;

				}
			}

			//추가 22.5.4 : 핀 데이터
			int nPin = otherSnapShot._pins != null ? otherSnapShot._pins.Count : 0;
			if(nPin > 0)
			{
				PinData srcPin = null;
				PinData dstPin = null;
				for (int i = 0; i < nPin; i++)
				{
					srcPin = otherSnapShot._pins[i];
					//Dst는 직접 찾자
					dstPin = _pins.Find(delegate(PinData a)
					{
						return a._key_Pin == srcPin._key_Pin;
					});
					if(dstPin == null)
					{
						//없으면 추가
						dstPin = new PinData(srcPin._key_Pin, Vector2.zero);
						_pins.Add(dstPin);
					}

					//값 누적
					dstPin._deltaPos += srcPin._deltaPos * weight;
				}
			}


			//2. 기본 TF 정보들
			if(otherSnapShot._transformMatrix != null)
			{
				_transformMatrix._pos += otherSnapShot._transformMatrix._pos * weight;
				_transformMatrix._angleDeg += otherSnapShot._transformMatrix._angleDeg * weight;
				if(isSumMethod)
				{
					//Sum 방식이면 : 1 > 모두 곱하기 (가중치 없음)
					_transformMatrix._scale.x *= otherSnapShot._transformMatrix._scale.x;
					_transformMatrix._scale.y *= otherSnapShot._transformMatrix._scale.y;
				}
				else
				{
					//Average 방식이면 : 0 > 가중치 더하기
					_transformMatrix._scale += otherSnapShot._transformMatrix._scale * weight;
				}
				
			}
			
			_meshColor += otherSnapShot._meshColor * weight;
			//하나만 Visible이면 그냥 Visible인 걸로?
			if(otherSnapShot._isVisible)
			{
				_isVisible = true;
			}
			
			//Extravalue는 마지막 값으로 갱신한다.
			_isExtraValueEnabled = false;
			_extraValue = null;

			//추가 3.29 : ExtraValue도 복사
			if(otherSnapShot._isExtraValueEnabled)
			{
				_isExtraValueEnabled = true;
				_extraValue = new ExtraDummyValue(otherSnapShot._extraValue);
			}
		}


		// Get / Set
		//--------------------------------------------
	}

}