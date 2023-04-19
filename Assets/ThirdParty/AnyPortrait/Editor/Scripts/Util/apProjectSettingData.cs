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
using System.IO;

using AnyPortrait;
using UnityEditor;

namespace AnyPortrait
{
	/// <summary>
	/// [1.4.2] 프로젝트의 설정을 저장하는 데이터. (Bake 다이얼로그의 속성들이다)
	/// 레지스트리로 저장되는 에디터 설정과 다르게 파일로 저장한다.
	/// Bake Setting의 일부의 데이터는 물론이고, 캐릭터별 공통의 설정 동기화를 위한 저장까지 한다.
	/// </summary>
	public class apProjectSettingData
	{
		// Members
		//-----------------------------------------------------------------------------
		// 저장되는 변수들
		//프로젝트 설정이 저장되었는지 여부
		
		//1. 프로젝트 설정
		private bool _prj_IsColorSpaceGamma = true;//Color Space (Gamma)		
		private bool _prj_IsUseSRP = false;//URP (Scriptable Render Pipeline)


		//2. 캐릭터 공통 설정들
		private bool _isCommonSettingSaved = false;//공통 설정이 저장되었는가

		//Sorting Layer
		private int _cmn_SortingLayerID = 0;
		private int _cmn_SortingOrder = 0;
		private apPortrait.SORTING_ORDER_OPTION _cmn_SortingLayerOption = apPortrait.SORTING_ORDER_OPTION.SetOrder;
		private int _cmn_SortingOrderPerDepth = 0;

		//메카님 사용 여부 (경로는 저장하지 않는다.)
		private bool _cmn_IsUsingMecanim = false;


		//Important
		private bool _cmn_isImportant = true;
		private int _cmn_FPS = 30;

		//빌보드
		private apPortrait.BILLBOARD_TYPE _cmn_BillboardOption = apPortrait.BILLBOARD_TYPE.None;
		private bool _cmn_IsForceCamSortModeToOrthographic = true;//빌보드시 Sort Mode를 Orthographic 강제하기 옵션

		//Shadow
		private apPortrait.SHADOW_CASTING_MODE _cmn_CastShadows = apPortrait.SHADOW_CASTING_MODE.Off;
		private bool _cmn_IsReceiveShadows = false;

		// VR 렌더 텍스쳐 사이즈 (이건 개별이다)
		private apPortrait.VR_SUPPORT_MODE _cmn_VRSupported = apPortrait.VR_SUPPORT_MODE.None;
		private apPortrait.VR_RT_SIZE _cmn_VRRtSize = apPortrait.VR_RT_SIZE.ByMeshSettings;

		// Flipped Mesh 검출법
		private apPortrait.FLIPPED_MESH_CHECK _cmn_FlippedMeshOption = apPortrait.FLIPPED_MESH_CHECK.TransformOnly;

		//루트 본의 스케일 옵션
		private apPortrait.ROOT_BONE_SCALE_METHOD _cmn_RootBoneScaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;

		//애니메이션 전환시의 지정되지 않은 값의 처리
		private apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM _cmn_UnspecifiedAnimControlParam = apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM.KeepLastValue;

		//텔레포트 보정
		private bool _cmn_TeleportCorrection = true;
		private float _cmn_TeleportDist = 0.5f;



		//저장/파싱 변수
		private apStringWrapper _strWrapper = null;


		private const string STR_0 = "0";
		private const string STR_00 = "00";
		private const string STR_DELIMETER = ":";

		//키값들
		private enum KEY_TYPES
		{
			None,

			//저장 여부
			CommonSettingSaved,

			//프로젝트 설정
			ColorSpaceGamma,
			UseSRP,


			//캐릭터별 설정의 공통값

			//Sorting Layer
			SortingLayerID,
			SortingOrder,
			SortingLayerOption,
			SortingOrderPerDepth,

			//메카님 사용 여부
			UsingMecanim,

			//Important 옵션
			IsImportant,
			FPS,

			//빌보드
			BillboardType,
			ForceCamSortToOrtho,

			//Shadow
			CastShadows,
			ReceiveShadows,

			//VR 설정
			VRSupported,
			VRRTSize,

			//Flipped Mesh 검출법
			FlippedMeshCheck,

