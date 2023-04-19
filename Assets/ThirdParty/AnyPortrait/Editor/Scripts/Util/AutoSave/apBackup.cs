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
//using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AnyPortrait;



namespace AnyPortrait
{
	
	/// <summary>
	/// 백업을 하는 클래스
	/// </summary>
	public class apBackup
	{
		// Members
		//--------------------------------------------------------------------
		private apBackupTable _table = new apBackupTable();

		// 자동 저장용 변수
		
		private bool _isAutoSaveBakeWorking = false;
		private apPortrait _autoSave_TargetPortrait = null;
		private string _autoSave_PortraitName = "";
		private string _autoSave_FilePath = "";

		private bool _isRequestAutoSaveStop = false;//<<강제로 종료하는 요청
		//private bool _isCheckTimeInit = false;
		//private DateTime _checkTime = new DateTime();
		//private DateTime _checkTime_InputIdle = new DateTime();//입력이 없는 상태
		//private apPortrait _prevWorkedPortrait = null;

		private string _processingLabel = "";
		
		//private bool _isReadyAutoSave = false;

		private enum ASYNC_SAVE_STEP
		{
			Step0_Ready,
			Step1_SerializeUnit,
			Step2_FileCreate,
			Step3_FileWrite,
			Step4_FileEnd,
		}
		private ASYNC_SAVE_STEP _async_Step = ASYNC_SAVE_STEP.Step0_Ready;
		private apBackupUnit _async_RootUnit = null;
		private FileStream _async_FileStream = null;
		private StreamWriter _async_StreamWriter = null;
		private int _async_SubProcessIndex = 0;
		private bool _async_FirstFrame = true;
		private System.DateTime _async_Timer = new System.DateTime();

		

		

		public bool IsAutoSaveWorking()
		{
			return _isAutoSaveBakeWorking;
		}
		public string Label
		{
			get
			{
				return _processingLabel + "[" + Mathf.Clamp((int)_async_Step + 1, 1, 4) + " / 4]";
			}
		}



		/// <summary>
		/// Auto Save를 중지한다.
		/// </summary>
		public void StopAutoSave()
		{
			
			_isRequestAutoSaveStop = true;
		}



		private class AsyncSerializeRequest
		{
			public object _targetObj = null;
			public apBackupUnit _parentInstance = null;
			public int _level = -1;

			public AsyncSerializeRequest(object targetObj, apBackupUnit parentInstance, int level)
			{
				_targetObj = targetObj;
				_parentInstance = parentInstance;
				_level = level;
			}
		}


		private List<AsyncSerializeRequest> _nextSerializeRequest = new List<AsyncSerializeRequest>();
		
		private class AsyncFileWriteRequest
		{
			public apBackupUnit _backupUnit;
			public int _level;

			public AsyncFileWriteRequest(apBackupUnit backupUnit, int level)
			{
				_backupUnit = backupUnit;
				_level = level;
			}
		}

		private List<AsyncFileWriteRequest> _nextFileWriteRequest = new List<AsyncFileWriteRequest>();
		private int _nFileWrite = 0;

		private apBackupTimer _timer = new apBackupTimer();
		private System.DateTime _dateTime_Prev = System.DateTime.Now;

		// Init
		//------------------------------------------------------------------
		public apBackup()
		{
			_isAutoSaveBakeWorking = false;
			_autoSave_TargetPortrait = null;
			_autoSave_FilePath = "";

			_isRequestAutoSaveStop = false;//<<강제로 종료하는 요청
			//_isCheckTimeInit = false;
			//_checkTime = DateTime.Now;
			//_checkTime_InputIdle = DateTime.Now;
			//_prevWorkedPortrait = null;


			//_isReadyAutoSave = false;
			_timer.Clear();
			
		}

		
		

		

		//------------------------------------------------------------------------------------------
		// 비동기 저장
		// 에디터에서 실행하는 함수(CheckAutoBackup)가 있고
		// 싱글톤에서 유니티 객체로부터 호출되는 함수가 하나있다.
		// 이건 apEditor에서 필요로 하는 경우 Hide된 GameObject를 임시로 만들어서 Update로 수행한다.<<- 이거 만들어야 함
		//------------------------------------------------------------------------------------------
		/// <summary>
		/// 에디터에서 계속 호출할 함수
		/// 자동 백업중이라면 백업을 계속 실시하고, 그렇지 않다면 시간과 작업 상태를 체크하여 백업 할지 여부글 결정한다.
		/// </summary>
		/// <param name="editor"></param>
		public void CheckAutoBackup(apEditor editor, EventType eventType)
		{
			try
			{
				if (_isAutoSaveBakeWorking)
				{
					//1. Auto Save 중이라면
					//_prevWorkedPortrait = null;

					bool isEnd = AutoSaveProcess();

					if(isEnd)
					{
						_isAutoSaveBakeWorking = false;
						_autoSave_TargetPortrait = null;
						_autoSave_FilePath = "";

						_isRequestAutoSaveStop = false;//<<강제로 종료하는 요청
						//_isCheckTimeInit = false;
						//_checkTime = DateTime.Now;
						//_checkTime_InputIdle = DateTime.Now;
						//_prevWorkedPortrait = null;
						//_isReadyAutoSave = false;

						_async_Step = ASYNC_SAVE_STEP.Step0_Ready;
						_async_RootUnit = null;

						if(_async_StreamWriter != null)
						{
							_async_StreamWriter.Close();
							_async_StreamWriter = null;
						}

						if(_async_FileStream != null)
						{
							_async_FileStream.Close();
							_async_FileStream = null;
						}
						_async_FileStream = null;
						_async_StreamWriter = null;
						_async_SubProcessIndex = 0;
						_async_FirstFrame = true;
						_async_Timer = System.DateTime.Now;


						_dateTime_Prev = System.DateTime.Now;
					}

				}
				else
				{
					//2. 시간을 체크하는 중이라면
					if (!editor._backupOption_IsAutoSave || editor._portrait == null || EditorWindow.focusedWindow != editor)
					{
						return;
					}

					float deltaTime = (float)System.DateTime.Now.Subtract(_dateTime_Prev).TotalSeconds;
					if(deltaTime < 20.0f)
					{
						//기본 업데이트 단위는 20초.
						return;
					}


					UnityEngine.SceneManagement.Scene curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
					if(string.IsNullOrEmpty(curScene.name))
					{
						return;
					}

					//일단 체크는 해볼 필요가 생겼다.
					_dateTime_Prev = System.DateTime.Now;

					string fileFolderPath = Application.dataPath + "/../" + editor._backupOption_BaseFolderName;
					bool isUpdatable = _timer.Update(deltaTime, fileFolderPath, editor._portrait, curScene.name);

					if(!isUpdatable)
					{
						return;
					}


					//백업 후 경과된 시간을 가져온다.
					int portraitBackupTime = _timer.GetPortraitBackupTime(curScene.name, editor._portrait);

					if (portraitBackupTime > editor._backupOption_Minute)
					{
						//저장을 해야한다.
						StartAutoSaveBackup(editor._portrait, Application.dataPath, editor._backupOption_BaseFolderName, curScene.name);

						//타이머를 리셋하고 저장을 한다.
						_timer.ResetTimerAndSave(curScene.name, editor._portrait, fileFolderPath);
					}


					
				}
				
			}
			catch(System.Exception ex)
			{
				Debug.LogError("AutoSave Check Exception : " + ex);
			}
		}

		

