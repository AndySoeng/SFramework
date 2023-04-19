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
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// AnyPortrait 패키지의 위치를 저장하고 여는 클래스.
	/// 별도의 텍스트 파일에 저장하는 간단한 역할을 수행한다.
	/// apEditor의 멤버 변수이다.
	/// </summary>
	public class apPathSetting
	{
		//싱글톤으로 변경 21.5.30
		private static apPathSetting s_instance = null;
		public static apPathSetting I { get { if (s_instance == null) { s_instance = new apPathSetting(); } return s_instance; } }

		// Members
		//-----------------------------------------------------
		public const string DEFAULT_PATH = "Assets/AnyPortrait/";
		private string _curPath = DEFAULT_PATH;

		private bool _isFirstLoaded = false;//처음으로 로드를 했는가



		// Init
		//-----------------------------------------------------
		private apPathSetting()
		{
			_curPath = DEFAULT_PATH;
			_isFirstLoaded = false;//처음으로 로드를 했는가
		}

		// Functions
		//-----------------------------------------------------
		//추가 21.10.4 : 경로 옵션 문제 개선
		public string RefreshAndGetBasePath(bool isForceLoad)
		{
			if(!_isFirstLoaded || isForceLoad)
			{
				Load();
				apEditorUtil.SetPackagePath(_curPath);
			}

			return _curPath;
		}


		private void Load()//변경 21.10.4 : Public > Private / string > void 리턴
		{
			_curPath = DEFAULT_PATH;
			string filePath = Application.dataPath + "/../AnyPortrait_EditorPath.txt";

			//저장 파일이 있는가
			FileInfo fi = new FileInfo(filePath);//파일 경로 체크됨 (21.9.10)
			if (!fi.Exists)
			{
				_curPath = DEFAULT_PATH;
				_isFirstLoaded = true;//일단 실패했어도 로드 함수가 실행되었으니 오케이
				
				//return DEFAULT_PATH;
				return;//변경
			}

			//열어서 경로를 읽자
			FileStream fs = null;
			StreamReader sr = null;
			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제와 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				_curPath = sr.ReadLine();

				sr.Close();
				fs.Close();

				sr = null;
				fs = null;


				_isFirstLoaded = true;

				//삭제 21.10.4
				//return _curPath;			

			}
			catch (Exception)
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

				_curPath = DEFAULT_PATH;

				_isFirstLoaded = true;//일단 실패했어도 로드 함수가 실행되었으니 오케이

				//삭제
				//return DEFAULT_PATH;
			}
		}

		public bool Save(string path)
		{
			//이전
			//_curPath = path;

			//변경 21.7.3 : 이스케이프 문자 삭제
			_curPath = apUtil.ConvertEscapeToPlainText(path);

			string filePath = Application.dataPath + "/../AnyPortrait_EditorPath.txt";

			FileStream fs = null;
			StreamWriter sw = null;
			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				sw.WriteLine(path);
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

		public void SetDefaultPath()
		{
			Save(DEFAULT_PATH);
		}


		public bool IsFirstLoaded
		{
			get
			{
				return _isFirstLoaded;
			}
		}

		public string CurrentPath
		{
			get
			{
				return _curPath;
			}
		}
	}
}