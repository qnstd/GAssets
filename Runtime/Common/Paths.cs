using System.IO;
using UnityEngine;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// ·������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class Paths
    {
        /// <summary>
        /// ��ȡ��ƽ̨ StreamingAssets Ŀ¼·��
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
        /// ��ȡ��ƽ̨ PersistentData Ŀ¼·��
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
        /// ������·����'\'�����滻Ϊ'/'����
        /// </summary>
        /// <param name="path">·��</param>
        /// <returns></returns>
        static public string Replace(string path)
        {
            return path.Replace("\\", "/");
        }



        /// <summary>
        /// �� StreamingAsset Ŀ¼��׷��Ŀ¼�ṹ
        /// </summary>
        /// <param name="p">Ŀ¼�ṹ</param>
        /// <returns></returns>
        static public string StreamingPathAppend(string p)
        {
            string newpath = string.IsNullOrEmpty(p)
                            ? StreamingPath
                            : Path.Combine(StreamingPath, p);
            return Replace(newpath);
        }



        /// <summary>
        /// �� PersistentData Ŀ¼��׷��Ŀ¼�ṹ
        /// </summary>
        /// <param name="p">Ŀ¼�ṹ</param>
        /// <returns></returns>
        static public string PersistentPathAppend(string p)
        {
            string newpath = string.IsNullOrEmpty(p)
                                ? PersistentPath
                                : Path.Combine(PersistentPath, p);
            return Replace(newpath);

        }

    }
}