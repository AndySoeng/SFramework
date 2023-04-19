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

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;

//using AnyPortrait;

//namespace AnyPortrait
//{

//	/// <summary>
//	/// Calculate 과정에서 어떤 계산과 어던 객체가 참조되었는지 기록하는 클래스
//	/// Log들이 저장되는게 아니라, 해당 계산 과정의 Log 타입들이 미리 정의되고, 각각이 연결되는 타입이다.
//	/// 크게 Modified -> LayeredStack -> CalculatedResultStackLayer-5으로 구분된다.
//	/// 값을 일일이 저장하기 보다는, 레퍼런스와 연결 방식만 저장하고, 필요할때 계산하자! (저장 방식이 모두 다르다)
//	/// </summary>
//	public class apCalculatedLog
//	{
//		// Members
//		//-----------------------------------------------
//		public enum LOG_TYPE
//		{
//			/// <summary>ModMesh/ModBone의 기록을 저장한다.</summary>
//			Modified,

//			Layer_ParamSetGroup,
//			Layer_CalParamResult,

//			CalResultStackLayer_0_Rigging,
//			CalResultStackLayer_1_StaticMesh,
//			CalResultStackLayer_2_VertLocal,
//			CalResultStackLayer_3_MeshTransform,
//			CalResultStackLayer_4_VertWorld,

//			Transform_Modified,
//		}


//		public LOG_TYPE _logType = LOG_TYPE.Modified;

//		public bool _isWeight = false;
//		public float _weight = 1.0f;
//		public BLEND_METHOD _layerBlendType = BLEND_METHOD.Additive;

//		public enum BLEND_METHOD
//		{
//			Additive = 0,
//			Interpolation = 1,

//		}

//		//Modified인 경우
//		public apModifierBase _modifier = null;
//		public apModifiedMesh _modMesh = null;
//		public apModifiedBone _modBone = null;

//		//Layer_ParamSetGroup인 경우
//		public apModifierParamSetGroup _paramSetGroup = null;
//		public int _paramSetGroupLayerIndex = -1;
//		//public apModifierParamSetGroup.BLEND_METHOD _paramSetGroupBlendType = apModifierParamSetGroup.BLEND_METHOD.Interpolation;
//		public apModifierParamSetGroupVertWeight _paramSetGroupVertWeight = null;//부분 vertWeight만 적용하는 경우

//		//Layer_CalParamResult인 경우
//		public apCalculatedResultParam _calResultParam = null;
//		public int _calResultParamLayerIndex = -1;
//		//public apModifierBase.BLEND_METHOD _calResultParamBlendType = apModifierBase.BLEND_METHOD.Interpolation;

//		//링크 기록
//		//Mod~Layer 단계에서는 Parent-Child로 연결이 된다.
//		//매번 초기화한 뒤 연결을 추가해야한다.
//		public apCalculatedLog _parentLayerLog;
//		public List<apCalculatedLog> _childLayerLogs = new List<apCalculatedLog>();

//		//Transform은 3개가 서로 연결된다.
//		public apCalculatedLog _linkedTF_Modified = null;
//		public apCalculatedLog _linkedTF_ToParent = null;
//		public apCalculatedLog _linkedTF_ParentWorld = null;
//		public apCalculatedLog _linkedTF_WorldTransform = null;

//		//public apRenderUnit _randerUnit = null;
//		public apTransform_Mesh _meshTransform = null;

//		//CalResultStackLayer는 5개가 서로 연결된다.
//		public apCalculatedLog _linkedStackLayer_0_Rigging = null;
//		public apCalculatedLog _linkedStackLayer_1_StaticMesh = null;
//		public apCalculatedLog _linkedStackLayer_2_VertLocal = null;
//		public apCalculatedLog _linkedStackLayer_3_MeshTransform = null;
//		public apCalculatedLog _linkedStackLayer_4_VertWorld = null;

//		public apCalculatedResultStack _calculateResultStack = null;


//		//CalParamResult->CalResultStackLayer로 이어지는 경우
//		public List<apCalculatedLog> _childCalParamResultLogs = new List<apCalculatedLog>();
//		public apCalculatedLog _parentCalResultStackLayer = null;

//		//StackLayer -> Transform_Modified로 이어지는 경우
//		public apCalculatedLog _parentTF_Modified = null;
//		public apCalculatedLog _childCalStackLayer_TF = null;


//		//처리 요청
//		private InverseResult _inverseResult = new InverseResult();


//		// Init
//		//-----------------------------------------------
//		/// <summary>Modified의 초기화</summary>
//		public apCalculatedLog(apModifiedMesh modMesh)
//		{
//			_logType = LOG_TYPE.Modified;
//			_modMesh = modMesh;
//		}

//		/// <summary>Modified의 초기화</summary>
//		public apCalculatedLog(apModifiedBone modBone)
//		{
//			_logType = LOG_TYPE.Modified;
//			_modBone = modBone;
//		}

