using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ������Դ����Դ����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        const string C_DllExten = ".dll";

        bool m_BuildFoldout = true;
        bool m_ManifestFoldout = true;

        string m_buildStr = "";
        string m_buildStr2 = "";

        bool m_bBackupManifest = true;
        private Vector2 m_scroll;


        private void OnEnable_BuildAndManifest()
        {
        }
        private void OnDisable_BuildAndManifest()
        {
            m_buildStr = "";
            m_buildStr2 = "";
            m_scroll = Vector2.zero;

        }
        private void OnGUI_BuildAndManifest()
        {
            // ����
            m_BuildFoldout = EditorGUILayout.Foldout(m_BuildFoldout, new GUIContent("����"), true);
            if (m_BuildFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("* �� <color=#ffcc00>��Դ�����</color> ����", Gui.LabelStyle, GUILayout.Width(140));
                if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                { BuildByABundleName(); }
                EditorGUILayout.EndHorizontal();
                m_buildStr = EditorGUILayout.TextField("", m_buildStr, GUILayout.Height(100));
                EditorGUILayout.LabelField("<color=#999999>* �������Ӧ������Դ�� AssetBundle �������. ������������ ',' Ӣ�Ķ��ŷֿ�.</color>", Gui.LabelStyle);

                EditorGUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("* �� <color=#ffcc00>��Դ������</color> ����", Gui.LabelStyle, GUILayout.Width(140));
                if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                { BuildByABundleDepends(); }
                EditorGUILayout.EndHorizontal();
                m_buildStr2 = EditorGUILayout.TextField("", m_buildStr2, GUILayout.Height(100));
                EditorGUILayout.LabelField(
                    "<color=#999999>" +
                    "* �����������Դ����������������е����������ִ�й���. ������������ ',' Ӣ�Ķ��ŷֿ�." +
                    "</color>", Gui.LabelStyle);


                EditorGUILayout.Space(18);
                EditorGUILayout.LabelField("��ݲ���", Gui.LabelStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(28));
                if (GUILayout.Button("����������Դ���ѱ�ǵģ�", Gui.BtnStyle, GUILayout.Width(150), GUILayout.Height(20)))
                { BuildAll(); }
                if (GUILayout.Button("�������õ� AssetBundle ��Դ�ļ�", Gui.BtnStyle, GUILayout.Width(200), GUILayout.Height(20)))
                { ClearAssetBundle(); }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;
            }

            EditorGUILayout.Space(10);

            // ����
            m_ManifestFoldout = EditorGUILayout.Foldout(m_ManifestFoldout, new GUIContent("Manifest"), true);
            if (m_ManifestFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("������Դ�嵥: ", Gui.LabelStyle, GUILayout.Width(110));
                if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    GenManifest();
                }
                m_bBackupManifest = EditorGUILayout.Toggle("", m_bBackupManifest, GUILayout.Width(20));
                EditorGUILayout.LabelField("<color=#cccccc>�����嵥�ļ�</color>", Gui.LabelStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(
                    "<color=#999999>" +
                    "* ֻҪ��������Դ����������������Ҫ�������� Manifest �ļ�." +
                    "</color>", Gui.LabelStyle);
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;
            }
        }



        /// <summary>
        /// �Ƿ�ɽ�����Դ��������
        /// </summary>
        /// <param name="outpath">���·��</param>
        /// <returns></returns>
        private bool IsCanOperateBuild(string outpath)
        {
            if (Dialog.Confirm("ȷ�Ͽ�ʼ������") != 0) { return false; }
            if (string.IsNullOrEmpty(outpath))
            {
                Dialog.Tip("����·��δ���ã�");
                return false;
            }
            if (!IO.IsDir(outpath) || !Directory.Exists(outpath))
            {
                Dialog.Tip("����·������һ��Ŀ¼�򲻴��ڣ�");
                return false;
            }
            if (settings.Platform == BuildTarget.NoTarget)
            {
                Dialog.Tip("����ƽ̨δ���ã�");
                return false;
            }
            if (settings.ABOptions == BuildAssetBundleOptions.None)
            {
                Dialog.Tip("��Դ��ѹ����ʽδ���ã�");
                return false;
            }
            return true;
        }


        /// <summary>
        /// ����������Դ(�ѱ�ǵģ�
        /// </summary>
        private void BuildAll()
        {
            string outpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (IsCanOperateBuild(outpath))
            {
                BuildPipeline.BuildAssetBundles(outpath, settings.ABOptions, settings.Platform);
                AssetDatabase.Refresh();
                Dialog.Tip("������ϣ�");
                GUIUtility.ExitGUI();
            }
        }


        /// <summary>
        /// ����Դ���������
        /// </summary>
        private void BuildByABundleName()
        {
            string outpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (!IsCanOperateBuild(outpath)) { return; }

            Str.Split(m_buildStr.Trim(), ",", out List<string> lst);
            List<AssetBundleBuild> abblst = new List<AssetBundleBuild>();
            foreach (string abname in lst)
            {
                string[] abfiles = AssetDatabase.GetAssetPathsFromAssetBundle(abname + "." + "ab");
                if (abfiles.Length == 0) { continue; }
                if (abblst.FindIndex((AssetBundleBuild abb) => { return abb.assetBundleName == abname; }) != -1)
                {
                    Debug.LogError($"�޷�����AssetBundle������������ǣ�ab name = {abname}");
                    continue;
                }

                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = abname;
                abb.assetBundleVariant = "ab";
                abb.assetNames = abfiles;
                abblst.Add(abb);
            }
            if (abblst.Count == 0)
            {
                Dialog.Tip("δ�ҵ������ڹ���AssetBundle����Դ��");
                return;
            }

            BuildPipeline.BuildAssetBundles(outpath, abblst.ToArray(), settings.ABOptions, settings.Platform);
            AssetDatabase.Refresh();
            Dialog.Tip("������ϣ�");
            GUIUtility.ExitGUI();
        }


        /// <summary>
        /// ����Դ������������
        /// </summary>
        private void BuildByABundleDepends()
        {
            if (string.IsNullOrEmpty(m_buildStr2))
            {
                Dialog.Tip("δ������Ϣ!");
                return;
            }

            string outpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (!IsCanOperateBuild(outpath)) { return; }

            Str.Split(m_buildStr2.Trim(), ",", out List<string> lst);
            List<string> abnames = new List<string>();
            foreach (string abname in lst)
            {
                string aname = abname + "." + "ab";
                string[] depends = AssetDatabase.GetAssetBundleDependencies(aname, true);
                if (depends.Length != 0)
                {
                    List<string> l = new List<string>(depends);
                    abnames.AddRange(l.FindAll(s => { return !abnames.Contains(s); }));
                }
                if (!abnames.Contains(aname))
                    abnames.Add(aname);
            }
            if (abnames.Count == 0)
            {
                Dialog.Tip("δ�ҵ������ڹ���AssetBundle����Դ��");
                return;
            }

            //����ab��Ϣ
            List<AssetBundleBuild> abblst = new List<AssetBundleBuild>();
            foreach (string name in abnames)
            {//abnames��߲�����������
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = name.Substring(0, name.IndexOf("."));
                abb.assetBundleVariant = "ab";
                abb.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                abblst.Add(abb);
            }

            //����
            BuildPipeline.BuildAssetBundles(outpath, abblst.ToArray(), settings.ABOptions, settings.Platform);
            AssetDatabase.Refresh();
            Dialog.Tip("������ϣ�");
            GUIUtility.ExitGUI();
        }


        /// <summary>
        /// �������õ�AssetBundle�ļ�
        /// </summary>
        private void ClearAssetBundle()
        {
            if (Dialog.Confirm("ȷ�Ͽ�ʼ���� AssetBundle �ļ�?") != 0) { return; }

            string buildoutpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (string.IsNullOrEmpty(buildoutpath)) { Dialog.Tip("�������������ù�����Դ�������·����"); return; }
            if (!IO.IsDir(buildoutpath) || !Directory.Exists(buildoutpath)) { Dialog.Tip("���·������һ��Ŀ¼�򲻴���."); return; }

            // ����ǰ������δʹ�õ� ab ���
            AssetDatabase.RemoveUnusedAssetBundleNames();
            string[] abnames = AssetDatabase.GetAllAssetBundleNames();
            if (abnames == null || abnames.Length == 0)
            {// ��������α�ǣ�����ǰ����Ŀ¼�µ�������Դȫ��ɾ���������Զ���� Manifest �ļ�
                IO.DirClear(buildoutpath);
                AssetDatabase.Refresh();
                Dialog.Tip("����������.");
                return;
            }


            // ��ȡ���·���µ������ļ�
            List<string> abs = new List<string>();
            IO.GetFiles(buildoutpath, abs, new List<string> { ".manifest" });
            if (abs.Count == 0) { Dialog.Tip("δ�ҵ��κ� AssetBundle �ļ�!"); return; }

            buildoutpath = cngraphi.gassets.common.Paths.Replace(buildoutpath);
            int indx = buildoutpath.LastIndexOf("/");
            string rootname = buildoutpath.Substring(indx + 1);

            List<string> dels = new List<string>();
            foreach (string f in abs)
            {
                string fname = Path.GetFileNameWithoutExtension(f);
                if (fname == "manifest" || fname == rootname) { continue; }
                if (Array.IndexOf(abnames, Path.GetFileName(f)) == -1)
                {//���Ŀ¼�µ�ab�ļ������������̵�AssetBundle��Դ�������δ�ҵ�����Ҫ�����ڲ�ʹ�õ�ab�ļ�ɾ��
                    string p = cngraphi.gassets.common.Paths.Replace(f);
                    p = p.Substring(p.IndexOf("Assets"));
                    dels.Add(p);
                    dels.Add(p + ".manifest");
                }
            }

            if (dels.Count != 0)
            {
                Debug.LogWarning($"ɾ�����ļ����£�{C_LineSeq}{string.Join(C_LineSeq, dels)}");

                List<string> outFailedPaths = new List<string>();
                AssetDatabase.DeleteAssets(dels.ToArray(), outFailedPaths);
                AssetDatabase.Refresh();

                if (outFailedPaths.Count != 0)
                    Debug.LogWarning($"δ������ɾ������Դ���£�{C_LineSeq}{string.Join(C_LineSeq, outFailedPaths)}");
            }

            Dialog.Tip("����������!");
        }


        /// <summary>
        /// �����Զ����Manifest�ļ�
        /// </summary>
        private void GenManifest()
        {
            if (Dialog.Confirm("ȷ�Ͽ�ʼ���� Manifest �ļ���") != 0) { return; }

            string version = settings.Version;
            if (string.IsNullOrEmpty(version)) { Dialog.Tip("δ����汾�ţ�"); return; }

            string buildoutpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);

            if (string.IsNullOrEmpty(buildoutpath)) { Dialog.Tip("�������������ù�����Դ�������·����"); return; }
            if (!IO.IsDir(buildoutpath) || !Directory.Exists(buildoutpath)) { Dialog.Tip("����·������һ��Ŀ¼�򲻴���."); return; }

            // ��ȡ����Ŀ¼�� ab ����Դ
            List<string> abs = new List<string>();
            IO.GetFiles(buildoutpath, abs, new List<string> { ".manifest" });
            if (abs.Count == 0)
            {
                Dialog.Tip("δ�ռ����κ� AssetBundle �ļ����޷����� GAssets Manifest �ļ�!");
                return;
            }

            //��ab����
            buildoutpath = cngraphi.gassets.common.Paths.Replace(buildoutpath);
            int indx = buildoutpath.LastIndexOf("/");
            string rootname = buildoutpath.Substring(indx + 1);

            StringBuilder manifest_base = null;
            StringBuilder manifest_info = null;
            AssetBundle ab = null;
            try
            {
                int allsize = 0;
                manifest_base = new StringBuilder();
                manifest_base.Append(version + C_LineSeq); //�汾��
                manifest_info = new StringBuilder();
                foreach (string f in abs)
                {
                    string exten = Path.GetFileNameWithoutExtension(f);
                    if (exten == rootname || exten == "manifest") { continue; }

                    string filename = Path.GetFileName(f); //�ļ���
                    int size = System.IO.File.ReadAllBytes(f).Length; //�ߴ�
                    string hash = Str.MD5_File(f); //hashֵ

                    if (C_DllExten == Path.GetExtension(f))
                    {//dll�ļ�����
                        manifest_info.Append(filename + "|" + size + "|" + hash + C_LineSeq); //׷��һ��
                        allsize += size;
                    }
                    else
                    {
                        ab = AssetBundle.LoadFromFile(f);

                        string containfiles;
                        if (ab.isStreamedSceneAssetBundle)
                        {// ����ab��
                            containfiles = string.Join(",", ab.GetAllScenePaths());
                        }
                        else
                        {// �ǳ���ab��
                            containfiles = string.Join(",", ab.GetAllAssetNames());
                        }

                        string[] depends = AssetDatabase.GetAssetBundleDependencies(ab.name, true); //������ݹ����У�
                        string combinestr = filename + "|" + size + "|" + hash + "|" + containfiles;
                        if (depends.Length != 0)
                            combinestr += "|" + string.Join(",", depends);
                        manifest_info.Append(combinestr + C_LineSeq); //׷��һ��
                        allsize += size;

                        ab.Unload(true);
                        ab = null;
                    }
                }
                manifest_base.Append(allsize.ToString()); //��Դ�ܳߴ�

                //д�벢����Manifest�ļ�
                string info = manifest_base.ToString() + C_LineSeq + manifest_info.ToString();
                info = info.Trim();
                string savefile = Path.Combine(buildoutpath, "manifest");
                System.IO.File.WriteAllBytes(savefile, Encoding.UTF8.GetBytes(info));

                //���ݵ�ǰ���ɵ�Manifest�ļ�
                if (m_bBackupManifest)
                {
                    string manifestBackupPath = Path.Combine(buildoutpath.Substring(0, indx), rootname + "_" + "manifest");
                    manifestBackupPath = cngraphi.gassets.common.Paths.Replace(manifestBackupPath);
                    if (!System.IO.Directory.Exists(manifestBackupPath)) { System.IO.Directory.CreateDirectory(manifestBackupPath); }

                    //long timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); // ʱ���
                    //IO.CopyFile(savefile, Path.Combine(manifestBackupPath, "manifest" + "_" + version + "_" + timestamp));
                    IO.CopyFile(savefile, Path.Combine(manifestBackupPath, "manifest" + "_" + version));
                }

                AssetDatabase.Refresh();
                Dialog.Tip("���� GAssets Manifest ��ɣ�");
            }
            catch (Exception e) { Debug.LogError(e.Message); }
            finally
            {
                abs.Clear();
                if (manifest_base != null) { manifest_base.Clear(); }
                if (manifest_info != null) { manifest_info.Clear(); }
                if (ab != null) { ab.Unload(true); }
            }
        }

    }
}