using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace cngraphi.gassets
{
    /// <summary>
    /// �첽����������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    /// <typeparam name="T">������</typeparam>
    public class EnumeratorAwaiter<T> : INotifyCompletion
    {
        bool m_isDone;
        Exception m_exception;
        Action m_action;
        T m_result;


        #region ʵ�� INotifyCompletion ��Ҫ�ӿ�

        /// <summary>
        /// �Ƿ�ִ�����
        /// </summary>
        public bool IsCompleted { get { return m_isDone; } }


        /// <summary>
        /// �����������
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            if (!m_isDone) { return default; }

            if (m_exception != null)
                ExceptionDispatchInfo.Capture(m_exception).Throw(); //����ջ��Ϣ�׳�

            return m_result;
        }


        /// <summary>
        /// ʵ��INotifyCompletion�ӿڷ���
        /// </summary>
        /// <param name="continuation"></param>
        public void OnCompleted(Action continuation)
        {
            if (m_action == null && !m_isDone)
                m_action = continuation;
        }

        #endregion


        /// <summary>
        /// �첽������ϣ���ִ�лص�ί��
        /// </summary>
        /// <param name="result">���</param>
        /// <param name="e">�쳣</param>
        public void Complete(T result, Exception e)
        {
            if (!m_isDone)
            {
                m_isDone = true;
                m_exception = e;
                m_result = result;

                if (m_action != null)
                {
                    SyncContextUtil.RunOnUnityScheduler(m_action);
                }
            }
        }

    }
}
