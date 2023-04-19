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
using System.IO;
using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 본 리타겟을 위해 파일에 저장되는 단위
	/// "기본 구조"에 대한 파일인지 "애니메이션/정적 포즈"에 대한 파일인지 구분한다.
	/// 본 구조는 계층적으로 설정한다.
	/// 키값은 별도의 ID로 구분한다.
	/// </summary>
	public class apRetargetBoneUnit
	{
		// Members
		//---------------------------------------------------
		//기본적인 본 정보를 가지고 있다.
		//UniqueID만 제외하고 UnitID로 교체한다.
		public int _unitID = -1;
		
		//Export할때만 가지는 값. Import할때는 -1이다.
		public int _boneUniqueID = -1;
		public apBone _linkedBone = null;


		public string _name = "";
		public int _parentUnitID = -1;//<<BoneID가 아닌 RetargetBoneUnitID를 사용한다.
		public int _level = -1;
		public int _depth = -1;
		public List<int> _childUnitID = new List<int>();

		public apMatrix _defaultMatrix = new apMatrix();

		public Color _color = Color.white;
		public int _shapeWidth = 30;
		public int _shapeLength = 50;
		public int _shapeTaper = 100;

		public apBone.OPTION_IK _optionIK = apBone.OPTION_IK.Disabled;

		public bool _isIKTail = false;

		public int _IKTargetBoneUnitID = -1;
		public int _IKNextChainedBoneUnitID = -1;
		public int _IKHeaderBoneUnitID = -1;

		public bool _isIKAngleRange = false;
		public float _IKAngleRange_Lower = -90.0f;
		public float _IKAngleRange_Upper = 90.0f;
		public float _IKAnglePreferred = 0.0f;

		public bool _isSocketEnabled = false;

		//추가 21.3.7
		//지글본 속성을 더 추가한다. (단 파싱은 구버전 호환 가능하게)
		public bool _isJiggle = false;
		public float _jiggle_Mass = 1.0f;
		public float _jiggle_K = 50.0f;
		public float _jiggle_Drag = 0.8f;
		public float _jiggle_Damping = 5.0f;
		public bool _isJiggleAngleConstraint = false;
		public float _jiggle_AngleLimit_Min = -30.0f;
		public float _jiggle_AngleLimit_Max = 30.0f;

		public bool _isJigglePropertyImported = false;

		//------------------------------------------------
		// 로드된 정보를 어떻게 적용할 것인가에 대한 정보
		public bool _isImportEnabled = true;
		public bool _isIKEnabled = true;
		public bool _isShapeEnabled = true;


		private const string TEXT_SLASH = "/";
		private const string TEXT_1_AS_TRUE = "1";
		private const string TEXT_0_AS_FALSE = "0";

		// Init
		//---------------------------------------------------
		public apRetargetBoneUnit()
		{
			
		}



		// Functions
		//---------------------------------------------------
		// Bone to File
		//---------------------------------------------------
		public void SetBone(int unitID, apBone bone, Dictionary<int, int> boneID2UnitIDs)
		{
			_unitID = unitID;

			_boneUniqueID = bone._uniqueID;
			_linkedBone = bone;


			_name = bone._name;

			if(bone._parentBoneID < 0)
			{
				_parentUnitID = -1;
			}
			else
			{
				_parentUnitID = boneID2UnitIDs[bone._parentBoneID];
			}
			 
			_level = bone._level;
			_depth = bone._depth;
			_childUnitID.Clear();
			if(bone._childBoneIDs != null)
			{
				for (int i = 0; i < bone._childBoneIDs.Count; i++)
				{
					int childID = bone._childBoneIDs[i];
					if(childID >= 0)
					{
						_childUnitID.Add(boneID2UnitIDs[childID]);
					}
				}
			}
			
			_defaultMatrix.SetMatrix(bone._defaultMatrix, true);

			_color = bone._color;
			_shapeWidth = bone._shapeWidth;
			_shapeLength = bone._shapeLength;
			_shapeTaper = bone._shapeTaper;

			_optionIK = bone._optionIK;

			_isIKTail = bone._isIKTail;

			_IKTargetBoneUnitID = -1;
			if(bone._IKTargetBoneID >= 0)
			{
				_IKTargetBoneUnitID = boneID2UnitIDs[bone._IKTargetBoneID];
			}

			_IKNextChainedBoneUnitID = -1;
			if(bone._IKNextChainedBoneID >= 0)
			{
				_IKNextChainedBoneUnitID = boneID2UnitIDs[bone._IKNextChainedBoneID];
			}
			_IKHeaderBoneUnitID = -1;
			if(bone._IKHeaderBoneID >= 0)
			{
				_IKHeaderBoneUnitID = boneID2UnitIDs[bone._IKHeaderBoneID];
			}

			_isIKAngleRange = bone._isIKAngleRange;
			_IKAngleRange_Lower = bone._IKAngleRange_Lower;
			_IKAngleRange_Upper = bone._IKAngleRange_Upper;
			_IKAnglePreferred = bone._IKAnglePreferred;
			_isSocketEnabled = false;

			//추가 21.3.7 : 지글본도 저장한다.
			_isJiggle = bone._isJiggle;
			_jiggle_Mass = bone._jiggle_Mass;
			_jiggle_K = bone._jiggle_K;
			_jiggle_Drag = bone._jiggle_Drag;
			_jiggle_Damping = bone._jiggle_Damping;
			_isJiggleAngleConstraint = bone._isJiggleAngleConstraint;
			_jiggle_AngleLimit_Min = bone._jiggle_AngleLimit_Min;
			_jiggle_AngleLimit_Max = bone._jiggle_AngleLimit_Max;
		}


		public string GetEncodingData()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (_name.Length < 10)
			{
				sb.Append("00" + _name.Length.ToString());
			}
			else if(_name.Length < 100)
			{
				sb.Append("0" + _name.Length.ToString());
			}
			else
			{
				sb.Append(_name.Length.ToString());
			}
			sb.Append(_name);
			sb.Append(_unitID);			sb.Append(TEXT_SLASH);
			sb.Append(_parentUnitID);	sb.Append(TEXT_SLASH);
			sb.Append(_level);			sb.Append(TEXT_SLASH);
			sb.Append(_depth);			sb.Append(TEXT_SLASH);

			sb.Append(_childUnitID.Count);			sb.Append(TEXT_SLASH);
			for (int i = 0; i < _childUnitID.Count; i++)
			{
				sb.Append(_childUnitID[i]);
				sb.Append(TEXT_SLASH);
			}

			sb.Append(_defaultMatrix._pos.x);		sb.Append(TEXT_SLASH);
			sb.Append(_defaultMatrix._pos.y);		sb.Append(TEXT_SLASH);
			sb.Append(_defaultMatrix._angleDeg);	sb.Append(TEXT_SLASH);
			sb.Append(_defaultMatrix._scale.x);		sb.Append(TEXT_SLASH);
			sb.Append(_defaultMatrix._scale.y);		sb.Append(TEXT_SLASH);

			sb.Append(_color.r);	sb.Append(TEXT_SLASH);
			sb.Append(_color.g);	sb.Append(TEXT_SLASH);
			sb.Append(_color.b);	sb.Append(TEXT_SLASH);
			sb.Append(_color.a);	sb.Append(TEXT_SLASH);

			sb.Append(_shapeWidth);		sb.Append(TEXT_SLASH);
			sb.Append(_shapeLength);	sb.Append(TEXT_SLASH);
			sb.Append(_shapeTaper);		sb.Append(TEXT_SLASH);

			sb.Append((int)_optionIK);			sb.Append(TEXT_SLASH);
			sb.Append((_isIKTail ? TEXT_1_AS_TRUE : TEXT_0_AS_FALSE));	sb.Append(TEXT_SLASH);

			sb.Append(_IKTargetBoneUnitID);			sb.Append(TEXT_SLASH);
			sb.Append(_IKNextChainedBoneUnitID);	sb.Append(TEXT_SLASH);
			sb.Append(_IKHeaderBoneUnitID);			sb.Append(TEXT_SLASH);

			sb.Append((_isIKAngleRange ? TEXT_1_AS_TRUE : TEXT_0_AS_FALSE));	sb.Append(TEXT_SLASH);
			sb.Append(_IKAngleRange_Lower);				sb.Append(TEXT_SLASH);
			sb.Append(_IKAngleRange_Upper);				sb.Append(TEXT_SLASH);
			sb.Append(_IKAnglePreferred);				sb.Append(TEXT_SLASH);

			sb.Append((_isSocketEnabled ? TEXT_1_AS_TRUE : TEXT_0_AS_FALSE));	sb.Append(TEXT_SLASH);

			//추가 21.3.7 : 지글본 옵션도 내보내자. 파싱시 조심할 것
			sb.Append((_isJiggle ? TEXT_1_AS_TRUE : TEXT_0_AS_FALSE));	sb.Append(TEXT_SLASH);
			sb.Append(_jiggle_Mass);									sb.Append(TEXT_SLASH);
			sb.Append(_jiggle_K);										sb.Append(TEXT_SLASH);
			sb.Append(_jiggle_Drag);									sb.Append(TEXT_SLASH);
			sb.Append(_jiggle_Damping);									sb.Append(TEXT_SLASH);
			sb.Append((_isJiggleAngleConstraint ? TEXT_1_AS_TRUE : TEXT_0_AS_FALSE));	sb.Append(TEXT_SLASH);
			sb.Append(_jiggle_AngleLimit_Min);							sb.Append(TEXT_SLASH);
			sb.Append(_jiggle_AngleLimit_Max);							sb.Append(TEXT_SLASH);



			return sb.ToString();
		}




		// File To Bone
		//---------------------------------------------------
		public bool DecodeData(string strSrc)
		{
			try
			{
				if(strSrc.Length < 3)
				{
					return false;
				}
				int nName = int.Parse(strSrc.Substring(0, 3));
				_name = strSrc.Substring(3, nName);

				strSrc = strSrc.Substring(3 + nName);

				//나머지는 델리미터를 이용한다.
				string[] strUnits = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);
				int iStr = 0;
				_unitID = int.Parse(strUnits[iStr]);
				iStr++;

				_parentUnitID = int.Parse(strUnits[iStr]);
				iStr++;

				_level = int.Parse(strUnits[iStr]);
				iStr++;

				_depth = int.Parse(strUnits[iStr]);
				iStr++;

				_childUnitID.Clear();
				int nChild = int.Parse(strUnits[iStr]);
				iStr++;

				for (int i = 0; i < nChild; i++)
				{
					_childUnitID.Add(int.Parse(strUnits[iStr]));
					iStr++;
				}

				_defaultMatrix.SetIdentity();
				_defaultMatrix.SetTRS(
					new Vector2(apUtil.ParseFloat(strUnits[iStr]), apUtil.ParseFloat(strUnits[iStr + 1])),
					apUtil.ParseFloat(strUnits[iStr + 2]),
					new Vector2(apUtil.ParseFloat(strUnits[iStr + 3]), apUtil.ParseFloat(strUnits[iStr + 4])),
					true
					);

				iStr += 5;

				_color.r = apUtil.ParseFloat(strUnits[iStr]);
				_color.g = apUtil.ParseFloat(strUnits[iStr + 1]);
				_color.b = apUtil.ParseFloat(strUnits[iStr + 2]);
				_color.a = apUtil.ParseFloat(strUnits[iStr + 3]);
				iStr += 4;

				_shapeWidth = int.Parse(strUnits[iStr]);
				_shapeLength = int.Parse(strUnits[iStr + 1]);
				_shapeTaper = int.Parse(strUnits[iStr + 2]);
				iStr += 3;

				_optionIK = (apBone.OPTION_IK)int.Parse(strUnits[iStr]);
				_isIKTail = (int.Parse(strUnits[iStr + 1]) == 1) ? true : false;
				iStr += 2;

				_IKTargetBoneUnitID = int.Parse(strUnits[iStr]);
				_IKNextChainedBoneUnitID = int.Parse(strUnits[iStr + 1]);
				_IKHeaderBoneUnitID = int.Parse(strUnits[iStr + 2]);
				iStr += 3;

				_isIKAngleRange = (int.Parse(strUnits[iStr]) == 1) ? true : false;
				_IKAngleRange_Lower = apUtil.ParseFloat(strUnits[iStr + 1]);
				_IKAngleRange_Upper = apUtil.ParseFloat(strUnits[iStr + 2]);
				_IKAnglePreferred = apUtil.ParseFloat(strUnits[iStr + 3]);
				iStr += 4;

				_isSocketEnabled = (int.Parse(strUnits[iStr]) == 1) ? true : false;
				iStr += 1;

				//추가 21.3.7 : 지글본 옵션이 있다면 가져오자
				//8개가 더 있어야 한다.
				if(iStr + 7 < strUnits.Length)
				{
					_isJigglePropertyImported = true;//Import가 되었다.

					_isJiggle = (int.Parse(strUnits[iStr]) == 1) ? true : false;
					_jiggle_Mass = apUtil.ParseFloat(strUnits[iStr + 1]);
					_jiggle_K = apUtil.ParseFloat(strUnits[iStr + 2]);
					_jiggle_Drag = apUtil.ParseFloat(strUnits[iStr + 3]);
					_jiggle_Damping = apUtil.ParseFloat(strUnits[iStr + 4]);
					_isJiggleAngleConstraint = (int.Parse(strUnits[iStr + 5]) == 1) ? true : false;
					_jiggle_AngleLimit_Min = apUtil.ParseFloat(strUnits[iStr + 6]);
					_jiggle_AngleLimit_Max = apUtil.ParseFloat(strUnits[iStr + 7]);
					iStr += 8;
				}
				else
				{
					//지글본 정보가 없다.
					_isJigglePropertyImported = false;
				}


				_isImportEnabled = true;
				_isIKEnabled = true;
				_isShapeEnabled = true;
			}
			catch(Exception ex)
			{
				Debug.LogError("Decode Exception : " + ex);
				return false;
			}
			

			return true;
		}


		// Get / Set
		//---------------------------------------------------
		//Bone 정보를 어떻게 저장해야하나..
	}
}