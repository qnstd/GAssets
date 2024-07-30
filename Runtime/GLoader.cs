using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ����������
    /// <para>����ά���������ļ��ء�ж�ء�ˢ�¡�״̬�����ü����Ȼ����������������˾�̬�������м������������ڼ��ػ�δʹ�õĻ��洦��ˢ�¼��ȣ�</para>
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GLoader
    {
        #region ��̬

        /// <summary>
        /// ���ڼ���״̬�ļ���������
        /// </summary>
        static public readonly List<GLoader> m_loading = new List<GLoader>();

        /// <summary>
        /// δʹ�õļ���������
        /// </summary>
        static public readonly List<GLoader> m_unused = new List<GLoader>();


        /// <summary>
        /// ִ�в��������м�����
        /// </summary>
        static public void UpdateAll()
        {
            //����
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
            //  ������ .unity �ļ����͵ļ��أ�������Ҫ�жϳ����Ƿ����ڼ��ػ�������ж��.
            //  ������������״̬������Ҫ�ڴ˴�ִ�� return ����.
            //
            //
            //END
            //////////////////////////////////////////////////////////////////////

            // δʹ�õļ�������ж��
            for (int index = 0, max = m_unused.Count; index < max; index++)
            {
                var loa = m_unused[index];
                if (GAssetManager.Ins.Busy) break;

                if (!loa.IsDone) continue; //����ok || fail || unloaded״̬������

                m_unused.RemoveAt(index);
                index--;
                max--;
                if (!loa.IsUnused) continue;//�������ã����ء�

                loa.Unload(); //ж��
            }
        }



        /// <summary>
        /// �������
        /// <para>���м��������������Լ��ص�����AssetBundle�ļ������ͷ�</para>
        /// </summary>
        static public void ClearAll()
        {
            GAssetLoader.m_cache.Clear();
            GBundleLoader.m_cache.Clear();
            GDependLoader.m_cache.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
        }



        /// <summary>
        /// ͨ�� AssetBundle �ļ�����ȡ�����
        /// </summary>
        /// <param name="abname">ab�ļ�����������׺��</param>
        /// <returns></returns>
        static public AssetBundle FindABundleByName(string abname)
        {
            /*
                GDependLoader / GAssetLoader �������е�key����Դ·����������AB�ļ�����
                ���ԣ�ֻ��Ҫ��GBundleLoader�в�ѯ���ɡ�
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
        /// ��ǰ������
        /// </summary>
        public int Ref { get; private set; } = 0;

        /// <summary>
        /// �Ƿ���δʹ��
        /// <para>true��δʹ�ã�false����������</para>
        /// </summary>
        public bool IsUnused { get { return Ref == 0; } }

        /// <summary>
        /// ���ü��� +1
        /// </summary>
        public void AddRef() { Ref++; }

        /// <summary>
        /// ���ü��� -1
        /// </summary>
        public void SubRef()
        {
            Ref--;
            if (Ref < 0) { Ref = 0; }
        }


        /// <summary>
        /// ˢ��
        /// </summary>
        private void Update() { OnUpdate(); }


        /// <summary>
        /// ���������ز�����ϵĴ���
        /// </summary>
        private void LoadingDone()
        {
            if (Status == GLoaderStatus.Fail)
            {
                Debug.LogError($"����ʧ��. path = {Path} / error = {Error}");
                Dispose();
            }
            OnComplete();
        }


        /// <summary>
        /// ���ü��ز������״̬
        /// <para>�������ý���=1������״̬��ֻ�гɹ���ʧ������״̬��</para>
        /// <para>����ֱ�ӵ��ã�������д�����ü������������</para>
        /// </summary>
        /// <param name="err"></param>
        protected void Finish(string err = null)
        {
            Error = err;
            Status = string.IsNullOrEmpty(Error) ? GLoaderStatus.Suc : GLoaderStatus.Fail;
            Progress = 1;
        }


        /// <summary>
        /// ����
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
        /// ж��
        /// </summary>
        protected void Unload()
        {
            if (Status == GLoaderStatus.Unloaded) return;

            Debug.Log($"ж����Դ. path = {Path}{(string.IsNullOrEmpty(Error) ? "" : $" / error = {Error}")}");

            OnUnload();
            Status = GLoaderStatus.Unloaded;
        }


        /// <summary>
        /// �Ƿ�������
        /// <param>���سɹ���ʧ�ܻ�ж��״̬֮һ������Ϊ�������</param>
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
        /// ��Դ��ַ
        /// </summary>
        public string Path { get; protected set; }


        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string Error { get; internal set; }


        /// <summary>
        /// ����״̬
        /// </summary>
        public GLoaderStatus Status { get; protected set; } = GLoaderStatus.Wait;


        /// <summary>
        /// ���ؽ���ֵ
        /// </summary>
        public float Progress { get; protected set; }


        /// <summary>
        /// ������Ӧ���ز������
        /// <para>��������д�������޷�ֱ�ӵ���</para>
        /// </summary>
        public virtual void Immediate()
        {
            throw new System.Exception("��������д�������޷�ֱ�ӵ���");
        }


        /// <summary>
        /// �ͷ�
        /// <para>�������ü�������Ϊ0������뵽δʹ�û������У�</para>
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



        #region ������д

        /// <summary>
        /// ����״̬�µ�ʵʱˢ��
        /// </summary>
        protected virtual void OnUpdate() { }
        /// <summary>
        /// ׼����ʼ����ʱ�Ĳ���
        /// </summary>
        protected virtual void OnLoad() { }
        /// <summary>
        /// ���������ڱ�ж��ʱ�Ĳ���
        /// </summary>
        protected virtual void OnUnload() { }
        /// <summary>
        /// ��������ִ�еļ����������ز������ʱ����Ӧ
        /// <para>1. ���ܼ��ش�������ʲô���ɹ���ʧ�ܡ�ж�صȣ����ᴥ������Ӧ��</para>
        /// <para>2. ������ز������ʱ����״̬Ϊʧ�ܣ���ô��ǰ���������ڵײ����Dispose���������ͷţ�</para>
        /// <para>3. ����������ʵ�ִ˺���ʱ�������ⲿ�ص�ί�н����ϲ��߼�������</para>
        /// </summary>
        protected virtual void OnComplete() { }
        /// <summary>
        /// ���ü���Ϊ0�������뵽δʹ�û������Ĳ���
        /// </summary>
        protected virtual void OnUnused() { }

        #endregion
    }

}