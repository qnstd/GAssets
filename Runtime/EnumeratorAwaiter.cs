using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace cngraphi.gassets
{
    /// <summary>
    /// 异步操作调制器
    /// <para>作者：强辰</para>
    /// </summary>
    /// <typeparam name="T">类类型</typeparam>
    public class EnumeratorAwaiter<T> : INotifyCompletion
    {
        bool m_isDone;
        Exception m_exception;
        Action m_action;
        T m_result;


        #region 实现 INotifyCompletion 必要接口

        /// <summary>
        /// 是否执行完毕
        /// </summary>
        public bool IsCompleted { get { return m_isDone; } }


        /// <summary>
        /// 操作结果数据
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            if (!m_isDone) { return default; }

            if (m_exception != null)
                ExceptionDispatchInfo.Capture(m_exception).Throw(); //将堆栈信息抛出

            return m_result;
        }


        /// <summary>
        /// 实现INotifyCompletion接口方法
        /// </summary>
        /// <param name="continuation"></param>
        public void OnCompleted(Action continuation)
        {
            if (m_action == null && !m_isDone)
                m_action = continuation;
        }

        #endregion


        /// <summary>
        /// 异步处理完毕，并执行回调委托
        /// </summary>
        /// <param name="result">结果</param>
        /// <param name="e">异常</param>
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
