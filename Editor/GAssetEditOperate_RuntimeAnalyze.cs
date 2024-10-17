using System.IO;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;


namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ����ʱ��Դ����Դ�ڴ���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        #region ���ݽṹ������
        // Tabҳ����
        enum TabType
        {
            Asset,
            Bundle
        }
        TabType tabType = TabType.Asset;
        Vector2 scrollV22 = Vector2.zero;
        GDependLoader currentResDetail = null; // ��ǰѡ��鿴����Դ��ϸ��Ϣ����
        ABInfo currentABDetail = null; // ��ǰѡ��鿴��AB����������Դ��Ϣ
        string restypSearch = ""; // ��Դ�����������
        string resSearch = ""; // ��Դ�������
        string abSearch = ""; // ��ab��ҳǩ�������Ľ��
        float refreshInterval = 1; // ����ˢ�¼������λ����
        float currentInterval = 0; // ��ǰ��¼��ʱ��
        long allsize = 0; // �ڴ���ռ��
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
            EditorGUILayout.LabelField("<color=#ffffff>* �������ݾ�������ʱ�ڴ��н��л�ȡ</color>", Gui.LabelStyle);

            #region ȫ������
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ˢ�¼��: ", Gui.LabelStyle, GUILayout.Width(50));
            refreshInterval = EditorGUILayout.FloatField(refreshInterval, GUILayout.Width(50));
            EditorGUILayout.LabelField("s <color=#ffcc00>( ���ڼ��Ӱ�δ����״̬��ˢ�µ�ʱ���� )</color>", Gui.LabelStyle);
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Tabҳǩ
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("helpbox");
            Color cc = GUI.backgroundColor;
            if (tabType == TabType.Asset)
                GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("��Դ", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                tabType = TabType.Asset;
            }
            GUI.backgroundColor = cc;
            cc = GUI.backgroundColor;
            if (tabType == TabType.Bundle)
                GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("AB��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                tabType = TabType.Bundle;
            }
            GUI.backgroundColor = cc;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("��������", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
            {
                if (!EditorApplication.isPlaying)
                {
                    OnDisable_RuntimeAnalyze();
                }
                else { Debug.LogWarning("����ʱ�޷����������������."); }

            }
            EditorGUILayout.EndHorizontal();
            #endregion


            if (!EditorApplication.isPlaying)
            {// �ڷ�����ʱ״̬�£������κλ���
                return;
            }

            #region ÿ��ҳǩ�� GUI ˢ��
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
        /// ������Դҳǩ
        /// </summary>
        /// <param name="w"></param>
        private void DrawTabe_Asset(float w)
        {
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

            EditorGUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("����: ", Gui.LabelStyle, GUILayout.Width(30));
            restypSearch = EditorGUILayout.TextField("", restypSearch, "ToolbarSearchTextField", GUILayout.Width(80));
            EditorGUILayout.LabelField("��Դ: ", Gui.LabelStyle, GUILayout.Width(30));
            resSearch = EditorGUILayout.TextField("", resSearch, "ToolbarSearchTextField", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("helpbox");
            EditorGUILayout.LabelField("��Դ", Gui.LabelStyle, layoutw);
            EditorGUILayout.LabelField("�ڴ����ü���", Gui.LabelStyle, layoutw);
            EditorGUILayout.LabelField("���� (��λ��AB��Ϣ)", Gui.LabelStyle, layoutw);
            EditorGUILayout.EndHorizontal();

            // ��Դ�ڴ���Ϣ
            scrollV22 = EditorGUILayout.BeginScrollView(scrollV22);
            foreach (var k in GAssetLoader.m_cache.Keys)
            {
                DrawTable_Asset__(k, GAssetLoader.m_cache[k].Ref, layoutw);
            }
            foreach (var k in GSceneOperate.Data)
            {
                DrawTable_Asset__(k.Path, GDependLoader.m_cache[k.Path].Ref, layoutw);
            }
            // ����

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
                {// ��Project����ж�λ��Դ
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(k));
                }
                if (GUILayout.Button("", "ToolbarSearchTextField", GUILayout.Width(17), GUILayout.Height(15)))
                {// ��¼��ǰ��Դ��������ab��
                    currentResDetail = GDependLoader.m_cache[k];
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                // ��ʾ��Դ������
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
                            if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(40), GUILayout.Height(18)))
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
        /// ���� AB�� ҳǩ
        /// </summary>
        private void DrawTable_Bundle()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"��ռ��: <color=#ff6b8e>{EditorUtility.FormatBytes(allsize)}</color>", Gui.LabelStyle);
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
                    {// ��ʾab���ڰ�������Դ��Ϣ
                        currentABDetail = GAssetManifest.GetABInfo(k);
                    }
                    EditorGUILayout.EndVertical();
                    if (currentABDetail != null && k == currentABDetail.m_name)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField($"<color=#bdf6ff># ��Ԫ��</color>", Gui.LabelStyle);
                        foreach (var content in currentABDetail.m_contains)
                        {
                            EditorGUILayout.BeginHorizontal("box");
                            EditorGUILayout.LabelField($"{content} <color=#ffaa00>({EditorUtility.FormatBytes(Profilers.GetResRuntimeSize(content))})</color>", Gui.LabelStyle);
                            if (GUILayout.Button("��λ", Gui.BtnStyle, GUILayout.Width(40), GUILayout.Height(18)))
                            {
                                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(content));
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if (currentABDetail.m_depends != null)
                        {
                            EditorGUILayout.LabelField($"<color=#bdf6ff># ������</color>", Gui.LabelStyle);
                            foreach (var content in currentABDetail.m_depends)
                            {
                                EditorGUILayout.BeginHorizontal("box");
                                EditorGUILayout.LabelField($"{content} <color=#ffffff>({EditorUtility.FormatBytes(Profilers.GetABRuntimeSize(content))})</color>", Gui.LabelStyle);
                                if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(40), GUILayout.Height(18)))
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

            // ������ Editor ����ʱ״̬�£����Ӱ���δ����״̬���ܼ�ʱˢ��
            currentInterval += Time.deltaTime;
            if (currentInterval >= refreshInterval)
            {
                currentInterval = 0;
                // �ػ�GUI
                Repaint();
            }
        }
    }
}