			//루트 본의 스케일 옵션
			RootBoneScaleMethod,

			//애니메이션 전환시의 지정되지 않은 값의 처리
			UnspecifiedAnimControlParam,

			//텔레포트 보정
			TeleportCorrection,
			TeleportDist
		}


		private Dictionary<KEY_TYPES, string> _key2StrID = null;
		private Dictionary<string, KEY_TYPES> _strID2Key = null;

		private bool _isLoaded = false;
		public bool IsLoaded { get { return _isLoaded; } }

		// Init
		//-----------------------------------------------------------------------------
		public apProjectSettingData()
		{
			_strWrapper = new apStringWrapper(128);
			_isLoaded = false;

			InitKeyStrIDs();
			Clear();
		}

		public void Clear()
		{
			//저장이 안되었다고 초기화 (나머지 값은 필요없다)
			_isCommonSettingSaved = false;

			//1.4.2의 이전 버전에서의 값을 가져오기 위해 레지스트리를 조회한다.
			//일단 가져왔다면, 더이상 열지 않도록 Load 후에 키를 삭제한다. (그 전에는 삭제하면 안된다)
			
			//EditorPrefs.SetBool("AnyPortrait_IsBakeColorSpace_ToGamma", false);
			//EditorPrefs.SetBool("AnyPortrait_IsUseLWRPShader", true);

			if(EditorPrefs.HasKey("AnyPortrait_IsBakeColorSpace_ToGamma"))
			{
				_prj_IsColorSpaceGamma = EditorPrefs.GetBool("AnyPortrait_IsBakeColorSpace_ToGamma", true);//기본값은 true(Gamma)
			}
			else
			{
				_prj_IsColorSpaceGamma = true;
			}
			
			if(EditorPrefs.HasKey("AnyPortrait_IsUseLWRPShader"))
			{
				_prj_IsUseSRP = EditorPrefs.GetBool("AnyPortrait_IsUseLWRPShader", false);//기본값은 false(URP 사용 안함)
			}
			else
			{
				_prj_IsUseSRP = false;
			}
			
			
			
			

			//기본값은 원래의 Portrait 멤버 변수의 초기값을 이용해야한다.
			//버전 호환성 때문 (저장이 일부만 되었을 경우 초기값을 기준으로 판단해야한다.)
			_cmn_SortingLayerID = 0;
			_cmn_SortingOrder = 0;
			_cmn_SortingLayerOption = apPortrait.SORTING_ORDER_OPTION.SetOrder;
			_cmn_SortingOrderPerDepth = 1;

			_cmn_IsUsingMecanim = false;

			_cmn_isImportant = true;
			_cmn_FPS = 30;

			_cmn_BillboardOption = apPortrait.BILLBOARD_TYPE.None;
			_cmn_IsForceCamSortModeToOrthographic = true;

			_cmn_CastShadows = apPortrait.SHADOW_CASTING_MODE.Off;
			_cmn_IsReceiveShadows = false;

			_cmn_VRSupported = apPortrait.VR_SUPPORT_MODE.None;
			_cmn_VRRtSize = apPortrait.VR_RT_SIZE.ByMeshSettings;

			_cmn_FlippedMeshOption = apPortrait.FLIPPED_MESH_CHECK.TransformOnly;

			_cmn_RootBoneScaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;

			_cmn_UnspecifiedAnimControlParam = apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM.RevertToDefaultValue;

			_cmn_TeleportCorrection = false;
			_cmn_TeleportDist = 10.0f;
		}

