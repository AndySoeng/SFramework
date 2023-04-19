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
using System.Collections.Generic;
using UnityEngine;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Root Object, Key, ID를 입력하면 Object를 리턴해주는 캐시
	/// Editor에서는 데이터가 계속 변하므로 전역이나 멤버로 사용하는 것을 권장하지 않는다.
	/// 로컬 로직에서 반복 호출이 많은 경우 사용하자.
	/// 내부 로직은 Dictionary를 이용한다.
	/// </summary>
	public class apCache<T> where T : UnityEngine.Object
	{
		// Members
		//---------------------------------------------------------------------------------------------------
		//private Dictionary<object, Dictionary<int, Dictionary<int, object>>> _cache = null;
		
		////바로 이전에 찾던 값을 저장하자
		//private bool _isAnyLastSaved = false;

		//private object _lastRootObject = null;
		//private Dictionary<int, Dictionary<int, object>> _lastRootObject_List = null;

		//private int _lastKey = -1;
		//private Dictionary<int, object> _lastKey_List = null;

		//private int _lastID = -1;
		//private object _lastObject = null;
		
		//public const int TYPE_MeshGroup = 1;
		//public const int TYPE_MeshTransform = 2;
		//public const int TYPE_MeshGroupTransform = 3;
		//public const int TYPE_RenderUnit_MeshTF = 4;
		//public const int TYPE_RenderUnit_MeshGroupTF = 5;
		
		//private int _report_PerfectHit = 0;
		//private int _report_SemiHit = 0;
		//private int _report_Miss = 0;

		private Dictionary<int, T> _objects = new Dictionary<int, T>();
		private bool _isAnyAdded = false;
		private T _lastObject = null;
		private int _lastID = -1;

		//---------------------------------------------------------------------------------------------------
		public apCache()
		{
			//if(_cache == null)
			//{
			//	_cache = new Dictionary<object, Dictionary<int, Dictionary<int, object>>>();
			//}
			//Clear();

			if (_objects == null)
			{
				_objects = new Dictionary<int, T>();
			}

			_isAnyAdded = false;
			_lastObject = null;
			_lastID = -1;
		}

		public void Clear()
		{
			//_cache.Clear();

			//_isAnyLastSaved = false;

			//_lastRootObject = null;
			//_lastRootObject_List = null;

			//_lastKey = -1;
			//_lastKey_List = null;

			//_lastID = -1;
			//_lastObject = null;

			//_report_PerfectHit = 0;
			//_report_SemiHit = 0;
			//_report_Miss = 0;
			if (_objects == null)
			{
				_objects = new Dictionary<int, T>();
			}
			_objects.Clear();

			_isAnyAdded = false;
			_lastObject = null;
			_lastID = -1;
		}

		/// <summary>
		/// 해당 ID에 해당하는 값을 가지고 있는가?
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool IsContain(int id)
		{
			if(!_isAnyAdded)
			{
				return false;
			}

			if(_lastID == id)
			{
				return true;
			}

			if(_objects.ContainsKey(id))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// IsContain 함수로 미리 확인해볼것
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public T Get(int id)
		{
			if(!_isAnyAdded)
			{
				return null;
			}

			if(_lastID == id)
			{
				return _lastObject;
			}

			_lastObject = _objects[id];
			_lastID = id;

			return _lastObject;
		}

		/// <summary>
		/// 없었던 오브젝트를 추가. (중복 검사 안하므로 주의)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="targetObject"></param>
		public void Add(int id, T targetObject)
		{
			_isAnyAdded = true;
			_objects.Add(id, targetObject);
		}

		///// <summary>
		///// 캐시에 데이터를 넣는다.
		///// </summary>
		///// <param name="rootObject"></param>
		///// <param name="key"></param>
		///// <param name="ID"></param>
		///// <param name="objData"></param>
		//public void Add(object rootObject, int key, int ID, object objData)
		//{	
		//	if(rootObject == null || objData == null)
		//	{
		//		return;
		//	}

		//	if(!_cache.ContainsKey(rootObject))
		//	{
		//		_cache.Add(rootObject, new Dictionary<int, Dictionary<int, object>>());
		//	}

		//	if(!_cache[rootObject].ContainsKey(key))
		//	{
		//		_cache[rootObject].Add(key, new Dictionary<int, object>());
		//	}

		//	if(!_cache[rootObject][key].ContainsKey(ID))
		//	{
		//		_cache[rootObject][key].Add(ID, objData);
		//	}

		//	_isAnyLastSaved = true;
		//	_lastRootObject = rootObject;
		//	_lastKey = key;
		//	_lastID = ID;
		//	_lastObject = objData;

		//	_lastRootObject_List = _cache[rootObject];
		//	_lastKey_List = _lastRootObject_List[key];
		//}


		//private Dictionary<int, Dictionary<int, object>> _curRootObject_List = null;
		//private Dictionary<int, object> _curKey_List = null;
		//private bool _curCacheHit = true;

		///// <summary>
		///// 캐시에 데이터가 있는지 확인합니다.
		///// </summary>
		///// <param name="rootObject">데이터를 가지고 있는 대상</param>
		///// <param name="key">enum 등으로 데이터의 종류를 나타내는 값</param>
		///// <param name="ID">오브젝트의 UniqueID</param>
		///// <returns></returns>
		//public T Find<T>(object rootObject, int key, int ID) where T : class
		//{
		//	if(_isAnyLastSaved)
		//	{
		//		if(_lastRootObject == rootObject
		//			&& _lastKey == key
		//			&& _lastID == ID)
		//		{
		//			//이전에 조회한 것과 같은걸 조회하네용.
		//			_report_PerfectHit++;
		//			return _lastObject as T;
		//		}
		//	}

		//	_curRootObject_List = null;
		//	_curKey_List = null;
		//	_curCacheHit = true;//<<일단 True로 진행하여, False가 되면 단순 검색을 한다.

		//	if(rootObject == _lastRootObject)
		//	{
		//		_curRootObject_List = _lastRootObject_List;
		//	}
		//	else if(_cache.ContainsKey(rootObject))
		//	{	
		//		_curRootObject_List = _cache[rootObject];
		//		_curCacheHit = false;
		//	}
		//	else
		//	{
		//		//아예 실패
		//		_report_Miss++;
		//		return null;
		//	}

		//	if(_curCacheHit && key == _lastKey)
		//	{
		//		//RootUnit 캐시 검색이 켜진 경우
		//		_curKey_List = _lastKey_List;
		//	}
		//	else if(_curRootObject_List.ContainsKey(key))
		//	{
		//		_curKey_List = _curRootObject_List[key];
		//		_curCacheHit = false;
		//	}
		//	else
		//	{
		//		_report_Miss++;
		//		return null;
		//	}

		//	if(_curCacheHit && _lastID == ID)
		//	{	
		//		_report_PerfectHit++;
		//		return _lastObject as T;
		//	}
		//	else if(_curKey_List.ContainsKey(ID))
		//	{
		//		//캐시 갱신하고 리턴
		//		_isAnyLastSaved = true;
		//		_lastRootObject = rootObject;
		//		_lastKey = key;
		//		_lastID = ID;
		//		_lastObject = _curKey_List[ID];

		//		_lastRootObject_List = _curRootObject_List;
		//		_lastKey_List = _curKey_List;

		//		_report_SemiHit++;
		//		return _lastObject as T;
		//	}

		//	_report_Miss++;
		//	return null;

			
		//}

		//public void PrintReport()
		//{
		//	Debug.LogError("Cache Report >>\n - Perfect Hit : " + _report_PerfectHit + "\n - Semit Hit : " + _report_SemiHit + "\n - Miss : " + _report_Miss);
		//}
		

	}



}