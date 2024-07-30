using UnityEditor;
using UnityEngine;


namespace cngraphi.gassets.editor.common
{
    /// <summary>
    /// 对话框工具
    /// <para>作者：强辰</para>
    /// </summary>
    public class Dialog
    {
        /// <summary>
        /// 选择目录对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <returns></returns>
        static public string Directory(string title)
        {
            string path = EditorUtility.OpenFolderPanel(title, Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
                path = cngraphi.gassets.common.Paths.Replace(path);
            return path;
        }


        /// <summary>
        /// 选择文件对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <returns></returns>
        static public string File(string title)
        {
            string path = EditorUtility.OpenFilePanel(title, Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
                path = cngraphi.gassets.common.Paths.Replace(path);
            return path;
        }



        /// <summary>
        /// 显示提示框
        /// </summary>
        /// <param name="msg">提示信息</param>
        static public void Tip(string msg, string title = "提示", string btnname = "确定")
        {
            if (string.IsNullOrEmpty(msg)) { return; }
            EditorUtility.DisplayDialog(title, msg, btnname);
        }


        /// <summary>
        /// 显示确认框
        /// </summary>
        /// <param name="msg">提示信息</param>
        /// <param name="title">标题</param>
        /// <param name="btnok">确认按钮标签</param>
        /// <param name="btncancel">取消按钮标签</param>
        /// <returns>0：代表确认；其他值代表取消操作</returns>
        static public int Confirm(string msg, string title = "提示", string btnok = "确定", string btncancel = "取消")
        {
            return EditorUtility.DisplayDialogComplex(title, msg, btnok, btncancel, "");
        }
    }
}