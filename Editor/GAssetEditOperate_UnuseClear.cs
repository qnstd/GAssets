using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ��Դ������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        string m_clearfolder = "";

        List<string> m_clscheckReses = new List<string>();
        List<string> m_allReses = new List<string>();
        string[] m_ignoresLst;
        List<string> m_unuses = new List<string>();

        Vector2 __v2 = Vector2.zero;
        string m_searchkey = "";


        private void OnEnable_UnuseClear()
        {
        }
        private void OnDisable_UnuseClear()
        {
            m_clscheckReses.Clear();
            m_allReses.Clear();
            m_unuses.Clear();
            if (m_ignoresLst != null)
            {
                Array.Clear(m_ignoresLst, 0, m_ignoresLst.Length);
                m_ignoresLst = null;
            }
        }
        private void OnGUI_UnuseClear()
        {
            EditorGUILayout.BeginHorizontal("helpbox");
            EditorGUILayout.LabelField("Ҫ���������ԴĿ¼", Gui.LabelStyle, GUILayout.Width(110));
            EditorGUI.BeginDisabledGroup(true);
            m_clearfolder = EditorGUILayout.TextField("", m_clearfolder);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("ѡ��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                string p = Dialog.Directory("��ѡ��Ҫ������Դ��Ŀ¼");
                m_clearfolder = string.IsNullOrEmpty(p) ? m_clearfolder : p;
            }
            Color c = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                RunClear();
            }
            GUI.backgroundColor = c;
            EditorGUILayout.EndHorizontal();

            // ��ʾδʹ����Դ
            if (m_unuses != null && m_unuses.Count != 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" <color=#ffcc00>δʹ�õ���Դ�б�</color>" + "��" + m_unuses.Count + "����", Gui.LabelStyle);
                EditorGUILayout.Space(1);
                m_searchkey = EditorGUILayout.TextField("", m_searchkey, "SearchTextField", GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
                __v2 = EditorGUILayout.BeginScrollView(__v2, "box");
                foreach (string uu in m_unuses)
                {
                    if (uu.IndexOf(m_searchkey) != -1)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        string tarpath = cngraphi.gassets.common.Paths.Replace(Path.Combine(settings.CleanBackupPath, uu));
                        EditorGUILayout.LabelField
                        (
                            "<color=#cccccc>" + uu + "</color>\n" +
                            "<color=#777777>[Backup]:  " + tarpath + "</color>",
                            Gui.LabelStyle, GUILayout.Height(34)
                        );
                        if (GUILayout.Button("��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                        {
                            OpenUnuseFilePath(tarpath);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }


        private void OpenUnuseFilePath(string p)
        {
            if (!File.Exists(p))
            {
                Dialog.Tip("Ŀ�겻����.");
            }
            else
            {
                EditorUtility.RevealInFinder(p);
            }
        }


        private void RunClear()
        {
            IO.DirDelete(settings.CleanBackupPath);

            int result = Dialog.Confirm("�Ƿ�ȷ��ִ����Դ������?");
            if (result == 0)
            {
                if (string.IsNullOrEmpty(m_clearfolder))
                {
                    Dialog.Tip("δѡ������Ŀ¼��");
                    return;
                }

                string ignores = settings.CleanIgnore;
                if (!string.IsNullOrEmpty(ignores))
                {
                    ignores = ignores.Replace("\n", "").Replace(" ", "");
                    Str.Split(ignores, ",", out List<string> lst);
                    m_ignoresLst = lst.ToArray();
                }

                m_unuses.Clear();
                UnuseClear();
                DirNullClear(m_clearfolder);
                Dialog.Tip("�������!");
            }
        }
        private void UnuseClear()
        {
            string checktypes = settings.CleanFileExtension;
            string backupFolder = settings.CleanBackupPath;

            //����
            m_clscheckReses.Clear();
            m_allReses.Clear();

            //��ȡҪ��������ԴĿ¼����Դ �Լ� AssetsĿ¼��������Դ
            IO.GetFiles(m_clearfolder, m_clscheckReses, new List<string> { ".manifest" }, false);
            IO.GetFiles(Application.dataPath, m_allReses, new List<string> { ".manifest" }, false);

            //��������Ŀ¼
            if (!System.IO.Directory.Exists(backupFolder))
                System.IO.Directory.CreateDirectory(backupFolder);

            //���
            checktypes = checktypes.Replace("\n", "").Replace(" ", "");
            string[] types = checktypes.Split(new char[] { ',' });
            List<string> unusefiles = new List<string>();
            for (int i = 0; i < m_clscheckReses.Count; i++)
            {
                string checkfile = m_clscheckReses[i];
                if (checkfile.EndsWith(".unity")) { continue; }

                string fextendsion = Path.GetExtension(checkfile).Substring(1);
                if (Array.IndexOf(m_ignoresLst, fextendsion) != -1) { continue; }

                //��ȡ����ļ����ļ���
                checkfile = checkfile.Substring(checkfile.IndexOf("Assets"));
                checkfile = cngraphi.gassets.common.Paths.Replace(checkfile);
                string clsfileName = checkfile.Substring(checkfile.LastIndexOf("/") + 1);
                int clsfileName_indx = clsfileName.IndexOf(".");
                if (clsfileName_indx != -1)
                {//�ļ�������"."�����ȡ"."֮ǰ������
                    clsfileName = clsfileName.Substring(0, clsfileName_indx);
                }

                Regex regex = new Regex(clsfileName); //����ԭ�ļ�����
                Regex regex2 = new Regex(clsfileName.ToLower());//��ԭ�ļ�����תȫ��Сд
                bool b = false;
                for (int j = 0; j < m_allReses.Count; j++)
                {
                    string file = m_allReses[j];
                    string assetfile = file.Substring(file.IndexOf("Assets"));
                    assetfile = cngraphi.gassets.common.Paths.Replace(assetfile);

                    string file_extension = Path.GetExtension(file);
                    if (file_extension == "" || Array.IndexOf(types, file_extension.Substring(1)) != -1)
                    {//�ű������û�֧�ֵ��ļ�����
                        string baseval = Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(file));
                        MatchCollection mc = regex.Matches(baseval);
                        MatchCollection mc2 = regex2.Matches(baseval);
                        if ((mc != null && mc.Count != 0) || (mc2 != null && mc2.Count != 0))
                        {//������ǰ����ļ�
                            b = true;
                            break;
                        }

                    }
                    else
                    { //���������ļ� 
                        string[] depends = AssetDatabase.GetDependencies(assetfile, true);
                        List<string> dependslst = new List<string>(depends);
                        int indx = Array.IndexOf(depends, assetfile);
                        if (indx != -1)
                            dependslst.RemoveAt(indx);
                        depends = dependslst.ToArray();
                        if (Array.IndexOf(depends, checkfile) != -1)
                        {//�����ļ��������а�������ļ�
                            b = true;
                            break;
                        }
                    }
                }

                if (!b)
                {//û���ļ�������ǰ�����ļ�
                    if (!unusefiles.Contains(checkfile))
                        unusefiles.Add(checkfile);
                }
            }

            //����
            if (unusefiles.Count != 0)
            {
                m_unuses.AddRange(unusefiles);
                int l = unusefiles.Count;
                string file, filepath;
                for (int a = 0; a < l; a++)
                {
                    file = unusefiles[a];
                    // ����·��
                    filepath = backupFolder + "/" + Path.GetDirectoryName(file);
                    IO.RecursionDirCreate(filepath);
                    //������ɾ��
                    System.IO.File.Copy(file, filepath + "/" + Path.GetFileName(file));
                    AssetDatabase.DeleteAsset(file);
                }
                AssetDatabase.Refresh();
                //�ݹ��������
                UnuseClear();
            }
            else { /* ��δʹ����Դ��Ҫ���� */ }
        }
        private void DirNullClear(string path)
        {
            if (string.IsNullOrEmpty(path)) { return; }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            if (files.Length != 0)
            {
                foreach (FileSystemInfo fsi in files)
                    if (fsi is DirectoryInfo) { DirNullClear(fsi.FullName); }
            }
            if (dir.GetFileSystemInfos().Length == 0)
                AssetDatabase.DeleteAsset(path.Substring(path.IndexOf("Assets")));
            AssetDatabase.Refresh();
        }

    }
}