		// 비동기 저장 시작하기
		public bool StartAutoSaveBackup(apPortrait portriat, string dataPath, string backupFolderPath, string sceneName)
		{
			if(_isAutoSaveBakeWorking || portriat == null)
			{
				return false;
			}


			//_autoSave_TargetPortrait = portriat;
			GameObject dummyGameObject = UnityEngine.Object.Instantiate<GameObject>(portriat.gameObject);
			dummyGameObject.name += "__autoback";
			dummyGameObject.hideFlags = HideFlags.HideAndDontSave;//<<나중에 이걸 붙이자
			_autoSave_TargetPortrait = dummyGameObject.GetComponent<apPortrait>();
			_autoSave_PortraitName = portriat.name;
			if(string.IsNullOrEmpty(_autoSave_PortraitName))
			{
				_autoSave_PortraitName = "NonamePortrait";
			}
			
			//Root Folder -> Portrait Name -> Scene 순으로 폴더를 나눈다.
			string autoSaveRootFolder = dataPath + "/../" + backupFolderPath;
			string autoSaveSub1Folder = autoSaveRootFolder + "/" + _autoSave_PortraitName;
			string autoSaveSub2Folder = autoSaveSub1Folder + "/" + sceneName;


			string defaultBackupFileName = _autoSave_PortraitName + "_autoback_"
								+ GetCurrentTimeString();

			
			//폴더가 없으면 만들어주자
			DirectoryInfo di_root = new DirectoryInfo(autoSaveRootFolder);
			if(!di_root.Exists)
			{
				di_root.Create();
			}

			DirectoryInfo di_sub1 = new DirectoryInfo(autoSaveSub1Folder);
			if(!di_sub1.Exists)
			{
				di_sub1.Create();
			}

			DirectoryInfo di_sub2 = new DirectoryInfo(autoSaveSub2Folder);
			if(!di_sub2.Exists)
			{
				di_sub2.Create();
			}

			//파일 개수가 너무 많다면 삭제
			FileInfo[] txtFileInfos = di_sub2.GetFiles("*.bck");
			List<FileInfo> autobackFiles = new List<FileInfo>();
			for (int i = 0; i < txtFileInfos.Length; i++)
			{
				if(txtFileInfos[i].Name.Contains("_autoback_"))
				{
					autobackFiles.Add(txtFileInfos[i]);
				}
			}
			
			
			if(autobackFiles.Count > 10)
			{
				//Debug.Log("Too Many Autoback Files");
				//정렬 순서가 맞는지 테스트하자
				int nDelete = autobackFiles.Count - 10;
				autobackFiles.Sort(delegate (FileInfo a, FileInfo b)
				{
					return (int)(a.LastWriteTime.Subtract(b.LastWriteTime).TotalMinutes);
				});
				//10개가 넘으면 그 이상의 것은 지운다.
				
				for (int i = 0; i < nDelete; i++)
				{
					autobackFiles[i].Delete();
				}
			}

			_autoSave_FilePath = autoSaveSub2Folder + "/" + defaultBackupFileName + ".bck";
			_isAutoSaveBakeWorking = true;
			//_isReadyAutoSave = false;
			_isRequestAutoSaveStop = false;
			_processingLabel = "[" + portriat.name + "] is being saved..";
			//Debug.Log("Label : " + _processingLabel);

			_async_Step = ASYNC_SAVE_STEP.Step0_Ready;
			_async_RootUnit = null;

			if(_async_StreamWriter != null)
			{
				_async_StreamWriter.Close();
			}

			if(_async_FileStream != null)
			{
				_async_FileStream.Close();
			}

			

			_async_FileStream = null;
			_async_StreamWriter = null;

			_async_SubProcessIndex = 0;
			_async_FirstFrame = true;

			_async_Timer = System.DateTime.Now;

			return true;
		}
		

		/// <summary>
		/// Editor가 꺼지는 경우
		/// 강제로 종료할 때.
		/// 그외에는 Stop AutoSave를 호출하자
		/// </summary>
		public void StopForce()
		{
			if(_isAutoSaveBakeWorking)
			{
				//자동 저장 중이었다면
				//마무리 저장을 아예 하자
				//Debug.LogError("자동 저장중에 Edtior가 종료되었다. 강제로 저장 진행");
				AutoSaveProcess(false);
			}


			if(_autoSave_TargetPortrait != null && _autoSave_TargetPortrait.gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(_autoSave_TargetPortrait.gameObject);
				_autoSave_TargetPortrait = null;
			}
			_isAutoSaveBakeWorking = false;
			_autoSave_TargetPortrait = null;
			_autoSave_FilePath = "";

			_isRequestAutoSaveStop = false;//<<강제로 종료하는 요청

			_async_Step = ASYNC_SAVE_STEP.Step0_Ready;
			_async_RootUnit = null;

			if(_async_StreamWriter != null)
			{
				_async_StreamWriter.Close();
				_async_StreamWriter = null;
			}

			if(_async_FileStream != null)
			{
				_async_FileStream.Close();
				_async_FileStream = null;
			}
			_async_FileStream = null;
			_async_StreamWriter = null;
			_async_SubProcessIndex = 0;
			_async_FirstFrame = true;
			_async_Timer = System.DateTime.Now;

		}


