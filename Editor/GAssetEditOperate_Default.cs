using cngraphi.gassets.editor.common;
using UnityEditor;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 工具默认页
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        const string ProjectName = "GAssets 资源管理框架";
        const string CodeVersion = "1.0.0";
        const string Copyright = "Copyright ©2021-2024 CN.Graphi. All rights reserved.";


        private void ShowDefaultPage()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField(ProjectName, Gui.LabelHead);
            EditorGUILayout.LabelField("<color=#ffcc00>v" + CodeVersion + "</color>", Gui.LabelStyleMiddle);
            EditorGUILayout.LabelField("<color=#777777>" + Copyright + "</color>", Gui.LabelStyleMiddle);
        }
    }
}