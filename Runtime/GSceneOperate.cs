using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace cngraphi.gassets
{
    /// <summary>
    /// 场景加载器
    /// <para>作者：强辰</para>
    /// </summary>
    public class GSceneOperate : GOperate
    {
        #region 静态
        static private readonly List<GSceneOperate> m_objs = new List<GSceneOperate>();

        /// <summary>
        /// 创建场景加载器
        /// </summary>
        /// <param name="scenename">场景名称（不带文件名后缀）</param>
        /// <param name="mode">场景加载模式</param>
        /// <returns></returns>
        static public GSceneOperate Create(string scenename, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (Get(scenename) != null)
            {
                Debug.LogWarning($"正在加载或已存在场景. {scenename}");
                return null;
            }

            string rname = (scenename + ".unity").ToLower();
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
            GSceneOperate obj = new GSceneOperate()
            {
                SceneName = scenename,
                Mode = mode,
                Path = path
            };
            obj.Start();
            return obj;
        }



        /// <summary>
        /// 获取当前缓存的场景加载器
        /// </summary>
        /// <param name="scenename">场景名称</param>
        /// <returns></returns>
        static public GSceneOperate Get(string scenename)
        {
            if (string.IsNullOrEmpty(scenename)) { return null; }
            if (m_objs == null || m_objs.Count == 0) { return null; }
            foreach (var loader in m_objs)
            {
                if (loader != null && loader.SceneName == scenename)
                {
                    return loader;
                }
            }
            return null;
        }


        /// <summary>
        /// 内部管理器调用
        /// </summary>
        static public void ClearAllObjs()
        {
            if (m_objs != null)
            {
                m_objs.Clear();
            }
        }
        #endregion



        #region 属性
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; private set; } = null;
        /// <summary>
        /// 场景资源路径（以Assets为开头）
        /// </summary>
        public string Path { get; private set; } = null;
        /// <summary>
        /// 场景加载的模式
        /// </summary>
        public LoadSceneMode Mode { get; private set; } = LoadSceneMode.Additive;
        /// <summary>
        /// 场景资源、依赖的加载状态
        /// </summary>
        public GLoaderStatus LoadStatus { get; private set; } = GLoaderStatus.Wait;
        /// <summary>
        /// 是否是子场景
        /// </summary>
        public bool SubScene { get; private set; } = false;
        /// <summary>
        /// 卸载场景完毕时的回调
        /// <para>若加载场景时，并没有完全加载或处于不可用无效状态，则不会触发此回调，而是触发 Completed 回调</para>
        /// </summary>
        public Action<GSceneOperate> DestoryCompleted;
        #endregion



        AsyncOperation m_scene = null;
        GDependLoader m_dependLoader = null;
        protected override void Start()
        {
            base.Start();
            m_dependLoader = GDependLoader.Load(Path);
            LoadStatus = GLoaderStatus.DependsLoading;
            SubScene = (Mode == LoadSceneMode.Additive);
            m_objs.Add(this);
        }



        protected override void Update()
        {
            if (Status != GOperateStatus.Ing) { return; }

            switch (LoadStatus)
            {
                case GLoaderStatus.DependsLoading:
                    DependLoading();
                    break;
                case GLoaderStatus.Loading:
                    ScnLoading();
                    break;
            }
        }


        private void DependLoading()
        {
            if (m_dependLoader == null)
            {
                Finish($"当前场景的依赖加载器为空. SceneName = {SceneName}");
                return;
            }

            Progress = 0.5f * m_dependLoader.Progress;
            if (!m_dependLoader.IsDone) { return; }

            if (m_dependLoader.Status == GLoaderStatus.Fail)
            {
                Finish($"当前场景的依赖加载失败. Reason = {m_dependLoader.Error} / SceneName = {SceneName}");
                return;
            }

            var ab = m_dependLoader.AssetBundle;
            if (ab == null)
            {
                Finish($"当前加载的场景所在的 AssetBundle 为空. SceneName = {SceneName}");
                return;
            }

            // 场景ab及ab依赖加载完毕，进行场景加载
            m_scene = SceneManager.LoadSceneAsync(SceneName, Mode);
            m_scene.allowSceneActivation = false;
            LoadStatus = GLoaderStatus.Loading;
        }


        private void ScnLoading()
        {
            if (m_scene == null)
            {
                Finish($"负责加载场景的加载器为空. SceneName = {SceneName}");
                return;
            }

            // 这里进度累加是因为加载场景前需要加载所有的依赖ab，加载ab占总进度的50%，而加载场景占剩余的50%。
            Progress += m_scene.progress * 0.5f;
            if (Progress >= 0.95f)
            {
                // 场景加载完毕
                Finish();

                // 切换并启动场景
                m_scene.allowSceneActivation = true;
            }
        }



        private void DestoryComplete()
        {
            if (DestoryCompleted == null) { return; }
            var c = DestoryCompleted;
            DestoryCompleted.Invoke(this);
            DestoryCompleted -= c;
        }



        /// <summary>
        /// 卸载并释放场景及相关的依赖资源
        /// </summary>
        public override void Destory()
        {
            if (!IsDone)
            {
                Finish($"主动取消场景 {SceneName} 的加载.");
            }


            // 卸载依赖
            if (m_dependLoader != null)
            {
                m_dependLoader.Dispose();
                m_dependLoader = null;
            }
            Param = null;
            m_scene = null;

            // 卸载引用
            int index = m_objs.IndexOf(this);
            if (index != -1)
            {
                m_objs.RemoveAt(index);
            }

            // 卸载场景
            Scene scn = SceneManager.GetSceneByName(SceneName);
            if (scn != null && scn.isLoaded && scn.IsValid())
            {
                AsyncOperation ao = SceneManager.UnloadSceneAsync(scn);
                ao.completed += (asyncoperate) =>
                {
                    DestoryComplete();
                };
            }
        }

    }
}