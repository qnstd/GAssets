using System.Collections.Generic;
using cngraphi.gassets.common;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ����������
    /// <para>���������AssetBundle�ļ��������Ĳ������䱾�������κ�ʵ����Դ�ļ��ء�</para>
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GDependLoader : GLoader
    {
        #region ��̬

        //�����������������
        static public readonly Dictionary<string, GDependLoader> m_cache = new Dictionary<string, GDependLoader>();

        /// <summary>
        /// ����
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



        //������������������ab�ļ�����������
        private readonly List<GBundleLoader> m_bundles = new List<GBundleLoader>();
        //����ab������
        private GBundleLoader m_mainbundle;

        /// <summary>
        /// ��ab�ļ��ڴ�ӳ��
        /// </summary>
        public AssetBundle AssetBundle => m_mainbundle?.AssetBundle;


        protected override void OnLoad()
        {
            //������Դ���ڵ�ab�ļ���Ϣ��������ab�ļ���Ϣ
            //�����Path��ָ��Դ·������AssetsΪ��ͷ�����·���������������ڵ�ab�ļ�·��
            string abname = GAssetManifest.GetABNameByResName(System.IO.Path.GetFileName(Path));
            ABInfo ab = GAssetManifest.GetABInfo(abname);
            if (ab == null)
            {
                Finish("������Դʧ�ܣ�δ�ҵ���Դ���ڵ�AssetBundle�ļ���Ϣ. ��Դ = " + Path);
                return;
            }

            //������Ϣ
            ABInfo[] infos = GAssetManifest.GetABDependsInfoFast(abname);

            //����
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
                {//ֻҪ��һ��ab�ļ����ش��󣬾�return������״̬����Ϊʧ��
                    Status = GLoaderStatus.Fail;
                    Error = bundle.Error;
                    Progress = 1;
                    return;
                }
                if (bundle.IsDone) continue;
                allDone = false;
                break;
            }

            Progress = totalProgress / m_bundles.Count * 0.5f; //����*0.5f����abȫ��������ϣ��ȴ�����ab�е���Դ���أ��ֳ������� = ab + ab�е���Դ����ռ50%���ȣ�
            if (!allDone) { return; }

            //���������֮�󣬲鿴��ab���ļ��ڴ�ӳ���Ƿ�ɹ����أ�������ɹ�����Ϊ����ʧ��
            if (AssetBundle == null)
            {
                Finish("��AssetBundle�ļ�����ʧ�ܣ�null����path = " + Path);
                return;
            }
            Finish();
        }
    }

}