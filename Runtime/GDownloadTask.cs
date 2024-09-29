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
    /// 下载任务
    /// <para>支持断点续传、大文件分段下载</para>
    /// <para>作者：强辰</para>
    /// </summary>
    public class GDownloadTask
    {
        /// <summary>
        /// 读取 http/https 包头信息中的资源字节总数的key值
        /// </summary>
        const string ContentLengthKey = "Content-Length";

        /// <summary>
        /// 分段字节数
        /// </summary>
        const double SubsectionSize = 550000000; // 约 500MB 左右  // 1100000: 约 1MB 左右， 1000000000: 约 0.9GB 左右


        /// <summary>
        /// 当前的协程操作器
        /// </summary>
        public Coroutine Corou { get; set; } = null;
        /// <summary>
        /// 当前的网络下载器
        /// </summary>
        public UnityWebRequest Request { get; set; } = null;
        /// <summary>
        /// 当前下载进度
        /// <para>范围：0-1</para>
        /// </summary>
        public double Progress { get; set; } = 0;
        /// <summary>
        /// 尺寸
        /// <para>单位：字节</para>
        /// </summary>
        public ulong Size { get; set; } = 0;
        /// <summary>
        /// 下载地址
        /// </summary>
        public string URL { get; set; } = null;
        /// <summary>
        /// 保存地址
        /// </summary>
        public string SaveURL { get; set; } = null;
        /// <summary>
        /// 文件名
        /// <para>带文件名后缀</para>
        /// </summary>
        public string FileName { get; set; } = null;
        /// <summary>
        /// 任务状态
        /// </summary>
        public GDownloadStatus Status { get; set; } = GDownloadStatus.None;
        /// <summary>
        /// 下载速度（每秒的速度）
        /// </summary>
        public ulong Speed { get; private set; } = 0;
        /// <summary>
        /// 操作句柄
        /// </summary>
        public GDownloadTaskHandler Handler { get; private set; } = new GDownloadTaskHandler();


        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="progress">下载进度回调</param>
        /// <param name="finish">下载完成回调</param>
        /// <param name="err">下载错误回调</param>
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
        /// 中断、取消下载，并释放.
        /// <para>调用此函数之后，任务对象不可再次使用</para>
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
                {// 停止当前协程
                    GDownload.Ins.StopCoroutine(Corou);
                }
                if (Request != null)
                {// 停止下载并释放下载器
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
        /// 启动下载
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

            // 获取资源大小
            Size = ulong.Parse(Request.GetResponseHeader(ContentLengthKey));
            Request.Dispose();

            // 开始下载
            Request = UnityWebRequest.Get(URL);
            Request.downloadHandler = new DownloadHandlerFile(SaveURL, true);
            FileInfo file = new FileInfo(SaveURL);
            ulong filelen = (ulong)file.Length;
            Request.SetRequestHeader("Range", "bytes=" + filelen + "-"); // 断点续传（ 从filelen长度开始到结束。格式: bytes=0-100 ）

            if (!string.IsNullOrEmpty(Request.error))
            {
                OnTaskErr(Request.error);
                yield break;
            }

            if (filelen < Size)
            {
                Request.SendWebRequest(); // 再次请求
                while (!Request.isDone)
                {
                    ulong downloadBytes = Request.downloadedBytes;
                    Progress = (downloadBytes + filelen) / (double)Size;

                    // 每隔1秒计算一次下载速度
                    time += Time.deltaTime;
                    if (time >= 1)
                    {
                        Speed = downloadBytes - havebytes;
                        time = 0;
                        havebytes = downloadBytes;
                    }

                    Handler?.Run(Handler.OnProgress, Progress, Speed, Size);

                    if (downloadBytes >= SubsectionSize)
                    {// 当下载量大于等于分段计数时，则另起协程处理。防止unity抛出过载问题（Error: Insecure connection not allowed）

                        GDownload.Ins.StopCoroutine(Corou);
                        Request.Abort(); // 发出尽快停止请求操作

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
            Debug.Log($"下载完成. URL = {URL}");
            Handler?.Run(Handler.OnFinish);
            Status = GDownloadStatus.Finish;
        }


        private void OnTaskErr(string msg)
        {
            Debug.LogError($"下载错误 ( {msg} ) / URL = " + URL);
            Handler?.Run(Handler.OnErr, msg, URL);
            Status = GDownloadStatus.Err;
        }

    }
}