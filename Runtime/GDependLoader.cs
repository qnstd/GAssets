using System.Collections.Generic;
using cngraphi.gassets.common;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 依赖加载器
    /// <para>负责管理多个AssetBundle文件加载器的操作，其本身不处理任何实际资源的加载。</para>
    /// <para>作者：强辰</para>
    /// </summary>
    public class GDependLoader : GLoader
    {
        #region 静态

        //依赖加载器缓存对象
        static public readonly Dictionary<string, GDependLoader> m_cache = new Dictionary<string, GDependLoader>();

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public GDependLoader Load(string path)
        {
            if (!m_cache.TryGetValue(path, out GDependLoader loa))
            {
                loa = new GDependLoader { Path = path };
                m_cache.Add(path, loa);
            }

            loa.Load();
            return loa;
        }

        #endregion



        //包含自身及自身依赖的ab文件加载器缓存
        private readonly List<GBundleLoader> m_bundles = new List<GBundleLoader>();
        //自身ab加载器
        private GBundleLoader m_mainbundle;

        /// <summary>
        /// 主ab文件内存映射
        /// </summary>
        public AssetBundle AssetBundle => m_mainbundle?.AssetBundle;


        protected override void OnLoad()
        {
            //查找资源所在的ab文件信息及其依赖ab文件信息
            //这里的Path是指资源路径（以Assets为开头的相对路径），并不是所在的ab文件路径
            string abname = GAssetManifest.GetABNameByResName(System.IO.Path.GetFileName(Path));
            ABInfo ab = GAssetManifest.GetABInfo(abname);
            if (ab == null)
            {
                Finish("加载资源失败！未找到资源所在的AssetBundle文件信息. 资源 = " + Path);
                return;
            }

            //依赖信息
            ABInfo[] infos = GAssetManifest.GetABDependsInfoFast(abname);

            //加载
            m_mainbundle = GBundleLoader.LoadInternal(ab);
            m_bundles.Add(m_mainbundle);

            if (infos == null) { return; }
            for (int i = 0; i < infos.Length; i++)
                m_bundles.Add(GBundleLoader.LoadInternal(infos[i]));
        }


        public override void Immediate()
        {
            if (IsDone) { return; }
            foreach (var loa in m_bundles)
            {
                loa.Immediate();
            }
        }


        protected override void OnUnload()
        {
            if (m_bundles.Count > 0)
            {
                foreach (var loa in m_bundles)
                {
                    if (string.IsNullOrEmpty(loa.Error))
                        loa.Dispose();
                }
                m_bundles.Clear();
            }
            m_mainbundle = null;
            m_cache.Remove(Path);
        }


        protected override void OnUpdate()
        {
            if (Status != GLoaderStatus.Loading) { return; }

            var totalProgress = 0f;
            var allDone = true;
            foreach (var bundle in m_bundles)
            {
                totalProgress += bundle.Progress;
                if (!string.IsNullOrEmpty(bundle.Error))
                {//只要有一个ab文件加载错误，就return，并将状态设置为失败
                    Status = GLoaderStatus.Fail;
                    Error = bundle.Error;
                    Progress = 1;
                    return;
                }
                if (bundle.IsDone) continue;
                allDone = false;
                break;
            }

            Progress = totalProgress / m_bundles.Count * 0.5f; //这里*0.5f代表ab全部加载完毕，等待后续ab中的资源加载（分成两部分 = ab + ab中的资源，各占50%进度）
            if (!allDone) { return; }

            //都加载完毕之后，查看主ab的文件内存映射是否成功加载，如果不成功，视为加载失败
            if (AssetBundle == null)
            {
                Finish("主AssetBundle文件加载失败（null）。path = " + Path);
                return;
            }
            Finish();
        }
    }

}