		private void InitKeyStrIDs()
		{
			_key2StrID = new Dictionary<KEY_TYPES, string>();
			_strID2Key = new Dictionary<string, KEY_TYPES>();
			_key2StrID.Clear();
			_strID2Key.Clear();


			//저장 여부
			AddKeyStrID(KEY_TYPES.CommonSettingSaved, "CommonSettingSaved");

			//프로젝트 설정
			AddKeyStrID(KEY_TYPES.ColorSpaceGamma, "ColorSpaceGamma");
			AddKeyStrID(KEY_TYPES.UseSRP, "UseSRP");


			//캐릭터별 설정의 공통값

			//Sorting Layer
			AddKeyStrID(KEY_TYPES.SortingLayerID, "SortingLayerID");
			AddKeyStrID(KEY_TYPES.SortingOrder, "SortingOrder");
			AddKeyStrID(KEY_TYPES.SortingLayerOption, "SortingLayerOption");
			AddKeyStrID(KEY_TYPES.SortingOrderPerDepth, "SortingOrderPerDepth");

			//메카님 사용 여부
			AddKeyStrID(KEY_TYPES.UsingMecanim, "UsingMecanim");

			//Important 옵션
			AddKeyStrID(KEY_TYPES.IsImportant, "IsImportant");
			AddKeyStrID(KEY_TYPES.FPS, "FPS");

			//빌보드
			AddKeyStrID(KEY_TYPES.BillboardType, "BillboardType");
			AddKeyStrID(KEY_TYPES.ForceCamSortToOrtho, "ForceCamSortToOrtho");

			//Shadow
			AddKeyStrID(KEY_TYPES.CastShadows, "CastShadows");
			AddKeyStrID(KEY_TYPES.ReceiveShadows, "ReceiveShadows");

			//VR 설정
			AddKeyStrID(KEY_TYPES.VRSupported, "VRSupported");
			AddKeyStrID(KEY_TYPES.VRRTSize, "VRRTSize");

			//Flipped Mesh 검출법
			AddKeyStrID(KEY_TYPES.FlippedMeshCheck, "FlippedMeshCheck");

			//루트 본의 스케일 옵션
			AddKeyStrID(KEY_TYPES.RootBoneScaleMethod, "RootBoneScaleMethod");

			//애니메이션 전환시의 지정되지 않은 값의 처리
			AddKeyStrID(KEY_TYPES.UnspecifiedAnimControlParam, "UnspecifiedAnimControlParam");

			//텔레포트 보정
			AddKeyStrID(KEY_TYPES.TeleportCorrection, "TeleportCorrection");
			AddKeyStrID(KEY_TYPES.TeleportDist, "TeleportDist");
		}

		private void AddKeyStrID(KEY_TYPES key, string strID)
		{
			_key2StrID.Add(key, strID);
			_strID2Key.Add(strID, key);
		}


		// Save / Load
		//-----------------------------------------------------------------------------
		public bool Save()
		{
			FileStream fs = null;
			StreamWriter sw = null;

			string filePath = Application.dataPath + "/../AnyPortrait_ProjectSettings.txt";

			try
			{
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs);

				//내용을 저장하자
				//저장 여부
				SavePref_Bool(sw, KEY_TYPES.CommonSettingSaved, _isCommonSettingSaved);//공통 설정이 저장되었는가

				//프로젝트 설정
				SavePref_Bool(sw, KEY_TYPES.ColorSpaceGamma, _prj_IsColorSpaceGamma);
				SavePref_Bool(sw, KEY_TYPES.UseSRP, _prj_IsUseSRP);

				//캐릭터 공통 설정들
				//- Sorting Layer
				SavePref_Int(sw, KEY_TYPES.SortingLayerID, _cmn_SortingLayerID);
				SavePref_Int(sw, KEY_TYPES.SortingOrder, _cmn_SortingOrder);
				SavePref_Int(sw, KEY_TYPES.SortingLayerOption, (int)_cmn_SortingLayerOption);
				SavePref_Int(sw, KEY_TYPES.SortingOrderPerDepth, _cmn_SortingOrderPerDepth);

				//메카님 사용 여부
				SavePref_Bool(sw, KEY_TYPES.UsingMecanim, _cmn_IsUsingMecanim);

				//Important
				SavePref_Bool(sw, KEY_TYPES.IsImportant, _cmn_isImportant);
				SavePref_Int(sw, KEY_TYPES.FPS, _cmn_FPS);

				//빌보드
				SavePref_Int(sw, KEY_TYPES.BillboardType, (int)_cmn_BillboardOption);
				SavePref_Bool(sw, KEY_TYPES.ForceCamSortToOrtho, _cmn_IsForceCamSortModeToOrthographic);

				//Shadow
				SavePref_Int(sw, KEY_TYPES.CastShadows, (int)_cmn_CastShadows);
				SavePref_Bool(sw, KEY_TYPES.ReceiveShadows, _cmn_IsReceiveShadows);

				//VR 렌더 텍스쳐 사이즈
				SavePref_Int(sw, KEY_TYPES.VRSupported, (int)_cmn_VRSupported);
				SavePref_Int(sw, KEY_TYPES.VRRTSize, (int)_cmn_VRRtSize);

				//Flipped Mesh 검출법
				SavePref_Int(sw, KEY_TYPES.FlippedMeshCheck, (int)_cmn_FlippedMeshOption);

				//루트 본의 스케일 옵션
				SavePref_Int(sw, KEY_TYPES.RootBoneScaleMethod, (int)_cmn_RootBoneScaleMethod);

				//애니메이션 전환시의 지정되지 않은 값의 처리
				SavePref_Int(sw, KEY_TYPES.UnspecifiedAnimControlParam, (int)_cmn_UnspecifiedAnimControlParam);

				//텔레포트 보정
				SavePref_Bool(sw, KEY_TYPES.TeleportCorrection, _cmn_TeleportCorrection);
				SavePref_Float(sw, KEY_TYPES.TeleportDist, _cmn_TeleportDist);


				//------------------------------



				sw.Flush();
				sw.Close();
				fs.Close();

				sw = null;
				fs = null;

				return true;
			}
			catch (Exception)
			{
				if (sw != null)
				{
					sw.Close();
					sw = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				return false;
			}

		}



