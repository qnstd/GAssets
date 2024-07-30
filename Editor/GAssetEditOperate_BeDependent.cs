using System.Collections.Generic;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 资源被依赖关系
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        private Vector2 m_scrollpos = Vector2.zero;
        List<string> m_lst;
        //文件被依赖关系缓存（key：文件；value：依赖Key的文件数组）
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
            EditorGUILayout.LabelField("资源对象", Gui.LabelStyle, GUILayout.Width(57));
            m_obj = EditorGUILayout.ObjectField("", m_obj, typeof(Object), false);
            if (GUILayout.Button("查询", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(20)))
            {
                Search();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            EditorGUI.indentLevel += 4;
            EditorGUILayout.BeginHorizontal();
            m_readAllDepends = EditorGUILayout.Toggle("", m_readAllDepends, GUILayout.Width(20));
            EditorGUILayout.LabelField("<color=#cccccc>是否重新拉取所有依赖关系</color>", Gui.LabelStyle);
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
                    if (GUILayout.Button("定位", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(f));
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }


        /// <summary>
        /// 搜索
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
            {//资源未被任何其他资源依赖
                Dialog.Tip("未被其他资源引用.");
            }
            else
            {
                m_have.Add(asset, beDependent);
            }
        }


        /// <summary>
        /// 查找所有依赖关系
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
                string[] depends = AssetDatabase.GetDependencies(assetPath, false); //不递归搜索，直接获取

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

                //显示进度
                proval = (float)(count * 1.0f / guidsLen);
                pro = (proval * 100).ToString("f1") + "%";
                EditorUtility.DisplayProgressBar("检索依赖关系", "进度：" + pro, proval);
            }
            EditorUtility.ClearProgressBar();
        }

    }
}