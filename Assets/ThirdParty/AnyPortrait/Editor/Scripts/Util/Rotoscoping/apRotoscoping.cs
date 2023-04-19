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


using AnyPortrait;
using System.IO;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.2.27 : 외부의 이미지 파일을 백그라운드에 출력할 수 있게 만드는 기능.
	/// 파일로 데이터를 저장한다.
	/// 파일 데이터는 ImageSet으로 구분. 이미지 한개만 열거나 여러개를 순차적으로 열고 바꾸거나 프레임과 동기화를 할 수 있다.
	/// </summary>
	public class apRotoscoping
	{

		// Sub Unit
		//---------------------------------------------------		
		public class ImageFileData
		{
			public int _index = 0;
			public string _filePath = "";

			public bool _isImageLoaded = false;
			public Texture2D _image = null;
			

			public ImageFileData(int index, string filePath)
			{
				_index = index;
				_filePath = filePath;

				_isImageLoaded = false;
				_image = null;
			}

			public void LoadImage()
			{
				if(_isImageLoaded)
				{
					return;
				}

				if(_image != null)
				{
					UnityEngine.Object.Destroy(_image);
					_image = null;
				}

				_isImageLoaded = true;//로드 성공 여부에 상관없이 값은 True (실패했을때 여러번 생성하는 것을 막기 위함)
				//성공, 실패 여부 상관없이 로드하자


				

				if (File.Exists(_filePath))
				{
					byte[] fileData = null;
					_image = new Texture2D(2, 2);
					_image.wrapMode = TextureWrapMode.Clamp;
					fileData = File.ReadAllBytes(_filePath);
					_image.LoadImage(fileData);
					_image.Apply();
				}
				
			}

			public void DestroyImage()
			{
				_isImageLoaded = false;
				if(_image != null)
				{
					//Debug.LogError("Destroy Image");
					UnityEngine.Object.DestroyImmediate(_image);
					_image = null;
				}
			}

			public Texture2D GetImage()
			{
				if(!_isImageLoaded && _image == null)
				{
					//로드 시도를 안했다면
					LoadImage();
				}
				return _image;
			}
		}
		public class ImageSetData
		{
			//이미지 파일 경로들			
			public string _name = null;
			
			public List<ImageFileData> _filePathList = null;
			public bool _isSyncToAnimation = false;//애니메이션 재생과 동기화를 할 것인가.
			public int _framePerSwitch = 5;//몇프레임마다 이미지가 바뀌는가
			public int _frameOffsetToSwitch = 0;//몇 프레임을 더한 후 Slide를 하자
			
			private const string KEY_NAME = "NAM";
			private const string KEY_FILE_PATH = "PTH";
			private const string KEY_SYNC = "SYN";
			private const string KEY_SLIDE_PER_FRAME = "SPF";
			private const string KEY_SLIDE_OFFSET = "SOF";
			

			public ImageSetData()
			{
				if(_filePathList == null)
				{
					_filePathList = new List<ImageFileData>();
				}
				_name = "";
				_filePathList.Clear();
				_isSyncToAnimation = false;
				_framePerSwitch = 5;
				_frameOffsetToSwitch = 0;
			}

			public void Sort()
			{
				_filePathList.Sort(delegate(ImageFileData a, ImageFileData b)
				{
					return a._index - b._index;
				});
			}

			public void ChangeFileOrder(ImageFileData fileData, bool toForward)
			{
				if(!_filePathList.Contains(fileData))
				{
					return;
				}

				

				//순서를 올리려면
				int iPrevIndex = fileData._index;
				int iNextIndex = (toForward ? (iPrevIndex - 1) : (iPrevIndex + 1));
				//Debug.Log("Order " + (toForward ? "Up" : "Down") + " / " + iPrevIndex + " > " + iNextIndex);

				ImageFileData curFileData = null;
				if(toForward)
				{
					//앞쪽으로(인덱스 감소, 다른 하나가 뒤로 이동함) 이동할 경우,
					//iNextIndex보다 크거나 같은 Index는 +10을 한다.
					for (int i = 0; i < _filePathList.Count; i++)
					{
						curFileData = _filePathList[i];
						if(curFileData == fileData)
						{
							continue;
						}
						if(curFileData._index >= iNextIndex)
						{
							curFileData._index += 10;
						}
					}
				}
				else
				{
					//뒤쪽으로(인덱스 증가, 다른 하나가 앞으로 이동함) 이동할 경우,
					//iNextIndex보다 작거나 같은 Index는 -10을 한다.
					for (int i = 0; i < _filePathList.Count; i++)
					{
						curFileData = _filePathList[i];
						if(curFileData == fileData)
						{
							continue;
						}
						if(curFileData._index <= iNextIndex)
						{
							curFileData._index -= 10;
						}
					}
				}
				fileData._index = iNextIndex;

				Sort();

				for (int i = 0; i < _filePathList.Count; i++)
				{
					_filePathList[i]._index = i;
				}
			}

			public void Save(StreamWriter sw)
			{
				sw.WriteLine(KEY_NAME + _name);
				int nFileList = _filePathList != null ? _filePathList.Count : 0;

				//Sort 먼저
				if(nFileList > 0)
				{
					Sort();
				}

				for (int i = 0; i < nFileList; i++)
				{
					sw.WriteLine(KEY_FILE_PATH + _filePathList[i]._filePath);
				}

				sw.WriteLine(KEY_SYNC + (_isSyncToAnimation ? "T" : "F"));
				sw.WriteLine(KEY_SLIDE_PER_FRAME + _framePerSwitch);
				sw.WriteLine(KEY_SLIDE_OFFSET + _frameOffsetToSwitch);
			}

			public void Load(string strRead)
			{
				string strKey = strRead.Substring(0, 3);
				string strValue = strRead.Length > 3 ? strRead.Substring(3) : "";

				if(string.Equals(strKey, KEY_NAME))
				{
					_name = strValue;
				}
				else if(string.Equals(strKey, KEY_FILE_PATH))
				{
					int iFile = _filePathList.Count;
					_filePathList.Add(new ImageFileData(iFile, strValue));
				}
				else if(string.Equals(strKey, KEY_SYNC))
				{
					_isSyncToAnimation = string.Equals(strValue, "T");
				}
				else if(string.Equals(strKey, KEY_SLIDE_PER_FRAME))
				{
					try
					{
						_framePerSwitch = int.Parse(strValue);
					}
					catch(Exception)
					{
						_framePerSwitch = 5;
					}
				}
				else if(string.Equals(strKey, KEY_SLIDE_OFFSET))
				{
					try
					{
						_frameOffsetToSwitch = int.Parse(strValue);
					}
					catch(Exception)
					{
						_frameOffsetToSwitch = 0;
					}
				}
			}

			public void AddImageFile(string filePath)
			{
				if(_filePathList == null)
				{
					_filePathList = new List<ImageFileData>();
				}
				int index = _filePathList.Count;
				_filePathList.Add(new ImageFileData(index, filePath));
			}

			public void RemoveImageFile(ImageFileData fileData)
			{
				_filePathList.Remove(fileData);
				Sort();
			}


			public void LoadImages()
			{
				int nFiles = _filePathList != null ? _filePathList.Count : 0;
				for (int i = 0; i < nFiles; i++)
				{
					_filePathList[i].LoadImage();
				}
			}

			public void DestroyImages()
			{
				int nFiles = _filePathList != null ? _filePathList.Count : 0;
				for (int i = 0; i < nFiles; i++)
				{
					_filePathList[i].DestroyImage();
				}
			}

			public Texture2D GetImage(int iImageFile)
			{
				int nFiles = _filePathList != null ? _filePathList.Count : 0;
				if(iImageFile < 0 || iImageFile >= nFiles)
				{
					return null;
				}
				return _filePathList[iImageFile].GetImage();
			}
		}


		// Members
		//---------------------------------------------------
		public List<ImageSetData> _imageSetDataList = null;

		//출력 위치와 투명도
		public int _posOffset_X = 0;
		public int _posOffset_Y = 0;		
		public int _opacity = 128;//255면 불투명
		public int _scaleWithinScreen = 80;//작업 공간 대비 80% 비율로 들어간다. 세로 기준

		private const string DELIMETER = "----";
		private const string KEY_POS_X = "POX";
		private const string KEY_POS_Y = "POY";
		private const string KEY_OPACITY = "OPC";
		private const string KEY_SCALE = "SCL";//스크린대비 크기

		private const string SAVE_FILE_NAME = "AnyPortrait_Rotoscoping.txt";

		private Texture2D _testImage = null;
		private string _testPath = "C:/AnyWorks/그림/AnyPortrait/액션게임/ActionGameSD_Character2.png";

		// Init
		//---------------------------------------------------
		public apRotoscoping()
		{
			if(_imageSetDataList == null)
			{
				_imageSetDataList = new List<ImageSetData>();
			}
			_imageSetDataList.Clear();
		}


		public void Clear()
		{
			_imageSetDataList.Clear();
		}
		
		/// <summary>
		/// 로토스코핑을 끄면 이미지를 모두 해제하자
		/// </summary>
		public void DestroyAllImages()
		{
			for (int i = 0; i < _imageSetDataList.Count; i++)
			{
				_imageSetDataList[i].DestroyImages();
			}
		}


		// Save / Load
		//---------------------------------------------------
		public bool Save()
		{
			FileStream fs = null;
			StreamWriter sw = null;
			
			DirectoryInfo di = new DirectoryInfo(Application.dataPath);
			string filePath = di.Parent.FullName;
			filePath = filePath.Replace("\\", "/");
			if(!filePath.EndsWith("/"))
			{
				filePath += "/";
			}
			filePath += SAVE_FILE_NAME;

			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				//값 저장시 KEY (3) + VALUE 방식으로 저장한다.
				//Delimeter 단위로 값을 나눈다.
				sw.WriteLine(KEY_POS_X + _posOffset_X);
				sw.WriteLine(KEY_POS_Y + _posOffset_Y);
				sw.WriteLine(KEY_OPACITY + _opacity);
				sw.WriteLine(KEY_SCALE + _scaleWithinScreen);
				
				for (int i = 0; i < _imageSetDataList.Count; i++)
				{
					sw.WriteLine(DELIMETER);
					_imageSetDataList[i].Save(sw);
				}

				sw.Flush();

				sw.Close();
				fs.Close();

				sw = null;
				fs = null;

				return true;
			}
			catch(Exception ex)
			{
				Debug.LogError("Rotoscoping Save Exception : " + ex);

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

				return false;
			}
		}


		public void Load()
		{
			Clear();

			FileStream fs = null;
			StreamReader sr = null;

			DirectoryInfo di = new DirectoryInfo(Application.dataPath);
			string filePath = di.Parent.FullName;
			filePath = filePath.Replace("\\", "/");
			if(!filePath.EndsWith("/"))
			{
				filePath += "/";
			}
			filePath += SAVE_FILE_NAME;


			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제와 인코딩 문제
				filePath = apUtil.ConvertEscapeToPlainText(filePath);
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				
				ImageSetData curImageSetData = null;
				while(sr.Peek() > -1)
				{
					string strRead = sr.ReadLine();
					if(string.IsNullOrEmpty(strRead))
					{
						continue;
					}
					if (string.Equals(strRead, DELIMETER))
					{
						//새로운 값을 만든다.
						curImageSetData = new ImageSetData();
						_imageSetDataList.Add(curImageSetData);
					}
					else
					{
						if(curImageSetData == null)
						{
							//기본 데이터
							string strKey = strRead.Substring(0, 3);
							string strValue = strRead.Length > 3 ? strRead.Substring(3) : "";
							if(string.Equals(strKey, KEY_POS_X))
							{
								try
								{
									_posOffset_X = int.Parse(strValue);
								}
								catch(Exception)
								{
									_posOffset_X = 0;
								}
							}
							else if(string.Equals(strKey, KEY_POS_Y))
							{
								try
								{
									_posOffset_Y = int.Parse(strValue);
								}
								catch(Exception)
								{
									_posOffset_Y = 0;
								}
							}
							else if(string.Equals(strKey, KEY_OPACITY))
							{
								try
								{
									_opacity = int.Parse(strValue);
									if(_opacity < 0)
									{
										_opacity = 0;
									}
									else if(_opacity > 255)
									{
										_opacity = 255;
									}
								}
								catch(Exception)
								{
									_opacity = 128;
								}
							}
							else if(string.Equals(strKey, KEY_SCALE))
							{
								try
								{
									_scaleWithinScreen = int.Parse(strValue);
									if(_scaleWithinScreen < 0)
									{
										_scaleWithinScreen = 0;
									}
									else if(_scaleWithinScreen > 300)
									{
										_scaleWithinScreen = 300;
									}
								}
								catch(Exception)
								{
									_scaleWithinScreen = 80;
								}
							}
						}
						else
						{
							curImageSetData.Load(strRead);
						}
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
					Debug.LogError("Rotoscoping Load Exception : " + ex);
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
		//---------------------------------------------------
		public void CheckAndLoadExternalImage()
		{
			if(_testImage == null)
			{
				byte[] fileData = null;
				_testImage = new Texture2D(2, 2);
				if(File.Exists(_testPath))
				{
					fileData = File.ReadAllBytes(_testPath);
					_testImage.LoadImage(fileData);
				}
			}
		}


		public ImageSetData AddNewImageSet()
		{
			ImageSetData newSetData = new ImageSetData();

			if(_imageSetDataList == null)
			{
				_imageSetDataList = new List<ImageSetData>();
			}
			_imageSetDataList.Add(newSetData);

			newSetData._name = "New Image Set (" + _imageSetDataList.Count + ")";
			return newSetData;
		}

		public void RemoveImageSet(ImageSetData targetData)
		{
			_imageSetDataList.Remove(targetData);
		}


		// Get / Set
		//---------------------------------------------------
		public Texture2D TestImage { get { return _testImage; } }

	}
}