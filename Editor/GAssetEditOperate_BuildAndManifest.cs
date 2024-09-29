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
    /// 构建资源及资源配置
    /// <para>作者：强辰</para>
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
            // 构建
            m_BuildFoldout = EditorGUILayout.Foldout(m_BuildFoldout, new GUIContent("构建"), true);
            if (m_BuildFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("* 按 <color=#ffcc00>资源标记名</color> 构建", Gui.LabelStyle, GUILayout.Width(140));
                if (GUILayout.Button("构建", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                { BuildByABundleName(); }
                EditorGUILayout.EndHorizontal();
                m_buildStr = EditorGUILayout.TextField("", m_buildStr, GUILayout.Height(100));
                EditorGUILayout.LabelField("<color=#999999>* 输入框内应输入资源的 AssetBundle 标记名称. 多个标记名称以 ',' 英文逗号分开.</color>", Gui.LabelStyle);

                EditorGUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("* 按 <color=#ffcc00>资源依赖项</color> 构建", Gui.LabelStyle, GUILayout.Width(140));
                if (GUILayout.Button("构建", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                { BuildByABundleDepends(); }
                EditorGUILayout.EndHorizontal();
                m_buildStr2 = EditorGUILayout.TextField("", m_buildStr2, GUILayout.Height(100));
                EditorGUILayout.LabelField(
                    "<color=#999999>" +
                    "* 根据输入的资源标记名，搜索其所有的依赖项及自身执行构建. 多个标记名称以 ',' 英文逗号分开." +
                    "</color>", Gui.LabelStyle);


                EditorGUILayout.Space(18);
                EditorGUILayout.LabelField("快捷操作", Gui.LabelStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(28));
                if (GUILayout.Button("构建所有资源（已标记的）", Gui.BtnStyle, GUILayout.Width(150), GUILayout.Height(20)))
                { BuildAll(); }
                if (GUILayout.Button("清理无用的 AssetBundle 资源文件", Gui.BtnStyle, GUILayout.Width(200), GUILayout.Height(20)))
                { ClearAssetBundle(); }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;
            }

            EditorGUILayout.Space(10);

            // 配置
            m_ManifestFoldout = EditorGUILayout.Foldout(m_ManifestFoldout, new GUIContent("Manifest"), true);
            if (m_ManifestFoldout)
            {
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("构建资源清单: ", Gui.LabelStyle, GUILayout.Width(110));
                if (GUILayout.Button("构建", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    GenManifest();
                }
                m_bBackupManifest = EditorGUILayout.Toggle("", m_bBackupManifest, GUILayout.Width(20));
                EditorGUILayout.LabelField("<color=#cccccc>保存清单文件</color>", Gui.LabelStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(
                    "<color=#999999>" +
                    "* 只要操作过资源包构建或清理，都需要重新生成 Manifest 文件." +
                    "</color>", Gui.LabelStyle);
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;
            }
        }



        /// <summary>
        /// 是否可进行资源构建操作
        /// </summary>
        /// <param name="outpath">输出路径</param>
        /// <returns></returns>
        private bool IsCanOperateBuild(string outpath)
        {
            if (Dialog.Confirm("确认开始构建？") != 0) { return false; }
            if (string.IsNullOrEmpty(outpath))
            {
                Dialog.Tip("构建路径未配置！");
                return false;
            }
            if (!IO.IsDir(outpath) || !Directory.Exists(outpath))
            {
                Dialog.Tip("构建路径不是一个目录或不存在！");
                return false;
            }
            if (settings.Platform == BuildTarget.NoTarget)
            {
                Dialog.Tip("构建平台未配置！");
                return false;
            }
            if (settings.ABOptions == BuildAssetBundleOptions.None)
            {
                Dialog.Tip("资源包压缩方式未配置！");
                return false;
            }
            return true;
        }


        /// <summary>
        /// 构建所有资源(已标记的）
        /// </summary>
        private void BuildAll()
        {
            string outpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (IsCanOperateBuild(outpath))
            {
                BuildPipeline.BuildAssetBundles(outpath, settings.ABOptions, settings.Platform);
                AssetDatabase.Refresh();
                Dialog.Tip("构建完毕！");
                GUIUtility.ExitGUI();
            }
        }


        /// <summary>
        /// 按资源标记名构建
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
                    Debug.LogError($"无法构建AssetBundle，遇到重名标记！ab name = {abname}");
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
                Dialog.Tip("未找到可用于构建AssetBundle的资源！");
                return;
            }

            BuildPipeline.BuildAssetBundles(outpath, abblst.ToArray(), settings.ABOptions, settings.Platform);
            AssetDatabase.Refresh();
            Dialog.Tip("构建完毕！");
            GUIUtility.ExitGUI();
        }


        /// <summary>
        /// 按资源标记名依赖项构建
        /// </summary>
        private void BuildByABundleDepends()
        {
            if (string.IsNullOrEmpty(m_buildStr2))
            {
                Dialog.Tip("未输入信息!");
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
                Dialog.Tip("未找到可用于构建AssetBundle的资源！");
                return;
            }

            //创建ab信息
            List<AssetBundleBuild> abblst = new List<AssetBundleBuild>();
            foreach (string name in abnames)
            {//abnames里边不会有重名项
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = name.Substring(0, name.IndexOf("."));
                abb.assetBundleVariant = "ab";
                abb.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                abblst.Add(abb);
            }

            //构建
            BuildPipeline.BuildAssetBundles(outpath, abblst.ToArray(), settings.ABOptions, settings.Platform);
            AssetDatabase.Refresh();
            Dialog.Tip("构建完毕！");
            GUIUtility.ExitGUI();
        }


        /// <summary>
        /// 清理无用的AssetBundle文件
        /// </summary>
        private void ClearAssetBundle()
        {
            if (Dialog.Confirm("确认开始清理 AssetBundle 文件?") != 0) { return; }

            string buildoutpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (string.IsNullOrEmpty(buildoutpath)) { Dialog.Tip("请在配置中设置构建资源包的输出路径！"); return; }
            if (!IO.IsDir(buildoutpath) || !Directory.Exists(buildoutpath)) { Dialog.Tip("输出路径不是一个目录或不存在."); return; }

            // 清理当前工程内未使用的 ab 标记
            AssetDatabase.RemoveUnusedAssetBundleNames();
            string[] abnames = AssetDatabase.GetAllAssetBundleNames();
            if (abnames == null || abnames.Length == 0)
            {// 不存在如何标记，将当前构建目录下的所有资源全部删除，包括自定义的 Manifest 文件
                IO.DirClear(buildoutpath);
                AssetDatabase.Refresh();
                Dialog.Tip("清理操作完毕.");
                return;
            }


            // 获取输出路径下的所有文件
            List<string> abs = new List<string>();
            IO.GetFiles(buildoutpath, abs, new List<string> { ".manifest" });
            if (abs.Count == 0) { Dialog.Tip("未找到任何 AssetBundle 文件!"); return; }

            buildoutpath = cngraphi.gassets.common.Paths.Replace(buildoutpath);
            int indx = buildoutpath.LastIndexOf("/");
            string rootname = buildoutpath.Substring(indx + 1);

            List<string> dels = new List<string>();
            foreach (string f in abs)
            {
                string fname = Path.GetFileNameWithoutExtension(f);
                if (fname == "manifest" || fname == rootname) { continue; }
                if (Array.IndexOf(abnames, Path.GetFileName(f)) == -1)
                {//输出目录下的ab文件名在整个工程的AssetBundle资源标记组中未找到，需要将过期不使用的ab文件删除
                    string p = cngraphi.gassets.common.Paths.Replace(f);
                    p = p.Substring(p.IndexOf("Assets"));
                    dels.Add(p);
                    dels.Add(p + ".manifest");
                }
            }

            if (dels.Count != 0)
            {
                Debug.LogWarning($"删除的文件如下：{C_LineSeq}{string.Join(C_LineSeq, dels)}");

                List<string> outFailedPaths = new List<string>();
                AssetDatabase.DeleteAssets(dels.ToArray(), outFailedPaths);
                AssetDatabase.Refresh();

                if (outFailedPaths.Count != 0)
                    Debug.LogWarning($"未能正常删除的资源如下：{C_LineSeq}{string.Join(C_LineSeq, outFailedPaths)}");
            }

            Dialog.Tip("清理操作完毕!");
        }


        /// <summary>
        /// 生成自定义的Manifest文件
        /// </summary>
        private void GenManifest()
        {
            if (Dialog.Confirm("确认开始构建 Manifest 文件？") != 0) { return; }

            string version = settings.Version;
            if (string.IsNullOrEmpty(version)) { Dialog.Tip("未输入版本号！"); return; }

            string buildoutpath = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);

            if (string.IsNullOrEmpty(buildoutpath)) { Dialog.Tip("请在配置中设置构建资源包的输出路径！"); return; }
            if (!IO.IsDir(buildoutpath) || !Directory.Exists(buildoutpath)) { Dialog.Tip("构建路径不是一个目录或不存在."); return; }

            // 拉取构建目录的 ab 包资源
            List<string> abs = new List<string>();
            IO.GetFiles(buildoutpath, abs, new List<string> { ".manifest" });
            if (abs.Count == 0)
            {
                Dialog.Tip("未收集到任何 AssetBundle 文件，无法生成 GAssets Manifest 文件!");
                return;
            }

            //主ab名称
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
                manifest_base.Append(version + C_LineSeq); //版本号
                manifest_info = new StringBuilder();
                foreach (string f in abs)
                {
                    string exten = Path.GetFileNameWithoutExtension(f);
                    if (exten == rootname || exten == "manifest") { continue; }

                    string filename = Path.GetFileName(f); //文件名
                    int size = System.IO.File.ReadAllBytes(f).Length; //尺寸
                    string hash = Str.MD5_File(f); //hash值

                    if (C_DllExten == Path.GetExtension(f))
                    {//dll文件处理
                        manifest_info.Append(filename + "|" + size + "|" + hash + C_LineSeq); //追加一条
                        allsize += size;
                    }
                    else
                    {
                        ab = AssetBundle.LoadFromFile(f);

                        string containfiles;
                        if (ab.isStreamedSceneAssetBundle)
                        {// 场景ab包
                            containfiles = string.Join(",", ab.GetAllScenePaths());
                        }
                        else
                        {// 非场景ab包
                            containfiles = string.Join(",", ab.GetAllAssetNames());
                        }

                        string[] depends = AssetDatabase.GetAssetBundleDependencies(ab.name, true); //依赖项（递归所有）
                        string combinestr = filename + "|" + size + "|" + hash + "|" + containfiles;
                        if (depends.Length != 0)
                            combinestr += "|" + string.Join(",", depends);
                        manifest_info.Append(combinestr + C_LineSeq); //追加一条
                        allsize += size;

                        ab.Unload(true);
                        ab = null;
                    }
                }
                manifest_base.Append(allsize.ToString()); //资源总尺寸

                //写入并保存Manifest文件
                string info = manifest_base.ToString() + C_LineSeq + manifest_info.ToString();
                info = info.Trim();
                string savefile = Path.Combine(buildoutpath, "manifest");
                System.IO.File.WriteAllBytes(savefile, Encoding.UTF8.GetBytes(info));

                //备份当前生成的Manifest文件
                if (m_bBackupManifest)
                {
                    string manifestBackupPath = Path.Combine(buildoutpath.Substring(0, indx), rootname + "_" + "manifest");
                    manifestBackupPath = cngraphi.gassets.common.Paths.Replace(manifestBackupPath);
                    if (!System.IO.Directory.Exists(manifestBackupPath)) { System.IO.Directory.CreateDirectory(manifestBackupPath); }

                    //long timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); // 时间戳
                    //IO.CopyFile(savefile, Path.Combine(manifestBackupPath, "manifest" + "_" + version + "_" + timestamp));
                    IO.CopyFile(savefile, Path.Combine(manifestBackupPath, "manifest" + "_" + version));
                }

                AssetDatabase.Refresh();
                Dialog.Tip("构建 GAssets Manifest 完成！");
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