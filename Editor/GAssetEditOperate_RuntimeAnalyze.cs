using System.Collections.Generic;
using System.IO;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ����ʱ��Դ����Դ�ڴ���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        enum TabType
        {
            Asset,
            Bundle
        }
        Vector2 scrollV22 = Vector2.zero;
        AssetDetail currentDetail = null; // ��ǰѡ��鿴����Դ��ϸ��Ϣ����
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
            EditorGUILayout.LabelField("<color=#ffcc00>* �������ݾ�������ʱ�ڴ��н��л�ȡ</color>", Gui.LabelStyle);

            float w = EditorGUIUtility.currentViewWidth;
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

            // Tab
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("helpbox");
            Color cc = GUI.backgroundColor;
            if (tabType == TabType.Asset)
                GUI.backgroundColor = Color.green;
            if(GUILayout.Button("��Դ", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                tabType = TabType.Asset;
            }
            GUI.backgroundColor = cc;
            cc = GUI.backgroundColor;
            if (tabType == TabType.Bundle)
                GUI.backgroundColor = Color.green;
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


            if(tabType == TabType.Asset)
            {// ��ҳǩΪ Asset ʱ
                EditorGUILayout.BeginHorizontal("helpbox");
                EditorGUILayout.LabelField("��Դ", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField("�ڴ����ü���", Gui.LabelStyle, layoutw);
                EditorGUILayout.LabelField("������AssetBundle��", Gui.LabelStyle, layoutw);
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
                        {// ��ʾ��Դ��������ab��
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
            {// ��ҳǩΪ AB�� ʱ
                foreach(var k in GBundleLoader.m_cache.Keys)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"{k} / <color=#ffcc00>({ EditorUtility.FormatBytes(GetABRuntimeSize(k)) })</color>", Gui.LabelStyle);
                    EditorGUILayout.EndVertical();
                }
            }
        }


        /// <summary>
        /// �õ� AB�� �ڵ�ǰƽ̨������ʱ���ڴ�ռ�ô�С
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



        #region ��ȡ����
        // ��Դ������ϸ��Ϣ�ṹ��
        class AssetDetail
        {
            public string Key;
            public int Ref;
            public List<GBundleLoader> Depends;
        }
        // �洢���ж�̬��������Դ������Ϣ��
        Dictionary<string, Dictionary<string, AssetDetail>> assetData = new Dictionary<string, Dictionary<string, AssetDetail>>();


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
            if (!EditorApplication.isPlaying) { return; }

            // ����
            assetData.Clear();

            // ��ȡ����
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