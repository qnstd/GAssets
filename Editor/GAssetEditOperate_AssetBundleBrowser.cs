using System.Collections.Generic;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// AB����Ϣ
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class EditorABInformation
    {
        public string m_name = "";
        public long m_size = 0;
        public List<EditorABChildInformation> m_childs = null;
        public string[] m_depends = null;
    }


    /// <summary>
    /// AB����Ϣ�а�����Ԫ�ص���Ϣ
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class EditorABChildInformation
    {
        public string m_path = "";
        public long m_size = 0;
    }


    /// <summary>
    /// AB����Ϣ���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        Vector2 m_scrollviewV2;
        Vector2 m_scrollviewV2_C;
        Vector2 m_scrollviewV2_t;
        string m_search = "";
        int m_tabCurSelect = -1;
        List<EditorABInformation> m_abLst = new List<EditorABInformation>();
        EditorABInformation m_CurrentABInfo = null;

        Color m_tabLightColor = new Color(164 / 255.0f, 230 / 255.0f, 131 / 255.0f);


        /// <summary>
        /// ��ȡ��ǰ���������е�ab����Ϣ
        /// </summary>
        private void GetAllAbInformation()
        {
            m_abLst.Clear();
            string[] allAbNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in allAbNames)
            {
                if (name.LastIndexOf(".ab") == -1)
                {
                    continue;
                }
                //ab�а�������Ԫ����Ϣ
                long allbytes = 0;
                List<EditorABChildInformation> childLst = new List<EditorABChildInformation>();
                string[] childs = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                for (int i = 0; i < childs.Length; i++)
                {
                    string p = childs[i];
                    EditorABChildInformation c = new EditorABChildInformation()
                    {
                        m_size = CalculateMemory(p),
                        m_path = p
                    };
                    childLst.Add(c);
                    allbytes += c.m_size;
                }

                //����ab��Ϣ�������б�
                EditorABInformation ab = new EditorABInformation()
                {
                    m_name = name,
                    m_size = allbytes,
                    m_childs = childLst,
                    m_depends = AssetDatabase.GetAssetBundleDependencies(name, true)
                };
                m_abLst.Add(ab);
            }
        }


        private void OnEnable_AssetBundleBrowser() { }
        private void OnDisable_AssetBundleBrowser()
        {
            m_search = "";
            m_tabCurSelect = -1;
            m_abLst.Clear();
            m_CurrentABInfo = null;
        }
        private void OnGUI_AssetBundleBrowser()
        {
            EditorGUILayout.BeginHorizontal();

            #region �б�
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            m_search = EditorGUILayout.TextField("", m_search, "SearchTextField");
            if (GUILayout.Button("��ȡ", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                Refresh();
            }
            EditorGUILayout.EndHorizontal();

            m_scrollviewV2 = EditorGUILayout.BeginScrollView(m_scrollviewV2, "box");
            for (int i = 0; i < m_abLst.Count; i++)
            {
                EditorABInformation abi = m_abLst[i];
                if (abi.m_name.ToLower().Contains(m_search.ToLower()))
                {
                    Color c = GUI.backgroundColor;
                    GUI.backgroundColor = (m_tabCurSelect == i) ? m_tabLightColor : Color.white;
                    if (GUILayout.Button(abi.m_name, Gui.BtnStyle2))
                    {
                        TabSelect(i);
                    }
                    GUI.backgroundColor = c;
                    EditorGUILayout.Space(-8);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            #endregion


            #region ��ϸ��Ϣ
            EditorGUILayout.BeginVertical();
            if (m_CurrentABInfo != null)
            {
                EditorGUILayout.LabelField("��Ϣ" + "  <color=#b6ea94>" + EditorUtility.FormatBytes(m_CurrentABInfo.m_size) + "</color>" + "   <color=#666666>(������ʾ�ĳߴ�Ϊ����ʱ�ڴ�ռ�õĴ�С)</color>", Gui.LabelStyle);
                m_scrollviewV2_C = EditorGUILayout.BeginScrollView(m_scrollviewV2_C, "box");
                if (m_CurrentABInfo.m_childs != null && m_CurrentABInfo.m_childs.Count != 0)
                {
                    for (int i = 0; i < m_CurrentABInfo.m_childs.Count; i++)
                    {
                        EditorABChildInformation child = m_CurrentABInfo.m_childs[i];
                        GUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(child.m_path + "  <color=#ffcc00>(" + EditorUtility.FormatBytes(child.m_size) + ")</color>", Gui.LabelStyle);
                        if (GUILayout.Button("��λ", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                        {
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(child.m_path));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();


                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("������", Gui.LabelStyle);
                m_scrollviewV2_t = EditorGUILayout.BeginScrollView(m_scrollviewV2_t, "box");
                foreach (string s in m_CurrentABInfo.m_depends)
                {
                    GUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(s, Gui.LabelStyle);
                    if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                    {
                        GUIUtility.systemCopyBuffer = s;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
            #endregion

            EditorGUILayout.EndHorizontal();
        }


        /// <summary>
        /// ˢ��ab����Ϣ�б�
        /// </summary>
        private void Refresh()
        {
            GetAllAbInformation();

            m_tabCurSelect = -1;
            m_CurrentABInfo = null;
            m_search = "";

            if (m_abLst == null || m_abLst.Count == 0)
            {
                Dialog.Tip("δ��ѯ�� AssetBundle �����Ϣ.");
            }
        }


        /// <summary>
        /// tabѡ��
        /// </summary>
        /// <param name="i">ѡ������</param>
        private void TabSelect(int i)
        {
            if (m_tabCurSelect == i) { return; }
            m_tabCurSelect = i;
            m_CurrentABInfo = m_abLst[m_tabCurSelect];
        }



        /// <summary>
        /// ��ȡ��Դ�ڴ�
        /// </summary>
        /// <param name="p">��Դ·��</param>
        /// <returns></returns>
        private long CalculateMemory(string p)
        {
            // �����ڴ�
            //return new FileInfo(p).Length;

            // ��ǰʹ�ñ���������ʱ�ڴ�
            return Profiler.GetRuntimeMemorySizeLong(AssetDatabase.LoadAssetAtPath<Object>(p));

            // ��Դ�ı����洢�ڴ棨Ŀǰunity dllֻ�ṩ��������Դ�Ļ�ȡ��������Դ��ʱû���ҵ��ɷ���ĺ�����
            //System.Type t = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
            //MethodInfo method = t.GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            //return (long)method.Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<Texture>(p) });
        }

    }
}