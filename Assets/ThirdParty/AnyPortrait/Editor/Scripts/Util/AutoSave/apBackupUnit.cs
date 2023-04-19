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
using System.Linq;

namespace AnyPortrait
{
	/// <summary>
	/// 백업시 필드, 멤버 정보가 저장되는 유닛
	/// </summary>
	public class apBackupUnit
	{
		public bool _isRoot = false;
		public bool _isListArrayItem = false;
		public string _typeName_Full = "";
		public string _typeName_Assembly = "";
		public string _typeName_Partial = "";
		public string _fieldName = "";
		public int _itemIndex = -1;
		public object _value = null;

		//private string _strEncoded = null;

		public int _monoInstanceID = -1;
		public string _monoName = "";
		public Vector3 _monoPosition = Vector3.zero;
		public Quaternion _monoQuat = Quaternion.identity;
		public Vector3 _monoScale = Vector3.zero;
		public string _monoAssetPath = "";

		public int _parsedNumChild = 0;

		public enum FIELD_CATEGORY
		{
			Primitive = 1,
			Enum = 2,
			String = 3,
			Vector2 = 4,
			Vector3 = 5,
			Vector4 = 6,
			Color = 7,
			Matrix4x4 = 8,
			Matrix3x3 = 9,
			UnityMonobehaviour = 10,
			UnityGameObject = 11,
			UnityObject = 12,//<< Monobehaviour 제외
			Texture2D = 13,//Texture2D 또는 Texture : 이건 링크를 하는거라 따로 값을 저장
			CustomShader = 14,
			Instance = 15,
			List = 16,
			Array = 17
		}
		public FIELD_CATEGORY _fieldCategory = FIELD_CATEGORY.Primitive;

		//1. List의 컨테이너-Item인 경우
		// Item의 입장에서 List 또는 Array를 참조할 때
		public apBackupUnit _parentContainer = null;

		// List 또는 Array 입장에서 Item을 참조할 때
		public List<apBackupUnit> _childItems = null;


		//2. Instance 타입인 경우
		// Instance의 Field 입장에서의 Instance를 참조할 때
		public apBackupUnit _parentInstance = null;

		// Instance가 자신의 Field를 참조할 때
		public List<apBackupUnit> _childFields = null;

		public int _level = 0;


		//인코딩을 위한 Wrapper를 이용하자
		//불필요한 메모리 누수를 막을 수 있다.
		private static apStringWrapper s_tmpStrWrapper = new apStringWrapper(256);
		private static apStringWrapper s_tmpStrWrapper_Value = new apStringWrapper(256);
		private const string STR_COMMA = ",";
		private const string STR_COMMA_AND_SPACE = ", ";
		private const string STR_EMPTY = "";
		private const string STR_R =  "R";
		private const string STR_I =  "I";
		private const string STR_N =  "N";
		private const string STR_RETURN =  "\n";
		private const string STR_CR =  "\r";
		private const string STR_DOUBLE_BRACKET =  "[]";
		private const string STR_SLASH =  "/";
		private const string STR_REPLACE_SLASH = "<S$#_+_D=#>";//"/"를 가진 String을 만나면 이걸로 변환을 하자.		
		private const string STR_CORRECTED_VERSION = "FIX";

		public apBackupUnit()
		{
			//추가 21.3.6
			if(s_tmpStrWrapper == null)
			{
				s_tmpStrWrapper = new apStringWrapper(256);
			}

			if(s_tmpStrWrapper_Value == null)
			{
				s_tmpStrWrapper_Value = new apStringWrapper(256);
			}
		}

		public void SetRoot()
		{
			_isRoot = true;
			_isListArrayItem = false;
			_level = 0;
		}

