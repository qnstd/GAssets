using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 资源加载器
    /// <para>作者：强辰</para>
    /// </summary>
    public class GAssetLoader : GLoader, IEnumerator
    {

        #region 静态

        /// <summary>
        /// 加载器缓存
        /// </summary>
        static public readonly Dictionary<string, GAssetLoader> m_cache = new Dictionary<string, GAssetLoader>();

        /// <summary>
        /// 创建ab包内资源加载的加载器对象
        /// </summary>
        static public Func<string, Type, GAssetLoader> Creator { get; set; } = GBundleAssetLoader.Create;

        /// <summary>
        /// 创建 GAssetLoader 实例对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static private GAssetLoader CreateInstance(string path, Type type)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException(nameof(path));
            return Creator(path, type);
        }


        static private GAssetLoader LoadInternal(string resname, Type type, Action<GAssetLoader> com = null)
        {
            //根据资源名称，判断所在的AssetBundle文件是否存在
            string rname = resname.ToLower();
            string abname = GAssetManifest.GetABNameByResName(rname);
            if (abname == null)
            {
                Debug.LogError($"要加载资源所在的AssetBundle文件，在当前版本Manifest中不存在. resname = {rname}");
                return null;
            }
            if (!GAssetManager.Ins.ExistAssetBundleOnLocal(abname))
            {
                Debug.LogError($"要加载资源所在的AssetBundle文件，在本地资源缓存目录中未找到. resname = {rname} / abname = {abname}");
                return null;
            }

            string path = GAssetManifest.GetResPathInAB(rname); //资源路径，以Assets开头的相对路径
            if (!m_cache.TryGetValue(path, out var loa))
            {
                loa = CreateInstance(path, type);
                m_cache.Add(path, loa);
            }

            if (com != null) loa.Completed += com;

            loa.Load();
            return loa;
        }



        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="resname">资源名称</param>
        /// <param name="type">资源类型</param>
        /// <param name="com">回调委托</param>
        /// <returns></returns>
        static public GAssetLoader LoadAsync(string resname, Type type, Action<GAssetLoader> com = null)
        {
            return LoadInternal(resname, type, com);
        }


        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="resname">资源名称</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        static public GAssetLoader Load(string resname, Type type)
        {
            var assetloader = LoadInternal(resname, type);
            assetloader.Immediate();
            return assetloader;
        }

        /// <summary>
        /// 加载含有多个嵌套Object的复合Asset资源
        /// <para>
        /// 例如：加载嵌入了动画的FBX文件，或者加载嵌入了多个精灵的图集。如果要加载的Object都来自于同一Asset，
        /// 而他们又和很多不相关的其他Object存放在同一个AssetBundle中，那就应该使用此方法。
        /// </para>
        /// </summary>
        /// <param name="resname">资源名称</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        static public GAssetLoader LoadSubAssets(string resname, Type type)
        {
            var assetloader = LoadInternal(resname, type);
            if (assetloader == null)
                return null;
            assetloader.IsSubAssets = true;
            assetloader.Immediate();
            return assetloader;
        }


        /// <summary>
        /// 加载含有多个嵌套Object的复合Asset资源（异步）
        /// <para>
        /// 例如：加载嵌入了动画的FBX文件，或者加载嵌入了多个精灵的图集。如果要加载的Object都来自于同一Asset，
        /// 而他们又和很多不相关的其他Object存放在同一个AssetBundle中，那就应该使用此方法。
        /// </para>
        /// </summary>
        /// <param name="resname">资源名称</param>
        /// <param name="type">资源类型</param>
        /// <param name="com">委托回调</param>
        /// <returns></returns>
        static public GAssetLoader LoadSubAssetsAsync(string resname, Type type, Action<GAssetLoader> com = null)
        {
            var assetloader = LoadInternal(resname, type, com);
            if (assetloader == null)
                return null;
            assetloader.IsSubAssets = true;
            return assetloader;
        }


        /// <summary>
        /// 获取加载器
        /// </summary>
        /// <param name="key">加载器的索引（索引：资源文件名（带文件名后缀））</param>
        /// <returns></returns>
        static public GAssetLoader GetLoader(string key)
        {
            string respath = GAssetManifest.GetResPathInAB(key);
            GAssetLoader loa = null;
            foreach (string k in m_cache.Keys)
            {
                if (k == respath)
                {
                    loa = m_cache[k];
                    break;
                }
            }
            return loa;
        }


        /// <summary>
        /// 释放加载器
        /// <para>若加载器还有其他引用，则对引用计数-1. 若不存在任何引用，引用计数为0，则进行释放.</para>
        /// </summary>
        /// <param name="key">加载器的索引（索引：资源文件名（带文件名后缀））</param>
        static public void DisposeLoader(string key)
        {
            GAssetLoader loa = GetLoader(key);
            if (loa == null) { return; }
            loa.Dispose();
        }

        #endregion


        #region 实现 IEnumerator 接口

        public object Current => null;

        public bool MoveNext()
        {
            return !IsDone; //当操作完成时，返回false；如果加载操作未完成，则返回true。
        }

        public void Reset() { }

        #endregion


        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object Asset { get; protected set; }

        /// <summary>
        /// 嵌套复合型资源组
        /// </summary>
        public UnityEngine.Object[] SubAssets { get; protected set; }

        /// <summary>
        /// 操作完成的委托
        /// </summary>
        public Action<GAssetLoader> Completed;

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : UnityEngine.Object
        {
            return Asset as T;
        }

        /// <summary>
        /// 资源类型
        /// </summary>
        protected Type Typ { get; set; }

        /// <summary>
        /// 是否正在做嵌套复合Asset的加载
        /// </summary>
        protected bool IsSubAssets { get; set; }

        /// <summary>
        /// 加载完成的处理
        /// </summary>
        /// <param name="target"></param>
        protected void OnLoaded(UnityEngine.Object target)
        {
            Asset = target;
            Finish(Asset == null ? "资源加载失败，为空（null）" : null);
        }

        /// <summary>
        /// 操作完成时的调用
        /// </summary>
        protected override void OnComplete()
        {
            if (Completed == null) { return; }

            var c = Completed;
            Completed?.Invoke(this);
            Completed -= c;
        }


        /// <summary>
        /// 未使用后续操作
        /// </summary>
        protected override void OnUnused()
        {
            Completed = null;
        }

        /// <summary>
        /// 卸载时的操作
        /// </summary>
        protected override void OnUnload()
        {
            m_cache.Remove(Path);
        }

    }
}