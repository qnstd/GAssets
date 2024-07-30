using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 异步实例化对象
    /// <para>作者：强辰</para>
    /// </summary>
    public class GInstanceOperate : GOperate
    {
        #region 静态

        static private readonly List<GInstanceOperate> m_objs = new List<GInstanceOperate>();

        /// <summary>
        /// 创建一个异步实例化对象
        /// </summary>
        /// <param name="resname">资源名称</param>
        /// <returns></returns>
        static public GInstanceOperate Create(string resname)
        {
            GInstanceOperate obj = new GInstanceOperate()
            {
                Resname = resname
            };
            obj.Start();
            return obj;
        }


        /// <summary>
        /// 刷新所有异步实例对象的操作
        /// </summary>
        static public void UpdateAllObjs()
        {
            for (int i = 0; i < m_objs.Count; i++)
            {
                var o = m_objs[i];
                if (GAssetManager.Ins.Busy) { return; }

                if (!o.IsDone || o.Result != null) continue;

                //如果IsDone操作完成，并且Result数据为空的情况下，会自动从缓存中移除并进行释放操作
                m_objs.RemoveAt(i);
                i--;
                o.Destory();
            }
        }


        /// <summary>
        /// 清理所有异步实例对象的操作
        /// </summary>
        static public void ClearAllObjs()
        {
            m_objs.Clear();
        }

        #endregion



        /// <summary>
        /// 资源加载器
        /// </summary>
        private GAssetLoader m_assetloader;

        /// <summary>
        /// 资源名称
        /// </summary>
        private string Resname { get; set; }

        /// <summary>
        /// 实例化对象
        /// </summary>
        public GameObject Result { get; private set; }


        protected override void Start()
        {
            base.Start();
            m_assetloader = GAssetLoader.LoadAsync(Resname, typeof(GameObject));
            m_objs.Add(this);
        }


        protected override void Update()
        {
            if (Status != GOperateStatus.Ing) { return; }

            if (m_assetloader == null)
            {
                Finish("资源加载器为空（null）. resname = " + Resname);
                return;
            }

            Progress = m_assetloader.Progress;
            if (!m_assetloader.IsDone) { return; }

            if (m_assetloader.Status == GLoaderStatus.Fail)
            {
                Finish(m_assetloader.Error);
                return;
            }

            if (m_assetloader.Asset == null)
            {
                Finish("资源加载失败。resname = " + Resname);
                return;
            }

            Result = GameObject.Instantiate(m_assetloader.Asset as GameObject);
            Finish();
        }


        /// <summary>
        /// 卸载、释放
        /// </summary>
        public override void Destory()
        {
            Param = null;
            if (!IsDone)
            {
                Finish("主动取消该异步实例操作。");
                return;
            }

            if (Status == GOperateStatus.Suc)
            {
                if (Result != null)
                {
                    //GameObject.DestroyImmediate(Result);
                    GameObject.Destroy(Result);
                    Result = null;
                }
            }

            if (m_assetloader == null) { return; }

            if (string.IsNullOrEmpty(m_assetloader.Error)) { m_assetloader.Dispose(); }
            m_assetloader = null;
        }


    }

}
