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
	/// 필요한 데이터를 입력하면 이후 출력을 할 수 있다.
	/// 싱글톤으로 작성
	/// 필요한 데이터 구조는 그때그때 작성하자
	/// 저장된 파일은 시간+Index로 저장된다.
	/// </summary>
	public class apDebugLog
	{
		// SingleTone
		//----------------------------------------------------------------------------
		private static apDebugLog _instance = new apDebugLog();
		public static apDebugLog I { get { return _instance; } }

		// Members
		//----------------------------------------------------------------------------

		private class SavedUnit
		{
			//사용자 정의 Data
			//Physic 로그를 정리하자
			private int _vertexIndex = 0;
			private int _updateIndex = 0;
			private float _tDelta = 0.0f;
			//private Vector2 _F_gravity;
			//private Vector2 _F_wind;
			private Vector2 _F_stretch;
			private Vector2 _F_airDrag;
			private Vector2 _F_recover;
			private Vector2 _F_sum;
			//private Vector2 _pos_World_NoMod;
			private Vector2 _pos_1F;
			private Vector2 _pos_Real;
			private Vector2 _pos_Predict;
			//private Vector2 _velocity_1F;
			private Vector2 _velocity_Real;
			private Vector2 _velocity_Next;
			private Vector2 _result_CalPos;
			private DateTime _recordTime;

			public SavedUnit(int vertexIndex,
								int updateIndex,
								float tDelta,
								Vector2 F_gravity,
								Vector2 F_wind,
								Vector2 F_stretch,
								Vector2 F_airDrag,
								Vector2 F_recover,
								Vector2 F_sum,
								Vector2 pos_World_NoMod,
								Vector2 pos_1F,
								Vector2 pos_Real,
								Vector2 pos_Predict,
								Vector2 velocity_1F,
								Vector2 velocity_Real,
								Vector2 velocity_Next,
								Vector2 result_CalPos
								)
			{
				_vertexIndex = vertexIndex;
				_updateIndex = updateIndex;
				_tDelta = tDelta;
				//_F_gravity = F_gravity;
				//_F_wind = F_wind;
				_F_stretch = F_stretch;
				_F_airDrag = F_airDrag;
				_F_recover = F_recover;
				_F_sum = F_sum;
				//_pos_World_NoMod = pos_World_NoMod;
				_pos_1F = pos_1F;
				_pos_Real = pos_Real;
				_pos_Predict = pos_Predict;
				//_velocity_1F = velocity_1F;
				_velocity_Real = velocity_Real;
				_velocity_Next = velocity_Next;
				_result_CalPos = result_CalPos;
				_recordTime = DateTime.Now;
			}

			public static string GetTitleLabel()
			{
				return "Vert Index,"
						+ "Update Index,"
						+ "tDelta,"
						+ "Record Time,"
						+ "Real dTime(Sec),"
						+ "tDelta Correct,"
						//+ "PosW NoMod,"
						+ "PosW 1F,"
						+ "PosW Real,"
						+ "PosW Predict,"
						//+ "Vel 1F,"
						+ "Vel Real,"
						//+ "F gravity,"
						//+ "F wind,"
						+ "F stretch,"
						+ "F airDrag,"
						+ "F recover,"
						+ "F sum,"
						+ "Vel Next,"
						+ "Cal Pos,"
						;
			}

			public string GetDataRow(SavedUnit prevData)
			{
				float real_deltaTime = 0.0f;
				float tDeltaCorrect = 100.0f;
				if (prevData != null)
				{
					int iDeltaTime = (_recordTime.Second * 1000 + _recordTime.Millisecond)
						- (prevData._recordTime.Second * 1000 + prevData._recordTime.Millisecond);

					real_deltaTime = (float)iDeltaTime / 1000.0f;

					if (_tDelta > 0.0f)
					{
						tDeltaCorrect = (real_deltaTime / _tDelta) * 100.0f;
					}
				}
				return _vertexIndex + ","
					+ _updateIndex + ","
					+ _tDelta + ","
					+ (_recordTime.Second * 1000 + _recordTime.Millisecond) + ","
					+ real_deltaTime + ","
					+ (int)tDeltaCorrect + ","
					//+ Vec2ToString(_pos_World_NoMod, false) + ","
					+ Vec2ToString(_pos_1F, false) + ","
					+ Vec2ToString(_pos_Real, false) + ","
					+ Vec2ToString(_pos_Predict, false) + ","

					//+ Vec2ToString(_velocity_1F, true) + ","
					+ Vec2ToString(_velocity_Real, true) + ","

					//+ Vec2ToString(_F_gravity, true) + ","
					//+ Vec2ToString(_F_wind, true) + ","
					+ Vec2ToString(_F_stretch, true) + ","
					+ Vec2ToString(_F_airDrag, true) + ","
					+ Vec2ToString(_F_recover, true) + ","
					+ Vec2ToString(_F_sum, true) + ","

					+ Vec2ToString(_velocity_Next, true) + ","

					+ Vec2ToString(_result_CalPos, false) + ","
					;
			}

			private string Vec2ToString(Vector2 vec2, bool isSizeContain)
			{
				if (isSizeContain)
				{
					//return string.Format("({0:F3} {1:F3} / {2:F3})", vec2.x, vec2.y, vec2.magnitude);
					return string.Format("{0:F4}", vec2.magnitude);
				}
				else
				{
					return string.Format("({0:F3} {1:F3})", vec2.x, vec2.y);
				}
			}


		}

		private List<SavedUnit> _saveUnits = new List<SavedUnit>();

		// Init
		//----------------------------------------------------------------------------
		private apDebugLog()
		{
			Clear();
		}


		public void Clear()
		{
			_saveUnits.Clear();
		}


		public void AddUnit(int vertexIndex,
								int updateIndex,
								float tDelta,
								Vector2 F_gravity,
								Vector2 F_wind,
								Vector2 F_stretch,
								Vector2 F_airDrag,
								Vector2 F_recover,
								Vector2 F_sum,
								Vector2 pos_World_NoMod,
								Vector2 pos_1F,
								Vector2 pos_Real,
								Vector2 pos_Predict,
								Vector2 velocity_1F,
								Vector2 velocity_Real,
								Vector2 velocity_Next,
								Vector2 result_CalPos
								)
		{
			_saveUnits.Add(new SavedUnit(vertexIndex,
								updateIndex,
								tDelta,
								F_gravity,
								F_wind,
								F_stretch,
								F_airDrag,
								F_recover,
								F_sum,
								pos_World_NoMod,
								pos_1F,
								pos_Real,
								pos_Predict,
								velocity_1F,
								velocity_Real,
								velocity_Next,
								result_CalPos));
		}





		// Basic Functions
		//----------------------------------------------------------------------------



		public string Save(string fileLabel)
		{
			string filePath = Application.dataPath;
			if (filePath.Length > 6)
			{
				//Assets [6]를 뺀다.
				filePath = filePath.Substring(0, filePath.Length - 6);
			}
			string strDateTime = DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
			filePath += "DebugLogs/APDebugFile__" + fileLabel + "_" + strDateTime + ".csv";
			FileStream fs = null;
			StreamWriter sw = null;

			try
			{
				fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs);

				sw.WriteLine("AnyPortriat Debug [ " + strDateTime + " ]");

				//여기서부터 디버그 대상을 적자 (가급적 엑셀로 옮길 수 있게 )
				sw.WriteLine(SavedUnit.GetTitleLabel());
				SavedUnit prevUnit = null;
				for (int i = 0; i < _saveUnits.Count; i++)
				{
					sw.WriteLine(_saveUnits[i].GetDataRow(prevUnit));

					prevUnit = _saveUnits[i];
				}

				sw.Close();
				fs.Close();
				sw = null;
				fs = null;
			}
			catch (Exception ex)
			{
				Debug.LogError("Debug Save Exception : " + ex);

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
				Clear();
				return null;
			}

			Clear();

			return filePath;
		}


		// Get / Set
		//----------------------------------------------------------------------------
	}

}