		private void AppendKey(KEY_TYPES key)
		{
			//Key > Str ID로 변환해서 넣고
			_strWrapper.Append(_key2StrID[key], false);

			//구분자를 넣자
			_strWrapper.Append(STR_DELIMETER, false);
		}

		//키/값을 저장하자.
		private void SavePref_Bool(StreamWriter sw, KEY_TYPES key, bool boolValue)
		{
			_strWrapper.Clear();

			AppendKey(key);
			_strWrapper.Append((boolValue ? "TRUE" : "FALSE"), true);

			sw.WriteLine(_strWrapper.ToString());
		}

		private void SavePref_Int(StreamWriter sw, KEY_TYPES key, int intValue)
		{
			_strWrapper.Clear();

			AppendKey(key);
			_strWrapper.Append(intValue, true);

			sw.WriteLine(_strWrapper.ToString());
		}

		private void SavePref_Float(StreamWriter sw, KEY_TYPES key, float floatValue)
		{
			_strWrapper.Clear();

			AppendKey(key);

			string strFloat = (floatValue.ToString()).Replace(',', '.');//소수점을 ,로 적는 문화권에선 .로 저장되도록 만든다.
			_strWrapper.Append(strFloat, true);

			sw.WriteLine(_strWrapper.ToString());
		}

