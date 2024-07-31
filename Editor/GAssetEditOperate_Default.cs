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


        private void ShowDefaultPage()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField(ProjectName, Gui.LabelHead);
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("<color=#999999>轻量级 Unity3D 资源管理器</color>", Gui.LabelStyleMiddle);
        }
    }
}