using System;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 加载 AssetBundle 中资源的加载器
    /// <para>作者：强辰</para>
    /// </summary>
    public class GBundleAssetLoader : GAssetLoader
    {
        private GDependLoader m_dependLoader;
        private AssetBundleRequest m_req;

        static internal GBundleAssetLoader Create(string path, Type type)
        {
            return new GBundleAssetLoader
            {
                Path = path, //资源路径
                Typ = type //类型
            };
        }


        /// <summary>
        /// 开始加载准备
        /// </summary>
        protected override void OnLoad()
        {
            m_dependLoader = GDependLoader.Load(Path);
            Status = GLoaderStatus.DependsLoading;
        }


        /// <summary>
        /// 卸载
        /// </summary>
        protected override void OnUnload()
        {
            if (m_dependLoader != null)
            {
                m_dependLoader.Dispose();
                m_dependLoader = null;
            }
            m_req = null;
            Asset = null;
            base.OnUnload();
        }


        /// <summary>
        /// 立刻完成加载操作
        /// </summary>
        public override void Immediate()
        {
            if (IsDone) { return; }

            if (m_dependLoader == null)
            {
                Finish("当前内部加载处理的GDependLoader加载器为null。");
                return;
            }

            if (!m_dependLoader.IsDone) m_dependLoader.Immediate();

            if (m_dependLoader.AssetBundle == null)
            {
                Finish("当前内部加载处理的 GDependLoader.AssetBundle 主ab文件内存映射为空.");
                return;
            }

            if (IsSubAssets)
            {//加载复合型资源
                SubAssets = m_dependLoader.AssetBundle.LoadAssetWithSubAssets(Path, Typ);
                Finish();
            }
            else
            {//加载指定资源
                OnLoaded(m_dependLoader.AssetBundle.LoadAsset(Path, Typ));
            }
        }

        protected override void OnUpdate()
        {
            switch (Status)
            {
                case GLoaderStatus.Loading:
                    ResLoading();
                    break;
                case GLoaderStatus.DependsLoading:
                    DepLoading();
                    break;
            }
        }


        /// <summary>
        /// 加载所需的资源
        /// </summary>
        private void ResLoading()
        {
            if (m_req == null)
            {
                Finish("负责资源加载的AssetBundleRequest对象为空（null）");
                return;
            }

            Progress = 0.5f + m_req.progress * 0.5f;
            if (!m_req.isDone) { return; }

            if (IsSubAssets)
            {
                SubAssets = m_req.allAssets;
                Finish(SubAssets == null ? "复合型资源加载后为空" : null);
            }
            else
            {
                OnLoaded(m_req.asset);
            }
        }


        /// <summary>
        /// 加载资源所需的ab文件及依赖ab文件
        /// </summary>
        private void DepLoading()
        {
            if (m_dependLoader == null)
            {
                Finish("当前内部加载处理的GDependLoader加载器为null。");
                return;
            }

            Progress = 0.5f * m_dependLoader.Progress;
            if (!m_dependLoader.IsDone) { return; }

            var ab = m_dependLoader.AssetBundle;
            if (ab == null)
            {
                Finish("当前内部加载处理的 GDependLoader.AssetBundle 主ab文件内存映射为空.");
                return;
            }

            //加载ab包及依赖完毕，进行资源加载
            m_req = IsSubAssets ? ab.LoadAssetWithSubAssetsAsync(Path, Typ) : ab.LoadAssetAsync(Path, Typ);
            Status = GLoaderStatus.Loading;
        }
    }

}