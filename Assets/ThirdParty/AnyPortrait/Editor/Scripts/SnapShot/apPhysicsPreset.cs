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

namespace AnyPortrait
{
	/// <summary>
	/// 물리 값을 저장하는 방식
	/// Snap Shot이 아니라 Preset 방식으로 저장한다.
	/// PresetUnit이 실제 데이터이고, 이 곳에서는 검색/저장/추가/삭제 등의 API를 제공한다.
	/// Unit의 ID로 참조하며, Editor에 포함된다.
	/// 저장은 Serial이 아닌 파일로 저장된다.
	/// 일부 정보는 미리 저장되는데 이때 ID는 0~99의 값을 가지고
	/// 사용자가 저장하는 타입은 100이상의 값을 가진다.
	/// </summary>
	public class apPhysicsPreset
	{
		// Member
		//-------------------------------------------

		private List<apPhysicsPresetUnit> _units = new List<apPhysicsPresetUnit>();
		public List<apPhysicsPresetUnit> Presets { get { return _units; } }

		// Init
		//-------------------------------------------
		public apPhysicsPreset()
		{
			
		}

		public void Clear()
		{
			_units.Clear();
		}



		// Save/Load
		//-------------------------------------------
		public void Save()
		{
			FileStream fs = null;
			StreamWriter sw = null;

			//string defaultPath = Application.dataPath;
			//string filePath = defaultPath.Substring(0, defaultPath.Length - 6) + "/AnyPortrait_PhysicsParam.txt";
			string filePath = Application.dataPath + "/../AnyPortrait_PhysicsParam.txt";
			try
			{
				MakeReservedPresets();//Reserved가 추가되지 않았으면 자동으로 미리 추가하자

				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				for (int i = 0; i < _units.Count; i++)
				{
					_units[i].Save(sw);

					if (i < _units.Count - 1)
					{
						sw.WriteLine("--");//구분자
					}
				}

				sw.Flush();

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
			}
			catch (Exception ex)
			{
				Debug.LogError("PhysicsPreset Save Exception : " + ex);

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
			}


		}


		public void Load()
		{
			FileStream fs = null;
			StreamReader sr = null;


			//string defaultPath = Application.dataPath;
			//string filePath = defaultPath.Substring(0, defaultPath.Length - 6) + "/AnyPortrait_PhysicsParam.txt";
			string filePath = Application.dataPath + "/../AnyPortrait_PhysicsParam.txt";

			try
			{
				MakeReservedPresets();//Reserved가 추가되지 않았으면 자동으로 미리 추가하자

				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제와 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				List<string> strData = new List<string>();
				//유효한 데이터를 긁어온 후,
				//구분자를 만나면>>
				//하나씩 Unit으로 만들어준다.
				//일단 새로 하나 생성 후, 로드한 뒤,
				//겹치는게 있으면... 패스 (Save를 먼저 하세요)

				while (true)
				{
					if (sr.Peek() < 0)
					{
						//남은게 있으면 이것도 처리
						if (strData.Count > 0)
						{
							apPhysicsPresetUnit newUnit = new apPhysicsPresetUnit();
							newUnit.Load(strData);

							if (newUnit._uniqueID < 0)
							{
								continue;
							}

							//이제 추가 가능한 데이터인지 확인하자
							if (GetPresetUnit(newUnit._uniqueID) == null)
							{
								_units.Add(newUnit);//추가!
							}
							strData.Clear();
						}
						break;
					}
					string strRead = sr.ReadLine();
					if (strRead.Length < 3)
					{
						//구분자를 만난 듯 하다.
						apPhysicsPresetUnit newUnit = new apPhysicsPresetUnit();
						newUnit.Load(strData);

						if (newUnit._uniqueID < 0)
						{
							continue;
						}

						//이제 추가 가능한 데이터인지 확인하자
						if (GetPresetUnit(newUnit._uniqueID) == null)
						{
							_units.Add(newUnit);//추가!
						}

						strData.Clear();
					}
					else
					{
						//데이터 누적
						strData.Add(strRead);
					}
				}



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
			}
			catch (Exception ex)
			{
				if (ex is FileNotFoundException)
				{

				}
				else
				{
					Debug.LogError("PhysicsPreset Load Exception : " + ex);
				}


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

				//일단 저장을 한번 더 하자 (파일이 없을 수 있음)
				Save();
			}
		}

