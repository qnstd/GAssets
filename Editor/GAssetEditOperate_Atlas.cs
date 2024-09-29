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
    /// 创建、更新 SpriteAtlas 图集工具
    /// <para>作者：强辰</para>
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
            #region 创建、更新 SpriteAtlas
            m_atlasFoldout = EditorGUILayout.Foldout(m_atlasFoldout, new GUIContent("图集"), true);
            if (m_atlasFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel += 2;
                //散图尺寸
                m_limitSize = EditorGUILayout.Vector2IntField("单张纹理尺寸限制", m_limitSize);

                //图集尺寸
                EditorGUILayout.Space(10);
                m_size = EditorGUILayout.IntField("图集尺寸", m_size);

                //图集格式
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("图集格式");
                EditorGUI.indentLevel += 2;
                EditorGUILayout.BeginVertical("box");
                OnGUI_PlatformTexInfo("默认：", ref m_default, ref m_defaultQuality);
                OnGUI_PlatformTexInfo("PC：", ref m_pc, ref m_pcQuality);
                OnGUI_PlatformTexInfo("Android：", ref m_android, ref m_androidQuality);
                OnGUI_PlatformTexInfo("iOS：", ref m_ios, ref m_iosQuality);
                OnGUI_PlatformTexInfo("WebGL：", ref m_webgl, ref m_webglQuality);
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("<color=#ffcc00>* 压缩品质 50% 视为正常品质</color>", Gui.LabelStyle);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 2;

                //图集选择
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("<color=#999999>散图目录（支持多个目录。从 Project 窗口选择目录后直接拖至下方区域）</color>", Gui.LabelStyle);
                Gui.DragTextArea(this, 160, ref m_resdirs, ref m_resDirsAry);


                //其他模块
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(1);
                if (GUILayout.Button("生成", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(20))) { CreateAtlas(); }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel -= 2;
                EditorGUILayout.EndVertical();
            }
            #endregion

            EditorGUILayout.Space(8);

            #region SpriteAtlas 变体
            m_atlasVariFoldout = EditorGUILayout.Foldout(m_atlasVariFoldout, new GUIContent("图集变体"), true);
            if (m_atlasVariFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel += 2;
                m_Master = (SpriteAtlas)EditorGUILayout.ObjectField("主图集", m_Master, typeof(SpriteAtlas), false);
                EditorGUILayout.Space(5);
                m_MasterVariScale = EditorGUILayout.Slider("分辨率", m_MasterVariScale, 0.1f, 1.0f);
                EditorGUILayout.Space(5);
                m_VariName = EditorGUILayout.TextField("变体名称前缀", m_VariName);
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(1);
                if (GUILayout.Button("生成", Gui.BtnStyle, GUILayout.Width(60), GUILayout.Height(20)))
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
            quality = EditorGUILayout.IntSlider("压缩品质", quality, 0, 100); // 范围 [0, 100]
            EditorGUILayout.EndHorizontal();
        }


        private void CreateAtlas()
        {
            if (m_resDirsAry == null || m_resDirsAry.Length == 0)
            {
                Dialog.Tip("请选择操作目录！");
                return;
            }
            if (string.IsNullOrEmpty(settings.AtlasOutputPath))
            {
                Dialog.Tip("请选择输出目录!");
                return;
            }
            if (m_size <= 0)
            {
                Dialog.Tip("图集尺寸不正确！");
                return;
            }
            // 处理
            foreach (string s in m_resDirsAry)
            {
                if (IO.IsDir(s))
                {
                    DealOne(s);
                }
            }
            Dialog.Tip("操作完毕!");
        }
        private void DealOne(string dir)
        {
            //获取图集绑定的精灵
            List<string> lst = new List<string>();
            IO.GetFiles(dir, lst);
            if (lst.Count == 0)
            {
                Debug.LogError($"无法创建图集！散图目录为空。Path = {dir}");
                return;
            }

            //图集文件名以及AB包标签名
            string filename = dir.Substring(dir.LastIndexOf("/") + 1).ToLower();

            //创建atlas图集，并设置相关操作
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
                    Debug.LogError($"单张散图尺寸越界。width = {curwidth} / height = {curheight} / file = {s}");
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
                Debug.LogError($"无法创建图集！单张散图的尺寸均越界。Path = {dir}");
                return;
            }
            atlas.Add(splst.ToArray());

            //创建文件
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

            //设置AB包标签
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
        /// 生成主图集的变体（其他分辨率图集）
        /// </summary>
        private void GenMaterVari()
        {
            if (m_Master == null || string.IsNullOrEmpty(m_VariName))
            {
                Dialog.Tip("参数不正确.");
                return;
            }

            SpriteAtlas atlas = new SpriteAtlas();
            atlas.SetIsVariant(true); // 设置变体类型
            atlas.SetMasterAtlas(m_Master); // 绑定主图集
            atlas.SetIncludeInBuild(false);
            atlas.SetVariantScale(m_MasterVariScale); // 设置变体的缩放比

            // 将主图集的纹理设置信息传给变体
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

            //创建文件
            if (File.Exists(newp))
            {
                AssetDatabase.DeleteAsset(newp);
            }
            AssetDatabase.CreateAsset(atlas, newp);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //设置AB包标签
            //AssetImporter importer = AssetImporter.GetAtPath(newp);
            //importer.assetBundleName = filename.Split(".")[0]; // 单独为变体生成一个ab标记
            //importer.assetBundleVariant = "ab";
            //importer.SaveAndReimport();

            Dialog.Tip("生成完毕.");
        }

    }
}