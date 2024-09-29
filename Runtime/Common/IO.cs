using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// IO操作
    /// <para>作者：强辰</para>
    /// </summary>
    public class IO
    {
        /// <summary>
        /// 清空目录内所有子文件及子目录
        /// <para>保留当前参数目录</para>
        /// </summary>
        /// <param name="path">目录路径</param>
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
        /// 删除目录
        /// <para>支持递归删除，同时会将目录本身删除</para>
        /// </summary>
        /// <param name="path">目录路径</param>
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
        /// 拷贝文件
        /// <para>如果目标存在要拷贝的文件，则进行替换</para>
        /// </summary>
        /// <param name="source">要被拷贝的文件</param>
        /// <param name="tar">目标位置</param>
        static public void CopyFile(string source, string tar)
        {
            if (File.Exists(tar))
                File.Delete(tar);
            File.Copy(source, tar);
        }



        /// <summary>
        /// 参数是否是目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool IsDir(string path)
        {
            return (new FileInfo(path).Attributes & FileAttributes.Directory) != 0;
        }


        /// <summary>
        /// 获取目录下所有文件
        /// </summary>
        /// <param name="path">目录</param>
        /// <param name="lst">结果集</param>
        /// <param name="ignores">忽略文件（不管参数是否有值，都会去除 .meta 文件）</param>
        /// <param name="starWithAsset">是否以Asset为开头的路径</param>
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

                    if (ext == ".meta") { continue; } // 去除 .meta
                    if (ignores != null && ignores.IndexOf(ext) != -1) { continue; } // 是否存在于忽略组中

                    if (starWithAsset)
                    {// 以 Assets 为开头的路径
                        p = Paths.Replace(p);
                        if (p.IndexOf(Application.dataPath) == 0)
                        {// 必须是 Unity 工程下的资源才可进行路径设置
                            p = "Assets" + p.Substring(Application.dataPath.Length);
                        }
                    }
                    lst.Add(p);
                }
            }
        }



        /// <summary>
        /// 递归创建目录
        /// </summary>
        /// <param name="path">路径</param>
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
        /// 移动目录
        /// <para>* 如果目标目录存在要被移动的目录，则先删除再移动</para>
        /// </summary>
        /// <param name="source">要移动的目录</param>
        /// <param name="tar">目标目录</param>
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
        /// 移动文件
        /// <para>* 如果目标存在，则先删除再移动</para>
        /// </summary>
        /// <param name="source">要被移动的文件</param>
        /// <param name="tar">文件移动目录位置（这里的位置不是目录，而是移动后的文件）</param>

        static public void MoveFile(string source, string tar)
        {
            if (File.Exists(tar))
                File.Delete(tar);
            File.Move(source, tar);
        }


    }
}