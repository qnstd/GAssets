using System;
using System.Threading;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// �߳������ĵ���
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class SyncContextUtil
    {
        //Ԫ��ǣ��ڼ��س���֮ǰ�Զ���ʼ��
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static private void Initialize()
        {
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }


        /// <summary>
        /// ��ǰ�߳�ID
        /// </summary>
        static public int UnityThreadId
        {
            get; private set;
        }


        /// <summary>
        /// ��ǰִ�г����������
        /// </summary>
        static public SynchronizationContext UnitySynchronizationContext
        {
            get; private set;
        }


        /// <summary>
        /// ��unity�߳�������������
        /// </summary>
        /// <param name="action">ί��</param>
        static public void RunOnUnityScheduler(Action action)
        {
            if (SynchronizationContext.Current == UnitySynchronizationContext)
            {//��ǰ�̵߳��ã�ͬ��ִ��
                action();
            }
            else
            {//�ǵ�ǰ�̵߳��ã��첽POSTִ��
                UnitySynchronizationContext.Post(_ => action(), null);
            }
        }
    }

}