//		/// <summary>Layer_ParamSetGroup의 초기화</summary>
//		/// <param name="paramSetGroup"></param>
//		public apCalculatedLog(apModifierParamSetGroup paramSetGroup)
//		{
//			_logType = LOG_TYPE.Layer_ParamSetGroup;
//			_paramSetGroup = paramSetGroup;
//		}

//		/// <summary>
//		/// CalResultParam의 초기화
//		/// </summary>
//		/// <param name="calResultParam"></param>
//		public apCalculatedLog(apCalculatedResultParam calResultParam)
//		{
//			_logType = LOG_TYPE.Layer_CalParamResult;
//			_calResultParam = calResultParam;
//		}

//		/// <summary>
//		/// Transform에서의 로그 초기화
//		/// </summary>
//		/// <param name="renderUnit"></param>
//		public apCalculatedLog(apTransform_Mesh meshTransform)
//		{
//			_logType = LOG_TYPE.Transform_Modified;
//			_meshTransform = meshTransform;
//		}

//		/// <summary>
//		/// CalResultStackLayer에서의 로그 초기화
//		/// </summary>
//		/// <param name="logType"></param>
//		/// <param name="calculateResultStack"></param>
//		public apCalculatedLog(LOG_TYPE logType, apCalculatedResultStack calculateResultStack)
//		{
//			_logType = logType;
//			_calculateResultStack = calculateResultStack;
//		}


//		// Functions
//		//-----------------------------------------------
//		/// <summary>
//		/// 기록을 초기화 한다.
//		/// </summary>
//		public void ReadyToRecord()
//		{
//			_parentLayerLog = null;
//			_childLayerLogs.Clear();
//			_childCalParamResultLogs.Clear();
//			_childCalStackLayer_TF = null;

//			_parentCalResultStackLayer = null;

//			_parentTF_Modified = null;
//			_childCalStackLayer_TF = null;

//			_paramSetGroupLayerIndex = -1;
//			_calResultParamLayerIndex = -1;
//		}





//		// Link
//		private void LinkLog_ModLayerParent(apCalculatedLog parentLayer)
//		{
//			_parentLayerLog = parentLayer;
//			if (!parentLayer._childLayerLogs.Contains(this))
//			{
//				parentLayer._childLayerLogs.Add(this);
//			}
//		}
//		public void LinkLog_Transform(apCalculatedLog modified, apCalculatedLog toParent, apCalculatedLog parentWorld, apCalculatedLog worldTransform)
//		{
//			_linkedTF_Modified = modified;
//			_linkedTF_ToParent = toParent;
//			_linkedTF_ParentWorld = parentWorld;
//			_linkedTF_WorldTransform = worldTransform;
//		}

//		public void LinkLog_CalResultStackLayer(apCalculatedLog layer0_rigging,
//												apCalculatedLog layer1_StaticMesh,
//												apCalculatedLog layer2_VertLocal,
//												apCalculatedLog layer3_MeshTransform,
//												apCalculatedLog layer4_VertWorld
//												)
//		{
//			_linkedStackLayer_0_Rigging = layer0_rigging;
//			_linkedStackLayer_1_StaticMesh = layer1_StaticMesh;
//			_linkedStackLayer_2_VertLocal = layer2_VertLocal;
//			_linkedStackLayer_3_MeshTransform = layer3_MeshTransform;
//			_linkedStackLayer_4_VertWorld = layer4_VertWorld;
//		}


//		/// <summary>
//		/// CalResultStackLayer 하위에서 계산된 CalResultParam의 Log를 연결하는 함수
//		/// CalResultParam -> CalResultStackLayer으로 연결할때의 함수
//		/// </summary>
//		/// <param name="calResultParam"></param>
//		public void LinkLog_ResultParam(apCalculatedLog calResultParam)
//		{
//			if (!_childCalParamResultLogs.Contains(calResultParam))
//			{
//				_childCalParamResultLogs.Add(calResultParam);
//			}
//			calResultParam._parentCalResultStackLayer = this;
//		}

//		/// <summary>
//		/// MeshTransform_Modified에 CalStackResult-TF의 Lof를 연결하는 함수
//		/// CalResultStackLayer (TF) -> MeshTrasnform으로 연결할때의 함수
//		/// </summary>
//		/// <param name="calResultStackTF"></param>
//		public void LinkLog_CalculateResultStackTF(apCalculatedLog calResultStackTF)
//		{
//			_childCalStackLayer_TF = calResultStackTF;
//			calResultStackTF._parentTF_Modified = this;
//		}





//		// 계산 갱신
//		//Modified,

//		//	Layer_ParamSetGroup,
//		//	Layer_CalParamResult,

//		//	Transform_Modified,
//		//	Transform_ToParent,
//		//	Transform_ParentWorld,
//		//	Transform_WorldTransform,//<<위 3개(또는 1개)가 완성

