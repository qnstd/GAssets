using System.IO;
using UnityEngine;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// 路径工具
    /// <para>作者：强辰</para>
    /// </summary>
    public class Paths
    {
        /// <summary>
        /// 获取各平台 StreamingAssets 目录路径
        /// </summary>
        /// <returns></returns>
        static public string StreamingPath
        {
            get
            {
                string path = "";
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                path = Application.streamingAssetsPath;

#elif UNITY_ANDROID
            path = "jar:file://" + Application.dataPath + "!/assets";

#elif UNITY_IOS || UNITY_IPHONE
            path = Application.dataPath + "/Raw";
#endif
                return path;
            }
        }



        /// <summary>
        /// 获取各平台 PersistentData 目录路径
        /// </summary>
        /// <returns></returns>
        static public string PersistentPath
        {
            get
            {
                return Application.persistentDataPath;
            }
        }



        /// <summary>
        /// 将参数路径的'\'符号替换为'/'符号
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        static public string Replace(string path)
        {
            return path.Replace("\\", "/");
        }



        /// <summary>
        /// 在 StreamingAsset 目录下追加目录结构
        /// </summary>
        /// <param name="p">目录结构</param>
        /// <returns></returns>
        static public string StreamingPathAppend(string p)
        {
            string newpath = string.IsNullOrEmpty(p)
                            ? StreamingPath
                            : Path.Combine(StreamingPath, p);
            return Replace(newpath);
        }



        /// <summary>
        /// 在 PersistentData 目录下追加目录结构
        /// </summary>
        /// <param name="p">目录结构</param>
        /// <returns></returns>
        static public string PersistentPathAppend(string p)
        {
            string newpath = string.IsNullOrEmpty(p)
                                ? PersistentPath
                                : Path.Combine(PersistentPath, p);
            return Replace(newpath);
        }



        /// <summary>
        /// 将 unity 工程内的资源或目录的路径转为以 ‘Assets’ 为开头的路径
        /// </summary>
        /// <param name="p">路径（例如：F:/CustomProject/Assets/custom.png）</param>
        /// <returns>以 ‘Assets’ 为开头的路径</returns>
        static public string GetPathStartWithAssets(string p)
        {
            if (string.IsNullOrEmpty(p)) { return p; }
            if (p.StartsWith("Assets")) { return p; } // 若已经是以 Assets 为开头的路径则直接返回，不做任何处理

            string newp = $"Assets{p[Application.dataPath.Length..]}";
            newp = Replace(newp);
            //Debug.Log(newp);

            return newp;
        }

    }
}