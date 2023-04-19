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
using System.Runtime.InteropServices;

namespace AnyPortrait
{

	/// <summary>
	/// Render Unit에 포함되어서
	/// 1. Result Type별로 분류하고
	/// 2. Blend를 하여
	/// 3. 최종적인 Calculate 결과를 리턴해준다.
	/// </summary>
	public class apCalculatedResultStack
	{
		// Members
		//---------------------------------------------------
		private apRenderUnit _parentRenderUnit = null;

		//계산된 결과 값들
		//(계산은 Modifier에서 직접 하기 때문에 여기엔 이미 계산된 값만 들어오게 된다)
		private List<apCalculatedResultParam> _resultParams_Rigging = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_VertLocal = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_Transform = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_MeshColor = new List<apCalculatedResultParam>();
		private List<apCalculatedResultParam> _resultParams_VertWorld = new List<apCalculatedResultParam>();

		//추가 : Extra Option에 대한 결과 : Depth/Texture를 한번에 계산한다. ParamKeyValue중에 하나만이라도 ExtraProperty가 포함되면 바로 적용한다.
		private List<apCalculatedResultParam> _resultParams_Extra = new List<apCalculatedResultParam>();


		//BoneTransform은 바로 apCalculatedResultParam 리스트를 만드는게 아니라 2중으로 묶어야 한다.
		//키값은 Bone
		private List<BoneAndModParamPair> _resultParams_BoneTransform = new List<BoneAndModParamPair>();
		
		/// <summary>
		/// Bone 처리에 대한 Pair
		/// Bone을 키값으로 하여 Modifier -> CalculateResultParam List를 저장한다.
		/// </summary>
		public class BoneAndModParamPair
		{
			public apBone _keyBone = null;
			public Dictionary<apModifierBase, ModifierAndResultParamListPair> _modParamPairs_ModKey = new Dictionary<apModifierBase, ModifierAndResultParamListPair>();
			public List<ModifierAndResultParamListPair> _modParamPairs = new List<ModifierAndResultParamListPair>();

			public BoneAndModParamPair(apBone bone)
			{
				_keyBone = bone;
			}

			public void AddCalculatedResultParam(apCalculatedResultParam calculatedResultParam, apRenderUnit modOwnerRenderUnit)
			{
				apModifierBase modifier = calculatedResultParam._linkedModifier;
				if (modifier == null)
				{ return; }

				ModifierAndResultParamListPair modParamPair = null;
				if (!_modParamPairs_ModKey.ContainsKey(modifier))
				{
					modParamPair = new ModifierAndResultParamListPair(modifier);
					_modParamPairs_ModKey.Add(modifier, modParamPair);
					_modParamPairs.Add(modParamPair);
				}
				else
				{
					modParamPair = _modParamPairs_ModKey[modifier];
				}
				modParamPair.AddCalculatedResultParam(calculatedResultParam, modOwnerRenderUnit);
			}

			public bool Remove(apCalculatedResultParam calculatedResultParam)
			{
				bool isAnyClearedParam = false;
				for (int i = 0; i < _modParamPairs.Count; i++)
				{
					_modParamPairs[i].Remove(calculatedResultParam);
					if (_modParamPairs[i]._resultParams.Count == 0)
					{
						isAnyClearedParam = true;
					}
				}
				if (isAnyClearedParam)
				{
					//Param이 없는 Pair는 삭제하고, Dictionary를 다시 만들어주자
					_modParamPairs_ModKey.Clear();
					_modParamPairs.RemoveAll(delegate (ModifierAndResultParamListPair a)
					{
						return a._resultParams.Count == 0;
					});

					for (int i = 0; i < _modParamPairs.Count; i++)
					{
						ModifierAndResultParamListPair modPair = _modParamPairs[i];

						//빠른 참조를 위해 Dictionary도 세팅해주자
						if (!_modParamPairs_ModKey.ContainsKey(modPair._keyModifier))
						{
							_modParamPairs_ModKey.Add(modPair._keyModifier, modPair);
						}
					}
				}

				return isAnyClearedParam;
			}

			//추가 20.4.21 : 모디파이어와 연결된 CalResultParam을 삭제한다.
			//그 외에는 그대로 둔다.
			public bool RemoveOfModifier(apModifierBase targetModifier)
			{
				bool isAnyClearedParam = false;
				int nRemoved = _modParamPairs.RemoveAll(delegate(ModifierAndResultParamListPair a)
				{
					return a._keyModifier == targetModifier;
				});

				if(nRemoved > 0)
				{
					isAnyClearedParam = true;
				
					//Param이 없는 Pair는 삭제하고, Dictionary를 다시 만들어주자
					_modParamPairs_ModKey.Clear();
					
					for (int i = 0; i < _modParamPairs.Count; i++)
					{
						ModifierAndResultParamListPair modPair = _modParamPairs[i];

						//빠른 참조를 위해 Dictionary도 세팅해주자
						if (!_modParamPairs_ModKey.ContainsKey(modPair._keyModifier))
						{
							_modParamPairs_ModKey.Add(modPair._keyModifier, modPair);
						}
					}
				}

				return isAnyClearedParam;
			}



			public void Sort()
			{
				//if (a._targetRenderUnit == b._targetRenderUnit)
				//{ return a.ModifierLayer - b.ModifierLayer; }
				//else
				//{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }


				_modParamPairs.Sort(delegate (ModifierAndResultParamListPair a, ModifierAndResultParamListPair b)
				{
					
					return a._keyModifier._layer - b._keyModifier._layer;
				});
			}

		}
		/// <summary>
		/// Bone 처리에 대한 Result Param은 같은 RenderUnit에 대해서
		/// Bone에 따라 리스트가 계속 추가되는 문제가 있다. (레이어를 구분할 수 없다)
		/// 따라서 Modifier를 키값으로 하여 연산 레벨을 구분해야한다.
		/// </summary>
		public class ModifierAndResultParamListPair
		{
			public apModifierBase _keyModifier;
			public List<apCalculatedResultParam> _resultParams = new List<apCalculatedResultParam>();
			public apRenderUnit _modOwnerRenderUnit = null;

			public ModifierAndResultParamListPair(apModifierBase modifier)
			{
				_keyModifier = modifier;
			}

			public void AddCalculatedResultParam(apCalculatedResultParam calculatedResultParam, apRenderUnit modOwnerRenderUnit)
			{
				_modOwnerRenderUnit = modOwnerRenderUnit;
				if (!_resultParams.Contains(calculatedResultParam))
				{
					_resultParams.Add(calculatedResultParam);
				}
			}

			public void Remove(apCalculatedResultParam calculatedResultParam)
			{
				_resultParams.Remove(calculatedResultParam);
			}
		}

		public int _nResultVerts = 0;
		public int _nResultPins = 0;

		public Vector2[] _result_VertLocal = null;

		//추가 22.3.28 [v1.4.0]
		public Vector2[] _result_PinLocal = null;

		apMatrix _result_MeshTransform = new apMatrix();
		

		public Color _result_Color = new Color(0.5f, 0.5f, 0.5f, 1f);
		public bool _result_IsVisible = true;

		//public List<Vector2> _result_VertWorld = null;
		public Vector2[] _result_VertWorld = null;

		//추가
		//Rigging Result
		public Vector2[] _result_Rigging = null;
		public float _result_RiggingWeight = 0.0f;
		public apMatrix3x3[] _result_RiggingMatrices = null;

		//Bone Transform
		//값을 계속 초기화해서 사용하는 지역변수의 역할
		
		private apMatrix _result_BoneTransform = new apMatrix();
		
		private bool _result_CalculatedColor = false;

		//추가 11.29 : Extra Option
		private bool _result_IsExtraDepthChanged = false;
		private bool _result_IsExtraTextureChanged = false;
		private int _result_ExtraDeltaDepth = 0;
		private apTextureData _result_ExtraTextureData = null;

		private bool _isAnyRigging = false;
		private bool _isAnyVertLocal = false;
		private bool _isAnyTransformation = false;
		private bool _isAnyMeshColor = false;

		private bool _isAnyVertWorld = false;
		private bool _isAnyBoneTransform = false;

		//추가 11.29 : ExtraOption 결과
		private bool _isAnyExtra = false;

		private Color _color_Default = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		