		/// <summary>
		/// AutoSave를 진행한다.
		/// 진행이 끝났다면 true 리턴 (성공이든 실패이든)
		/// </summary>
		/// <returns></returns>
		private bool AutoSaveProcess(bool isAsyncProcess = true)
		{
			try
			{
				if(_autoSave_TargetPortrait == null || _isRequestAutoSaveStop)
				{
					//정지해야하거나 정지 요청이 온 경우
					if (_async_StreamWriter != null)
					{
						_async_StreamWriter.Close();
						_async_StreamWriter = null;
					}

					if (_async_FileStream != null)
					{
						_async_FileStream.Close();
						_async_FileStream = null;
					}

					return true;
				}
				float tDelta = (float)(System.DateTime.Now.Subtract(_async_Timer).TotalSeconds);

				while (true)
				{
					//동기 방식이면 이 loop가 끝날때까지 처리한다.
					if(!isAsyncProcess)
					{
						//동기 방식이면 시간을 강제로 주어서 처리함
						tDelta = 1000.0f;
					}

					switch (_async_Step)
					{
						case ASYNC_SAVE_STEP.Step0_Ready:
							{
								//0. Ready 단계
								//잠깐 쉬었다가 시작한다.
								if (_async_FirstFrame)
								{
									//Debug.Log("<< Async : 0. Ready >>");
									_async_FirstFrame = false;
									_async_SubProcessIndex = 0;

									//추가. 타입/변수 테이블을 만들어서 데이터를 줄이자.
									_table.Clear();
								}
								if (tDelta > 2.0f)
								{
									_async_Timer = System.DateTime.Now;
									_async_SubProcessIndex++;

									if (_async_SubProcessIndex >= 1)
									{
										ChangeAsyncStep(ASYNC_SAVE_STEP.Step1_SerializeUnit);
									}
								}
							}
							break;
						case ASYNC_SAVE_STEP.Step1_SerializeUnit:
							{
								//1. SerializeUnit
								//Serialize 함수를 실행하여 BackUp Unit을 완성한다.
								if (_async_FirstFrame)
								{
									//Debug.Log("<< Async : 1. Serialize Unit >>");
									_async_FirstFrame = false;
									_async_SubProcessIndex = 0;

									//Root 생성
									_async_RootUnit = new apBackupUnit();
									_async_RootUnit.SetRoot();


									//시작 요청은 Root 데이터를 넣는다.
									_nextSerializeRequest.Clear();
									_nextSerializeRequest.Add(new AsyncSerializeRequest(_autoSave_TargetPortrait, _async_RootUnit, 0));


									//UnityEngine.Profiling.Profiler.BeginSample("Backup - 1.Serialize");
									//Serialize(_autoSave_TargetPortrait, _async_RootUnit, 0);
									//UnityEngine.Profiling.Profiler.EndSample();

								}
								if (tDelta > 0.05f)
								{
									_async_Timer = System.DateTime.Now;
									_async_SubProcessIndex++;

									int curRequests = _nextSerializeRequest.Count;
									//Serialize를 일부분씩 실행한다.
									//한번에 처리할 개수는..
									//일단 절반 이상 처리를 하는게 맞지 않을까
									int nProcess = Mathf.Clamp(_nextSerializeRequest.Count, 100, 300);


									for (int iProcess = 0; iProcess < nProcess; iProcess++)
									{
										//_resultSerializeRequest.Clear();
										AsyncSerializeRequest popRequest = _nextSerializeRequest[0];
										_nextSerializeRequest.RemoveAt(0);

										List<AsyncSerializeRequest> result = AsyncSerialize(popRequest);
										if (result != null)
										{
											for (int i = 0; i < result.Count; i++)
											{
												_nextSerializeRequest.Add(result[i]);
											}
										}
										if (_nextSerializeRequest.Count == 0)
										{
											break;
										}
									}

									//if (_async_SubProcessIndex % 100 == 0)
									//{
									//	Debug.Log("Async Serialize [" + _async_SubProcessIndex + "] : Request : " + curRequests + " >> " + _nextSerializeRequest.Count);
									//}

									if (_nextSerializeRequest.Count == 0)
									{
										//Debug.LogError("Async Serialize End [Loop " + _async_SubProcessIndex + "]");
										ChangeAsyncStep(ASYNC_SAVE_STEP.Step2_FileCreate);
									}

								}
							}
							break;
						case ASYNC_SAVE_STEP.Step2_FileCreate:
							{
								//2. FileCreate
								// FileStream을 이용하여 txt 파일을 생성한다.
								//바로 넘어간다.
								if (_async_FirstFrame)
								{
									//Debug.Log("<< Async : 2. File Create >>");
									_async_FirstFrame = false;
									_async_SubProcessIndex = 0;
								}
								if (tDelta > 0.5f)
								{
									_async_Timer = System.DateTime.Now;
									_async_SubProcessIndex++;

									if (_async_SubProcessIndex >= 1)
									{
										//이전
										//_async_FileStream = new FileStream(_autoSave_FilePath, FileMode.Create, FileAccess.Write);
										//_async_StreamWriter = new StreamWriter(_async_FileStream);

										//변경 21.7.3 : 경로 + 인코딩 문제
										_async_FileStream = new FileStream(apUtil.ConvertEscapeToPlainText(_autoSave_FilePath), FileMode.Create, FileAccess.Write);
										_async_StreamWriter = new StreamWriter(_async_FileStream, System.Text.Encoding.UTF8);



										_async_StreamWriter.WriteLine(apVersion.I.APP_VERSION_INT + "");
										_async_StreamWriter.WriteLine(_autoSave_PortraitName);
										_async_StreamWriter.WriteLine(System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "_" + System.DateTime.Now.Hour.ToString() + "_" + System.DateTime.Now.Minute.ToString());

										//테이블을 작성하자
										_table.FileWrite(_async_StreamWriter);

										ChangeAsyncStep(ASYNC_SAVE_STEP.Step3_FileWrite);
									}
								}
							}
							break;
						case ASYNC_SAVE_STEP.Step3_FileWrite:
							{
								//3. File Write
								//Bakeup Unit의 정보를 텍스트 파일로 저장한다.
								if (_async_FirstFrame)
								{
									//Debug.Log("<< Async : 3. File Write >>");
									_async_FirstFrame = false;
									_async_SubProcessIndex = 0;

									//시작 요청은 RootUnit
									_nextFileWriteRequest.Clear();
									_nextFileWriteRequest.Add(new AsyncFileWriteRequest(_async_RootUnit, 0));
									_nFileWrite = 0;
								}
								if (tDelta > 0.05f)
								{
									_async_Timer = System.DateTime.Now;
									_async_SubProcessIndex++;


									int curRequests = _nextFileWriteRequest.Count;
									//int prevWrite = _nFileWrite;
									//int nProcess = Mathf.Clamp(_nextFileWriteRequest.Count, 500, 1000);
									
									//시간이 좀 걸리더라도 버벅이지 않게 하자
									for (int iProcess = 0; iProcess < 700; iProcess++)
									{
										AsyncFileWriteRequest popRequest = _nextFileWriteRequest[0];
										_nextFileWriteRequest.RemoveAt(0);

										List<AsyncFileWriteRequest> result = AsyncFileWrite(_async_StreamWriter, popRequest);
										_nFileWrite++;

										if (result != null && result.Count > 0)
										{
											//리스트의 앞쪽에 붙여야 한다. (Recursive와 동일한 순서로 작성되어야 한다.)
											_nextFileWriteRequest.InsertRange(0, result);
										}


										if (_nextFileWriteRequest.Count == 0)
										{
											ChangeAsyncStep(ASYNC_SAVE_STEP.Step4_FileEnd);
											break;
										}
									}

									//if (_async_SubProcessIndex % 100 == 0)
									//{
									//	Debug.Log("Async FileWrite [" + _async_SubProcessIndex + "] / Write " + prevWrite + " >> " + _nFileWrite);
									//}
									if (_nextFileWriteRequest.Count == 0)
									{
										//Debug.LogError("Async File Write End [Loop " + _async_SubProcessIndex + "]");
										ChangeAsyncStep(ASYNC_SAVE_STEP.Step4_FileEnd);
									}
								}
							}
							break;


						case ASYNC_SAVE_STEP.Step4_FileEnd:
							{
								//4. File End
								//File Stream을 닫고 처리를 종료한다.
								if (_async_FirstFrame)
								{
									//Debug.Log("<< Async : 4. File End >>");
									_async_FirstFrame = false;
									_async_SubProcessIndex = 0;
								}

								//Debug.LogError("Back Step 4 : Destroy Dummy");
								UnityEngine.Object.DestroyImmediate(_autoSave_TargetPortrait.gameObject);
								_autoSave_TargetPortrait = null;

								_isAutoSaveBakeWorking = false;
								//_isReadyAutoSave = false;
								_isRequestAutoSaveStop = false;

								_async_Step = ASYNC_SAVE_STEP.Step0_Ready;
								_async_RootUnit = null;

								if (_async_StreamWriter != null)
								{
									_async_StreamWriter.Flush();
									_async_StreamWriter.Close();
								}

								if (_async_FileStream != null)
								{
									_async_FileStream.Close();
								}

								_async_StreamWriter = null;
								_async_FileStream = null;

								

								System.GC.Collect();


								//종료!!
								return true;
							}
							//break;
					}

					if(isAsyncProcess)
					{
						//비동기 방식이면 바로 while loop를 탈출한다.
						break;
					}
				}
				return false;
			}
			catch (System.Exception ex)
			{
				Debug.LogError("Async Exception : " + ex);
				return true;
			}
			
		}