//		//	CalResultStackLayer_0_Rigging,
//		//	CalResultStackLayer_1_StaticMesh,
//		//	CalResultStackLayer_2_VertLocal,
//		//	CalResultStackLayer_3_MeshTransform,
//		//	CalResultStackLayer_4_VertWorld
//		//1) Modified
//		public void CalculateModified(float weight, apCalculatedLog paramSetGroupLog)
//		{
//			_weight = weight;
//			LinkLog_ModLayerParent(paramSetGroupLog);
//		}

//		//2) Layer_ParamSetGroup
//		public void CalculateParamSetGroup(float layerWeight,
//			int iLayer,
//			apModifierParamSetGroup.BLEND_METHOD blendType,
//			apModifierParamSetGroupVertWeight paramSetGroupVertWeight,
//			apCalculatedLog calPraramResultLog
//			)
//		{
//			_weight = layerWeight;
//			_paramSetGroupLayerIndex = iLayer;
//			switch (blendType)
//			{
//				case apModifierParamSetGroup.BLEND_METHOD.Additive:
//					_layerBlendType = BLEND_METHOD.Additive;
//					break;

//				case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
//					_layerBlendType = BLEND_METHOD.Interpolation;
//					break;
//			}
//			//_paramSetGroupBlendType = blendType;
//			_paramSetGroupVertWeight = paramSetGroupVertWeight;//<<보통은 Null
//			LinkLog_ModLayerParent(calPraramResultLog);
//		}

//		//3) Layer_CalParamResult
//		public void Calculate_CalParamResult(float layerWeight,
//												int iLayer,
//												apModifierBase.BLEND_METHOD blendMethod,
//												apCalculatedLog resultStackLayerLog
//												)
//		{
//			_weight = layerWeight;
//			_calResultParamLayerIndex = iLayer;
//			//_calResultParamBlendType = blendMethod;
//			switch (blendMethod)
//			{
//				case apModifierBase.BLEND_METHOD.Additive:
//					_layerBlendType = BLEND_METHOD.Additive;
//					break;

//				case apModifierBase.BLEND_METHOD.Interpolation:
//					_layerBlendType = BLEND_METHOD.Interpolation;
//					break;
//			}
//			resultStackLayerLog.LinkLog_ResultParam(this);
//		}


//		// CalculateRequest
//		//------------------------------------------------------------
//		public class InverseResult
//		{
//			public Vector2 _posW_next;
//			public Vector2 _posW_prev;
//			public Vector2 _deltaPosW;

//			public bool _isSuccess = false;
//			public Vector2 _posL_prev;
//			public Vector2 _posL_next;
//			public Vector2 _deltaPosL;


//			public InverseResult()
//			{
//				_isSuccess = false;
//			}

//			public void Request(Vector2 curPosW, Vector2 deltaPosW)
//			{
//				_posW_prev = curPosW;
//				_posW_next = _posW_prev + deltaPosW;

//				_isSuccess = false;
//				_posL_prev = Vector2.zero;
//				_posL_next = Vector2.zero;
//				_deltaPosL = Vector2.zero;
//			}

//			public void SetResult(Vector2 posL_Prev, Vector2 posL_Next)
//			{
//				_isSuccess = true;
//				_posL_prev = posL_Prev;
//				_posL_next = posL_Next;
//				_deltaPosL = _posL_next - _posL_prev;
//			}

//		}
//		/// <summary>
//		/// Modified 단계에서 위로 검색하여 현재 World Position으로부터 다시 Local로 내려온다.
//		/// 결과값은 ModMesh(TF)의 TransformMatrix에 사용될 값으로 변환된다.
//		/// ModMesh에서 호출해야한다.
//		/// </summary>
//		/// <param name="deltaPosW"></param>
//		/// <returns></returns>
//		//public InverseResult World2ModLocalPos_TransformMove(Vector2 nextPosW)
//		public InverseResult World2ModLocalPos_TransformMove(Vector2 nextPosW, Vector2 deltaPosW)
//		{
//			if (_modMesh == null)
//			{
//				Debug.LogError("World2ModLocalPos_Transform 실패 : ModMesh에서 호출하지 않았다.");
//				return null;
//			}
//			if (_modMesh._transform_Mesh == null)
//			{
//				Debug.LogError("World2ModLocalPos_Transform 실패 : ModMesh의 MeshTransform이 Null이다.");
//				return null;
//			}
//			//현재 위치를 받자
//			Vector2 posW_prev = _modMesh._transform_Mesh._matrix_TFResult_World._pos;
//			//Vector2 deltaPosW = nextPosW - posW_prev;
//			nextPosW = posW_prev + deltaPosW;
//			_inverseResult.Request(posW_prev, deltaPosW);

