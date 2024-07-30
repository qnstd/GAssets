using UnityEditor;
using UnityEngine;


namespace cngraphi.gassets.editor.common
{
    /// <summary>
    /// �Ի��򹤾�
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class Dialog
    {
        /// <summary>
        /// ѡ��Ŀ¼�Ի���
        /// </summary>
        /// <param name="title">����</param>
        /// <returns></returns>
        static public string Directory(string title)
        {
            string path = EditorUtility.OpenFolderPanel(title, Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
                path = cngraphi.gassets.common.Paths.Replace(path);
            return path;
        }


        /// <summary>
        /// ѡ���ļ��Ի���
        /// </summary>
        /// <param name="title">����</param>
        /// <returns></returns>
        static public string File(string title)
        {
            string path = EditorUtility.OpenFilePanel(title, Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
                path = cngraphi.gassets.common.Paths.Replace(path);
            return path;
        }



        /// <summary>
        /// ��ʾ��ʾ��
        /// </summary>
        /// <param name="msg">��ʾ��Ϣ</param>
        static public void Tip(string msg, string title = "��ʾ", string btnname = "ȷ��")
        {
            if (string.IsNullOrEmpty(msg)) { return; }
            EditorUtility.DisplayDialog(title, msg, btnname);
        }


        /// <summary>
        /// ��ʾȷ�Ͽ�
        /// </summary>
        /// <param name="msg">��ʾ��Ϣ</param>
        /// <param name="title">����</param>
        /// <param name="btnok">ȷ�ϰ�ť��ǩ</param>
        /// <param name="btncancel">ȡ����ť��ǩ</param>
        /// <returns>0������ȷ�ϣ�����ֵ����ȡ������</returns>
        static public int Confirm(string msg, string title = "��ʾ", string btnok = "ȷ��", string btncancel = "ȡ��")
        {
            return EditorUtility.DisplayDialogComplex(title, msg, btnok, btncancel, "");
        }
    }
}