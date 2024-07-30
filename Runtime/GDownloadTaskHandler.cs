using System;
using System.Collections.Generic;

namespace cngraphi.gassets
{
    /// <summary>
    /// ��������ʱ�Ĳ������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GDownloadTaskHandler
    {
        public List<Action<double, ulong, ulong>> OnProgress { get; private set; } = new List<Action<double, ulong, ulong>>();
        public List<Action> OnFinish { get; private set; } = new List<Action>();
        public List<Action<string, string>> OnErr { get; private set; } = new List<Action<string, string>>();



        public void Add<T>(List<T> lst, T action)
        {
            if (action == null || lst == null) { return; };
            lst.Add(action);
        }


        public void Run(List<Action> lst)
        {
            if (lst == null) { return; }
            foreach (var action in lst)
            {
                action?.Invoke();
            }
        }


        public void Run(List<Action<double, ulong, ulong>> lst, double progress, ulong speed, ulong size)
        {
            if (lst == null) { return; }
            foreach (var action in lst)
            {
                action?.Invoke(progress, speed, size); // ���ؽ��ȣ������ٶȣ�������Դ���ܳߴ�
            }
        }


        public void Run(List<Action<string, string>> lst, string msg, string url)
        {
            if (lst == null) { return; }
            foreach (var action in lst)
            {
                action?.Invoke(msg, url); // ������Ϣ�� ���ص�ַ
            }
        }


        public void Close()
        {
            OnProgress?.Clear();
            OnFinish?.Clear();
            OnErr?.Clear();

            OnProgress = null;
            OnFinish = null;
            OnErr = null;
        }
    }

}