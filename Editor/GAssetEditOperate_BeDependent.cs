using System.Collections.Generic;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ��Դ��������ϵ
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        private Vector2 m_scrollpos = Vector2.zero;
        List<string> m_lst;
        //�ļ���������ϵ���棨key���ļ���value������Key���ļ����飩
        static private Dictionary<string, List<string>> m_cache = new Dictionary<string, List<string>>();
        static private Dictionary<string, List<string>> m_have = new Dictionary<string, List<string>>();

        Object m_obj = null;
        bool m_readAllDepends = true;


        private void OnEnable_BeDependent() { }
        private void OnDisable_BeDependent()
        {
            m_cache.Clear();
            m_have.Clear();
            m_scrollpos = Vector2.zero;
            m_obj = null;
        }
        private void OnGUI_BeDependent()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("��Դ����", Gui.LabelStyle, GUILayout.Width(57));
            m_obj = EditorGUILayout.ObjectField("", m_obj, typeof(Object), false);
            if (GUILayout.Button("��ѯ", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(20)))
            {
                Search();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            EditorGUI.indentLevel += 4;
            EditorGUILayout.BeginHorizontal();
            m_readAllDepends = EditorGUILayout.Toggle("", m_readAllDepends, GUILayout.Width(20));
            EditorGUILayout.LabelField("<color=#cccccc>�Ƿ�������ȡ����������ϵ</color>", Gui.LabelStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel -= 4;
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();


            if (m_obj == null)
            {
                m_have.Clear();
            }


            m_scrollpos = GUILayout.BeginScrollView(m_scrollpos, "box");
            foreach (string s in m_have.Keys)
            {
                m_have.TryGetValue(s, out m_lst);
                for (int i = 0; i < m_lst.Count; i++)
                {
                    string f = m_lst[i];
                    GUILayout.BeginHorizontal("box", GUILayout.Height(14));
                    EditorGUILayout.LabelField(f, Gui.LabelStyle);
                    if (GUILayout.Button("��λ", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(f));
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }


        /// <summary>
        /// ����
        /// </summary>
        private void Search()
        {
            if (m_obj == null) { return; }

            if (m_readAllDepends)
                AllDepends();

            m_have.Clear();

            List<string> beDependent;
            string asset = AssetDatabase.GetAssetPath(m_obj);
            if (!m_cache.TryGetValue(asset, out beDependent))
            {//��Դδ���κ�������Դ����
                Dialog.Tip("δ��������Դ����.");
            }
            else
            {
                m_have.Add(asset, beDependent);
            }
        }


        /// <summary>
        /// ��������������ϵ
        /// </summary>
        private void AllDepends()
        {
            m_cache.Clear();

            int count = 0;
            string pro;
            float proval;

            string[] guids = AssetDatabase.FindAssets("");
            int guidsLen = guids.Length;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string[] depends = AssetDatabase.GetDependencies(assetPath, false); //���ݹ�������ֱ�ӻ�ȡ

                foreach (string file in depends)
                {
                    List<string> lst;
                    if (!m_cache.TryGetValue(file, out lst))
                    {
                        lst = new List<string>();
                        m_cache[file] = lst;
                    }
                    lst.Add(assetPath);
                }

                count++;

                //��ʾ����
                proval = (float)(count * 1.0f / guidsLen);
                pro = (proval * 100).ToString("f1") + "%";
                EditorUtility.DisplayProgressBar("����������ϵ", "���ȣ�" + pro, proval);
            }
            EditorUtility.ClearProgressBar();
        }

    }
}