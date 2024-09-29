using System.IO;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;

namespace cngraphi.gassets.editor
{
    /// <summary>
    /// 工具默认页
    /// <para>作者：强辰</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {

        private void OnEnable_Default() { }

        private void OnDisable_Default() { }


        private void OnGUI_Default()
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField(Constants.LIBRARY_DISPLAYNAME, Gui.LabelHeadLeft);
            EditorGUILayout.Space(1);
            EditorGUILayout.LabelField("<color=#999999>轻量级 Unity3D 资源管理器</color>", Gui.LabelStyle);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            foreach (string k in Constants.MDS.Keys)
            {
                if (GUILayout.Button($"<color=#cccccc>{k}</color>", Gui.BtnStyle, GUILayout.Width(90)))
                {
                    string p = Path.GetFullPath($"Packages/{Constants.PACKAGES_NAME}");
                    p = p.Replace("\\", "/");
                    //Debug.Log(p);

                    string filepath = Path.Combine(p, Constants.MDS[k]).Replace("\\", "/");
                    EditorUtility.RevealInFinder(filepath); // 打开文件所在目录，并选中此文件
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}