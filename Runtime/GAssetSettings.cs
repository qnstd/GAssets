using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.IO;
using System;

using cngraphi.gassets.common;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cngraphi.gassets
{
    /// <summary>
    /// 配置
    /// <para>作者：强辰</para>
    /// </summary>
    public class GAssetSettings : ScriptableObject
    {
        [InfoPropAttri("资源版本号", DomainType.General)]
        public string Version = "1.0.0";


        [InfoPropAttri("资源包目录", DomainType.General, "* 若未设置，则为 StreamingAssets 目录；否则为 StreamingAssets + 设置的目录.", 26)]
        public string AssetRootPath = "Bundles";


        [InfoPropAttri("持久化数据目录", DomainType.General, "* 若未设置，则为 PersistentData 目录；否则为 PersistentData + 设置的目录.", 26)]
        public string AssetDataPath = "Data/Bundles";


        [InfoPropAttri("资源下载的存储目录", DomainType.General, "* 若未设置，则为 PersistentData 目录；否则为 PersistentData + 设置的目录.", 26)]
        public string AssetDownloadPath = "Data/Download";


#if UNITY_EDITOR
        [InfoPropAttri("平台", DomainType.Editor)]
        public BuildTarget Platform = BuildTarget.StandaloneWindows;


        [InfoPropAttri("资源包压缩方式", DomainType.Editor)]
        public BuildAssetBundleOptions ABOptions = BuildAssetBundleOptions.ChunkBasedCompression;


        [InfoPropAttri("SpriteAtlas 图集输出目录", DomainType.Editor)]
        public string AtlasOutputPath = "Assets/Res/Atlas";


        [InfoPropAttri
        (
            "清理未使用资源的备份目录",
            DomainType.Editor,
            "* 建议将备份目录放置在 Assets 之外. 若在 Assets 之内会进行资源编译，从而影响处理速度."
        )]
        public string CleanBackupPath = Paths.Replace(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "BackupUnused")); // Assets/BackupUnused


        [InfoPropAttri
        (
            "清理未使用资源时的忽略项",
            DomainType.Editor,
            "* 默认忽略 .unity 文件类型"
        )]
        public string CleanIgnore = "prefab,spriteatlas,shader,shadergraph,shadersubgraph,hlsl,compute,cs,txt,json,xml";


        [InfoPropAttri
        (
            "清理未使用资源时的文件扩展",
            DomainType.Editor,
            "* 当资源以资源名在脚本、配置等文本文件中被引用时，是无法通过常规依赖关系进行引用检索的。因此，扩展编辑框内可输入需要进行对资源引用检索的文本文件类型。\n" +
            "* 若文本文件中检索到某一资源名，但检索位置处于注释状态，则依然判定存在引用关系。",
            26
        )]
        public string CleanFileExtension = "cs,txt,json,xml";
#endif
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(GAssetSettings))]
    internal class GAssetSettingsEditor : Editor
    {
        FieldInfo[] fields;

        Dictionary<string, InfoPropAttri> g = new Dictionary<string, InfoPropAttri>();
        Dictionary<string, InfoPropAttri> e = new Dictionary<string, InfoPropAttri>();

        bool bothFoldout = true;
        bool editorFoldout = true;

        Color titleBackgroundColor = new Color(155 / 255.0f, 201 / 255.0f, 132 / 255.0f, 1.0f);
        GUIStyle titleStyle = null;
        GUIStyle labelStyle = null;

        private void OnEnable()
        {
            fields = typeof(GAssetSettings).GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields != null && fields.Length > 0)
            {
                g.Clear();
                e.Clear();
                foreach (FieldInfo field in fields)
                {
                    InfoPropAttri attri = field.GetCustomAttribute<InfoPropAttri>();
                    Dictionary<string, InfoPropAttri> dic = null;
                    switch (attri.Type)
                    {
                        case DomainType.General:
                            dic = g;
                            break;
                        case DomainType.Editor:
                            dic = e;
                            break;
                    }
                    if (dic != null)
                        dic.Add(field.Name, attri);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (titleStyle == null)
                titleStyle = new GUIStyle("OverrideMargin") { richText = true, fontSize = 17, contentOffset = new Vector2(10, 8) };
            if (labelStyle == null)
                labelStyle = new GUIStyle("MiniLabel") { richText = true, fontSize = 10 };


            serializedObject.Update();

            Color c = GUI.backgroundColor;
            GUI.backgroundColor = titleBackgroundColor;
            GUILayout.Box("<color=#333333><b>GAssets 配置</b></color>", titleStyle, GUILayout.Height(40));
            GUI.backgroundColor = c;

            EditorGUILayout.Space(10);

            DrawPropertyField(DomainType.General, ref bothFoldout, g);
            DrawPropertyField(DomainType.Editor, ref editorFoldout, e);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyField(DomainType domain, ref bool flag, Dictionary<string, InfoPropAttri> dic)
        {
            if (dic == null || dic.Count == 0) { return; }

            EditorGUILayout.BeginVertical("box");
            if (domain != DomainType.General)
            {
                flag = EditorGUILayout.Foldout(flag, new GUIContent(domain.ToString()), true);
            }
            if (flag)
            {
                EditorGUILayout.Space(5);
                EditorGUI.indentLevel += (domain != DomainType.General) ? 2 : 0;
                foreach (string k in dic.Keys)
                {
                    InfoPropAttri attri = dic[k];
                    EditorGUILayout.LabelField(attri.Desc); // 属性标签
                    SerializedProperty prop = serializedObject.FindProperty(k);
                    EditorGUILayout.PropertyField(prop, new GUIContent("")); // 属性
                    if (!string.IsNullOrEmpty(attri.Help))
                    {// 帮助说明

                        string str = attri.Help;
                        if (k == "AssetRootPath")
                        {
                            str += "\n<color=#88b075>" + Paths.StreamingPathAppend(prop.stringValue) + "</color>";
                        }
                        else if (k == "AssetDataPath" || k == "AssetDownloadPath")
                        {
                            str += "\n<color=#88b075>" + Paths.PersistentPathAppend(prop.stringValue) + "</color>";
                        }
                        EditorGUILayout.LabelField("<color=#777777>" + str + "</color>", labelStyle, GUILayout.Height(attri.HelpHeight));
                    }
                    EditorGUILayout.Space(15);
                }
                EditorGUI.indentLevel -= (domain != DomainType.General) ? 2 : 0;
            }
            EditorGUILayout.EndVertical();
        }
    }

#endif

}