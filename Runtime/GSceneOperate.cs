using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace cngraphi.gassets
{
    /// <summary>
    /// ����������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GSceneOperate : GOperate
    {
        #region ��̬
        static private readonly List<GSceneOperate> m_objs = new List<GSceneOperate>();

        /// <summary>
        /// ��������������
        /// </summary>
        /// <param name="scenename">�������ƣ������ļ�����׺��</param>
        /// <param name="mode">��������ģʽ</param>
        /// <returns></returns>
        static public GSceneOperate Create(string scenename, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (Get(scenename) != null)
            {
                Debug.LogWarning($"���ڼ��ػ��Ѵ��ڳ���. {scenename}");
                return null;
            }

            string rname = (scenename + ".unity").ToLower();
            string abname = GAssetManifest.GetABNameByResName(rname);
            if (abname == null)
            {
                Debug.LogError($"Ҫ������Դ���ڵ�AssetBundle�ļ����ڵ�ǰ�汾Manifest�в�����. resname = {rname}");
                return null;
            }
            if (!GAssetManager.Ins.ExistAssetBundleOnLocal(abname))
            {
                Debug.LogError($"Ҫ������Դ���ڵ�AssetBundle�ļ����ڱ�����Դ����Ŀ¼��δ�ҵ�. resname = {rname} / abname = {abname}");
                return null;
            }

            string path = GAssetManifest.GetResPathInAB(rname); //��Դ·������Assets��ͷ�����·��
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
        /// ��ȡ��ǰ����ĳ���������
        /// </summary>
        /// <param name="scenename">��������</param>
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
        /// �ڲ�����������
        /// </summary>
        static public void ClearAllObjs()
        {
            if (m_objs != null)
            {
                m_objs.Clear();
            }
        }
        #endregion



        #region ����
        /// <summary>
        /// ��������
        /// </summary>
        public string SceneName { get; private set; } = null;
        /// <summary>
        /// ������Դ·������AssetsΪ��ͷ��
        /// </summary>
        public string Path { get; private set; } = null;
        /// <summary>
        /// �������ص�ģʽ
        /// </summary>
        public LoadSceneMode Mode { get; private set; } = LoadSceneMode.Additive;
        /// <summary>
        /// ������Դ�������ļ���״̬
        /// </summary>
        public GLoaderStatus LoadStatus { get; private set; } = GLoaderStatus.Wait;
        /// <summary>
        /// �Ƿ����ӳ���
        /// </summary>
        public bool SubScene { get; private set; } = false;
        /// <summary>
        /// ж�س������ʱ�Ļص�
        /// <para>�����س���ʱ����û����ȫ���ػ��ڲ�������Ч״̬���򲻻ᴥ���˻ص������Ǵ��� Completed �ص�</para>
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
                Finish($"��ǰ����������������Ϊ��. SceneName = {SceneName}");
                return;
            }

            Progress = 0.5f * m_dependLoader.Progress;
            if (!m_dependLoader.IsDone) { return; }

            if (m_dependLoader.Status == GLoaderStatus.Fail)
            {
                Finish($"��ǰ��������������ʧ��. Reason = {m_dependLoader.Error} / SceneName = {SceneName}");
                return;
            }

            var ab = m_dependLoader.AssetBundle;
            if (ab == null)
            {
                Finish($"��ǰ���صĳ������ڵ� AssetBundle Ϊ��. SceneName = {SceneName}");
                return;
            }

            // ����ab��ab����������ϣ����г�������
            m_scene = SceneManager.LoadSceneAsync(SceneName, Mode);
            m_scene.allowSceneActivation = false;
            LoadStatus = GLoaderStatus.Loading;
        }


        private void ScnLoading()
        {
            if (m_scene == null)
            {
                Finish($"������س����ļ�����Ϊ��. SceneName = {SceneName}");
                return;
            }

            // ��������ۼ�����Ϊ���س���ǰ��Ҫ�������е�����ab������abռ�ܽ��ȵ�50%�������س���ռʣ���50%��
            Progress += m_scene.progress * 0.5f;
            if (Progress >= 0.95f)
            {
                // �����������
                Finish();

                // �л�����������
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
        /// ж�ز��ͷų�������ص�������Դ
        /// </summary>
        public override void Destory()
        {
            if (!IsDone)
            {
                Finish($"����ȡ������ {SceneName} �ļ���.");
            }


            // ж������
            if (m_dependLoader != null)
            {
                m_dependLoader.Dispose();
                m_dependLoader = null;
            }
            Param = null;
            m_scene = null;

            // ж������
            int index = m_objs.IndexOf(this);
            if (index != -1)
            {
                m_objs.RemoveAt(index);
            }

            // ж�س���
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