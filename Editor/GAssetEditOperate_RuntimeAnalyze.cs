using System.Collections.Generic;
using System.IO;
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
        private void OnEnable_RuntimeAnalyze() { }
        private void OnDisable_RuntimeAnalyze()
        {
            assetData.Clear();
            scrollV22 = Vector2.zero;
        }



        Vector2 scrollV22 = Vector2.zero;
        private void OnGUI_RuntimeAnalyze()
        {
            EditorGUILayout.LabelField("<color=#ffcc00>* ����ʱ״̬����</color>", Gui.LabelStyle);

            float w = EditorGUIUtility.currentViewWidth;
            float perw = w / 3.0f - 71;
            GUILayoutOption layoutw = GUILayout.Width(perw);

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("helpbox");
            EditorGUILayout.LabelField("��Դ", Gui.LabelStyle, layoutw);
            EditorGUILayout.LabelField("���ü���", Gui.LabelStyle, layoutw);
            EditorGUILayout.LabelField("������AssetBundle��", Gui.LabelStyle, layoutw);
            EditorGUILayout.EndHorizontal();

            scrollV22 = EditorGUILayout.BeginScrollView(scrollV22);
            foreach (var k in assetData.Keys)
            {
                foreach (var m in assetData[k].Keys)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(m, Gui.LabelStyle, layoutw);
                    EditorGUILayout.LabelField(assetData[k][m].Ref.ToString(), Gui.LabelStyle, layoutw);
                    EditorGUILayout.BeginVertical(layoutw);
                    if (GUILayout.Button("�鿴", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(15)))
                    {
                        //foreach (var bundle in assetData[k][m].Depends)
                        //{
                        //    Debug.Log(bundle.AssetBundle?.name);
                        //}
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space(2);
            EditorGUILayout.EndScrollView();
        }




        #region ��ȡ����
        struct AssetAnalyze
        {
            public int Ref;
            public List<GBundleLoader> Depends;
        }
        Dictionary<string, Dictionary<string, AssetAnalyze>> assetData = new Dictionary<string, Dictionary<string, AssetAnalyze>>();


        Dictionary<string, AssetAnalyze> GetGroupByType(string k)
        {
            string typ = Path.GetExtension(k)[1..];
            Dictionary<string, AssetAnalyze> dic;
            if (!assetData.ContainsKey(typ))
            {
                dic = new Dictionary<string, AssetAnalyze>();
                assetData.Add(typ, dic);
            }
            else
            {
                assetData.TryGetValue(typ, out dic);
            }
            return dic;
        }


        private void OnUpdate_RuntimeAnalyze()
        {
            if (!EditorApplication.isPlaying) { return; }

            // ����
            assetData.Clear();

            // ��ȡ����
            foreach (var k in GAssetLoader.m_cache.Keys)
            {
                Dictionary<string, AssetAnalyze> dic = GetGroupByType(k);
                dic.Add(k, new AssetAnalyze()
                {
                    Ref = GAssetLoader.m_cache[k].Ref,
                    Depends = GDependLoader.m_cache[k].Bundles
                });
            }
            foreach (var k in GDependLoader.m_cache.Keys)
            {
                Dictionary<string, AssetAnalyze> dic = GetGroupByType(k);
                if (!dic.TryGetValue(k, out _))
                {
                    dic.Add(k, new AssetAnalyze()
                    {
                        Ref = GDependLoader.m_cache[k].Ref,
                        Depends = GDependLoader.m_cache[k].Bundles
                    });
                }
            }
        }
        #endregion
    }


}