//			//이제 위로 하나씩 계산하면서 어떤 CalculateLog가 기록되었는지 확인하자
//			//StatckLResultLayer까지 올라가면 된다. (거기는 값이 고정이므로)
//			apCalculatedLog modified = this;
//			apCalculatedLog layer_ParamSetGroup = modified._parentLayerLog;
//			apCalculatedLog layer_CalParamResult = layer_ParamSetGroup._parentLayerLog;
//			apCalculatedLog stackLayer_MeshTransform = layer_CalParamResult._parentCalResultStackLayer;
//			apCalculatedLog transform_Modified = stackLayer_MeshTransform._parentTF_Modified;

//			//Debug.Log("Calculate Log");
//			//이제 역으로 내려오자
//			Vector2 posW_next = nextPosW;
//			apMatrix worldMatrix = transform_Modified._meshTransform._matrix_TFResult_World;

//			// ParentWorld <- Modified <- ToParent = World
//			// [Modified <- ToParent] = [ParentWorld-1 <- World]
//			// Modified = [ParentWorld-1 <- World <- ToParent-1]


//			apMatrix worldMatrix_next = new apMatrix(worldMatrix);
//			worldMatrix_next.SetPos(worldMatrix_next._pos + deltaPosW);


//			apMatrix modMatrix_next = transform_Modified._meshTransform._matrix_TF_ToParent.RInverseMatrix;
//			modMatrix_next.RMultiply(worldMatrix_next);
//			modMatrix_next.RInverse(transform_Modified._meshTransform._matrix_TF_ParentWorld);


//			//위 수식은 "StackLayer-MeshTF" 레벨에서 계산된 것이다.
//			//StackLayer -> CalParam -> ParamSetGroup으로 이동하자.
//			apMatrix modMatrix_paramSet = GetInverseInterpolatedMatrix(layer_ParamSetGroup, layer_CalParamResult, stackLayer_MeshTransform, modMatrix_next);

//			if (modMatrix_paramSet == null)
//			{
//				Debug.LogError("StackLayer -> CalParam -> ParamSetGroup 전환에 실패했다.");
//				return null;
//			}
//			//_inverseResult.SetResult(transform_Modified._meshTransform._matrix_TF_LocalModified._pos,
//			//	modMatrix_next._pos);

//			_inverseResult.SetResult(transform_Modified._meshTransform._matrix_TF_LocalModified._pos,
//				modMatrix_paramSet._pos);




//			return _inverseResult;
//		}


//		public InverseResult World2ModLocalPos_TransformRotationScaling(float deltaRotateAngle, Vector2 deltaScale)
//		{
//			if (_modMesh == null)
//			{
//				Debug.LogError("World2ModLocalPos_Transform 실패 : ModMesh에서 호출하지 않았다.");
//				return null;
//			}
//			if (_modMesh._transform_Mesh == null)
//			{
//				Debug.LogError("World2ModLocalPos_Transform 실패 : ModMesh의 MeshTransform이 Null이다.");
//				return null;
//			}
//			//현재 위치를 받자


//			//이제 위로 하나씩 계산하면서 어떤 CalculateLog가 기록되었는지 확인하자
//			//StatckLResultLayer까지 올라가면 된다. (거기는 값이 고정이므로)
//			apCalculatedLog modified = this;
//			apCalculatedLog layer_ParamSetGroup = modified._parentLayerLog;
//			apCalculatedLog layer_CalParamResult = layer_ParamSetGroup._parentLayerLog;
//			apCalculatedLog stackLayer_MeshTransform = layer_CalParamResult._parentCalResultStackLayer;
//			apCalculatedLog transform_Modified = stackLayer_MeshTransform._parentTF_Modified;

//			//현재 posW 를 저장하자
//			Vector2 posW_prev = _modMesh._transform_Mesh._matrix_TFResult_World._pos;

//			//일단 회전값/스케일을 추가하자
//			apMatrix RSModMatrix = new apMatrix(_modMesh._transform_Mesh._matrix_TF_LocalModified);
//			RSModMatrix.SetRotate(RSModMatrix._angleDeg + deltaRotateAngle);
//			RSModMatrix.SetScale(RSModMatrix._scale + deltaScale);
//			apMatrix RSWorldMatrix = new apMatrix(_modMesh._transform_Mesh._matrix_TF_ToParent);
//			RSWorldMatrix.RMultiply(RSModMatrix);
//			RSWorldMatrix.RMultiply(_modMesh._transform_Mesh._matrix_TF_ParentWorld);

//			//이 WorldMatrix의 계산 후 Position을 확인하자
//			Vector2 posW_rs = RSWorldMatrix._pos;

//			//posW_rotated가 아닌 posW_prev로 돌려야 한다. 고치자.
//			Vector2 deltaPosW = posW_prev - posW_rs;
//			_inverseResult.Request(posW_rs, deltaPosW);


//			//이제 역으로 내려오자
//			// ParentWorld <- Modified <- ToParent = World
//			// [Modified <- ToParent] = [ParentWorld-1 <- World]
//			// Modified = [ParentWorld-1 <- World <- ToParent-1]