		// Init
		//---------------------------------------------------
		public apCalculatedResultStack(apRenderUnit parentRenderUnit)
		{
			//_tmpID = UnityEngine.Random.Range(0, 1000);

			_parentRenderUnit = parentRenderUnit;
			//Debug.Log("[" + _tmpID + "] Init [R " + _parentRenderUnit._tmpName + "]");
			ClearResultParams();
		}



		// Add / Remove / Sort
		//---------------------------------------------------
		public void AddCalculatedResultParam(apCalculatedResultParam resultParam, apRenderUnit modOwnerRenderUnit)
		{

			//Debug.Log("[" + _tmpID + "] AddCalculatedResultParam >> " + resultParam._resultType + "[R " + _parentRenderUnit._tmpName + "]");
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				if (resultParam._targetBone == null)
				{
					if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.Object)
					{
						if (!_resultParams_VertLocal.Contains(resultParam))
						{
							_resultParams_VertLocal.Add(resultParam);
						}
						_isAnyVertLocal = true;
					}
					else if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.World)
					{
						if (!_resultParams_VertWorld.Contains(resultParam))
						{
							_resultParams_VertWorld.Add(resultParam);
						}
						_isAnyVertWorld = true;
					}
					else if (resultParam._calculatedSpace == apCalculatedResultParam.CALCULATED_SPACE.Rigging)//<<추가되었다.
					{
						if (!_resultParams_Rigging.Contains(resultParam))
						{
							_resultParams_Rigging.Add(resultParam);
						}
						_isAnyRigging = true;
					}
					else
					{
						Debug.LogError("허용되지 않은 데이터 타입 [Calculate : Vertex + Loca]");
					}
				}
			}
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0)
			{
				//변경 : Bone타입과 일반 Transform타입으로 나뉜다.
				if (resultParam._targetBone != null)
				{
					//Bone 타입이다.
					//Modifier + ResultParam Pair로 저장해야한다.
					BoneAndModParamPair modParamPair = _resultParams_BoneTransform.Find(delegate (BoneAndModParamPair a)
					{
						return a._keyBone == resultParam._targetBone;
					});
					if (modParamPair == null)
					{
						modParamPair = new BoneAndModParamPair(resultParam._targetBone);
						_resultParams_BoneTransform.Add(modParamPair);
					}

					modParamPair.AddCalculatedResultParam(resultParam, modOwnerRenderUnit);
					_isAnyBoneTransform = true;
					//Debug.Log("   -- Cur CalculatedResultParam : " + _resultParams_BoneTransform.Count);
					//Debug.Log("_isAnyBoneTransform : True로 전환됨 [" + _parentRenderUnit.Name + "]");
					//이전 코드
					//if(!_resultParams_BoneTransform.Contains(resultParam))
					//{
					//	_resultParams_BoneTransform.Add(resultParam);
					//	_isAnyBoneTransform = true;
					//}
				}
				else
				{
					//Mesh/MeshGroup Transform 타입이다.
					if (!_resultParams_Transform.Contains(resultParam))
					{
						_resultParams_Transform.Add(resultParam);
						_isAnyTransformation = true;
					}
				}

			}
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//Bone 타입은 제외한다.
				if (resultParam._targetBone == null)
				{
					if (!_resultParams_MeshColor.Contains(resultParam))
					{
						_resultParams_MeshColor.Add(resultParam);
						_isAnyMeshColor = true;

					}
				}
			}

			//추가 11.29 : ExtraOption
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0
				|| (int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0
				|| (int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0//버그 21.9.28 : 이게 안적혀있어서 Color Modifier의 ExtraOption이 동작하지 않았다.
				
				)
			{
				if(resultParam._linkedModifier._isExtraPropertyEnabled)
				{
					//Modifer에서 ExtraProperty를 허용했을 경우
					//모든 ParamKeyValueSet 중에서 하나라도 ParamKeyValueSet에 ExtraOption이 포함되어 있는지 확인
					
					bool isExtraEnabledParam = false;
					apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
					
					
					for (int i = 0; i < resultParam._paramKeyValues.Count; i++)
					{
						curParamKeyValue = resultParam._paramKeyValues[i];
						
						//추가 3.18 : ModifiedMesh가 없는 경우가 있다. > 본을 대상으로 할 수도 있기 때문 > 본은 예외
						if(curParamKeyValue._modifiedMesh == null)
						{	
							continue;
						}

						if(curParamKeyValue._modifiedMesh._isExtraValueEnabled
							&&
							(curParamKeyValue._modifiedMesh._extraValue._isDepthChanged || curParamKeyValue._modifiedMesh._extraValue._isTextureChanged))
						{
							//하나라도 Extra Option이 켜진 ParamKeyValueSet이 있다면
							//이 ResultParam은 ExtraOption 계산에 참조되어야 한다.
							isExtraEnabledParam = true;
							break;
						}
					}

					if(isExtraEnabledParam)
					{
						if(!_resultParams_Extra.Contains(resultParam))
						{
							_resultParams_Extra.Add(resultParam);
							_isAnyExtra = true;
						}
					}
				}
			}

			//else
			//{
			//	Debug.LogError("apCalculatedResultStack / AddCalculatedResultParam : 알수없는 Result Type : " + resultParam._calculatedValueType);
			//}
			#region [미사용 코드] 변경되기 전의 Caculated Value
			//switch (resultParam._calculatedValueType)
			//{
			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos:
			//		{

			//		}
			//		break;

			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix:
			//		if(!_resultParams_Transform.Contains(resultParam))
			//		{
			//			_resultParams_Transform.Add(resultParam);
			//			_isAnyTransformation = true;
			//		}
			//		break;

			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.MeshGroup_Color:
			//		if(!_resultParams_MeshColor.Contains(resultParam))
			//		{
			//			_resultParams_MeshColor.Add(resultParam);
			//			_isAnyMeshColor = true;
			//		}
			//		break;

			//	case apCalculatedResultParam.CALCULATED_VALUE_TYPE.Vertex_World:
			//		if(!_resultParams_VertWorld.Contains(resultParam))
			//		{
			//			_resultParams_VertWorld.Add(resultParam);
			//			_isAnyVertWorld = true;
			//		}
			//		break;

			//	default:
			//		Debug.LogError("apCalculatedResultStack / AddCalculatedResultParam : 알수없는 Result Type : " + resultParam._calculatedValueType);
			//		break;
			//} 
			#endregion
		}

		public void RemoveCalculatedResultParam(apCalculatedResultParam resultParam)
		{
			_resultParams_Rigging.Remove(resultParam);
			_resultParams_VertLocal.Remove(resultParam);
			_resultParams_Transform.Remove(resultParam);
			_resultParams_MeshColor.Remove(resultParam);
			_resultParams_VertWorld.Remove(resultParam);
			_resultParams_Extra.Remove(resultParam);

			//Bone Transform은 Pair된 SubList로 관리되므로, 해당 Pair를 먼저 찾고 거기서 체크를 해야한다.
			bool isAnyClearedBoneParam = false;
			for (int i = 0; i < _resultParams_BoneTransform.Count; i++)
			{
				if (_resultParams_BoneTransform[i].Remove(resultParam))
				{
					isAnyClearedBoneParam = true;
				}
			}

			if (isAnyClearedBoneParam)
			{
				//전체에서 Param 개수가 0인 것들을 빼자
				_resultParams_BoneTransform.RemoveAll(delegate (BoneAndModParamPair a)
				{
					return a._modParamPairs.Count == 0;
				});
			}

			//bool prevBoneTransform = _isAnyBoneTransform;

			_isAnyRigging = (_resultParams_Rigging.Count != 0);
			_isAnyVertLocal = (_resultParams_VertLocal.Count != 0);
			_isAnyTransformation = (_resultParams_Transform.Count != 0);
			_isAnyMeshColor = (_resultParams_MeshColor.Count != 0);
			_isAnyVertWorld = (_resultParams_VertWorld.Count != 0);
			_isAnyBoneTransform = (_resultParams_BoneTransform.Count != 0);
			_isAnyExtra = (_resultParams_Extra.Count != 0);
			//Debug.LogError("[" + _tmpID + "] <<Remove Result Params>>");

			//if(prevBoneTransform && !_isAnyBoneTransform)
			//{
			//	Debug.LogError("[" + _parentRenderUnit.Name + "] Bone Param Cleared (Remove)");
			//}
			

			
		}

		public void ClearResultParams()
		{
			//if(_resultParams_VertLocal.Count > 0 || _isAnyVertLocal)
			//{
			//	Debug.LogError("[" + _tmpID + "] <<Clear Result Params>> < Vert Local Count : " + _resultParams_VertLocal.Count);
			//}
			//Debug.LogError("[" + _tmpID + "] <<Clear Result Params>>");
			_resultParams_Rigging.Clear();
			_resultParams_VertLocal.Clear();
			_resultParams_Transform.Clear();
			_resultParams_MeshColor.Clear();
			_resultParams_VertWorld.Clear();
			_resultParams_BoneTransform.Clear();
			_resultParams_Extra.Clear();

			//bool prevBoneTransform = _isAnyBoneTransform;

			_isAnyRigging = false;
			_isAnyVertLocal = false;
			_isAnyTransformation = false;
			_isAnyMeshColor = false;
			_isAnyVertWorld = false;
			_isAnyBoneTransform = false;
			_isAnyExtra = false;

			//if(prevBoneTransform && !_isAnyBoneTransform)
			//{
			//	Debug.LogError("[" + _parentRenderUnit.Name + "] Bone Param Cleared");
			//}
		}




		//추가 20.4.21 : 특정 모디파이어와 관련된 ResultParam만 삭제한다.
		//다른 모디파이어는 변경 내역이 없기 때문 (으로 간주)
		public void ClearResultParamsOfModifier(apModifierBase targetModifier)
		{
			if(targetModifier == null)
			{
				ClearResultParams();
			}

			_resultParams_Rigging.RemoveAll(delegate(apCalculatedResultParam a)
			{
				return a._linkedModifier == targetModifier;
			});
			_resultParams_VertLocal.RemoveAll(delegate(apCalculatedResultParam a)
			{
				return a._linkedModifier == targetModifier;
			});
			_resultParams_Transform.RemoveAll(delegate(apCalculatedResultParam a)
			{
				return a._linkedModifier == targetModifier;
			});
			_resultParams_MeshColor.RemoveAll(delegate(apCalculatedResultParam a)
			{
				return a._linkedModifier == targetModifier;
			});
			_resultParams_VertWorld.RemoveAll(delegate(apCalculatedResultParam a)
			{
				return a._linkedModifier == targetModifier;
			});

			//BoneTransform은 다르게 처리해야한다.
			bool isAnyRemoveBoneTransform = false;
			for (int iBT = 0; iBT < _resultParams_BoneTransform.Count; iBT++)
			{
				if(_resultParams_BoneTransform[iBT].RemoveOfModifier(targetModifier))
				{
					isAnyRemoveBoneTransform = true;
				}
			}
			if(isAnyRemoveBoneTransform)
			{
				_resultParams_BoneTransform.RemoveAll(delegate(BoneAndModParamPair a)
				{
					return a._modParamPairs.Count == 0;
				});
			}


			_resultParams_Extra.RemoveAll(delegate(apCalculatedResultParam a)
			{
				return a._linkedModifier == targetModifier;
			});

			_isAnyRigging = (_resultParams_Rigging.Count != 0);
			_isAnyVertLocal = (_resultParams_VertLocal.Count != 0);
			_isAnyTransformation = (_resultParams_Transform.Count != 0);
			_isAnyMeshColor = (_resultParams_MeshColor.Count != 0);
			_isAnyVertWorld = (_resultParams_VertWorld.Count != 0);
			_isAnyBoneTransform = (_resultParams_BoneTransform.Count != 0);
			_isAnyExtra = (_resultParams_Extra.Count != 0);
		}





		//추가 12.3
		//Stack에 ResultParam을 Add하는 과정은
		//(1) 첫번째 ModMesh/ModBone이 Result Param을 Stack에 넣는다.
		//(2) 두번째 부터는 "이미 Stack에 저장된 ResultParam"을 대상으로 ParamKeyValueSet을 추가한다.
		//> 이때, (1)이 아닌 ParamKeyValueSet이 추가될 때(2) 처리 성격이 바뀌는 경우가 있다. (Extra Option)
		//> 그래서 ParamKeyValueSet이 추가되는 상황에서도 갱신을 해야한다.
		public void OnParamKeyValueAddedOnCalculatedResultParam(apCalculatedResultParam resultParam)
		{
			//Extra Option을 다시 검사하자
			//추가 11.29 : ExtraOption
			if ((int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0
				|| (int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0
				|| (int)(resultParam._calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0//버그 21.9.28 : 이것도 수정
				)
			{
				if(resultParam._linkedModifier._isExtraPropertyEnabled)
				{
					if (!_resultParams_Extra.Contains(resultParam))
					{
						//Modifer에서 ExtraProperty를 허용했을 경우
						//모든 ParamKeyValueSet 중에서 하나라도 ParamKeyValueSet에 ExtraOption이 포함되어 있는지 확인

						bool isExtraEnabledParam = false;
						apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
						for (int i = 0; i < resultParam._paramKeyValues.Count; i++)
						{
							curParamKeyValue = resultParam._paramKeyValues[i];

							if (curParamKeyValue._modifiedMesh._isExtraValueEnabled
								&&
								(curParamKeyValue._modifiedMesh._extraValue._isDepthChanged || curParamKeyValue._modifiedMesh._extraValue._isTextureChanged))
							{
								//하나라도 Extra Option이 켜진 ParamKeyValueSet이 있다면
								//이 ResultParam은 ExtraOption 계산에 참조되어야 한다.
								isExtraEnabledParam = true;
								break;
							}
						}
						if (isExtraEnabledParam)
						{
							_resultParams_Extra.Add(resultParam);
							_isAnyExtra = true;

							
						}
					}
				}
			}
		}






		public void Sort()
		{
			//>Opt 연동할 것
			//다른 RenderUnit에 대해서는
			//Level이 큰게(하위) 먼저 계산되도록 내림차순 정렬 > 변경 ) Level 낮은 상위가 먼저 계산되도록 (오름차순)

			//같은 RenderUnit에 대해서는
			//오름차순 정렬 (레이어 값이 낮은 것 부터 처리할 수 있도록)

			_resultParams_Rigging.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_VertLocal.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_Transform.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_MeshColor.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			_resultParams_VertWorld.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});

			//_resultParams_BoneTransform.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			//{
			//	if(a._targetRenderUnit == b._targetRenderUnit)	{ return a.ModifierLayer - b.ModifierLayer; }
			//	return 0;//<이건 Sort가 그닥 필요하지 않다. Bone이니까..
			//});

			for (int i = 0; i < _resultParams_BoneTransform.Count; i++)
			{
				_resultParams_BoneTransform[i].Sort();
			}

			//추가 11.29 : Extra
			_resultParams_Extra.Sort(delegate (apCalculatedResultParam a, apCalculatedResultParam b)
			{
				if (a._targetRenderUnit == b._targetRenderUnit)
				{ return a.ModifierLayer - b.ModifierLayer; }
				else
				{ return a._targetRenderUnit._level - b._targetRenderUnit._level; }
			});
			
		}


		// Functions
		//---------------------------------------------------
		public void ReadyToCalculate()
		{
			//이전
			//int nVerts = _parentRenderUnit._renderVerts.Count;

			//변경 22.3.23 [v1.4.0]
			_nResultVerts = _parentRenderUnit._renderVerts != null ? _parentRenderUnit._renderVerts.Length : 0;
			_nResultPins = 0;

			if(_isAnyVertLocal)
			{
				if (_nResultVerts > 0)
				{
					if (_result_VertLocal == null || _result_VertLocal.Length != _nResultVerts)
					{
						_result_VertLocal = new Vector2[_nResultVerts];
					}
					//이전
					//for (int i = 0; i < nVerts; i++)
					//{
					//	_result_VertLocal[i] = Vector2.zero;
					//}

					//변경 22.3.22 : 이걸로 바꿔보자 [v1.4.0]
					Array.Clear(_result_VertLocal, 0, _nResultVerts);
				}


				//추가 22.3.28 : Pin 리스트도 계산
				_nResultPins = _parentRenderUnit._renderPinGroup != null ? _parentRenderUnit._renderPinGroup.NumPins : 0;
				if(_nResultPins > 0)
				{
					if (_result_PinLocal == null || _result_PinLocal.Length != _nResultPins)
					{
						_result_PinLocal = new Vector2[_nResultPins];
					}
					Array.Clear(_result_PinLocal, 0, _nResultPins);
				}
			}

			if(_isAnyVertWorld)
			{
				if (_nResultVerts > 0)
				{
					if (_result_VertWorld == null || _result_VertWorld.Length != _nResultVerts)
					{
						_result_VertWorld = new Vector2[_nResultVerts];
					}

					//for (int i = 0; i < nVerts; i++)
					//{
					//	_result_VertWorld[i] = Vector2.zero;
					//}
					//변경 22.3.22 : 이걸로 바꿔보자 [v1.4.0]
					Array.Clear(_result_VertWorld, 0, _nResultVerts);
				}
			}

			if(_isAnyRigging)
			{
				if (_nResultVerts > 0)
				{
					if (_result_RiggingMatrices == null || _result_RiggingMatrices.Length != _nResultVerts)
					{
						_result_RiggingMatrices = new apMatrix3x3[_nResultVerts];
						_result_Rigging = new Vector2[_nResultVerts];
					}

					//for (int i = 0; i < nVerts; i++)
					//{
					//	_result_RiggingMatrices[i].SetIdentity();
					//	//_result_Rigging[i] = Vector2.zero;
					//}

					//변경 22.3.22 : 이걸로 바꿔보자 [v1.4.0]
					Array.Clear(_result_Rigging, 0, _nResultVerts);
					Array.Copy(apMemUtil.I.GetInitMatrix3x3(_nResultVerts), _result_RiggingMatrices, _nResultVerts);

					_result_RiggingWeight = 0.0f;
				}
			}

			_result_BoneTransform.SetIdentity();
			_result_MeshTransform.SetIdentity();

			_result_MeshTransform.MakeMatrix();
			
			_result_Color = _color_Default;
			_result_IsVisible = true;
			_result_CalculatedColor = false;
			//_result_BoneIKWeight = 0.0f;
			//_result_CalculatedBoneIK = false;

			//추가 11.29 : Extra Option
			_result_IsExtraDepthChanged = false;
			_result_IsExtraTextureChanged = false;
			_result_ExtraDeltaDepth = 0;
			_result_ExtraTextureData = null;
		}

		/// <summary>
		/// Modifier등의 변동 사항이 있는 경우 RenderVert의 업데이트 데이터를 초기화한다.
		/// </summary>
		public void ResetRenderVerts()
		{
			if (_parentRenderUnit != null)
			{
				//이전
				//for (int i = 0; i < _parentRenderUnit._renderVerts.Count; i++)

				//변경 22.3.23 [v1.4.0]
				int nVerts = _parentRenderUnit._renderVerts != null ? _parentRenderUnit._renderVerts.Length : 0;

				if (nVerts > 0)
				{
					for (int i = 0; i < nVerts; i++)
					{
						_parentRenderUnit._renderVerts[i].ResetData();
					}
				}

				//추가 22.3.28 [v1.4.0] Render Pin
				if(_parentRenderUnit._renderPinGroup != null)
				{
					_parentRenderUnit._renderPinGroup.ResetData();
				}
			}
		}

		/// <summary>
		/// RenderUnit의 CalculateStack을 업데이트한다.
		/// 기본 단계의 업데이트이며, Rigging, VertWorld는 Post Update에서 처리한다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isMakeHashCode"></param>
		//public void Calculate_Pre(float tDelta, bool isMakeHashCode)
		public void Calculate_Pre(float tDelta)
		{
			float curWeight_Transform = 0.0f;
			float curWeight_Color = 0.0f;
			apCalculatedResultParam resultParam = null;
			apModifierBase linkedModifier = null;

			//추가) 처음 실행되는 CalParam은 Additive로 작동하지 않도록 한다.
			int iCalculatedParam = 0;


			//--------------------------------------------------------------------
			// 1. Local Morph
			if (_isAnyVertLocal)
			{
				//prevWeight = 0.0f;
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;
				Vector2[] posPins = null;

				iCalculatedParam = 0;

				int nResultParams_VertLocal = _resultParams_VertLocal.Count;

				for (int iParam = 0; iParam < nResultParams_VertLocal; iParam++)
				{
					resultParam = _resultParams_VertLocal[iParam];

					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (
						//!resultParam.IsModifierAvailable // 이전						
						!resultParam._isMainCalculated // 변경 22.5.11 [v1.4.0]
						|| curWeight_Transform <= 0.001f
						)
					{
						//Debug.LogError("실행 불가 : RenderUnit : " + _parentRenderUnit.Name + " > Modifier : " + resultParam._linkedModifier.DisplayName);
						//Debug.LogError("실행 불가 : Available : " + resultParam.IsModifierAvailable + " / Weight : " + curWeight_Transform);
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//추가 22.5.7

					//추가 Ex Edit 3.22
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}

					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;
						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
							{
								isRunnable = true;
							}
							break;
						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								//Debug.Log("선택되지 않은건데 실행 중 : RenderUnit : " + _parentRenderUnit.Name + " > Modifier : " + resultParam._linkedModifier.DisplayName);
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}



					//추가 22.5.7 [1.4.0] 버텍스가 없는 경우 패스
					if (_nResultVerts > 0)
					{
						posVerts = resultParam._result_Positions;

						//if (posVerts.Length != _result_VertLocal.Length)
						if (posVerts.Length != _nResultVerts)
						{
							//결과가 잘못 들어왔다 갱신 필요
							Debug.LogError("Wrong Vert Local Result (Cal : " + posVerts.Length + " / Verts : " + _result_VertLocal.Length + ")");
							continue;
						}

						// Blend 방식에 맞게 Pos를 만들자
						if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
						{
							for (int i = 0; i < _nResultVerts; i++)
							{
								_result_VertLocal[i] = BlendPosition_ITP(_result_VertLocal[i], posVerts[i], curWeight_Transform);
							}
						}
						else
						{
							for (int i = 0; i < _nResultVerts; i++)
							{
								_result_VertLocal[i] = BlendPosition_Add(_result_VertLocal[i], posVerts[i], curWeight_Transform);
							}
						}
					}



					//추가 22.3.28 Pin
					if (_nResultPins > 0)
					{
						posPins = resultParam._result_PinPositions;
						int nSrcPins = posPins != null ? posPins.Length : 0;
						if (nSrcPins == _nResultPins)
						{
							//핀이 1개 이상으로 유효할 때만 동작 (둘다 값이 있어야 한다)
							// Blend 방식에 맞게 Pos를 만들자
							if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
							{
								for (int i = 0; i < _nResultPins; i++)
								{
									_result_PinLocal[i] = BlendPosition_ITP(_result_PinLocal[i], posPins[i], curWeight_Transform);
								}
							}
							else
							{
								for (int i = 0; i < _nResultPins; i++)
								{
									_result_PinLocal[i] = BlendPosition_Add(_result_PinLocal[i], posPins[i], curWeight_Transform);
								}
							}
						}
					}
					

					iCalculatedParam++;

				}
			}

			//--------------------------------------------------------------------

			// 2. Mesh / MeshGroup Transformation
			if (_isAnyTransformation)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;

				iCalculatedParam = 0;

				int nResultParams_Transform = _resultParams_Transform.Count;

				for (int iParam = 0; iParam < nResultParams_Transform; iParam++)
				{
					resultParam = _resultParams_Transform[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable //이전 
						!resultParam._isMainCalculated // 변경 22.5.11 [v1.4.0]
						|| curWeight_Transform <= 0.001f)
					{
						continue;
					}


					linkedModifier = resultParam._linkedModifier;//22.5.7

					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}


					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}

					// Blend 방식에 맞게 Matrix를 만들자 하자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						BlendMatrix_ITP(_result_MeshTransform, resultParam._result_Matrix, curWeight_Transform);
					}
					else
					{
						BlendMatrix_Add(_result_MeshTransform, resultParam._result_Matrix, curWeight_Transform);
					}

					iCalculatedParam++;
				}

				_result_MeshTransform.MakeMatrix();
			}

			//--------------------------------------------------------------------

			// 3. Mesh Color
			if (_isAnyMeshColor)
			{
				curWeight_Color = 0.0f;
				resultParam = null;

				iCalculatedParam = 0;

				_result_IsVisible = false;
				_result_CalculatedColor = false;

				int nMeshColorCalculated = 0;

				int nResultParams_MeshColor = _resultParams_MeshColor.Count;

				for (int iParam = 0; iParam < nResultParams_MeshColor; iParam++)
				{
					resultParam = _resultParams_MeshColor[iParam];
					curWeight_Color = resultParam.ModifierWeight_Color;

					//이전
					//if (!resultParam.IsModifierAvailable
					//	|| curWeight_Color <= 0.001f
					//	|| !resultParam.IsColorValueEnabled
					//	|| !resultParam._isColorCalculated//<<추가 : Color로 등록했지만 아예 계산이 안되었을 수도 있다.
					//	)

					//변경 22.5.11 : 메인 데이터 처리 여부는 더이상 체크하지 않는다. (색상은 별개로 친다.)
					if (!resultParam.IsColorValueEnabled
						|| !resultParam._isColorCalculated
						|| curWeight_Color <= 0.001f)
					{
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//22.5.7

					//추가: 색상은 ExMode에서 별도로 취급
					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}

					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)//색상은 이것도 추가
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}

					// Blend 방식에 맞게 Matrix를 만들자 하자
					
					//이전
					//if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					
					//변경 21.4.6 : 색상의 경우 기본 BlendMethod를 사용해야한다. 별도로 알아서 잘 걸러져서 오기 때문
					if (linkedModifier._blendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						_result_Color = apUtil.BlendColor_ITP(_result_Color, resultParam._result_Color, Mathf.Clamp01(curWeight_Color));
					}
					else
					{
						_result_Color = apUtil.BlendColor_Add(_result_Color, resultParam._result_Color, curWeight_Color);
					}

					//Visible 여부도 결정
					_result_IsVisible |= resultParam._result_IsVisible;
					nMeshColorCalculated++;
					_result_CalculatedColor = true;//<<"계산된 MeshColor" Result가 있음을 알린다.

					iCalculatedParam++;
				}

				if (nMeshColorCalculated == 0)
				{
					//색상 처리값이 없다면 자동으로 True
					_result_IsVisible = true;
				}
			}
			else
			{
				//색상 처리값이 없다면 자동으로 True
				_result_IsVisible = true;
			}

			//--------------------------------------------------------------------

			//5. Bone을 업데이트 하자
			//Bone은 값 저장만 할게 아니라 직접 업데이트를 해야한다.
			if (_isAnyBoneTransform)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;


				//추가 3.22 ExEdit
				//본의 경우는 별도로 ExMode를 가진다.

				
				int nResultParams_BoneTransform = _resultParams_BoneTransform.Count;

				BoneAndModParamPair boneModPair = null;
				apBone targetBone = null;
				List<ModifierAndResultParamListPair> modParamPairs = null;
				ModifierAndResultParamListPair modParamPair = null;

				

				for (int iBonePair = 0; iBonePair < nResultParams_BoneTransform; iBonePair++)
				{
					boneModPair = _resultParams_BoneTransform[iBonePair];
					targetBone = boneModPair._keyBone;
					modParamPairs = boneModPair._modParamPairs;

					if (targetBone == null || modParamPairs.Count == 0)
					{
						continue;
					}

					iCalculatedParam = 0;
					_result_BoneTransform.SetIdentity();
					
					int nModPairs = modParamPairs.Count;
					

					for (int iModParamPair = 0; iModParamPair < nModPairs; iModParamPair++)
					{
						modParamPair = modParamPairs[iModParamPair];
						int nResultPairParams = modParamPair._resultParams.Count;

						for (int iParam = 0; iParam < nResultPairParams; iParam++)
						{
							resultParam = modParamPair._resultParams[iParam];
							curWeight_Transform = resultParam.ModifierWeight_Transform;

							if (//!resultParam.IsModifierAvailable // 이전
								!resultParam._isMainCalculated // 변경 22.5.11
								|| curWeight_Transform <= 0.001f)
							{
								continue;
							}

							//코드 개선 21.2.15
							linkedModifier = resultParam._linkedModifier;//22.5.7

							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
							{
								continue;
							}

							bool isRunnable = false;
							switch (targetBone._exCalculateMode)
							{
								case apBone.EX_CALCULATE.Enabled_Run:
									//무조건 실행
									isRunnable = true;
									break;

								case apBone.EX_CALCULATE.Enabled_Edit:
									//편집 중인 것만 실행하려면
									if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)//색상은 이것도 추가
									{
										isRunnable = true;
									}
									break;

								case apBone.EX_CALCULATE.Disabled_ExRun:
									//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
									if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
									{
										isRunnable = true;
									}
									break;
							}

							if (!isRunnable)
							{
								continue;
							}



							// Blend 방식에 맞게 Matrix를 만들자 하자
							if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
							{
								BlendMatrix_ITP(_result_BoneTransform, resultParam._result_Matrix, curWeight_Transform);
							}
							else
							{
								BlendMatrix_Add(_result_BoneTransform, resultParam._result_Matrix, curWeight_Transform);
							}

							iCalculatedParam++;
						}
					}

					
					
					//참조된 본에 직접 값을 넣어주자
					targetBone.UpdateModifiedValue(_result_BoneTransform._pos, _result_BoneTransform._angleDeg, _result_BoneTransform._scale);
				}



			}

			//추가 11.30 : Extra Option
			if(_isAnyExtra)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;

				_result_IsExtraDepthChanged = false;
				_result_IsExtraTextureChanged = false;
				_result_ExtraDeltaDepth = 0;
				_result_ExtraTextureData = null;

				int nResultParams_Extra = _resultParams_Extra.Count;

				for (int iParam = 0; iParam < nResultParams_Extra; iParam++)
				{
					resultParam = _resultParams_Extra[iParam];

					//삭제 22.5.11 : Extra는 메인 데이터 처리 여부와 별개이다.
					//if(!resultParam.IsModifierAvailable)
					//{
					//	continue;
					//}

					//Extra Option은 무조건 나중에 나온 값으로 적용된다.
					//Blend가 불가능하기 때문
					if(resultParam._isExtra_DepthChanged)
					{
						//1. Depth에 변화가 있을 경우
						_result_IsExtraDepthChanged = true;
						_result_ExtraDeltaDepth = resultParam._extra_DeltaDepth;
					}
					if(resultParam._isExtra_TextureChanged)
					{
						//2. Texture에 변화가 있을 경우
						_result_IsExtraTextureChanged = true;
						_result_ExtraTextureData = resultParam._extra_TextureData;
					}
				}
			}
		}



		/// <summary>
		/// RenderUnit의 CalculateStack을 업데이트한다.
		/// 1차 업데이트 이후에 실행되며, Rigging, VertWorld를 처리한다.
		/// </summary>
		/// <param name="tDelta"></param>
		/// <param name="isMakeHashCode"></param>
		//public void Calculate_Post(float tDelta, bool isMakeHashCode)
		public void Calculate_Post(float tDelta)
		{
			float curWeight_Transform = 0.0f;
			apCalculatedResultParam resultParam = null;
			apModifierBase linkedModifier = null;

			//추가) 처음 실행되는 CalParam은 Additive로 작동하지 않도록 한다.
			int iCalculatedParam = 0;


			//--------------------------------------------------------------------
			// 0. Rigging
			if (_isAnyRigging)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;
				apMatrix3x3[] vertMatrice = null;

				iCalculatedParam = 0;

				_result_RiggingWeight = 0.0f;

				int nResultParams_Rigging = _resultParams_Rigging.Count;

				for (int iParam = 0; iParam < nResultParams_Rigging; iParam++)
				{
					resultParam = _resultParams_Rigging[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable
						!resultParam._isMainCalculated // 변경 22.5.11
						|| curWeight_Transform <= 0.001f)
					{
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//22.5.7

					//코드 개선 21.2.15 : 리깅도 안될때가 있다.
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}

					posVerts = resultParam._result_Positions;
					vertMatrice = resultParam._result_VertMatrices;

					if(posVerts == null)
					{
						Debug.LogError("Pos Vert is NULL");
					}
					
					//if (posVerts.Length != _result_Rigging.Length)
					if (posVerts.Length != _nResultVerts)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert Local Result (Cal : " + posVerts.Length + " / Verts : " + _result_Rigging.Length + ")");
						continue;
					}

					_result_RiggingWeight += curWeight_Transform;

					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						for (int i = 0; i < _nResultVerts; i++)
						{
							_result_Rigging[i] = BlendPosition_ITP(_result_Rigging[i], posVerts[i], curWeight_Transform);
							_result_RiggingMatrices[i].SetMatrixWithWeight(ref vertMatrice[i], curWeight_Transform);//<<추가
						}
					}
					else
					{
						for (int i = 0; i < _nResultVerts; i++)
						{
							_result_Rigging[i] = BlendPosition_Add(_result_Rigging[i], posVerts[i], curWeight_Transform);
							_result_RiggingMatrices[i].AddMatrixWithWeight(ref vertMatrice[i], curWeight_Transform);//<<추가
						}
					}

					iCalculatedParam++;

				}

				if (_result_RiggingWeight > 1.0f)
				{
					_result_RiggingWeight = 1.0f;
				}
			}

			//--------------------------------------------------------------------
			// 4. World Morph
			if (_isAnyVertWorld)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;

				iCalculatedParam = 0;

				int nResultParams_VertWorld = _resultParams_VertWorld.Count;

				for (int iParam = 0; iParam < nResultParams_VertWorld; iParam++)
				{
					resultParam = _resultParams_VertWorld[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable
						!resultParam._isMainCalculated // 변경 22.5.11
						|| curWeight_Transform <= 0.001f
						)
					{
						continue;
					}

					posVerts = resultParam._result_Positions;

					//if (posVerts.Length != _result_VertWorld.Length)
					if (posVerts.Length != _nResultVerts)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert World Result (Cal : " + posVerts.Length + " / Verts : " + _result_VertWorld.Length + ")");
						continue;
					}

					//추가 Ex Edit 3.22
					//if(isExCalculated)
					//{
					//	//TODO  21.2.15 이것도 수정하자
					//	//if((isExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
					//	//	|| (isSubExEnabledOnly && resultParam._linkedModifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled))
					//	{
					//		//ExEdit 모드에 맞지 않는다.
					//		continue;
					//	}
					//}

					linkedModifier = resultParam._linkedModifier;//22.5.7

					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}

					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}



					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						for (int i = 0; i < _nResultVerts; i++)
						{
							_result_VertWorld[i] = BlendPosition_ITP(_result_VertWorld[i], posVerts[i], curWeight_Transform);
						}
					}
					else
					{
						for (int i = 0; i < _nResultVerts; i++)
						{
							_result_VertWorld[i] = BlendPosition_Add(_result_VertWorld[i], posVerts[i], curWeight_Transform);
						}
					}

					iCalculatedParam++;
				}
			}
			//--------------------------------------------------------------------
		}


		private Vector2 BlendPosition_ITP(Vector2 prevResult, Vector2 nextResult, float nextWeight)//<<Prev를 삭제했다.
		{
			//return ((prevResult * prevWeight) + (nextResult * nextWeight)) / (prevWeight + nextWeight);
			return ((prevResult * (1.0f - nextWeight)) + (nextResult * nextWeight));
		}

		private Vector2 BlendPosition_Add(Vector2 prevResult, Vector2 nextResult, float nextWeight)
		{
			return prevResult + nextResult * nextWeight;
		}



		// 이전 : apMatrix 를 이용하는 Blend 함수
		//private void BlendMatrix_ITP(apMatrix prevResult, apMatrix nextResult, float nextWeight)
		//{
		//	if (nextWeight <= 0.0f)
		//	{
		//		return;
		//	}

		//	prevResult.LerpMartix(nextResult, nextWeight / 1.0f);
		//}

		//private void BlendMatrix_Add(apMatrix prevResult, apMatrix nextResult, float nextWeight)
		//{
		//	prevResult._pos += nextResult._pos * nextWeight;
		//	prevResult._angleDeg += nextResult._angleDeg * nextWeight;
			
		//	prevResult._scale.x = (prevResult._scale.x * (1.0f - nextWeight)) + (prevResult._scale.x * nextResult._scale.x * nextWeight);
		//	prevResult._scale.y = (prevResult._scale.y * (1.0f - nextWeight)) + (prevResult._scale.y * nextResult._scale.y * nextWeight);
			
		//}

		// 변경 3.26 : apMatrixCal을 이용하는 것으로 변경
		private void BlendMatrix_ITP(apMatrix prevResult, apMatrixCal nextResult, float nextWeight)
		{
			if (nextWeight <= 0.0f)
			{
				return;
			}

			prevResult.LerpMartixCal(nextResult, nextWeight / 1.0f);
		}

		private void BlendMatrix_Add(apMatrix prevResult, apMatrixCal nextResult, float nextWeight)
		{
			prevResult._pos += nextResult._pos * nextWeight;
			prevResult._angleDeg += nextResult._angleDeg * nextWeight;
			
			prevResult._scale.x = (prevResult._scale.x * (1.0f - nextWeight)) + (prevResult._scale.x * nextResult._calculatedScale.x * nextWeight);
			prevResult._scale.y = (prevResult._scale.y * (1.0f - nextWeight)) + (prevResult._scale.y * nextResult._calculatedScale.y * nextWeight);
		}





		//-----------------------------------------------------
		// C++ DLL 버전
		//-----------------------------------------------------


