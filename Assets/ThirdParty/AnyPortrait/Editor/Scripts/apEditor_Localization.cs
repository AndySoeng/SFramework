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
using System.IO;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{

	//public class apEditor_Localization : EditorWindow
	//{
	//	// Members
	//	//------------------------------------------------------------------
	//	private static apEditor_Localization s_window = null;

	//	private class TextSet
	//	{
	//		public int _index = -1;
	//		public string _typeName = "";
	//		public Dictionary<apEditor.LANGUAGE, string> _textSet = new Dictionary<apEditor.LANGUAGE, string>();

	//		public TextSet(int index, string typeName)
	//		{
	//			_index = index;
	//			_typeName = typeName;
	//		}

	//		public void SetText(apEditor.LANGUAGE language, string text)
	//		{
	//			text = text.Replace("\t", "");
	//			text = text.Replace("[]", "\r\n");
	//			text = text.Replace("[c]", ",");
	//			text = text.Replace("[u]", "\"");

	//			_textSet.Add(language, text);
	//		}
	//		public string GetText(apEditor.LANGUAGE language)
	//		{
	//			string resultText = _textSet[language];
	//			resultText = resultText.Replace("[]", "\r\n");
	//			resultText = resultText.Replace("[c]", ",");
	//			resultText = resultText.Replace("[u]", "\"");

	//			return resultText;
	//		}

	//		public string GetTextToSave(apEditor.LANGUAGE language)
	//		{
	//			string resultText = _textSet[language];
	//			resultText = resultText.Replace("\r\n", "[]");
	//			resultText = resultText.Replace("\r", "");
	//			resultText = resultText.Replace("\n", "[]");
	//			resultText = resultText.Replace(",", "[c]");
	//			resultText = resultText.Replace("\"", "[u]");

	//			return resultText;
	//		}

	//		public void Init()
	//		{
	//			_textSet.Clear();
	//			_textSet.Add(apEditor.LANGUAGE.English, "");
	//			_textSet.Add(apEditor.LANGUAGE.Korean, "");
	//			_textSet.Add(apEditor.LANGUAGE.French, "");
	//			_textSet.Add(apEditor.LANGUAGE.German, "");
	//			_textSet.Add(apEditor.LANGUAGE.Spanish, "");
	//			_textSet.Add(apEditor.LANGUAGE.Italian, "");
	//			_textSet.Add(apEditor.LANGUAGE.Danish, "");
	//			_textSet.Add(apEditor.LANGUAGE.Japanese, "");
	//			_textSet.Add(apEditor.LANGUAGE.Chinese_Traditional, "");
	//			_textSet.Add(apEditor.LANGUAGE.Chinese_Simplified, "");
	//			_textSet.Add(apEditor.LANGUAGE.Polish, "");
	//		}
	//	}

	//	private List<TextSet> _textSets = new List<TextSet>();
	//	private TextSet _selectedTextSet = null;

	//	private Vector2 _scroll = Vector2.zero;

	//	// Show Window
	//	//------------------------------------------------------------------
	//	//[MenuItem("Window/AnyPortrait/Localization [Dialog] Table", false, 61)]//<<사용할때만 이걸 켭시다.
	//	public static void ShowDialog()
	//	{
	//		CloseDialog();


	//		EditorWindow curWindow = EditorWindow.GetWindow(typeof(apEditor_Localization), false, "Localization Table", true);
	//		apEditor_Localization curTool = curWindow as apEditor_Localization;

	//		//object loadKey = new object();
	//		if (curTool != null && curTool != s_window)
	//		{
	//			int width = 1500;
	//			int height = 800;
	//			s_window = curTool;
	//			s_window.position = new Rect((width / 2),
	//											(height / 2),
	//											width, height);

	//			s_window.Init();
	//		}
	//	}

	//	public static void CloseDialog()
	//	{
	//		if (s_window != null)
	//		{
	//			try
	//			{
	//				s_window.Close();
	//			}
	//			catch (Exception ex)
	//			{
	//				Debug.LogError("Close Exception : " + ex);
	//			}
	//			s_window = null;
	//		}
	//	}

	//	// Init
	//	//------------------------------------------------------------------
	//	public void Init()
	//	{
	//		_selectedTextSet = null;
	//		if (_textSets == null)
	//		{
	//			_textSets = new List<TextSet>();
	//		}
	//		_textSets.Clear();
	//	}

	//	// GUI
	//	//------------------------------------------------------------------
	//	void OnGUI()
	//	{
	//		int width = (int)position.width;
	//		int height = (int)position.height;

	//		width -= 10;
	//		//Bake 설정
	//		EditorGUILayout.LabelField("Localization Table", GUILayout.Width(width));
	//		//X, Y 개수를 표시

	//		GUILayout.Space(10);

	//		EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
	//		GUILayout.Space(5);
	//		if (GUILayout.Button("Load", GUILayout.Width(80), GUILayout.Height(25)))
	//		{
	//			Load();
	//			_selectedTextSet = null;
	//		}
	//		if (GUILayout.Button("Save", GUILayout.Width(80), GUILayout.Height(25)))
	//		{
	//			Save();
	//		}
	//		GUILayout.Space(20);
	//		if (GUILayout.Button("Add Item", GUILayout.Width(120), GUILayout.Height(25)))
	//		{
	//			TextSet newTextSet = new TextSet(_textSets.Count, "");
	//			newTextSet.Init();
	//			_textSets.Add(newTextSet);
	//			_selectedTextSet = newTextSet;
	//			GUI.FocusControl(null);
	//		}

	//		if (GUILayout.Button("Sort", GUILayout.Width(80), GUILayout.Height(25)))
	//		{
	//			//일단 정렬
	//			_textSets.Sort(delegate (TextSet a, TextSet b)
	//			{
	//				return a._index - b._index;
	//			});
	//		}

	//		if (GUILayout.Button("Open Enums", GUILayout.Width(100), GUILayout.Height(25)))
	//		{
	//			TextAsset enumAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apLangPack_Enum.txt");
	//			if (enumAsset != null)
	//			{
	//				AssetDatabase.OpenAsset(enumAsset);
	//			}
	//		}
	//		GUILayout.Space(10);
	//		if (GUILayout.Button("Auto Index", GUILayout.Width(100), GUILayout.Height(25)))
	//		{
	//			//일단 정렬
	//			for (int i = 0; i < _textSets.Count; i++)
	//			{
	//				_textSets[i]._index = i;
	//			}
	//		}
	//		EditorGUILayout.EndHorizontal();

	//		int height_Table = height - (30 + 25 + 20 + 200 + 40);


	//		int width_Table = width - 30;

	//		int width_Select = 30;
	//		int width_Name = 150;
	//		int width_Index = 50;
	//		int width_Text = (width_Table - (5 + width_Select + width_Name + width_Index)) / 11 - 4;


	//		EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//		GUILayout.Space(5);
	//		GUILayout.Space(width_Select + 2);
	//		EditorGUILayout.LabelField("Name", GUILayout.Width(width_Name));
	//		EditorGUILayout.LabelField("ID", GUILayout.Width(width_Index));
	//		EditorGUILayout.LabelField("영어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("한국어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("프랑스어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("독일어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("스페인어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("이탈리아어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("덴마크어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("일본어", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("중국어-번체", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("중국어-간체", GUILayout.Width(width_Text));
	//		EditorGUILayout.LabelField("폴란드어", GUILayout.Width(width_Text));

	//		EditorGUILayout.EndHorizontal();
	//		_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(width), GUILayout.Height(height_Table));



	//		EditorGUILayout.BeginVertical(GUILayout.Width(width_Table));
	//		TextSet curTextSet = null;
	//		for (int i = 0; i < _textSets.Count; i++)
	//		{
	//			curTextSet = _textSets[i];
	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Table));
	//			GUILayout.Space(5);
	//			if (_selectedTextSet != curTextSet)
	//			{
	//				if (GUILayout.Button("Sel", GUILayout.Width(width_Select)))
	//				{
	//					_selectedTextSet = curTextSet;
	//					GUI.FocusControl(null);
	//				}
	//			}
	//			else
	//			{
	//				if (GUILayout.Button("★", GUILayout.Width(width_Select)))
	//				{
	//					//..
	//				}
	//			}

	//			curTextSet._typeName = EditorGUILayout.TextField(curTextSet._typeName, GUILayout.Width(width_Name));
	//			curTextSet._index = EditorGUILayout.IntField(curTextSet._index, GUILayout.Width(width_Index));

	//			curTextSet._textSet[apEditor.LANGUAGE.English] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.English], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Korean] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Korean], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.French] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.French], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.German] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.German], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Spanish] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Spanish], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Italian] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Italian], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Danish] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Danish], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Japanese] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Japanese], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Chinese_Traditional] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Chinese_Traditional], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Chinese_Simplified] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Chinese_Simplified], GUILayout.Width(width_Text));
	//			curTextSet._textSet[apEditor.LANGUAGE.Polish] = EditorGUILayout.TextField(curTextSet._textSet[apEditor.LANGUAGE.Polish], GUILayout.Width(width_Text));

	//			EditorGUILayout.EndHorizontal();
	//		}





	//		EditorGUILayout.EndVertical();

	//		GUILayout.Space(height_Table + 20);

	//		EditorGUILayout.EndScrollView();

	//		GUILayout.Space(10);
	//		GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(4));
	//		GUILayout.Space(10);

	//		if (_selectedTextSet != null)
	//		{
	//			int width_selLabel = 100;
	//			int width_selData = ((width - 10) / 2) - (width_selLabel + 4);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("타입명", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._typeName = EditorGUILayout.TextField(_selectedTextSet._typeName, GUILayout.Width(200));

	//			EditorGUILayout.LabelField("Index", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._index = EditorGUILayout.IntField(_selectedTextSet._index, GUILayout.Width(80));
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(10);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("1 영어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.English] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.English], GUILayout.Width(width_selData));

	//			EditorGUILayout.LabelField("2 한국어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Korean] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Korean], GUILayout.Width(width_selData));
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(5);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("3 프랑스어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.French] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.French], GUILayout.Width(width_selData));

	//			EditorGUILayout.LabelField("4 독일어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.German] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.German], GUILayout.Width(width_selData));
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(5);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("5 스페인어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Spanish] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Spanish], GUILayout.Width(width_selData));

	//			EditorGUILayout.LabelField("6 이탈리아어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Italian] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Italian], GUILayout.Width(width_selData));
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(5);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("7 덴마크어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Danish] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Danish], GUILayout.Width(width_selData));

	//			EditorGUILayout.LabelField("8 일본어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Japanese] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Japanese], GUILayout.Width(width_selData));
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(5);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("9 중국어-번체", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Chinese_Traditional] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Chinese_Traditional], GUILayout.Width(width_selData));

	//			EditorGUILayout.LabelField("10 중국어-간체", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Chinese_Simplified] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Chinese_Simplified], GUILayout.Width(width_selData));
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(5);

	//			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
	//			GUILayout.Space(5);
	//			EditorGUILayout.LabelField("11 폴란드어", GUILayout.Width(width_selLabel));
	//			_selectedTextSet._textSet[apEditor.LANGUAGE.Polish] = EditorGUILayout.TextField(_selectedTextSet._textSet[apEditor.LANGUAGE.Polish], GUILayout.Width(width_selData));

				
	//			EditorGUILayout.EndHorizontal();

	//			GUILayout.Space(5);
	//		}

	//	}


	//	// Functions
	//	//----------------------------------------------------------------------------
	//	private void Load()
	//	{
	//		_textSets.Clear();

	//		TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.ResourcePath_Text + "apLangPack.txt");

	//		string[] strParseLines = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.None);
	//		string strCurParseLine = null;
	//		for (int i = 1; i < strParseLines.Length; i++)
	//		{
	//			//첫줄(index 0)은 빼고 읽는다.
	//			strCurParseLine = strParseLines[i].Replace("\r", "");
	//			string[] strSubParseLine = strCurParseLine.Split(new string[] { "," }, StringSplitOptions.None);
	//			//Parse 순서
	//			//0 : TEXT 타입 (string) - 파싱 안한다.
	//			//1 : TEXT 타입 (int)
	//			//2 : English (영어)
	//			//3 : Korean (한국어)
	//			//4 : French (프랑스어)
	//			//5 : German (독일어)
	//			//6 : Spanish (스페인어)
	//			//7 : Italian (이탈리아어)
	//			//8 : Danish (덴마크어)
	//			//9 : Japanese (일본어)
	//			//10 : Chinese_Traditional (중국어-번체)
	//			//11 : Chinese_Simplified (중국어-간체)
	//			//12 : Polich (폴란드어)

	//			if (strSubParseLine.Length < 13)
	//			{
	//				//Debug.LogError("인식할 수 없는 Text (" + i + " : " + strCurParseLine + ")");
	//				continue;
	//			}
	//			try
	//			{
	//				string strTextType = strSubParseLine[0];
	//				int index = (int.Parse(strSubParseLine[1]));
	//				TextSet newTextSet = new TextSet(index, strTextType);

	//				newTextSet.SetText(apEditor.LANGUAGE.English, strSubParseLine[2]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Korean, strSubParseLine[3]);
	//				newTextSet.SetText(apEditor.LANGUAGE.French, strSubParseLine[4]);
	//				newTextSet.SetText(apEditor.LANGUAGE.German, strSubParseLine[5]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Spanish, strSubParseLine[6]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Italian, strSubParseLine[7]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Danish, strSubParseLine[8]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Japanese, strSubParseLine[9]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Chinese_Traditional, strSubParseLine[10]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Chinese_Simplified, strSubParseLine[11]);
	//				newTextSet.SetText(apEditor.LANGUAGE.Polish, strSubParseLine[12]);

	//				_textSets.Add(newTextSet);
	//			}
	//			catch (Exception)
	//			{
	//				Debug.LogError("Parsing 실패 (" + i + " : " + strCurParseLine + ")");
	//			}


	//		}
	//	}

	//	private void Save()
	//	{
	//		if (_textSets.Count == 0)
	//		{
	//			return;
	//		}
	//		//string filePath = Application.dataPath + "/Editor/AnyPortraitTool/Util/apLangPack.txt";
	//		//string filePath_Enum = Application.dataPath + "/Editor/AnyPortraitTool/Util/apLangPack_Enum.txt";

	//		string filePath = Application.dataPath + "/" + apEditorUtil.ResourcePath_Text_WithoutAssets + "apLangPack.txt";
	//		string filePath_Enum = Application.dataPath + "/" + apEditorUtil.ResourcePath_Text_WithoutAssets + "apLangPack_Enum.txt";
	//		FileStream fs = null;
	//		StreamWriter sw = null;



	//		//일단 정렬
	//		_textSets.Sort(delegate (TextSet a, TextSet b)
	//		{
	//			return a._index - b._index;
	//		});

	//		try
	//		{
	//			fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
	//			sw = new StreamWriter(fs);

	//			sw.WriteLine("Text Type,Text Index,1 English (영어),2 Korean (한국어),3 French (프랑스어),4 German (독일어),5 Spanish (스페인어),6 Italian (이탈리아어),7 Danish (덴마크어),8 Japanese (일본어),9 Chinese_Traditional (중국어-번체),10 Chinese_Simplified (중국어-간체), Polish (폴란드어)");
	//			TextSet curSet = null;
	//			for (int i = 0; i < _textSets.Count; i++)
	//			{
	//				curSet = _textSets[i];
	//				System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();

	//				strBuilder.Append(curSet._typeName).Append(",");
	//				strBuilder.Append(curSet._index).Append(",");
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.English)).Append(",");             //2 : English (영어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Korean)).Append(",");              //3 : Korean (한국어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.French)).Append(",");              //4 : French (프랑스어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.German)).Append(",");              //5 : German (독일어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Spanish)).Append(",");             //6 : Spanish (스페인어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Italian)).Append(",");             //7 : Italian (이탈리아어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Danish)).Append(",");              //8 : Danish (덴마크어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Japanese)).Append(",");            //9 : Japanese (일본어)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Chinese_Traditional)).Append(",");	//10 : Chinese_Traditional (중국어-번체)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Chinese_Simplified)).Append(",");  //11 : Chinese_Simplified (중국어-간체)
	//				strBuilder.Append(curSet.GetTextToSave(apEditor.LANGUAGE.Polish)).Append(",");              //12 : Polish (폴란드어)

	//				sw.WriteLine(strBuilder.ToString());
	//			}

	//			sw.Flush();

	//			if (sw != null)
	//			{
	//				sw.Close();
	//				sw = null;
	//			}
	//			if (fs != null)
	//			{
	//				fs.Close();
	//				fs = null;
	//			}


	//			fs = new FileStream(filePath_Enum, FileMode.Create, FileAccess.Write);
	//			sw = new StreamWriter(fs);

	//			sw.WriteLine("\tpublic enum TEXT");
	//			sw.WriteLine("\t{");
	//			for (int i = 0; i < _textSets.Count; i++)
	//			{
	//				curSet = _textSets[i];
	//				sw.WriteLine("\t\t" + curSet._typeName + " = " + curSet._index + ",");
	//			}
	//			sw.WriteLine("\t}");


	//			sw.Flush();

	//			if (sw != null)
	//			{
	//				sw.Close();
	//				sw = null;
	//			}
	//			if (fs != null)
	//			{
	//				fs.Close();
	//				fs = null;
	//			}

	//			//혹시 모를 복사를 하자
	//			FileInfo fi = new FileInfo(filePath);
	//			fi.CopyTo(Application.dataPath + "/../../LangPackBackUp_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".txt");
	//		}
	//		catch (Exception ex)
	//		{
	//			Debug.LogError("Save Exception : " + ex);
	//			if (sw != null)
	//			{
	//				sw.Close();
	//				sw = null;
	//			}
	//			if (fs != null)
	//			{
	//				fs.Close();
	//				fs = null;
	//			}
	//		}

	//		AssetDatabase.Refresh(ImportAssetOptions.Default);


	//	}
	//}

}