//			apMatrix worldMatrix_next = new apMatrix(RSWorldMatrix);
//			worldMatrix_next.SetPos(worldMatrix_next._pos + deltaPosW);


//			apMatrix modMatrix_next = transform_Modified._meshTransform._matrix_TF_ToParent.RInverseMatrix;
//			modMatrix_next.RMultiply(worldMatrix_next);
//			modMatrix_next.RInverse(transform_Modified._meshTransform._matrix_TF_ParentWorld);

//			//위 수식은 "StackLayer-MeshTF" 레벨에서 계산된 것이다.
//			//StackLayer -> CalParam -> ParamSetGroup으로 이동하자.
//			apMatrix modMatrix_paramSet = GetInverseInterpolatedMatrix(layer_ParamSetGroup, layer_CalParamResult, stackLayer_MeshTransform, modMatrix_next);

//			if (modMatrix_paramSet == null)
//			{
//				Debug.LogError("StackLayer -> CalParam -> ParamSetGroup 전환에 실패했다.");
//				return null;
//			}
//			//_inverseResult.SetResult(transform_Modified._meshTransform._matrix_TF_LocalModified._pos,
//			//	modMatrix_next._pos);

//			_inverseResult.SetResult(transform_Modified._meshTransform._matrix_TF_LocalModified._pos,
//				modMatrix_paramSet._pos);


//			return _inverseResult;
//		}


//		/// <summary>
//		/// 변형된 modMatrix에 대해서 StackLayer, CalParamResult, ParamSet 상에서 레이어 보간된 연산을 역으로 수행하여
//		/// 요청된 Target Modified Matrix 레벨에서 어떤 값을 가져야 하는지 리턴한다.
//		/// [Matrix에 대한 역 보간처리 함수]
//		/// </summary>
//		/// <param name="targetModifiedLog"></param>
//		/// <param name="layer_ParamSetGroupLog"></param>
//		/// <param name="layer_CalParamResultLog"></param>
//		/// <param name="stackLayer_MeshTransformLog"></param>
//		/// <param name="modMatrix_next"></param>
//		/// <returns></returns>
//		private apMatrix GetInverseInterpolatedMatrix(apCalculatedLog layer_ParamSetGroupLog,
//														apCalculatedLog layer_CalParamResultLog,
//														apCalculatedLog stackLayer_MeshTransformLog,
//														apMatrix modMatrix_next
//													)
//		{
//			//1. StackLayer -> CalParamResultLog
//			if (!stackLayer_MeshTransformLog._childCalParamResultLogs.Contains(layer_CalParamResultLog))
//			{
//				return null;
//			}

//			//Debug.Log("GetInvMatrix");
//			//for (int i = 0; i < stackLayer_MeshTransformLog._childCalParamResultLogs.Count; i++)
//			//{
//			//	Debug.Log("Cal Param [" + i + "] : " + stackLayer_MeshTransformLog._childCalParamResultLogs[i]._calResultParam._linkedModifier.DisplayName + " / " 
//			//		+ stackLayer_MeshTransformLog._childCalParamResultLogs[i]._layerBlendType + " (" + stackLayer_MeshTransformLog._childCalParamResultLogs[i]._weight + ")");

//			//}
//			//for (int i = 0; i < layer_CalParamResultLog._childLayerLogs.Count; i++)
//			//{
//			//	Debug.Log("ParamSetGroup [" + i + "] : " + layer_CalParamResultLog._childLayerLogs[i]._paramSetGroup._keyControlParam._keyName + " / "
//			//		+ layer_CalParamResultLog._childLayerLogs[i]._layerBlendType + " ( " + layer_CalParamResultLog._childLayerLogs[i]._weight + ")"
//			//		+ "\r\n" + layer_CalParamResultLog._childLayerLogs[i]._paramSetGroup._tmpMatrix);

//			//}


//			//1. StackLayer -> CalParamResultLog
//			apMatrix modMatrix_CalParamResultLog = GetSubInverseLayeredMatrix(layer_CalParamResultLog, stackLayer_MeshTransformLog, modMatrix_next);
//			if (modMatrix_CalParamResultLog == null)
//			{
//				Debug.LogError("GetInverseInterpolatedMatrix 실패 : StackLayer -> CalParamResultLog를 처리할 수 없다.");
//				return null;
//			}
//			//Debug.Log("Calculate : 1. StackLayer -> CalParamResultLog\r\n" + modMatrix_next + "\r\n >>>> \r\n" + modMatrix_CalParamResultLog);


//			//2. CalParamResultLog -> ParamSetGroupLog (ParamSet은 하나만 선택했으니 ParamSet 레벨과 동일하다)
//			apMatrix modMatrix_ParamSetGroupLog = GetSubInverseLayeredMatrix(layer_ParamSetGroupLog, layer_CalParamResultLog, modMatrix_CalParamResultLog);
//			if (modMatrix_ParamSetGroupLog == null)
//			{
//				Debug.LogError("GetInverseInterpolatedMatrix 실패 : StackLayer -> CalParamResultLog를 처리할 수 없다.");
//				return null;
//			}

