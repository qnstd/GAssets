using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace cngraphi.gassets.editor.common
{
    /// <summary>
    /// 分析相关操作的工具类
    /// <para>作者：强辰</para>
    /// </summary>
    public class Profilers
    {

        /// <summary>
        /// 得到 AB包 在当前平台下运行时的内存占用大小
        /// </summary>
        /// <param name="k">ab包名称</param>
        /// <returns>字节数</returns>
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
        /// 得到资源在当前平台下（本机）运行时的内存占用大小
        /// </summary>
        /// <param name="k">资源路径（以Asset为开头的目录）</param>
        /// <returns>字节数</returns>
        static public long GetResRuntimeSize(string k)
        {
            return Profiler.GetRuntimeMemorySizeLong(AssetDatabase.LoadAssetAtPath<Object>(k));

            #region 记录其他内存类型的获取方式
            // 物理内存
            //return new FileInfo(p).Length;

            // 资源的本机存储内存（目前unity dll只提供了纹理资源的获取，其他资源暂时没有找到可反射的函数）
            //System.Type t = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
            //MethodInfo method = t.GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            //return (long)method.Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<Texture>(p) });
            #endregion
        }


    }
}