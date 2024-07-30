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
    /// ����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GAssetSettings : ScriptableObject
    {
        [InfoPropAttri("��Դ�汾��", DomainType.General)]
        public string Version = "1.0.0";


        [InfoPropAttri("��Դ��Ŀ¼", DomainType.General, "* ��δ���ã���Ϊ StreamingAssets Ŀ¼������Ϊ StreamingAssets + ���õ�Ŀ¼.", 26)]
        public string AssetRootPath = "Bundles";


        [InfoPropAttri("�־û�����Ŀ¼", DomainType.General, "* ��δ���ã���Ϊ PersistentData Ŀ¼������Ϊ PersistentData + ���õ�Ŀ¼.", 26)]
        public string AssetDataPath = "Data/Bundles";


        [InfoPropAttri("��Դ���صĴ洢Ŀ¼", DomainType.General, "* ��δ���ã���Ϊ PersistentData Ŀ¼������Ϊ PersistentData + ���õ�Ŀ¼.", 26)]
        public string AssetDownloadPath = "Data/Download";


#if UNITY_EDITOR
        [InfoPropAttri("ƽ̨", DomainType.Editor)]
        public BuildTarget Platform = BuildTarget.StandaloneWindows;


        [InfoPropAttri("��Դ��ѹ����ʽ", DomainType.Editor)]
        public BuildAssetBundleOptions ABOptions = BuildAssetBundleOptions.ChunkBasedCompression;


        [InfoPropAttri("SpriteAtlas ͼ�����Ŀ¼", DomainType.Editor)]
        public string AtlasOutputPath = "Assets/Res/Atlas";


        [InfoPropAttri
        (
            "����δʹ����Դ�ı���Ŀ¼",
            DomainType.Editor,
            "* ���齫����Ŀ¼������ Assets ֮��. ���� Assets ֮�ڻ������Դ���룬�Ӷ�Ӱ�촦���ٶ�."
        )]
        public string CleanBackupPath = Paths.Replace(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "BackupUnused")); // Assets/BackupUnused


        [InfoPropAttri
        (
            "����δʹ����Դʱ�ĺ�����",
            DomainType.Editor,
            "* Ĭ�Ϻ��� .unity �ļ�����"
        )]
        public string CleanIgnore = "prefab,spriteatlas,shader,shadergraph,shadersubgraph,hlsl,compute,cs,txt,json,xml";


        [InfoPropAttri
        (
            "����δʹ����Դʱ���ļ���չ",
            DomainType.Editor,
            "* ����Դ����Դ���ڽű������õ��ı��ļ��б�����ʱ�����޷�ͨ������������ϵ�������ü����ġ���ˣ���չ�༭���ڿ�������Ҫ���ж���Դ���ü������ı��ļ����͡�\n" +
            "* ���ı��ļ��м�����ĳһ��Դ����������λ�ô���ע��״̬������Ȼ�ж��������ù�ϵ��",
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
            GUILayout.Box("<color=#333333><b>GAssets ����</b></color>", titleStyle, GUILayout.Height(40));
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
                    EditorGUILayout.LabelField(attri.Desc); // ���Ա�ǩ
                    SerializedProperty prop = serializedObject.FindProperty(k);
                    EditorGUILayout.PropertyField(prop, new GUIContent("")); // ����
                    if (!string.IsNullOrEmpty(attri.Help))
                    {// ����˵��

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