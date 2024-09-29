using System;
using System.Collections.Generic;
using cngraphi.gassets.common;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 负责AssetBundle文件加载的加载器基类
    /// <para>作者：强辰</para>
    /// </summary>
    public class GBundleLoader : GLoader
    {
        #region 静态

        //缓存bundle加载器
        static public readonly Dictionary<string, GBundleLoader> m_cache = new Dictionary<string, GBundleLoader>();

        /// <summary>
        /// 内部加载
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        internal static GBundleLoader LoadInternal(ABInfo bundle)
        {
            if (bundle == null) throw new NullReferenceException();

            if (!m_cache.TryGetValue(bundle.m_name, out var loa))
            {
                if (loa == null)
                {
                    loa = new GBundleLoaderLocal
                    {
                        Path = GAssetManager.Ins.GetResPath(bundle.m_name),//获取ab文件本地路径
                        m_info = bundle
                    };
                    m_cache.Add(bundle.m_name, loa);
                }
            }

            loa.Load();
            return loa;
        }

        #endregion



        protected ABInfo m_info;

        /// <summary>
        /// 当前加载的AssetBundle内存映射对象
        /// </summary>
        public AssetBundle AssetBundle { get; protected set; }


        //异步加载AssetBundle操作
        protected AssetBundleCreateRequest LoadAssetBundleAsync(string url)
        {
            return AssetBundle.LoadFromFileAsync(url);
        }

        //同步加载AssetBundle操作
        protected AssetBundle LoadAssetBundle(string url)
        {
            return AssetBundle.LoadFromFile(url);
        }

        //加载完毕的处理
        //子类调用
        protected void OnLoaded(AssetBundle bundle)
        {
            AssetBundle = bundle;
            Finish(AssetBundle == null ? "加载AssetBundle为空（null）" : null);
        }

        //卸载处理
        //卸载AssetBundle内存映射以及ab包内关联的资源
        protected override void OnUnload()
        {
            m_cache.Remove(m_info.m_name);
            if (AssetBundle == null) return;

            AssetBundle.Unload(true);
            AssetBundle = null;
        }
    }

}
