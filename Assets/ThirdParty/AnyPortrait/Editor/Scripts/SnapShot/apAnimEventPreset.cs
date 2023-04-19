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
using System.Text;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 애니메이션 이벤트 프리셋을 저장해둘 수 있다.
	/// 파일로 저장된다. (Serialized는 아니다.)
	/// </summary>
	public class apAnimEventPreset
	{
		// Members
		//----------------------------------------------------------
		private List<apAnimEventPresetUnit> _units = new List<apAnimEventPresetUnit>();
		public List<apAnimEventPresetUnit> Presets { get { return _units; } }


		private string FILE_PATH = "/../AnyPortrait_AnimEventPreset.txt";
		private string STR_DELIMETER = "-----";


		// Init
		//----------------------------------------------------------
		public apAnimEventPreset()
		{
			Clear();
		}

		public void Clear()
		{
			if(_units == null)
			{
				_units = new List<apAnimEventPresetUnit>();
			}
			_units.Clear();
		}


		// Functions
		//----------------------------------------------------------
		/// <summary>
		/// 애니메이션 이벤트를 프리셋으로 저장하자.
		/// </summary>
		/// <param name="srcAnimEvent"></param>
		public apAnimEventPresetUnit AddEventAsPreset(apAnimEvent srcAnimEvent)
		{
			if(srcAnimEvent == null)
			{
				return null;
			}

			apAnimEventPresetUnit newUnit = new apAnimEventPresetUnit();
			
			newUnit.CopyFromAnimEvent(srcAnimEvent);
			_units.Add(newUnit);

			//파일로도 저장하자
			Save();

			return newUnit;
		}


		public void AdaptToEventFromPreset(apAnimEventPresetUnit srcPresetUnit, apAnimEvent targetEvent)
		{
			if(srcPresetUnit == null || targetEvent == null)
			{
				return;
			}

			//프리셋의 값을 이벤트에 복사하자
			srcPresetUnit.CopyToAnimEvent(targetEvent);
		}

		//프리셋을 삭제한다.
		public void RemovePreset(apAnimEventPresetUnit targetPresetUnit)
		{
			if(targetPresetUnit == null)
			{
				return;
			}

			_units.Remove(targetPresetUnit);
		}

		// Functions : File Save
		//----------------------------------------------------------
		/// <summary>
		/// 파일로 저장한다. AnyPortrait_AnimEventPreset.txt에 저장된다.
		/// </summary>
		public void Save()
		{
			FileStream fs = null;
			StreamWriter sw = null;

			string filePath = apUtil.ConvertEscapeToPlainText(Application.dataPath + FILE_PATH);

			try
			{
				
				fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);


				apAnimEventPresetUnit curUnit = null;
				int nUnits = _units != null ? _units.Count : 0;

				if (nUnits > 0)
				{
					for (int i = 0; i < nUnits; i++)
					{
						curUnit = _units[i];

						//Write
						curUnit.EncodeToText(sw);

						sw.WriteLine(STR_DELIMETER);
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
				Debug.LogError("Animation Event Preset Save Exception : " + ex);

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


		// Functions : File Load
		//----------------------------------------------------------
		public void Load()
		{
			FileStream fs = null;
			StreamReader sr = null;

			string filePath = apUtil.ConvertEscapeToPlainText(Application.dataPath + FILE_PATH);

			Clear();

			try
			{
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8);

				

				//구분자 전까지 모든 데이터를 리스트로 긁어와서 처리한다.
				List<string> strData = new List<string>();

				while (true)
				{
					if (sr.Peek() < 0)
					{
						//파일 끝에 도달.
						//만약 남아있는 데이터가 있다면 이것도 파싱해서 저장한다
						if(strData.Count > 0)
						{
							apAnimEventPresetUnit newUnit = new apAnimEventPresetUnit();

							bool isResult = newUnit.DecodeFromText(strData);
							if(isResult)
							{
								//파싱 성공
								_units.Add(newUnit);
							}

							strData.Clear();
						}
						break;
					}

					string strRead = sr.ReadLine();
					bool isDelimeter = string.Equals(strRead, STR_DELIMETER) || strRead.StartsWith(STR_DELIMETER);

					if(isDelimeter)
					{
						//Delimeter를 만났다면
						//파싱해서 유닛에 추가한다.

						apAnimEventPresetUnit newUnit = new apAnimEventPresetUnit();

						bool isResult = newUnit.DecodeFromText(strData);
						if(isResult)
						{
							//파싱 성공
							_units.Add(newUnit);
						}

						//다음 파싱을 위해 저장된 리스트 비우기
						strData.Clear();
					}
					else
					{
						//데이터를 추가한다.
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


				if (ex is FileNotFoundException)
				{
					//파일이 없다
				}
				else
				{
					Debug.LogError("Animation Event Preset Load Exception : " + ex);
				}
				

				
			}
		}


	}





	//---------------------------------------------------------------------
	// 서브 클래스 : 유닛
	//---------------------------------------------------------------------

	/// <summary>
	/// 애니메이션 이벤트 프리셋의 유닛.
	/// 프레임과 같이 특수한 경우를 제외하고, 애니메이션 이벤트의 기본형들을 저장한다.
	/// 파일에 저장되므로, Serialized가 필요없다.
	/// </summary>
	public class apAnimEventPresetUnit
	{
		// Members
		//---------------------------------------------------------------------
		//1. 이벤트 이름
		public string _eventName = "";

		//2. Call Type
		public apAnimEvent.CALL_TYPE _callType = apAnimEvent.CALL_TYPE.Once;

		//3. 색상
		public apAnimEvent.ICON_COLOR _iconColor = apAnimEvent.ICON_COLOR.Yellow;


		//파라미터들 (기본값을 지정할 수 있다.)
		public class SubParamInfo
		{
			public apAnimEvent.PARAM_TYPE _paramType = apAnimEvent.PARAM_TYPE.Integer;

			public bool _defValue_Bool = false;
			public int _defValue_Int = 0;
			public float _defValue_Float = 0.0f;
			public Vector2 _defValue_Vector2 = Vector2.zero;
			public string _defValue_String = "";

			//End도
			public int _defValue_Int_End = 0;
			public float _defValue_Float_End = 0;
			public Vector2 _defValue_Vector2_End = Vector2.zero;

			public SubParamInfo()
			{
				_paramType = apAnimEvent.PARAM_TYPE.Integer;
				_defValue_Bool = false;
				_defValue_Int = 0;
				_defValue_Float = 0.0f;
				_defValue_Vector2 = Vector2.zero;
				_defValue_String = "";

				_defValue_Int_End = 0;
				_defValue_Float_End = 0;
				_defValue_Vector2_End = Vector2.zero;
			}

			public void CopyFromSrcAnimParam(apAnimEvent.SubParameter srcParam)
			{
				_paramType = srcParam._paramType;

				//현재 값을 기본값으로 한다.
				_defValue_Bool = srcParam._boolValue;
				_defValue_Int = srcParam._intValue;
				_defValue_Float = srcParam._floatValue;
				_defValue_Vector2 = srcParam._vec2Value;
				_defValue_String = srcParam._strValue;

				_defValue_Int_End = srcParam._intValue_End;
				_defValue_Float_End = srcParam._floatValue_End;
				_defValue_Vector2_End = srcParam._vec2Value_End;
			}

			public void EncodeToTexts(StreamWriter sw)
			{
				//값들을 하나씩 리스트에 순서대로 넣자
				//S_타입(3) + : "값" 순서대로 넣자
				//타입에 맞는 값만 저장하자

				switch (_paramType)
				{
					case apAnimEvent.PARAM_TYPE.Bool:
						{
							sw.WriteLine("S_TYP:BOOL");
							sw.WriteLine("S_BOL:" + (_defValue_Bool ? "TRUE" : "FALSE"));
						}
						break;

					case apAnimEvent.PARAM_TYPE.Integer:
						{
							sw.WriteLine("S_TYP:INT");
							sw.WriteLine("S_INT:" + _defValue_Int);
							sw.WriteLine("S_INE:" + _defValue_Int_End);
						}
						break;

					case apAnimEvent.PARAM_TYPE.Float:
						{
							sw.WriteLine("S_TYP:FLOAT");
							sw.WriteLine("S_FLT:" + _defValue_Float);
							sw.WriteLine("S_FLE:" + _defValue_Float_End);
						}
						break;

					case apAnimEvent.PARAM_TYPE.Vector2:
						{
							sw.WriteLine("S_TYP:VECTOR");
							sw.WriteLine("S_V2X:" + _defValue_Vector2.x);
							sw.WriteLine("S_V2Y:" + _defValue_Vector2.y);
							sw.WriteLine("S_VXE:" + _defValue_Vector2_End.x);
							sw.WriteLine("S_VYE:" + _defValue_Vector2_End.y);
						}
						break;

					case apAnimEvent.PARAM_TYPE.String:
						{
							//특수 문자 제거
							string strValue = _defValue_String;
							strValue = strValue.Replace("\n", "");
							strValue = strValue.Replace("\r", "");
							strValue = strValue.Replace("\t", "");

							sw.WriteLine("S_TYP:STRING");
							sw.WriteLine("S_STR:" + strValue);
						}
						break;

					default:
						{
							sw.WriteLine("S_TYP:UNKNOWN");
						}
						break;
				}

				
			}

			

			
		}

		public List<SubParamInfo> _subParams = new List<SubParamInfo>();


		// Init
		//---------------------------------------------------------------------
		public apAnimEventPresetUnit()
		{

		}

		public void Init()
		{
			_eventName = "";
			_callType = apAnimEvent.CALL_TYPE.Once;
			_iconColor = apAnimEvent.ICON_COLOR.Yellow;
			if(_subParams == null)
			{
				_subParams = new List<SubParamInfo>();
			}
			_subParams.Clear();
		}

		// 애니메이션 이벤트로부터 값을 받아서 프리셋으로 저장하기
		//---------------------------------------------------------------------
		public void CopyFromAnimEvent(apAnimEvent srcAnimEvent)
		{
			Init();

			_eventName = srcAnimEvent._eventName;
			_callType = srcAnimEvent._callType;
			_iconColor = srcAnimEvent._iconColor;
			
			int nParams = srcAnimEvent._subParams != null ? srcAnimEvent._subParams.Count : 0;

			_subParams.Clear();

			if(nParams > 0)
			{
				apAnimEvent.SubParameter srcSubParam = null;
				SubParamInfo newParamInfo = null;

				for (int i = 0; i < nParams; i++)
				{
					srcSubParam = srcAnimEvent._subParams[i];
					newParamInfo = new SubParamInfo();
					newParamInfo.CopyFromSrcAnimParam(srcSubParam);

					_subParams.Add(newParamInfo);
				}
			}
		}

		// 프리셋 > 이벤트로 복사하기
		//-----------------------------------------------------------------------------
		public void CopyToAnimEvent(apAnimEvent targetAnimEvent)
		{	
			targetAnimEvent._eventName = _eventName;
			targetAnimEvent._callType = _callType;
			targetAnimEvent._iconColor = _iconColor;

			if(targetAnimEvent._subParams == null)
			{
				targetAnimEvent._subParams = new List<apAnimEvent.SubParameter>();
			}
			targetAnimEvent._subParams.Clear();
			
			int nParams = _subParams != null ? _subParams.Count : 0;

			if(nParams > 0)
			{
				SubParamInfo srcParamInfo = null;
				apAnimEvent.SubParameter targetSubParam = null;

				for (int i = 0; i < nParams; i++)
				{
					srcParamInfo = _subParams[i];
					targetSubParam = new apAnimEvent.SubParameter();
					

					//모든 기본값을 넣진 말고, 타입에 맞는것만 넣자
					targetSubParam._paramType = srcParamInfo._paramType;

					switch (srcParamInfo._paramType)
					{
						case apAnimEvent.PARAM_TYPE.Bool:
							targetSubParam._boolValue = srcParamInfo._defValue_Bool;
							break;

						case apAnimEvent.PARAM_TYPE.Integer:
							targetSubParam._intValue = srcParamInfo._defValue_Int;
							targetSubParam._intValue_End = srcParamInfo._defValue_Int_End;
							break;

						case apAnimEvent.PARAM_TYPE.Float:
							targetSubParam._floatValue = srcParamInfo._defValue_Float;
							targetSubParam._floatValue_End = srcParamInfo._defValue_Float_End;
							break;

						case apAnimEvent.PARAM_TYPE.Vector2:
							targetSubParam._vec2Value = srcParamInfo._defValue_Vector2;
							targetSubParam._vec2Value_End = srcParamInfo._defValue_Vector2_End;
							break;

						case apAnimEvent.PARAM_TYPE.String:
							targetSubParam._strValue = srcParamInfo._defValue_String;
							break;
					}

					//리스트에 넣기
					targetAnimEvent._subParams.Add(targetSubParam);
				}
			}
		}

		// 텍스트로 변환하기 / 그 반대
		//--------------------------------------------------------------------------------------
		public void EncodeToText(StreamWriter sw)
		{
			//기본값
			//3글자 키 + : + 값으로 하자
			sw.WriteLine("NAM:" + (string.IsNullOrEmpty(_eventName) ? "<Noname>" : _eventName));
			
			if(_callType == apAnimEvent.CALL_TYPE.Once)
			{
				sw.WriteLine("CAL:ONCE");
			}
			else
			{
				sw.WriteLine("CAL:CONT");
			}

			switch (_iconColor)
			{
				case apAnimEvent.ICON_COLOR.Yellow:		sw.WriteLine("ICO:YELLOW");		break;
				case apAnimEvent.ICON_COLOR.Green:		sw.WriteLine("ICO:GREEN");		break;
				case apAnimEvent.ICON_COLOR.Blue:		sw.WriteLine("ICO:BLUE");		break;
				case apAnimEvent.ICON_COLOR.Red:		sw.WriteLine("ICO:RED");		break;
				case apAnimEvent.ICON_COLOR.Cyan:		sw.WriteLine("ICO:CYAN");		break;
				case apAnimEvent.ICON_COLOR.Magenta:	sw.WriteLine("ICO:MAGENTA");	break;
				case apAnimEvent.ICON_COLOR.White:		sw.WriteLine("ICO:WHITE");		break;
			}

			int nParams = _subParams != null ? _subParams.Count : 0;
			sw.WriteLine("NPR:" + nParams);

			//Param Delimeter
			
			//파라미터들은 순서대로 한줄씩 입력한다.
			if(nParams > 0)
			{
				SubParamInfo curParam = null;
				for (int i = 0; i < nParams; i++)
				{
					curParam = _subParams[i];

					//파싱용 시작 라인
					sw.WriteLine("S_PAR:" + i);

					//문자열 리스트에 인코딩을 하자
					curParam.EncodeToTexts(sw);

					sw.WriteLine("S_END:<<");
				}
			}
		}


		//텍스트에서 값을 받아온다.
		public bool DecodeFromText(List<string> strData)
		{
			Init();

			if(strData == null || strData.Count == 0)
			{
				return false;
			}

			bool isParse_Basic = true;
			
			int nStrData = strData.Count;

			SubParamInfo curParseParam = null;

			for (int i = 0; i < nStrData; i++)
			{
				string curStr = strData[i];


				if(isParse_Basic)
				{
					//기본 설정을 가져오자
					if(curStr.Length < 5)
					{
						//텍스트 길이 부족
						continue;
					}
					string strKey = curStr.Substring(0, 3);
					string strValue = curStr.Substring(4);

					if(string.Equals(strKey, "NAM"))
					{
						//이벤트 이름
						_eventName = strValue;
					}
					else if(string.Equals(strKey, "CAL"))
					{
						//호출 방식
						_callType = strValue.StartsWith("ONCE") ? apAnimEvent.CALL_TYPE.Once : apAnimEvent.CALL_TYPE.Continuous;
					}
					else if(string.Equals(strKey, "ICO"))
					{
						//아이콘
						if(strValue.StartsWith("YELLOW"))		{ _iconColor = apAnimEvent.ICON_COLOR.Yellow; }
						else if(strValue.StartsWith("GREEN"))	{ _iconColor = apAnimEvent.ICON_COLOR.Green; }
						else if(strValue.StartsWith("BLUE"))	{ _iconColor = apAnimEvent.ICON_COLOR.Blue; }
						else if(strValue.StartsWith("RED"))		{ _iconColor = apAnimEvent.ICON_COLOR.Red; }
						else if(strValue.StartsWith("CYAN"))	{ _iconColor = apAnimEvent.ICON_COLOR.Cyan; }
						else if(strValue.StartsWith("MAGENTA"))	{ _iconColor = apAnimEvent.ICON_COLOR.Magenta; }
						else if(strValue.StartsWith("WHITE"))	{ _iconColor = apAnimEvent.ICON_COLOR.White; }
					}
					else if(string.Equals(strKey, "NPR"))
					{
						//서브 파라미터 파싱 시작
						isParse_Basic = false;
					}
				}
				else
				{
					//서브 파라미터를 파싱한다.
					//키값의 길이가 5글자다
					//키(5) + :(1) + (6..)

					if(curStr.Length < 7)
					{
						//텍스트 길이 부족
						continue;
					}
					string strKey = curStr.Substring(0, 5);
					string strValue = curStr.Substring(6);

					if (string.Equals(strKey, "S_PAR"))
					{
						//파싱 시작 (바로 리스트에 넣는다.)
						if(curParseParam != null)
						{
							//남아있는 파싱중인 파라미터가 리스트에 속하지 않는다면
							if(!_subParams.Contains(curParseParam))
							{
								_subParams.Add(curParseParam);
							}
						}

						//새로운 파라미터 생성
						curParseParam = new SubParamInfo();
						_subParams.Add(curParseParam);
					}
					else if (string.Equals(strKey, "S_END"))
					{
						//현재 유닛 파싱 끝
						curParseParam = null;
					}
					else if(curParseParam != null)
					{
						//나머지는 파싱중인 유닛이 있어야만 한다.
						if (string.Equals(strKey, "S_TYP"))
						{
							//타입
							if (strValue.StartsWith("BOOL"))		{ curParseParam._paramType = apAnimEvent.PARAM_TYPE.Bool; }
							else if (strValue.StartsWith("INT"))	{ curParseParam._paramType = apAnimEvent.PARAM_TYPE.Integer; }
							else if (strValue.StartsWith("FLOAT"))	{ curParseParam._paramType = apAnimEvent.PARAM_TYPE.Float; }
							else if (strValue.StartsWith("VECTOR"))	{ curParseParam._paramType = apAnimEvent.PARAM_TYPE.Vector2; }
							else if (strValue.StartsWith("STRING"))	{ curParseParam._paramType = apAnimEvent.PARAM_TYPE.String; }
						}
						else if (string.Equals(strKey, "S_BOL"))
						{
							curParseParam._defValue_Bool = strValue.StartsWith("TRUE") ? true : false;
						}
						else if (string.Equals(strKey, "S_INT"))
						{
							int.TryParse(strValue, out curParseParam._defValue_Int);
						}
						else if (string.Equals(strKey, "S_INE"))
						{
							int.TryParse(strValue, out curParseParam._defValue_Int_End);
						}
						else if (string.Equals(strKey, "S_FLT"))
						{
							float.TryParse(strValue, out curParseParam._defValue_Float);
						}
						else if (string.Equals(strKey, "S_FLE"))
						{
							float.TryParse(strValue, out curParseParam._defValue_Float_End);
						}
						else if (string.Equals(strKey, "S_V2X"))
						{
							float.TryParse(strValue, out curParseParam._defValue_Vector2.x);
						}
						else if (string.Equals(strKey, "S_V2Y"))
						{
							float.TryParse(strValue, out curParseParam._defValue_Vector2.y);
						}
						else if (string.Equals(strKey, "S_VXE"))
						{
							float.TryParse(strValue, out curParseParam._defValue_Vector2_End.x);
						}
						else if (string.Equals(strKey, "S_VYE"))
						{
							float.TryParse(strValue, out curParseParam._defValue_Vector2_End.y);
						}
						else if (string.Equals(strKey, "S_STR"))
						{
							curParseParam._defValue_String = strValue;
						}
					}
				}
			}


			return true;
		}

		public Color GetIconColor()
		{
			switch (_iconColor)
			{
				case apAnimEvent.ICON_COLOR.Yellow:		return Color.yellow;
				case apAnimEvent.ICON_COLOR.Green:		return Color.green;
				case apAnimEvent.ICON_COLOR.Blue:		return Color.blue;
				case apAnimEvent.ICON_COLOR.Red:		return Color.red;
				case apAnimEvent.ICON_COLOR.Cyan:		return Color.cyan;
				case apAnimEvent.ICON_COLOR.Magenta:	return Color.magenta;
				case apAnimEvent.ICON_COLOR.White:		return Color.white;
			}
			return Color.gray;
		}

	}
}