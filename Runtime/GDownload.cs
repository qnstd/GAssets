using System;
using System.Collections.Generic;
using UnityEngine;

namespace cngraphi.gassets
{
    /// <summary>
    /// ����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GDownload : MonoBehaviour
    {
        static private Dictionary<string, GDownloadTask> m_dic = new Dictionary<string, GDownloadTask>();
        static private List<GDownloadTask> m_deletes = new List<GDownloadTask>();


        static public GDownload Ins { get; set; } = null;

        [RuntimeInitializeOnLoadMethod]
        static private void Initialize()
        {
            GameObject obj = new GameObject("GDownload");
            Ins = obj.AddComponent<GDownload>();
            DontDestroyOnLoad(obj);
        }



        private void OnDestroy()
        {
            foreach (var task in m_dic.Values)
            {
                task?.Cancel();
            }
            m_dic.Clear();
            foreach (var task in m_deletes)
            {
                task?.Cancel();
            }
            m_deletes.Clear();

            m_dic = null;
            m_deletes = null;
            Ins = null;
        }



        private void Update()
        {
            m_deletes.Clear();
            foreach (var task in m_dic.Values)
            {
                if
                (
                    task.Status == GDownloadStatus.None ||
                    task.Status == GDownloadStatus.Finish ||
                    task.Status == GDownloadStatus.Err
                )
                {
                    m_deletes.Add(task);
                }
            }
            foreach (var task in m_deletes)
            {
                string key = task.FileName;
                task?.Cancel();
                m_dic.Remove(key);
            }
        }



        /// <summary>
        /// ִ����������
        /// </summary>
        /// <param name="url">���ص�ַ</param>
        /// <param name="progress">���ؽ��Ȼص�</param>
        /// <param name="finish">������ɻص�</param>
        /// <param name="err">���ش���ص�</param>
        /// <returns>��������</returns>
        static public GDownloadTask Excute(string url, Action<double, ulong, ulong> progress = null, Action finish = null, Action<string, string> err = null)
        {
            if (string.IsNullOrEmpty(url)) { return null; }

            GDownloadTask task = new GDownloadTask(url, progress, finish, err);
            string key = task.FileName;

            if (m_dic.TryGetValue(key, out GDownloadTask _task))
            {// �Ѵ���
                task.Cancel();
                GDownloadTaskHandler handler = _task.Handler;
                if (handler != null)
                {
                    handler.Add(handler.OnErr, err);
                    handler.Add(handler.OnFinish, finish);
                    handler.Add(handler.OnProgress, progress);
                }
                return _task;
            }

            m_dic.Add(key, task);
            task.Start();
            return task;
        }



        /// <summary>
        /// ȡ������ִ�е���������
        /// </summary>
        /// <param name="key">����ֵ�������ļ����ļ�����</param>
        static public void UnExcute(string key)
        {
            if (string.IsNullOrEmpty(key)) { return; }
            if (!m_dic.TryGetValue(key, out GDownloadTask task)) { return; } // ������Ӧ������δ��ִ���б���

            if (task.Status == GDownloadStatus.Start || task.Status == GDownloadStatus.Ing)
            {
                task?.Cancel();
                m_dic.Remove(key);
            }
        }


    }
}