		// Load
		public void Load()
		{
			FileStream fs = null;
			StreamReader sr = null;

			string filePath = Application.dataPath + "/../AnyPortrait_ProjectSettings.txt";

			try
			{
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs);

				while (true)
				{
					if (sr.Peek() < 0)
					{
						break;
					}

					//키 + : (구분자) + 값
					//나머지 : 값
					string strRead = sr.ReadLine();
					if (strRead.Length < 2)
					{
						continue;
					}

					if (!strRead.Contains(STR_DELIMETER))
					{
						continue;
					}

					int iDel = strRead.IndexOf(STR_DELIMETER);
					string strKey = strRead.Substring(0, iDel);

					KEY_TYPES curKey = KEY_TYPES.None;
					if (!_strID2Key.TryGetValue(strKey, out curKey))
					{
						Debug.LogError("Undefined Key [" + strKey + "]");
						continue;
					}

					string strValue = "";
					if (iDel + 1 < strRead.Length)
					{
						strValue = strRead.Substring(iDel + 1);
					}

					

					switch (curKey)
					{
						//저장 여부
						case KEY_TYPES.CommonSettingSaved: _isCommonSettingSaved = LoadPref_Bool(ref strValue); break;
						case KEY_TYPES.ColorSpaceGamma: _prj_IsColorSpaceGamma = LoadPref_Bool(ref strValue); break;
						case KEY_TYPES.UseSRP: _prj_IsUseSRP = LoadPref_Bool(ref strValue); break;

						//캐릭터 공통 설정들
						//- Sorting Layer
						case KEY_TYPES.SortingLayerID: _cmn_SortingLayerID = LoadPref_Int(ref strValue); break;
						case KEY_TYPES.SortingOrder: _cmn_SortingOrder = LoadPref_Int(ref strValue); break;
						case KEY_TYPES.SortingLayerOption: _cmn_SortingLayerOption = (apPortrait.SORTING_ORDER_OPTION)LoadPref_Int(ref strValue); break;
						case KEY_TYPES.SortingOrderPerDepth: _cmn_SortingOrderPerDepth = LoadPref_Int(ref strValue); break;

						//메카님 사용 여부
						case KEY_TYPES.UsingMecanim: _cmn_IsUsingMecanim = LoadPref_Bool(ref strValue); break;

						//Important
						case KEY_TYPES.IsImportant: _cmn_isImportant = LoadPref_Bool(ref strValue); break;
						case KEY_TYPES.FPS:			_cmn_FPS = LoadPref_Int(ref strValue); break;

						//빌보드
						case KEY_TYPES.BillboardType: _cmn_BillboardOption = (apPortrait.BILLBOARD_TYPE)LoadPref_Int(ref strValue); break;
						case KEY_TYPES.ForceCamSortToOrtho: _cmn_IsForceCamSortModeToOrthographic = LoadPref_Bool(ref strValue); break;

						//Shadow
						case KEY_TYPES.CastShadows: _cmn_CastShadows = (apPortrait.SHADOW_CASTING_MODE)LoadPref_Int(ref strValue); break;
						case KEY_TYPES.ReceiveShadows: _cmn_IsReceiveShadows = LoadPref_Bool(ref strValue); break;

						//VR 렌더 모드
						case KEY_TYPES.VRSupported: _cmn_VRSupported = (apPortrait.VR_SUPPORT_MODE)LoadPref_Int(ref strValue); break;
						case KEY_TYPES.VRRTSize: _cmn_VRRtSize = (apPortrait.VR_RT_SIZE)LoadPref_Int(ref strValue); break;

						//Flipped Mesh 검출법
						case KEY_TYPES.FlippedMeshCheck: _cmn_FlippedMeshOption = (apPortrait.FLIPPED_MESH_CHECK)LoadPref_Int(ref strValue); break;

						//루트 본의 스케일 옵션
						case KEY_TYPES.RootBoneScaleMethod: _cmn_RootBoneScaleMethod = (apPortrait.ROOT_BONE_SCALE_METHOD)LoadPref_Int(ref strValue); break;

						//애니메이션 전환시의 지정되지 않은 값의 처리
						case KEY_TYPES.UnspecifiedAnimControlParam: _cmn_UnspecifiedAnimControlParam = (apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM)LoadPref_Int(ref strValue); break;

						//텔레포트 보정
						case KEY_TYPES.TeleportCorrection: _cmn_TeleportCorrection = LoadPref_Bool(ref strValue); break;
						case KEY_TYPES.TeleportDist: _cmn_TeleportDist = LoadPref_Float(ref strValue); break;
					}
				}

				//----------------------------------------------------------------------

				_isLoaded = true;


				//로드가 완료되었다면 이전 버전에서의 레지키도 삭제한다.
				EditorPrefs.DeleteKey("AnyPortrait_IsBakeColorSpace_ToGamma");
				EditorPrefs.DeleteKey("AnyPortrait_IsUseLWRPShader");

				sr.Close();
				fs.Close();

				sr = null;
				fs = null;
			}
			catch (Exception ex)
			{
				if (sr != null)
				{
					sr.Close();
					sr = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				//파일이 없었다면 초기화 후 저장을 하자
				if (ex is FileNotFoundException)
				{
					Clear();
					Save();
				}
			}
		}

		private bool LoadPref_Bool(ref string strValue)
		{
			return strValue.StartsWith("TRUE");
		}

		private int LoadPref_Int(ref string strValue)
		{
			return int.Parse(strValue);
		}

		private float LoadPref_Float(ref string strValue)
		{
			return apUtil.ParseFloat(strValue);//소수점 파싱 (, ,)이슈때문에 별도의 함수 이용
		}





