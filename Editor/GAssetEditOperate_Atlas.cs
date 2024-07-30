using System.Collections.Generic;
using System.IO;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ���������� SpriteAtlas ͼ������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        const string C_FileExten = ".spriteatlas";

        SpriteAtlasPackingSettings m_packingSettings = new SpriteAtlasPackingSettings()
        {
            blockOffset = 1,
            enableRotation = false,
            enableTightPacking = false,
            padding = 2
        };
        SpriteAtlasTextureSettings m_textureSettings = new SpriteAtlasTextureSettings()
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        };
        TextureImporterPlatformSettings m_defaultsettings = new TextureImporterPlatformSettings()
        {
            name = "DefaultTexturePlatform",
            crunchedCompression = true
        };
        TextureImporterPlatformSettings m_standalonesettings = new TextureImporterPlatformSettings()
        {
            name = "Standalone",
            overridden = true
        };
        TextureImporterPlatformSettings m_iossettings = new TextureImporterPlatformSettings()
        {
            name = "iPhone",
            overridden = true
        };
        TextureImporterPlatformSettings m_androidsettings = new TextureImporterPlatformSettings()
        {
            name = "Android",
            overridden = true
        };
        TextureImporterPlatformSettings m_webglsettings = new TextureImporterPlatformSettings()
        {
            name = "WebGL",
            overridden = true
        };
        TextureImporterFormat m_default = TextureImporterFormat.Automatic;
        TextureImporterFormat m_pc = TextureImporterFormat.DXT5Crunched;
        TextureImporterFormat m_ios = TextureImporterFormat.PVRTC_RGBA4;
        TextureImporterFormat m_android = TextureImporterFormat.ETC2_RGBA8;
        TextureImporterFormat m_webgl = TextureImporterFormat.DXT5Crunched;
        int m_defaultQuality = 50;
        int m_pcQuality = 50;
        int m_iosQuality = 50;
        int m_androidQuality = 50;
        int m_webglQuality = 50;


        Vector2Int m_limitSize = new Vector2Int(512, 512);
        int m_size = 1024;
        string m_resdirs;
        string[] m_resDirsAry;

        SpriteAtlas m_Master = null;
        float m_MasterVariScale = 1.0f;
        string m_VariName = "vari_";

        bool m_atlasFoldout = true;
        bool m_atlasVariFoldout = true;


        // ////////////////////////////////////////////////////////////////////////


        private void OnEnable_Atlas()
        {
        }
        private void OnDisable_Atlas()
        {
            m_Master = null;
            m_resDirsAry = null;
            m_resdirs = "";
        }
        private void OnGUI_Atlas()
        {
            #region ���������� SpriteAtlas
            m_atlasFoldout = EditorGUILayout.Foldout(m_atlasFoldout, new GUIContent("ͼ��"), true);
            if (m_atlasFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel += 2;
                //ɢͼ�ߴ�
                m_limitSize = EditorGUILayout.Vector2IntField("��������ߴ�����", m_limitSize);

                //ͼ���ߴ�
                EditorGUILayout.Space(10);
                m_size = EditorGUILayout.IntField("ͼ���ߴ�", m_size);

                //ͼ����ʽ
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("ͼ����ʽ");
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                OnGUI_PlatformTexInfo("Ĭ�ϣ�", ref m_default, ref m_defaultQuality);
                OnGUI_PlatformTexInfo("PC��", ref m_pc, ref m_pcQuality);
                OnGUI_PlatformTexInfo("Android��", ref m_android, ref m_androidQuality);
                OnGUI_PlatformTexInfo("iOS��", ref m_ios, ref m_iosQuality);
                OnGUI_PlatformTexInfo("WebGL��", ref m_webgl, ref m_webglQuality);
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("<color=#ffcc00>* ѹ��Ʒ�� 50% ��Ϊ����Ʒ��</color>", Gui.LabelStyle);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;

                //ͼ��ѡ��
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("<color=#999999>ɢͼĿ¼��֧�ֶ��Ŀ¼���� Project ����ѡ��Ŀ¼��ֱ�������·�����</color>", Gui.LabelStyle);
                Gui.DragTextArea(this, 160, ref m_resdirs, ref m_resDirsAry);


                //����ģ��
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(1);
                if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(20))) { CreateAtlas(); }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel -= 2;
                EditorGUILayout.EndVertical();
            }
            #endregion

            EditorGUILayout.Space(8);

            #region SpriteAtlas ����
            m_atlasVariFoldout = EditorGUILayout.Foldout(m_atlasVariFoldout, new GUIContent("ͼ������"), true);
            if (m_atlasVariFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel += 2;
                m_Master = (SpriteAtlas)EditorGUILayout.ObjectField("��ͼ��", m_Master, typeof(SpriteAtlas), false);
                EditorGUILayout.Space(5);
                m_MasterVariScale = EditorGUILayout.Slider("�ֱ���", m_MasterVariScale, 0.1f, 1.0f);
                EditorGUILayout.Space(5);
                m_VariName = EditorGUILayout.TextField("��������ǰ׺", m_VariName);
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(1);
                if (GUILayout.Button("����", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(20)))
                {
                    GenMaterVari();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel -= 2;
                EditorGUILayout.EndVertical();
            }
            #endregion
        }
        private void OnGUI_PlatformTexInfo(string platform, ref TextureImporterFormat f, ref int quality)
        {
            EditorGUILayout.BeginHorizontal();
            f = (TextureImporterFormat)EditorGUILayout.EnumPopup(platform, f);
            quality = EditorGUILayout.IntSlider("ѹ��Ʒ��", quality, 0, 100); // ��Χ [0, 100]
            EditorGUILayout.EndHorizontal();
        }


        private void CreateAtlas()
        {
            if (m_resDirsAry == null || m_resDirsAry.Length == 0)
            {
                Dialog.Tip("��ѡ�����Ŀ¼��");
                return;
            }
            if (string.IsNullOrEmpty(settings.AtlasOutputPath))
            {
                Dialog.Tip("��ѡ�����Ŀ¼!");
                return;
            }
            if (m_size <= 0)
            {
                Dialog.Tip("ͼ���ߴ粻��ȷ��");
                return;
            }
            // ����
            foreach (string s in m_resDirsAry)
            {
                if (IO.IsDir(s))
                {
                    DealOne(s);
                }
            }
            Dialog.Tip("�������!");
        }
        private void DealOne(string dir)
        {
            //��ȡͼ���󶨵ľ���
            List<string> lst = new List<string>();
            IO.GetFiles(dir, lst);
            if (lst.Count == 0)
            {
                Debug.LogError($"�޷�����ͼ����ɢͼĿ¼Ϊ�ա�Path = {dir}");
                return;
            }

            //ͼ���ļ����Լ�AB����ǩ��
            string filename = dir.Substring(dir.LastIndexOf("/") + 1).ToLower();

            //����atlasͼ������������ز���
            SpriteAtlas atlas = new SpriteAtlas();
            atlas.SetIncludeInBuild(false);
            atlas.SetPackingSettings(m_packingSettings);
            atlas.SetTextureSettings(m_textureSettings);

            PlatformSettingsInfo(ref atlas, ref m_defaultsettings, m_default, m_defaultQuality);
            PlatformSettingsInfo(ref atlas, ref m_standalonesettings, m_pc, m_pcQuality);
            PlatformSettingsInfo(ref atlas, ref m_iossettings, m_ios, m_iosQuality);
            PlatformSettingsInfo(ref atlas, ref m_androidsettings, m_android, m_androidQuality);
            PlatformSettingsInfo(ref atlas, ref m_webglsettings, m_webgl, m_webglQuality);

            List<Sprite> splst = new List<Sprite>();
            foreach (string s in lst)
            {
                //AssetImporter im = AssetImporter.GetAtPath(s);

                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(s);
                int curwidth = sp.texture.width;
                int curheight = sp.texture.height;
                if (curwidth > m_limitSize.x || curheight > m_limitSize.y)
                {
                    Debug.LogError($"����ɢͼ�ߴ�Խ�硣width = {curwidth} / height = {curheight} / file = {s}");
                    //im.assetBundleName = string.Empty;
                }
                else
                {
                    splst.Add(sp);
                    //im.assetBundleName = filename;
                    //im.assetBundleVariant = "ab";
                }
                //im.SaveAndReimport();
            }
            if (splst.Count == 0)
            {
                Debug.LogError($"�޷�����ͼ��������ɢͼ�ĳߴ��Խ�硣Path = {dir}");
                return;
            }
            atlas.Add(splst.ToArray());

            //�����ļ�
            string file = filename + C_FileExten;
            file = Path.Combine(settings.AtlasOutputPath, file);
            file = file.Substring(file.IndexOf("Assets"));
            if (System.IO.File.Exists(file))
            {
                AssetDatabase.DeleteAsset(file);
            }
            AssetDatabase.CreateAsset(atlas, file);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //����AB����ǩ
            //AssetImporter importer = AssetImporter.GetAtPath(file);
            //importer.assetBundleName = filename;
            //importer.assetBundleVariant = "ab";
            //importer.SaveAndReimport();
        }
        private void PlatformSettingsInfo(ref SpriteAtlas atlas, ref TextureImporterPlatformSettings setting, TextureImporterFormat format, int quality)
        {
            setting.maxTextureSize = m_size;
            setting.format = format;
            setting.compressionQuality = quality;
            atlas.SetPlatformSettings(setting);
        }


        /// <summary>
        /// ������ͼ���ı��壨�����ֱ���ͼ����
        /// </summary>
        private void GenMaterVari()
        {
            if (m_Master == null || string.IsNullOrEmpty(m_VariName))
            {
                Dialog.Tip("��������ȷ.");
                return;
            }

            SpriteAtlas atlas = new SpriteAtlas();
            atlas.SetIsVariant(true); // ���ñ�������
            atlas.SetMasterAtlas(m_Master); // ����ͼ��
            atlas.SetIncludeInBuild(false);
            atlas.SetVariantScale(m_MasterVariScale); // ���ñ�������ű�

            // ����ͼ��������������Ϣ��������
            SpriteAtlasTextureSettings sats = m_Master.GetTextureSettings();
            SpriteAtlasTextureSettings texset = new SpriteAtlasTextureSettings()
            {
                readable = sats.readable,
                generateMipMaps = sats.generateMipMaps,
                sRGB = sats.sRGB,
                filterMode = sats.filterMode
            };
            atlas.SetTextureSettings(texset);

            atlas.SetPlatformSettings(m_Master.GetPlatformSettings("DefaultTexturePlatform"));
            atlas.SetPlatformSettings(m_Master.GetPlatformSettings("Standalone"));
            atlas.SetPlatformSettings(m_Master.GetPlatformSettings("iPhone"));
            atlas.SetPlatformSettings(m_Master.GetPlatformSettings("Android"));
            atlas.SetPlatformSettings(m_Master.GetPlatformSettings("WebGL"));

            string p = AssetDatabase.GetAssetPath(m_Master);
            string prefix = p.Substring(0, p.LastIndexOf("/") + 1);
            string filename = m_VariName + Path.GetFileName(p);
            string newp = prefix + filename;

            //�����ļ�
            if (File.Exists(newp))
            {
                AssetDatabase.DeleteAsset(newp);
            }
            AssetDatabase.CreateAsset(atlas, newp);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //����AB����ǩ
            //AssetImporter importer = AssetImporter.GetAtPath(newp);
            //importer.assetBundleName = filename.Split(".")[0]; // ����Ϊ��������һ��ab���
            //importer.assetBundleVariant = "ab";
            //importer.SaveAndReimport();

            Dialog.Tip("�������.");
        }

    }
}