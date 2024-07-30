using System;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// �첽���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GAsyncCom
    {
        #region ��̬
        static private readonly List<GAsyncCom> m_pool = new List<GAsyncCom>();
        static private readonly List<GAsyncCom> m_use = new List<GAsyncCom>();


        /// <summary>
        /// ִ��һ���첽����
        /// </summary>
        /// <param name="finish">�������ʱ��ί��</param>
        /// <param name="update">��������ί��</param>
        /// <param name="done">���ί�з��ص���true����˵���첽������ϣ�ִ�в���finishί�У����򣬳���ִ�в���updateί�С���������Ϊnull��Ĭ�����÷���ֵΪtrue��ί�к�����</param>
        /// <returns></returns>
        static public GAsyncCom Excute(Action finish, Action update = null, Func<bool> done = null)
        {
            GAsyncCom com = null;
            if (m_pool.Count <= 0)
            {//�����в����ڴ�ʹ�õĶ���
                com = new GAsyncCom();
            }
            else
            {//�����д��ڴ�ʹ�ö���ȡ����һ��
                com = m_pool[0];
                m_pool.RemoveAt(0);
            }

            com.m_isDone = done ?? (() => true);
            com.m_complete = finish;
            com.m_update = update;
            com.Run();

            return com;
        }


        /// <summary>
        /// ˢ�������첽����ִ����
        /// </summary>
        static public void UpdateAll()
        {
            for (int i = 0; i < m_use.Count; i++)
            {
                GAsyncCom c = m_use[i];
                try
                {
                    if (c.Update()) continue;
                    Remove(c, ref i);

                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    Remove(c, ref i);
                }
                if (GAssetManager.Ins.Busy) return;
            }
        }


        static private void Remove(GAsyncCom com, ref int index)
        {
            com.m_running = false;
            m_use.RemoveAt(index);
            com.Stop();
            m_pool.Add(com);
            index--;
        }


        /// <summary>
        /// ж���������ڲ������첽�������������ʹ�õ��첽������
        /// </summary>
        static public void ClearAll()
        {
            m_use.Clear();
            m_pool.Clear();
        }
        #endregion


        private Action m_complete;
        private bool m_running;
        private Action m_update;
        private Func<bool> m_isDone;

        private bool Update()
        {
            if (!m_running) { return false; }
            m_update?.Invoke();
            if (m_isDone == null || !m_isDone()) { return true; }
            m_complete?.Invoke();
            return false;
        }

        private void Run()
        {
            if (m_running) { return; }
            m_use.Add(this);
            m_running = true;
        }

        /// <summary>
        /// ֹͣ�첽����
        /// </summary>
        public void Stop()
        {
            m_complete = null;
            m_update = null;
            m_isDone = null;
            m_running = false;
        }

    }
}