#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_SetWeightedPosList(	ref Vector2[] dstVectorArr, 
																ref Vector2[] srcVectorArr, 
																int arrLength, 
																float weight);


#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_AddWeightedPosList(	ref Vector2[] dstVectorArr, 
																ref Vector2[] srcVectorArr, 
																int arrLength, 
																float weight);

#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_InterpolateWeightedPosList(	ref Vector2[] dstVectorArr, 
																		ref Vector2[] srcVectorArr, 
																		int arrLength, 
																		float weight);

#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_AddWeightedRiggedPosList(	ref Vector2[] dstVectorArr, 
																		ref Vector2[] srcVectorArr, 
																		ref apMatrix3x3[] dstMatrixArr, 
																		ref apMatrix3x3[] srcMatrixArr, int arrLength, float weight);

#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void Modifier_InterpolateWeightedRiggedPosList(	ref Vector2[] dstVectorArr, 
																				ref Vector2[] srcVectorArr, 
																				ref apMatrix3x3[] dstMatrixArr, 
																				ref apMatrix3x3[] srcMatrixArr, int arrLength, float weight);

		/// <summary>
		/// Calculate_Pre의 DLL 버전
		/// </summary>
		public void Calculate_Pre_DLL(float tDelta)
		{	
			float curWeight_Transform = 0.0f;
			float curWeight_Color = 0.0f;
			apCalculatedResultParam resultParam = null;
			apModifierBase linkedModifier = null;

			//추가) 처음 실행되는 CalParam은 Additive로 작동하지 않도록 한다.
			int iCalculatedParam = 0;

			//--------------------------------------------------------------------
			// 1. Local Morph
			if (_isAnyVertLocal)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;
				Vector2[] posPins = null;

				iCalculatedParam = 0;

				int nResultParams_VertLocal = _resultParams_VertLocal.Count;

				for (int iParam = 0; iParam < nResultParams_VertLocal; iParam++)
				{
					resultParam = _resultParams_VertLocal[iParam];

					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable
						!resultParam._isMainCalculated // 변경 22.5.11
						|| curWeight_Transform <= 0.001f
						)
					{
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//추가 22.5.7

					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}

					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								//Debug.Log("선택되지 않은건데 실행 중 : RenderUnit : " + _parentRenderUnit.Name + " > Modifier : " + resultParam._linkedModifier.DisplayName);
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}


					//추가 22.5.7 [1.4.0] 버텍스가 없는 경우 패스
					if (_nResultVerts > 0)
					{
						posVerts = resultParam._result_Positions;
						
						//if (posVerts.Length != _result_VertLocal.Length)
						if (posVerts.Length != _nResultVerts)
						{
							//결과가 잘못 들어왔다 갱신 필요
							Debug.LogError("Wrong Vert Local Result (Cal : " + posVerts.Length + " / Verts : " + _result_VertLocal.Length + ")");
							continue;
						}

						// Blend 방식에 맞게 Pos를 만들자
						if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
						{
							//< C++ DLL >
							Modifier_InterpolateWeightedPosList(ref _result_VertLocal, ref posVerts, _nResultVerts, curWeight_Transform);
						}
						else
						{
							//< C++ DLL >
							Modifier_AddWeightedPosList(ref _result_VertLocal, ref posVerts, _nResultVerts, curWeight_Transform);
						}
					}


					//추가 22.3.28 Pin
					if (_nResultPins > 0)
					{
						posPins = resultParam._result_PinPositions;
						int nSrcPins = posPins != null ? posPins.Length : 0;
						if (nSrcPins == _nResultPins)
						{
							//핀이 1개 이상으로 유효할 때만 동작 (둘다 값이 있어야 한다)
							// Blend 방식에 맞게 Pos를 만들자
							if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
							{
								//for (int i = 0; i < _nResultPins; i++)
								//{
								//	_result_PinLocal[i] = BlendPosition_ITP(_result_PinLocal[i], posPins[i], curWeight_Transform);
								//}

								//< C++ DLL >
								Modifier_InterpolateWeightedPosList(ref _result_PinLocal, ref posPins, _nResultPins, curWeight_Transform);
							}
							else
							{
								//for (int i = 0; i < _nResultPins; i++)
								//{
								//	_result_PinLocal[i] = BlendPosition_Add(_result_PinLocal[i], posPins[i], curWeight_Transform);
								//}

								//< C++ DLL >
								Modifier_AddWeightedPosList(ref _result_PinLocal, ref posPins, _nResultPins, curWeight_Transform);
							}
						}
					}
					iCalculatedParam++;

				}
			}

			//--------------------------------------------------------------------

			// 2. Mesh / MeshGroup Transformation
			if (_isAnyTransformation)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;

				iCalculatedParam = 0;

				int nResultParams_Transform = _resultParams_Transform.Count;

				for (int iParam = 0; iParam < nResultParams_Transform; iParam++)
				{
					resultParam = _resultParams_Transform[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable
						!resultParam._isMainCalculated // 변경 22.5.11
						|| curWeight_Transform <= 0.001f)
					{
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//22.5.7

					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}
					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}

					// Blend 방식에 맞게 Matrix를 만들자 하자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						BlendMatrix_ITP(_result_MeshTransform, resultParam._result_Matrix, curWeight_Transform);
					}
					else
					{
						BlendMatrix_Add(_result_MeshTransform, resultParam._result_Matrix, curWeight_Transform);
						
					}

					iCalculatedParam++;
				}

				_result_MeshTransform.MakeMatrix();
			}

			//--------------------------------------------------------------------

			// 3. Mesh Color
			if (_isAnyMeshColor)
			{
				curWeight_Color = 0.0f;
				resultParam = null;

				iCalculatedParam = 0;

				_result_IsVisible = false;
				_result_CalculatedColor = false;

				int nMeshColorCalculated = 0;

				int nResultParams_MeshColor = _resultParams_MeshColor.Count;

				for (int iParam = 0; iParam < nResultParams_MeshColor; iParam++)
				{
					resultParam = _resultParams_MeshColor[iParam];
					curWeight_Color = resultParam.ModifierWeight_Color;

					//if (!resultParam.IsModifierAvailable
					//	|| curWeight_Color <= 0.001f
					//	|| !resultParam.IsColorValueEnabled
					//	|| !resultParam._isColorCalculated//<<추가 : Color로 등록했지만 아예 계산이 안되었을 수도 있다.
					//	)

					//변경 22.5.11 : 메인 데이터 처리 여부는 고려하지 않는다.
					if (!resultParam.IsColorValueEnabled
						|| curWeight_Color <= 0.001f
						|| !resultParam._isColorCalculated//<<추가 : Color로 등록했지만 아예 계산이 안되었을 수도 있다.
						)
					{
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//22.5.7
					
					//추가: 색상은 ExMode에서 별도로 취급
					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}
					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)//색상은 이것도 추가
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}


					// Blend 방식에 맞게 Matrix를 만들자 하자
					
					//변경 21.4.6 : 색상의 경우 기본 BlendMethod를 사용해야한다. 별도로 알아서 잘 걸러져서 오기 때문
					if (linkedModifier._blendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						_result_Color = apUtil.BlendColor_ITP(_result_Color, resultParam._result_Color, Mathf.Clamp01(curWeight_Color));
					}
					else
					{
						_result_Color = apUtil.BlendColor_Add(_result_Color, resultParam._result_Color, curWeight_Color);
					}

					//Visible 여부도 결정
					_result_IsVisible |= resultParam._result_IsVisible;
					nMeshColorCalculated++;
					_result_CalculatedColor = true;//<<"계산된 MeshColor" Result가 있음을 알린다.

					iCalculatedParam++;
				}

				if (nMeshColorCalculated == 0)
				{
					//색상 처리값이 없다면 자동으로 True
					_result_IsVisible = true;
				}
			}
			else
			{
				//색상 처리값이 없다면 자동으로 True
				_result_IsVisible = true;
			}

			//--------------------------------------------------------------------

			//5. Bone을 업데이트 하자
			//Bone은 값 저장만 할게 아니라 직접 업데이트를 해야한다.
			if (_isAnyBoneTransform)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;
				
				int nResultParams_BoneTransform = _resultParams_BoneTransform.Count;

				BoneAndModParamPair boneModPair = null;
				apBone targetBone = null;
				List<ModifierAndResultParamListPair> modParamPairs = null;
				ModifierAndResultParamListPair modParamPair = null;


				for (int iBonePair = 0; iBonePair < nResultParams_BoneTransform; iBonePair++)
				{
					boneModPair = _resultParams_BoneTransform[iBonePair];
					targetBone = boneModPair._keyBone;
					modParamPairs = boneModPair._modParamPairs;
					if (targetBone == null || modParamPairs.Count == 0)
					{
						continue;
					}

					iCalculatedParam = 0;
					_result_BoneTransform.SetIdentity();
					
					int nModPairs = modParamPairs.Count;

					for (int iModParamPair = 0; iModParamPair < nModPairs; iModParamPair++)
					{
						modParamPair = modParamPairs[iModParamPair];
						int nResultPairParams = modParamPair._resultParams.Count;

						for (int iParam = 0; iParam < nResultPairParams; iParam++)
						{
							resultParam = modParamPair._resultParams[iParam];
							curWeight_Transform = resultParam.ModifierWeight_Transform;

							if (//!resultParam.IsModifierAvailable
								!resultParam._isMainCalculated // 변경 22.5.11
								|| curWeight_Transform <= 0.001f
								)
							{ continue; }

							linkedModifier = resultParam._linkedModifier;//22.5.7


							//코드 개선 21.2.15
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
							{
								continue;
							}

							bool isRunnable = false;
							switch (targetBone._exCalculateMode)
							{
								case apBone.EX_CALCULATE.Enabled_Run:
									//무조건 실행
									isRunnable = true;
									break;

								case apBone.EX_CALCULATE.Enabled_Edit:
									//편집 중인 것만 실행하려면
									if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)//색상은 이것도 추가
									{
										isRunnable = true;
									}
									break;

								case apBone.EX_CALCULATE.Disabled_ExRun:
									//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
									if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
										|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
									{
										isRunnable = true;
									}
									break;
							}

							if (!isRunnable)
							{
								continue;
							}


							// Blend 방식에 맞게 Matrix를 만들자 하자
							if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
							{
								BlendMatrix_ITP(_result_BoneTransform, resultParam._result_Matrix, curWeight_Transform);
							}
							else
							{
								BlendMatrix_Add(_result_BoneTransform, resultParam._result_Matrix, curWeight_Transform);
							}

							iCalculatedParam++;
						}
					}

					
					
					//참조된 본에 직접 값을 넣어주자
					targetBone.UpdateModifiedValue(_result_BoneTransform._pos, _result_BoneTransform._angleDeg, _result_BoneTransform._scale);
				}



			}

			//추가 11.30 : Extra Option
			if(_isAnyExtra)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;

				_result_IsExtraDepthChanged = false;
				_result_IsExtraTextureChanged = false;
				_result_ExtraDeltaDepth = 0;
				_result_ExtraTextureData = null;

				int nResultParams_Extra = _resultParams_Extra.Count;

				for (int iParam = 0; iParam < nResultParams_Extra; iParam++)
				{
					resultParam = _resultParams_Extra[iParam];

					//삭제 22.5.11 : 메인 데ㅔ이터 
					//if(!resultParam.IsModifierAvailable)
					//{
					//	continue;
					//}

					//Extra Option은 무조건 나중에 나온 값으로 적용된다.
					//Blend가 불가능하기 때문
					if(resultParam._isExtra_DepthChanged)
					{
						//1. Depth에 변화가 있을 경우
						_result_IsExtraDepthChanged = true;
						_result_ExtraDeltaDepth = resultParam._extra_DeltaDepth;
					}
					if(resultParam._isExtra_TextureChanged)
					{
						//2. Texture에 변화가 있을 경우
						_result_IsExtraTextureChanged = true;
						_result_ExtraTextureData = resultParam._extra_TextureData;
					}
				}
			}
		}



		/// <summary>
		/// Calculate_Post의 DLL 버전
		/// </summary>
		public void Calculate_Post_DLL(float tDelta)
		{
			float curWeight_Transform = 0.0f;
			apCalculatedResultParam resultParam = null;
			apModifierBase linkedModifier = null;

			//추가) 처음 실행되는 CalParam은 Additive로 작동하지 않도록 한다.
			int iCalculatedParam = 0;


			//--------------------------------------------------------------------
			// 0. Rigging
			if (_isAnyRigging)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;
				apMatrix3x3[] vertMatrice = null;

				iCalculatedParam = 0;

				_result_RiggingWeight = 0.0f;

				int nResultParams_Rigging = _resultParams_Rigging.Count;

				for (int iParam = 0; iParam < nResultParams_Rigging; iParam++)
				{
					resultParam = _resultParams_Rigging[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable
						!resultParam._isMainCalculated // 변경 22.5.11
						|| curWeight_Transform <= 0.001f)
					{
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//22.5.7

					//코드 개선 21.2.15 : 리깅도 안될때가 있다.
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}

					posVerts = resultParam._result_Positions;
					vertMatrice = resultParam._result_VertMatrices;

					if(posVerts == null)
					{
						Debug.LogError("Pos Vert is NULL");
					}
					
					//if (posVerts.Length != _result_Rigging.Length)
					if (posVerts.Length != _nResultVerts)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert Local Result (Cal : " + posVerts.Length + " / Verts : " + _result_Rigging.Length + ")");
						continue;
					}

					_result_RiggingWeight += curWeight_Transform;

					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						//< C++ DLL >
						Modifier_InterpolateWeightedRiggedPosList(	ref _result_Rigging, 
																	ref posVerts, 
																	ref _result_RiggingMatrices, 
																	ref vertMatrice, 
																	_nResultVerts, curWeight_Transform);
					}
					else
					{
						//< C++ DLL >
						Modifier_AddWeightedRiggedPosList(	ref _result_Rigging, 
															ref posVerts, 
															ref _result_RiggingMatrices, 
															ref vertMatrice, 
															_nResultVerts, curWeight_Transform);
					}
					iCalculatedParam++;
				}

				if (_result_RiggingWeight > 1.0f)
				{
					_result_RiggingWeight = 1.0f;
				}
			}

			//--------------------------------------------------------------------
			// 4. World Morph
			if (_isAnyVertWorld)
			{
				curWeight_Transform = 0.0f;
				resultParam = null;
				Vector2[] posVerts = null;

				iCalculatedParam = 0;

				int nResultParams_VertWorld = _resultParams_VertWorld.Count;

				for (int iParam = 0; iParam < nResultParams_VertWorld; iParam++)
				{
					resultParam = _resultParams_VertWorld[iParam];
					curWeight_Transform = resultParam.ModifierWeight_Transform;

					if (//!resultParam.IsModifierAvailable
						!resultParam._isMainCalculated // 변경 22.5.11
						|| curWeight_Transform <= 0.001f)
					{
						continue;
					}

					posVerts = resultParam._result_Positions;
					
					//if (posVerts.Length != _result_VertWorld.Length)
					if (posVerts.Length != _nResultVerts)
					{
						//결과가 잘못 들어왔다 갱신 필요
						Debug.LogError("Wrong Vert World Result (Cal : " + posVerts.Length + " / Verts : " + _result_VertWorld.Length + ")");
						continue;
					}

					linkedModifier = resultParam._linkedModifier;//22.5.7

					//코드 개선 21.2.15
					if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force)
					{
						//이 모디파이어는 실행하지 않는다.
						continue;
					}
					bool isRunnable = false;
					switch (_parentRenderUnit._exCalculateMode)
					{
						case apRenderUnit.EX_CALCULATE.Enabled_Run:
							//무조건 실행
							isRunnable = true;
							break;

						case apRenderUnit.EX_CALCULATE.Enabled_Edit:
							//편집 중인 것만 실행하려면
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
							{
								isRunnable = true;
							}
							break;

						case apRenderUnit.EX_CALCULATE.Disabled_ExRun:
							//편집되지 않지만, 그외의 것이 실행되려면 (또는 무시하고 실행하는 것만)
							if (linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background
								|| linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor)
							{
								isRunnable = true;
							}
							break;
					}

					if(!isRunnable)
					{
						continue;
					}



					// Blend 방식에 맞게 Pos를 만들자
					if (resultParam.ModifierBlendMethod == apModifierBase.BLEND_METHOD.Interpolation || iCalculatedParam == 0)
					{
						//< C++ DLL >
						Modifier_InterpolateWeightedPosList(ref _result_VertWorld, ref posVerts, posVerts.Length, curWeight_Transform);
					}
					else
					{
						//< C++ DLL >
						Modifier_AddWeightedPosList(ref _result_VertWorld, ref posVerts, posVerts.Length, curWeight_Transform);
					}

					iCalculatedParam++;
				}
			}
			//--------------------------------------------------------------------
		}





		// Get / Set
		//---------------------------------------------------
		public bool IsRigging { get { return _isAnyRigging; } }
		public bool IsVertexLocal { get { return _isAnyVertLocal; } }
		public bool IsVertexWorld { get { return _isAnyVertWorld; } }

		//삭제 22.5.11 : 최적화
		//public apMatrix3x3 GetMatrixRigging(int vertexIndex)
		//{
		//	return _result_RiggingMatrices[vertexIndex];
		//}

		//public Vector2 GetVertexRigging(int vertexIndex)
		//{
		//	return _result_Rigging[vertexIndex];
		//}

		//public float GetRiggingWeight()
		//{
		//	return _result_RiggingWeight;
		//}

		//public Vector2 GetVertexLocalPos(int vertexIndex)
		//{
		//	return _result_VertLocal[vertexIndex];
		//}


		//추가 22.3.28 [v1.4.0] : Pin
		//public Vector2 GetPinLocalPos(int pinIndex)
		//{
		//	return _result_PinLocal[pinIndex];
		//}
		
			


		public apMatrix3x3 MeshWorldMatrix
		{
			get
			{
				if (_isAnyTransformation)
				{
					return _result_MeshTransform.MtrxToSpace;
				}
				return apMatrix3x3.identity;
			}
		}

		public apMatrix MeshWorldMatrixWrap
		{
			get
			{
				if (_isAnyTransformation)
				{
					return _result_MeshTransform;
				}
				return null;
			}
		}

		public Vector2 GetVertexWorldPos(int vertexIndex)
		{
			return _result_VertWorld[vertexIndex];
		}

		/// <summary>
		/// MeshColor/Visible이 Modifier로 계산이 되었는가
		/// </summary>
		public bool IsAnyColorCalculated
		{
			get
			{
				return _isAnyMeshColor && _result_CalculatedColor;
			}
		}

		public Color MeshColor
		{
			get
			{
				if (_isAnyMeshColor)
				{
					return _result_Color;
				}
				return _color_Default;
			}
		}

		public bool IsMeshVisible
		{
			get
			{
				if (_isAnyMeshColor)
				{
					return _result_IsVisible;
				}
				return true;
			}
		}

		//추가 11.30 : Extra Option
		public bool IsExtraDepthChanged
		{
			get
			{
				return _isAnyExtra && _result_IsExtraDepthChanged;
			}
		}

		public bool IsExtraTextureChanged
		{
			get
			{
				return _isAnyExtra && _result_IsExtraTextureChanged;
			}
		}

		public int ExtraDeltaDepth
		{
			get {  return _result_ExtraDeltaDepth; }
		}

		public apTextureData ExtraTextureData
		{
			get {  return _result_ExtraTextureData; }
		}

	}

}