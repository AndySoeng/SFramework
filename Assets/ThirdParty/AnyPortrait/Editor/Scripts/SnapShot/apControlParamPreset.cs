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
	public class apControlParamPreset
	{
		// Members
		//-----------------------------------------------------
		private List<apControlParamPresetUnit> _units = new List<apControlParamPresetUnit>();
		public List<apControlParamPresetUnit> Presets {  get { return _units; } }

		// Init
		//-----------------------------------------------------
		public apControlParamPreset()
		{
			Clear();
		}

		public void Clear()
		{
			_units.Clear();
		}


		// Save / Load
		//-----------------------------------------------------
		public void Save()
		{
			FileStream fs = null;
			StreamWriter sw = null;

			string filePath = Application.dataPath + "/../AnyPortrait_ControlParam.txt";

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
			catch(Exception ex)
			{
				Debug.LogError("ControlParamPreset Save Exception : " + ex);

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


			string filePath = Application.dataPath + "/../AnyPortrait_ControlParam.txt";

			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로, 인코딩 문제
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
							apControlParamPresetUnit newUnit = new apControlParamPresetUnit();
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
						apControlParamPresetUnit newUnit = new apControlParamPresetUnit();
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
		//-----------------------------------------------------

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

		// Add New Preset / Remove Preset
		//-----------------------------------------------------------------------------
		public bool AddNewPreset(apControlParam srcControlParam)
		{
			int newID = GetNewCustomID();
			if(newID < 0)
			{
				return false;
			}

			apControlParamPresetUnit newUnit = new apControlParamPresetUnit();
			newUnit.SetControlParam(srcControlParam, newID);

			_units.Add(newUnit);

			Save();//<<바로바로 저장
			return true;
		}

		public void RemovePreset(int targetUniqueID)
		{
			apControlParamPresetUnit targetUnit = GetPresetUnit(targetUniqueID);
			if(targetUnit == null)
			{
				return;
			}
			if(targetUnit._isReserved)
			{
				return;
			}

			_units.Remove(targetUnit);
			Save();
		}


		// 프리셋 만들어주는 함수들
		//----------------------------------------------------------------------------------------
		private void MakeReservedPresets()
		{
			//1. 머리 각도
			//2. 눈 각도
			//3. 눈 깜빡임
			//4. 입 벌리기
			//5. 몸 각도
			//6. 숨쉬기

			//공통적인 기본 Param만 저장

			//1. 머리 각도
			MakeReservedPresetUnit(	1, "Head Direction",
									apControlParam.CATEGORY.Head, apControlParam.ICON_PRESET.Head, apControlParam.TYPE.Vector2,
									0, 0.0f, Vector2.zero,
									-1, 1, -1.0f, 1.0f, new Vector2(-1.0f, -1.0f), new Vector2(1.0f, 1.0f),
									"X", "Y", 4);

			//2. 눈 각도
			MakeReservedPresetUnit(	2, "Eye Direction",
									apControlParam.CATEGORY.Face, apControlParam.ICON_PRESET.Eye, apControlParam.TYPE.Vector2,
									0, 0.0f, Vector2.zero,
									-1, 1, -1.0f, 1.0f, new Vector2(-1.0f, -1.0f), new Vector2(1.0f, 1.0f),
									"X", "Y", 4);

			//3. 눈 깜빡임
			MakeReservedPresetUnit(3, "Eye Blink",
									apControlParam.CATEGORY.Face, apControlParam.ICON_PRESET.Eye, apControlParam.TYPE.Float,
									0, 1.0f, Vector2.zero,
									-1, 1, 0.0f, 1.0f, new Vector2(-1.0f, -1.0f), new Vector2(1.0f, 1.0f),
									"Close", "Open", 4);

			//4. 입 벌리기
			MakeReservedPresetUnit(	4, "Mouth Open",
									apControlParam.CATEGORY.Face, apControlParam.ICON_PRESET.Face, apControlParam.TYPE.Float,
									0, 0.0f, Vector2.zero,
									-1, 1, 0.0f, 1.0f, new Vector2(-1.0f, -1.0f), new Vector2(1.0f, 1.0f),
									"Close", "Open", 4);

			//5. 몸 각도
			MakeReservedPresetUnit( 5, "Body Direction",
									apControlParam.CATEGORY.Body, apControlParam.ICON_PRESET.Body, apControlParam.TYPE.Vector2,
									0, 0.0f, Vector2.zero,
									-1, 1, -1.0f, 1.0f, new Vector2(-1.0f, -1.0f), new Vector2(1.0f, 1.0f),
									"X", "Y", 4);

			//6. 숨쉬기
			MakeReservedPresetUnit(6, "Breath",
									apControlParam.CATEGORY.Body, apControlParam.ICON_PRESET.Body, apControlParam.TYPE.Float,
									0, 0.0f, Vector2.zero,
									-1, 1, 0.0f, 1.0f, new Vector2(-1.0f, 1.0f), new Vector2(-1.0f, 1.0f),
									"Default", "Breathe", 4);
		}


		private void MakeReservedPresetUnit(int uniqueID,
												string keyName,
												apControlParam.CATEGORY category,
												apControlParam.ICON_PRESET iconPreset,
												apControlParam.TYPE valueType,
												int int_Def, float float_Def, Vector2 vec2_Def,
												int int_Min, int int_Max,
												float float_Min, float float_Max,
												Vector2 vec2_Min, Vector2 vec2_Max,
												string label_Min, string label_Max,
												int snapSize)
		{
			apControlParamPresetUnit unit = GetPresetUnit(uniqueID);
			if(unit == null)
			{
				unit = new apControlParamPresetUnit();
				_units.Add(unit);
			}

			unit.SetReservedControlParam(uniqueID, keyName,
										category, iconPreset, valueType,
										int_Def, float_Def, vec2_Def,
										int_Min, int_Max,
										float_Min, float_Max,
										vec2_Min, vec2_Max,
										label_Min, label_Max,
										snapSize);

		}

		// Get / Set
		//-----------------------------------------------------
		public apControlParamPresetUnit GetPresetUnit(int uniqueID)
		{
			return _units.Find(delegate (apControlParamPresetUnit a)
			{
				return a._uniqueID == uniqueID;
			});
		}



	}






	public class apControlParamPresetUnit
	{
		// Members
		//-----------------------------------------------------------
		public int _uniqueID = -1;
		public string _keyName = "";

		public apControlParam.CATEGORY _category = apControlParam.CATEGORY.Etc;
		public apControlParam.ICON_PRESET _iconPreset = apControlParam.ICON_PRESET.None;
		public apControlParam.TYPE _valueType = apControlParam.TYPE.Int;

		//값은 Min-Max-Default로 나뉜다.
		public int _int_Def = 0;
		public float _float_Def = 0.0f;
		public Vector2 _vec2_Def = Vector2.zero;

		public int _int_Min = 0;
		public int _int_Max = 0;

		public float _float_Min = 0;
		public float _float_Max = 0;

		public Vector2 _vec2_Min = Vector2.zero;
		public Vector2 _vec2_Max = Vector2.zero;

		public string _label_Min = "";
		public string _label_Max = "";

		
		public int _snapSize = 4;//<기본값은 4이다.

		public bool _isReserved = false;

		// Init
		//-----------------------------------------------------------
		public apControlParamPresetUnit()
		{
			_isReserved = false;
		}

		public void SetControlParam(apControlParam srcControlParam, int uniqueID)
		{
			_uniqueID = uniqueID;
			_keyName = srcControlParam._keyName;
			

			_category = srcControlParam._category;
			_iconPreset = srcControlParam._iconPreset;
			_valueType = srcControlParam._valueType;

			//값은 Min-Max-Default로 나뉜다.
			_int_Def = srcControlParam._int_Def;
			_float_Def = srcControlParam._float_Def;
			_vec2_Def = srcControlParam._vec2_Def;

			_int_Min = srcControlParam._int_Min;
			_int_Max = srcControlParam._int_Max;

			_float_Min = srcControlParam._float_Min;
			_float_Max = srcControlParam._float_Max;

			_vec2_Min = srcControlParam._vec2_Min;
			_vec2_Max = srcControlParam._vec2_Max;

			_label_Min = srcControlParam._label_Min;
			_label_Max = srcControlParam._label_Max;


			_snapSize = srcControlParam._snapSize;

			_isReserved = false;//<<Reserved는 아니다.
		}

		public void SetReservedControlParam(	int uniqueID,
												string keyName,
												apControlParam.CATEGORY category,
												apControlParam.ICON_PRESET iconPreset,
												apControlParam.TYPE valueType,
												int int_Def, float float_Def, Vector2 vec2_Def,
												int int_Min, int int_Max,
												float float_Min, float float_Max,
												Vector2 vec2_Min, Vector2 vec2_Max,
												string label_Min, string label_Max,
												int snapSize)
		{
			_uniqueID = uniqueID;
			_keyName = keyName;
			_category = category;
			_iconPreset = iconPreset;
			_valueType = valueType;

			_int_Def = int_Def;
			_float_Def = float_Def;
			_vec2_Def = vec2_Def;

			_int_Min = int_Min;
			_int_Max = int_Max;
			_float_Min = float_Min;
			_float_Max = float_Max;
			_vec2_Min = vec2_Min;
			_vec2_Max = vec2_Max;

			_label_Min = label_Min;
			_label_Max = label_Max;

			_snapSize = snapSize;

			_isReserved = true;
		}

		// Save / Load
		//--------------------------------------------------------------------------------------
		public void Save(StreamWriter sw)
		{	
			try
			{	
				sw.WriteLine("NAME" + _keyName);
				sw.WriteLine("CATG" + (int)_category);
				sw.WriteLine("ICON" + (int)_iconPreset);
				sw.WriteLine("VTYP" + (int)_valueType);

				sw.WriteLine("ITDF" + _int_Def);
				sw.WriteLine("FLDF" + _float_Def);
				sw.WriteLine("VXDF" + _vec2_Def.x);
				sw.WriteLine("VYDF" + _vec2_Def.y);

				sw.WriteLine("ITMN" + _int_Min);
				sw.WriteLine("ITMX" + _int_Max);

				sw.WriteLine("FLMN" + _float_Min);
				sw.WriteLine("FLMX" + _float_Max);

				sw.WriteLine("VXMN" + _vec2_Min.x);
				sw.WriteLine("VYMN" + _vec2_Min.y);

				sw.WriteLine("VXMX" + _vec2_Max.x);
				sw.WriteLine("VYMX" + _vec2_Max.y);

				sw.WriteLine("LBMN" + _label_Min);
				sw.WriteLine("LBMX" + _label_Max);

				sw.WriteLine("SNAP" + _snapSize);

				sw.WriteLine("UQID" + _uniqueID);
				sw.WriteLine("RSVD" + (_isReserved ? 1 : 0));

			}
			catch (Exception ex)
			{
				Debug.LogError("Save Exception : " + ex);
			}
		}

		public void Load(List<string> loadedStringSet)
		{
			string strKey = "", strValue = "";
			string strCur = "";
			for (int i = 0; i < loadedStringSet.Count; i++)
			{
				strCur = loadedStringSet[i];

				if (strCur.Length < 4)
				{ continue; }

				strKey = strCur.Substring(0, 4);

				if (strCur.Length > 4)
				{ strValue = strCur.Substring(4); }
				else
				{ strValue = ""; }

				try
				{
					if (strKey == "NAME")
					{
						_keyName = strValue;
						if (string.IsNullOrEmpty(_keyName))
						{
							_keyName = "<NoName>";
						}
					}
					else if (strKey == "CATG")
					{
						_category = (apControlParam.CATEGORY)int.Parse(strValue);
					}
					else if (strKey == "ICON")
					{
						_iconPreset = (apControlParam.ICON_PRESET)int.Parse(strValue);
						//sw.WriteLine( + (int)_iconPreset);
					}
					else if (strKey == "VTYP")
					{
						_valueType = (apControlParam.TYPE)int.Parse(strValue);
						//sw.WriteLine( + (int)_valueType);
					}
					else if (strKey == "ITDF")
					{
						_int_Def = int.Parse(strValue);
						//sw.WriteLine("ITDF" + _int_Def);
					}
					else if (strKey == "FLDF")
					{
						_float_Def = apUtil.ParseFloat(strValue);
						//sw.WriteLine("FLDF" + _float_Def);
					}
					else if (strKey == "VXDF")
					{
						_vec2_Def.x = apUtil.ParseFloat(strValue);
						//sw.WriteLine("VXDF" + _vec2_Def.x);
					}
					else if (strKey == "VYDF")
					{
						_vec2_Def.y = apUtil.ParseFloat(strValue);
						//sw.WriteLine("VYDF" + _vec2_Def.y);
					}
					else if (strKey == "ITMN")
					{
						_int_Min = int.Parse(strValue);
						//sw.WriteLine("ITMN" + _int_Min);
					}
					else if (strKey == "ITMX")
					{
						_int_Max = int.Parse(strValue);
						//sw.WriteLine("ITMX" + _int_Max);
					}
					else if (strKey == "FLMN")
					{
						_float_Min = apUtil.ParseFloat(strValue);
						//sw.WriteLine("FLMN" + _float_Min);
					}
					else if (strKey == "FLMX")
					{
						_float_Max = apUtil.ParseFloat(strValue);
						//sw.WriteLine("FLMX" + _float_Max);
					}
					else if (strKey == "VXMN")
					{
						_vec2_Min.x = apUtil.ParseFloat(strValue);
						//sw.WriteLine("VXMN" + _vec2_Min.x);
					}
					else if (strKey == "VYMN")
					{
						_vec2_Min.y = apUtil.ParseFloat(strValue);
						//sw.WriteLine("VYMN" + _vec2_Min.y);
					}
					else if (strKey == "VXMX")
					{
						_vec2_Max.x = apUtil.ParseFloat(strValue);
						//sw.WriteLine("VXMX" + _vec2_Max.x);
					}
					else if (strKey == "VYMX")
					{
						_vec2_Max.y = apUtil.ParseFloat(strValue);
						//sw.WriteLine("VYMX" + _vec2_Max.y);
					}
					else if (strKey == "LBMN")
					{
						_label_Min = strValue;
						//sw.WriteLine("LBMN" + _label_Min);
					}
					else if (strKey == "LBMX")
					{
						_label_Max = strValue;
						//sw.WriteLine("LBMX" + _label_Max);
					}
					else if (strKey == "SNAP")
					{
						_snapSize = int.Parse(strValue);
						//sw.WriteLine("SNAP" + _snapSize);
					}
					else if (strKey == "UQID")
					{
						_uniqueID = int.Parse(strValue);
					}
					else if (strKey == "RSVD")
					{
						_isReserved = (int.Parse(strValue) == 1);
					}

				}
				catch (Exception ex)
				{
					Debug.LogError("Load Exception : " + ex);
				}
			}
		}
	}
}