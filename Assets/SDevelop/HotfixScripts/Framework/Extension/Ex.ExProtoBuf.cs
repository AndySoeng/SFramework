
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using UnityEngine;

namespace Ex
{
    public static class ExProtobuf
    {
        public static byte[] Serialize(IMessage iMessage)
        {
            using (MemoryStream ms = new MemoryStream())

            {
                iMessage.WriteTo(ms);

                byte[] result = new byte[ms.Length];

                ms.Position = 0;

                ms.Read(result, 0, result.Length);

                return result;
            }

            #region 直接转换

            //return iMessage.ToByteArray();

            #endregion
        }

        public static T Deserialize<T>(byte[] b) where T : IMessage, new()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(b, 0, b.Length);
                ms.Position = 0;
                //TODO 看了以下，这个MessageParser应该是可以复用的
                MessageParser<T> iMessageParser = new MessageParser<T>(() => new T());
                return iMessageParser.ParseFrom(ms);
            }

            #region 直接转换

            // MessageParser<T> iMessageParser = new MessageParser<T>(() => new T());
            // return iMessageParser.ParseFrom(b);

            #endregion
        }

        public static void DumpAsString(object obj, string hint = "")
        {
            Dumper.Dump(obj, hint);
        }

        /// <summary>
        /// 仅能在HotFix中使用
        /// </summary>
        private class Dumper
        {
            private static readonly StringBuilder _text = new StringBuilder("", 1024);

            private static void AppendIndent(int num)
            {
                _text.Append(' ', num);
            }

            private static void DoDump(object obj)
            {
                if (obj == null)
                {
                    _text.Append("null");
                    _text.Append(",");
                    return;
                }

                Type t = obj.GetType();

                //repeat field
                if (obj is IList)
                {
                    /*
                    _text.Append(t.FullName);
                    _text.Append(",");
                    AppendIndent(1);
                    */

                    _text.Append("[");
                    IList list = obj as IList;
                    foreach (object v in list)
                    {
                        DoDump(v);
                    }

                    _text.Append("]");
                }
                else if (t.IsValueType || obj is string || obj is ByteString)
                {
                    _text.Append(obj);
                    _text.Append(",");
                    AppendIndent(1);
                }
                else if (t.IsArray)
                {
                    var a = (Array) obj;
                    _text.Append("[");
                    for (int i = 0; i < a.Length; i++)
                    {
                        _text.Append(i);
                        _text.Append("=");
                        DoDump(a.GetValue(i));
                    }

                    _text.Append("]");
                }
                else if (t.IsClass)
                {
                    _text.Append($"<{t.Name}>");
                    _text.Append("{");
                    var fields = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    if (fields.Length > 0)
                    {
                        foreach (var info in fields)
                        {
                            _text.Append(info.Name);
                            _text.Append("=");
                            var value = info.GetGetMethod().Invoke(obj, null);
                            DoDump(value);
                        }
                    }

                    _text.Append("}");
                }
                else
                {
                    Debug.LogWarning("unsupported type: " + t.FullName);
                    _text.Append(obj);
                    _text.Append(",");
                    AppendIndent(1);
                }
            }

            private static string DumpAsString(object obj, string hint = "")
            {
                _text.Remove(0, _text.Length);
                _text.Append(hint);
                DoDump(obj);
                return _text.ToString();
            }

            public static void Dump(object obj, string hint = "")
            {
                //#if UNITY_EDITOR
                Debug.Log(DumpAsString(obj, hint));
                //#endif
            }
        }
    }
}