		// 값 할당 및 저장
		//------------------------------------------------------------------------
		public void SetColorSpaceGamma(bool isColorSpaceGamma)
		{
			if(_prj_IsColorSpaceGamma != isColorSpaceGamma)
			{
				_prj_IsColorSpaceGamma = isColorSpaceGamma;
				Save();
			}
		}
		public void SetUseSRP(bool isUseURP)
		{
			if(_prj_IsUseSRP != isUseURP)
			{
				_prj_IsUseSRP = isUseURP;
				Save();
			}
		}


		public enum SAVE_RESULT
		{
			FileSaved,
			NoChanged,
			Failed
		}
		public SAVE_RESULT SetPortraitCommonSettings(apPortrait portrait)
		{
			if (portrait == null)
			{
				return SAVE_RESULT.NoChanged;
			}

			bool isAnyChanged = false;//하나라도 변경이 되었다면 저장을 하자

			if (!_isCommonSettingSaved)
			{
				_isCommonSettingSaved = true;
				isAnyChanged = true;
			}

			//Sorting Option
			if (_cmn_SortingLayerID != portrait._sortingLayerID
				|| _cmn_SortingOrder != portrait._sortingOrder
				|| _cmn_SortingLayerOption != portrait._sortingOrderOption
				|| _cmn_SortingOrderPerDepth != portrait._sortingOrderPerDepth)
			{
				_cmn_SortingLayerID = portrait._sortingLayerID;
				_cmn_SortingOrder = portrait._sortingOrder;
				_cmn_SortingLayerOption = portrait._sortingOrderOption;
				_cmn_SortingOrderPerDepth = portrait._sortingOrderPerDepth;
				isAnyChanged = true;
			}

			//메카님
			if (_cmn_IsUsingMecanim != portrait._isUsingMecanim)
			{
				_cmn_IsUsingMecanim = portrait._isUsingMecanim;
				isAnyChanged = true;
			}

			//Important
			if(_cmn_isImportant != portrait._isImportant
				|| _cmn_FPS != portrait._FPS)
			{
				_cmn_isImportant = portrait._isImportant;
				_cmn_FPS = portrait._FPS;
				isAnyChanged = true;
			}

			//빌보드
			if (_cmn_BillboardOption != portrait._billboardType
				|| _cmn_IsForceCamSortModeToOrthographic != portrait._isForceCamSortModeToOrthographic)
			{
				_cmn_BillboardOption = portrait._billboardType;
				_cmn_IsForceCamSortModeToOrthographic = portrait._isForceCamSortModeToOrthographic;
				isAnyChanged = true;
			}

			//Shadow
			if (_cmn_CastShadows != portrait._meshShadowCastingMode
				|| _cmn_IsReceiveShadows != portrait._meshReceiveShadow)
			{
				_cmn_CastShadows = portrait._meshShadowCastingMode;
				_cmn_IsReceiveShadows = portrait._meshReceiveShadow;
				isAnyChanged = true;
			}

			// VR 렌더 텍스쳐 사이즈 (이건 개별이다)
			if (_cmn_VRSupported != portrait._vrSupportMode
				|| _cmn_VRRtSize != portrait._vrRenderTextureSize)
			{
				_cmn_VRSupported = portrait._vrSupportMode;
				_cmn_VRRtSize = portrait._vrRenderTextureSize;
				isAnyChanged = true;
			}

			// Flipped Mesh 검출법
			if (_cmn_FlippedMeshOption != portrait._flippedMeshOption)
			{
				_cmn_FlippedMeshOption = portrait._flippedMeshOption;
				isAnyChanged = true;
			}

			//루트 본의 스케일 옵션
			if (_cmn_RootBoneScaleMethod != portrait._rootBoneScaleMethod)
			{
				_cmn_RootBoneScaleMethod = portrait._rootBoneScaleMethod;
				isAnyChanged = true;
			}

			//애니메이션 전환시의 지정되지 않은 값의 처리
			if (_cmn_UnspecifiedAnimControlParam != portrait._unspecifiedAnimControlParamOption)
			{
				_cmn_UnspecifiedAnimControlParam = portrait._unspecifiedAnimControlParamOption;
				isAnyChanged = true;
			}

			//텔레포트 보정
			if (_cmn_TeleportCorrection != portrait._isTeleportCorrectionOption
				|| Mathf.Abs(_cmn_TeleportDist - portrait._teleportMovementDist) > 0.001f)
			{
				_cmn_TeleportCorrection = portrait._isTeleportCorrectionOption;
				_cmn_TeleportDist = portrait._teleportMovementDist;
				isAnyChanged = true;
			}

			//저장을 하자
			if (isAnyChanged)
			{
				bool isSuccess = Save();

				if(isSuccess)
				{
					return SAVE_RESULT.FileSaved;
				}
				else
				{
					return SAVE_RESULT.Failed;
				}
			}

			return SAVE_RESULT.NoChanged;
		}

