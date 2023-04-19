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
	/// Backup을 저장/로드할 때, 타입명과 변수명은 반복되고 긴 편이다.
	/// 이를 테이블로 만들어서 "타입/변수명" - 인덱스로 교환한다.
	/// </summary>
	public class apBackupTable
	{
		// Members
		//-----------------------------------------------------------
		private class TypePreset
		{
			public int _index = -1;
			public string _typeName = null;

			public TypePreset(int index, string typeName)
			{
				_index = index;
				_typeName = typeName;
			}
		}

		private class FieldPreset
		{
			public int _index = -1;
			public string _fieldName = null;
			public FieldPreset(int index, string fieldName)
			{
				_index = index;
				_fieldName = fieldName;
			}
		}

		// Index <-> Type/Field Preset을 빠르게 상호 참조할 수 있게 두개의 Dictionary를 둔다.
		private Dictionary<string, TypePreset> _typePreset_ByName = new Dictionary<string, TypePreset>();
		private Dictionary<int, TypePreset> _typePreset_ByIndex = new Dictionary<int, TypePreset>();

		private Dictionary<string, FieldPreset> _fieldPreset_ByName = new Dictionary<string, FieldPreset>();
		private Dictionary<int, FieldPreset> _fieldPreset_ByIndex = new Dictionary<int, FieldPreset>();

		private Dictionary<string, System.Type> _parsedTypes = new Dictionary<string, System.Type>();

		private int _nextIndex_Type = 0;
		private int _nextIndex_Field = 0;

		//파일 파싱용
		private struct ParseUnit
		{
			public int index;
			public string strValue;
		}

		private ParseUnit _parseUnit;
		

		// Init
		//-----------------------------------------------------------
		public apBackupTable()
		{
			Clear();
		}


		public void Clear()
		{
			_typePreset_ByName.Clear();
			_typePreset_ByIndex.Clear();
			_fieldPreset_ByName.Clear();
			_fieldPreset_ByIndex.Clear();

			_nextIndex_Type = 0;
			_nextIndex_Field = 0;

			_parsedTypes.Clear();
		}



		// Set
		//-----------------------------------------------------------
		// 저장할때의 함수. BackupUnit에서 값을 받자
		public int AddTypeName(string typeName)
		{
			TypePreset typePreset = GetTypePreset(typeName);
			if(typePreset == null)
			{
				TypePreset newPreset = new TypePreset(_nextIndex_Type, typeName);
				_typePreset_ByName.Add(typeName, newPreset);
				_typePreset_ByIndex.Add(_nextIndex_Type, newPreset);

				_nextIndex_Type++;

				return newPreset._index;
			}

			return typePreset._index;
		}

		public int AddFieldName(string fieldName)
		{
			FieldPreset fieldPreset = GetFieldPreset(fieldName);
			if(fieldPreset == null)
			{
				FieldPreset newPreset = new FieldPreset(_nextIndex_Field, fieldName);
				_fieldPreset_ByName.Add(fieldName, newPreset);
				_fieldPreset_ByIndex.Add(_nextIndex_Field, newPreset);

				_nextIndex_Field++;

				return newPreset._index;
			}

			return fieldPreset._index;
		}

		// 로드할 때의 함수. 저장된 txt 파일에서 파싱하자
		public void ParseType(string typeName, int index)
		{
			TypePreset typePreset = new TypePreset(index, typeName);
			_typePreset_ByName.Add(typeName, typePreset);
			_typePreset_ByIndex.Add(index, typePreset);
		}

		public void ParseField(string fieldName, int index)
		{
			FieldPreset fieldPreset = new FieldPreset(index, fieldName);
			_fieldPreset_ByName.Add(fieldName, fieldPreset);
			_fieldPreset_ByIndex.Add(index, fieldPreset);
		}

		// Get (Name, Index 각각)
		public int GetTypeIndex(string typeName)
		{
			TypePreset typePreset = GetTypePreset(typeName);

			if(typePreset == null)
			{
				Debug.LogError("GetTypeIndex 실패 [" + typeName + "]");
				return -1;
			}
			return typePreset._index;
		}

		public string GetTypeName(int index)
		{
			TypePreset typePreset = GetTypePreset(index);

			if(typePreset == null)
			{
				Debug.LogError("GetTypeName 실패 [" + index + "]");
				return null;
			}
			return typePreset._typeName;
		}

		public System.Type GetTypeParsed(int index)
		{
			string typeName = GetTypeName(index);
			if(typeName == null)
			{
				Debug.LogError("GetTypeParsed 실패 [" + index + "]");
				return null;
			}
			if(_parsedTypes.ContainsKey(typeName))
			{
				return _parsedTypes[typeName];
			}
			_parsedTypes.Add(typeName, System.Type.GetType(typeName));
			return _parsedTypes[typeName];
		}


		public int GetFieldIndex(string fieldName)
		{
			FieldPreset fieldPreset = GetFieldPreset(fieldName);
			if(fieldPreset == null)
			{
				Debug.LogError("GetFieldIndex 실패 [" + fieldName + "]");
				return -1;
			}
			return fieldPreset._index;
		}

		public string GetFieldName(int fieldIndex)
		{
			FieldPreset fieldPreset = GetFieldPreset(fieldIndex);
			if(fieldPreset == null)
			{
				Debug.LogError("GetFieldName 실패 [" + fieldIndex + "]");
				return null;
			}
			return fieldPreset._fieldName;
		}



		// Get
		//-----------------------------------------------------------
		private TypePreset GetTypePreset(string typeName)
		{
			if(!_typePreset_ByName.ContainsKey(typeName))
			{
				return null;
			}
			return _typePreset_ByName[typeName];
		} 
		private TypePreset GetTypePreset(int index)
		{
			if(!_typePreset_ByIndex.ContainsKey(index))
			{
				return null;
			}
			return _typePreset_ByIndex[index];
		}

		private FieldPreset GetFieldPreset(string fieldName)
		{
			if(!_fieldPreset_ByName.ContainsKey(fieldName))
			{
				return null;
			}
			return _fieldPreset_ByName[fieldName];
		}
		private FieldPreset GetFieldPreset(int index)
		{
			if(!_fieldPreset_ByIndex.ContainsKey(index))
			{
				return null;
			}
			return _fieldPreset_ByIndex[index];
		}


		// File Write / Read
		//----------------------------------------------------------------------
		public void FileWrite(StreamWriter sw)
		{
			sw.WriteLine("------------------------------------------------");
			sw.WriteLine(_typePreset_ByName.Count.ToString());
			sw.WriteLine(_fieldPreset_ByName.Count.ToString());
			foreach (KeyValuePair<string, TypePreset> preset in _typePreset_ByName)
			{
				sw.WriteLine(preset.Value._index + ":" + preset.Value._typeName);
			}
			foreach (KeyValuePair<string, FieldPreset> preset in _fieldPreset_ByName)
			{
				sw.WriteLine(preset.Value._index + ":" + preset.Value._fieldName);
			}
			sw.WriteLine("------------------------------------------------");
		}

		public bool FileRead(StreamReader sr)
		{
			Clear();

			string curText = "";
			try
			{
				curText = sr.ReadLine(); // "-----"
				if(curText.Contains("-"))
				{
					//라인이 입력되었다.
					//한줄 더 읽자
					curText = sr.ReadLine();
				}
				int nTypes = int.Parse(curText);

				curText = sr.ReadLine();
				int nFields = int.Parse(curText);

				for (int i = 0; i < nTypes; i++)
				{
					ParseIndexString(sr.ReadLine());
					TypePreset typePreset = new TypePreset(_parseUnit.index, _parseUnit.strValue);
					_typePreset_ByIndex.Add(typePreset._index, typePreset);
					_typePreset_ByName.Add(typePreset._typeName, typePreset);

					_parsedTypes.Add(typePreset._typeName, System.Type.GetType(typePreset._typeName));
				}

				for (int i = 0; i < nFields; i++)
				{
					ParseIndexString(sr.ReadLine());
					FieldPreset fieldPreset = new FieldPreset(_parseUnit.index, _parseUnit.strValue);
					_fieldPreset_ByIndex.Add(fieldPreset._index, fieldPreset);
					_fieldPreset_ByName.Add(fieldPreset._fieldName, fieldPreset);
				}

				sr.ReadLine(); // "-----"
			}
			catch (Exception ex)
			{
				Debug.LogError("Backup Table FileRead Exception : " + ex);
				return false;
			}

			return true;
		}


		private void ParseIndexString(string strRead)
		{
			int iDelimeter = strRead.IndexOf(":");
			_parseUnit.index = int.Parse(strRead.Substring(0, iDelimeter));
			if(iDelimeter < strRead.Length - 1)
			{
				_parseUnit.strValue = strRead.Substring(iDelimeter + 1);
			}
			else
			{
				_parseUnit.strValue = "";
			}
			

		}

		
	}
}