		// Functions
		//-------------------------------------------
		public apPhysicsPresetUnit GetPresetUnit(int uniqueID)
		{
			return _units.Find(delegate (apPhysicsPresetUnit a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		private int GetNewCustomID()
		{
			//사용자 ID는 100부터 시작
			//최대 999까지
			for (int iCurID = 100; iCurID <= 999; iCurID++)
			{
				if (GetPresetUnit(iCurID) == null)
				{
					//엥 없네영. 사용 가능
					return iCurID;
				}
			}
			return -1;//ID 얻기 실패
		}

		public bool AddNewPreset(apPhysicsMeshParam srcMeshParam, string name, apPhysicsPresetUnit.ICON icon)
		{
			int newID = GetNewCustomID();
			if (newID < 0)
			{
				return false;
			}

			if (string.IsNullOrEmpty(name))
			{
				name = "<No Name>";
			}
			apPhysicsPresetUnit newUnit = new apPhysicsPresetUnit(newID, name, icon);
			newUnit.SetPhysicsMeshParam(srcMeshParam);

			_units.Add(newUnit);

			Save();
			return true;
		}

		public void RemovePreset(int targetUniqueID)
		{
			apPhysicsPresetUnit targetUnit = GetPresetUnit(targetUniqueID);
			if (targetUnit == null)
			{
				return;
			}
			if (targetUnit._isReserved)
			{
				return;
			}

			_units.Remove(targetUnit);
			Save();

		}

		//---------------------------------------------------------------------------------------
		// 프리셋을 여기서 만들어주자
		//---------------------------------------------------------------------------------------
		private void MakeReservedPresets()
		{
			//예약된 프리들을 만들자
			//이미 등록된게 있으면 걍 갱신
			//MakeReservedPresetUnit <<이걸로 호출


			//기본값에서는 중력만 적용한다.
			//Cloth1
			MakeReservedPresetUnit(1, "Cloth 1", apPhysicsPresetUnit.ICON.Cloth1,
				false,                      //Is Restrict Move Range
				0,                          //Move Range
				true,                       //Is Restrict Stretch Range
				0.1f, 300,                  //stretchRange(Max), stretchK
				0.5f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				10, 0.5f, 1);               //airDrag, viscosity, restoring

			//Cloth2
			MakeReservedPresetUnit(2, "Cloth 2", apPhysicsPresetUnit.ICON.Cloth2,
				false,                      //Is Restrict Move Range
				0,                          //Move Range
				true,                       //Is Restrict Stretch Range
				0.2f, 50,                   //stretchRange(Max), stretchK
				0.8f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				2, 0.3f, 5);                //airDrag, viscosity, restoring

			//Cloth3
			MakeReservedPresetUnit(3, "Cloth 3", apPhysicsPresetUnit.ICON.Cloth3,
				false,                      //Is Restrict Move Range
				0,                          //Move Range
				true,                       //Is Restrict Stretch Range
				1.0f, 100,                  //stretchRange(Max), stretchK
				1.0f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				2, 0.3f, 10);               //airDrag, viscosity, restoring

			//Flag
			MakeReservedPresetUnit(4, "Flag", apPhysicsPresetUnit.ICON.Flag,
				false,                      //Is Restrict Move Range
				0,                          //Move Range
				true,                       //Is Restrict Stretch Range
				1.0f, 100,                  //stretchRange(Max), stretchK
				1.0f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				2, 0.3f, 0);                //airDrag, viscosity, restoring

			//Hair
			MakeReservedPresetUnit(5, "Hair", apPhysicsPresetUnit.ICON.Hair,
				true,                       //Is Restrict Move Range
				150,                        //Move Range
				true,                       //Is Restrict Stretch Range
				0.5f, 150,                  //stretchRange(Max), stretchK
				0.8f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				2, 0.3f, 5.0f);				//airDrag, viscosity, restoring

			//Ribbon
			MakeReservedPresetUnit(6, "Ribbon", apPhysicsPresetUnit.ICON.Ribbon,
				false,                      //Is Restrict Move Range
				0,                          //Move Range
				true,                       //Is Restrict Stretch Range
				2.0f, 150,                  //stretchRange(Max), stretchK
				0.8f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				5.0f, 0.3f, 1.0f);              //airDrag, viscosity, restoring

			//Rubber Soft
			MakeReservedPresetUnit(7, "Rubber Soft", apPhysicsPresetUnit.ICON.RubberSoft,
				true,                       //Is Restrict Move Range
				100,                         //Move Range
				false,                      //Is Restrict Stretch Range
				0.0f, 150,                  //stretchRange(Max), stretchK
				1.0f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				5, 0.4f, 50.0f);         //airDrag, viscosity, restoring

			//Rubber Hard
			MakeReservedPresetUnit(8, "Rubber Hard", apPhysicsPresetUnit.ICON.RubberHard,
				true,                       //Is Restrict Move Range
				100,                         //Move Range
				false,                      //Is Restrict Stretch Range
				0.0f, 150,                  //stretchRange(Max), stretchK
				1.0f, 1,                    //inertiaK, damping
				1,                          //mass
				new Vector2(0.0f, -100.0f), //gravity
				new Vector2(0.0f, 0.0f),    //wind
				new Vector2(0.0f, 0.0f),    //windRandom
				5, 0.6f, 150.0f);        //airDrag, viscosity, restoring
		}
		//---------------------------------------------------------------------------------------

		private void MakeReservedPresetUnit(int uniqueID,
												string name, apPhysicsPresetUnit.ICON icon,
												bool isRestrictMoveRange,
												float moveRange,
												bool isRestrictStretchRange,
												//float stretchRange_Min,
												float stretchRange_Max,
												float stretchK,


												float inertiaK,
												float damping,
												float mass,
												Vector2 gravityConstValue,
												Vector2 windConstValue,
												Vector2 windRandomRange,

												float airDrag,
												float viscosity,
												float restoring
												)
		{
			apPhysicsPresetUnit unit = GetPresetUnit(uniqueID);
			if (unit == null)
			{
				unit = new apPhysicsPresetUnit(uniqueID, name, icon);
				_units.Add(unit);
			}

			unit._moveRange = moveRange;
			unit._isRestrictMoveRange = isRestrictMoveRange;
			unit._isRestrictStretchRange = isRestrictStretchRange;
			//unit._stretchRange_Min = stretchRange_Min;
			unit._stretchRange_Max = stretchRange_Max;
			unit._stretchK = stretchK;
			unit._inertiaK = inertiaK;
			unit._damping = damping;
			unit._mass = mass;

			unit._gravityParamType = apPhysicsMeshParam.ExternalParamType.Constant;
			unit._gravityConstValue = gravityConstValue;
			unit._windParamType = apPhysicsMeshParam.ExternalParamType.Constant;
			unit._windConstValue = windConstValue;
			unit._windRandomRange = windRandomRange;

			unit._airDrag = airDrag;
			unit._viscosity = viscosity;
			unit._restoring = restoring;

			unit._isReserved = true;
		}
	}


	public class apPhysicsPresetUnit
	{
		// Member
		//-------------------------------------------
		//등록된 설정
		public int _uniqueID = -1;
		public string _name = "";
		public enum ICON
		{
			Cloth1 = 0,
			Cloth2 = 1,
			Cloth3 = 2,
			Flag = 3,
			Hair = 4,
			Ribbon = 5,
			RubberHard = 6,
			RubberSoft = 7,
			Custom1 = 8,
			Custom2 = 9,
			Custom3 = 10,
		}
		public ICON _icon = ICON.Cloth1;

		//저장되는 값들
		public bool _isRestrictMoveRange = false;
		public float _moveRange = 0.0f;

		public bool _isRestrictStretchRange = false;
		//public float _stretchRange_Min = 0.0f;
		public float _stretchRange_Max = 0.0f;
		public float _stretchK = 0.0f;
		public float _inertiaK = 0.0f;
		public float _damping = 0.0f;
		public float _mass = 100.0f;

		//Gravity와 Wind는 Constant로 저장된다.
		//ControlParam인 경우는 ID를 제외하고 저장 (ID는 저장하지 않는다. 링크는 직접 해야함)
		public apPhysicsMeshParam.ExternalParamType _gravityParamType = apPhysicsMeshParam.ExternalParamType.Constant;
		public Vector2 _gravityConstValue = Vector2.zero;
		public apPhysicsMeshParam.ExternalParamType _windParamType = apPhysicsMeshParam.ExternalParamType.Constant;
		public Vector2 _windConstValue = Vector2.zero;
		public Vector2 _windRandomRange = Vector2.zero;

		public float _airDrag = 0.0f;
		public float _viscosity = 0.0f;
		public float _restoring = 1.0f;

		public bool _isReserved = false;


		// Init
		//-------------------------------------------
		public apPhysicsPresetUnit()
		{
			_isReserved = false;
		}
		public apPhysicsPresetUnit(int uniqueID, string name, ICON icon)
		{
			_uniqueID = uniqueID;
			_name = name;
			_icon = icon;
			_isReserved = false;
		}

		public void SetPhysicsMeshParam(apPhysicsMeshParam srcMeshParam)
		{
			_isRestrictStretchRange = srcMeshParam._isRestrictStretchRange;
			_isRestrictMoveRange = srcMeshParam._isRestrictMoveRange;

			_moveRange = srcMeshParam._moveRange;
			//_stretchRange_Min = srcMeshParam._stretchRangeRatio_Min;
			_stretchRange_Max = srcMeshParam._stretchRangeRatio_Max;
			_stretchK = srcMeshParam._stretchK;
			_inertiaK = srcMeshParam._inertiaK;
			_damping = srcMeshParam._damping;
			_mass = srcMeshParam._mass;

			//Gravity와 Wind는 Constant로 저장된다.
			//ControlParam인 경우는 ID를 제외하고 저장 (ID는 저장하지 않는다. 링크는 직접 해야함)
			_gravityParamType = srcMeshParam._gravityParamType;
			_gravityConstValue = srcMeshParam._gravityConstValue;
			_windParamType = srcMeshParam._windParamType;
			_windConstValue = srcMeshParam._windConstValue;
			_windRandomRange = srcMeshParam._windRandomRange;

			_airDrag = srcMeshParam._airDrag;
			_viscosity = srcMeshParam._viscosity;
			_restoring = srcMeshParam._restoring;
		}

		// Functions
		//-------------------------------------------
		/// <summary>
		/// 해당 Preset을 "그대로 사용 중"인지 "일부 변경했는지" 체크
		/// </summary>
		/// <param name="srcMeshParam"></param>
		/// <returns></returns>
		public bool IsSameProperties(apPhysicsMeshParam srcMeshParam)
		{
			if (srcMeshParam == null)
			{
				return false;
			}
			//TODO
			if (
				//!IsSameFloat(_stretchRange_Min, srcMeshParam._stretchRangeRatio_Min)
				!IsSameFloat(_stretchRange_Max, srcMeshParam._stretchRangeRatio_Max)
				|| !IsSameFloat(_moveRange, srcMeshParam._moveRange)
				|| !IsSameFloat(_stretchK, srcMeshParam._stretchK)
				|| !IsSameFloat(_inertiaK, srcMeshParam._inertiaK)
				|| !IsSameFloat(_damping, srcMeshParam._damping)
				|| !IsSameFloat(_mass, srcMeshParam._mass)
				|| (_gravityParamType != srcMeshParam._gravityParamType)
				|| !IsSameVector2(_gravityConstValue, srcMeshParam._gravityConstValue)
				|| (_windParamType != srcMeshParam._windParamType)
				|| !IsSameVector2(_windConstValue, srcMeshParam._windConstValue)
				|| !IsSameVector2(_windRandomRange, srcMeshParam._windRandomRange)
				|| !IsSameFloat(_airDrag, srcMeshParam._airDrag)
				|| !IsSameFloat(_viscosity, srcMeshParam._viscosity)
				|| !IsSameFloat(_restoring, srcMeshParam._restoring)
				|| _isRestrictMoveRange != srcMeshParam._isRestrictMoveRange
				|| _isRestrictStretchRange != srcMeshParam._isRestrictStretchRange
				)
			{
				//하나라도 다르면 false
				return false;
			}
			return true;
		}

		private bool IsSameFloat(float fVal1, float fVal2)
		{
			return Mathf.Abs(fVal1 - fVal2) < 0.00001f;
		}

		private bool IsSameVector2(Vector2 vec1, Vector2 vec2)
		{
			return Mathf.Abs(vec1.x - vec2.x) < 0.00001f
				&& Mathf.Abs(vec1.y - vec2.y) < 0.00001f;
		}


		public void Save(StreamWriter sw)
		{
			try
			{
				//앞 3글자가 키
				sw.WriteLine("UID" + _uniqueID);
				sw.WriteLine("NAM" + _name);
				sw.WriteLine("ICN" + (int)_icon);

				sw.WriteLine("IMR" + _isRestrictMoveRange);
				sw.WriteLine("ISR" + _isRestrictStretchRange);
				sw.WriteLine("MRG" + _moveRange);
				//sw.WriteLine("SMN" + _stretchRange_Min);
				sw.WriteLine("SMX" + _stretchRange_Max);
				sw.WriteLine("STK" + _stretchK);
				sw.WriteLine("INK" + _inertiaK);
				sw.WriteLine("DMP" + _damping);
				sw.WriteLine("MSS" + _mass);

				sw.WriteLine("GPT" + (int)_gravityParamType);
				sw.WriteLine("GVX" + _gravityConstValue.x);
				sw.WriteLine("GVY" + _gravityConstValue.y);

				sw.WriteLine("WPT" + (int)_windParamType);
				sw.WriteLine("WVX" + _windConstValue.x);
				sw.WriteLine("WVY" + _windConstValue.y);
				sw.WriteLine("WRX" + _windRandomRange.x);
				sw.WriteLine("WRY" + _windRandomRange.y);

				sw.WriteLine("ADG" + _airDrag);
				sw.WriteLine("VCS" + _viscosity);
				sw.WriteLine("RST" + _restoring);


			}
			catch (Exception ex)
			{
				Debug.LogError("PhysicsPreset Write Exception : " + ex);
			}
		}

		public void Load(List<string> loadedStringSet)
		{
			_uniqueID = -1;//<<이게 안바뀌면 실패다

			string strKey = "", strValue = "";
			string strCur = "";
			for (int i = 0; i < loadedStringSet.Count; i++)
			{
				strCur = loadedStringSet[i];
				if (strCur.Length < 3)
				{ continue; }

				strKey = strCur.Substring(0, 3);

				if (strCur.Length > 3)
				{
					strValue = strCur.Substring(3);
				}
				else
				{
					strValue = "";
				}

				try
				{

					if (strKey == "UID")
					{
						//sw.WriteLine("UID" + _uniqueID);
						_uniqueID = int.Parse(strValue);
					}
					else if (strKey == "NAM")
					{
						//sw.WriteLine("NAM" + _name);
						_name = strValue;
					}
					else if (strKey == "ICN")
					{
						//sw.WriteLine("ICN" + _icon);
						_icon = (ICON)int.Parse(strValue);
					}
					else if (strKey == "IMR")
					{
						_isRestrictMoveRange = bool.Parse(strValue);
					}
					else if (strKey == "ISR")
					{
						_isRestrictStretchRange = bool.Parse(strValue);
					}
					//else if (strKey == "SMN")
					//{
					//	//sw.WriteLine("STR" + _stretchRange);
					//	_stretchRange_Min = float.Parse(strValue);
					//}
					else if (strKey == "SMX")
					{
						//sw.WriteLine("STR" + _stretchRange);
						_stretchRange_Max = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "MRG")
					{
						//sw.WriteLine("STR" + _stretchRange);
						_moveRange = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "STK")
					{
						//sw.WriteLine("STK" + _stretchK);
						_stretchK = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "INK")
					{
						//sw.WriteLine("INK" + _inertiaK);
						_inertiaK = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "DMP")
					{
						//sw.WriteLine("DMP" + _damping);
						_damping = apUtil.ParseFloat(strValue);

					}
					else if (strKey == "MSS")
					{
						//sw.WriteLine("MSS" + _mass);
						_mass = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "GPT")
					{
						//sw.WriteLine("GPT" + (int)_gravityParamType);
						_gravityParamType = (apPhysicsMeshParam.ExternalParamType)(int.Parse(strValue));
					}
					else if (strKey == "GVX")
					{
						//sw.WriteLine("GVX" + _gravityConstValue.x);
						_gravityConstValue.x = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "GVY")
					{
						//sw.WriteLine("GVY" + _gravityConstValue.y);
						_gravityConstValue.y = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "WPT")
					{
						//sw.WriteLine("WPT" + (int)_windParamType);
						_windParamType = (apPhysicsMeshParam.ExternalParamType)(int.Parse(strValue));
					}
					else if (strKey == "WVX")
					{
						//sw.WriteLine("WVX" + _windConstValue.x);
						_windConstValue.x = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "WVY")
					{
						//sw.WriteLine("WVY" + _windConstValue.y);
						_windConstValue.y = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "WRX")
					{
						//sw.WriteLine("WRX" + _windRandomRange.x);
						_windRandomRange.x = apUtil.ParseFloat(strValue);

					}
					else if (strKey == "WRY")
					{
						//sw.WriteLine("WRY" + _windRandomRange.y);
						_windRandomRange.y = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "ADG")
					{
						//sw.WriteLine("ADG" + _airDrag);
						_airDrag = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "VCS")
					{
						//sw.WriteLine("VCS" + _viscosity);
						_viscosity = apUtil.ParseFloat(strValue);
					}
					else if (strKey == "RST")
					{
						//sw.WriteLine("RST" + _restoring);
						_restoring = apUtil.ParseFloat(strValue);
					}
					else
					{
						Debug.LogError("Unknown PhysicPreset Load Keyword [" + strKey + "]");
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("PhysicsPreset Load Exception : " + ex);
				}







			}
		}
	}

}