//			//Debug.Log("Calculate : 2. CalParamResultLog -> ParamSetGroupLog\r\n" + modMatrix_CalParamResultLog + "\r\n >>>> \r\n" + modMatrix_ParamSetGroupLog);


//			return modMatrix_ParamSetGroupLog;



//		}

//		public apMatrix GetSubInverseLayeredMatrix(apCalculatedLog targetLog, apCalculatedLog parentLog, apMatrix parentMatrix)
//		{
//			apMatrix invUpperLayerMatrix = new apMatrix(parentMatrix);
//			apMatrix lowerLayerMatrix = new apMatrix();//타겟보다 낮은 레이어는 따로 합을 구한다.

//			LOG_TYPE layerLogType = targetLog._logType;

//			int nLayers = 0;
//			int iLayer = -1;

//			if (layerLogType == LOG_TYPE.Layer_CalParamResult)
//			{
//				nLayers = parentLog._childCalParamResultLogs.Count;
//				iLayer = parentLog._childCalParamResultLogs.IndexOf(targetLog);
//			}
//			else if (layerLogType == LOG_TYPE.Layer_ParamSetGroup)
//			{
//				nLayers = parentLog._childLayerLogs.Count;
//				iLayer = parentLog._childLayerLogs.IndexOf(targetLog);
//			}
//			else
//			{
//				Debug.LogError("Layer가 없는 LogType [" + layerLogType + "] 이다");
//				return null;
//			}

//			if (iLayer < 0)
//			{
//				Debug.LogError("존재하지 않는 Log [" + layerLogType + "]");
//				return null;
//			}



//			lowerLayerMatrix.SetIdentity();

//			//Debug.LogWarning(">> Lower To Layer [0 > " + iLayer + "]");
//			//낮은 레이어에서는 값을 더하자
//			if (iLayer > 0)
//			{
//				for (int i = 0; i < iLayer; i++)
//				{
//					apCalculatedLog calLog = null;
//					apMatrix calMatrix = null;
//					switch (layerLogType)
//					{
//						case LOG_TYPE.Layer_CalParamResult:
//							calLog = parentLog._childCalParamResultLogs[i];
//							calMatrix = calLog._calResultParam._result_Matrix;
//							break;

//						case LOG_TYPE.Layer_ParamSetGroup:
//							calLog = parentLog._childLayerLogs[i];
//							calMatrix = calLog._paramSetGroup._tmpMatrix;
//							break;
//					}

//					float weight = calLog._weight;
//					if (!calLog._isWeight)
//					{
//						weight = 1.0f;
//					}

//					//string prevMatrix = lowerLayerMatrix.ToString();

//					if (i == 0)
//					{
//						lowerLayerMatrix.SetPos(calMatrix._pos * weight);
//						lowerLayerMatrix.SetRotate(calMatrix._angleDeg * weight);
//						lowerLayerMatrix.SetScale(calMatrix._scale * weight + Vector2.one * (1.0f - weight));
//						lowerLayerMatrix.MakeMatrix();
//					}
//					else
//					{
//						switch (calLog._layerBlendType)
//						{
//							case BLEND_METHOD.Interpolation:
//								BlendMatrix_ITP(lowerLayerMatrix, calMatrix, weight);
//								//Debug.Log("[" + i + "] ITP/" + weight + " : " + prevMatrix + " >> " + calMatrix + " => " + lowerLayerMatrix);
//								break;

//							case BLEND_METHOD.Additive:
//								BlendMatrix_Add(lowerLayerMatrix, calMatrix, weight);
//								//Debug.Log("[" + i + "] ADD/" + weight + " : " + prevMatrix + " >> " + calMatrix + " => " + lowerLayerMatrix);
//								break;
//						}
//					}
//				}
//			}

//			//Debug.LogWarning(">> Upper To Layer [" + (nLayers - 1) + " > " + iLayer + "]");
//			//위 레이어에서는 값을 빼자
//			if (iLayer < nLayers - 1)
//			{
//				for (int i = (nLayers - 1); i >= iLayer + 1; i--)
//				{
//					apCalculatedLog calLog = null;
//					apMatrix calMatrix = null;
//					switch (layerLogType)
//					{
//						case LOG_TYPE.Layer_CalParamResult:
//							calLog = parentLog._childCalParamResultLogs[i];
//							calMatrix = calLog._calResultParam._result_Matrix;
//							break;

//						case LOG_TYPE.Layer_ParamSetGroup:
//							calLog = parentLog._childLayerLogs[i];
//							calMatrix = calLog._paramSetGroup._tmpMatrix;
//							break;
//					}


