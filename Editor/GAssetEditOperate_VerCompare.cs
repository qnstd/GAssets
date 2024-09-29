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
    /// 资源差异化比对
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        // 版本差异化文件存储的目录名称后缀 及 差异化文件配置的文件名前缀
        const string C_HotfixDirPrefix = "hotfix";
        // 版本差异化（各个历史版本）文件存储目录名称后缀
        const string C_HotfixDirPrefix_History = "hotfix_histroy";


        string m_diffverPath = "";
        bool m_isDelDiff = false;
        string m_diffpath = "";


        private void OnEnable_VerCompare() { }
        private void OnDisable_VerCompare() { }
        private void OnGUI_VerCompare()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("差异化比对清单", Gui.LabelStyle, GUILayout.Width(90));
            if (GUILayout.Button("选择", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                string p = Dialog.File("选择差异化比对清单");
                m_diffverPath = string.IsNullOrEmpty(p) ? m_diffverPath : p;
            }
            if (GUILayout.Button("执行", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                GenDiffVersion();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            m_diffverPath = EditorGUILayout.TextField("", m_diffverPath, GUILayout.Height(30));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            m_isDelDiff = EditorGUILayout.Toggle(m_isDelDiff, GUILayout.Width(20), GUILayout.Height(20));
            EditorGUILayout.LabelField("<color=#d0a600>是否将差异化文件从当前资源构建目录内删除</color>", Gui.LabelStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "<color=#999999>" +
                "* 差异化比对清单是指过往版本对应的 Manifest 清单文件，操作时与当前的清单文件进行比对；\n" +
                "* 此功能用于当前清单与上一版本号对应的清单进行比对；\n" +
                "* 差异化比对操作为增量方式，只对新增或修改文件进行记录，而已删除的文件不作为资源差异化比对范畴；\n" +
                "* 每次执行差异化比对时，会清除上一次的比对结果，操作时请留意；" +
                "</color>"
                , Gui.LabelStyle, GUILayout.Height(45));

            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);


            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("差异化清单目录", Gui.LabelStyle, GUILayout.Width(90));
            if (GUILayout.Button("选择", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                string p = Dialog.Directory("选择差异化 Manifest 清单目录");
                m_diffpath = string.IsNullOrEmpty(p) ? m_diffpath : p;
            }
            if (GUILayout.Button("执行", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                GenAllDiffVersionFiles();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            m_diffpath = EditorGUILayout.TextField("", m_diffpath, GUILayout.Height(30));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "<color=#999999>" +
                "* 根据当前版本的 Manifest，为之前每一个版本的 Manifest 生成差异化清单；\n" +
                "* 只生成差异化清单，并不包含差异化文件；" +
                "</color>", Gui.LabelStyle, GUILayout.Height(34));
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }



        /// <summary>
        /// 差异化比对
        /// </summary>
        private void GenDiffVersion()
        {
            if (Dialog.Confirm("确认开始执行差异化比对?") != 0) { return; }
            if (string.IsNullOrEmpty(m_diffverPath)) { Dialog.Tip("请选择差异化比对文件"); return; }

            string outp = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (string.IsNullOrEmpty(outp)) { Dialog.Tip("请在配置中设置构建资源的路径！"); return; }

            //生成存储根目录
            outp = cngraphi.gassets.common.Paths.Replace(outp);
            int indx = outp.LastIndexOf("/");
            string diffdir = Path.Combine(
                                            outp.Substring(0, indx),
                                            outp.Substring(indx + 1) + "_" + C_HotfixDirPrefix
                                          );
            diffdir = cngraphi.gassets.common.Paths.Replace(diffdir);
            if (!System.IO.Directory.Exists(diffdir)) { System.IO.Directory.CreateDirectory(diffdir); }
            else { IO.DirClear(diffdir); }


            //读取当前输出目录下的 Manifest 清单
            string curpath = Path.Combine(outp, "manifest");
            if (!System.IO.File.Exists(curpath))
            {
                Dialog.Confirm("当前资源包输出目录下未找到 Manifest 文件");
                return;
            }
            string[] news = System.IO.File.ReadAllLines(curpath);
            string newversion = news[0];

            //读取已选择的 Manifest 清单
            Dictionary<string, string> origins = new Dictionary<string, string>();
            string[] orign = System.IO.File.ReadAllLines(m_diffverPath);
            int len = orign.Length;
            for (int i = 2; i < len; i++)
            {
                Str.Split(orign[i], "|", out List<string> rs);
                origins.Add(rs[0], rs[2]);
            }

            //进行比对
            int newslen = news.Length;
            int allsize = 0;
            StringBuilder cnf_base = new StringBuilder();
            //cnf_base.Append(newversion + C_LineSeq); //版本号（与当前最新 manifest 配置中的版本号一致）
            StringBuilder cnf_info = new StringBuilder();
            for (int i = 2; i < newslen; i++)
            {
                Str.Split(news[i], "|", out List<string> rs);
                string filename = rs[0];
                origins.TryGetValue(filename, out string originval);
                if (string.IsNullOrEmpty(originval) || originval != rs[2])
                {//新增 or 资源有修改。增量包操作，当前 Manifest 配置已删除的文件不作为资源更新的标准。
                    //拷贝文件
                    string srcfile = Path.Combine(outp, filename);
                    IO.CopyFile(srcfile, Path.Combine(diffdir, filename));
                    allsize += int.Parse(rs[1]);
                    cnf_info.Append(filename + C_LineSeq);
                    //删除文件
                    if (m_isDelDiff)
                    {
                        System.IO.File.Delete(srcfile);
                        System.IO.File.Delete(srcfile + ".manifest");
                    }
                }
            }
            cnf_base.Append(allsize.ToString()); //差异化文件大小（单位：字节）

            //生成比对清单
            string cnf = cnf_base.ToString().Trim() + C_LineSeq + cnf_info.ToString().Trim();
            cnf = cnf.Trim();
            string savefileName = C_HotfixDirPrefix + "_" + newversion + "_" + orign[0];
            System.IO.File.WriteAllBytes(Path.Combine(diffdir, savefileName), Encoding.UTF8.GetBytes(cnf));

            //将最新的资源清单也拷贝到差异化目录
            IO.CopyFile(curpath, Path.Combine(diffdir, "manifest"));

            AssetDatabase.Refresh();
            Dialog.Tip("差异化比对完毕！");
        }


        /// <summary>
        /// 生成所有历史版本的差异化配置
        /// </summary>
        private void GenAllDiffVersionFiles()
        {
            if (string.IsNullOrEmpty(m_diffpath))
            {
                Dialog.Tip("未选择差异化清单目录");
                return;
            }

            string outp = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (string.IsNullOrEmpty(outp)) { Dialog.Tip("请在配置中设置构建资源的路径！"); return; }
            if (Dialog.Confirm("确认开始执行所有历史版本的差异化清单比对?") != 0) { return; }

            List<string> files = new List<string>();
            IO.GetFiles(m_diffpath, files);
            int len = files.Count;
            if (len == 0)
            {
                Dialog.Tip("选择的目录内未找到任何 Manifest 清单文件");
                return;
            }

            //读取当前输出目录下的最新 Manifest 文件
            string curpath = Path.Combine(outp, "manifest");
            if (!System.IO.File.Exists(curpath))
            {
                Dialog.Confirm("当前输出目录下未找到 Manifest 文件");
                return;
            }
            string[] news = System.IO.File.ReadAllLines(curpath);
            string newversion = news[0];
            int newslen = news.Length;

            //生成存储目录
            outp = cngraphi.gassets.common.Paths.Replace(outp);
            int indx = outp.LastIndexOf("/");
            string savedir = Path.Combine(
                                            outp.Substring(0, indx),
                                            outp.Substring(indx + 1) + "_" + C_HotfixDirPrefix_History
                                          );
            savedir = cngraphi.gassets.common.Paths.Replace(savedir);
            if (!System.IO.Directory.Exists(savedir)) { System.IO.Directory.CreateDirectory(savedir); }
            else { IO.DirClear(savedir); }

            //开始为每一个历史版本的 manifest 进行差异化比对
            for (int i = 0; i < len; i++)
            {
                //读取历史版本
                string fname = files[i];
                Str.Split(fname, "_", out List<string> result);
                if (result[2] == newversion) { continue; } //历史版本的版本号与当前最新 Manifest中 的版本号一致，则不处理

                Dictionary<string, string> origins = new Dictionary<string, string>(); // 资源ab名称，资源的md5
                string[] orign = System.IO.File.ReadAllLines(fname);
                int _len = orign.Length;
                for (int j = 2; j < _len; j++)
                {
                    Str.Split(orign[j], "|", out List<string> rs);
                    origins.Add(rs[0], rs[2]);
                }

                //进行比对
                int allsize = 0;
                StringBuilder cnf_base = new StringBuilder();
                //cnf_base.Append(newversion + C_LineSeq); //版本号（与当前最新 manifest 配置中的版本号一致）
                StringBuilder cnf_info = new StringBuilder();
                for (int k = 2; k < newslen; k++)
                {
                    Str.Split(news[k], "|", out List<string> rs);
                    string filename = rs[0];
                    origins.TryGetValue(filename, out string originval);
                    if (string.IsNullOrEmpty(originval) || originval != rs[2])
                    {//新增 or 资源有修改。增量包操作，当前Manifest配置已删除的文件不作为资源更新的标准。
                        allsize += int.Parse(rs[1]);
                        cnf_info.Append(filename + C_LineSeq);
                    }
                }
                cnf_base.Append(allsize.ToString()); //差异化文件大小（单位：字节）

                //生成比对配置
                string cnf = cnf_base.ToString().Trim() + C_LineSeq + cnf_info.ToString().Trim();
                cnf = cnf.Trim();
                string savefileName = C_HotfixDirPrefix + "_" + newversion + "_" + orign[0];
                System.IO.File.WriteAllBytes(Path.Combine(savedir, savefileName), Encoding.UTF8.GetBytes(cnf));
            }

            AssetDatabase.Refresh();
            Dialog.Tip("差异化比对完成！");
        }
    }

}