using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 本地 AssetBundle 加载器
    /// <para>作者：强辰</para>
    /// </summary>
    public class GBundleLoaderLocal : GBundleLoader
    {
        private AssetBundleCreateRequest m_req;

        protected override void OnLoad()
        {
            m_req = LoadAssetBundleAsync(Path);
        }

        public override void Immediate()
        {
            if (IsDone) { return; }
            OnLoaded(m_req.assetBundle);
            m_req = null;
        }

        protected override void OnUpdate()
        {
            if (Status != GLoaderStatus.Loading) { return; }
            Progress = m_req.progress;
            if (m_req.isDone)
            {
                OnLoaded(m_req.assetBundle);
            }
        }
    }

}