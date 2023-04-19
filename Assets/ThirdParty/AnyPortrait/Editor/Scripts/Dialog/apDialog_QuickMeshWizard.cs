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
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.10.4 : 일괄적으로 메시를 생성할 수 있다.
	/// </summary>
	public class apDialog_QuickMeshWizard : EditorWindow
	{
		// SubUnits
		//------------------------------------------------------------
		public enum GenerateOption
		{
			Simple,
			Moderate,
			Complex
		}

		public enum ReplaceOption
		{
			Replace, Append
		}

		public enum UNAVAILABLE_REASON
		{
			NoImage,
			NoArea
		}

		//생성 요청
		//메시들의 정보와 생성 요청을 정한다.
		public class MakeRequest
		{
			public apMesh _linkedMesh = null;
			public int _vertCount = 0;//버텍스 개수. 제대로 생성되었는지 확인하자
			
			public bool _isMake = false;//생성할 것인가
			public bool _isAvailable = false;//생성 가능한가 (이미지, 영역이 생성되어 있어야 함)

			public UNAVAILABLE_REASON _unavailableReason = UNAVAILABLE_REASON.NoArea;

			//생성 옵션 (초기값은 다이얼로그 생성시의 옵션)
			public GenerateOption _genOption = GenerateOption.Simple;

			public ReplaceOption _replaceOption = ReplaceOption.Replace;

			public MakeRequest(apMesh mesh, int defaultOption, bool defaultMake)
			{
				_linkedMesh = mesh;
				
				_vertCount = _linkedMesh._vertexData != null ? _linkedMesh._vertexData.Count : 0;
			
				if(_linkedMesh._textureData_Linked != null
					&& _linkedMesh._textureData_Linked._image != null
					&& _linkedMesh._isPSDParsed)
				{
					//생성 가능
					_isAvailable = true;
					_isMake = defaultMake;
				}
				else
				{
					//생성 불가
					_isAvailable = false;
					_isMake = false;

					//불가 사유
					if(_linkedMesh._textureData_Linked == null
						|| _linkedMesh._textureData_Linked._image == null)
					{
						_unavailableReason = UNAVAILABLE_REASON.NoImage;
					}
					else
					{
						_unavailableReason = UNAVAILABLE_REASON.NoArea;
					}
				}

				
				//생성 옵션 (초기값은 다이얼로그 생성시의 옵션)
				if(defaultOption == 0)
				{
					_genOption = GenerateOption.Simple;
				}
				else if(defaultOption == 1)
				{
					_genOption = GenerateOption.Moderate;
				}
				else
				{
					_genOption = GenerateOption.Complex;
				}

				_replaceOption = ReplaceOption.Replace;
			}

		}

		
		

		
		// Members
		//------------------------------------------------------------
		public delegate void FUNC_QUICK_MESH_WIZARD(bool isSuccess, object loadKey, List<MakeRequest> requests);

		private FUNC_QUICK_MESH_WIZARD _func_MeshWizard = null;

		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private List<MakeRequest> _makeRequests = null;
		private object _loadKey = null;


		private GUIContent _guiContent_MeshIcon = null;
		private GUIContent _guiContent_QuickMakeAll = null;
		private GUIStyle _guiStyle_CenterLabel = null;
		private GUIStyle _guiStyle_LeftLabel = null;
		private GUIStyle _guiStyle_EnumPopup = null;
		private GUIStyle _guiStyle_BoxBtnMargin = null;

		private Vector2 _scroll = Vector2.zero;

		//일괄 설정 옵션
		private GenerateOption _batchGenOption = GenerateOption.Moderate;

		private ReplaceOption _batchReplaceOption = ReplaceOption.Replace;

		// Show Window
		//------------------------------------------------------------
		private static apDialog_QuickMeshWizard s_window = null;

		public static object ShowDialog(	apEditor editor, 
											apMesh mesh,
											int iGenOption,
											FUNC_QUICK_MESH_WIZARD funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_QuickMeshWizard), true, "Multiple Quick Generate", true);
			apDialog_QuickMeshWizard curTool = curWindow as apDialog_QuickMeshWizard;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 600;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, mesh, iGenOption, funcResult, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		private static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}

		// Init
		//----------------------------------------------------------------------
		public void Init(apEditor editor,
							apMesh mesh,
							int iGenOption,
							FUNC_QUICK_MESH_WIZARD funcResult,
							object loadKey)
		{
			_func_MeshWizard = funcResult;

			_editor = editor;
			_portrait = editor._portrait;
			_loadKey = loadKey;

			_makeRequests = new List<MakeRequest>();

			//Portrait의 메시들을 보여주자
			int nMeshes = _portrait._meshes != null ? _portrait._meshes.Count : 0;
			if(nMeshes > 0)
			{
				apMesh curMesh = null;
				for (int i = 0; i < nMeshes; i++)
				{
					curMesh = _portrait._meshes[i];
					bool isDefaultMake = curMesh == mesh;//현재 선택된 메시는 생성이 기본, 그 외에는 생성 비활성화
					_makeRequests.Add(new MakeRequest(curMesh, iGenOption, isDefaultMake));
				}
			}
		}

		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _func_MeshWizard == null)
			{
				return;
			}

			if(_guiContent_MeshIcon == null)
			{
				_guiContent_MeshIcon = new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh));
			}

			if(_guiStyle_CenterLabel == null)
			{
				_guiStyle_CenterLabel = new GUIStyle(GUI.skin.label);
				_guiStyle_CenterLabel.alignment = TextAnchor.MiddleCenter;
			}

			if(_guiStyle_LeftLabel == null)
			{
				_guiStyle_LeftLabel = new GUIStyle(GUI.skin.label);
				_guiStyle_LeftLabel.alignment = TextAnchor.MiddleLeft;
			}

			if(_guiStyle_EnumPopup == null)
			{
				_guiStyle_EnumPopup = new GUIStyle(EditorStyles.popup);
				
				//버전에 따라 여백이 다르다
#if UNITY_2019_1_OR_NEWER
				_guiStyle_EnumPopup.margin = new RectOffset(_guiStyle_EnumPopup.margin.left,
															_guiStyle_EnumPopup.margin.right,
															3,
															4);
#else
				_guiStyle_EnumPopup.margin = new RectOffset(_guiStyle_EnumPopup.margin.left,
															_guiStyle_EnumPopup.margin.right,
															5,
															4);
#endif
			}

			if(_guiStyle_BoxBtnMargin == null)
			{
				_guiStyle_BoxBtnMargin = new GUIStyle(GUI.skin.box);
				_guiStyle_BoxBtnMargin.margin = GUI.skin.button.margin;
				_guiStyle_BoxBtnMargin.alignment = TextAnchor.MiddleCenter;
			}
			

			//구성
			//- 항목 이름들
			//항목 리스트
			//- 메시 아이콘, 메시 이름
			//- 버텍스 개수
			//- 생성 옵션
			//- Make 버튼 / 토글. 불가

			//모두 선택 / 모두 선택 해제
			//Start Quick Make

			//int height_Top = 25;
			int height_Top = 5;//여백만

			int width_Item__VertCount = 50;
			int width_Item__GenOption = 80;
			int width_Item__ReplaceOption = 80;
			int width_Item__MakeBtn = 150;
			int width_Item__MeshName = width - (width_Item__VertCount + width_Item__GenOption + width_Item__MakeBtn + width_Item__ReplaceOption + 10 + 40);

			//항목 이름들을 Label로 표시 > 그냥 생략
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Top));
			//GUILayout.Space(10);
			//EditorGUILayout.LabelField("Meshes", _guiStyle_CenterLabel, GUILayout.Width(width_Item__MeshName));
			//EditorGUILayout.LabelField("Vertices", _guiStyle_CenterLabel, GUILayout.Width(width_Item__VertCount));
			//EditorGUILayout.LabelField("Quality", _guiStyle_CenterLabel, GUILayout.Width(width_Item__GenOption));
			//EditorGUILayout.LabelField("Option", _guiStyle_CenterLabel, GUILayout.Width(width_Item__GenOption));
			//EditorGUILayout.LabelField("Target", _guiStyle_CenterLabel, GUILayout.Width(width_Item__MakeBtn));
			//EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			int height_Bottom = 130;
			int height_List = height - (height_Top + height_Bottom + 30);
			int height_Item = 25;
			//int width_Item = width - 30;
			
			
			//리스트
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);

			GUI.Box(new Rect(0, height_Top - 1, width, height_List + 2), "");

			GUI.backgroundColor = prevColor;

			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(width), GUILayout.Height(height_List));
			EditorGUILayout.BeginVertical(GUILayout.Width(width - 30));

			GUILayout.Space(5);

			//리스트 내용
			MakeRequest curRequest = null;
			for (int i = 0; i < _makeRequests.Count; i++)
			{
				curRequest = _makeRequests[i];
				EditorGUILayout.BeginHorizontal(GUILayout.Height(height_Item));
				GUILayout.Space(5);
				
				//아이콘과 이름
				EditorGUILayout.LabelField(_guiContent_MeshIcon, _guiStyle_CenterLabel, GUILayout.Width(25), GUILayout.Height(height_Item));
				EditorGUILayout.LabelField(curRequest._linkedMesh._name, _guiStyle_LeftLabel, GUILayout.Width(width_Item__MeshName - 26), GUILayout.Height(height_Item));

				//버텍스 개수
				EditorGUILayout.LabelField("⁖ " + curRequest._vertCount.ToString(), _guiStyle_CenterLabel, GUILayout.Width(width_Item__VertCount), GUILayout.Height(height_Item));

				//퀄리티 옵션
				GenerateOption genOption = (GenerateOption)EditorGUILayout.EnumPopup(curRequest._genOption, _guiStyle_EnumPopup, GUILayout.Width(width_Item__GenOption));
				if(genOption != curRequest._genOption)
				{
					curRequest._genOption = genOption;
					apEditorUtil.ReleaseGUIFocus();
				}

				//교체 옵션
				ReplaceOption replaceOption = (ReplaceOption)EditorGUILayout.EnumPopup(curRequest._replaceOption, _guiStyle_EnumPopup, GUILayout.Width(width_Item__GenOption));
				if(replaceOption != curRequest._replaceOption)
				{
					curRequest._replaceOption = replaceOption;
					apEditorUtil.ReleaseGUIFocus();
				}

				//선택
				if (!curRequest._isAvailable)
				{

					GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.0f, 
														GUI.backgroundColor.g * 0.7f,
														GUI.backgroundColor.b * 0.7f, 1.0f);
					if(curRequest._unavailableReason == UNAVAILABLE_REASON.NoImage)
					{
						//이미지가 없어서 불가
						GUILayout.Box(_editor.GetText(TEXT.NoImage), _guiStyle_BoxBtnMargin, GUILayout.Width(width_Item__MakeBtn), GUILayout.Height(20));
					}
					else
					{
						GUILayout.Box(_editor.GetUIWord(UIWORD.AreaOptionDisabled), _guiStyle_BoxBtnMargin, GUILayout.Width(width_Item__MakeBtn), GUILayout.Height(20));
					}

					GUI.backgroundColor = prevColor;
				}
				else
				{
					if (apEditorUtil.ToggledButton_2Side(	_editor.GetText(TEXT.DLG_Selected), 
															_editor.GetText(TEXT.DLG_NotSelected), curRequest._isMake, curRequest._isAvailable, width_Item__MakeBtn, 20))
					{
						if (curRequest._isAvailable)
						{
							curRequest._isMake = !curRequest._isMake;
						}
					}
				}
				

				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			
			

			GUILayout.Space(height + 20);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);

			//하단 버튼
			int width_BtnBottom = ((width - 10) / 2) - 1;
			EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
			GUILayout.Space(4);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_SelectAll), GUILayout.Width(width_BtnBottom), GUILayout.Height(20)))
			{
				//모두 선택
				for (int i = 0; i < _makeRequests.Count; i++)
				{
					curRequest = _makeRequests[i];
					if(curRequest._isAvailable)
					{
						curRequest._isMake = true;
					}
				}
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_DeselectAll), GUILayout.Width(width_BtnBottom), GUILayout.Height(20)))
			{
				for (int i = 0; i < _makeRequests.Count; i++)
				{
					curRequest = _makeRequests[i];
					if(curRequest._isAvailable)
					{
						curRequest._isMake = false;
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			//추가 22.8.2 [v1.4.1] : 설정 일괄 변경
			//왼쪽에 Batch 옵션, 오른쪽에 Replace 옵션 (설정 + Set All)
			int width_BtnBatchSet = 120;
			int width_BtnBatchOption = width_BtnBottom - (width_BtnBatchSet + 4);
			EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
			GUILayout.Space(4);
			_batchGenOption = (GenerateOption)EditorGUILayout.EnumPopup(_batchGenOption, _guiStyle_EnumPopup, GUILayout.Width(width_BtnBatchOption));
			if(GUILayout.Button(_editor.GetText(TEXT.ChangeAll), GUILayout.Width(width_BtnBatchSet), GUILayout.Height(20)))
			{
				//일괄적으로 설정을 바꾸자
				for (int i = 0; i < _makeRequests.Count; i++)
				{
					curRequest = _makeRequests[i];
					curRequest._genOption = _batchGenOption;
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			_batchReplaceOption = (ReplaceOption)EditorGUILayout.EnumPopup(_batchReplaceOption, _guiStyle_EnumPopup, GUILayout.Width(width_BtnBatchOption));
			if(GUILayout.Button(_editor.GetText(TEXT.ChangeAll), GUILayout.Width(width_BtnBatchSet), GUILayout.Height(20)))
			{
				//일괄적으로 설정을 바꾸자
				for (int i = 0; i < _makeRequests.Count; i++)
				{
					curRequest = _makeRequests[i];
					curRequest._replaceOption = _batchReplaceOption;
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();


			if(_guiContent_QuickMakeAll == null)
			{
				_guiContent_QuickMakeAll = new GUIContent("  " + _editor.GetUIWord(UIWORD.MultipleQuickGenerate), _editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MultipleQuickMake));
			}

			bool isClose = false;
			if(GUILayout.Button(_guiContent_QuickMakeAll, GUILayout.Height(40)))
			{
				if(_func_MeshWizard != null)
				{
					//메시 생성 요청을 보내자
					List<MakeRequest> resultRequests = new List<MakeRequest>();

					MakeRequest curReq = null;
					for (int i = 0; i < _makeRequests.Count; i++)
					{
						curReq = _makeRequests[i];
						if(curReq._isAvailable && curReq._isMake)
						{
							resultRequests.Add(curReq);
						}
					}
					_func_MeshWizard(resultRequests.Count > 0, _loadKey, resultRequests);
				}

				//그리고 종료
				isClose = true;
			}

			GUILayout.Space(10);

			if(GUILayout.Button(_editor.GetText(TEXT.Close), GUILayout.Height(25)))
			{
				isClose = true;
			}

			if (isClose)
			{
				CloseDialog();
			}
		}
	}
}