		private void ChangeAsyncStep(ASYNC_SAVE_STEP nextStep)
		{
			_async_Step = nextStep;
			_async_FirstFrame = true;
			_async_SubProcessIndex = 0;
			_async_Timer = System.DateTime.Now;
		}


		
		/// <summary>
		/// Serialize의 비동기 버전.
		/// 함수 자체가 비동기인게 아니라, Recursive 함수를 뜯어서 중간 결과를 리턴하여
		/// 다음 프레임에서 이어서 처리할 수 있도록 분리한 함수
		/// 리턴된 Request를 저장했다가 다음에 요청하면 된다.
		/// 하위에 이어서 처리할 Request가 없다면 null이 리턴된다.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private List<AsyncSerializeRequest> AsyncSerialize(AsyncSerializeRequest request)
		{
			object targetObj = request._targetObj;
			apBackupUnit parentInstance = request._parentInstance;
			int curLevel = request._level;

			string name = targetObj.GetType().Name;
			//Type objType = targetObj.GetType();

			List<FieldInfo> fieldsTotal = GetAllFields(targetObj);

			if (curLevel > 50)
			{
				Debug.LogError("Serialize Level > 50");
				return null;
			}


			List<AsyncSerializeRequest> nextRequests = new List<AsyncSerializeRequest>();

			for (int i = 0; i < fieldsTotal.Count; i++)
			{
				bool isSerializedAttribute = false;
				bool isSerializedField = false;
				bool isSerialized = false;
				bool isBackupTargetSkip = false;
				object[] attrs = fieldsTotal[i].GetCustomAttributes(true);
				string strAttrs = "";
				if (attrs != null && attrs.Length != 0)
				{
					
					for (int iAttr = 0; iAttr < attrs.Length; iAttr++)
					{
						strAttrs += attrs[iAttr].ToString() + " ";
						
						if(attrs[iAttr].GetType().Equals(typeof(UnityEngine.SerializeField)))
						{
							isSerializedAttribute = true;
						}
						else if(attrs[iAttr].GetType().Equals(typeof(AnyPortrait.NonBackupField)))
						{
							isBackupTargetSkip = true;
						}
						else if(attrs[iAttr].GetType().Equals(typeof(System.NonSerializedAttribute)))
						{
							isBackupTargetSkip = true;
						}
					}
				}
				
				if(!fieldsTotal[i].IsNotSerialized)
				{
					isSerializedField = true;
				}

				//isSerialized 조건
				//Public 타입이면 => isSerializedField이거나 SerializedAttrubute 가 있으면 된다.
				//Private 타입이면 => SerializedAttrubute + isSerializedField 있어야 한다.
				//만약 Private 타입인데 SerializedAttrubute가 없다면 => 
				if(fieldsTotal[i].IsPublic)
				{
					isSerialized = isSerializedAttribute | isSerializedField;
				}
				else
				{
					isSerialized = isSerializedAttribute & isSerializedField;
				}
				if(!isSerialized || isBackupTargetSkip || fieldsTotal[i].IsLiteral || fieldsTotal[i].IsInitOnly)//직렬화가 아니거나 백업 대상이 아닌 경우
				{
					continue;
				}

				//시리얼 유닛 생성 + 필드를 소유하는 인스턴스에 등록
				apBackupUnit newUnit = new apBackupUnit();
				newUnit.SetField(fieldsTotal[i], fieldsTotal[i].GetValue(targetObj), parentInstance, _table);

				
				
				if(newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.List ||
					newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Array)
				{
					//1. List나 Array라면
					if (newUnit._value != null)
					{
						IList arrayValues = newUnit._value as IList;
						if (arrayValues != null)
						{
							int index = 0;
							foreach (object arrItem in arrayValues)
							{
								apBackupUnit arrUnit = new apBackupUnit();
								arrUnit.SetItem(arrItem, newUnit, index, _table);

								if (arrUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.UnityMonobehaviour ||
									arrUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance)
								{
									//1-2. 리스트나 배열의 객체가 Instance라면
									//Serialize(arrItem, arrUnit, curLevel + 1);
									//수정 -> 바로 함수를 호출하는게 아니라 요청 값을 받아서 다음으로 미룬다.
									nextRequests.Add(new AsyncSerializeRequest(arrItem, arrUnit, curLevel + 1));
								}
								index++;
							}
						}
					}
				}
				else if(newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.UnityMonobehaviour ||
					newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance)
				{
					//2. Monobehaviour로부터 상속받은 객체 또는 일반 Instance라면
					if (newUnit._value != null)
					{
						//Serialize(newUnit._value, newUnit, curLevel + 1);
						//수정 -> 바로 함수를 호출하는게 아니라 요청 값을 받아서 다음으로 미룬다.
						nextRequests.Add(new AsyncSerializeRequest(newUnit._value, newUnit, curLevel + 1));
					}
				}
			}


			return nextRequests;
		}



		/// <summary>
		/// FileWriteRecursive의 비동기 버전
		/// 요청 처리 후 "다음에 처리할 새로운 요청"값을 리스트로 리턴한다.
		/// 작성 순서가 중요하므로
		/// </summary>
		/// <param name="sw"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		private List<AsyncFileWriteRequest> AsyncFileWrite(StreamWriter sw, AsyncFileWriteRequest request)
		{
			string strWrite = request._backupUnit.GetEncodingString(_table);
			sw.WriteLine(strWrite);

			

			
			if(request._backupUnit._childFields != null && request._backupUnit._childFields.Count > 0)
			{
				List<AsyncFileWriteRequest> result = new List<AsyncFileWriteRequest>();
				for (int i = 0; i < request._backupUnit._childFields.Count; i++)
				{
					//FileWriteRecursive(sw, request._backupUnit._childFields[i], request._level + 1);
					//바로 Recursive 함수를 호출하지 말고 다음 요청값을 꺼내주자
					result.Add(new AsyncFileWriteRequest(request._backupUnit._childFields[i], request._level + 1));
					
				}

				return result;
			}

			if(request._backupUnit._childItems != null && request._backupUnit._childItems.Count > 0)
			{
				List<AsyncFileWriteRequest> result = new List<AsyncFileWriteRequest>();
				for (int i = 0; i < request._backupUnit._childItems.Count; i++)
				{
					//FileWriteRecursive(sw, request._backupUnit._childItems[i], request._level + 1);
					//바로 Recursive 함수를 호출하지 말고 다음 요청값을 꺼내주자
					result.Add(new AsyncFileWriteRequest(request._backupUnit._childItems[i], request._level + 1));
				}

				return result;
			}

			return null;
		}



		// Serialize
		//---------------------------------------------
		public bool SaveBackupManual(string filePath, apPortrait srcPortriat)
		{
			if(_isAutoSaveBakeWorking)
			{
				//자동 저장 중이다.
				return false;
			}

			//추가. 타입/변수 테이블을 만들어서 데이터를 줄이자.
			_table.Clear();

			//변경 10.4 : 더미 GameObject를 만들어서 복사하자.
			GameObject dummyGameObject = UnityEngine.Object.Instantiate<GameObject>(srcPortriat.gameObject);
			dummyGameObject.name = srcPortriat.gameObject.name;
			dummyGameObject.hideFlags = HideFlags.HideAndDontSave;//<<나중에 이걸 붙이자

			apPortrait targetPortrait = dummyGameObject.GetComponent<apPortrait>();

			apBackupUnit rootUnit = new apBackupUnit();//<<Root 생성
			rootUnit.SetRoot();
			Serialize(targetPortrait, rootUnit, 0);

			//rootUnit.PrintDebugRecursive();

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

				sw.WriteLine(apVersion.I.APP_VERSION_INT + "");
				sw.WriteLine(targetPortrait.name);
				//sw.WriteLine(DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString());
				sw.WriteLine(GetCurrentTimeString());

				//테이블을 작성하자
				_table.FileWrite(sw);

				FileWriteRecursive(sw, rootUnit, 0);

				sw.Flush();

				sw.Close();
				fs.Close();
				sw = null;
				fs = null;


				//저장된 파일을 연다. 현재는 작동 안함
				//Application.OpenURL("file://" + filePath);

				if(dummyGameObject != null)
				{
					UnityEngine.Object.DestroyImmediate(dummyGameObject);
					dummyGameObject = null;
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError("SaveBackup Exception : " + ex);

				if(sw != null)
				{
					sw.Close();
					sw = null;
				}
				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				if(dummyGameObject != null)
				{
					UnityEngine.Object.DestroyImmediate(dummyGameObject);
					dummyGameObject = null;
				}
				return false;
			}


			return true;
			
		}





