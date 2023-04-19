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
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.5.13 : 에디터 가속을 위해서 C++로 작성된 플러그인을 실행시킬 수 있다.
	/// C++ 함수들은 필요한 곳에서 직접 사용하되, "유효성 검사", "플러그 설치"등을 도와준다.
	/// </summary>
	[InitializeOnLoad]
	public class apPluginUtil
	{
		//싱글톤
		//-------------------------------------------------
		private static apPluginUtil s_instance = null;
		public static apPluginUtil I { get { if (s_instance == null) { s_instance = new apPluginUtil(); } return s_instance; } }

		// Members
		//-------------------------------------------------
		public enum VALIDATE_RESULT
		{
			/// <summary>유효성 검사를 하지 않았다.</summary>
			Unknown,
			/// <summary>설치가 요청된 상태이다.</summary>
			InstallationRequested,
			/// <summary>지원되지 않은 환경이다.</summary>
			NotSupported,
			/// <summary>설치되지 않았은 것 같다. 해당 함수를 동작시킬 수 없다.</summary>
			NotInstalled,
			/// <summary>설치는 되었으나 값이 유효하지 않다.</summary>
			InstalledButInvalid,
			/// <summary>설치는 되었으나 이전 버전의 값이다.</summary>
			InstalledButOldVersion,
			/// <summary>DLL을 사용할 수 있다.</summary>
			Valid
		}



		

		//설치 가능한가
		//- 한번 설치 여부 조회된 이후엔 불가
		//- Validate 이후엔 불가
		private bool _isInstallable = true;
		private bool _isImporting = false;//임포트 중일 때에는 Validate 불가


		public const string PACKAGE_NAME_WIN64 = "Accelerated Mode Plugin Win64";
		public const string PACKAGE_NAME_MACOS = "Accelerated Mode Plugin MacOS";


		//설치 요청은 두단계로 동작한다.
		//설치 요청 > Step1 활성 (Step2 비활성)
		//에디터 재시작 > Step1이 활성화 되었다면 Step2 활성 (Step1 비활성)
		//AnyPortrait 실행
		// - 활성화된 Pref 키가 없다면 : 플러그인 Validate가 제한되지 않는다.
		// - Step1 활성 : Step2 여부에 상관없이 Validate가 제한된다.
		// - Step2 활성 : 설치를 한다. 설치 결과에 상관없이 Step1, Step2는 모두 비활성화된다.

		public const string PREF_KEY_REQUEST_INSTALL_STEP1 = "AnyPortrait_RequestInstallCPPDLL";
		public const string PREF_KEY_REQUEST_INSTALL_STEP2 = "AnyPortrait_RequestInstallCPPDLL_Step2";

		//에디터가 처음 열리는지 확인하는 방법 : 설치 요청을 했을때의 "에디터 시간"보다 작으면 된다.
		public const string PREF_KEY_REQUEST_INSTALL_TIME = "AnyPortrait_RequestInstallTime";
		public const int MAX_INSTALL_REQUEST_TIME_SEC = 100000;//86400초가 1일. 최대 저장값은 에디터가 1일 이상 실행되었다는 가정

		///// <summary>
		///// 각 Step1, Step2의 활성, 비활성 상태를 조회하고 저장한다.
		///// </summary>
		//public enum STEP_STATUS
		//{
		//	Unknown,
		//	NoRequest,
		//	Step1,
		//	Step2,
		//}
		//private STEP_STATUS _stepStatus = STEP_STATUS.Unknown;
		
		// Init
		//--------------------------------------------------------------------
		static apPluginUtil()
		{
			//Debug.LogError("에디터 생성시 apPluginUtil 시작 > [ 실행 시간 : " + EditorApplication.timeSinceStartup + " ]");
			
			//플러그인 요청 단계
			if(EditorPrefs.GetBool(PREF_KEY_REQUEST_INSTALL_STEP1, false))
			{
				//플러그인 설치 요청이 있다면
				//에디터가 "리셋된 상태"에서 초기화 함수가 호출된 것인지 확인하자.
				int requestedTime = EditorPrefs.GetInt(PREF_KEY_REQUEST_INSTALL_TIME, MAX_INSTALL_REQUEST_TIME_SEC);//설치가 요청된 에디터 시간
				double editorTime = EditorApplication.timeSinceStartup;

				//Debug.LogWarning("플러그인 설치 요청 시간 : " + requestedTime + " / 현재 에디터 시간 : " + editorTime);

				if(editorTime < requestedTime || editorTime < 20.0)
				{	
					EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP1);//Step1 비활성
					EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_TIME);//설치 요청 시간 삭제
					EditorPrefs.SetBool(PREF_KEY_REQUEST_INSTALL_STEP2, true);//Step2 활성
				}
			}
		}

		private apPluginUtil()
		{
			//_stepStatus = STEP_STATUS.Unknown;//처음엔 알 수 없다.
		}


		// Get/Set Status
		//--------------------------------------------------------------------
		///// <summary>설치 요청을 확인한다. (설치 요청이 있다면 Validate 불가)</summary>
		//public STEP_STATUS GetInstallRequestStatus()
		//{
		//	if(_stepStatus == STEP_STATUS.Unknown)
		//	{
		//		if (EditorPrefs.GetBool(PREF_KEY_REQUEST_INSTALL_STEP1, false))
		//		{
		//			//Step1 요청
		//			_stepStatus = STEP_STATUS.Step1;
		//		}
		//		else if (EditorPrefs.GetBool(PREF_KEY_REQUEST_INSTALL_STEP2, false))
		//		{
		//			//Step2 요청
		//			_stepStatus = STEP_STATUS.Step2;
		//		}
		//		else
		//		{
		//			//요청이 없다.
		//			_stepStatus = STEP_STATUS.NoRequest;
		//		}
		//	}
		//	return _stepStatus;
		//}

		///// <summary>
		///// 설치 요청을 Dirty (Unknown) 상태로 만든다.
		///// </summary>
		//public void SetRequestDirty()
		//{
		//	_stepStatus = STEP_STATUS.Unknown;
		//}

		///// <summary>
		///// 모든 설치 요청을 비활성화한다.
		///// </summary>
		//public void ReleaseAllRequests()
		//{
		//	_stepStatus = STEP_STATUS.NoRequest;
		//	EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP1);//Step1 비활성
		//	EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP2);//Step2 비활성
		//}

		///// <summary>
		///// 설치를 요청한다. (Step1)
		///// </summary>
		//public void RequestInstall()
		//{
		//	_stepStatus = STEP_STATUS.Step1;

		//	EditorPrefs.SetBool(PREF_KEY_REQUEST_INSTALL_STEP1, true);//Step1 활성
		//	EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP2);//Step2 비활성
		//}

		// Functions
		//--------------------------------------------------------------------
