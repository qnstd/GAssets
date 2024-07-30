using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// �Զ���֡���������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GOperate : IEnumerator
    {

        #region �ӿ�ʵ��
        public object Current => null;

        public bool MoveNext()
        {
            return !IsDone; //��IsDoneΪfalseʱ��˵����������ִ�С�ͬʱҲ˵���������£�����������ִ�еĲ�����
        }

        public void Reset() { }
        #endregion


        #region ��̬
        static private readonly List<GOperate> m_runs = new List<GOperate>();

        static private void Running(GOperate op)
        {
            m_runs.Add(op);
        }

        /// <summary>
        /// ˢ�²�ִ�����еĲ���
        /// </summary>
        static public void UpdateAll()
        {
            for (int i = 0; i < m_runs.Count; i++)
            {
                GOperate op = m_runs[i];
                if (GAssetManager.Ins.Busy) { return; }

                //ִ��
                op.Update();
                if (!op.IsDone) { continue; }

                //ִ����Ϻ��Ƴ�
                m_runs.RemoveAt(i);
                i--;
                if (op.Status == GOperateStatus.Fail)
                {
                    Debug.LogWarning($"�����������. type = {op.GetType().Name} / message = {op.Error}");
                }

                //ִ�в�����ɵ�ί�лص�
                op.Complete();
            }

            //����첽ʵ�������Ƿ��в�����ɵ����������Ϊ�յģ�����н������ͷš�
            GInstanceOperate.UpdateAllObjs();
        }



        /// <summary>
        /// �������еĲ���
        /// </summary>
        static public void ClearAll()
        {
            m_runs.Clear();
            GInstanceOperate.ClearAllObjs();
            GSceneOperate.ClearAllObjs();
        }
        #endregion




        /// <summary>
        /// �������ʱ�ص�
        /// <para>���������ɹ���ʧ��</para>
        /// </summary>
        public Action<GOperate> Completed;
        /// <summary>
        /// ����״̬
        /// </summary>
        public GOperateStatus Status { get; protected set; } = GOperateStatus.Wait;
        /// <summary>
        /// ��������
        /// </summary>
        public float Progress { get; protected set; }
        /// <summary>
        /// �Ƿ�������
        /// </summary>
        public bool IsDone { get { return Status == GOperateStatus.Fail || Status == GOperateStatus.Suc; } }
        /// <summary>
        /// ����ʧ��ʱ�Ĵ�����Ϣ
        /// </summary>
        public string Error { get; protected set; }
        /// <summary>
        /// �󶨵Ĳ���
        /// </summary>
        public object Param { get; set; } = null;



        /// <summary>
        /// ˢ�£����ڲ������߼�Ӧд�ڴ˴���
        /// <para>����ʵ��</para>
        /// </summary>
        protected virtual void Update() { }

        /// <summary>
        /// ���١��ͷ�
        /// <para>����ʵ��</para>
        /// </summary>
        public virtual void Destory() { }

        /// <summary>
        /// ��������
        /// <para>����ɼ�����չ</para>
        /// </summary>
        protected virtual void Start()
        {
            Status = GOperateStatus.Ing;
            Running(this);
        }

        /// <summary>
        /// ������ɵĴ���
        /// </summary>
        /// <param name="errmsg"></param>
        protected virtual void Finish(string errmsg = null)
        {
            Error = errmsg;
            Status = string.IsNullOrEmpty(Error) ? GOperateStatus.Suc : GOperateStatus.Fail;
            Progress = 1;
        }

        /// <summary>
        /// ִ�в�����ɵ�ί�лص�
        /// </summary>
        private void Complete()
        {
            if (Completed == null) { return; }
            var c = Completed;
            Completed.Invoke(this);
            Completed -= c;
        }

    }
}