//					float weight = calLog._weight;
//					if (!calLog._isWeight)
//					{
//						weight = 1.0f;
//					}
//					switch (calLog._layerBlendType)
//					{
//						case BLEND_METHOD.Interpolation:
//							InverseBlendMatrix_ITP(invUpperLayerMatrix, calMatrix, weight);
//							break;

//						case BLEND_METHOD.Additive:
//							InverseBlendMatrix_Add(invUpperLayerMatrix, calMatrix, weight);
//							break;
//					}
//				}
//			}

//			apMatrix result = null;
//			float weight_calParam = targetLog._weight;
//			if (!targetLog._isWeight)
//			{
//				weight_calParam = 1.0f;
//			}

//			apMatrix calTargetMatrix = null;
//			switch (layerLogType)
//			{
//				case LOG_TYPE.Layer_CalParamResult:
//					calTargetMatrix = targetLog._calResultParam._result_Matrix;
//					break;

//				case LOG_TYPE.Layer_ParamSetGroup:
//					calTargetMatrix = targetLog._paramSetGroup._tmpMatrix;
//					break;
//			}

//			switch (targetLog._layerBlendType)
//			{
//				case BLEND_METHOD.Additive:
//					result = InverseBlendMatrixTarget_Add(invUpperLayerMatrix, lowerLayerMatrix, calTargetMatrix, weight_calParam);
//					break;

//				case BLEND_METHOD.Interpolation:
//					result = InverseBlendMatrixTarget_ITP(invUpperLayerMatrix, lowerLayerMatrix, calTargetMatrix, weight_calParam);
//					break;
//			}
//			if (result == null)
//			{
//				return null;
//			}

//			return result;
//		}




//		public void BlendMatrix_ITP(apMatrix curMatrix, apMatrix itpLayer, float itpWeight)
//		{
//			curMatrix.LerpMartix(itpLayer, itpWeight);
//		}

//		public void BlendMatrix_Add(apMatrix curMatrix, apMatrix itpLayer, float itpWeight)
//		{
//			curMatrix._pos += itpLayer._pos * itpWeight;
//			curMatrix._angleDeg += itpLayer._angleDeg * itpWeight;

//			//Scale은 Multiply의 보간 방식
//			curMatrix._scale.x = (curMatrix._scale.x * (1.0f - itpWeight)) + (curMatrix._scale.x * itpLayer._scale.x * itpWeight);
//			curMatrix._scale.y = (curMatrix._scale.y * (1.0f - itpWeight)) + (curMatrix._scale.y * itpLayer._scale.y * itpWeight);
//			//curMatrix._scale.z = (curMatrix._scale.z * (1.0f - itpWeight)) + (curMatrix._scale.z * itpLayer._scale.z * itpWeight);
//		}






//		/// <summary>
//		/// BlendMatrix_ITP의 역연산. [ (curMatrix - (subtractedLayer * weight)) / (1-weight) ]
//		/// weight가 1이면 Identity로 만든다.
//		/// </summary>
//		/// <param name="curMatrix"></param>
//		/// <param name="subtractedLayer"></param>
//		/// <param name="weight"></param>
//		public void InverseBlendMatrix_ITP(apMatrix curMatrix, apMatrix subtractedLayer, float weight)
//		{
//			if (weight >= 1.0f)
//			{
//				curMatrix.SetIdentity();
//				return;
//			}
//			if (weight <= 0.0f)
//			{
//				//처리하지 않는다.
//				return;
//			}

//			curMatrix._pos.x = (curMatrix._pos.x - subtractedLayer._pos.x * weight) / (1.0f - weight);
//			curMatrix._pos.y = (curMatrix._pos.y - subtractedLayer._pos.y * weight) / (1.0f - weight);

//			curMatrix._angleDeg = (curMatrix._angleDeg - subtractedLayer._angleDeg * weight) / (1.0f - weight);

//			curMatrix._scale.x = (curMatrix._scale.x - subtractedLayer._scale.x * weight) / (1.0f - weight);
//			curMatrix._scale.y = (curMatrix._scale.y - subtractedLayer._scale.y * weight) / (1.0f - weight);
//			//curMatrix._scale.z = (curMatrix._scale.z - subtractedLayer._scale.z * weight) / (1.0f - weight);

//			curMatrix.MakeMatrix();

//		}

//		/// <summary>
//		/// BlendMatrix_Add의 역연산 [ (curMatrix - subtractedLayer * weight) ]
//		/// </summary>
//		/// <param name="curMatrix"></param>
//		/// <param name="subtractedLayer"></param>
//		/// <param name="weight"></param>
//		public void InverseBlendMatrix_Add(apMatrix curMatrix, apMatrix subtractedLayer, float weight)
//		{
//			if (weight <= 0.0f)
//			{
//				//처리하지 않는다.
//				return;
//			}

//			//curMatrix._pos = curMatrix._pos + itpLayer._pos * itpWeight;
//			//Result = Prev + Next * Weight
//			//Prev = Result - (Next * Weight)