		public void FileWriteRecursive(StreamWriter sw, apBackupUnit backupUnit, int curLevel)
		{
			string strWrite = backupUnit.GetEncodingString(_table);
			sw.WriteLine(strWrite);

			

			//if(curLevel > 5)
			//{
			//	Debug.LogError("Level 5 초과");
			//	return;
			//}
			if(backupUnit._childFields != null && backupUnit._childFields.Count > 0)
			{
				for (int i = 0; i < backupUnit._childFields.Count; i++)
				{
					FileWriteRecursive(sw, backupUnit._childFields[i], curLevel + 1);
				}
			}

			if(backupUnit._childItems != null && backupUnit._childItems.Count > 0)
			{
				for (int i = 0; i < backupUnit._childItems.Count; i++)
				{
					FileWriteRecursive(sw, backupUnit._childItems[i], curLevel + 1);
				}
			}
			//if(backupUnit)
		}





		//---------------------------------------------------------------------------------
		// Primitive 타입인 경우
		// Type : Name : Value
		
		// List<Primitive> 타입인 경우
		// List or Array : <Type> : Name : [ Item ]

		// Instance 타입인 경우
		// Type : Name : Value ->> Child Fields

		
		private List<FieldInfo> GetAllFields(object targetObj)
		{
			System.Type objType = targetObj.GetType();
			List<FieldInfo> fieldsTotal = new List<FieldInfo>();

			//일반 인스턴스여야함
			apBackupUnit.FIELD_CATEGORY fieldCategory = apBackupUnit.GetFieldCategory(objType);

			if (fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance)
			{
				System.Type curObjType = objType;
				while (true)
				{
					FieldInfo[] fields = curObjType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					for (int i = 0; i < fields.Length; i++)
					{
						fieldsTotal.Add(fields[i]);
					}

					System.Type baseType = curObjType.BaseType;
					if (baseType.Equals(typeof(System.Object))
						|| baseType == null
						|| curObjType == baseType)
					{
						break;
					}
					curObjType = baseType;

				}
			}
			else
			{
				FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0; i < fields.Length; i++)
				{
					fieldsTotal.Add(fields[i]);
				}
			}

			return fieldsTotal;
		}
		
		private void GetDebugReverseLevel(apBackupUnit curInstance, List<string> strDebug, int curLevel)
		{
			if(curInstance._parentInstance != null && curLevel > 10)
			{
				GetDebugReverseLevel(curInstance._parentInstance, strDebug, curLevel - 1);
			}
			else if(curInstance._parentContainer != null && curLevel > 10)
			{
				GetDebugReverseLevel(curInstance._parentContainer, strDebug, curLevel - 1);
			}
			strDebug.Add("[" + curLevel + "] Field Name : " + curInstance._fieldName + " (" + curInstance._typeName_Full + ")");
		}

		private void Serialize(object targetObj, apBackupUnit parentInstance, int curLevel)
		{
			string name = targetObj.GetType().Name;
			//Type objType = targetObj.GetType();

			List<FieldInfo> fieldsTotal = GetAllFields(targetObj);
			//FieldInfo[] fields_Cur = objType.GetFields();

			//if(fields_Cur != null && fields_Cur.Li)


			//Debug.Log("Name : " + name);
			if (curLevel > 50)
			{
				Debug.LogError("Serialize Level > 50");
				List<string> strDebug = new List<string>();
				GetDebugReverseLevel(parentInstance, strDebug, curLevel);
				//Debug.Log("[" + curLevel + "] Field Name : " + parentInstance._fieldName + " (" + parentInstance._typeName_Full + ")");
				string totalDebug = "";
				for (int i = 0; i < strDebug.Count; i++)
				{
					totalDebug += strDebug[i] + "\n";
				}
				Debug.Log(totalDebug);
				return;
			}

			for (int i = 0; i < fieldsTotal.Count; i++)
			{
				bool isSerializedAttribute = false;
				bool isSerializedField = false;
				bool isSerialized = false;
				bool isBackupTargetSkip = false;
				object[] attrs = fieldsTotal[i].GetCustomAttributes(true);
				string strAttrs = "";
				if (attrs != null && attrs.Length != 0)
				{
					
					for (int iAttr = 0; iAttr < attrs.Length; iAttr++)
					{
						strAttrs += attrs[iAttr].ToString() + " ";
						
						if(attrs[iAttr].GetType().Equals(typeof(UnityEngine.SerializeField)))
						{
							isSerializedAttribute = true;
						}
						else if(attrs[iAttr].GetType().Equals(typeof(AnyPortrait.NonBackupField)))
						{
							isBackupTargetSkip = true;
						}
						else if(attrs[iAttr].GetType().Equals(typeof(System.NonSerializedAttribute)))
						{
							isBackupTargetSkip = true;
						}
					}
				}
				
				if(!fieldsTotal[i].IsNotSerialized)
				{
					isSerializedField = true;
				}

				//isSerialized 조건
				//Public 타입이면 => isSerializedField이거나 SerializedAttrubute 가 있으면 된다.
				//Private 타입이면 => SerializedAttrubute + isSerializedField 있어야 한다.
				//만약 Private 타입인데 SerializedAttrubute가 없다면 => 
				if(fieldsTotal[i].IsPublic)
				{
					isSerialized = isSerializedAttribute | isSerializedField;
				}
				else
				{
					isSerialized = isSerializedAttribute & isSerializedField;
				}
				if(!isSerialized || isBackupTargetSkip || fieldsTotal[i].IsLiteral || fieldsTotal[i].IsInitOnly)//직렬화가 아니거나 백업 대상이 아닌 경우
				{
					continue;
				}

				//시리얼 유닛 생성 + 필드를 소유하는 인스턴스에 등록
				apBackupUnit newUnit = new apBackupUnit();
				newUnit.SetField(fieldsTotal[i], fieldsTotal[i].GetValue(targetObj), parentInstance, _table);

				//Debug.Log("[" + curLevel + "] Field Name : " + newUnit._fieldName + " (" + newUnit._typeName_Full + ")");
				//Debug.Log("[" + i + "] Name : " + newUnit._fieldName + "(" + newUnit._typeName + " [" + newUnit._fieldCategory + "]");


				
				if(newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.List ||
					newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Array)
				{
					//1. List나 Array라면
					if (newUnit._value != null)
					{
						IList arrayValues = newUnit._value as IList;
						if (arrayValues != null)
						{
							int index = 0;
							foreach (object arrItem in arrayValues)
							{
								apBackupUnit arrUnit = new apBackupUnit();
								arrUnit.SetItem(arrItem, newUnit, index, _table);

								if (arrUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.UnityMonobehaviour ||
									arrUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance)
								{
									//1-2. 리스트나 배열의 객체가 Instance라면
									Serialize(arrItem, arrUnit, curLevel + 1);
								}
								index++;
							}
						}
					}
				}
				else if(newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.UnityMonobehaviour ||
					newUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance)
				{
					//2. Monobehaviour로부터 상속받은 객체 또는 일반 Instance라면
					if (newUnit._value != null)
					{
						Serialize(newUnit._value, newUnit, curLevel + 1);
					}
				}
			}
			//Debug.LogError("-------------------------------------");
			
		}