		public void ClearCommonSettingsAndSave()
		{
			_isCommonSettingSaved = false;

			//기본값은 원래의 Portrait 멤버 변수의 초기값을 이용
			_cmn_SortingLayerID = 0;
			_cmn_SortingOrder = 0;
			_cmn_SortingLayerOption = apPortrait.SORTING_ORDER_OPTION.SetOrder;
			_cmn_SortingOrderPerDepth = 1;

			_cmn_IsUsingMecanim = false;

			_cmn_isImportant = true;
			_cmn_FPS = 30;

			_cmn_BillboardOption = apPortrait.BILLBOARD_TYPE.None;
			_cmn_IsForceCamSortModeToOrthographic = true;

			_cmn_CastShadows = apPortrait.SHADOW_CASTING_MODE.Off;
			_cmn_IsReceiveShadows = false;

			_cmn_VRSupported = apPortrait.VR_SUPPORT_MODE.None;
			_cmn_VRRtSize = apPortrait.VR_RT_SIZE.ByMeshSettings;

			_cmn_FlippedMeshOption = apPortrait.FLIPPED_MESH_CHECK.TransformOnly;

			_cmn_RootBoneScaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;

			_cmn_UnspecifiedAnimControlParam = apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM.RevertToDefaultValue;

			_cmn_TeleportCorrection = false;
			_cmn_TeleportDist = 10.0f;

			//저장
			Save();
		}


		/// <summary>
		/// 새로운 Portrait 생성시 저장된 공통 설정을 그대로 반영한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void AdaptCommonSettingsToPortrait(apPortrait portrait)
		{
			if (portrait == null || !_isCommonSettingSaved)
			{
				return;
			}

			//Sorting Option
			portrait._sortingLayerID = _cmn_SortingLayerID;
			portrait._sortingOrder = _cmn_SortingOrder;
			portrait._sortingOrderOption = _cmn_SortingLayerOption;
			portrait._sortingOrderPerDepth = _cmn_SortingOrderPerDepth;

			//메카님
			portrait._isUsingMecanim = _cmn_IsUsingMecanim;

			//Important
			portrait._isImportant = _cmn_isImportant;
			portrait._FPS = _cmn_FPS;

			//빌보드
			portrait._billboardType = _cmn_BillboardOption;
			portrait._isForceCamSortModeToOrthographic = _cmn_IsForceCamSortModeToOrthographic;

			//Shadow
			portrait._meshShadowCastingMode = _cmn_CastShadows;
			portrait._meshReceiveShadow = _cmn_IsReceiveShadows;

			// VR 렌더 텍스쳐 사이즈 (이건 개별이다)
			portrait._vrSupportMode = _cmn_VRSupported;
			portrait._vrRenderTextureSize = _cmn_VRRtSize;

			// Flipped Mesh 검출법
			portrait._flippedMeshOption = _cmn_FlippedMeshOption;

			//루트 본의 스케일 옵션
			portrait._rootBoneScaleMethod = _cmn_RootBoneScaleMethod;

			//애니메이션 전환시의 지정되지 않은 값의 처리
			portrait._unspecifiedAnimControlParamOption = _cmn_UnspecifiedAnimControlParam;

			//텔레포트 보정
			portrait._isTeleportCorrectionOption = _cmn_TeleportCorrection;
			portrait._teleportMovementDist = _cmn_TeleportDist;
		}