//			curMatrix._pos = curMatrix._pos - subtractedLayer._pos * weight;
//			curMatrix._angleDeg = curMatrix._angleDeg - subtractedLayer._angleDeg * weight;

//			//curMatrix._scale.x = (curMatrix._scale.x * (1.0f - itpWeight)) + (curMatrix._scale.x * itpLayer._scale.x * itpWeight);
//			//Result = Prev * (1 - Weight) + (Prev * Next * Weight)
//			//Result = Prev * (1 - Weight + Next * Weight)
//			//Prev = Result / ((1 - Weight) + Next * Weight)
//			float divScaleX = (1.0f - weight) + subtractedLayer._scale.x * weight;
//			float divScaleY = (1.0f - weight) + subtractedLayer._scale.y * weight;
//			//float divScaleZ = (1.0f - weight) + subtractedLayer._scale.z * weight;

//			if (divScaleX == 0.0f)
//			{ curMatrix._scale.x = 0.0f; }
//			else
//			{ curMatrix._scale.x = curMatrix._scale.x / divScaleX; }

//			if (divScaleY == 0.0f)
//			{ curMatrix._scale.y = 0.0f; }
//			else
//			{ curMatrix._scale.y = curMatrix._scale.y / divScaleY; }

//			//if(divScaleZ == 0.0f)	{ curMatrix._scale.z = 0.0f; }
//			//else					{ curMatrix._scale.z = curMatrix._scale.z / divScaleZ; }

//		}



//		/// <summary>
//		/// BlendMatrix_ITP의 역연산 중 TargetLayer에서 최종 결과값을 만드는 과정.
//		/// [ X = (inverseUpperLayer - (lowerLayer * (1 - weight))) / (weight) ]
//		/// weight가 0이면 Null 리턴
//		/// </summary>
//		/// <param name="curMatrix"></param>
//		/// <param name="subtractedLayer"></param>
//		/// <param name="weight"></param>
//		public apMatrix InverseBlendMatrixTarget_ITP(apMatrix invUpperLayerResult, apMatrix lowerLayerResult, apMatrix targetLayer, float weight)
//		{
//			if (weight <= 0.0f)
//			{
//				return null;
//			}

//			// ( InvUpperLayer - (lowerLayer * (1-weight)) ) / weight
//			apMatrix result = new apMatrix();
//			result._pos.x = (invUpperLayerResult._pos.x - lowerLayerResult._pos.x * (1.0f - weight)) / weight;
//			result._pos.y = (invUpperLayerResult._pos.y - lowerLayerResult._pos.y * (1.0f - weight)) / weight;

//			result._angleDeg = (invUpperLayerResult._angleDeg - lowerLayerResult._angleDeg * (1.0f - weight)) / weight;

//			result._scale.x = (invUpperLayerResult._scale.x - lowerLayerResult._scale.x * (1.0f - weight)) / weight;
//			result._scale.y = (invUpperLayerResult._scale.y - lowerLayerResult._scale.y * (1.0f - weight)) / weight;
//			//result._scale.z = (invUpperLayerResult._scale.z - lowerLayerResult._scale.z * (1.0f - weight)) / weight;

//			result.MakeMatrix();

//			return result;
//		}


//		/// <summary>
//		/// BlendMatrix_Add의 역연산 중 TargetLayer에서 최종 결과값을 만드는 과정.
//		/// Pos/Angle : [ X = (inverseUpperLayer - lowerLayer) / weight ]
//		/// Scale : [ X = ( (inverseUpperLayer / lowerLayer) - 1 + weight ) / weight ]
//		/// weight가 0이면 Null리턴
//		/// </summary>
//		/// <param name="curMatrix"></param>
//		/// <param name="subtractedLayer"></param>
//		/// <param name="weight"></param>
//		public apMatrix InverseBlendMatrixTarget_Add(apMatrix invUpperLayerResult, apMatrix lowerLayerResult, apMatrix targetLayer, float weight)
//		{
//			if (weight <= 0.0f)
//			{
//				//처리하지 않는다.
//				return null;
//			}

//			apMatrix result = new apMatrix();

//			result._pos.x = (invUpperLayerResult._pos.x - lowerLayerResult._pos.x) / weight;
//			result._pos.y = (invUpperLayerResult._pos.y - lowerLayerResult._pos.y) / weight;
//			result._angleDeg = (invUpperLayerResult._angleDeg - lowerLayerResult._angleDeg) / weight;

//			result._scale.x = ((invUpperLayerResult._scale.x / lowerLayerResult._scale.x) - (1.0f - weight)) / weight;
//			result._scale.y = ((invUpperLayerResult._scale.y / lowerLayerResult._scale.y) - (1.0f - weight)) / weight;
//			//result._scale.z = ((invUpperLayerResult._scale.z / lowerLayerResult._scale.z) - (1.0f - weight)) / weight;

//			return result;

//		}

//		// Get / Set
//		//-----------------------------------------------

//	}
//}