using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// �첽ʵ��������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GInstanceOperate : GOperate
    {
        #region ��̬

        static private readonly List<GInstanceOperate> m_objs = new List<GInstanceOperate>();

        /// <summary>
        /// ����һ���첽ʵ��������
        /// </summary>
        /// <param name="resname">��Դ����</param>
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
        /// ˢ�������첽ʵ������Ĳ���
        /// </summary>
        static public void UpdateAllObjs()
        {
            for (int i = 0; i < m_objs.Count; i++)
            {
                var o = m_objs[i];
                if (GAssetManager.Ins.Busy) { return; }

                if (!o.IsDone || o.Result != null) continue;

                //���IsDone������ɣ�����Result����Ϊ�յ�����£����Զ��ӻ������Ƴ��������ͷŲ���
                m_objs.RemoveAt(i);
                i--;
                o.Destory();
            }
        }


        /// <summary>
        /// ���������첽ʵ������Ĳ���
        /// </summary>
        static public void ClearAllObjs()
        {
            m_objs.Clear();
        }

        #endregion



        /// <summary>
        /// ��Դ������
        /// </summary>
        private GAssetLoader m_assetloader;

        /// <summary>
        /// ��Դ����
        /// </summary>
        private string Resname { get; set; }

        /// <summary>
        /// ʵ��������
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
                Finish("��Դ������Ϊ�գ�null��. resname = " + Resname);
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
                Finish("��Դ����ʧ�ܡ�resname = " + Resname);
                return;
            }

            Result = GameObject.Instantiate(m_assetloader.Asset as GameObject);
            Finish();
        }


        /// <summary>
        /// ж�ء��ͷ�
        /// </summary>
        public override void Destory()
        {
            Param = null;
            if (!IsDone)
            {
                Finish("����ȡ�����첽ʵ��������");
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
