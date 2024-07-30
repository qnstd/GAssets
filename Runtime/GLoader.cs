using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 加载器基类
    /// <para>负责维护加载器的加载、卸载、刷新、状态、引用计数等基础操作，并包含了静态处理（所有加载器处于正在加载或未使用的缓存处理、刷新检测等）</para>
    /// <para>作者：强辰</para>
    /// </summary>
    public class GLoader
    {
        #region 静态

        /// <summary>
        /// 处于加载状态的加载器缓存
        /// </summary>
        static public readonly List<GLoader> m_loading = new List<GLoader>();

        /// <summary>
        /// 未使用的加载器缓存
        /// </summary>
        static public readonly List<GLoader> m_unused = new List<GLoader>();


        /// <summary>
        /// 执行并更新所有加载器
        /// </summary>
        static public void UpdateAll()
        {
            //加载
            for (var index = 0; index < m_loading.Count; index++)
            {
                var loa = m_loading[index];
                if (GAssetManager.Ins.Busy) return;

                loa.Update();
                if (!loa.IsDone) continue;

                m_loading.RemoveAt(index);
                index--;
                loa.LoadingDone();
            }

            //////////////////////////////////////////////////////////////////////
            //TODO:
            //  若存在 .unity 文件类型的加载，这里需要判断场景是否正在加载或者正在卸载.
            //  若处于这两种状态，则需要在此处执行 return 处理.
            //
            //
            //END
            //////////////////////////////////////////////////////////////////////

            // 未使用的加载器做卸载
            for (int index = 0, max = m_unused.Count; index < max; index++)
            {
                var loa = m_unused[index];
                if (GAssetManager.Ins.Busy) break;

                if (!loa.IsDone) continue; //不是ok || fail || unloaded状态，返回

                m_unused.RemoveAt(index);
                index--;
                max--;
                if (!loa.IsUnused) continue;//还有引用，返回。

                loa.Unload(); //卸载
            }
        }



        /// <summary>
        /// 清理操作
        /// <para>所有加载器的清理并将以加载的所有AssetBundle文件进行释放</para>
        /// </summary>
        static public void ClearAll()
        {
            GAssetLoader.m_cache.Clear();
            GBundleLoader.m_cache.Clear();
            GDependLoader.m_cache.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
        }



        /// <summary>
        /// 通过 AssetBundle 文件名获取其对象
        /// </summary>
        /// <param name="abname">ab文件名（不带后缀）</param>
        /// <returns></returns>
        static public AssetBundle FindABundleByName(string abname)
        {
            /*
                GDependLoader / GAssetLoader 缓存组中的key是资源路径，并非是AB文件名。
                所以，只需要从GBundleLoader中查询即可。
             */
            string key = abname + ".ab";
            if (GBundleLoader.m_cache.ContainsKey(key))
            {
                return GBundleLoader.m_cache[key].AssetBundle;
            }
            return null;
        }


        #endregion


        /// <summary>
        /// 当前引用数
        /// </summary>
        public int Ref { get; private set; } = 0;

        /// <summary>
        /// 是否是未使用
        /// <para>true：未使用；false：存在引用</para>
        /// </summary>
        public bool IsUnused { get { return Ref == 0; } }

        /// <summary>
        /// 引用计数 +1
        /// </summary>
        public void AddRef() { Ref++; }

        /// <summary>
        /// 引用计数 -1
        /// </summary>
        public void SubRef()
        {
            Ref--;
            if (Ref < 0) { Ref = 0; }
        }


        /// <summary>
        /// 刷新
        /// </summary>
        private void Update() { OnUpdate(); }


        /// <summary>
        /// 加载器加载操作完毕的处理
        /// </summary>
        private void LoadingDone()
        {
            if (Status == GLoaderStatus.Fail)
            {
                Debug.LogError($"加载失败. path = {Path} / error = {Error}");
                Dispose();
            }
            OnComplete();
        }


        /// <summary>
        /// 设置加载操作完成状态
        /// <para>包含设置进度=1、加载状态（只有成功或失败两种状态）</para>
        /// <para>子类直接调用，无需重写，设置加载器操作结果</para>
        /// </summary>
        /// <param name="err"></param>
        protected void Finish(string err = null)
        {
            Error = err;
            Status = string.IsNullOrEmpty(Error) ? GLoaderStatus.Suc : GLoaderStatus.Fail;
            Progress = 1;
        }


        /// <summary>
        /// 加载
        /// </summary>
        protected void Load()
        {
            if (Status != GLoaderStatus.Wait && IsUnused)
                m_unused.Remove(this);

            AddRef();
            m_loading.Add(this);

            if (Status != GLoaderStatus.Wait) { return; }
            Status = GLoaderStatus.Loading;
            Progress = 0;
            OnLoad();
        }


        /// <summary>
        /// 卸载
        /// </summary>
        protected void Unload()
        {
            if (Status == GLoaderStatus.Unloaded) return;

            Debug.Log($"卸载资源. path = {Path}{(string.IsNullOrEmpty(Error) ? "" : $" / error = {Error}")}");

            OnUnload();
            Status = GLoaderStatus.Unloaded;
        }


        /// <summary>
        /// 是否操作完毕
        /// <param>加载成功、失败或卸载状态之一，即视为操作完毕</param>
        /// </summary>
        public bool IsDone
        {
            get
            {
                return Status == GLoaderStatus.Suc ||
                        Status == GLoaderStatus.Unloaded ||
                        Status == GLoaderStatus.Fail;
            }
        }


        /// <summary>
        /// 资源地址
        /// </summary>
        public string Path { get; protected set; }


        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; internal set; }


        /// <summary>
        /// 加载状态
        /// </summary>
        public GLoaderStatus Status { get; protected set; } = GLoaderStatus.Wait;


        /// <summary>
        /// 加载进度值
        /// </summary>
        public float Progress { get; protected set; }


        /// <summary>
        /// 立刻响应加载操作完成
        /// <para>需子类重写，基类无法直接调用</para>
        /// </summary>
        public virtual void Immediate()
        {
            throw new System.Exception("子类需重写，基类无法直接调用");
        }


        /// <summary>
        /// 释放
        /// <para>更新引用计数（若为0，则加入到未使用缓存组中）</para>
        /// </summary>
        public void Dispose()
        {
            if (Ref <= 0)
            {
                return;
            }
            SubRef();
            if (!IsUnused) { return; }

            m_unused.Add(this);
            OnUnused();
        }



        #region 子类重写

        /// <summary>
        /// 加载状态下的实时刷新
        /// </summary>
        protected virtual void OnUpdate() { }
        /// <summary>
        /// 准备开始加载时的操作
        /// </summary>
        protected virtual void OnLoad() { }
        /// <summary>
        /// 加载器正在被卸载时的操作
        /// </summary>
        protected virtual void OnUnload() { }
        /// <summary>
        /// 对于正在执行的加载器，加载操作完毕时的响应
        /// <para>1. 不管加载处理结果是什么（成功、失败、卸载等）都会触发此响应；</para>
        /// <para>2. 如果加载操作完毕时加载状态为失败，那么当前加载器会在底层调用Dispose进行引用释放；</para>
        /// <para>3. 可以在子类实现此函数时，增加外部回调委托进行上层逻辑操作；</para>
        /// </summary>
        protected virtual void OnComplete() { }
        /// <summary>
        /// 引用计数为0，被加入到未使用缓存组后的操作
        /// </summary>
        protected virtual void OnUnused() { }

        #endregion
    }

}