#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern int Validate(int src1, float src2, ref Vector2 src3, ref apMatrix3x3 src4, ref Vector2[] src5Arr, int arrLength,
									ref int dst1, ref float dst2, ref Vector2 dst3, ref apMatrix3x3 dst4, ref Vector2[] dst5Arr, ref int dstVersion);

		
		/// <summary>현재 DLL을 호출할 수 있는지 체크한다. (자동 업데이트는 하지 않음)</summary>
		public VALIDATE_RESULT ValidateDLL()
		{
			//Import 도중에는 확인할 수 없다.
			if(_isImporting)
			{
				return VALIDATE_RESULT.Unknown;
			}

			if(EditorPrefs.GetBool(PREF_KEY_REQUEST_INSTALL_STEP1, false)
				|| EditorPrefs.GetBool(PREF_KEY_REQUEST_INSTALL_STEP2, false))
			{
				//설치 요청이 있다.
				return VALIDATE_RESULT.InstallationRequested;
			}

			_isInstallable = false;//ValidateDLL 함수가 호출된 이후부터는 설치가 불가능하다.


			if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows
				&& SystemInfo.operatingSystemFamily != OperatingSystemFamily.MacOSX)
			{
				//Debug.LogError("지원되지 않는 환경 : " + SystemInfo.operatingSystemFamily);
				return VALIDATE_RESULT.NotSupported;
			}


			//랜덤하게 값을 만든다.
			int src1 = UnityEngine.Random.Range(10, 50);
			float src2 = UnityEngine.Random.Range(10.0f, 20.0f);
			Vector2 src3 = new Vector2(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f));
			apMatrix3x3 src4 = apMatrix3x3.TRS(new Vector2(5.0f, 8.0f), 75.0f, new Vector2(1.5f, -2.0f));
			int dst1_CPP = 0;
			int dst1_CS = 0;
			float dst2_CPP = 0;
			float dst2_CS = 0;
			Vector2 dst3_CPP = Vector2.zero;
			Vector2 dst3_CS = Vector2.zero;
			apMatrix3x3 dst4_CPP = apMatrix3x3.identity;
			apMatrix3x3 dst4_CS = apMatrix3x3.identity;
			int dstVersion = -1;

			int arrLength = 20;
			Vector2[] src5Arr = new Vector2[arrLength];
			Vector2[] dst5Arr_CPP = new Vector2[arrLength];
			Vector2[] dst5Arr_CS = new Vector2[arrLength];

			for (int i = 0; i < arrLength; i++)
			{
				src5Arr[i] = new Vector2(2.0f * i, 1.5f * i);
				dst5Arr_CPP[i] = Vector2.zero;
				dst5Arr_CS[i] = Vector2.zero;
			}

			try
			{
				//C++ 함수 호출
				int result_CPP = Validate(src1, src2, ref src3, ref src4, ref src5Arr, arrLength,
											ref dst1_CPP, ref dst2_CPP, ref dst3_CPP, ref dst4_CPP, ref dst5Arr_CPP, ref dstVersion);

				//CS 코드 (C++와 동일)
				dst1_CS = (src1 - 49) * 88;
				dst2_CS = Mathf.Cos(src2 * Mathf.Deg2Rad) * 100.0f;
				dst3_CS.x = (src3.x * 7.6f) - (src3.y * 3.4f);
				dst3_CS.y = (src3.x * 5.35f) + (src3.y * -5.9f);
				dst4_CS.SetTRS(src3, src2, dst3_CS);
				dst4_CS.Multiply(ref src4);

				for (int i = 0; i < arrLength; i++)
				{
					dst5Arr_CS[i].x = src5Arr[i].x + 10.0f;
					dst5Arr_CS[i].y = src5Arr[i].y - 10.0f;
				}

				int result_CS = src1 + 19;

				//이제 C++과 CS가 같은지 보자
				float bias = 0.001f;//정확도는 0.001 정도로만 체크
				bool result1 = dst1_CPP == dst1_CS;
				bool result2 = Mathf.Abs(dst2_CPP - dst2_CS) < bias;
				bool result3 = Mathf.Abs(dst3_CPP.x - dst3_CS.x) < bias && Mathf.Abs(dst3_CPP.y - dst3_CS.y) < bias;
				bool result4 = Mathf.Abs(dst4_CPP._m00 - dst4_CS._m00) < bias
								&& Mathf.Abs(dst4_CPP._m01 - dst4_CS._m01) < bias
								&& Mathf.Abs(dst4_CPP._m02 - dst4_CS._m02) < bias
								&& Mathf.Abs(dst4_CPP._m10 - dst4_CS._m10) < bias
								&& Mathf.Abs(dst4_CPP._m11 - dst4_CS._m11) < bias
								&& Mathf.Abs(dst4_CPP._m12 - dst4_CS._m12) < bias
								&& Mathf.Abs(dst4_CPP._m20 - dst4_CS._m20) < bias
								&& Mathf.Abs(dst4_CPP._m21 - dst4_CS._m21) < bias
								&& Mathf.Abs(dst4_CPP._m22 - dst4_CS._m22) < bias;

				bool resultArr = true;
				for (int i = 0; i < arrLength; i++)
				{
					if (Mathf.Abs(dst5Arr_CPP[i].x - dst5Arr_CS[i].x) > bias
						|| Mathf.Abs(dst5Arr_CPP[i].y - dst5Arr_CS[i].y) > bias)
					{

						resultArr = false;
						//break;
					}
				}

				bool result5 = result_CPP == result_CS;


				//Debug.Log("Validate 결과  (CPP/CS 비교)");
				//Debug.Log("1 : " + dst1_CPP + " / " + dst1_CS);
				//Debug.Log("2 : " + dst2_CPP + " / " + dst2_CS);
				//Debug.Log("3 : " + dst3_CPP + " / " + dst3_CS);
				//Debug.Log("4 : " + dst4_CPP + " / " + dst4_CS);
				//Debug.Log("5 : " + result_CPP + " / " + result_CS);

				if (!result1 || !result2 || !result3 || !result4 || !result5 || !resultArr)
				{
					//값이 유효하지 않다.
					//Debug.LogError("값이 유효하지 않다 (" + result_CPP + ")");
					return VALIDATE_RESULT.InstalledButInvalid;
				}

				////배열 초기화 코드를 테스트하자
				//System.Array.Clear(dst5Arr_CS, 0, arrLength);//이걸 쓰면 되는걸..
				////Modifier_InitVectorArray(ref dst5Arr_CS, arrLength);
				//for (int i = 0; i < arrLength; i++)
				//{
				//	Debug.Log("> " + dst5Arr_CS[i]);
				//}

			}
			catch (DllNotFoundException)
			{
				//DLL이 없다. 설치되지 않음
				//Debug.LogError("설치되지 않음");

				//단 이 경우엔 유니티 에디터 재실행시 바로 업데이트가 되도록 EditorPref를 설정하자
				//EditorPrefs.SetBool("AnyPortrait_RequestInstallCPPDLL", true);//이게 강제가 되면 안된다. Mac에서는 설치 직후에 발견하지 못할 수 있기 때문

				//변경 > 자동 설치 안내는 하지 말고 수동으로 바꾸자
				return VALIDATE_RESULT.NotInstalled;
			}
			catch (Exception)
			{
				//그 외의 에러 : 유효하지 않음
				//Debug.LogError("알 수 없는 에러 : " + ex);
				return VALIDATE_RESULT.InstalledButInvalid;
			}

			if (apVersion.I.CPP_DLL_VERSION != dstVersion)
			{
				//설치는 되었지만 버전이 맞지 않는다.
				//단 이 경우엔 유니티 에디터 재실행시 바로 업데이트가 되도록 EditorPref를 설정하자
				//EditorPrefs.SetBool("AnyPortrait_RequestInstallCPPDLL", true);
				//Debug.Log("Set DLL");

				//변경 > 자동으로 설치 요청은 하지 말자

				return VALIDATE_RESULT.InstalledButOldVersion;
			}

			//유효성 통과
			//Debug.Log("유효성 테스트 통과");
			return VALIDATE_RESULT.Valid;
		}



		///// <summary>
		///// 바로 설치하기 위해 유니티 에디터를 재시작한다.
		///// </summary>
		//public void RequestInstallAndRestartUnityEditor(bool isRestartNow, apEditor linkedEditor)
		//{
		//	//다음에 에디터 실행시 설치를 요청하자
		//	EditorPrefs.SetBool("AnyPortrait_RequestInstallCPPDLL", true);

		//	if (isRestartNow)
		//	{
		//		//재시작을 하자
		//		AssetDatabase.SaveAssets();
				
		//		linkedEditor.Close();
		//		EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
		//	}
			
		//}


		public void RequestInstall()
		{	
			EditorPrefs.SetBool(PREF_KEY_REQUEST_INSTALL_STEP1, true);//Step1 활성
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP2);//Step2 비활성
			double editorTime = EditorApplication.timeSinceStartup;

			int requestTime = 0;
			if(editorTime > MAX_INSTALL_REQUEST_TIME_SEC)
			{
				requestTime = MAX_INSTALL_REQUEST_TIME_SEC;
			}
			else
			{
				requestTime = (int)editorTime;
			}
			EditorPrefs.SetInt(PREF_KEY_REQUEST_INSTALL_TIME, requestTime);//설치 요청 시간을 저장한다.
		}

		public void ReleaseAllInstallRequests()
		{
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP1);//Step1 비활성
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP2);//Step2 비활성
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_TIME);//설치 요청 시간 삭제
		}


		/// <summary>
		/// 플러그인 설치 요청이 있는지 확인하고 패키지를 열어서 설치한다.
		/// </summary>
		public void CheckAndInstallCPPDLLPackage()
		{
			try
			{
				//설치 요청(Step2)가 있어야 한다. (필수)
				bool isInstallRequest = EditorPrefs.GetBool(PREF_KEY_REQUEST_INSTALL_STEP2, false);
				if(!isInstallRequest)
				{
					//설치 요청이 없다.
					return;
				}

				if(!_isInstallable)
				{
					//이미 Validate 함수가 호출되어서 설치가 불가능하다.
					return;
				}
				//if (!isForce)
				//{
				//	bool isRequest = EditorPrefs.GetBool("AnyPortrait_RequestInstallCPPDLL", false);

				//	//Debug.LogError("C++ DLL Inatallable : " + _isInstallable + " / Is Request " + isRequest);
				//	if (!_isInstallable || !isRequest)
				//	{
				//		return;
				//	}
				//}

				_isInstallable = false;

				//Debug.Log("Start Install C++ DLL");

				//설치가 시작되면 모든 요청을 초기화한다.
				EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP1);
				EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP2);
				EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_TIME);//설치 요청 시간 삭제
				
				string packagePath = null;
				if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
				{
					packagePath = apPathSetting.I.CurrentPath + "Editor/Packages/" + PACKAGE_NAME_WIN64 + ".unitypackage";
					//테스트
					//packagePath = apPathSetting.I.CurrentPath + "Editor/Packages/Accelerated Mode Plugin Win64 (2).unitypackage";
				}
				else if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
				{
					packagePath = apPathSetting.I.CurrentPath + "Editor/Packages/" + PACKAGE_NAME_MACOS + ".unitypackage";
				}
				else
				{
					//실패
					DisplayDialog_Failed();
					return;
				}

				//업데이트 안내 다이얼로그를 보여주자
				if(!DisplayDialog_Info())
				{
					//취소 > 나중에 설치하기
					return;
				}

				//이거 동작하지 않는다.... 뭐여
				AssetDatabase.importPackageCompleted += OnDLLPackageImported_Completed;
				AssetDatabase.importPackageFailed += OnDLLPackageImported_Failed;
				AssetDatabase.importPackageCancelled += OnDLLPackageImported_Cancelled;

				_isImporting = true;//Import 시작

				AssetDatabase.ImportPackage(packagePath, false);
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();

				//이게 바로 동작하면 안된다.
				//복사 직전에 이 스크립트가 동작해버리므로 (블록이 안됨)
				//오히려 Validate가 복사 직전에 호출되어서 에러를 일으킨다.
				//패키지 Import 시작 > Validate가 수행 (이전 버전 DLL)로 > 이미 DLL 함수가 호출되었으므로 복사 실패 > Import 종료

				//VALIDATE_RESULT result = ValidateDLL();
				//if(result == VALIDATE_RESULT.Valid)
				//{
				//	//성공!
				//	DisplayDialog_Installed(true);
				//}
				//else
				//{
				//	//설치했지만 실패..
				//	DisplayDialog_Installed(false);

				//	//CPP 사용 옵션 초기화
				//	EditorPrefs.DeleteKey("AnyPortrait_UseCPPPlugin");
				//}
				//AssetDatabase.importPackageCompleted -= OnDLLPackageImported_Completed;
				//AssetDatabase.importPackageFailed -= OnDLLPackageImported_Failed;
				//AssetDatabase.importPackageCancelled -= OnDLLPackageImported_Cancelled;
			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : Import Package Failed.\n" + ex.ToString());

				//실패 메시지
				DisplayDialog_Failed();

				//CPP 사용 옵션 초기화 (이건 자동)
				EditorPrefs.DeleteKey("AnyPortrait_UseCPPPlugin");
			}
			
			//설치 후에는 키 제거 (위에서 했지만 한번더)
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP1);
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_STEP2);
			EditorPrefs.DeleteKey(PREF_KEY_REQUEST_INSTALL_TIME);//설치 요청 시간 삭제
		}

		private void OnDLLPackageImported_Completed(string packageName)
		{
			//Debug.Log("Package Imported : " + packageName);

			//Import 종료
			_isImporting = false;

			AssetDatabase.importPackageCompleted -= OnDLLPackageImported_Completed;
			AssetDatabase.importPackageFailed -= OnDLLPackageImported_Failed;
			AssetDatabase.importPackageCancelled -= OnDLLPackageImported_Cancelled;


			if (string.Equals(packageName, PACKAGE_NAME_WIN64)
				|| string.Equals(packageName, PACKAGE_NAME_MACOS))
			{
				VALIDATE_RESULT result = ValidateDLL();
				DisplayDialog_Installed(result);
				
				//이건 사용하지 말자
				////CPP 사용 옵션 초기화
				//EditorPrefs.DeleteKey("AnyPortrait_UseCPPPlugin");
			}
			
		}

		private void OnDLLPackageImported_Cancelled(string packageName)
		{
			//Debug.Log("Package Import Canclled : " + packageName);

			//Import 종료
			_isImporting = false;

			AssetDatabase.importPackageCompleted -= OnDLLPackageImported_Completed;
			AssetDatabase.importPackageFailed -= OnDLLPackageImported_Failed;
			AssetDatabase.importPackageCancelled -= OnDLLPackageImported_Cancelled;

			if (string.Equals(packageName, PACKAGE_NAME_WIN64)
				|| string.Equals(packageName, PACKAGE_NAME_MACOS))
			{
				//실패 메시지
				DisplayDialog_Failed();

				//CPP 사용 옵션 초기화
				EditorPrefs.DeleteKey("AnyPortrait_UseCPPPlugin");
			}
			
		}

		private void OnDLLPackageImported_Failed(string packageName, string error)
		{
			//Debug.Log("Package Import Failed : " + packageName + " > " + error);

			//Import 종료
			_isImporting = false;

			AssetDatabase.importPackageCompleted -= OnDLLPackageImported_Completed;
			AssetDatabase.importPackageFailed -= OnDLLPackageImported_Failed;
			AssetDatabase.importPackageCancelled -= OnDLLPackageImported_Cancelled;

			if (string.Equals(packageName, PACKAGE_NAME_WIN64)
				|| string.Equals(packageName, PACKAGE_NAME_MACOS))
			{
				//실패 메시지
				DisplayDialog_Failed();

				//CPP 사용 옵션 초기화
				EditorPrefs.DeleteKey("AnyPortrait_UseCPPPlugin");
			}
		}

		//public void ReleaseInstallRequest()
		//{
		//	//설치 요청을 삭제한다.
		//	//Debug.Log("Release Install Request");
		//	EditorPrefs.DeleteKey("AnyPortrait_RequestInstallCPPDLL");
		//}


		// 다이얼로그 보여주기
		//-----------------------------------------------------------------
		private bool DisplayDialog_Info()
		{
			apEditor.LANGUAGE language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
			string strTitle = null;
			string strBody = null;
			string strOkay = null;
			string strCancel = null;
			switch (language)
			{
				case apEditor.LANGUAGE.English:
					strTitle = "Install Plugin";
					strBody = "The new version of the [Accelerated mode plugin] will be installed.";
					strOkay = "Install Now";
					strCancel = "Install Later";
					break;

				case apEditor.LANGUAGE.Korean:
					strTitle = "플러그인 설치";
					strBody = "새로운 버전의 [가속 모드 플러그인]이 설치됩니다.";
					strOkay = "지금 설치하기";
					strCancel = "나중에 설치하기";
					break;

				case apEditor.LANGUAGE.French:
					strTitle = "Installer le plugin";
					strBody = "La nouvelle version du [plug-in de mode accéléré] sera installée.";
					strOkay = "Installer maintenant";
					strCancel = "Installer plus tard";
					break;

				case apEditor.LANGUAGE.German:
					strTitle = "PIugin installieren";
					strBody = "Die neue Version des [Beschleunigungsmodus-Plugin] wird installiert.";
					strOkay = "Jetzt installieren";
					strCancel = "Später installieren";
					break;

				case apEditor.LANGUAGE.Spanish:
					strTitle = "Instalar complemento";
					strBody = "Se instalará la nueva versión del [complemento de modo acelerado].";
					strOkay = "Instalar ahora";
					strCancel = "Instalar despues";
					break;

				case apEditor.LANGUAGE.Italian:
					strTitle = "Installa plugin";
					strBody = "Verrà installata la nuova versione del [plug-in Modalità accelerata].";
					strOkay = "Installa ora";
					strCancel = "Installa dopo";
					break;

				case apEditor.LANGUAGE.Danish:
					strTitle = "Installer plugin";
					strBody = "Den nye version af [Accelerated mode plugin] installeres.";
					strOkay = "Installer nu";
					strCancel = "Installer senere";
					break;

				case apEditor.LANGUAGE.Japanese:
					strTitle = "インストール";
					strBody = "新しいバージョンの「加速モード プラグイン」がインストールされます。";
					strOkay = "今すぐインストール";
					strCancel = "後でインストール";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					strTitle = "安裝插件";
					strBody = "安裝了新版本的加速模式插件。";
					strOkay = "現在安裝";
					strCancel = "稍後安裝";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					strTitle = "安装插件";
					strBody = "安装了新版本的加速模式插件。";
					strOkay = "现在安装";
					strCancel = "稍后安装";
					break;

				case apEditor.LANGUAGE.Polish:
					strTitle = "Zainstaluj wtyczkę";
					strBody = "Nowa wersja [Wtyczki trybu przyspieszonego] zostanie zainstalowana.";
					strOkay = "Zainstaluj teraz";
					strCancel = "Zainstaluj później";
					break;
			}


			return EditorUtility.DisplayDialog(strTitle, strBody, strOkay, strCancel);
		}


		// 설치 이후의 결과 안내 다이얼로그. 
		//설치 이후 다시 Validate를 시도하여 결과를 알려준다.
		private void DisplayDialog_Installed(VALIDATE_RESULT validateResult)
		{

			apEditor.LANGUAGE language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
			string strTitle = null;
			string strBody = null;
			string strClose = null;
			switch (language)
			{
				case apEditor.LANGUAGE.English:
					{
						strTitle = "Install Plugin";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "The plugin is installed.\nAccelerated mode is now supported.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "The plugin is installed, but it cannot be used yet.\nAfter restarting the Unity Editor, please check the AnyPortrait editor settings.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "The plugin is installed, but Accelerated mode is not supported in the current environment.";
								break;
						}					
						strClose = "Okay";
					}					
					break;
				case apEditor.LANGUAGE.Korean:
					{
						strTitle = "플러그인 설치";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "플러그인이 설치되었습니다.\n이제 가속 모드가 지원됩니다.";
								break;
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "플러그인이 설치되었지만, 아직 사용될 수 없습니다.\n유니티 에디터를 다시 실행한 이후 AnyPortrait 에디터의 설정을 확인해주세요.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "플러그인이 설치되었지만 가속 모드가 지원되지 않는 환경입니다.";
								break;
						}
						strClose = "확인";
					}
					break;
				case apEditor.LANGUAGE.French:
					{
						strTitle = "Installer le plugin";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "Le plugin est installé.\nLe mode accéléré est désormais pris en charge.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "Le plugin est installé, mais il ne peut pas encore être utilisé.\nAprès avoir redémarré l'éditeur Unity, veuillez vérifier les paramètres de l'éditeur AnyPortrait.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "Le plugin est installé, mais le mode accéléré n'est pas pris en charge dans l'environnement actuel.";
								break;
						}
						strClose = "Oui";
					}
					break;

				case apEditor.LANGUAGE.German:
					{
						strTitle = "PIugin installieren";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "Das Plugin ist installiert.\nDer beschleunigte Modus wird jetzt unterstützt.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "Das Plugin ist installiert, kann aber noch nicht verwendet werden.\nNach dem Neustart des Unity-Editors überprüfen Sie bitte die Einstellungen des AnyPortrait-Editors.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "Das Plugin ist installiert, aber der beschleunigte Modus wird in der aktuellen Umgebung nicht unterstützt.";
								break;
						}
						strClose = "Okay";
					}
					break;

				case apEditor.LANGUAGE.Spanish:
					{
						strTitle = "Instalar complemento";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "El complemento está instalado.\nAhora se admite el modo acelerado.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "El complemento está instalado, pero aún no se puede utilizar.\nDespués de reiniciar el editor de Unity, verifique la configuración del editor de AnyPortrait.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "El complemento está instalado, pero el modo acelerado no es compatible con el entorno actual.";
								break;
						}
						strClose = "Correcto";
					}
					break;

				case apEditor.LANGUAGE.Italian:
					{
						strTitle = "Installa plugin";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "Il plugin è installato.\nLa modalità accelerata è ora supportata.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "Il plugin è installato, ma non può essere ancora utilizzato.\nDopo aver riavviato l'editor Unity, controlla le impostazioni dell'editor AnyPortrait.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "Il plugin è installato, ma la modalità accelerata non è supportata nell'ambiente corrente.";
								break;
						}
						strClose = "Va bene";
					}
					break;

				case apEditor.LANGUAGE.Danish:
					{
						strTitle = "Installer plugin";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "Pluginet er installeret. \nAccelereret tilstand understøttes nu.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "Pluginet er installeret, men det kan ikke bruges endnu.\nEfter genstart af Unity Editor skal du kontrollere AnyPortrait-editorens indstillinger.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "Pluginet er installeret, men accelereret tilstand understøttes ikke i det nuværende miljø.";
								break;
						}
						strClose = "Okay";
					}
					break;

				case apEditor.LANGUAGE.Japanese:
					{
						strTitle = "インストール";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "プラグインがインストールされます。\n加速モードがサポートされるようになりました。";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "プラグインはインストールされていますが、まだ使用できません。 \nUnity エディターを再起動した後、AnyPortrait エディターの設定を確認してください。";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "プラグインはインストールされていますが、現在の環境では加速モードがサポートされていません。";
								break;
						}
						strClose = "はい";
					}
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					{
						strTitle = "安裝插件";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "插件已安裝。\n現在支持加速模式。";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "該插件已安裝，但還不能使用。\n重新啟動 Unity 編輯器後，請檢查 AnyPortrait 編輯器設置。";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "插件已安裝，但當前環境不支持加速模式。";
								break;
						}
						strClose = "確認";
					}
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					{
						strTitle = "安装插件";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "插件已安装。\n现在支持加速模式。";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "该插件已安装，但还不能使用。\n重新启动 Unity 编辑器后，请检查 AnyPortrait 编辑器设置。";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "插件已安装，但当前环境不支持加速模式。";
								break;
						}
						strClose = "确认";
					}
					break;

				case apEditor.LANGUAGE.Polish:
					{
						strTitle = "Zainstaluj wtyczkę";
						switch (validateResult)
						{		
							case VALIDATE_RESULT.Valid:	//설치 및 가속 모드 성공
								strBody = "Wtyczka jest zainstalowana.\nTryb przyspieszony jest teraz obsługiwany.";
								break;							
								
							case VALIDATE_RESULT.NotInstalled:	//Mac의 경우엔 설치 직후 로드가 되지 않을 수 있다.
								strBody = "Wtyczka jest zainstalowana, ale nie można jej jeszcze używać.\nPo ponownym uruchomieniu Unity Editor sprawdź ustawienia edytora AnyPortrait.";
								break;
								
							default:	//설치가 되었지만 가속 모드가 지원되지 않는다.
								strBody = "Wtyczka jest zainstalowana, ale tryb przyspieszony nie jest obsługiwany w bieżącym środowisku.";
								break;
						}
						strClose = "Tak";
					}
					break;
			}

			EditorUtility.DisplayDialog(strTitle, strBody, strClose);
		}

		private void DisplayDialog_Failed()
		{
			apEditor.LANGUAGE language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
			string strTitle = null;
			string strBody = null;
			string strClose = null;
			switch (language)
			{
				case apEditor.LANGUAGE.English:
					strTitle = "Install Plugin";
					strBody = "Installation failed.";
					strClose = "Okay";
					break;
				case apEditor.LANGUAGE.Korean:
					strTitle = "플러그인 설치";
					strBody = "플러그인 설치에 실패했습니다.";
					strClose = "확인";
					break;
				case apEditor.LANGUAGE.French:
					strTitle = "Installer le plugin";
					strBody = "L'installation a échoué.";
					strClose = "Oui";
					break;
				case apEditor.LANGUAGE.German:
					strTitle = "PIugin installieren";
					strBody = "Installation fehlgeschlagen.";
					strClose = "Okay";
					break;
				case apEditor.LANGUAGE.Spanish:
					strTitle = "Instalar complemento";
					strBody = "Instalación fallida.";
					strClose = "Correcto";
					break;
				case apEditor.LANGUAGE.Italian:
					strTitle = "Installa plugin";
					strBody = "Installazione fallita.";
					strClose = "Va bene";
					break;
				case apEditor.LANGUAGE.Danish:
					strTitle = "Installer plugin";
					strBody = "Installationen mislykkedes.";
					strClose = "Okay";
					break;
				case apEditor.LANGUAGE.Japanese:
					strTitle = "インストール";
					strBody = "インストールに失敗しました。";
					strClose = "はい";
					break;
				case apEditor.LANGUAGE.Chinese_Traditional:
					strTitle = "安裝插件";
					strBody = "安裝失敗。";
					strClose = "確認";
					break;
				case apEditor.LANGUAGE.Chinese_Simplified:
					strTitle = "安装插件";
					strBody = "安装失败。";
					strClose = "确认";
					break;
				case apEditor.LANGUAGE.Polish:
					strTitle = "Zainstaluj wtyczkę";
					strBody = "Instalacja nie powiodła się.";
					strClose = "Tak";
					break;
			}

			EditorUtility.DisplayDialog(strTitle, strBody, strClose);
		}


	}

	
}