		/// <summary>
		/// Portrait의 Bake 설정들을 초기값으로 되돌린다. (저장되지 않는 일부 설정은 되돌리지 않는다.)
		/// </summary>
		/// <param name="portrait"></param>
		public void ResetPortraitBakeSettings(apPortrait portrait)
		{
			if(portrait == null)
			{
				return;
			}


			//기본값은 원래의 Portrait 멤버 변수의 초기값을 이용
			portrait._sortingLayerID = 0;
			portrait._sortingOrder = 0;
			portrait._sortingOrderOption = apPortrait.SORTING_ORDER_OPTION.SetOrder;
			portrait._sortingOrderPerDepth = 1;

			portrait._isUsingMecanim = false;

			portrait._isImportant = true;
			portrait._FPS = 30;

			portrait._billboardType = apPortrait.BILLBOARD_TYPE.None;
			portrait._isForceCamSortModeToOrthographic = true;

			portrait._meshShadowCastingMode = apPortrait.SHADOW_CASTING_MODE.Off;
			portrait._meshReceiveShadow = false;

			portrait._vrSupportMode = apPortrait.VR_SUPPORT_MODE.None;
			portrait._vrRenderTextureSize = apPortrait.VR_RT_SIZE.ByMeshSettings;

			portrait._flippedMeshOption = apPortrait.FLIPPED_MESH_CHECK.TransformOnly;

			portrait._rootBoneScaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;

			portrait._unspecifiedAnimControlParamOption = apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM.RevertToDefaultValue;

			portrait._isTeleportCorrectionOption = false;
			portrait._teleportMovementDist = 10.0f;
		}


		// 저장된 값이 있는지 확인
		//------------------------------------------------------------------------		
		public bool IsCommonSettingSaved { get { return _isCommonSettingSaved; } }



		// 값 가져오기
		//------------------------------------------------------------------------
		// 프로젝트 설정
		public bool Project_IsColorSpaceGamma { get { return _prj_IsColorSpaceGamma; } }
		public bool Project_IsUseSRP { get { return _prj_IsUseSRP; } }

		//Sorting Layer
		public int Common_SortingLayerID { get { return _cmn_SortingLayerID; } }
		public int Common_SortingOrder { get { return _cmn_SortingOrder; } }
		public apPortrait.SORTING_ORDER_OPTION Common_SortingLayerOption { get { return _cmn_SortingLayerOption; } }
		public int Common_SortingOrderPerDepth { get { return _cmn_SortingOrderPerDepth; } }

		//메카님 사용 여부 (경로는 저장하지 않는다.)
		public bool Common_IsUsingMecanim { get { return _cmn_IsUsingMecanim; } }

		//Important
		public bool Common_IsImportant { get { return _cmn_isImportant; } }
		public int Common_FPS { get { return _cmn_FPS; } }

		//빌보드
		public apPortrait.BILLBOARD_TYPE Common_BillboardOption { get { return _cmn_BillboardOption; } }
		public bool Common_IsForceCamSortModeToOrthographic { get { return _cmn_IsForceCamSortModeToOrthographic; } }

		//Shadow
		public apPortrait.SHADOW_CASTING_MODE Common_CastShadows { get { return _cmn_CastShadows; } }
		public bool Common_IsReceiveShadows { get { return _cmn_IsReceiveShadows; } }

		// VR 렌더 텍스쳐
		public apPortrait.VR_SUPPORT_MODE Common_VRSupported { get { return _cmn_VRSupported; } }
		public apPortrait.VR_RT_SIZE Common_VRRenterTextureSize { get { return _cmn_VRRtSize; } }

		// Flipped Mesh 검출법
		public apPortrait.FLIPPED_MESH_CHECK Common_FlippedMeshOption { get { return _cmn_FlippedMeshOption; } }

		//루트 본의 스케일 옵션
		public apPortrait.ROOT_BONE_SCALE_METHOD Common_RootBoneScaleMethod { get { return _cmn_RootBoneScaleMethod; } }

		//애니메이션 전환시의 지정되지 않은 값의 처리
		public apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM Common_UnspecifiedAnimControlParam { get { return _cmn_UnspecifiedAnimControlParam; } }

		//텔레포트 보정
		public bool Common_TeleportCorrection { get { return _cmn_TeleportCorrection; } }
		public float Common_TeleportDist { get { return _cmn_TeleportDist; } }

	}
}