using System.Collections.Generic;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// AB包信息
    /// <para>作者：强辰</para>
    /// </summary>
    public class EditorABInformation
    {
        public string m_name = "";
        public long m_size = 0;
        public List<EditorABChildInformation> m_childs = null;
        public string[] m_depends = null;
    }


    /// <summary>
    /// AB包信息中包含子元素的信息
    /// <para>作者：强辰</para>
    /// </summary>
    public class EditorABChildInformation
    {
        public string m_path = "";
        public long m_size = 0;
    }


    /// <summary>
    /// AB包信息浏览
    /// <para>作者：强辰</para>
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
        /// 获取当前工程下所有的ab包信息
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
                //ab中包含的子元素信息
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

                //创建ab信息并加入列表
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

            #region 列表
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            m_search = EditorGUILayout.TextField("", m_search, "SearchTextField");
            if (GUILayout.Button("拉取", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
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


            #region 详细信息
            EditorGUILayout.BeginVertical();
            if (m_CurrentABInfo != null)
            {
                EditorGUILayout.LabelField("信息" + "  <color=#b6ea94>" + EditorUtility.FormatBytes(m_CurrentABInfo.m_size) + "</color>" + "   <color=#666666>(以下显示的尺寸为运行时内存占用的大小)</color>", Gui.LabelStyle);
                m_scrollviewV2_C = EditorGUILayout.BeginScrollView(m_scrollviewV2_C, "box");
                if (m_CurrentABInfo.m_childs != null && m_CurrentABInfo.m_childs.Count != 0)
                {
                    for (int i = 0; i < m_CurrentABInfo.m_childs.Count; i++)
                    {
                        EditorABChildInformation child = m_CurrentABInfo.m_childs[i];
                        GUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(child.m_path + "  <color=#ffcc00>(" + EditorUtility.FormatBytes(child.m_size) + ")</color>", Gui.LabelStyle);
                        if (GUILayout.Button("定位", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                        {
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(child.m_path));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();


                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("依赖项", Gui.LabelStyle);
                m_scrollviewV2_t = EditorGUILayout.BeginScrollView(m_scrollviewV2_t, "box");
                foreach (string s in m_CurrentABInfo.m_depends)
                {
                    GUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(s, Gui.LabelStyle);
                    if (GUILayout.Button("复制", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
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
        /// 刷新ab包信息列表
        /// </summary>
        private void Refresh()
        {
            GetAllAbInformation();

            m_tabCurSelect = -1;
            m_CurrentABInfo = null;
            m_search = "";

            if (m_abLst == null || m_abLst.Count == 0)
            {
                Dialog.Tip("未查询到 AssetBundle 相关信息.");
            }
        }


        /// <summary>
        /// tab选择
        /// </summary>
        /// <param name="i">选择索引</param>
        private void TabSelect(int i)
        {
            if (m_tabCurSelect == i) { return; }
            m_tabCurSelect = i;
            m_CurrentABInfo = m_abLst[m_tabCurSelect];
        }



        /// <summary>
        /// 获取资源内存
        /// </summary>
        /// <param name="p">资源路径</param>
        /// <returns></returns>
        private long CalculateMemory(string p)
        {
            // 物理内存
            //return new FileInfo(p).Length;

            // 当前使用本机的运行时内存
            return Profiler.GetRuntimeMemorySizeLong(AssetDatabase.LoadAssetAtPath<Object>(p));

            // 资源的本机存储内存（目前unity dll只提供了纹理资源的获取，其他资源暂时没有找到可反射的函数）
            //System.Type t = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
            //MethodInfo method = t.GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            //return (long)method.Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<Texture>(p) });
        }

    }
}