		public void SetField(FieldInfo fieldInfo, object value, apBackupUnit parentInstance, apBackupTable table)
		{
			_isRoot = false;
			_isListArrayItem = false;
			_typeName_Full = fieldInfo.FieldType.FullName;
			_typeName_Assembly = fieldInfo.FieldType.Assembly.FullName;

			//이전
			//_typeName_Partial = _typeName_Full + ", " + _typeName_Assembly.Substring(0, _typeName_Assembly.IndexOf(","));

			//변경 21.3.6 : Wrapper 이용
			s_tmpStrWrapper.Clear();
			s_tmpStrWrapper.Append(_typeName_Full, false);
			s_tmpStrWrapper.Append(STR_COMMA_AND_SPACE, false);
			s_tmpStrWrapper.Append(_typeName_Assembly.Substring(0, _typeName_Assembly.IndexOf(',')), true);
			_typeName_Partial = s_tmpStrWrapper.ToString();


			_fieldName = fieldInfo.Name;
			_itemIndex = -1;
			_value = value;

			_level = parentInstance._level + 1;

			System.Type fType = fieldInfo.FieldType;



			if (fType.IsPrimitive)						{ _fieldCategory = FIELD_CATEGORY.Primitive; }
			else if(fType.IsEnum)						{ _fieldCategory = FIELD_CATEGORY.Enum; }
			else if (fType.IsArray)						{ _fieldCategory = FIELD_CATEGORY.Array; }
			else if (fType.IsGenericType)				{ _fieldCategory = FIELD_CATEGORY.List; }
			else if (fType.Equals(typeof(string)))		{ _fieldCategory = FIELD_CATEGORY.String; }
			else if (fType.Equals(typeof(Vector2)))		{ _fieldCategory = FIELD_CATEGORY.Vector2; }
			else if (fType.Equals(typeof(Vector3)))		{ _fieldCategory = FIELD_CATEGORY.Vector3; }
			else if (fType.Equals(typeof(Vector4)))		{ _fieldCategory = FIELD_CATEGORY.Vector4; }
			else if (fType.Equals(typeof(Color)))		{ _fieldCategory = FIELD_CATEGORY.Color; }
			else if (fType.Equals(typeof(Matrix4x4)))	{ _fieldCategory = FIELD_CATEGORY.Matrix4x4; }
			else if (fType.Equals(typeof(apMatrix3x3))) { _fieldCategory = FIELD_CATEGORY.Matrix3x3; }
			else if (fType.Equals(typeof(Texture2D)))	{ _fieldCategory = FIELD_CATEGORY.Texture2D; }
			else if (fType.Equals(typeof(Texture)))		{ _fieldCategory = FIELD_CATEGORY.Texture2D; }//<<Texture도 같이 저장
			else if (fType.Equals(typeof(Shader)))		{ _fieldCategory = FIELD_CATEGORY.CustomShader; }
			else
			{
				if (IsInherited(fType, typeof(UnityEngine.MonoBehaviour)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityMonobehaviour;
				}
				else if (IsInherited(fType, typeof(UnityEngine.GameObject)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityGameObject;
				}
				else if (IsInherited(fType, typeof(UnityEngine.Object)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityObject;
				}
				else
				{
					_fieldCategory = FIELD_CATEGORY.Instance;
				}

			}

			SetParentInstance(parentInstance);

			//추가 : 테이블에 등록한다.
			table.AddFieldName(_fieldName);
			table.AddTypeName(_typeName_Partial);
		}

		public void SetItem(object value, apBackupUnit parentListOrArray, int itemIndex, apBackupTable table)
		{
			_isRoot = false;
			_isListArrayItem = true;
			System.Type valueType = value.GetType();
			_typeName_Full = valueType.FullName;
			_typeName_Assembly = valueType.Assembly.FullName;
			
			//이전
			//_typeName_Partial = _typeName_Full + ", " + _typeName_Assembly.Substring(0, _typeName_Assembly.IndexOf(","));
			
			//변경 21.3.6 : Wrapper 이용
			s_tmpStrWrapper.Clear();
			s_tmpStrWrapper.Append(_typeName_Full, false);
			s_tmpStrWrapper.Append(STR_COMMA_AND_SPACE, false);
			s_tmpStrWrapper.Append(_typeName_Assembly.Substring(0, _typeName_Assembly.IndexOf(',')), true);
			_typeName_Partial = s_tmpStrWrapper.ToString();

			//_fieldName = "";//<<필드명은 없죠..
			_fieldName = STR_EMPTY;//변경 21.3.6
			
			_itemIndex = itemIndex;
			_value = value;

			_level = parentListOrArray._level + 1;

			System.Type vType = value.GetType();

			if (vType.IsPrimitive)						{ _fieldCategory = FIELD_CATEGORY.Primitive; }
			else if (vType.IsEnum)						{ _fieldCategory = FIELD_CATEGORY.Enum; }
			else if (vType.IsArray)						{ _fieldCategory = FIELD_CATEGORY.Array; }//<이게 들어가면 2중 리스트가 된다.
			else if (vType.IsGenericType)				{ _fieldCategory = FIELD_CATEGORY.List; }
			else if (vType.Equals(typeof(string)))		{ _fieldCategory = FIELD_CATEGORY.String; }
			else if (vType.Equals(typeof(Vector2)))		{ _fieldCategory = FIELD_CATEGORY.Vector2; }
			else if (vType.Equals(typeof(Vector3)))		{ _fieldCategory = FIELD_CATEGORY.Vector3; }
			else if (vType.Equals(typeof(Vector4)))		{ _fieldCategory = FIELD_CATEGORY.Vector4; }
			else if (vType.Equals(typeof(Color)))		{ _fieldCategory = FIELD_CATEGORY.Color; }
			else if (vType.Equals(typeof(Matrix4x4)))	{ _fieldCategory = FIELD_CATEGORY.Matrix4x4; }
			else if (vType.Equals(typeof(apMatrix3x3)))	{ _fieldCategory = FIELD_CATEGORY.Matrix3x3; }
			else if (vType.Equals(typeof(Texture2D)))	{ _fieldCategory = FIELD_CATEGORY.Texture2D; }
			else if (vType.Equals(typeof(Texture)))		{ _fieldCategory = FIELD_CATEGORY.Texture2D; }//<<Texture도 같이 저장
			else if (vType.Equals(typeof(Shader)))		{ _fieldCategory = FIELD_CATEGORY.CustomShader; }
			else
			{
				if (IsInherited(vType, typeof(UnityEngine.MonoBehaviour)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityMonobehaviour;
				}
				else if (IsInherited(vType, typeof(UnityEngine.GameObject)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityGameObject;
				}
				else if (IsInherited(vType, typeof(UnityEngine.Object)))
				{
					_fieldCategory = FIELD_CATEGORY.UnityObject;
				}
				else
				{
					_fieldCategory = FIELD_CATEGORY.Instance;
				}
			}

			SetParentListArray(parentListOrArray);

			//추가 : 테이블에 등록한다. (Item의 필드명은 없다)
			table.AddTypeName(_typeName_Partial);
		}


		public static FIELD_CATEGORY GetFieldCategory(System.Type targetType)
		{
			if (targetType.IsPrimitive)						{ return FIELD_CATEGORY.Primitive; }
			else if(targetType.IsEnum)						{ return FIELD_CATEGORY.Enum; }
			else if (targetType.IsArray)						{ return FIELD_CATEGORY.Array; }
			else if (targetType.IsGenericType)				{ return FIELD_CATEGORY.List; }
			else if (targetType.Equals(typeof(string)))		{ return FIELD_CATEGORY.String; }
			else if (targetType.Equals(typeof(Vector2)))		{ return FIELD_CATEGORY.Vector2; }
			else if (targetType.Equals(typeof(Vector3)))		{ return FIELD_CATEGORY.Vector3; }
			else if (targetType.Equals(typeof(Vector4)))		{ return FIELD_CATEGORY.Vector4; }
			else if (targetType.Equals(typeof(Color)))		{ return FIELD_CATEGORY.Color; }
			else if (targetType.Equals(typeof(Matrix4x4)))	{ return FIELD_CATEGORY.Matrix4x4; }
			else if (targetType.Equals(typeof(apMatrix3x3))) { return FIELD_CATEGORY.Matrix3x3; }
			else if (targetType.Equals(typeof(Texture2D)))	{ return FIELD_CATEGORY.Texture2D; }
			else if (targetType.Equals(typeof(Texture)))		{ return FIELD_CATEGORY.Texture2D; }//<<Texture도 같이 저장
			else if (targetType.Equals(typeof(Shader)))		{ return FIELD_CATEGORY.CustomShader; }
			else
			{
				if (IsInherited(targetType, typeof(UnityEngine.MonoBehaviour)))
				{
					return FIELD_CATEGORY.UnityMonobehaviour;
				}
				else if (IsInherited(targetType, typeof(UnityEngine.GameObject)))
				{
					return FIELD_CATEGORY.UnityGameObject;
				}
				else if (IsInherited(targetType, typeof(UnityEngine.Object)))
				{
					return FIELD_CATEGORY.UnityObject;
				}
				else
				{
					return FIELD_CATEGORY.Instance;
				}
			}
		}

		private static bool IsInherited(System.Type targetType, System.Type baseType)
		{
			System.Type curType = targetType;
			int nCount = 0;
			while (true)
			{
				if (nCount > 10)
				{
					//10번 이상 위로 올라갈 수 있나?
					return false;
				}
				if (curType == typeof(object))
				{
					return false;
				}

				if (curType.Equals(baseType))
				{
					//찾았슴다.
					return true;
				}
				if (curType.BaseType == null)
				{
					//위로 올라갈 수 없네요
					return false;
				}
				if (curType.Equals(curType.BaseType))
				{
					//더이상 올라갈 수 없다.
					return false;
				}
				curType = curType.BaseType;
				nCount++;
			}
		}

		private void SetParentListArray(apBackupUnit parentListOrArray)
		{
			_parentContainer = parentListOrArray;
			if (parentListOrArray._childItems == null)
			{
				parentListOrArray._childItems = new List<apBackupUnit>();
			}
			parentListOrArray._childItems.Add(this);
		}

		private void SetParentInstance(apBackupUnit parentInstance)
		{
			_parentInstance = parentInstance;
			if (parentInstance._childFields == null)
			{
				parentInstance._childFields = new List<apBackupUnit>();
			}
			parentInstance._childFields.Add(this);
		}

		#region [미사용 코드] 디버그용 코드이다.
		//public void PrintDebugRecursive()
		//{
		//	string strIndent = "";
		//	for (int i = 0; i < _level; i++)
		//	{
		//		strIndent += "    ";
		//	}
		//	if(_isRoot)
		//	{
		//		Debug.Log(strIndent + ": Root Unit");
		//	}
		//	else if(_isListArrayItem)
		//	{
		//		Debug.Log(strIndent + "[" + _typeName_Partial + " - Item] / (" + _fieldCategory +")");
		//	}
		//	else
		//	{
		//		Debug.Log(strIndent + "[" + _typeName_Partial + " : " + _fieldName + "] / (" + _fieldCategory +")");
		//	}


		//	if(_childFields != null && _childFields.Count > 0)
		//	{
		//		Debug.Log(strIndent + ">> Child Fields ------------------------");
		//		for (int i = 0; i < _childFields.Count; i++)
		//		{
		//			_childFields[i].PrintDebugRecursive();
		//		}
		//		Debug.Log(strIndent + ">>--------------------------------------");
		//	}
		//	else if(_childItems != null && _childItems.Count > 0)
		//	{
		//		Debug.Log(strIndent + ">> Item Fields ------------------------");
		//		for (int i = 0; i < _childItems.Count; i++)
		//		{
		//			_childItems[i].PrintDebugRecursive();
		//		}
		//		Debug.Log(strIndent + ">>--------------------------------------");
		//	}
		//} 
		#endregion

		//------------------------------------------------------------------
		// Encode
		//------------------------------------------------------------------
		public string GetEncodingString(apBackupTable table)
		{
			//수정 -> Table을 이용하여 데이터 자체는 간략하게 한다.
			//[Level:3] [Root/Item/None] [000FieldName-Index] [000Type-Index] [Category (00)] [00000 Value]
			

			//변경 21.3.6 : 인코딩 부분에서 메모리 누수가 없도록 만들자.
			//이전에는 StringBuilder를 매번 만들었다.
			//변경 > 공통의 변수를 이용하자. 
			s_tmpStrWrapper.Clear();

			//System.Text.StringBuilder sb = new System.Text.StringBuilder();
			
			//1.Level:3 입력
			//sb.Append(Int2String(_level, 3));
			s_tmpStrWrapper.Append(Int2String(_level, 3), false);

			//2.Root/Item/Node 입력
			if(_isRoot)
			{
				//sb.Append("R");
				s_tmpStrWrapper.Append(STR_R, false);
			}
			else if(_isListArrayItem)
			{
				//sb.Append("I");
				s_tmpStrWrapper.Append(STR_I, false);
			}
			else
			{
				//sb.Append("N");
				s_tmpStrWrapper.Append(STR_N, false);
			}

			//Root 타입은 여기서 끝
			if(_isRoot)
			{	
				//return sb.ToString();
				s_tmpStrWrapper.MakeString();
				return s_tmpStrWrapper.ToString();
			}

			//FieldName의 Index 버전 입력
			string strFieldNameIndex = null;
			if(_isListArrayItem)
			{
				//List Item이라면 Field 이름이 없다. 인덱스로 지정
				strFieldNameIndex = _itemIndex.ToString();
			}
			else
			{
				strFieldNameIndex = table.GetFieldIndex(_fieldName).ToString();
			}
				
			string strTypeNameIndex = table.GetTypeIndex(_typeName_Partial).ToString();

			//이전
			//sb.Append(Int2String(strFieldNameIndex.Length, 3));
			//sb.Append(strFieldNameIndex);
			
			//변경
			s_tmpStrWrapper.Append(Int2String(strFieldNameIndex.Length, 3), false);
			s_tmpStrWrapper.Append(strFieldNameIndex, false);



			//TypeName의 Index 버전 입력
			//이전
			//sb.Append(Int2String(strTypeNameIndex.Length, 3));
			//sb.Append(strTypeNameIndex);
			//변경
			s_tmpStrWrapper.Append(Int2String(strTypeNameIndex.Length, 3), false);
			s_tmpStrWrapper.Append(strTypeNameIndex, false);

			//Category의 Int형 직접 입력
			//sb.Append(Int2String(((int)_fieldCategory), 2));
			s_tmpStrWrapper.Append(Int2String(((int)_fieldCategory), 2), false);

			//이제 value를 문자열로 입력해야한다.
			//string strValue = "";
			s_tmpStrWrapper_Value.Clear();


			switch (_fieldCategory)
			{
				case FIELD_CATEGORY.Primitive:
					//strValue = _value.ToString();//이전
					s_tmpStrWrapper_Value.Append(_value.ToString(), false);
					break;

				case FIELD_CATEGORY.Enum:
					//strValue = ((int)_value).ToString();
					s_tmpStrWrapper_Value.Append((int)_value, false);
					break;

				case FIELD_CATEGORY.String:
					//이전
					//strValue = _value.ToString();					
					
					//여기서 주의
					//개행 문자는 여기서 바꿔준다.
					//에러가 나도 어쩔 수 없다.
					//strValue = strValue.Replace("\n", "[]");
					//strValue = strValue.Replace("\r", "");

					//변경 21.3.6
					s_tmpStrWrapper_Value.Append(_value.ToString()
													.Replace(STR_RETURN, STR_DOUBLE_BRACKET)
													.Replace(STR_CR, STR_EMPTY),
												false);

					break;

				case FIELD_CATEGORY.Vector2:
					{	
						Vector2 vec2 = (Vector2)_value;
						//,를 구분자로 이용했는데, 이게 특정 환경에서는 소수점이 ,로 사용되면서 파싱 에러를 일으킨다.
						//구분자를 "/"로 변경한다.

						//이전
						//strValue = vec2.x + "," + vec2.y;

						//변경 21.3.6
						s_tmpStrWrapper_Value.Append(vec2.x, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(vec2.y, false);
					}
					break;

				case FIELD_CATEGORY.Vector3:
					{
						Vector3 vec3 = (Vector3)_value;
						//구분자 변경 , > /
						//이전
						//strValue = vec3.x + "," + vec3.y + "," + vec3.z;

						//변경 21.3.6
						s_tmpStrWrapper_Value.Append(vec3.x, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(vec3.y, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(vec3.z, false);
					}

					break;

				case FIELD_CATEGORY.Vector4:
					{
						Vector4 vec4 = (Vector4)_value;
						//구분자 변경 , > /
						//이전
						//strValue = vec4.x + "," + vec4.y + "," + vec4.z + "," + vec4.w;

						//변경 21.3.6
						s_tmpStrWrapper_Value.Append(vec4.x, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(vec4.y, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(vec4.z, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(vec4.w, false);
					}
					break;

				case FIELD_CATEGORY.Color:
					{
						Color color = (Color)_value;
						//구분자 변경 , > /
						//이전
						//strValue = color.r + "," + color.g + "," + color.b + "," + color.a;

						//변경 21.3.6
						s_tmpStrWrapper_Value.Append(color.r, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(color.g, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(color.b, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(color.a, false);
					}

					break;

				case FIELD_CATEGORY.Matrix4x4:
					{
						Matrix4x4 mat4 = (Matrix4x4)_value;

						//구분자 변경 , > /

						//이전
						//strValue =	mat4.m00 + "," + mat4.m10 + "," + mat4.m20 + "," + mat4.m30 + "," +
						//			mat4.m01 + "," + mat4.m11 + "," + mat4.m21 + "," + mat4.m31 + "," +
						//			mat4.m02 + "," + mat4.m12 + "," + mat4.m22 + "," + mat4.m32 + "," +
						//			mat4.m03 + "," + mat4.m13 + "," + mat4.m23 + "," + mat4.m33;

						s_tmpStrWrapper_Value.Append(mat4.m00, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m10, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m20, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m30, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m01, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m11, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m21, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m31, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m02, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m12, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m22, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m32, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m03, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m13, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m23, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat4.m33, false);
					}

					break;

				case FIELD_CATEGORY.Matrix3x3:
					{
						apMatrix3x3 mat3 = (apMatrix3x3)_value;

						//구분자 변경 , > /

						//이전
						//strValue =	mat3._m00 + "," + mat3._m10 + "," + mat3._m20 + "," +
						//			mat3._m01 + "," + mat3._m11 + "," + mat3._m21 + "," +
						//			mat3._m02 + "," + mat3._m12 + "," + mat3._m22 + ",";

						//변경
						s_tmpStrWrapper_Value.Append(mat3._m00, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m10, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m20, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m01, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m11, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m21, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m02, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m12, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(mat3._m22, false);
					}
					break;

				case FIELD_CATEGORY.UnityMonobehaviour:
					{
						MonoBehaviour monoBehaviour = (MonoBehaviour)_value;

						//구분자를 /로 바꾼다.
						//이전
						//strValue =	monoBehaviour.GetInstanceID() + "," +
						//			monoBehaviour.name + "," +
						//			monoBehaviour.transform.position.x + "," + monoBehaviour.transform.position.y + "," + monoBehaviour.transform.position.z + "," +
						//			monoBehaviour.transform.localRotation.x + "," + monoBehaviour.transform.localRotation.y + "," + monoBehaviour.transform.localRotation.z + "," + monoBehaviour.transform.localRotation.w + "," +
						//			monoBehaviour.transform.localScale.x + "," + monoBehaviour.transform.localScale.y + "," + monoBehaviour.transform.localScale.z;

						//변경
						//신버전임을 알리기 위해 앞에 글자를 추가해야한다. (구분자 없음)
						s_tmpStrWrapper_Value.Append(STR_CORRECTED_VERSION, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.GetInstanceID(), false);				s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.name.Replace(STR_SLASH, STR_REPLACE_SLASH), false); s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.position.x, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.position.y, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.position.z, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localRotation.x, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localRotation.y, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localRotation.z, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localRotation.w, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localScale.x, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localScale.y, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(monoBehaviour.transform.localScale.z, false);
						
					}
					break;

				case FIELD_CATEGORY.UnityGameObject:
					{
						GameObject gameObj = (GameObject)_value;

						//구분자 변경 , > /

						//이전
						//strValue =	gameObj.GetInstanceID() + "," +
						//			gameObj.name + "," +
						//			gameObj.transform.position.x + "," + gameObj.transform.position.y + "," + gameObj.transform.position.z + "," +
						//			gameObj.transform.localRotation.x + "," + gameObj.transform.localRotation.y + "," + gameObj.transform.localRotation.z + "," + gameObj.transform.localRotation.w + "," +
						//			gameObj.transform.localScale.x + "," + gameObj.transform.localScale.y + "," + gameObj.transform.localScale.z;

						//변경
						//신버전임을 알리기 위해 앞에 글자를 추가해야한다. (구분자 없음)
						s_tmpStrWrapper_Value.Append(STR_CORRECTED_VERSION, false);
						s_tmpStrWrapper_Value.Append(gameObj.GetInstanceID(), false);			s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.name.Replace(STR_SLASH, STR_REPLACE_SLASH), false); s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.position.x, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.position.y, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.position.z, false);		s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localRotation.x, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localRotation.y, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localRotation.z, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localRotation.w, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localScale.x, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localScale.y, false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
						s_tmpStrWrapper_Value.Append(gameObj.transform.localScale.z, false);
					}
					break;
				case FIELD_CATEGORY.UnityObject:
					{
						UnityEngine.Object uObj = (UnityEngine.Object)_value;

						//이전
						//strValue = uObj.GetInstanceID().ToString();

						//변경
						s_tmpStrWrapper_Value.Append(uObj.GetInstanceID().ToString(), false);
					}
					break;

				case FIELD_CATEGORY.Texture2D:
					{
						Texture2D tex = _value as Texture2D;

						//구분자 변경 , > /
						if (tex == null)
						{
							//이전
							//strValue = "-1, ,  ";
							
							//변경
							//신버전임을 앞에 추가해서 알려준다.
							s_tmpStrWrapper_Value.Append(STR_CORRECTED_VERSION, false);
							s_tmpStrWrapper_Value.Append("-1/ / ", false);
						}
						else
						{
							//이전
							//strValue =	tex.GetInstanceID() + "," +
							//			tex.name + "," +
							//			AssetDatabase.GetAssetPath(tex);

							//변경
							//신버전임을 앞에 추가해서 알려준다.
							s_tmpStrWrapper_Value.Append(STR_CORRECTED_VERSION, false);
							s_tmpStrWrapper_Value.Append(tex.GetInstanceID(), false);								s_tmpStrWrapper_Value.Append(STR_SLASH, false);
							s_tmpStrWrapper_Value.Append(tex.name.Replace(STR_SLASH, STR_REPLACE_SLASH), false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
							s_tmpStrWrapper_Value.Append(AssetDatabase.GetAssetPath(tex).Replace(STR_SLASH, STR_REPLACE_SLASH), false);
						}
					}
					break;

				case FIELD_CATEGORY.CustomShader:
					{
						Shader customShader = _value as Shader;
						if (customShader == null)
						{
							//이전
							//strValue = "-1, , ";

							//변경
							//신버전임을 앞에 추가해서 알려준다.
							s_tmpStrWrapper_Value.Append(STR_CORRECTED_VERSION, false);
							s_tmpStrWrapper_Value.Append("-1/ / ", false);
						}
						else
						{
							//이전
							//strValue =	customShader.GetInstanceID() + "," +
							//			customShader.name + "," +
							//			AssetDatabase.GetAssetPath(customShader);

							//변경
							//신버전임을 앞에 추가해서 알려준다.
							s_tmpStrWrapper_Value.Append(STR_CORRECTED_VERSION, false);
							s_tmpStrWrapper_Value.Append(customShader.GetInstanceID(), false);								s_tmpStrWrapper_Value.Append(STR_SLASH, false);
							s_tmpStrWrapper_Value.Append(customShader.name.Replace(STR_SLASH, STR_REPLACE_SLASH), false);	s_tmpStrWrapper_Value.Append(STR_SLASH, false);
							s_tmpStrWrapper_Value.Append(AssetDatabase.GetAssetPath(customShader).Replace(STR_SLASH, STR_REPLACE_SLASH), false);
						}
					}
					break;

				case FIELD_CATEGORY.Instance:
					{
						//Instance는 하위의 필드 개수를 넣어준다.
						int nChildFields = 0;
						if (_childFields != null)
						{
							nChildFields = _childFields.Count;

						}

						//이전
						//strValue = nChildFields.ToString();

						//변경
						s_tmpStrWrapper_Value.Append(nChildFields, false);
					}
					break;

				case FIELD_CATEGORY.List:
				case FIELD_CATEGORY.Array:
					{
						//Array/List는 개수를 넣어준다.
						int nChildItems = 0;
						if (_childItems != null)
						{
							nChildItems = _childItems.Count;
						}

						//이전
						//strValue = nChildItems.ToString();

						//변경
						s_tmpStrWrapper_Value.Append(nChildItems, false);
					}
					break;
			}

			


			//이전
			//sb.Append(Int2String(strValue.Length, 5));
			//sb.Append(strValue);

			//return sb.ToString();

			//변경

			s_tmpStrWrapper_Value.MakeString();
			string strValueResult = s_tmpStrWrapper_Value.ToString();

			s_tmpStrWrapper.Append(Int2String(strValueResult.Length, 5), false);
			s_tmpStrWrapper.Append(strValueResult, false);
			s_tmpStrWrapper.MakeString();

			return s_tmpStrWrapper.ToString();

			#region [미사용 코드]
			////return _level + "-";
			////구분자대신 {한글자 키워드:길이:값} 방식으로 조합 (레벨 제외)
			////1. Level
			////2. [H] Root / Item / None
			////3. [C] FieldCategory
			////4. [T] TypeName
			////5. [F] FieldName (Item이면 인덱스)
			////6. [V] Value < 여기에 : 가 들어갈 수 있으니 이건 구분자를 받지 않도록 주의
			//string strIndent = "";
			//for (int i = 0; i < _level; i++)
			//{
			//	strIndent += "   ";
			//}
			//if(_isRoot)
			//{
			//	return strIndent + _level + GetEncodingSet("H", "Root");
			//}


			//System.Text.StringBuilder sb = new System.Text.StringBuilder();
			//sb.Append(_level + GetEncodingSet("H", (_isListArrayItem) ? "Item" : "None"));
			//sb.Append(GetEncodingSet("F", (_isListArrayItem) ? _itemIndex.ToString() : _fieldName));
			//sb.Append(GetEncodingSet("T", _typeName_Partial));
			//sb.Append(GetEncodingSet("C", _fieldCategory.ToString()));



			//switch (_fieldCategory)
			//{
			//	case FIELD_CATEGORY.Primitive:
			//		sb.Append(GetEncodingSet("V", _value.ToString()));
			//		break;

			//	case FIELD_CATEGORY.Enum:
			//		sb.Append(GetEncodingSet("V", ((int)_value).ToString()));
			//		break;

			//	case FIELD_CATEGORY.String:
			//		sb.Append(GetEncodingSet("V", _value.ToString()));
			//		break;

			//	case FIELD_CATEGORY.Vector2:
			//		{
			//			Vector2 vec2 = (Vector2)_value;
			//			sb.Append(GetEncodingSet("V", vec2.x + "," + vec2.y));
			//		}
			//		break;

			//	case FIELD_CATEGORY.Vector3:
			//		{
			//			Vector3 vec3 = (Vector3)_value;
			//			sb.Append(GetEncodingSet("V", vec3.x + "," + vec3.y + "," + vec3.z));
			//		}

			//		break;

			//	case FIELD_CATEGORY.Vector4:
			//		{
			//			Vector4 vec4 = (Vector4)_value;
			//			sb.Append(GetEncodingSet("V", vec4.x + "," + vec4.y + "," + vec4.z + "," + vec4.w));
			//		}
			//		break;

			//	case FIELD_CATEGORY.Color:
			//		{
			//			Color color = (Color)_value;
			//			sb.Append(GetEncodingSet("V", color.r + "," + color.g + "," + color.b + "," + color.a));
			//		}

			//		break;

			//	case FIELD_CATEGORY.Matrix4x4:
			//		{
			//			Matrix4x4 mat4 = (Matrix4x4)_value;


			//			sb.Append(GetEncodingSet("V", 
			//				mat4.m00 + "," + mat4.m10 + "," + mat4.m20 + "," + mat4.m30 + "," + 
			//				mat4.m01 + "," + mat4.m11 + "," + mat4.m21 + "," + mat4.m31 + "," + 
			//				mat4.m02 + "," + mat4.m12 + "," + mat4.m22 + "," + mat4.m32 + "," + 
			//				mat4.m03 + "," + mat4.m13 + "," + mat4.m23 + "," + mat4.m33));
			//		}

			//		break;

			//	case FIELD_CATEGORY.Matrix3x3:
			//		{
			//			apMatrix3x3 mat3 = (apMatrix3x3)_value;

			//			sb.Append(GetEncodingSet("V",
			//				mat3._m00 + "," + mat3._m10 + "," + mat3._m20 + "," +
			//				mat3._m01 + "," + mat3._m11 + "," + mat3._m21 + "," +
			//				mat3._m02 + "," + mat3._m12 + "," + mat3._m22 + ","));
			//		}
			//		break;

			//	case FIELD_CATEGORY.UnityMonobehaviour:
			//		{
			//			MonoBehaviour monoBehaviour = (MonoBehaviour)_value;
			//			sb.Append(GetEncodingSet("V",
			//				monoBehaviour.GetInstanceID() + "," +
			//				monoBehaviour.name + "," +
			//				monoBehaviour.transform.position.x + "," + monoBehaviour.transform.position.y + "," + monoBehaviour.transform.position.z + "," +
			//				monoBehaviour.transform.localRotation.x + "," + monoBehaviour.transform.localRotation.y + "," + monoBehaviour.transform.localRotation.z + "," + monoBehaviour.transform.localRotation.w + "," + 
			//				monoBehaviour.transform.localScale.x + "," + monoBehaviour.transform.localScale.y + "," + monoBehaviour.transform.localScale.z
			//				));
			//		}
			//		break;

			//	case FIELD_CATEGORY.UnityGameObject:
			//		{
			//			GameObject gameObj = (GameObject)_value;
			//			sb.Append(GetEncodingSet("V",
			//				gameObj.GetInstanceID() + "," +
			//				gameObj.name + "," +
			//				gameObj.transform.position.x + "," + gameObj.transform.position.y + "," + gameObj.transform.position.z + "," +
			//				gameObj.transform.localRotation.x + "," + gameObj.transform.localRotation.y + "," + gameObj.transform.localRotation.z + "," + gameObj.transform.localRotation.w + "," + 
			//				gameObj.transform.localScale.x + "," + gameObj.transform.localScale.y + "," + gameObj.transform.localScale.z
			//				));
			//		}
			//		break;
			//	case FIELD_CATEGORY.UnityObject:
			//		{
			//			UnityEngine.Object uObj = (UnityEngine.Object)_value;

			//			sb.Append(GetEncodingSet("V", uObj.GetInstanceID().ToString()));
			//		}
			//		break;

			//	case FIELD_CATEGORY.Texture2D:
			//		{
			//			Texture2D tex = _value as Texture2D;
			//			if (tex == null)
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					-1 + "," +
			//					" " + "," +
			//					" "
			//					));
			//			}
			//			else
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					tex.GetInstanceID() + "," +
			//					tex.name + "," +
			//					AssetDatabase.GetAssetPath(tex)
			//					));
			//			}
			//		}
			//		break;

			//	case FIELD_CATEGORY.CustomShader:
			//		{
			//			Shader customShader = _value as Shader;
			//			if (customShader == null)
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					-1 + "," +
			//					" " + "," +
			//					" "
			//					));
			//			}
			//			else
			//			{
			//				sb.Append(GetEncodingSet("V",
			//					customShader.GetInstanceID() + "," +
			//					customShader.name + "," +
			//					AssetDatabase.GetAssetPath(customShader)
			//					));
			//			}
			//		}
			//		break;

			//	case FIELD_CATEGORY.Instance:
			//		{
			//			//Instance는 하위의 필드 개수를 넣어준다.
			//			int nChildFields = 0;
			//			if (_childFields != null)
			//			{
			//				nChildFields = _childFields.Count;

			//			}
			//			sb.Append(GetEncodingSet("V", nChildFields.ToString()));
			//		}
			//		break;

			//	case FIELD_CATEGORY.List:
			//	case FIELD_CATEGORY.Array:
			//		{	
			//			//Array/List는 개수를 넣어준다.
			//			int nChildItems = 0;
			//			if(_childItems != null)
			//			{
			//				nChildItems = _childItems.Count;
			//			}
			//			sb.Append(GetEncodingSet("V", nChildItems.ToString()));
			//		}
			//		break;
			//}


			//return strIndent + sb.ToString(); 
			#endregion
		}

		/// <summary>
		/// int -> string 변환시 자리수를 강제로 맞춘다.
		/// 12 -> 012로 바꾼다.
		/// 자리수는 2, 3, 4, 5를 지원한다.
		/// </summary>
		/// <param name="iValue"></param>
		/// <param name="nCipher"></param>
		/// <returns></returns>
		private string Int2String(int iValue, int nCipher)
		{
			switch (nCipher)
			{
				case 2:
					if(iValue < 10) { return "0" + iValue; }
					else			{ return iValue.ToString(); }
					
				case 3:
					if(iValue < 10)			{ return "00" + iValue; }
					else if(iValue < 100)	{ return "0" + iValue; }
					else					{ return iValue.ToString(); }

				case 4:
					if(iValue < 10)			{ return "000" + iValue; }
					else if(iValue < 100)	{ return "00" + iValue; }
					else if(iValue < 1000)	{ return "0" + iValue; }
					else					{ return iValue.ToString(); }

				case 5:
					if(iValue < 10)			{ return "0000" + iValue; }
					else if(iValue < 100)	{ return "000" + iValue; }
					else if(iValue < 1000)	{ return "00" + iValue; }
					else if(iValue < 10000)	{ return "0" + iValue; }
					else					{ return iValue.ToString(); }
			}

			return iValue.ToString();
			
		}

		private string GetEncodingSet(string strKeyword, string strEncode)
		{
			return "{" + strKeyword + ":" + strEncode.Length + ":" + strEncode + "}\t"; 
		}


		//------------------------------------------------------------------
		// Encode
		//------------------------------------------------------------------
		public bool Decode(string strEncoded, apBackupTable table)
		{
			try
			{

				// Table을 이용한 코드로 바꾸자
				//[Level:3] [R,I,N] [000 - FieldName Index] [000 - Type Index] [Category:2] [00000 Value]

				//[Level:3] [R,I,N]
				string strLevel = strEncoded.Substring(0, 3);
				string strRIN = strEncoded.Substring(3, 1);

				_level = int.Parse(strLevel);
				_isRoot = false;
				_isListArrayItem = false;

				//if(strRIN.Equals("R"))
				if(strRIN.Equals(STR_R))//변경
				{
					_isRoot = true;
				}
				//else if(strRIN.Equals("I"))
				else if(strRIN.Equals(STR_I))//변경
				{
					_isListArrayItem = true;
				}

				if(_isRoot)
				{
					//Root 타입은 여기서 끝
					return true;
				}

				// [000 - FieldName Index]
				int cursor = 4;//<<여기서부터 커서 이동 시작
				int fieldNameLength = int.Parse(strEncoded.Substring(cursor, 3));
				cursor += 3;

				int fieldNameIndex = int.Parse(strEncoded.Substring(cursor, fieldNameLength));
				cursor += fieldNameLength;

				if (_isListArrayItem)
				{
					_itemIndex = fieldNameIndex;
					_fieldName = _itemIndex.ToString();
				}
				else
				{
					_fieldName = table.GetFieldName(fieldNameIndex);//Table에서 필드 이름을 가져오자
				}
				

				// [000 - Type Index]
				int typeNameLength = int.Parse(strEncoded.Substring(cursor, 3));
				cursor += 3;

				int typeNameIndex = int.Parse(strEncoded.Substring(cursor, typeNameLength));
				cursor += typeNameLength;

				_typeName_Partial = table.GetTypeName(typeNameIndex);
				System.Type parseType = table.GetTypeParsed(typeNameIndex);


				// [Category:2]
				_fieldCategory = (FIELD_CATEGORY)int.Parse(strEncoded.Substring(cursor, 2));
				cursor += 2;

				// [00000 Value]
				int valueLength = int.Parse(strEncoded.Substring(cursor, 5));
				cursor += 5;

				_value = null;
				if (valueLength > 0)
				{
					string strValue = strEncoded.Substring(cursor, valueLength);


					//이제 Value 파싱을 하자
					switch (_fieldCategory)
					{
						case FIELD_CATEGORY.Primitive:
							{
								try
								{
									if (parseType.Equals(typeof(bool)))			{ _value = bool.Parse(strValue); }
									else if (parseType.Equals(typeof(int)))		{ _value = int.Parse(strValue); }
									else if (parseType.Equals(typeof(float)))
									{
										//추가 20.9.13 : 실수에 .대신 ,이 들어가는 버그가 있다.
										_value = apUtil.ParseFloat(strValue.Replace(',', '.'));
									}
									else if (parseType.Equals(typeof(double)))
									{
										//추가 20.9.13 : 실수에 .대신 ,이 들어가는 버그가 있다.
										_value = apUtil.ParseDouble(strValue.Replace(',', '.'));
									}
									else if (parseType.Equals(typeof(byte)))	{ _value = byte.Parse(strValue); }
									else if (parseType.Equals(typeof(char)))	{ _value = char.Parse(strValue); }
									else
									{
										Debug.LogError("알 수 없는 Primitive 타입 : " + _typeName_Partial);
										return false;
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Enum:
							{
								_value = int.Parse(strValue);
							}
							break;
						case FIELD_CATEGORY.String:
							{
								_value = strValue.ToString();
							}
							break;
						case FIELD_CATEGORY.Vector2:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//"/"가 있다면 > 개선된 버전으로 디코딩
									//"/"가 없다면 > 이전 버전으로 디코딩

									if (strValue.Contains(STR_SLASH))
									{
										//Debug.Log("신버전 Vector2");

										//개선된 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 2)
										{
											Debug.LogError("Vector2 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다. Replace할 것
										_value = new Vector2(	apUtil.ParseFloat(strValues[0].Replace(',', '.')),
																apUtil.ParseFloat(strValues[1].Replace(',', '.'))
																);
									}
									else
									{
										//이전 버전으로 디코딩 (버그가 있을 수 있다.)
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 2)
										{
											Debug.LogError("Vector2 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										//그 경우엔 데이터가 두배로 보일 것
										bool isFloatCommaBug = false;

										if (strValues.Length >= 4)
										{
											if (!strValues[0].Contains(".")
												&& !strValues[1].Contains(".")
												&& !strValues[2].Contains(".")
												&& !strValues[3].Contains("."))
											{
												//- 개수가 두배이며, . 이 없다.
												isFloatCommaBug = true;
											}
										}

										if (isFloatCommaBug)
										{
											//콤마 버그
											_value = new Vector2(	apUtil.ParseFloat(strValues[0] + "." + strValues[1]),
																	apUtil.ParseFloat(strValues[2] + "." + strValues[3])
																	);
										}
										else
										{
											//일반적인 경우
											_value = new Vector2(	apUtil.ParseFloat(strValues[0]),
																	apUtil.ParseFloat(strValues[1])
																	);
										}
									}


									
									
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.Vector3:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//"/"가 있다면 > 개선된 버전으로 디코딩
									//"/"가 없다면 > 이전 버전으로 디코딩

									if (strValue.Contains(STR_SLASH))
									{
										//Debug.Log("신버전 Vector3");

										//개선된 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 3)
										{
											Debug.LogError("Vector3 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다. Replace할 것
										_value = new Vector3(	apUtil.ParseFloat(strValues[0].Replace(',', '.')),
																apUtil.ParseFloat(strValues[1].Replace(',', '.')),
																apUtil.ParseFloat(strValues[2].Replace(',', '.'))
																);

									}
									else
									{
										//이전 버전으로 디코딩 (버그가 있을 수 있다.)
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 3)
										{
											Debug.LogError("Vector3 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										//그 경우엔 데이터가 두배로 보일 것
										bool isFloatCommaBug = false;

										if (strValues.Length >= 6)
										{
											if (!strValues[0].Contains(".")
												&& !strValues[1].Contains(".")
												&& !strValues[2].Contains(".")
												&& !strValues[3].Contains(".")
												&& !strValues[4].Contains(".")
												&& !strValues[5].Contains(".")
												)
											{
												//- 개수가 두배이며, . 이 없다.
												isFloatCommaBug = true;
											}
										}

										if (isFloatCommaBug)
										{
											//콤마 버그
											_value = new Vector3(	apUtil.ParseFloat(strValues[0] + "." + strValues[1]),
																	apUtil.ParseFloat(strValues[2] + "." + strValues[3]),
																	apUtil.ParseFloat(strValues[4] + "." + strValues[5])
																	);
										}
										else
										{
											//일반적인 경우
											_value = new Vector3(	apUtil.ParseFloat(strValues[0]),
																	apUtil.ParseFloat(strValues[1]),
																	apUtil.ParseFloat(strValues[2])
																	);
										}
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.Vector4:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//"/"가 있다면 > 개선된 버전으로 디코딩
									//"/"가 없다면 > 이전 버전으로 디코딩

									if (strValue.Contains(STR_SLASH))
									{
										//Debug.Log("신버전 Vector4");

										//개선된 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 4)
										{
											Debug.LogError("Vector4 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										_value = new Vector4(	apUtil.ParseFloat(strValues[0].Replace(',', '.')),
																apUtil.ParseFloat(strValues[1].Replace(',', '.')),
																apUtil.ParseFloat(strValues[2].Replace(',', '.')),
																apUtil.ParseFloat(strValues[3].Replace(',', '.'))
																);
									}
									else
									{
										//이전 버전으로 디코딩 (버그가 있을 수 있다.)
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 4)
										{
											Debug.LogError("Vector4 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										//그 경우엔 데이터가 두배로 보일 것
										bool isFloatCommaBug = false;

										if (strValues.Length >= 8)
										{
											if (!strValues[0].Contains(".")
												&& !strValues[1].Contains(".")
												&& !strValues[2].Contains(".")
												&& !strValues[3].Contains(".")
												&& !strValues[4].Contains(".")
												&& !strValues[5].Contains(".")
												&& !strValues[6].Contains(".")
												&& !strValues[7].Contains(".")
												)
											{
												//- 개수가 두배이며, . 이 없다.
												isFloatCommaBug = true;
											}
										}

										if (isFloatCommaBug)
										{
											//콤마 버그
											_value = new Vector4(	apUtil.ParseFloat(strValues[0] + "." + strValues[1]),
																	apUtil.ParseFloat(strValues[2] + "." + strValues[3]),
																	apUtil.ParseFloat(strValues[4] + "." + strValues[5]),
																	apUtil.ParseFloat(strValues[6] + "." + strValues[7])
																	);
										}
										else
										{
											//일반적인 경우
											_value = new Vector4(	apUtil.ParseFloat(strValues[0]),
																	apUtil.ParseFloat(strValues[1]),
																	apUtil.ParseFloat(strValues[2]),
																	apUtil.ParseFloat(strValues[3])
																	);
										}
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.Color:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//"/"가 있다면 > 개선된 버전으로 디코딩
									//"/"가 없다면 > 이전 버전으로 디코딩

									if (strValue.Contains(STR_SLASH))
									{
										//Debug.Log("신버전 Color");

										//개선된 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 4)
										{
											Debug.LogError("Color 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										_value = new Color(	apUtil.ParseFloat(strValues[0].Replace(',', '.')),
															apUtil.ParseFloat(strValues[1].Replace(',', '.')),
															apUtil.ParseFloat(strValues[2].Replace(',', '.')),
															apUtil.ParseFloat(strValues[3].Replace(',', '.'))
															);
									}
									else
									{
										//이전 버전으로 디코딩 (버그가 있을 수 있다.)
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 4)
										{
											Debug.LogError("Color 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										//그 경우엔 데이터가 두배로 보일 것
										bool isFloatCommaBug = false;

										if (strValues.Length >= 8)
										{
											if (!strValues[0].Contains(".")
												&& !strValues[1].Contains(".")
												&& !strValues[2].Contains(".")
												&& !strValues[3].Contains(".")
												&& !strValues[4].Contains(".")
												&& !strValues[5].Contains(".")
												&& !strValues[6].Contains(".")
												&& !strValues[7].Contains(".")
												)
											{
												//- 개수가 두배이며, . 이 없다.
												isFloatCommaBug = true;
											}
										}

										if (isFloatCommaBug)
										{
											//콤마 버그
											_value = new Color(		apUtil.ParseFloat(strValues[0] + "." + strValues[1]),
																	apUtil.ParseFloat(strValues[2] + "." + strValues[3]),
																	apUtil.ParseFloat(strValues[4] + "." + strValues[5]),
																	apUtil.ParseFloat(strValues[6] + "." + strValues[7])
																	);
										}
										else
										{
											//일반적인 경우
											_value = new Color(	apUtil.ParseFloat(strValues[0]),
																apUtil.ParseFloat(strValues[1]),
																apUtil.ParseFloat(strValues[2]),
																apUtil.ParseFloat(strValues[3])
																);
										}
									}
									
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Matrix4x4:
							{
								try
								{//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
								 //"/"가 있다면 > 개선된 버전으로 디코딩
								 //"/"가 없다면 > 이전 버전으로 디코딩

									if (strValue.Contains(STR_SLASH))
									{
										//Debug.Log("신버전 Matrix4x4");

										//개선된 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 16)
										{
											Debug.LogError("Matrix4x4 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										Matrix4x4 mat4 = new Matrix4x4();
										mat4.m00 = apUtil.ParseFloat(strValues[0].Replace(',', '.'));
										mat4.m10 = apUtil.ParseFloat(strValues[1].Replace(',', '.'));
										mat4.m20 = apUtil.ParseFloat(strValues[2].Replace(',', '.'));
										mat4.m30 = apUtil.ParseFloat(strValues[3].Replace(',', '.'));

										mat4.m01 = apUtil.ParseFloat(strValues[4].Replace(',', '.'));
										mat4.m11 = apUtil.ParseFloat(strValues[5].Replace(',', '.'));
										mat4.m21 = apUtil.ParseFloat(strValues[6].Replace(',', '.'));
										mat4.m31 = apUtil.ParseFloat(strValues[7].Replace(',', '.'));

										mat4.m02 = apUtil.ParseFloat(strValues[8].Replace(',', '.'));
										mat4.m12 = apUtil.ParseFloat(strValues[9].Replace(',', '.'));
										mat4.m22 = apUtil.ParseFloat(strValues[10].Replace(',', '.'));
										mat4.m32 = apUtil.ParseFloat(strValues[11].Replace(',', '.'));

										mat4.m03 = apUtil.ParseFloat(strValues[12].Replace(',', '.'));
										mat4.m13 = apUtil.ParseFloat(strValues[13].Replace(',', '.'));
										mat4.m23 = apUtil.ParseFloat(strValues[14].Replace(',', '.'));
										mat4.m33 = apUtil.ParseFloat(strValues[15].Replace(',', '.'));

										_value = mat4;
									}
									else
									{
										//이전 버전으로 디코딩 (버그가 있을 수 있다.)
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 16)
										{
											Debug.LogError("Matrix4x4 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										//그 경우엔 데이터가 두배로 보일 것
										bool isFloatCommaBug = false;

										if (strValues.Length >= 32)
										{
											//- 개수가 두배이며, . 이 없는지 체크
											isFloatCommaBug = true;

											for (int iCheckFloatData = 0; iCheckFloatData < 32; iCheckFloatData++)
											{
												if (strValues[iCheckFloatData].Contains("."))
												{
													//하나라도 .이 있으면 이 버그가 아니다.
													isFloatCommaBug = false;
													break;
												}
											}
										}

										Matrix4x4 mat4 = new Matrix4x4();

										if (isFloatCommaBug)
										{
											//콤마 버그
											mat4.m00 = apUtil.ParseFloat(strValues[0] + "." + strValues[1]);
											mat4.m10 = apUtil.ParseFloat(strValues[2] + "." + strValues[3]);
											mat4.m20 = apUtil.ParseFloat(strValues[4] + "." + strValues[5]);
											mat4.m30 = apUtil.ParseFloat(strValues[6] + "." + strValues[7]);

											mat4.m01 = apUtil.ParseFloat(strValues[8] + "." + strValues[9]);
											mat4.m11 = apUtil.ParseFloat(strValues[10] + "." + strValues[11]);
											mat4.m21 = apUtil.ParseFloat(strValues[12] + "." + strValues[13]);
											mat4.m31 = apUtil.ParseFloat(strValues[14] + "." + strValues[15]);

											mat4.m02 = apUtil.ParseFloat(strValues[16] + "." + strValues[17]);
											mat4.m12 = apUtil.ParseFloat(strValues[18] + "." + strValues[19]);
											mat4.m22 = apUtil.ParseFloat(strValues[20] + "." + strValues[21]);
											mat4.m32 = apUtil.ParseFloat(strValues[22] + "." + strValues[23]);

											mat4.m03 = apUtil.ParseFloat(strValues[24] + "." + strValues[25]);
											mat4.m13 = apUtil.ParseFloat(strValues[26] + "." + strValues[27]);
											mat4.m23 = apUtil.ParseFloat(strValues[28] + "." + strValues[29]);
											mat4.m33 = apUtil.ParseFloat(strValues[30] + "." + strValues[31]);
										}
										else
										{
											//일반적인 경우
											mat4.m00 = apUtil.ParseFloat(strValues[0]);
											mat4.m10 = apUtil.ParseFloat(strValues[1]);
											mat4.m20 = apUtil.ParseFloat(strValues[2]);
											mat4.m30 = apUtil.ParseFloat(strValues[3]);

											mat4.m01 = apUtil.ParseFloat(strValues[4]);
											mat4.m11 = apUtil.ParseFloat(strValues[5]);
											mat4.m21 = apUtil.ParseFloat(strValues[6]);
											mat4.m31 = apUtil.ParseFloat(strValues[7]);

											mat4.m02 = apUtil.ParseFloat(strValues[8]);
											mat4.m12 = apUtil.ParseFloat(strValues[9]);
											mat4.m22 = apUtil.ParseFloat(strValues[10]);
											mat4.m32 = apUtil.ParseFloat(strValues[11]);

											mat4.m03 = apUtil.ParseFloat(strValues[12]);
											mat4.m13 = apUtil.ParseFloat(strValues[13]);
											mat4.m23 = apUtil.ParseFloat(strValues[14]);
											mat4.m33 = apUtil.ParseFloat(strValues[15]);
										}
										_value = mat4;
									}
									
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Matrix3x3:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//"/"가 있다면 > 개선된 버전으로 디코딩
									//"/"가 없다면 > 이전 버전으로 디코딩

									if (strValue.Contains(STR_SLASH))
									{
										//Debug.Log("신버전 Matrix3x3");

										//개선된 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 9)
										{
											Debug.LogError("Matrix3x3 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										apMatrix3x3 mat3 = new apMatrix3x3();
										//일반적인 경우										
										mat3._m00 = apUtil.ParseFloat(strValues[0].Replace(',', '.'));
										mat3._m10 = apUtil.ParseFloat(strValues[1].Replace(',', '.'));
										mat3._m20 = apUtil.ParseFloat(strValues[2].Replace(',', '.'));

										mat3._m01 = apUtil.ParseFloat(strValues[3].Replace(',', '.'));
										mat3._m11 = apUtil.ParseFloat(strValues[4].Replace(',', '.'));
										mat3._m21 = apUtil.ParseFloat(strValues[5].Replace(',', '.'));

										mat3._m02 = apUtil.ParseFloat(strValues[6].Replace(',', '.'));
										mat3._m12 = apUtil.ParseFloat(strValues[7].Replace(',', '.'));
										mat3._m22 = apUtil.ParseFloat(strValues[8].Replace(',', '.'));

										_value = mat3;
										
									}
									else
									{
										//이전 버전으로 디코딩 (버그가 있을 수 있다.)
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 9)
										{
											Debug.LogError("Matrix3x3 파싱 실패 [" + strValue + "]");
											return false;
										}

										//추가 20.9.13 : 실수형이 .대신 ,으로 저장되는 버그가 있다.
										//그 경우엔 데이터가 두배로 보일 것
										bool isFloatCommaBug = false;

										if (strValues.Length >= 18)
										{
											//- 개수가 두배이며, . 이 없는지 체크
											isFloatCommaBug = true;

											for (int iCheckFloatData = 0; iCheckFloatData < 18; iCheckFloatData++)
											{
												if (strValues[iCheckFloatData].Contains("."))
												{
													//하나라도 .이 있으면 이 버그가 아니다.
													isFloatCommaBug = false;
													break;
												}
											}
										}

										apMatrix3x3 mat3 = new apMatrix3x3();

										if (isFloatCommaBug)
										{
											//콤마 버그
											mat3._m00 = apUtil.ParseFloat(strValues[0] + "." + strValues[1]);
											mat3._m10 = apUtil.ParseFloat(strValues[2] + "." + strValues[3]);
											mat3._m20 = apUtil.ParseFloat(strValues[4] + "." + strValues[5]);

											mat3._m01 = apUtil.ParseFloat(strValues[6] + "." + strValues[7]);
											mat3._m11 = apUtil.ParseFloat(strValues[8] + "." + strValues[9]);
											mat3._m21 = apUtil.ParseFloat(strValues[10] + "." + strValues[11]);

											mat3._m02 = apUtil.ParseFloat(strValues[12] + "." + strValues[13]);
											mat3._m12 = apUtil.ParseFloat(strValues[14] + "." + strValues[15]);
											mat3._m22 = apUtil.ParseFloat(strValues[16] + "." + strValues[17]);
										}
										else
										{
											//일반적인 경우										
											mat3._m00 = apUtil.ParseFloat(strValues[0]);
											mat3._m10 = apUtil.ParseFloat(strValues[1]);
											mat3._m20 = apUtil.ParseFloat(strValues[2]);

											mat3._m01 = apUtil.ParseFloat(strValues[3]);
											mat3._m11 = apUtil.ParseFloat(strValues[4]);
											mat3._m21 = apUtil.ParseFloat(strValues[5]);

											mat3._m02 = apUtil.ParseFloat(strValues[6]);
											mat3._m12 = apUtil.ParseFloat(strValues[7]);
											mat3._m22 = apUtil.ParseFloat(strValues[8]);
										}
										_value = mat3;
									}
									
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
						case FIELD_CATEGORY.UnityMonobehaviour:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//STR_CORRECTED_VERSION로 시작된다면 > 개선된 버전으로 디코딩
									//STR_CORRECTED_VERSION로 시작되지 않는다면 > 이전 버전으로 디코딩
									
									if (strValue.StartsWith(STR_CORRECTED_VERSION))
									{
										//Debug.Log("신버전 Monobehaviour");

										//개선된 버전으로 디코딩한다.
										strValue = strValue.Substring(STR_CORRECTED_VERSION.Length);

										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 12)
										{
											Debug.LogError("Unity MonoBehaviour 파싱 실패 [" + strValue + "]");
											return false;
										}

										//Value 대신 Mono 값을 넣자
										//_value = mat3;

										_monoInstanceID = int.Parse(strValues[0]);
										_monoName = strValues[1].ToString().Replace(STR_REPLACE_SLASH, STR_SLASH);
										_monoPosition = new Vector3(	apUtil.ParseFloat(strValues[2].Replace(',', '.')),
																		apUtil.ParseFloat(strValues[3].Replace(',', '.')),
																		apUtil.ParseFloat(strValues[4].Replace(',', '.')));

										_monoQuat = new Quaternion(	apUtil.ParseFloat(strValues[5].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[6].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[7].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[8].Replace(',', '.')));

										_monoScale = new Vector3(	apUtil.ParseFloat(strValues[9].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[10].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[11].Replace(',', '.')));
										_monoAssetPath = "";
									}
									else
									{
										//이전 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 12)
										{
											Debug.LogError("Unity MonoBehaviour 파싱 실패 [" + strValue + "]");
											return false;
										}

										//Value 대신 Mono 값을 넣자
										//_value = mat3;

										_monoInstanceID = int.Parse(strValues[0]);
										_monoName = strValues[1].ToString();
										_monoPosition = new Vector3(	apUtil.ParseFloat(strValues[2]),
																		apUtil.ParseFloat(strValues[3]),
																		apUtil.ParseFloat(strValues[4]));

										_monoQuat = new Quaternion(	apUtil.ParseFloat(strValues[5]),
																	apUtil.ParseFloat(strValues[6]),
																	apUtil.ParseFloat(strValues[7]),
																	apUtil.ParseFloat(strValues[8]));

										_monoScale = new Vector3(	apUtil.ParseFloat(strValues[9]),
																	apUtil.ParseFloat(strValues[10]),
																	apUtil.ParseFloat(strValues[11]));
										_monoAssetPath = "";
									}
									


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.UnityGameObject:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//STR_CORRECTED_VERSION로 시작된다면 > 개선된 버전으로 디코딩
									//STR_CORRECTED_VERSION로 시작되지 않는다면 > 이전 버전으로 디코딩

									if (strValue.StartsWith(STR_CORRECTED_VERSION))
									{
										//Debug.Log("신버전 GameObject");

										//개선된 버전으로 디코딩한다.
										strValue = strValue.Substring(STR_CORRECTED_VERSION.Length);

										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 12)
										{
											Debug.LogError("Unity GameObject 파싱 실패 [" + strValue + "]");
											return false;
										}

										//Value 대신 Mono 값을 넣자
										//_value = mat3;

										_monoInstanceID = int.Parse(strValues[0]);
										_monoName = strValues[1].ToString().Replace(STR_REPLACE_SLASH, STR_SLASH);
										_monoPosition = new Vector3(	apUtil.ParseFloat(strValues[2].Replace(',', '.')),
																		apUtil.ParseFloat(strValues[3].Replace(',', '.')),
																		apUtil.ParseFloat(strValues[4].Replace(',', '.')));

										_monoQuat = new Quaternion(	apUtil.ParseFloat(strValues[5].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[6].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[7].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[8].Replace(',', '.')));

										_monoScale = new Vector3(	apUtil.ParseFloat(strValues[9].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[10].Replace(',', '.')),
																	apUtil.ParseFloat(strValues[11].Replace(',', '.')));
										_monoAssetPath = "";
									}
									else
									{
										//이전 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 12)
										{
											Debug.LogError("Unity GameObject 파싱 실패 [" + strValue + "]");
											return false;
										}

										//Value 대신 Mono 값을 넣자
										//_value = mat3;

										_monoInstanceID = int.Parse(strValues[0]);
										_monoName = strValues[1].ToString();
										_monoPosition = new Vector3(	apUtil.ParseFloat(strValues[2]),
																		apUtil.ParseFloat(strValues[3]),
																		apUtil.ParseFloat(strValues[4]));

										_monoQuat = new Quaternion(	apUtil.ParseFloat(strValues[5]),
																	apUtil.ParseFloat(strValues[6]),
																	apUtil.ParseFloat(strValues[7]),
																	apUtil.ParseFloat(strValues[8]));

										_monoScale = new Vector3(	apUtil.ParseFloat(strValues[9]),
																	apUtil.ParseFloat(strValues[10]),
																	apUtil.ParseFloat(strValues[11]));
										_monoAssetPath = "";
									}
									


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.UnityObject:
							{
								try
								{
									_monoInstanceID = int.Parse(strValue);

									//string[] strValues = strValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
									//if(strValues.Length < 2)
									//{
									//	Debug.LogError("Unity Object 파싱 실패 [" + strValue + "]");
									//	return false;
									//}

									////Value 대신 Mono 값을 넣자
									////_value = mat3;

									//_monoInstanceID = int.Parse(strValues[0]);
									//_monoName = strValues[1].ToString();


								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Texture2D:
						case FIELD_CATEGORY.CustomShader:
							{
								try
								{
									//변경 21.3.6 : 파싱 버그때문에 구분자가 /로 바뀌었다.
									//STR_CORRECTED_VERSION로 시작된다면 > 개선된 버전으로 디코딩
									//STR_CORRECTED_VERSION로 시작되지 않는다면 > 이전 버전으로 디코딩

									if (strValue.StartsWith(STR_CORRECTED_VERSION))
									{
										//Debug.Log("신버전 Texture2D/Shader");
										//개선된 버전으로 디코딩한다.
										strValue = strValue.Substring(STR_CORRECTED_VERSION.Length);

										string[] strValues = strValue.Split(new string[] { STR_SLASH }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 3)
										{
											Debug.LogError("Unity Texture2D/Shader 파싱 실패 [" + strValue + "]");
											return false;
										}

										//Value 대신 Mono 값을 넣자
										//_value = mat3;

										_monoInstanceID =	int.Parse(strValues[0]);
										_monoName =			strValues[1].ToString().Replace(STR_REPLACE_SLASH, STR_SLASH);
										_monoAssetPath =	strValues[2].ToString().Replace(STR_REPLACE_SLASH, STR_SLASH);
									}
									else
									{
										//이전 버전으로 디코딩
										string[] strValues = strValue.Split(new string[] { STR_COMMA }, StringSplitOptions.RemoveEmptyEntries);
										if (strValues.Length < 3)
										{
											Debug.LogError("Unity Texture2D/Shader 파싱 실패 [" + strValue + "]");
											return false;
										}

										//Value 대신 Mono 값을 넣자
										_monoInstanceID = int.Parse(strValues[0]);
										_monoName = strValues[1].ToString();
										_monoAssetPath = strValues[2].ToString();
									}
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;

						case FIELD_CATEGORY.Instance:
						case FIELD_CATEGORY.List:
						case FIELD_CATEGORY.Array:
							{
								try
								{
									_parsedNumChild = int.Parse(strValue);
								}
								catch (Exception exParse)
								{
									Debug.LogError("Value Parse 실패 [" + strValue + "] - " + exParse);
									return false;
								}
							}
							break;
					}

				}

			}
			catch(Exception ex)
			{
				Debug.LogError("Decode Exception [" + strEncoded + "] : " + ex);
				return false;
			}

			return true;
		}
		
	}
}