		//------------------------------------------------------------------------------------------------------


		private int _nPackUnit = 0;
		public apPortrait LoadBackup(string filePath)
		{
			//Debug.Log("LoadBackup : " + filePath);
			FileStream fs = null;
			StreamReader sr = null;

			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				//일단 리스트로 모두 파싱하자
				List<apBackupUnit> parsedUnit = new List<AnyPortrait.apBackupUnit>();

				//추가:테이블 초기화
				_table.Clear();

				string strRead = "";

				//파일 버전, 이름, DateTime
				//int fileVersion = int.Parse(sr.ReadLine());
				sr.ReadLine();//<<수정 : 이게 빠져서 백업 로드가 안되었다. ("fileVersion")
				

				string portraitName = sr.ReadLine();
				string strDateTime = sr.ReadLine();

				//테이블을 읽는다.
				if(!_table.FileRead(sr))
				{
					return null;
				}


				//Debug.Log("Version : " + fileVersion);

				while (true)
				{
					if (sr.Peek() < 0)
					{
						//Debug.Log("Peek End");
						break;
					}
					strRead = sr.ReadLine();
					//Debug.Log("ReadLine : " + strRead);

					apBackupUnit newUnit = new apBackupUnit();
					bool isDecodeResult = newUnit.Decode(strRead, _table);
					if (!isDecodeResult)
					{
						Debug.LogError("Decode 실패 : " + strRead);
						break;
					}

					parsedUnit.Add(newUnit);
				}

				sr.Close();
				fs.Close();
				sr = null;
				fs = null;


				//이제 재귀적으로 Unit을 돌면서 Instance의 Child나 Item의 Child를 묶어주도록 하자
				//Debug.Log("Load Backup - PackRecursive");

				_nPackUnit = 0;
				PackChildsRecursive(0, parsedUnit, null, 0);

				apBackupUnit rootUnit = parsedUnit[0];
				
				if(rootUnit == null)
				{
					Debug.LogError("Parent-Child 연결 실패");
					return null;
				}


				//rootUnit.PrintDebugRecursive();//<<Debug

				//이제 하나씩 만들어봅시다.
				//일단 Portrait부터
				GameObject newPortraitObj = new GameObject("Backup_" + portraitName + "__" + strDateTime);
				newPortraitObj.transform.position = Vector3.zero;
				newPortraitObj.transform.rotation = Quaternion.identity;
				newPortraitObj.transform.localScale = Vector3.one;

				apPortrait portrait = newPortraitObj.AddComponent<apPortrait>();

				// Monobehaviour 타입의 멤버가 저장될 Group을 만들자.
				MakeObjectGroup(portrait);

				//Debug.LogError("----------- Parsed Data 2 Portrait -----------------");
				bool isResult = RecursiveMakePortrait(rootUnit, portrait, null, null, null, 0);
				if(!isResult)
				{
					UnityEngine.MonoBehaviour.DestroyImmediate(newPortraitObj);
					return null;
				}

				return portrait;
			}
			catch (System.Exception ex)
			{
				Debug.LogError("Backup Exception : " + ex);

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

				return null;
			}


			//return null;
			//FileStream fs = null;
			//try
			//{
			//	fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			//	XmlSerializer ser = new XmlSerializer(typeof(apPortrait));
			//	apPortrait resultPortrait = ser.Deserialize(fs) as apPortrait;

			//	fs.Close();
			//	fs = null;

