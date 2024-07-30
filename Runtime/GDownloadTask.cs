using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using cngraphi.gassets.common;
using UnityEngine;
using UnityEngine.Networking;

namespace cngraphi.gassets
{
    /// <summary>
    /// ��������
    /// <para>֧�ֶϵ����������ļ��ֶ�����</para>
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GDownloadTask
    {
        /// <summary>
        /// ��ȡ http/https ��ͷ��Ϣ�е���Դ�ֽ�������keyֵ
        /// </summary>
        const string ContentLengthKey = "Content-Length";

        /// <summary>
        /// �ֶ��ֽ���
        /// </summary>
        const double SubsectionSize = 550000000; // Լ 500MB ����  // 1100000: Լ 1MB ���ң� 1000000000: Լ 0.9GB ����


        /// <summary>
        /// ��ǰ��Э�̲�����
        /// </summary>
        public Coroutine Corou { get; set; } = null;
        /// <summary>
        /// ��ǰ������������
        /// </summary>
        public UnityWebRequest Request { get; set; } = null;
        /// <summary>
        /// ��ǰ���ؽ���
        /// <para>��Χ��0-1</para>
        /// </summary>
        public double Progress { get; set; } = 0;
        /// <summary>
        /// �ߴ�
        /// <para>��λ���ֽ�</para>
        /// </summary>
        public ulong Size { get; set; } = 0;
        /// <summary>
        /// ���ص�ַ
        /// </summary>
        public string URL { get; set; } = null;
        /// <summary>
        /// �����ַ
        /// </summary>
        public string SaveURL { get; set; } = null;
        /// <summary>
        /// �ļ���
        /// <para>���ļ�����׺</para>
        /// </summary>
        public string FileName { get; set; } = null;
        /// <summary>
        /// ����״̬
        /// </summary>
        public GDownloadStatus Status { get; set; } = GDownloadStatus.None;
        /// <summary>
        /// �����ٶȣ�ÿ����ٶȣ�
        /// </summary>
        public ulong Speed { get; private set; } = 0;
        /// <summary>
        /// �������
        /// </summary>
        public GDownloadTaskHandler Handler { get; private set; } = new GDownloadTaskHandler();


        /// <summary>
        /// ����
        /// </summary>
        /// <param name="url">���ص�ַ</param>
        /// <param name="progress">���ؽ��Ȼص�</param>
        /// <param name="finish">������ɻص�</param>
        /// <param name="err">���ش���ص�</param>
        public GDownloadTask(string url, Action<double, ulong, ulong> progress = null, Action finish = null, Action<string, string> err = null)
        {
            URL = url;

            Str.Split(URL, "/", out List<string> lst);
            FileName = lst[lst.Count - 1];

            string folder = Paths.PersistentPathAppend(GAssetManager.settings.AssetDownloadPath);
            SaveURL = Paths.Replace(Path.Combine(folder, FileName));

            Handler.Add(Handler.OnErr, err);
            Handler.Add(Handler.OnFinish, finish);
            Handler.Add(Handler.OnProgress, progress);

            Status = GDownloadStatus.None;
        }



        /// <summary>
        /// �жϡ�ȡ�����أ����ͷ�.
        /// <para>���ô˺���֮��������󲻿��ٴ�ʹ��</para>
        /// </summary>
        public void Cancel()
        {
            URL = null;
            SaveURL = null;
            FileName = null;
            Progress = 0;
            Size = 0;
            if (Handler != null)
                Handler.Close();
            Handler = null;

            if
            (
                Status == GDownloadStatus.Start ||
                Status == GDownloadStatus.Ing ||
                Status == GDownloadStatus.Err
            )
            {
                if (Corou != null)
                {// ֹͣ��ǰЭ��
                    GDownload.Ins.StopCoroutine(Corou);
                }
                if (Request != null)
                {// ֹͣ���ز��ͷ�������
                    Request.Abort();
                    Request.Dispose();
                }
            }

            Corou = null;
            Request = null;
            time = 0;
            havebytes = 0;
            Status = GDownloadStatus.None;
        }



        /// <summary>
        /// ��������
        /// </summary>
        public void Start()
        {
            Status = GDownloadStatus.Start;
            time = 0;
            havebytes = 0;
            Corou = GDownload.Ins.StartCoroutine(OnTask());
        }




        float time = 0;
        ulong havebytes = 0;
        IEnumerator OnTask()
        {
            Request = UnityWebRequest.Head(URL);
            Status = GDownloadStatus.Ing;
            yield return Request.SendWebRequest();
            if (!string.IsNullOrEmpty(Request.error))
            {
                OnTaskErr(Request.error);
                yield break;
            }

            // ��ȡ��Դ��С
            Size = ulong.Parse(Request.GetResponseHeader(ContentLengthKey));
            Request.Dispose();

            // ��ʼ����
            Request = UnityWebRequest.Get(URL);
            Request.downloadHandler = new DownloadHandlerFile(SaveURL, true);
            FileInfo file = new FileInfo(SaveURL);
            ulong filelen = (ulong)file.Length;
            Request.SetRequestHeader("Range", "bytes=" + filelen + "-"); // �ϵ������� ��filelen���ȿ�ʼ����������ʽ: bytes=0-100 ��

            if (!string.IsNullOrEmpty(Request.error))
            {
                OnTaskErr(Request.error);
                yield break;
            }

            if (filelen < Size)
            {
                Request.SendWebRequest(); // �ٴ�����
                while (!Request.isDone)
                {
                    ulong downloadBytes = Request.downloadedBytes;
                    Progress = (downloadBytes + filelen) / (double)Size;

                    // ÿ��1�����һ�������ٶ�
                    time += Time.deltaTime;
                    if (time >= 1)
                    {
                        Speed = downloadBytes - havebytes;
                        time = 0;
                        havebytes = downloadBytes;
                    }

                    Handler?.Run(Handler.OnProgress, Progress, Speed, Size);

                    if (downloadBytes >= SubsectionSize)
                    {// �����������ڵ��ڷֶμ���ʱ��������Э�̴�����ֹunity�׳��������⣨Error: Insecure connection not allowed��

                        GDownload.Ins.StopCoroutine(Corou);
                        Request.Abort(); // ��������ֹͣ�������

                        if (!string.IsNullOrEmpty(Request.error))
                        {
                            OnTaskErr(Request.error);
                            yield break;
                        }

                        Request.Dispose();
                        Corou = GDownload.Ins.StartCoroutine(OnTask());
                        yield return Corou;
                    }

                    yield return null;
                }
            }

            Request.Dispose();
            Progress = 1.0f;
            Debug.Log($"�������. URL = {URL}");
            Handler?.Run(Handler.OnFinish);
            Status = GDownloadStatus.Finish;
        }


        private void OnTaskErr(string msg)
        {
            Debug.LogError($"���ش��� ( {msg} ) / URL = " + URL);
            Handler?.Run(Handler.OnErr, msg, URL);
            Status = GDownloadStatus.Err;
        }

    }
}