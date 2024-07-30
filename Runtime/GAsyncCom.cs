using System;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 异步组件
    /// <para>作者：强辰</para>
    /// </summary>
    public class GAsyncCom
    {
        #region 静态
        static private readonly List<GAsyncCom> m_pool = new List<GAsyncCom>();
        static private readonly List<GAsyncCom> m_use = new List<GAsyncCom>();


        /// <summary>
        /// 执行一个异步操作
        /// </summary>
        /// <param name="finish">操作完成时的委托</param>
        /// <param name="update">操作过程委托</param>
        /// <param name="done">如果委托返回的是true，则说明异步操作完毕，执行参数finish委托；否则，持续执行参数update委托。若果参数为null，默认设置返回值为true的委托函数。</param>
        /// <returns></returns>
        static public GAsyncCom Excute(Action finish, Action update = null, Func<bool> done = null)
        {
            GAsyncCom com = null;
            if (m_pool.Count <= 0)
            {//缓存中不存在待使用的对象
                com = new GAsyncCom();
            }
            else
            {//缓存中存在待使用对象，取出第一个
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
        /// 刷新所有异步操作执行器
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
        /// 卸载所有正在操作的异步操作器及缓存待使用的异步操作器
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
        /// 停止异步操作
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