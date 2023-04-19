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

using System.Collections;
//using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AnyPortrait;



namespace AnyPortrait
{
	/// <summary>
	/// Backup시 각 백업을 해야하는지 체크하는 타이머이다.
	/// 씬 이름 + Portrait의 이름을 기준으로 타이머를 작동한다.
	/// 1분에 한번씩 시간을 갱신하며, 5분에 한번씩 파일로 시간을 저장한다.
	/// </summary>
	public class apBackupTimer
	{
		// Members
		//------------------------------------------------------
		public class PortraitUnit
		{
			public string _sceneName = "";
			public string _portraitName = "";
			public int _timer_Min = 0;
			public DateTime _lastWorkDateTime = DateTime.Now;
			
			public PortraitUnit(string sceneName, string portraitName, int timerMin, DateTime dateTime)
			{
				_sceneName = sceneName;
				_portraitName = portraitName;

				_timer_Min = timerMin;
				_lastWorkDateTime = dateTime;
			}

			
		}

		public List<PortraitUnit> _units = new List<PortraitUnit>();
		public bool _isInitLoaded = false;
		public float _refreshTimerSec = 0;
		public float _fileSaveTimerSec = 0.0f;

		// Init
		//------------------------------------------------------
		public apBackupTimer()
		{
			Clear();
		}

		public void Clear()
		{
			_units.Clear();
			_isInitLoaded = false;
			_refreshTimerSec = 0;
			_fileSaveTimerSec = 0.0f;
		}

		// Update
		//------------------------------------------------------
		/// <summary>
		/// Update를 한다.
		/// return이 True인 경우에 백업 시간을 체크할 타이밍이다.
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		public bool Update(float deltaTime, string fileFolder, apPortrait portrait, string sceneName)
		{
			if(!_isInitLoaded)
			{
				Load(fileFolder);

				_isInitLoaded = true;
			}

			if(portrait == null || string.IsNullOrEmpty(portrait.name) || string.IsNullOrEmpty(sceneName))
			{
				return false;
			}

			//Debug.Log("BackupTimer Update : Delta Time : " + deltaTime);

			bool isUpdatable = false;
			_refreshTimerSec += deltaTime;
			_fileSaveTimerSec += deltaTime;

			if(_refreshTimerSec > 60.0f)
			{
				

				_refreshTimerSec -= 60.0f;
				isUpdatable = true;


				PortraitUnit workUnit = _units.Find(delegate (PortraitUnit a)
				{
					return string.Equals(a._sceneName, sceneName) && string.Equals(a._portraitName, portrait.name);
				});

				if(workUnit == null)
				{
					//현재 등록 안된 Portrait이다. 추가해주자
					workUnit = new PortraitUnit(sceneName, portrait.name, 0, DateTime.Now);
					_units.Add(workUnit);
				}

				//유닛의 타이머를 1 올리고, 갱신 날짜를 오늘로 한다.
				workUnit._timer_Min++;
				workUnit._lastWorkDateTime = DateTime.Now;

				//Debug.Log("BackupTimer Update : Timer Refresh : " + sceneName + "-" + portrait.name + " / " + workUnit._timer_Min);
			}

			
			//일정 시간(5분)이 지나면 타이머 시간도 저장을 하자
			if(_fileSaveTimerSec > 300.0f)
			{
				Save(fileFolder);
				_fileSaveTimerSec = 0.0f;
			}

			return isUpdatable;
		}


		// Save / Load
		//------------------------------------------------------
		public void Load(string fileFolder)
		{
			FileStream fs = null;
			StreamReader sr = null;

			try
			{
				//변경 21.7.3 : 경로 + 인코딩 문제
				string filePath = apUtil.ConvertEscapeToPlainText(fileFolder + "/anyportraitBackupTime.dat");
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				Clear();

				sr.ReadLine();//Backup Table
				int nUnit = int.Parse(sr.ReadLine());
				for (int i = 0; i < nUnit; i++)
				{
					string strSceneName = sr.ReadLine();
					string strPortraitName = sr.ReadLine();
					int iTime = int.Parse(sr.ReadLine());
					int iYear = int.Parse(sr.ReadLine());
					int iMonth = int.Parse(sr.ReadLine());
					int iDay = int.Parse(sr.ReadLine());

					PortraitUnit unit = new PortraitUnit(strSceneName, strPortraitName, iTime, new DateTime(iYear, iMonth, iDay));
					_units.Add(unit);
				}

				sr.Close();
				fs.Close();
				sr = null;
				fs = null;

				_isInitLoaded = true;
			}
			catch (Exception)
			{
				if(sr != null)
				{
					sr.Close();
					sr = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}

				//파일이 없었다면
				//Save를 한다.
				Save(fileFolder);
			}
		}

		private void Save(string fileFolder)
		{
			FileStream fs = null;
			StreamWriter sw = null;

			//Debug.Log("----------- Save Backup Timer ------------");
			try
			{
				//이전
				//fs = new FileStream(fileFolder + "/anyportraitBackupTime.dat", FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(fileFolder + "/anyportraitBackupTime.dat"), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				sw.WriteLine("Backup Table");
				sw.WriteLine(_units.Count);
				for (int i = 0; i < _units.Count; i++)
				{
					PortraitUnit unit = _units[i];

					//저장은 : 작업 날짜가 일주일 이내인 것들만
					if(DateTime.Now.Subtract(unit._lastWorkDateTime).TotalDays > 7)
					{
						//일주일 이상 작업하지 않은 Portrait이다.
						//백업하지 않는다.
						//Debug.LogError("저장한지 오래된 Portrait [" + unit._sceneName + " - " + unit._portraitName + " / " + unit._lastWorkDateTime.ToString());
						continue;
					}

					sw.WriteLine(unit._sceneName);
					sw.WriteLine(unit._portraitName);
					sw.WriteLine(unit._timer_Min);
					sw.WriteLine(unit._lastWorkDateTime.Year);
					sw.WriteLine(unit._lastWorkDateTime.Month);
					sw.WriteLine(unit._lastWorkDateTime.Day);

					//Debug.Log("[" + i + "] Name : " + unit._sceneName + " - " + unit._portraitName + " / Time : " + unit._timer_Min + " / Last Saved : " + unit._lastWorkDateTime.ToString());
				}

				sw.Flush();

				sw.Close();
				fs.Close();
				sw = null;
				fs = null;

				
			}
			catch (Exception)
			{
				if(sw != null)
				{
					sw.Close();
					sw = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}
			}

			_isInitLoaded = true;//<<저장을 했으면 Load한 것과 마찬가지
		}


		// Get / Set
		//------------------------------------------------------
		public int GetPortraitBackupTime(string sceneName, apPortrait portrait)
		{
			PortraitUnit workUnit = _units.Find(delegate (PortraitUnit a)
				{
					return string.Equals(a._sceneName, sceneName) && string.Equals(a._portraitName, portrait.name);
				});

			if(workUnit == null)
			{
				return 0;
			}

			return workUnit._timer_Min;
		}

		public void ResetTimerAndSave(string sceneName, apPortrait portrait, string fileFolder)
		{
			PortraitUnit workUnit = _units.Find(delegate (PortraitUnit a)
				{
					return string.Equals(a._sceneName, sceneName) && string.Equals(a._portraitName, portrait.name);
				});

			if(workUnit != null)
			{
				workUnit._timer_Min = 0;
				workUnit._lastWorkDateTime = DateTime.Now;
			}

			Save(fileFolder);
		}
	}
}