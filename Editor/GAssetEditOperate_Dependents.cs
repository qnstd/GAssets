using System.Collections.Generic;
using System.IO;
using System.Linq;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 资源依赖关系
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        enum DependRelationTabType
        {
            BeDepend = 0,
            Depend
        }
        struct DependRelationTab
        {
            public string label;
            public DependRelationTabType type;
        }
        List<DependRelationTab> dependRelationTabs = new List<DependRelationTab>()
        {
            new DependRelationTab(){ label="被依赖", type = DependRelationTabType.BeDepend},
            new DependRelationTab(){ label="依赖", type = DependRelationTabType.Depend},
        };
        DependRelationTabType curDependRelationTabType = DependRelationTabType.BeDepend;


        #region 被依赖页签的变量
        /// <summary>
        /// 所有资产文件被依赖信息缓存组（ Key：资产文件；value：资产文件被其他资产依赖得文件列表 ）
        /// </summary>
        static private Dictionary<string, List<string>> m_cache = new Dictionary<string, List<string>>();
        /// <summary>
        /// 根据用户要查询的资产，获取其被依赖信息（ Key：资产文件；value：资产文件被其他资产依赖得文件列表 ）
        /// </summary>
        static private Dictionary<string, List<string>> m_have = new Dictionary<string, List<string>>();
        /// <summary>
        /// 资产没有被任何其他资产所依赖
        /// </summary>
        static private List<string> m_nohave = new List<string>();
        Vector2 m_v2;
        Vector2 m_v22;
        List<string> m_lst;
        bool isBackup = false;
        #endregion


        #region 依赖页签的变量
        static List<string> res = new List<string>();
        static string filename = string.Empty;
        Vector2 v2 = Vector2.zero;
        #endregion



        private void OnEnable_Dependents() { }
        private void OnDisable_Dependents()
        {
            curDependRelationTabType = 0;

            m_cache.Clear();
            m_have.Clear();
            m_nohave.Clear();
            m_lst?.Clear();
            m_lst = null;

            res.Clear();
            filename = string.Empty;
        }
        private void OnGUI_Dependents()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal("helpbox");
            foreach (var t in dependRelationTabs)
            {
                Color c = GUI.backgroundColor;
                GUI.backgroundColor = (t.type == curDependRelationTabType) ? Color.cyan : Color.white;
                if (GUILayout.Button(t.label, Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    if (t.type != curDependRelationTabType)
                        curDependRelationTabType = t.type;

                }
                GUI.backgroundColor = c;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
            // 根据类型显示对应的GUI
            switch (curDependRelationTabType)
            {
                case DependRelationTabType.BeDepend:
                    Draw_BeDependGUI();
                    break;
                case DependRelationTabType.Depend:
                    Draw_DependGUI();
                    break;
            }
            // 结束
            EditorGUILayout.EndVertical();
        }



        private void Draw_BeDependGUI()
        {
            EditorGUILayout.HelpBox
            (
                "1. 资产被依赖检索，不支持资产名称或路径在脚本、配置等文本文件中以文本形式的引用检测，但支持资产对象被脚本直接引用检测（将资产挂载到脚本的公共属性，也就是Inspector的区域）；\n" +
                "2. 支持多文件同时搜索检测；\n" +
                "3. 操作方式：在 Project 资源面板中选择要检测的文件，然后鼠标右键选择【BeDepend Search】菜单项即可；",
                MessageType.None
            );
            //未被依赖
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("<color=#fd7878ff>未被其他资产依赖的文件</color>", Gui.LabelStyle);
            GUILayout.FlexibleSpace();
            Color cc = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("一键忽略", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(16))) { IngoresAll(); }
            GUI.backgroundColor = cc;
            cc = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("一键删除", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(16))) { DeleteAllUnused(); }
            GUI.backgroundColor = cc;
            isBackup = EditorGUILayout.Toggle("", isBackup, GUILayout.Width(20));
            EditorGUILayout.LabelField("删除备份", Gui.LabelStyle, GUILayout.Width(24));
            EditorGUILayout.EndHorizontal();
            m_v2 = EditorGUILayout.BeginScrollView(m_v2, "box");
            foreach (string s in m_nohave)
            {
                GUILayout.BeginHorizontal("box", GUILayout.Height(14));
                EditorGUILayout.LabelField($"{s}", Gui.LabelStyle);
                if (GUILayout.Button("定位", Gui.BtnStyle, GUILayout.Width(45), GUILayout.Height(16))) { GPS(s); }
                if (GUILayout.Button("拷贝名称", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(16))) { CopyToClipboard(s); }
                Color c = GUI.backgroundColor;
                GUI.backgroundColor = new Color(153 / 255.0f, 192 / 255.0f, 221 / 255.0f);
                if (GUILayout.Button("忽略", Gui.BtnStyle, GUILayout.Width(45), GUILayout.Height(16))) { Ingores(s); }
                if (GUILayout.Button("删除", Gui.BtnStyle, GUILayout.Width(45), GUILayout.Height(16))) { DeleteUnused(s); }
                GUI.backgroundColor = c;
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            //被依赖
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("被其他资产依赖的文件", Gui.LabelStyle);
            EditorGUILayout.EndHorizontal();
            m_v22 = EditorGUILayout.BeginScrollView(m_v22, "box");
            foreach (string s in m_have.Keys)
            {
                GUILayout.BeginHorizontal("box", GUILayout.Height(25));
                EditorGUILayout.LabelField($"<color=#fbf78dff>{s}</color>", Gui.LabelStyle, GUILayout.Height(35));
                GUILayout.EndHorizontal();
                m_have.TryGetValue(s, out m_lst);
                foreach (string f in m_lst)
                {//当前资产被依赖的资产展示
                    GUILayout.BeginHorizontal("box", GUILayout.Height(20));
                    EditorGUILayout.LabelField(f, Gui.LabelStyle);
                    if (GUILayout.Button("定位", Gui.BtnStyle, GUILayout.Width(45), GUILayout.Height(18))) { GPS(f); }
                    if (GUILayout.Button("拷贝名称", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18))) { CopyToClipboard(f); }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        private void Draw_DependGUI()
        {
            if (res.Count == 0) { return; }

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"<color=#ffcc00>文件数: {res.Count}</color>", Gui.LabelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("导出", Gui.BtnStyle, GUILayout.Width(70), GUILayout.Height(18)))
            {
                ExportPkg();
            }
            EditorGUILayout.EndHorizontal();

            v2 = EditorGUILayout.BeginScrollView(v2, "box");
            string curtype;
            string prevtype = null;
            foreach (string s in res)
            {
                // type
                curtype = Path.GetExtension(s);
                if (string.IsNullOrEmpty(prevtype))
                {
                    prevtype = curtype;
                    DrawTypeTitle(prevtype);
                }
                else
                {
                    if (prevtype != curtype)
                    {
                        prevtype = curtype;
                        DrawTypeTitle(prevtype);
                    }
                }
                // elements
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField($"{s}", Gui.LabelStyle);
                if (GUILayout.Button("定位", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
                {
                    GPS(s);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(5);
        }
        private void DrawTypeTitle(string typ)
        {
            Color c = GUI.backgroundColor;
            GUI.backgroundColor = Color.cyan;
            EditorGUILayout.LabelField($"<color=#ffffff>{typ}</color>", Gui.LabelStyle);
            GUI.backgroundColor = c;
        }




        private void DeleteAllUnused()
        {
            if (m_nohave == null || m_nohave.Count == 0) { return; }
            int rs = Dialog.Confirm("确定执行删除？\n\n提示：虽然资源相互之间没有被依赖，但可能存在文本文件中引用当前资源的名称或者路径。请时刻注意！\n");
            if (rs == 0)
            {
                for (int i = 0; i < m_nohave.Count; i++)
                {
                    string origin = m_nohave[i];
                    Backup(origin);
                    AssetDatabase.DeleteAsset(origin);  // 删除
                }
                m_nohave.Clear();
                AssetDatabase.Refresh();
            }
        }
        private void DeleteUnused(string f)
        {
            int rs = Dialog.Confirm("确定执行删除？\n\n提示：虽然资源相互之间没有被依赖，但可能存在文本文件中引用当前资源的名称或者路径。请时刻注意！\n");
            if (rs == 0)
            {
                Backup(f);
                m_nohave.Remove(f);
                AssetDatabase.DeleteAsset(f);
                AssetDatabase.Refresh();
                GUIUtility.ExitGUI();
            }
        }
        private void IngoresAll()
        {
            int rs = Dialog.Confirm("确定执行忽略？\n\n提示：\n1.虽然资源相互之间没有被依赖，但可能存在文本文件中引用当前资源的名称或者路径。请时刻注意！\n2.若确定文本文件中没有间接引用，点击一键忽略后可从清除列表，但工程内仍存在这些资产。 \n");
            if (rs == 0)
            {
                m_nohave.Clear();
            }
        }
        private void Ingores(string f)
        {
            int rs = Dialog.Confirm("确定执行忽略？\n\n提示：\n1.虽然资源相互之间没有被依赖，但可能存在文本文件中引用当前资源的名称或者路径。请时刻注意！\n2.若确定文本文件中没有间接引用，点击忽略后可从列表中移除，但工程内仍存在此资产。 \n");
            if (rs == 0)
            {
                m_nohave.Remove(f);
                GUIUtility.ExitGUI();
            }
        }
        private void Backup(string origin)
        {
            if (isBackup)
            {
                string file = Path.Combine(settings.CleanBackupPath, origin);
                IO.RecursionDirCreate(Path.GetDirectoryName(file));
                IO.CopyFile(origin, file);  // 拷贝
            }
        }
        private void CopyToClipboard(string s)
        {
            if (string.IsNullOrEmpty(s)) { return; }

            s = s.Replace("\\", "/");
            string filename = s.Substring(s.LastIndexOf("/") + 1);

            TextEditor t = new TextEditor();
            t.text = filename;
            t.OnFocus();
            t.Copy();

        }
        private void GPS(string s)
        {
            if (string.IsNullOrEmpty(s)) { return; }
            UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
            if (o == null) { return; }
            Selection.activeObject = o;
            EditorGUIUtility.PingObject(o);
        }
        private void ExportPkg()
        {
            if (res == null || res.Count == 0) { Debug.Log("数据为空，无法导出."); return; }

            string p = $"Assets/{(string.IsNullOrEmpty(filename) ? "customexport" : filename)}.unitypackage";
            AssetDatabase.ExportPackage(res.ToArray(), p, ExportPackageOptions.Default);
            AssetDatabase.Refresh();
            Dialog.Tip($"导出路径：{p}");
        }




        #region 静态
        [MenuItem("Assets/BeDepend Search", false, priority = 9999)]
        static private void FindBeDependInfo()
        {
            string[] guids = Selection.assetGUIDs;
            if (guids == null || guids.Length == 0)
            {
                Dialog.Tip("无法检测！\n\n原因：未选择任何资产对象!");
                return;
            }

            if (FindDependOnReverse())
            {
                Dialog.Tip("已取消检测.");
                return;
            }

            m_have.Clear();
            m_nohave.Clear();
            foreach (string guid in guids)
            {
                List<string> beDependent;
                string asset = AssetDatabase.GUIDToAssetPath(guid);
                if (!m_cache.TryGetValue(asset, out beDependent))
                {//资源未被任何其他资源依赖
                    m_nohave.Add(asset);
                    continue;
                }
                m_have.Add(asset, beDependent);
            }

            //显示查询结果
            ShowJump("Dependents", (win) => { win.curDependRelationTabType = DependRelationTabType.BeDepend; });
        }
        static private bool FindDependOnReverse()
        {
            //清理缓存
            m_cache.Clear();
            int count = 0;
            string pro;
            float proval;
            bool flag = false;

            //获取所有资源的 GUID 值
            string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/" });
            int guidslen = guids.Length;
            foreach (string guid in guids)
            {
                //获取资源的直接依赖项，并对其所有的直接依赖项进行反向被依赖检测
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string[] depends = AssetDatabase.GetDependencies(assetPath, false); //不递归搜索，获取直接依赖，不对间接依赖进行获取
                foreach (string file in depends)
                {
                    List<string> lst;
                    if (!m_cache.TryGetValue(file, out lst))
                    {
                        lst = new List<string>();
                        m_cache[file] = lst;
                    }
                    lst.Add(assetPath);
                }

                //显示进度
                count++;
                proval = (float)(count * 1.0f / guidslen);
                pro = (proval * 100).ToString("f1") + "%";
                //EditorUtility.DisplayProgressBar("资产被依赖的信息检索", "当前进度：" + pro, proval);
                flag = EditorUtility.DisplayCancelableProgressBar("资产被依赖的信息检索", "当前进度：" + pro, proval); //由于检测时间可能较长，所以这里使用带取消的进度窗体进行显示
                if (flag) { break; }
            }

            EditorUtility.ClearProgressBar();
            return flag;
        }

        [MenuItem("Assets/Depend Search", false, priority = 10000)]
        static private void FindDependInfo()
        {
            string[] guids = Selection.assetGUIDs;
            if (guids == null || guids.Length == 0)
            {
                Dialog.Tip("无法检测！\n\n原因：未选择任何资产对象!");
                return;
            }

            res.Clear();
            string pathName = AssetDatabase.GUIDToAssetPath(guids[0]);
            filename = Path.GetFileNameWithoutExtension(pathName);
            string[] strs = AssetDatabase.GetDependencies(pathName, true);

            foreach (string s in strs)
            {
                if (s.IndexOf("com.unity") != -1) { continue; } // 取消 com.unity 依赖项
                if (res.IndexOf(s) != -1) { continue; }
                res.Add(s);
                //Debug.Log(s);
            }
            //Debug.Log(res.Count);
            if (res.Count == 0) { return; }

            // 排序
            res = res.ToArray().OrderBy(f => Path.GetExtension(f)).ToList<string>();

            //显示查询结果
            ShowJump("Dependents", (win) => { win.curDependRelationTabType = DependRelationTabType.Depend; });
        }
        #endregion
    }
}