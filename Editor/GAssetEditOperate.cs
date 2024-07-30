using System.Collections.Generic;
using System.IO;
using System.Reflection;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 资源库编辑器
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        [MenuItem("Window/GAssets #&X")]
        static private void Run()
        {
            GAssetEditOperate win = GetWindow<GAssetEditOperate>();
            win.titleContent = new GUIContent("GAssets");
            win.Show();
        }


        const string C_LineSeq = "\n";
        Vector2 tabV2 = Vector2.zero;
        Vector2 ContentV2 = Vector2.zero;
        string tabIndex = "";
        Color tabBackground;
        GAssetSettings settings;

        // 功能页签
        readonly Dictionary<string, string> tabs = new Dictionary<string, string>()
        {
            { "Atlas", "SrpiteAtlas 图集" },
            { "MarkAndUnmark", "资源标记" },
            { "BuildAndManifest", "资源、Manifest构建" },
            { "AssetBundleBrowser", "AssetBundle 浏览" },
            { "VerCompare", "版本差异化" },
            { "BeDependent", "资源被依赖查询" },
            { "UnuseClear", "未使用资源清理" },
        };


        private void OnEnable()
        {
            LoadConfigure();
        }


        private void OnDestroy()
        {
            tabIndex = "";
            tabV2 = Vector2.zero;
            ContentV2 = Vector2.zero;
            settings = null;

            foreach (string k in tabs.Keys)
            {
                InvokeMethod("OnDisable", k);
            }
        }


        private void OnGUI()
        {
            // 菜单项
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal("helpbox");
            EditorGUILayout.LabelField("菜单项", Gui.LabelStyle, GUILayout.Width(45));
            if (GUILayout.Button("工程适配", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(18))) { AdapterProject(); }
            if (GUILayout.Button("创建/获取配置", Gui.BtnStyle, GUILayout.Width(90), GUILayout.Height(18))) { CreateConfigure(); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);


            EditorGUILayout.BeginHorizontal();

            // 页签
            EditorGUILayout.BeginVertical("box", GUILayout.Width(160));
            tabV2 = EditorGUILayout.BeginScrollView(tabV2);
            foreach (string k in tabs.Keys)
            {
                tabBackground = GUI.backgroundColor;
                if (tabIndex == k) { GUI.backgroundColor = Color.green; }
                if (GUILayout.Button(tabs[k], Gui.BtnStyle, GUILayout.Height(25)))
                {
                    GUI.FocusControl(null); // 切换tab页签按钮按下时，取消当前窗体的聚焦
                    if (tabIndex != k)
                    {
                        if (!string.IsNullOrEmpty(tabIndex))
                        {
                            InvokeMethod("OnDisable", tabIndex);
                        }
                        tabIndex = k;
                        InvokeMethod("OnEnable", tabIndex);
                    }
                }
                if (tabIndex == k) { GUI.backgroundColor = tabBackground; }
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // 具体内容
            EditorGUILayout.BeginVertical("box");
            ContentV2 = EditorGUILayout.BeginScrollView(ContentV2);
            if (!string.IsNullOrEmpty(tabIndex))
            {
                InvokeMethod("OnGUI", tabIndex);
            }
            else
            {
                ShowDefaultPage();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }


        private void InvokeMethod(string methodname, string methodIndex)
        {
            MethodInfo method = GetType().GetDeclaredMethod(methodname + "_" + methodIndex);
            method.Invoke(this, null);
        }



        #region 菜单项

        // 创建资源管理器配置文件的路径。此路径必须是 Resources 目录，且必须在根目录下保存。运行时状态下会自动从此路径下加载。
        const string CONFIGURE_PATH = "Assets/Resources";

        // 工程适配
        private void AdapterProject()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            Dialog.Tip("适配完毕.");
        }

        // 加载配置
        private void LoadConfigure()
        {
            settings = Resources.Load<GAssetSettings>(GAssetManager.CONFIGURE_FILENAME);
        }

        // 创建配置
        private void CreateConfigure()
        {
            string file = Path.Combine(CONFIGURE_PATH, $"{GAssetManager.CONFIGURE_FILENAME}.asset"); // 文件
            file = cngraphi.gassets.common.Paths.Replace(file);

            if (!File.Exists(file))
            {// 不存在，则创建
                IO.RecursionDirCreate(CONFIGURE_PATH);
                AssetDatabase.CreateAsset
                (
                    ScriptableObject.CreateInstance<GAssetSettings>(),
                    file
                );
                AssetDatabase.Refresh();
                Dialog.Tip($"配置创建完毕!\n\n路径: {file}");
            }
            else
            {// 存在
                //Debug.LogWarning("GAssets 配置已存在.");
            }

            LoadConfigure();
            EditorGUIUtility.PingObject(settings);
        }

        #endregion

    }
}