			//	return resultPortrait;
			//}
			//catch(System.Exception ex)
			//{
			//	Debug.LogError("LoadBackup Exception : " + ex);
			//	if(fs != null)
			//	{
			//		fs.Close();
			//		fs = null;
			//	}
			//	return null;
			//}
		}



		private int PackChildsRecursive(int iCur, List<apBackupUnit> targetUnits, apBackupUnit parentUnit, int targetLevel)
		{
			//if(targetLevel > 5)
			//{
			//	Debug.LogError("너무 많은 레벨 [" + targetLevel + "]");
			//	return iCur;
			//}
			//iCur 처리 후, 마지막으로 "계산된" iCur를 리턴한다.
			//마지막 인덱스가 계산되지 않았다면 iCur - 1을 리턴한다.
			if(targetUnits.Count == 0 || iCur >= targetUnits.Count)
			{
				return iCur;
			}

			//int targetLevel = 0;
			//if(parentUnit != null)
			//{
			//	targetLevel = parentUnit._level + 1;
			//}

			//Debug.Log("PackChildsRecursive [ Level : " + targetLevel + " ] (Cur Index : " + iCur + ")");

			//parent의 레벨을 기준으로 -> 높으면 Child이므로 Recursive 호출
			//낮으면 리턴 (이때는 Index를 -1 한다. 처리되지 않았으므로)
			//Next 찾지 말고 Cur로 검색
			apBackupUnit prevUnit = parentUnit;

			while(true)
			{
				//Debug.Log("[" + targetLevel + "] (Cur Index : " + iCur + ")");
				//if(_nPackUnit > 100)
				//{
				//	Debug.LogError("너무 많은 처리 횟수 [" + _nPackUnit + "]");
				//	return iCur;
				//}
				if(iCur >= targetUnits.Count || _nPackUnit > targetUnits.Count)
				{
					//마지막 인덱스
					//Debug.LogError("Last Index (" + iCur + "/" + targetUnits.Count + ")");
					return iCur;
				}

				apBackupUnit curUnit = targetUnits[iCur];
				if(iCur >= 1)
				{
					prevUnit = targetUnits[iCur - 1];
				}
				
				if(curUnit._level < targetLevel)
				{
					//현재 재귀 콜보다 Parent Level이다.
					//Debug.LogError("Return (상위 레벨 " + curUnit._level + " < " + targetLevel + ")");
					//Debug.LogWarning("상위 레벨로 이동(" + targetLevel + " >> " + (targetLevel - 1) + ")");
					return iCur - 1;
				}
				else if(curUnit._level > targetLevel && prevUnit != null)
				{
					

					//현재 재귀 콜보다 Child Level이다.
					//Debug.Log("Start Recursive [" + iCur + " / Prev Level : " + prevUnit._level + "]");
					int iLastChild = PackChildsRecursive(iCur, targetUnits, prevUnit, targetLevel + 1);
					
					int iNext = iLastChild + 1;
					//Debug.Log("Recursive Call [" + iCur + " >> " + iLastChild + "(Next : " + iNext + ")");

					if(iNext <= iCur)
					{
						//?? 더 처리 안되나여..
						Debug.LogError("Pack Error : Recursive 후에 Index가 전혀 진행이 안되었다.");
						iCur++;
					}
					else
					{
						iCur = iNext;
					}
				}
				else
				{
					_nPackUnit++;
					if(parentUnit != null)
					{
						if(parentUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance ||
							parentUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.UnityMonobehaviour ||
							parentUnit._isRoot)
						{
							if(parentUnit._childFields == null)
							{
								parentUnit._childFields = new List<apBackupUnit>();
							}
							parentUnit._childFields.Add(curUnit);
							//if(parentUnit._isRoot)
							//{
							//	Debug.Log("Add To Root [" + curUnit._fieldName + "]");
							//}
						}
						else if(parentUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.List || 
							parentUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Array)
						{
							if(parentUnit._childItems == null)
							{
								parentUnit._childItems = new List<apBackupUnit>();
							}
							parentUnit._childItems.Add(curUnit);
						}
					}

					iCur++;
				}

				
			}


			//return iCur;


			//apBackupUnit firstUnit = null;
			////맨 앞의 것을 빼자
			//while (true)
			//{
			//	if (targetUnits.Count == 0)
			//	{
			//		break;
			//	}

			//	apBackupUnit curUnit = targetUnits[0];
			//	if(curUnit._level != targetLevel)
			//	{
			//		break;
			//	}


			//	targetUnits.RemoveAt(0);
				
			//	if (parentUnit != null)
			//	{
			//		if (isChildField)
			//		{
			//			if (parentUnit._childFields == null)
			//			{
			//				parentUnit._childFields = new List<apBackupUnit>();
			//			}
			//			parentUnit._childFields.Add(curUnit);
			//		}
			//		else if (isChildItem)
			//		{
			//			if (parentUnit._childItems == null)
			//			{
			//				parentUnit._childItems = new List<apBackupUnit>();
			//			}
			//			parentUnit._childItems.Add(curUnit);
			//		}
			//		else
			//		{
			//			Debug.LogError("Child 옵션 실패");
			//		}

			//		//Debug.Log("Pack : " + parentUnit._fieldName + " > " + curUnit._fieldName + " (" + curLevel + ")");
			//	}
			//	else
			//	{
			//		//Debug.Log("Pack : Root");
			//	}
				

			//	if(firstUnit == null)
			//	{
			//		firstUnit = curUnit;
			//	}

			//	//다음 호출을 해야할 때
			//	if (targetUnits.Count == 0)
			//	{
			//		break;
			//	}


			//	apBackupUnit nextUnit = targetUnits[0];
			//	if (nextUnit._level > targetLevel)
			//	{
			//		//1. curUnit의 Category가 Intance일때 + 다음 Level이 더 높을때
			//		// parentUnit을 자기 자신으로 두고 재귀 호출
					
					

			//		//2. curUnit의 Category가 Array나 List일때 + 다음 Level이 더 높을때
			//		// parentUnit을 자기 자신으로 두고 재귀 호출
			//		if(curUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Instance
			//			|| curUnit._isRoot)
			//		{
			//			PackChildsRecursive(targetUnits, curUnit, true, false);
			//		}
			//		else if(curUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.List || 
			//			curUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Array)
			//		{
			//			PackChildsRecursive(targetUnits, curUnit, false, true);
			//		}
			//		else
			//		{
			//			Debug.LogError("Child가 있는데 Instance도 아니고 List/Array도 아니다.");
			//		}
			//	}
			//	else if (nextUnit._level == targetLevel)
			//	{
			//		//3. 다음 Level이 같을때 : 그냥 다음 루프를 돈다.
			//	}
			//	else
			//	{
			//		//리턴하고 위로 올라가야 한다.
			//		break;
			//	}


				
			//}
			
			////4. 리턴
			//return firstUnit;
		}



		public void MakeObjectGroup(apPortrait portrait)
		{
			if (portrait._subObjectGroup == null)
			{
				portrait._subObjectGroup = new GameObject("EditorObjects");
				portrait._subObjectGroup.transform.parent = portrait.transform;
				portrait._subObjectGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_Mesh == null)
			{
				portrait._subObjectGroup_Mesh = new GameObject("Meshes");
				portrait._subObjectGroup_Mesh.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Mesh.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Mesh.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Mesh.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Mesh.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_MeshGroup == null)
			{
				portrait._subObjectGroup_MeshGroup = new GameObject("MeshGroups");
				portrait._subObjectGroup_MeshGroup.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_MeshGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_MeshGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_MeshGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if(portrait._subObjectGroup_Modifier == null)
			{
				portrait._subObjectGroup_Modifier = new GameObject("Modifiers");
				portrait._subObjectGroup_Modifier.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Modifier.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Modifier.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Modifier.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Modifier.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
		}



		private bool RecursiveMakePortrait(apBackupUnit curUnit, apPortrait targetPortrait, apBackupUnit parentUnit, object parentInstance, object parentList, int indexOfListArray)
		{
			try
			{
				object curInstance = null;
				object curListArray = null;

				if (curUnit._isRoot)
				{
					//Root 타입이면 별도의 값 지정은 없다.
					curInstance = targetPortrait;
					//Debug.Log("Root");
				}
				else
				{
					//현재 해당하는 필드를 찾자
					FieldInfo fi = null;
					IList list = null;

					if (curUnit._isListArrayItem)
					{
						list = parentList as IList;

						//Debug.Log("Item [" + curUnit._level + " / " + curUnit._fieldCategory + "] : " + curUnit._fieldName);

					}
					else
					{
						//현재 해당하는 필드를 찾자
						fi = parentInstance.GetType().GetField(curUnit._fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

						//Debug.Log("[" + curUnit._level + " / " + curUnit._fieldCategory + "] : " + curUnit._fieldName);
					}

					//if (fi != null)
					{
						//필드가 있는 경우
						//없다면 업데이트하면서 멤버가 빠진 경우이다.



						switch (curUnit._fieldCategory)
						{
							case apBackupUnit.FIELD_CATEGORY.Primitive:
							case apBackupUnit.FIELD_CATEGORY.String:
							case apBackupUnit.FIELD_CATEGORY.Vector2:
							case apBackupUnit.FIELD_CATEGORY.Vector3:
							case apBackupUnit.FIELD_CATEGORY.Vector4:
							case apBackupUnit.FIELD_CATEGORY.Color:
							case apBackupUnit.FIELD_CATEGORY.Matrix4x4:
							case apBackupUnit.FIELD_CATEGORY.Matrix3x3:
								{
									try
									{
										if (curUnit._isListArrayItem)
										{
											//??>>이거 체크
											if (parentUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.Array)
											{
												list[indexOfListArray] = curUnit._value;
											}
											else if (parentUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.List)
											{
												list.Add(curUnit._value);
											}

										}
										else
										{	
											if (fi != null)
											{
												fi.SetValue(parentInstance, curUnit._value);
											}
										}
									}
									catch (System.Exception ex)
									{
										Debug.LogError("Recursive make Portrait Exception [" + parentUnit._fieldName + " ( " + parentUnit._typeName_Partial + " ) > " + curUnit._fieldCategory + " / " + curUnit._fieldName + " / " + curUnit._typeName_Partial + "] : " + ex);
										return false;
									}
								}
								break;

							case apBackupUnit.FIELD_CATEGORY.Enum:
								{
									if (curUnit._isListArrayItem)
									{
										list.Add((int)curUnit._value);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, (int)curUnit._value);
										}
									}
								}
								break;

							case apBackupUnit.FIELD_CATEGORY.UnityMonobehaviour:
								{
									//Monobehaviour 타입인 경우 GameObject를 필요로 한다.
									System.Type monoType = System.Type.GetType(curUnit._typeName_Partial);
									GameObject groupGameObject = null;
									if (monoType.Equals(typeof(apMesh)))
									{
										groupGameObject = targetPortrait._subObjectGroup_Mesh;
									}
									else if (monoType.Equals(typeof(apMeshGroup)))
									{
										groupGameObject = targetPortrait._subObjectGroup_MeshGroup;
									}
									else if (monoType.Equals(typeof(apModifierBase)) ||
										monoType.Equals(typeof(apModifier_AnimatedFFD)) ||
										monoType.Equals(typeof(apModifier_AnimatedMorph)) ||
										monoType.Equals(typeof(apModifier_AnimatedTF)) ||
										monoType.Equals(typeof(apModifier_FFD)) ||
										monoType.Equals(typeof(apModifier_Morph)) ||
										monoType.Equals(typeof(apModifier_Physic)) ||
										monoType.Equals(typeof(apModifier_Rigging)) ||
										monoType.Equals(typeof(apModifier_TF)) ||
										monoType.Equals(typeof(apModifier_Volume)) ||
										//추가 21.7.21
										monoType.Equals(typeof(apModifier_ColorOnly)) ||
										monoType.Equals(typeof(apModifier_AnimatedColorOnly))
										)
									{
										groupGameObject = targetPortrait._subObjectGroup_Modifier;
									}
									else
									{
										Debug.LogError("Unity MonoBehaviour 타입이지만 대상이 속할 GameGroup이 없다. [" + curUnit._typeName_Partial + "]");
										groupGameObject = targetPortrait._subObjectGroup;
									}

									GameObject newGameObject = new GameObject(curUnit._monoName);
									newGameObject.transform.parent = groupGameObject.transform;
									newGameObject.transform.localPosition = curUnit._monoPosition;
									newGameObject.transform.localRotation = curUnit._monoQuat;
									newGameObject.transform.localScale = curUnit._monoScale;

									object monoObject = newGameObject.AddComponent(monoType);
									curInstance = monoObject;

									if (curUnit._isListArrayItem)
									{
										list.Add(monoObject);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, monoObject);
										}
									}


								}
								break;
							case apBackupUnit.FIELD_CATEGORY.UnityGameObject:
								{
									//Debug.LogError("GameObject?? 이건 어디다 파싱하져..");

									GameObject newGameObject = new GameObject(curUnit._monoName);
									newGameObject.transform.parent = targetPortrait._subObjectGroup.transform;
									newGameObject.transform.localPosition = curUnit._monoPosition;
									newGameObject.transform.localRotation = curUnit._monoQuat;
									newGameObject.transform.localScale = curUnit._monoScale;

									if (curUnit._isListArrayItem)
									{
										list.Add(newGameObject);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, newGameObject);
										}
									}

								}
								break;
							case apBackupUnit.FIELD_CATEGORY.UnityObject:
								{
									//Debug.LogError("UnityObject?? 이건 어디다 파싱하져.. [" + curUnit._typeName_Partial + " / " + curUnit._fieldName + "]");


									object instanceObj = System.Activator.CreateInstance(System.Type.GetType(curUnit._typeName_Partial));
									if (fi != null)
									{
										fi.SetValue(parentInstance, instanceObj);
									}
									curInstance = instanceObj;

									if (curUnit._isListArrayItem)
									{
										list.Add(instanceObj);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, instanceObj);
										}
									}
								}
								break;

							case apBackupUnit.FIELD_CATEGORY.Texture2D:
								{
									string assetPath = curUnit._monoAssetPath;
									Texture2D tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

									if (curUnit._isListArrayItem)
									{
										list.Add(tex2D);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, tex2D);
										}
									}
								}
								break;
							case apBackupUnit.FIELD_CATEGORY.CustomShader:
								{
									string assetPath = curUnit._monoAssetPath;
									Shader customShader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

									if (curUnit._isListArrayItem)
									{
										list.Add(customShader);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, customShader);
										}
									}
								}
								break;
							case apBackupUnit.FIELD_CATEGORY.Instance:
								{

									//Debug.Log("Instance [" + curUnit._typeName_Partial + "]");
									//Assembly asm = Assembly.GetEntryAssembly();
									System.Type unitType = System.Type.GetType(curUnit._typeName_Partial);
									if (unitType == null)
									{
										Debug.LogError("Type is Wrong");
									}
									else
									{
										//Debug.Log("Type is Founded [" + unitType.Name + "]");
									}
									object instanceObj = System.Activator.CreateInstance(unitType);


									curInstance = instanceObj;


									if (curUnit._isListArrayItem)
									{
										list.Add(instanceObj);
									}
									else
									{
										if (fi != null)
										{
											fi.SetValue(parentInstance, instanceObj);
										}
									}

									//Debug.Log("Instance Child Fields [" + curUnit._childFields.Count + "]");
								}
								break;

							case apBackupUnit.FIELD_CATEGORY.List:
							case apBackupUnit.FIELD_CATEGORY.Array:
								{
									bool isList = false;
									try
									{
										IList listObject = null;

										if (curUnit._fieldCategory == apBackupUnit.FIELD_CATEGORY.List)
										{
											isList = true;
											listObject = System.Activator.CreateInstance(System.Type.GetType(curUnit._typeName_Partial)) as IList;
										}
										else
										{
											isList = false;
											//listObject = Activator.CreateInstance(Type.GetType(curUnit._typeName_Partial), ) as IList;
											int nItem = 0;
											if (curUnit._childItems != null)
											{
												nItem = curUnit._childItems.Count;
											}
											System.Type arrayType = System.Type.GetType(curUnit._typeName_Partial);
											System.Type elementType = arrayType.GetElementType();
											listObject = System.Array.CreateInstance(elementType, nItem) as IList;
										}

										curListArray = listObject;

										if (curUnit._isListArrayItem)
										{
											list.Add(listObject);
										}
										else
										{
											if (fi != null)
											{
												fi.SetValue(parentInstance, listObject);
											}
										}
									}
									catch (System.Exception ex)
									{
										Debug.LogError("배열 CreateInstance 실패 : [" + curUnit._fieldName + "/" + curUnit._typeName_Partial + " (Is List:" + isList + ")] > " + ex);
										throw;
									}

									//Debug.Log("ListArray Child Items [" + curUnit._childItems.Count + "]");
								}
								break;
						}
					}

				}

				if (curUnit._childFields != null && curUnit._childFields.Count > 0)
				{
					//Debug.Log("Child Fields [" + curUnit._childFields.Count + "]");
					if (curInstance != null)
					{
						for (int i = 0; i < curUnit._childFields.Count; i++)
						{
							if(!RecursiveMakePortrait(curUnit._childFields[i], targetPortrait, curUnit, curInstance, null, i))
							{
								//에러 발생
								return false;
							}
						}
					}
					else
					{
						Debug.LogError("CurInstance -> Null");
					}
				}

				if (curUnit._childItems != null && curUnit._childItems.Count > 0)
				{
					//Debug.Log("Child Items [" + curUnit._childItems.Count + "]");
					if (curListArray != null)
					{
						for (int i = 0; i < curUnit._childItems.Count; i++)
						{
							if(!RecursiveMakePortrait(curUnit._childItems[i], targetPortrait, curUnit, null, curListArray, i))
							{
								//에러 발생
								return false;
							}
						}
					}
					else
					{
						Debug.LogError("curListArray -> Null");
					}
				}
			}
			catch(System.Exception ex)
			{
				Debug.LogError("Recursive make Portrait Exception : " + ex);
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------------
		public static string GetCurrentTimeString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(System.DateTime.Now.Year);
			if (System.DateTime.Now.Month < 10)
			{
				sb.Append("0");
				sb.Append(System.DateTime.Now.Month);
			}
			else
			{
				sb.Append(System.DateTime.Now.Month);
			}
			
			if(System.DateTime.Now.Day < 10)
			{
				sb.Append("0");
				sb.Append(System.DateTime.Now.Day);
			}
			else
			{
				sb.Append(System.DateTime.Now.Day);
			}

			sb.Append("_");

			if(System.DateTime.Now.Hour < 10)
			{
				sb.Append("0");
				sb.Append(System.DateTime.Now.Hour);
			}
			else
			{
				sb.Append(System.DateTime.Now.Hour);
			}
			if(System.DateTime.Now.Minute < 10)
			{
				sb.Append("0");
				sb.Append(System.DateTime.Now.Minute);
			}
			else
			{
				sb.Append(System.DateTime.Now.Minute);
			}

			return sb.ToString();
		}
	}
}