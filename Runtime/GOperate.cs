using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// 自动分帧操作类基类
    /// <para>作者：强辰</para>
    /// </summary>
    public class GOperate : IEnumerator
    {

        #region 接口实现
        public object Current => null;

        public bool MoveNext()
        {
            return !IsDone; //当IsDone为false时，说明操作正在执行。同时也说明迭代器下，还存在正在执行的操作。
        }

        public void Reset() { }
        #endregion


        #region 静态
        static private readonly List<GOperate> m_runs = new List<GOperate>();

        static private void Running(GOperate op)
        {
            m_runs.Add(op);
        }

        /// <summary>
        /// 刷新并执行所有的操作
        /// </summary>
        static public void UpdateAll()
        {
            for (int i = 0; i < m_runs.Count; i++)
            {
                GOperate op = m_runs[i];
                if (GAssetManager.Ins.Busy) { return; }

                //执行
                op.Update();
                if (!op.IsDone) { continue; }

                //执行完毕后移除
                m_runs.RemoveAt(i);
                i--;
                if (op.Status == GOperateStatus.Fail)
                {
                    Debug.LogWarning($"任务操作警告. type = {op.GetType().Name} / message = {op.Error}");
                }

                //执行操作完成的委托回调
                op.Complete();
            }

            //检测异步实例对象是否有操作完成但操作结果集为空的，如果有将进行释放。
            GInstanceOperate.UpdateAllObjs();
        }



        /// <summary>
        /// 清理所有的操作
        /// </summary>
        static public void ClearAll()
        {
            m_runs.Clear();
            GInstanceOperate.ClearAllObjs();
            GSceneOperate.ClearAllObjs();
        }
        #endregion




        /// <summary>
        /// 操作完成时回调
        /// <para>包含操作成功、失败</para>
        /// </summary>
        public Action<GOperate> Completed;
        /// <summary>
        /// 操作状态
        /// </summary>
        public GOperateStatus Status { get; protected set; } = GOperateStatus.Wait;
        /// <summary>
        /// 操作进度
        /// </summary>
        public float Progress { get; protected set; }
        /// <summary>
        /// 是否操作完成
        /// </summary>
        public bool IsDone { get { return Status == GOperateStatus.Fail || Status == GOperateStatus.Suc; } }
        /// <summary>
        /// 操作失败时的错误信息
        /// </summary>
        public string Error { get; protected set; }
        /// <summary>
        /// 绑定的参数
        /// </summary>
        public object Param { get; set; } = null;



        /// <summary>
        /// 刷新（正在操作的逻辑应写在此处）
        /// <para>子类实现</para>
        /// </summary>
        protected virtual void Update() { }

        /// <summary>
        /// 销毁、释放
        /// <para>子类实现</para>
        /// </summary>
        public virtual void Destory() { }

        /// <summary>
        /// 启动操作
        /// <para>子类可继续扩展</para>
        /// </summary>
        protected virtual void Start()
        {
            Status = GOperateStatus.Ing;
            Running(this);
        }

        /// <summary>
        /// 操作完成的处理
        /// </summary>
        /// <param name="errmsg"></param>
        protected virtual void Finish(string errmsg = null)
        {
            Error = errmsg;
            Status = string.IsNullOrEmpty(Error) ? GOperateStatus.Suc : GOperateStatus.Fail;
            Progress = 1;
        }

        /// <summary>
        /// 执行操作完成的委托回调
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