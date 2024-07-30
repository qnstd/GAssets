using System.Collections.Generic;
using System.IO;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ��Դ���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        bool m_MarkFoldout = true;
        bool m_singleMark = false;
        string m_markpath = "";
        string[] m_markpathAry = null;

        string m_markCustomStr = "";
        string[] m_markCustom;
        string m_markCustomname = "";

        bool m_UnmarkFoldout = true;
        string m_clearCustomStr = "";
        string[] m_clearCustom;



        private void OnEnable_MarkAndUnmark() { }
        private void OnDisable_MarkAndUnmark()
        {
            m_markpath = "";
            m_markpathAry = null;

            m_markCustomStr = "";
            m_markCustom = null;
            m_markCustomname = "";

            m_clearCustomStr = "";
            m_clearCustom = null;
        }
        private void OnGUI_MarkAndUnmark()
        {
            //mark
            m_MarkFoldout = EditorGUILayout.Foldout(m_MarkFoldout, new GUIContent("��Դ���"), true);
            if (m_MarkFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("��ԴĿ¼", Gui.LabelStyle, GUILayout.Width(95));
                Gui.DragTextArea(this, 100, ref m_markpath, ref m_markpathAry);
                EditorGUILayout.BeginHorizontal();
                m_singleMark = EditorGUILayout.Toggle("", m_singleMark, GUILayout.Width(20));
                EditorGUILayout.LabelField("<color=#999999>* ���ļ���ʽ (ѡ���Ŀ¼�������ļ��Ը����ļ��������б�ǣ�������ѡ���Ŀ¼���Ʊ��)</color>", Gui.LabelStyle);
                EditorGUILayout.Space(1);
                if (GUILayout.Button("���", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
                {
                    RunFolderMark();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("�Զ�����Դ���", Gui.LabelStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("������ƣ�", Gui.LabelStyle, GUILayout.Width(80));
                m_markCustomname = EditorGUILayout.TextField("", m_markCustomname, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                Gui.DragTextArea(this, 100, ref m_markCustomStr, ref m_markCustom);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("<color=#999999>* ֧��Ŀ¼���ļ����룬Ŀ¼�Եݹ鷽ʽ������������Դ</color>", Gui.LabelStyle);
                if (GUILayout.Button("���", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
                {
                    if (Dialog.Confirm("ȷ�Ͽ�ʼ��Դ��ǣ�") != 0) { return; }
                    if (string.IsNullOrEmpty(m_markCustomname))
                    {
                        Dialog.Tip("��δ�����Զ�����Դ�������", "����");
                        return;
                    }
                    if (m_markCustom == null || m_markCustom.Length == 0)
                    {
                        Dialog.Tip("��δѡ���Զ������Դ����ԴĿ¼.", "����");
                        return;
                    }
                    CustomAssetBundleNameOperate(m_markCustom, true);
                    Dialog.Tip("��Դ�����ϣ�");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel -= 2;
            }


            //Unmark
            EditorGUILayout.Space(10);
            m_UnmarkFoldout = EditorGUILayout.Foldout(m_UnmarkFoldout, new GUIContent("ȡ�����"), true);
            if (m_UnmarkFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("�Զ�����Դȡ�����", Gui.LabelStyle);
                Gui.DragTextArea(this, 100, ref m_clearCustomStr, ref m_clearCustom);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("<color=#999999>* ֧��Ŀ¼���ļ����룬Ŀ¼�Եݹ鷽ʽ������������Դ</color>", Gui.LabelStyle);
                EditorGUILayout.Space(1);
                if (GUILayout.Button("ȡ�����", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
                {
                    if (Dialog.Confirm("ȷ�Ͽ�ʼȡ����Դ��ǣ�") != 0) { return; }
                    if (m_clearCustom == null || m_clearCustom.Length == 0)
                    {
                        Dialog.Tip("��δѡ���Զ������Դ����ԴĿ¼.", "����");
                        return;
                    }
                    CustomAssetBundleNameOperate(m_clearCustom, false);
                    Dialog.Tip("��Դ���ȡ����ϣ�");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(15);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("* ɾ��������δʹ�õ���Դ������ƣ�", Gui.LabelStyle, GUILayout.Width(200));
                Color c = GUI.backgroundColor;
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("ɾ��", Gui.BtnStyle, GUILayout.Height(18), GUILayout.Width(50)))
                {
                    if (Dialog.Confirm("ȷ��ɾ��������δʹ�õ���Դ���������") != 0) { return; }
                    RemoveUnusedABNames();
                    Dialog.Tip("�����ɾ��.");
                }
                GUI.backgroundColor = c;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;
            }
        }


        /// <summary>
        /// Ŀ¼��ǲ���
        /// </summary>
        private void RunFolderMark()
        {
            if (Dialog.Confirm("ȷ�Ͽ�ʼ��Դ��ǣ�") != 0) { return; }
            if (m_markpathAry == null || m_markpathAry.Length == 0) { return; }
            foreach (string s in m_markpathAry)
            {
                //ֻ��Ŀ¼��ִ�����²���
                if (!IO.IsDir(s)) { continue; }

                string foldername = "";
                if (!m_singleMark)
                {//��Ŀ¼���ƽ��б��
                    string _p = s.Substring(s.IndexOf("Assets"));
                    _p = cngraphi.gassets.common.Paths.Replace(_p);
                    Str.Split(_p, "/", out List<string> lst);
                    string[] dirnames = lst.ToArray();
                    foldername = dirnames[dirnames.Length - 1];
                }
                AssetBundleNameOperate(s, foldername);
            }
            Dialog.Tip("��Դ�����ϣ�");
        }
        private void AssetBundleNameOperate(string path, string foldername)
        {
            if (!IO.IsDir(path)) { return; } //��Ŀ¼
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] fsi = dir.GetFileSystemInfos();
            foreach (FileSystemInfo f in fsi)
            {
                if (f is DirectoryInfo) { AssetBundleNameOperate(f.FullName, foldername); }
                else
                {
                    //meta�ļ�����
                    string p = f.FullName;
                    if (Path.GetExtension(p) == ".meta") { continue; }

                    AssetImporter ai = AssetImporter.GetAtPath(p.Substring(p.IndexOf("Assets")));
                    string assetbundleName;
                    if (m_singleMark)
                        assetbundleName = Path.GetFileNameWithoutExtension(p);
                    else
                        assetbundleName = foldername;

                    ai.assetBundleName = assetbundleName;
                    ai.assetBundleVariant = "ab";
                    ai.SaveAndReimport();
                }
            }
        }



        /// <summary>
        /// �Զ�����Դ����ǡ�ȡ����ǣ�
        /// </summary>
        /// <param name="paths">��Դ·��</param>
        /// <param name="b">true����ǣ�false��ȡ�����</param>
        private void CustomAssetBundleNameOperate(string[] paths, bool b)
        {
            SortedSet<string> reslist = new SortedSet<string>(); // ����һ��������Ԫ���ظ��Ҳ���������������������б�
            for (int i = 0; i < paths.Length; i++)
            {
                if (IO.IsDir(paths[i]))
                {//��Ŀ¼
                    List<string> rs = new List<string>();
                    IO.GetFiles(paths[i], rs);
                    reslist.AddRange(rs);
                }
                else
                {//��Դ
                    reslist.Add(paths[i]);
                }
            }
            foreach (string p in reslist)
            {
                AssetImporter ai = AssetImporter.GetAtPath(p.Substring(p.IndexOf("Assets")));
                if (b)
                {
                    ai.assetBundleName = m_markCustomname;
                    ai.assetBundleVariant = "ab";
                }
                else
                {
                    ai.assetBundleName = string.Empty;
                }
                ai.SaveAndReimport();
            }
            if (!b)
            {
                RemoveUnusedABNames();
            }
        }



        /// <summary>
        /// ɾ��δʹ�õ���Դ�����
        /// </summary>
        private void RemoveUnusedABNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
    }

}