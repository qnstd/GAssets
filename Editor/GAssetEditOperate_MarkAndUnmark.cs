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
    /// 资源标记
    /// <para>作者：强辰</para>
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
            m_MarkFoldout = EditorGUILayout.Foldout(m_MarkFoldout, new GUIContent("资源标记"), true);
            if (m_MarkFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("资源目录", Gui.LabelStyle, GUILayout.Width(95));
                Gui.DragTextArea(this, 100, ref m_markpath, ref m_markpathAry);
                EditorGUILayout.BeginHorizontal();
                m_singleMark = EditorGUILayout.Toggle("", m_singleMark, GUILayout.Width(20));
                EditorGUILayout.LabelField("<color=#999999>* 单文件形式 (选择的目录内所有文件以各自文件名来进行标记，否则以选择的目录名称标记)</color>", Gui.LabelStyle);
                EditorGUILayout.Space(1);
                if (GUILayout.Button("标记", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
                {
                    RunFolderMark();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("自定义资源标记", Gui.LabelStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("标记名称：", Gui.LabelStyle, GUILayout.Width(80));
                m_markCustomname = EditorGUILayout.TextField("", m_markCustomname, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();

                Gui.DragTextArea(this, 100, ref m_markCustomStr, ref m_markCustom);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("<color=#999999>* 支持目录及文件拖入，目录以递归方式搜索所有子资源</color>", Gui.LabelStyle);
                if (GUILayout.Button("标记", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
                {
                    if (Dialog.Confirm("确认开始资源标记？") != 0) { return; }
                    if (string.IsNullOrEmpty(m_markCustomname))
                    {
                        Dialog.Tip("尚未输入自定义资源标记名称", "错误");
                        return;
                    }
                    if (m_markCustom == null || m_markCustom.Length == 0)
                    {
                        Dialog.Tip("尚未选择自定义的资源或资源目录.", "错误");
                        return;
                    }
                    CustomAssetBundleNameOperate(m_markCustom, true);
                    Dialog.Tip("资源标记完毕！");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel -= 2;
            }


            //Unmark
            EditorGUILayout.Space(10);
            m_UnmarkFoldout = EditorGUILayout.Foldout(m_UnmarkFoldout, new GUIContent("取消标记"), true);
            if (m_UnmarkFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("自定义资源取消标记", Gui.LabelStyle);
                Gui.DragTextArea(this, 100, ref m_clearCustomStr, ref m_clearCustom);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("<color=#999999>* 支持目录及文件拖入，目录以递归方式搜索所有子资源</color>", Gui.LabelStyle);
                EditorGUILayout.Space(1);
                if (GUILayout.Button("取消标记", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18)))
                {
                    if (Dialog.Confirm("确认开始取消资源标记？") != 0) { return; }
                    if (m_clearCustom == null || m_clearCustom.Length == 0)
                    {
                        Dialog.Tip("尚未选择自定义的资源或资源目录.", "错误");
                        return;
                    }
                    CustomAssetBundleNameOperate(m_clearCustom, false);
                    Dialog.Tip("资源标记取消完毕！");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(15);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("* 删除工程中未使用的资源标记名称：", Gui.LabelStyle, GUILayout.Width(200));
                Color c = GUI.backgroundColor;
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("删除", Gui.BtnStyle, GUILayout.Height(18), GUILayout.Width(50)))
                {
                    if (Dialog.Confirm("确定删除工程中未使用的资源标记名称吗？") != 0) { return; }
                    RemoveUnusedABNames();
                    Dialog.Tip("已完成删除.");
                }
                GUI.backgroundColor = c;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;
            }
        }


        /// <summary>
        /// 目录标记操作
        /// </summary>
        private void RunFolderMark()
        {
            if (Dialog.Confirm("确认开始资源标记？") != 0) { return; }
            if (m_markpathAry == null || m_markpathAry.Length == 0) { return; }
            foreach (string s in m_markpathAry)
            {
                //只有目录才执行以下操作
                if (!IO.IsDir(s)) { continue; }

                string foldername = "";
                if (!m_singleMark)
                {//以目录名称进行标记
                    string _p = s.Substring(s.IndexOf("Assets"));
                    _p = cngraphi.gassets.common.Paths.Replace(_p);
                    Str.Split(_p, "/", out List<string> lst);
                    string[] dirnames = lst.ToArray();
                    foldername = dirnames[dirnames.Length - 1];
                }
                AssetBundleNameOperate(s, foldername);
            }
            Dialog.Tip("资源标记完毕！");
        }
        private void AssetBundleNameOperate(string path, string foldername)
        {
            if (!IO.IsDir(path)) { return; } //非目录
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] fsi = dir.GetFileSystemInfos();
            foreach (FileSystemInfo f in fsi)
            {
                if (f is DirectoryInfo) { AssetBundleNameOperate(f.FullName, foldername); }
                else
                {
                    //meta文件忽略
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
        /// 自定义资源（标记、取消标记）
        /// </summary>
        /// <param name="paths">资源路径</param>
        /// <param name="b">true：标记；false：取消标记</param>
        private void CustomAssetBundleNameOperate(string[] paths, bool b)
        {
            SortedSet<string> reslist = new SortedSet<string>(); // 创建一个不允许元素重复且不允许对现有项更改排序的列表
            for (int i = 0; i < paths.Length; i++)
            {
                if (IO.IsDir(paths[i]))
                {//是目录
                    List<string> rs = new List<string>();
                    IO.GetFiles(paths[i], rs);
                    reslist.AddRange(rs);
                }
                else
                {//资源
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
        /// 删除未使用的资源标记名
        /// </summary>
        private void RemoveUnusedABNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
    }

}