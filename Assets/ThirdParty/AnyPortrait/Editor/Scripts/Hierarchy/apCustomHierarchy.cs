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
using UnityEditor;

using AnyPortrait;

namespace AnyPortrait
{
	[InitializeOnLoad]
	public class apCustomHierarchy
	{
		private static Texture2D _img_Portrait;

		private static List<int> _targets = null;
		private static bool _isIconDrawLeft = true;
		
		static apCustomHierarchy()
		{
			//Init
			_img_Portrait = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon("Hierarchy/CustomHierarchy_Portrait", false));

			bool isEvent = false;
			//옵션값도 봐야한다.
			bool isCustomHierarchy = EditorPrefs.GetBool("AnyPortrait_ShowCustomHierarchyIcon", true);
			_isIconDrawLeft = EditorPrefs.GetBool("AnyPortrait_ShowCustomIconLeft", true);

			if(_img_Portrait != null && isCustomHierarchy)
			{
				isEvent = true;
			}

			//일단 이벤트에서 제거하고
#if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
#else
			EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
#endif
			EditorApplication.hierarchyWindowItemOnGUI -= OnDrawHierarchyItem;
			
			if(isEvent)
			{
				InitList();

				//옵션에 따라 이벤트 재등록
#if UNITY_2018_1_OR_NEWER
				EditorApplication.hierarchyChanged += OnHierarchyChanged;
#else
				EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
#endif
				EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierarchyItem;
			}
			else
			{
				_targets = null;
			}
		}

		private static void InitList()
		{
			if(_targets == null)
			{
				_targets = new List<int>();
			}
			_targets.Clear();
			
			apPortrait[] portraits = UnityEngine.Object.FindObjectsOfType<apPortrait>();
			
			int nPortraits = portraits != null ? portraits.Length : 0;

			int curID = -1;
			if(nPortraits > 0)
			{
				for (int i = 0; i < nPortraits; i++)
				{
					curID = portraits[i].gameObject.GetInstanceID();
					_targets.Add(curID);
				}
			}
		}
		static void OnHierarchyChanged()
		{
			InitList();

			//Hierarchy가 바뀌었을때도 값을 한번더 체크하자
			bool isCustomHierarchy = EditorPrefs.GetBool("AnyPortrait_ShowCustomHierarchyIcon", true);
			_isIconDrawLeft = EditorPrefs.GetBool("AnyPortrait_ShowCustomIconLeft", true);

			//만약 옵션이 꺼졌다면 더이상 작동 안함
			if(!isCustomHierarchy)
			{
#if UNITY_2018_1_OR_NEWER
				EditorApplication.hierarchyChanged -= OnHierarchyChanged;
#else
				EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
#endif
				EditorApplication.hierarchyWindowItemOnGUI -= OnDrawHierarchyItem;
			}
		}

		static void OnDrawHierarchyItem(int instanceID, Rect selectionRect)
		{
			if(_targets == null 
				|| _targets.Count == 0)
			{
				return;				
			}

			if(!_targets.Contains(instanceID))
			{
				return;
			}
			
			Rect r = new Rect(selectionRect);
			
			if(_isIconDrawLeft)
			{
				//왼쪽
				r.x = 4.0f;
			}
			else
			{
				//오른쪽
				r.x = EditorGUIUtility.currentViewWidth - (r.height + 4);
			}
			
			r.width = r.height;


			//GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			//if(go != null && go.GetComponent<apPortrait>() != null)
			//{
			//	GUI.Label(r, _img_Portrait);
			//}

			GUI.Label(r, _img_Portrait);
		}

		public static void CheckEventOptions()
		{
			bool isEvent = false;
			//옵션값도 봐야한다.
			bool isCustomHierarchy = EditorPrefs.GetBool("AnyPortrait_ShowCustomHierarchyIcon", true);
			_isIconDrawLeft = EditorPrefs.GetBool("AnyPortrait_ShowCustomIconLeft", true);

			if(_img_Portrait != null && isCustomHierarchy)
			{
				isEvent = true;
			}

			//일단 이벤트에서 제거하고
#if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
#else
			EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
#endif
			EditorApplication.hierarchyWindowItemOnGUI -= OnDrawHierarchyItem;
			
			if(isEvent)
			{
				InitList();

				//옵션에 따라 이벤트 재등록
#if UNITY_2018_1_OR_NEWER
				EditorApplication.hierarchyChanged += OnHierarchyChanged;
#else
				EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
#endif
				EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierarchyItem;
			}
			else
			{
				_targets = null;
			}
		}
	}
}