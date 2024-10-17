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
        TabType tabType = TabType.Asset;
        Vector2 scrollV22 = Vector2.zero;
        GDependLoader currentResDetail = null; // 当前选择查看的资源详细信息对象
        ABInfo currentABDetail = null; // 当前选择查看的AB包包含的资源信息
        string restypSearch = ""; // 资源类型搜索结果
        string resSearch = ""; // 资源搜索结果
        string abSearch = ""; // 在ab包页签下搜索的结果
        float refreshInterval = 1; // 数据刷新间隔，单位：秒
        float currentInterval = 0; // 当前记录的时间
        long allsize = 0; // 内存总占用
        #endregion



        private void OnEnable_RuntimeAnalyze() { }

        private void OnDisable_RuntimeAnalyze()
        {
            currentResDetail = null;
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
            float w = EditorGUIUtility.currentViewWidth;
            EditorGUILayout.LabelField("<color=#ffffff>* 以下数据均在运行时内存中进行获取</color>", Gui.LabelStyle);

            #region 全局设置
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("刷新间隔: ", Gui.LabelStyle, GUILayout.Width(50));
            refreshInterval = EditorGUILayout.FloatField(refreshInterval, GUILayout.Width(50));
            EditorGUILayout.LabelField("s <color=#ffcc00>( 用于检视板未激活状态下刷新的时间间隔 )</color>", Gui.LabelStyle);
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


            if (!EditorApplication.isPlaying)
            {// 在非运行时状态下，不做任何绘制
                return;
            }

            #region 每个页签的 GUI 刷新
            if (tabType == TabType.Asset)
            {
                DrawTabe_Asset(w);
            }
            else if (tabType == TabType.Bundle)
            {
                DrawTable_Bundle();
            }
            else { }
            #endregion
        }



        /// <summary>
        /// 绘制资源页签
        /// </summary>
        /// <param name="w"></param>
        private void DrawTabe_Asset(float w)
        {
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

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
            foreach (var k in GAssetLoader.m_cache.Keys)
            {
                DrawTable_Asset__(k, GAssetLoader.m_cache[k].Ref, layoutw);
            }
            foreach (var k in GSceneOperate.Data)
            {
                DrawTable_Asset__(k.Path, GDependLoader.m_cache[k.Path].Ref, layoutw);
            }
            // 结束

            EditorGUILayout.Space(2);
            EditorGUILayout.EndScrollView();
        }
        private void DrawTable_Asset__(string k, int refs, GUILayoutOption layoutw)
        {
            if (Path.GetExtension(k)[1..].Contains(restypSearch) && Path.GetFileNameWithoutExtension(k).Contains(resSearch))
            {
                EditorGUILayout.BeginHorizontal("helpbox");
                EditorGUILayout.LabelField($"<color=#bdf6ff>{k}</color>", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField(refs.ToString(), Gui.LabelStyle, layoutw);
                EditorGUILayout.BeginVertical(layoutw);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("", "ToolbarSearchTextFieldPopup", GUILayout.Width(17), GUILayout.Height(15)))
                {// 在Project面板中定位资源
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(k));
                }
                if (GUILayout.Button("", "ToolbarSearchTextField", GUILayout.Width(17), GUILayout.Height(15)))
                {// 记录当前资源所依赖的ab包
                    currentResDetail = GDependLoader.m_cache[k];
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                // 显示资源的依赖
                if (currentResDetail != null && k == currentResDetail.Path && currentResDetail.Bundles != null)
                {
                    EditorGUI.indentLevel++;
                    foreach (var bundle in currentResDetail.Bundles)
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



        /// <summary>
        /// 绘制 AB包 页签
        /// </summary>
        private void DrawTable_Bundle()
        {
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
                    EditorGUILayout.LabelField($"{k} <color=#ffffff><b>({EditorUtility.FormatBytes(size)})</b></color>", Gui.LabelStyle);
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
                                EditorGUILayout.LabelField($"{content} <color=#ffffff>({EditorUtility.FormatBytes(Profilers.GetABRuntimeSize(content))})</color>", Gui.LabelStyle);
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



        private void OnUpdate_RuntimeAnalyze()
        {
            if (!EditorApplication.isPlaying)
            {
                currentInterval = 0;
                return;
            }

            // 用于在 Editor 运行时状态下，检视板在未激活状态下能及时刷新
            currentInterval += Time.deltaTime;
            if (currentInterval >= refreshInterval)
            {
                currentInterval = 0;
                // 重绘GUI
                Repaint();
            }
        }
    }
}