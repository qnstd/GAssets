using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ���� AssetBundle ������
    /// <para>���ߣ�ǿ��</para>
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