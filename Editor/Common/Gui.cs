using System;
using System.Collections.Generic;
using cngraphi.gassets.common;
using UnityEditor;
using UnityEngine;


namespace cngraphi.gassets.editor.common
{
    /// <summary>
    /// 窗体工具
    /// <para>作者：强辰</para>
    /// </summary>
    public class Gui
    {
        #region Style
        static GUIStyle m_BtnStyle;
        static GUIStyle m_BtnStyle2;
        static GUIStyle m_LabelStyle;
        static GUIStyle m_LabelStyleMiddle;
        static GUIStyle m_LabelHead;
        static GUIStyle m_LabelHeadLeft;
        static public GUIStyle BtnStyle
        {
            get
            {
                if (m_BtnStyle == null)
                {
                    m_BtnStyle = new GUIStyle("IN EditColliderButton") { richText = true, fontSize = 10 };
                }
                return m_BtnStyle;
            }
        }
        static public GUIStyle BtnStyle2
        {
            get
            {
                if (m_BtnStyle2 == null)
                {
                    m_BtnStyle2 = new GUIStyle("AC Button") { richText = true, fontSize = 11, alignment = TextAnchor.MiddleLeft };
                }
                return m_BtnStyle2;
            }
        }
        static public GUIStyle LabelStyle
        {
            get
            {
                if (m_LabelStyle == null)
                {
                    m_LabelStyle = new GUIStyle("MiniLabel") { richText = true, fontSize = 10 };
                }
                return m_LabelStyle;
            }
        }
        static public GUIStyle LabelStyleMiddle
        {
            get
            {
                if (m_LabelStyleMiddle == null)
                {
                    m_LabelStyleMiddle = new GUIStyle("MiniLabel") { richText = true, fontSize = 10, alignment = TextAnchor.MiddleCenter };
                }
                return m_LabelStyleMiddle;
            }
        }
        static public GUIStyle LabelHead
        {
            get
            {
                if (m_LabelHead == null)
                {
                    m_LabelHead = new GUIStyle("AM MixerHeader2") { richText = true, alignment = TextAnchor.MiddleCenter };
                }
                return m_LabelHead;
            }
        }
        static public GUIStyle LabelHeadLeft
        {
            get
            {
                if (m_LabelHeadLeft == null)
                {
                    m_LabelHeadLeft = new GUIStyle("AM MixerHeader2") { richText = true, alignment = TextAnchor.MiddleLeft };
                }
                return m_LabelHeadLeft;
            }
        }
        #endregion


        /// <summary>
        /// 创建可由外部拖拽的文本域
        /// </summary>
        static public void DragTextArea(EditorWindow win, int h, ref string content, ref string[] contentAry, Action drag = null, Action checkChange = null)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, h);
            EditorGUILayout.Space(-h);
            EditorGUI.BeginChangeCheck();
            content = EditorGUILayout.TextArea(content, GUILayout.Height(h));
            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(content))
                {
                    if (contentAry != null) { Array.Clear(contentAry, 0, contentAry.Length); }
                    contentAry = null;
                }
                else
                {
                    Str.Split(content, "\n", out List<string> rs);
                    contentAry = rs.ToArray();
                }
                checkChange?.Invoke();
            }

            // 拖拽监听
            if (EditorWindow.mouseOverWindow == win && rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else if (Event.current.type == EventType.DragExited)
                {
                    win.Focus();
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length != 0)
                    {
                        string[] paths = DragAndDrop.paths;
                        if (contentAry == null)
                        {
                            contentAry = paths;
                        }
                        else
                        {
                            foreach (string s in paths)
                            {
                                if (Array.IndexOf(contentAry, s) != -1) { continue; }
                                int newlen = contentAry.Length + 1;
                                Array.Resize(ref contentAry, newlen);
                                contentAry[newlen - 1] = s;
                            }
                        }
                        content = string.Join("\n", contentAry);
                        drag?.Invoke();
                    }
                }
            }
        }

    }
}