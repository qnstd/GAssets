using System.Collections.Generic;
using System.IO;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 运行时资源库资源内存检测
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        #region 数据结构及变量
        // Tab页类型
        enum TabType
        {
            Asset,
            Bundle
        }
        // 资源对象详细信息结构体
        class AssetDetail
        {
            public string Key;
            public int Ref;
            public List<GBundleLoader> Depends;
        }

        Vector2 scrollV22 = Vector2.zero;
        TabType tabType = TabType.Asset;
        AssetDetail currentDetail = null; // 当前选择查看的资源详细信息对象
        ABInfo currentABDetail = null; // 当前选择查看的AB包包含的资源信息
        string restypSearch = ""; // 资源类型搜索结果
        string resSearch = ""; // 资源搜索结果
        string abSearch = ""; // 在ab包页签下搜索的结果
        Dictionary<string, Dictionary<string, AssetDetail>> assetData = new Dictionary<string, Dictionary<string, AssetDetail>>(); // 存储所有动态创建的资源对象信息组
        float refreshInterval = 1; // 数据刷新间隔，单位：秒
        float currentInterval = 0; // 当前记录的时间
        long allsize = 0; // 内存总占用
        #endregion



        private void OnEnable_RuntimeAnalyze() { }

        private void OnDisable_RuntimeAnalyze()
        {
            assetData.Clear();
            currentDetail = null;
            currentABDetail = null;
            scrollV22 = Vector2.zero;
            tabType = TabType.Asset;
            currentInterval = 0;
            abSearch = "";
            restypSearch = "";
            resSearch = "";
            allsize = 0;
        }

        private void OnGUI_RuntimeAnalyze()
        {
            EditorGUILayout.LabelField("<color=#ffcc00>* 以下数据均在运行时内存中进行获取</color>", Gui.LabelStyle);

            float w = EditorGUIUtility.currentViewWidth;
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

            #region 全局设置
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数据刷新间隔: ", Gui.LabelStyle, GUILayout.Width(80));
            refreshInterval = EditorGUILayout.FloatField(refreshInterval, GUILayout.Width(50));
            EditorGUILayout.LabelField("s", Gui.LabelStyle, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Tab页签
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("helpbox");
            Color cc = GUI.backgroundColor;
            if (tabType == TabType.Asset)
                GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("资源", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                tabType = TabType.Asset;
            }
            GUI.backgroundColor = cc;
            cc = GUI.backgroundColor;
            if (tabType == TabType.Bundle)
                GUI.backgroundColor = Color.cyan;
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
            #endregion

            #region 每个页签的GUI刷新
            if (tabType == TabType.Asset)
            {// 当页签为 Asset 时
                EditorGUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("类型: ", Gui.LabelStyle, GUILayout.Width(30));
                restypSearch = EditorGUILayout.TextField("", restypSearch, "ToolbarSearchTextField", GUILayout.Width(80));
                EditorGUILayout.LabelField("资源: ", Gui.LabelStyle, GUILayout.Width(30));
                resSearch = EditorGUILayout.TextField("", resSearch, "ToolbarSearchTextField", GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("helpbox");
                EditorGUILayout.LabelField("资源", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField("内存引用计数", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField("操作 (定位、AB信息)", Gui.LabelStyle, layoutw);
                EditorGUILayout.EndHorizontal();

                // 资源内存信息
                scrollV22 = EditorGUILayout.BeginScrollView(scrollV22);
                foreach (var k in assetData.Keys)
                {
                    if (k.Contains(restypSearch))
                    {
                        EditorGUILayout.LabelField($"<color=#bdf6ff># {k}</color>", Gui.LabelStyle);
                        foreach (var m in assetData[k].Keys)
                        {
                            if (m.Contains(resSearch))
                            {
                                EditorGUILayout.BeginHorizontal("helpbox");
                                EditorGUILayout.LabelField(m, Gui.LabelStyle, layoutw);
                                EditorGUILayout.LabelField(assetData[k][m].Ref.ToString(), Gui.LabelStyle, layoutw);
                                EditorGUILayout.BeginVertical(layoutw);
                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button("", "ToolbarSearchTextFieldPopup", GUILayout.Width(17), GUILayout.Height(15)))
                                {// 在Project面板中定位资源
                                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(m));
                                }
                                if (GUILayout.Button("", "ToolbarSearchTextField", GUILayout.Width(17), GUILayout.Height(15)))
                                {// 显示资源所依赖的ab包
                                    currentDetail = assetData[k][m];
                                }
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();
                                if (currentDetail != null && m == currentDetail.Key && currentDetail.Depends != null)
                                {
                                    EditorGUI.indentLevel++;
                                    foreach (var bundle in currentDetail.Depends)
                                    {
                                        if (bundle != null && bundle.AssetBundle != null)
                                        {
                                            EditorGUILayout.BeginHorizontal("box");
                                            EditorGUILayout.LabelField(bundle.AssetBundle.name, Gui.LabelStyle);
                                            GUILayout.FlexibleSpace();
                                            if (GUILayout.Button("复制", Gui.BtnStyle, GUILayout.Width(40), GUILayout.Height(18)))
                                            {
                                                GUIUtility.systemCopyBuffer = bundle.AssetBundle.name;
                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                    EditorGUI.indentLevel--;
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.Space(2);
                EditorGUILayout.EndScrollView();
            }
            else if (tabType == TabType.Bundle)
            {// 当页签为 AB包 时
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField($"总占用: <color=#ff6b8e>{EditorUtility.FormatBytes(allsize)}</color>", Gui.LabelStyle);
                GUILayout.FlexibleSpace();
                abSearch = EditorGUILayout.TextField("", abSearch, "ToolbarSearchTextField", GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                allsize = 0;
                foreach (var k in GBundleLoader.m_cache.Keys)
                {
                    long size = Profilers.GetABRuntimeSize(k);
                    allsize += size;
                    if (k.Contains(abSearch))
                    {
                        EditorGUILayout.BeginHorizontal("helpbox");
                        EditorGUILayout.LabelField($"{k} <color=#ffcc00>({EditorUtility.FormatBytes(size)})</color>", Gui.LabelStyle);
                        if (GUILayout.Button("", "ToolbarSearchTextField", GUILayout.Width(17), GUILayout.Height(15)))
                        {// 显示ab包内包含的资源信息
                            currentABDetail = GAssetManifest.GetABInfo(k);
                        }
                        EditorGUILayout.EndVertical();
                        if (currentABDetail != null && k == currentABDetail.m_name)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField($"<color=#bdf6ff># 子元素</color>", Gui.LabelStyle);
                            foreach (var content in currentABDetail.m_contains)
                            {
                                EditorGUILayout.BeginHorizontal("box");
                                EditorGUILayout.LabelField($"{content} <color=#ffaa00>({EditorUtility.FormatBytes(Profilers.GetResRuntimeSize(content))})</color>", Gui.LabelStyle);
                                if (GUILayout.Button("定位", Gui.BtnStyle, GUILayout.Width(40), GUILayout.Height(18)))
                                {
                                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(content));
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            if (currentABDetail.m_depends != null)
                            {
                                EditorGUILayout.LabelField($"<color=#bdf6ff># 依赖包</color>", Gui.LabelStyle);
                                foreach (var content in currentABDetail.m_depends)
                                {
                                    EditorGUILayout.BeginHorizontal("box");
                                    EditorGUILayout.LabelField($"{content} <color=#ffcc00>({EditorUtility.FormatBytes(Profilers.GetABRuntimeSize(content))})</color>", Gui.LabelStyle);
                                    if (GUILayout.Button("复制", Gui.BtnStyle, GUILayout.Width(40), GUILayout.Height(18)))
                                    {
                                        GUIUtility.systemCopyBuffer = content;
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
            #endregion
        }



        #region 获取数据
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
            if (!EditorApplication.isPlaying)
            {
                currentInterval = 0;
                assetData.Clear();
                return;
            }


            currentInterval += Time.deltaTime;
            if (currentInterval >= refreshInterval)
            {
                currentInterval = 0;

                // 清理
                assetData.Clear();

                // 获取数据
                foreach (var k in GAssetLoader.m_cache.Keys)
                {
                    Dictionary<string, AssetDetail> dic = GetGroupByType(k);
                    dic.Add(k, CreateAssetAnalyzeObject(k, GAssetLoader.m_cache[k].Ref));
                }
                foreach (var k in GDependLoader.m_cache.Keys)
                {// 循环检测 GDependLoader 是因为场景加载直接使用的是此加载器
                    Dictionary<string, AssetDetail> dic = GetGroupByType(k);
                    if (!dic.TryGetValue(k, out _))
                    {
                        dic.Add(k, CreateAssetAnalyzeObject(k, GDependLoader.m_cache[k].Ref));
                    }
                }

                // 重绘GUI
                Repaint();
            }
        }
        #endregion
    }


}