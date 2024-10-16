using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace cngraphi.gassets.editor.common
{
    /// <summary>
    /// ������ز����Ĺ�����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class Profilers
    {

        /// <summary>
        /// �õ� AB�� �ڵ�ǰƽ̨������ʱ���ڴ�ռ�ô�С
        /// </summary>
        /// <param name="k">ab������</param>
        /// <returns>�ֽ���</returns>
        static public long GetABRuntimeSize(string k)
        {
            string[] childs = AssetDatabase.GetAssetPathsFromAssetBundle(k);
            long allbytes = 0;
            for (int i = 0; i < childs.Length; i++)
            {
                allbytes += GetResRuntimeSize(childs[i]);
            }
            return allbytes;
        }



        /// <summary>
        /// �õ���Դ�ڵ�ǰƽ̨�£�����������ʱ���ڴ�ռ�ô�С
        /// </summary>
        /// <param name="k">��Դ·������AssetΪ��ͷ��Ŀ¼��</param>
        /// <returns>�ֽ���</returns>
        static public long GetResRuntimeSize(string k)
        {
            return Profiler.GetRuntimeMemorySizeLong(AssetDatabase.LoadAssetAtPath<Object>(k));

            #region ��¼�����ڴ����͵Ļ�ȡ��ʽ
            // �����ڴ�
            //return new FileInfo(p).Length;

            // ��Դ�ı����洢�ڴ棨Ŀǰunity dllֻ�ṩ��������Դ�Ļ�ȡ��������Դ��ʱû���ҵ��ɷ���ĺ�����
            //System.Type t = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
            //MethodInfo method = t.GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            //return (long)method.Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<Texture>(p) });
            #endregion
        }


    }
}