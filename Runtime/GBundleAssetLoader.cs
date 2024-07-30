using System;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ���� AssetBundle ����Դ�ļ�����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GBundleAssetLoader : GAssetLoader
    {
        private GDependLoader m_dependLoader;
        private AssetBundleRequest m_req;

        static internal GBundleAssetLoader Create(string path, Type type)
        {
            return new GBundleAssetLoader
            {
                Path = path, //��Դ·��
                Typ = type //����
            };
        }


        /// <summary>
        /// ��ʼ����׼��
        /// </summary>
        protected override void OnLoad()
        {
            m_dependLoader = GDependLoader.Load(Path);
            Status = GLoaderStatus.DependsLoading;
        }


        /// <summary>
        /// ж��
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
        /// ������ɼ��ز���
        /// </summary>
        public override void Immediate()
        {
            if (IsDone) { return; }

            if (m_dependLoader == null)
            {
                Finish("��ǰ�ڲ����ش����GDependLoader������Ϊnull��");
                return;
            }

            if (!m_dependLoader.IsDone) m_dependLoader.Immediate();

            if (m_dependLoader.AssetBundle == null)
            {
                Finish("��ǰ�ڲ����ش���� GDependLoader.AssetBundle ��ab�ļ��ڴ�ӳ��Ϊ��.");
                return;
            }

            if (IsSubAssets)
            {//���ظ�������Դ
                SubAssets = m_dependLoader.AssetBundle.LoadAssetWithSubAssets(Path, Typ);
                Finish();
            }
            else
            {//����ָ����Դ
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
        /// �����������Դ
        /// </summary>
        private void ResLoading()
        {
            if (m_req == null)
            {
                Finish("������Դ���ص�AssetBundleRequest����Ϊ�գ�null��");
                return;
            }

            Progress = 0.5f + m_req.progress * 0.5f;
            if (!m_req.isDone) { return; }

            if (IsSubAssets)
            {
                SubAssets = m_req.allAssets;
                Finish(SubAssets == null ? "��������Դ���غ�Ϊ��" : null);
            }
            else
            {
                OnLoaded(m_req.asset);
            }
        }


        /// <summary>
        /// ������Դ�����ab�ļ�������ab�ļ�
        /// </summary>
        private void DepLoading()
        {
            if (m_dependLoader == null)
            {
                Finish("��ǰ�ڲ����ش����GDependLoader������Ϊnull��");
                return;
            }

            Progress = 0.5f * m_dependLoader.Progress;
            if (!m_dependLoader.IsDone) { return; }

            var ab = m_dependLoader.AssetBundle;
            if (ab == null)
            {
                Finish("��ǰ�ڲ����ش���� GDependLoader.AssetBundle ��ab�ļ��ڴ�ӳ��Ϊ��.");
                return;
            }

            //����ab����������ϣ�������Դ����
            m_req = IsSubAssets ? ab.LoadAssetWithSubAssetsAsync(Path, Typ) : ab.LoadAssetAsync(Path, Typ);
            Status = GLoaderStatus.Loading;
        }
    }

}