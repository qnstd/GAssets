using System.Collections.Generic;
using System.IO;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 运行时资源库资源内存检测
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        enum TabType
        {
            Asset,
            Bundle
        }
        Vector2 scrollV22 = Vector2.zero;
        AssetDetail currentDetail = null; // 当前选择查看的资源详细信息对象
        TabType tabType = TabType.Asset;


        private void OnEnable_RuntimeAnalyze() { }

        private void OnDisable_RuntimeAnalyze()
        {
            assetData.Clear();
            currentDetail = null;
            scrollV22 = Vector2.zero;
            tabType = TabType.Asset;
        }

        private void OnGUI_RuntimeAnalyze()
        {
            EditorGUILayout.LabelField("<color=#ffcc00>* 以下数据均在运行时内存中进行获取</color>", Gui.LabelStyle);

            float w = EditorGUIUtility.currentViewWidth;
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

            // Tab
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("helpbox");
            Color cc = GUI.backgroundColor;
            if (tabType == TabType.Asset)
                GUI.backgroundColor = Color.green;
            if(GUILayout.Button("资源", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                tabType = TabType.Asset;
            }
            GUI.backgroundColor = cc;
            cc = GUI.backgroundColor;
            if (tabType == TabType.Bundle)
                GUI.backgroundColor = Color.green;
            if (GUILayout.Button("AB包", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                tabType = TabType.Bundle;
            }
            GUI.backgroundColor = cc;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("清理数据", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
            {
                if (!EditorApplication.isPlaying) 
                {
                    OnDisable_RuntimeAnalyze();
                }
                else { Debug.LogWarning("运行时无法进行数据清理操作."); }
                
            }
            EditorGUILayout.EndHorizontal();


            if(tabType == TabType.Asset)
            {// 当页签为 Asset 时
                EditorGUILayout.BeginHorizontal("helpbox");
                EditorGUILayout.LabelField("资源", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField("内存引用计数", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField("依赖（AssetBundle）", Gui.LabelStyle, layoutw);
                EditorGUILayout.EndHorizontal();

                scrollV22 = EditorGUILayout.BeginScrollView(scrollV22);
                foreach (var k in assetData.Keys)
                {
                    EditorGUILayout.LabelField($"<color=#bdf6ff># {k}</color>", Gui.LabelStyle);
                    foreach (var m in assetData[k].Keys)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(m, Gui.LabelStyle, layoutw);
                        EditorGUILayout.LabelField(assetData[k][m].Ref.ToString(), Gui.LabelStyle, layoutw);
                        EditorGUILayout.BeginVertical(layoutw);
                        if (GUILayout.Button("", "ToolbarSearchTextField", GUILayout.Width(17), GUILayout.Height(15)))
                        {// 显示资源所依赖的ab包
                            currentDetail = assetData[k][m];
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        if (currentDetail != null && m == currentDetail.Key && currentDetail.Depends != null)
                        {
                            EditorGUI.indentLevel++;
                            foreach (var bundle in currentDetail.Depends)
                            {
                                if (bundle != null && bundle.AssetBundle != null)
                                {
                                    Color c = GUI.backgroundColor;
                                    GUI.backgroundColor = Color.yellow;
                                    EditorGUILayout.BeginVertical("box");
                                    EditorGUILayout.LabelField(bundle.AssetBundle.name, Gui.LabelStyle);
                                    EditorGUILayout.EndVertical();
                                    GUI.backgroundColor = c;
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
                EditorGUILayout.Space(2);
                EditorGUILayout.EndScrollView();
            }
            else if(tabType == TabType.Bundle)
            {// 当页签为 AB包 时
                foreach(var k in GBundleLoader.m_cache.Keys)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"{k} / <color=#ffcc00>({ EditorUtility.FormatBytes(GetABRuntimeSize(k)) })</color>", Gui.LabelStyle);
                    EditorGUILayout.EndVertical();
                }
            }
        }


        /// <summary>
        /// 得到 AB包 在当前平台下运行时的内存占用大小
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        private long GetABRuntimeSize(string k)
        {
            string[] childs = AssetDatabase.GetAssetPathsFromAssetBundle(k);
            long allbytes = 0;
            for (int i = 0; i < childs.Length; i++)
            {
                allbytes += Profiler.GetRuntimeMemorySizeLong(AssetDatabase.LoadAssetAtPath<Object>(childs[i]));
            }
            return allbytes;
        }



        #region 获取数据
        // 资源对象详细信息结构体
        class AssetDetail
        {
            public string Key;
            public int Ref;
            public List<GBundleLoader> Depends;
        }
        // 存储所有动态创建的资源对象信息组
        Dictionary<string, Dictionary<string, AssetDetail>> assetData = new Dictionary<string, Dictionary<string, AssetDetail>>();


        /// <summary>
        /// 通过资源文件类型获取对应的管理组
        /// </summary>
        /// <param name="k">资源文件路径（以asset为开头的路径）</param>
        /// <returns></returns>
        Dictionary<string, AssetDetail> GetGroupByType(string k)
        {
            string typ = Path.GetExtension(k)[1..];
            Dictionary<string, AssetDetail> dic;
            if (!assetData.ContainsKey(typ))
            {
                dic = new Dictionary<string, AssetDetail>();
                assetData.Add(typ, dic);
            }
            else
            {
                assetData.TryGetValue(typ, out dic);
            }
            return dic;
        }


        /// <summary>
        /// 创建一个新的资源详细信息结构对象
        /// </summary>
        /// <param name="k">资源文件路径（以asset为开头的路径）</param>
        /// <param name="refval">资源当前引用值</param>
        /// <returns></returns>
        private AssetDetail CreateAssetAnalyzeObject(string k, int refval)
        {
            return new AssetDetail()
            {
                Key = k,
                Ref = refval,
                Depends = GDependLoader.m_cache[k].Bundles
            };
        }

        /// <summary>
        /// 在 Editor 的运行时状态下，实时获取动态创建的资源信息数据
        /// </summary>
        private void OnUpdate_RuntimeAnalyze()
        {
            if (!EditorApplication.isPlaying) { return; }

            // 清理
            assetData.Clear();

            // 获取数据
            foreach (var k in GAssetLoader.m_cache.Keys)
            {
                Dictionary<string, AssetDetail> dic = GetGroupByType(k);
                dic.Add(k, CreateAssetAnalyzeObject(k, GAssetLoader.m_cache[k].Ref));
            }
            foreach (var k in GDependLoader.m_cache.Keys)
            {
                Dictionary<string, AssetDetail> dic = GetGroupByType(k);
                if (!dic.TryGetValue(k, out _))
                {
                    dic.Add(k, CreateAssetAnalyzeObject(k, GDependLoader.m_cache[k].Ref));
                }
            }

            //Repaint();
        }
        #endregion
    }


}