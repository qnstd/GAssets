using System;
using System.Collections.Generic;
using cngraphi.gassets.common;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ����AssetBundle�ļ����صļ���������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GBundleLoader : GLoader
    {
        #region ��̬

        //����bundle������
        static public readonly Dictionary<string, GBundleLoader> m_cache = new Dictionary<string, GBundleLoader>();

        /// <summary>
        /// �ڲ�����
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
                        Path = GAssetManager.Ins.GetResPath(bundle.m_name),//��ȡab�ļ�����·��
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
        /// ��ǰ���ص�AssetBundle�ڴ�ӳ�����
        /// </summary>
        public AssetBundle AssetBundle { get; protected set; }


        //�첽����AssetBundle����
        protected AssetBundleCreateRequest LoadAssetBundleAsync(string url)
        {
            return AssetBundle.LoadFromFileAsync(url);
        }

        //ͬ������AssetBundle����
        protected AssetBundle LoadAssetBundle(string url)
        {
            return AssetBundle.LoadFromFile(url);
        }

        //������ϵĴ���
        //�������
        protected void OnLoaded(AssetBundle bundle)
        {
            AssetBundle = bundle;
            Finish(AssetBundle == null ? "����AssetBundleΪ�գ�null��" : null);
        }

        //ж�ش���
        //ж��AssetBundle�ڴ�ӳ���Լ�ab���ڹ�������Դ
        protected override void OnUnload()
        {
            m_cache.Remove(m_info.m_name);
            if (AssetBundle == null) return;

            AssetBundle.Unload(true);
            AssetBundle = null;
        }
    }

}
