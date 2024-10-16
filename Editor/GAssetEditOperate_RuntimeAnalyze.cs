using System.Collections.Generic;
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
        // ��Դ������ϸ��Ϣ�ṹ��
        class AssetDetail
        {
            public string Key;
            public int Ref;
            public List<GBundleLoader> Depends;
        }

        Vector2 scrollV22 = Vector2.zero;
        TabType tabType = TabType.Asset;
        AssetDetail currentDetail = null; // ��ǰѡ��鿴����Դ��ϸ��Ϣ����
        ABInfo currentABDetail = null; // ��ǰѡ��鿴��AB����������Դ��Ϣ
        string restypSearch = ""; // ��Դ�����������
        string resSearch = ""; // ��Դ�������
        string abSearch = ""; // ��ab��ҳǩ�������Ľ��
        Dictionary<string, Dictionary<string, AssetDetail>> assetData = new Dictionary<string, Dictionary<string, AssetDetail>>(); // �洢���ж�̬��������Դ������Ϣ��
        float refreshInterval = 1; // ����ˢ�¼������λ����
        float currentInterval = 0; // ��ǰ��¼��ʱ��
        long allsize = 0; // �ڴ���ռ��
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
            EditorGUILayout.LabelField("<color=#ffcc00>* �������ݾ�������ʱ�ڴ��н��л�ȡ</color>", Gui.LabelStyle);

            float w = EditorGUIUtility.currentViewWidth;
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

            #region ȫ������
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("����ˢ�¼��: ", Gui.LabelStyle, GUILayout.Width(80));
            refreshInterval = EditorGUILayout.FloatField(refreshInterval, GUILayout.Width(50));
            EditorGUILayout.LabelField("s", Gui.LabelStyle, GUILayout.Width(20));
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

            #region ÿ��ҳǩ��GUIˢ��
            if (tabType == TabType.Asset)
            {// ��ҳǩΪ Asset ʱ
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
                                {// ��Project����ж�λ��Դ
                                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(m));
                                }
                                if (GUILayout.Button("", "ToolbarSearchTextField", GUILayout.Width(17), GUILayout.Height(15)))
                                {// ��ʾ��Դ��������ab��
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
                    }
                }
                EditorGUILayout.Space(2);
                EditorGUILayout.EndScrollView();
            }
            else if (tabType == TabType.Bundle)
            {// ��ҳǩΪ AB�� ʱ
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
                        EditorGUILayout.LabelField($"{k} <color=#ffcc00>({EditorUtility.FormatBytes(size)})</color>", Gui.LabelStyle);
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
                                    EditorGUILayout.LabelField($"{content} <color=#ffcc00>({EditorUtility.FormatBytes(Profilers.GetABRuntimeSize(content))})</color>", Gui.LabelStyle);
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
            #endregion
        }



        #region ��ȡ����
        /// <summary>
        /// ͨ����Դ�ļ����ͻ�ȡ��Ӧ�Ĺ�����
        /// </summary>
        /// <param name="k">��Դ�ļ�·������assetΪ��ͷ��·����</param>
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
        /// ����һ���µ���Դ��ϸ��Ϣ�ṹ����
        /// </summary>
        /// <param name="k">��Դ�ļ�·������assetΪ��ͷ��·����</param>
        /// <param name="refval">��Դ��ǰ����ֵ</param>
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
        /// �� Editor ������ʱ״̬�£�ʵʱ��ȡ��̬��������Դ��Ϣ����
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

                // ����
                assetData.Clear();

                // ��ȡ����
                foreach (var k in GAssetLoader.m_cache.Keys)
                {
                    Dictionary<string, AssetDetail> dic = GetGroupByType(k);
                    dic.Add(k, CreateAssetAnalyzeObject(k, GAssetLoader.m_cache[k].Ref));
                }
                foreach (var k in GDependLoader.m_cache.Keys)
                {// ѭ����� GDependLoader ����Ϊ��������ֱ��ʹ�õ��Ǵ˼�����
                    Dictionary<string, AssetDetail> dic = GetGroupByType(k);
                    if (!dic.TryGetValue(k, out _))
                    {
                        dic.Add(k, CreateAssetAnalyzeObject(k, GDependLoader.m_cache[k].Ref));
                    }
                }

                // �ػ�GUI
                Repaint();
            }
        }
        #endregion
    }


}