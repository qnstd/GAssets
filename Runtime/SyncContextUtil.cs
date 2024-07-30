using System;
using System.Threading;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 线程上下文调用
    /// <para>作者：强辰</para>
    /// </summary>
    public class SyncContextUtil
    {
        //元标记，在加载场景之前自动初始化
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static private void Initialize()
        {
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }


        /// <summary>
        /// 当前线程ID
        /// </summary>
        static public int UnityThreadId
        {
            get; private set;
        }


        /// <summary>
        /// 当前执行程序的上下文
        /// </summary>
        static public SynchronizationContext UnitySynchronizationContext
        {
            get; private set;
        }


        /// <summary>
        /// 在unity线程上下文中运行
        /// </summary>
        /// <param name="action">委托</param>
        static public void RunOnUnityScheduler(Action action)
        {
            if (SynchronizationContext.Current == UnitySynchronizationContext)
            {//当前线程调用，同步执行
                action();
            }
            else
            {//非当前线程调用，异步POST执行
                UnitySynchronizationContext.Post(_ => action(), null);
            }
        }
    }

}
