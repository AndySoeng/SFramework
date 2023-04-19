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

using AnyPortrait;
using System.Runtime.InteropServices;

namespace AnyPortrait
{

	public static class apUtil
	{

		public static List<T> ResizeList<T>(List<T> srcList, int resizeSize)
		{
			if (resizeSize < 0)
			{
				return null;
			}
			List<T> resultList = new List<T>();
			for (int i = 0; i < resizeSize; i++)
			{
				if (i < srcList.Count)
				{
					resultList.Add(srcList[i]);
				}
				else
				{
					resultList.Add(default(T));
				}
			}

			return resultList;

		}

		// 색상 처리
		//------------------------------------------------------------------------------------------
		public static Color BlendColor_ITP(Color prevResult, Color nextResult, float nextWeight)
		{
			return (prevResult * (1.0f - nextWeight)) + (nextResult * nextWeight);
		}

		//public static Vector3 _color_2XTmp_Prev = new Vector3(0, 0, 0);
		//public static Vector3 _color_2XTmp_Next = new Vector3(0, 0, 0);

		public static Color BlendColor_Add(Color prevResult, Color nextResult, float nextWeight)
		{
			//_color_2XTmp_Prev.x = (float)(prevResult.r);
			//_color_2XTmp_Prev.y = (float)(prevResult.g);
			//_color_2XTmp_Prev.z = (float)(prevResult.b);

			//_color_2XTmp_Next.x = (float)(nextResult.r);
			//_color_2XTmp_Next.y = (float)(nextResult.g);
			//_color_2XTmp_Next.z = (float)(nextResult.b);

			//_color_2XTmp_Prev += (_color_2XTmp_Next * nextWeight);
			//_color_2XTmp_Next = _color_2XTmp_Prev * (1.0f - nextWeight) + ((_color_2XTmp_Prev + _color_2XTmp_Next) * nextWeight);



			//return new Color(	Mathf.Clamp01(_color_2XTmp_Prev.x + 0.5f),
			//					Mathf.Clamp01(_color_2XTmp_Prev.y + 0.5f),
			//					Mathf.Clamp01(_color_2XTmp_Prev.z + 0.5f),
			//					//Mathf.Clamp01(prevResult.a + (nextResult.a * nextWeight))
			//					Mathf.Clamp01(prevResult.a * (1.0f - nextWeight) + (prevResult.a * nextResult.a) * nextWeight)
			//				);

			//return new Color(	Mathf.Clamp01(_color_2XTmp_Next.x),
			//					Mathf.Clamp01(_color_2XTmp_Next.y),
			//					Mathf.Clamp01(_color_2XTmp_Next.z),
			//					//Mathf.Clamp01(prevResult.a + (nextResult.a * nextWeight))
			//					Mathf.Clamp01(prevResult.a * (1.0f - nextWeight) + (prevResult.a * nextResult.a) * nextWeight)
			//				);

			//return prevResult + (nextResult * nextWeight);

			nextResult.r = prevResult.r * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.r + nextResult.r - 0.5f) * nextWeight);
			nextResult.g = prevResult.g * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.g + nextResult.g - 0.5f) * nextWeight);
			nextResult.b = prevResult.b * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.b + nextResult.b - 0.5f) * nextWeight);
			//nextResult.a = prevResult.a * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.a + nextResult.a - 0.5f) * nextWeight);
			nextResult.a = prevResult.a * (1.0f - nextWeight) + (Mathf.Clamp01(prevResult.a * nextResult.a) * nextWeight);//Alpha는 Multiply 연산



			return nextResult;
		}


		//--------------------------------------------------------------------------------------------
		public static float AngleTo180(float angle)
		{
			while(angle > 180.0f)
			{
				angle -= 360.0f;
			}
			while(angle < -180.0f)
			{
				angle += 360.0f;
			}
			return angle;
		}

		public static float AngleTo360(float angle)
		{
			while(angle > 360.0f)
			{
				angle -= 360.0f;
			}
			while(angle < -360.0f)
			{
				angle += 360.0f;
			}
			return angle;
		}

		public static float AngleTo360Positive(float angle)
		{
			while(angle > 360.0f)
			{
				angle -= 360.0f;
			}
			while(angle < 0.0f)
			{
				angle += 360.0f;
			}
			return angle;
		}

		/// <summary>
		/// +- 180도 즈음에서 각도 Min-Max Clamp를 비교하기가 어렵다.
		/// 범위를 보정하여 Clamp를 한다.
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="minAngle"></param>
		/// <param name="maxAngle"></param>
		/// <returns></returns>
		public static float AngleClamp360(float angle, float minAngle, float maxAngle)
		{
			//return Mathf.Clamp(angle, minAngle, maxAngle);

			//입력된 각도에 맞게 minAngle, maxAngle을 고친다.
			if (minAngle < angle && angle < maxAngle)
			{
				//이미 Clamp 범위 안에 들어갔다.
				return angle;
			}

			if (maxAngle > angle + 360.0f)
			{

				//범위가 +360 더 돌아있다.
				minAngle -= 360.0f;
				maxAngle -= 360.0f;
				//Debug.Log("Clamp 범위 보정 : " + angle + " / (" + (minAngle + 360.0f) + " ~ " + (maxAngle + 360.0f) + ") >> (" + minAngle + " ~ " + maxAngle + ")");

			}
			else if (minAngle < angle - 360.0f)
			{
				//범위가 -360 더 돌아있다
				minAngle += 360.0f;
				maxAngle += 360.0f;

				//Debug.Log("Clamp 범위 보정 : " + angle + " / (" + (minAngle - 360.0f) + " ~ " + (maxAngle - 360.0f) + ") >> (" + minAngle + " ~ " + maxAngle + ")");
			}

			//범위가 보정되었으므로 다시 Clamp
			return Mathf.Clamp(angle, minAngle, maxAngle);
		}


		/// <summary>
		/// SrcAngle의 +0, +360, -360의 값 중에서 TargetAngle과 가장 비슷한 각도를 리턴한다.
		/// 360도로 인하여 실제로 가까운 회전값을 찾기 위함
		/// </summary>
		/// <param name="srcAngle"></param>
		/// <param name="targetAngle"></param>
		/// <returns></returns>
		public static float GetNearestLoopedAngle360(float srcAngle, float targetAngle)
		{
			float deltaAngle_0 = Mathf.Abs(srcAngle - targetAngle);
			float deltaAngle_P360 = Mathf.Abs((srcAngle + 360) - targetAngle);
			float deltaAngle_N360 = Mathf.Abs((srcAngle - 360) - targetAngle);

			if(deltaAngle_0 < deltaAngle_P360)
			{
				//deltaAngle_0 < deltaAngle_P360
				if(deltaAngle_0 < deltaAngle_N360)
				{
					//deltaAngle_0 >> 원본
					return srcAngle;
				}
				else
				{
					//deltaAngle_N360 >> -360
					return srcAngle - 360;
				}
			}
			else
			{
				//deltaAngle_P360 < deltaAngle_0
				if(deltaAngle_P360 < deltaAngle_N360)
				{
					//deltaAngle_P360 >> +360
					return srcAngle + 360;
				}
				else
				{
					//deltaAngle_N360 >> -360
					return srcAngle - 360;
				}
			}

		}

		/// <summary>
		/// 360 좌표계의 원형 보간을 구한다. Weight가 0이면 angleA, Weight가 1이면 angleB가 리턴된다.
		/// 리턴되는 값은 +- 180 이내의 값으로 변환된다.
		/// </summary>
		/// <param name="angleA"></param>
		/// <param name="angleB"></param>
		/// <param name="weight"></param>
		/// <returns></returns>
		public static float AngleSlerp(float angleA, float angleB, float weight)
		{
			angleA = AngleTo180(angleA);
			angleB = AngleTo180(angleB);
			if(angleA > angleB)
			{
				if(angleA > angleB + 180)
				{
					angleB += 360;
				}
			}
			else
			{
				if(angleB > angleA + 180)
				{
					angleA += 360;
				}
			}

			return AngleTo180(angleA * (1.0f - weight) + angleB * weight);
		}


		//---------------------------------------------------------------------------------------------------
		//문자열 압축
		public static string GetShortString(string strSrc, int length)
		{
			if(string.IsNullOrEmpty(strSrc))
			{
				return "";
			}

			if(strSrc.Length > length)
			{
				return strSrc.Substring(0, length) + "..";
			}
			return strSrc;
		}


		/// <summary>
		/// 문자열 길이를 리턴한다. 조금 다를 순 있으니 약간의 여백을 고려하자 (GUI용 / 영어 위주)
		/// </summary>
		/// <param name="strSrc"></param>
		/// <returns></returns>
		public static int GetStringRealLength(string strSrc)
		{	
			if(string.IsNullOrEmpty(strSrc))
			{
				return 0;
			}

			int nSrc = strSrc.Length;
			int result = 0;
			char curChar = '0';

			for (int i = 0; i < nSrc; i++)
			{
				curChar = strSrc[i];
				switch (curChar)
				{
					case 'A': result += 9; break;
					case 'B': result += 8; break;
					case 'C': result += 9; break;
					case 'D': result += 9; break;
					case 'E': result += 7; break;
					case 'F': result += 6; break;
					case 'G': result += 9; break;
					case 'H': result += 8; break;
					case 'I': result += 4; break;
					case 'J': result += 6; break;
					case 'K': result += 8; break;
					case 'L': result += 6; break;
					case 'M': result += 9; break;
					case 'N': result += 8; break;
					case 'O': result += 9; break;
					case 'P': result += 7; break;
					case 'Q': result += 9; break;
					case 'R': result += 8; break;
					case 'S': result += 7; break;
					case 'T': result += 8; break;
					case 'U': result += 7; break;
					case 'V': result += 9; break;
					case 'W': result += 10; break;
					case 'X': result += 8; break;
					case 'Y': result += 8; break;
					case 'Z': result += 8; break;


					case 'a': result += 7; break;
					case 'b': result += 7; break;
					case 'c': result += 6; break;
					case 'd': result += 7; break;
					case 'e': result += 6; break;
					case 'f': result += 5; break;
					case 'g': result += 7; break;
					case 'h': result += 7; break;
					case 'i': result += 2; break;
					case 'j': result += 5; break;
					case 'k': result += 7; break;
					case 'l': result += 3; break;
					case 'm': result += 11; break;
					case 'n': result += 7; break;
					case 'o': result += 7; break;
					case 'p': result += 7; break;
					case 'q': result += 7; break;
					case 'r': result += 5; break;
					case 's': result += 5; break;
					case 't': result += 5; break;
					case 'u': result += 7; break;
					case 'v': result += 7; break;
					case 'w': result += 9; break;
					case 'x': result += 7; break;
					case 'y': result += 7; break;
					case 'z': result += 6; break;


					case '1': result += 7; break;
					case '2': result += 7; break;
					case '3': result += 6; break;
					case '4': result += 8; break;
					case '5': result += 7; break;
					case '6': result += 7; break;
					case '7': result += 7; break;
					case '8': result += 7; break;
					case '9': result += 7; break;
					case '0': result += 7; break;
					case '(': result += 5; break;
					case ')': result += 5; break;
					case '[': result += 5; break;
					case ']': result += 5; break;
					case ' ': result += 4; break;
					case '/': result += 6; break;
					case '+': result += 9; break;
					case '-': result += 6; break;
					case '!': result += 3; break;
					case ':': result += 3; break;
					case '?': result += 7; break;
					case '.': result += 4; break;
					case ',': result += 4; break; 
					case ';': result += 4; break; 
					case '>': result += 8; break;
					case '<': result += 8; break;

					default: result += 7; break;
				}
			}

			return result;
		}




		// 추가 21.7.3 : URI 관련 에러 (인식 안되는 아스키값 변경)
		/// <summary>
		/// 이스케이프 문자를 일반 문자로 변환시킨다.
		/// </summary>
		/// <param name="srcURI"></param>
		/// <returns></returns>
		public static string ConvertEscapeToPlainText(string srcURI)
		{
			if(string.IsNullOrEmpty(srcURI))
			{
				return "";
			}
			//HTTP API를 사용할 수 없으므로
			//중요한 문자만 변환하자
			srcURI = srcURI.Replace("%20", " ");//공백
			srcURI = srcURI.Replace("%21", "!");			
			srcURI = srcURI.Replace("%22", "\"");
			srcURI = srcURI.Replace("%23", "#");
			srcURI = srcURI.Replace("%24", "$");
			srcURI = srcURI.Replace("%25", "%");
			srcURI = srcURI.Replace("%26", "&");
			srcURI = srcURI.Replace("%27", "'");

			srcURI = srcURI.Replace("%28", "(");
			srcURI = srcURI.Replace("%29", ")");
			srcURI = srcURI.Replace("%2A", "*");
			srcURI = srcURI.Replace("%2B", "+");
			srcURI = srcURI.Replace("%2C", ",");
			srcURI = srcURI.Replace("%2D", "-");
			srcURI = srcURI.Replace("%2E", ".");
			srcURI = srcURI.Replace("%2F", "/");

			srcURI = srcURI.Replace("%3A", ":");
			srcURI = srcURI.Replace("%3B", ";");
			srcURI = srcURI.Replace("%3C", "<");
			srcURI = srcURI.Replace("%3D", "=");
			srcURI = srcURI.Replace("%3E", ">");
			srcURI = srcURI.Replace("%3F", "?");
			srcURI = srcURI.Replace("%40", "@");

			srcURI = srcURI.Replace("%5B", "[");
			srcURI = srcURI.Replace("%5C", "\\");
			srcURI = srcURI.Replace("%5D", "]");
			srcURI = srcURI.Replace("%5E", "^");
			srcURI = srcURI.Replace("%5F", "_");

			srcURI = srcURI.Replace("%60", "`");
			srcURI = srcURI.Replace("%7B", "{");
			srcURI = srcURI.Replace("%7C", "|");
			srcURI = srcURI.Replace("%7D", "}");
			srcURI = srcURI.Replace("%7E", "~");
			
			return srcURI;
		}


		//파싱 (추가 21.7.11)
		//프랑스어 등의 문제로 쉼표(,)가 소수점인 경우가 있다.
		//--------------------------------------------------------------------------------------------
		private const char PARSE_DOT = '.';
		private const char PARSE_COMMA = ',';
		private static System.Globalization.CultureInfo s_cultureInfo = null;
		public static float ParseFloat(string strSrc)
		{
			//문화권은 영어로 통일한다. (점(.)을 소수점으로 인식한다.)
			if(s_cultureInfo == null)
			{
				s_cultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en-us");
			}
			return float.Parse(strSrc.Replace(PARSE_COMMA, PARSE_DOT), s_cultureInfo);
			//return float.Parse(strSrc.Replace(PARSE_COMMA, PARSE_DOT));
		}

		public static double ParseDouble(string strSrc)
		{
			//문화권은 영어로 통일한다. (점(.)을 소수점으로 인식한다.)
			if(s_cultureInfo == null)
			{
				s_cultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en-us");
			}
			return double.Parse(strSrc.Replace(PARSE_COMMA, PARSE_DOT), s_cultureInfo);
			//return double.Parse(strSrc.Replace(PARSE_COMMA, PARSE_DOT));
		}

		//public static double ParseDouble(string strSrc)
		//{
		//	return double.Parse(strSrc.Replace(PARSE_COMMA, PARSE_DOT));
		//}




		//추가 20.4.3 : 갱신 요청에 관련된 변수를 별도로 만든다.
		//---------------------------------------------------------------------------------------------------
		//public enum LINK_REFRESH_REQUEST_TYPE
		//{
		//	AllObjects,
		//	MeshGroup_AllAnimMods,
		//	MeshGroup_ExceptAnimMods,
		//	AnimClip,
		//}

		public enum LR_REQUEST__MESHGROUP
		{
			/// <summary>메시 그룹에 상관 없음 또는 모든 메시 그룹. 단 메시 그룹을 입력하면 RenderUnit을 갱신한다.</summary>
			AllMeshGroups,
			/// <summary>선택된 메시 그룹 1개만</summary>
			SelectedMeshGroup,
		}

		public enum LR_REQUEST__MODIFIER
		{
			/// <summary>모든 모디파이어</summary>
			AllModifiers,
			/// <summary>선택된 모디파이어만</summary>
			SelectedModifier,
			/// <summary>애니메이션 모디파이어는 제외. (메시 그룹 메뉴용)</summary>
			AllModifiers_ExceptAnimMods,
		}

		public enum LR_REQUEST__PSG
		{
			/// <summary>모든 ParamSetGroup들</summary>
			AllPSGs,
			/// <summary>
			/// (애니메이션 모디파이어인 경우) 선택된 애니메이션 클립에 대한 PSG만. 
			/// 그 외의 모디파이어는 모든 PSG를 대상 (애니메이션 메뉴용)
			/// </summary>
			SelectedAnimClipPSG_IfAnimModifier
		}

		public class LinkRefreshRequest
		{
			//Members
			private LR_REQUEST__MESHGROUP _request_MeshGroup = LR_REQUEST__MESHGROUP.AllMeshGroups;
			private apMeshGroup _meshGroup = null;

			private LR_REQUEST__MODIFIER _request_Modifier = LR_REQUEST__MODIFIER.AllModifiers;
			private apModifierBase _modifier = null;

			private LR_REQUEST__PSG _request_PSG = LR_REQUEST__PSG.AllPSGs;
			private apAnimClip _animClip = null;

			//Get
			public LR_REQUEST__MESHGROUP Request_MeshGroup { get { return _request_MeshGroup; } }
			public apMeshGroup MeshGroup { get { return _meshGroup; } }

			public LR_REQUEST__MODIFIER Request_Modifier { get { return _request_Modifier; } }
			public apModifierBase Modifier { get { return _modifier; } }

			public LR_REQUEST__PSG Request_PSG {  get { return _request_PSG; } }
			public apAnimClip AnimClip { get { return _animClip; } }


			public override string ToString()
			{
				return _request_MeshGroup.ToString() + " / " + _request_Modifier.ToString() + " / " + _request_PSG.ToString();
			}

			//public bool IsLinkAllObjects { get { return _requestType == LINK_REFRESH_REQUEST_TYPE.AllObjects; } }
			//public bool IsSkipAllAnimModifiers { get { return _requestType == LINK_REFRESH_REQUEST_TYPE.MeshGroup_ExceptAnimMods; } }
			//public bool IsSkipUnselectedAnimPSGs {  get { return _requestType == LINK_REFRESH_REQUEST_TYPE.AnimClip; } }

			//Init
			public LinkRefreshRequest()
			{
				Set_AllObjects(null);
			}

			//Functions
			/// <summary>
			/// 모든 메시 그룹과 모든 모디파이어에 대해서 Link. (주의 : 오래 걸림)
			/// 메시 그룹을 인자로 넣으면 해당 메시 그룹은 RenderUnit을 갱신한다.
			/// </summary>
			public LinkRefreshRequest Set_AllObjects(apMeshGroup curSelectedMeshGroup)
			{
				_request_MeshGroup = LR_REQUEST__MESHGROUP.AllMeshGroups;
				_meshGroup = curSelectedMeshGroup;

				_request_Modifier = LR_REQUEST__MODIFIER.AllModifiers;
				_modifier = null;

				_request_PSG = LR_REQUEST__PSG.AllPSGs;
				_animClip = null;
				return this;
			}

			/// <summary>
			/// 선택된 메시 그룹과, 메시 그룹의 모든 모디파이어, PSG에 대해 Link 
			/// </summary>
			public LinkRefreshRequest Set_MeshGroup_AllModifiers(apMeshGroup meshGroup)
			{
				if(meshGroup == null)
				{	
					return Set_AllObjects(null);
				}
				_request_MeshGroup = LR_REQUEST__MESHGROUP.SelectedMeshGroup;
				_meshGroup = meshGroup;

				_request_Modifier = LR_REQUEST__MODIFIER.AllModifiers;
				_modifier = null;

				_request_PSG = LR_REQUEST__PSG.AllPSGs;
				_animClip = null;
				return this;
			}

			/// <summary>
			/// 선택된 메시 그룹과 특정 모디파이어만 Link (PSG는 상관 없음) (메시 그룹 메뉴에서 특정 모디파이어 편집용)
			/// </summary>
			public LinkRefreshRequest Set_MeshGroup_Modifier(apMeshGroup meshGroup, apModifierBase modifier)
			{
				_request_MeshGroup = LR_REQUEST__MESHGROUP.SelectedMeshGroup;
				_meshGroup = meshGroup;

				_request_Modifier = LR_REQUEST__MODIFIER.SelectedModifier;
				_modifier = modifier;

				_request_PSG = LR_REQUEST__PSG.AllPSGs;
				_animClip = null;
				return this;
			}

			/// <summary>
			/// 선택된 메시 그룹과 Anim 모디파이어를 제외한 모든 모디파이어 (메시 그룹 메뉴 편집용)
			/// </summary>
			/// <param name="meshGroup"></param>
			/// <returns></returns>
			public LinkRefreshRequest Set_MeshGroup_ExceptAnimModifiers(apMeshGroup meshGroup)
			{
				_request_MeshGroup = LR_REQUEST__MESHGROUP.SelectedMeshGroup;
				_meshGroup = meshGroup;

				_request_Modifier = LR_REQUEST__MODIFIER.AllModifiers_ExceptAnimMods;
				_modifier = null;

				_request_PSG = LR_REQUEST__PSG.AllPSGs;
				_animClip = null;
				return this;
			}

			/// <summary>
			/// 선택된 애니메이션 클립과 이 애니메이션 클립에서 실행되는 PSG 및 Static-모디파이어들 (애니메이션 편집용)
			/// </summary>
			public LinkRefreshRequest Set_AnimClip(apAnimClip animClip)
			{
				_request_MeshGroup = LR_REQUEST__MESHGROUP.SelectedMeshGroup;
				_meshGroup = _animClip != null ? _animClip._targetMeshGroup : null;

				_request_Modifier = LR_REQUEST__MODIFIER.AllModifiers;
				_modifier = null;

				_request_PSG = LR_REQUEST__PSG.SelectedAnimClipPSG_IfAnimModifier;
				_animClip = animClip;
				return this;
			}
		}

		private static LinkRefreshRequest _linkRefreshRequest = new LinkRefreshRequest();
		/// <summary>
		/// 에디터에서 Link나 Refresh 함수를 쓸 때, 불필요한 객체로의 접근을 막는 요청을 위한 변수.
		/// </summary>
		public static LinkRefreshRequest LinkRefresh
		{
			get
			{
				if (_linkRefreshRequest == null) { _linkRefreshRequest = new LinkRefreshRequest(); }
				return _linkRefreshRequest;
			}
		}
	}

	

	/// <summary>
	/// 이 Attribute가 있다면 SerializedField라도 백업 대상에서 제외된다.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.All)]
	public class NonBackupField : System.Attribute
	{
		
	}

	/// <summary>
	/// 이 Attribute가 있다면 백업 시에 특정 값을 저장할 수 있다.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.All)]
	public class CustomBackupField : System.Attribute
	{
		private string _name;
		public string Name {  get { return _name; } }
		public CustomBackupField(string strName)
		{
			_name = strName;
		}
	}


	
	
	
}