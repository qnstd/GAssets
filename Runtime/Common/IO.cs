using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// IO����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class IO
    {
        /// <summary>
        /// ���Ŀ¼���������ļ�����Ŀ¼
        /// <para>������ǰ����Ŀ¼</para>
        /// </summary>
        /// <param name="path">Ŀ¼·��</param>
        static public void DirClear(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
                files[i].Delete();
            DirectoryInfo[] cdirs = dir.GetDirectories();
            string foldername;
            for (int j = 0; j < cdirs.Length; j++)
            {
                foldername = cdirs[j].FullName;
                DirClear(foldername);
                Directory.Delete(foldername);
            }
        }


        /// <summary>
        /// ɾ��Ŀ¼
        /// <para>֧�ֵݹ�ɾ����ͬʱ�ὫĿ¼����ɾ��</para>
        /// </summary>
        /// <param name="path">Ŀ¼·��</param>
        static public void DirDelete(string path)
        {
            if (!Directory.Exists(path)) { return; }
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (FileSystemInfo f in files)
            {
                if (f is DirectoryInfo)
                    DirDelete(f.FullName);
                else
                    File.Delete(f.FullName);
            }
            Directory.Delete(path);
        }



        /// <summary>
        /// �����ļ�
        /// <para>���Ŀ�����Ҫ�������ļ���������滻</para>
        /// </summary>
        /// <param name="source">Ҫ���������ļ�</param>
        /// <param name="tar">Ŀ��λ��</param>
        static public void CopyFile(string source, string tar)
        {
            if (File.Exists(tar))
                File.Delete(tar);
            File.Copy(source, tar);
        }



        /// <summary>
        /// �����Ƿ���Ŀ¼
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool IsDir(string path)
        {
            return (new FileInfo(path).Attributes & FileAttributes.Directory) != 0;
        }


        /// <summary>
        /// ��ȡĿ¼�������ļ�
        /// </summary>
        /// <param name="path">Ŀ¼</param>
        /// <param name="lst">�����</param>
        /// <param name="ignores">�����ļ������ܲ����Ƿ���ֵ������ȥ�� .meta �ļ���</param>
        /// <param name="starWithAsset">�Ƿ���AssetΪ��ͷ��·��</param>
        static public void GetFiles(string path, List<string> lst, List<string> ignores = null, bool starWithAsset = true)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] fsi = dir.GetFileSystemInfos();
            foreach (FileSystemInfo f in fsi)
            {
                if (f is DirectoryInfo) { GetFiles(f.FullName, lst, ignores); }
                else
                {
                    string p = f.FullName;
                    string ext = f.Extension;

                    if (ext == ".meta") { continue; } // ȥ�� .meta
                    if (ignores != null && ignores.IndexOf(ext) != -1) { continue; } // �Ƿ�����ں�������

                    if (starWithAsset)
                    {// �� Assets Ϊ��ͷ��·��
                        p = Paths.Replace(p);
                        if (p.IndexOf(Application.dataPath) == 0)
                        {// ������ Unity �����µ���Դ�ſɽ���·������
                            p = "Assets" + p.Substring(Application.dataPath.Length);
                        }
                    }
                    lst.Add(p);
                }
            }
        }



        /// <summary>
        /// �ݹ鴴��Ŀ¼
        /// </summary>
        /// <param name="path">·��</param>
        static public void RecursionDirCreate(string path)
        {
            if (Directory.Exists(path)) { return; }
            path = Paths.Replace(path);

            Str.Split(path, "/", out List<string> lst);
            string[] strs = lst.ToArray();
            int len = strs.Length;
            string p = "";
            for (int i = 0; i < len; i++)
            {
                p += strs[i] + "/";
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
            }
        }


        /// <summary>
        /// �ƶ�Ŀ¼
        /// <para>* ���Ŀ��Ŀ¼����Ҫ���ƶ���Ŀ¼������ɾ�����ƶ�</para>
        /// </summary>
        /// <param name="source">Ҫ�ƶ���Ŀ¼</param>
        /// <param name="tar">Ŀ��Ŀ¼</param>
        static public void MoveFolder(string source, string tar)
        {
            if (Directory.Exists(tar))
            {
                DirClear(tar);
                Directory.Delete(tar);
            }
            Directory.Move(source, tar);
        }



        /// <summary>
        /// �ƶ��ļ�
        /// <para>* ���Ŀ����ڣ�����ɾ�����ƶ�</para>
        /// </summary>
        /// <param name="source">Ҫ���ƶ����ļ�</param>
        /// <param name="tar">�ļ��ƶ�Ŀ¼λ�ã������λ�ò���Ŀ¼�������ƶ�����ļ���</param>

        static public void MoveFile(string source, string tar)
        {
            if (File.Exists(tar))
                File.Delete(tar);
            File.Move(source, tar);
        }


    }
}