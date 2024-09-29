namespace cngraphi.gassets
{
    /// <summary>
    /// 下载状态
    /// <para>作者：强辰</para>
    /// </summary>
    public enum GDownloadStatus
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None,
        /// <summary>
        /// 开始下载
        /// </summary>
        Start,
        /// <summary>
        /// 正在下载
        /// </summary>
        Ing,
        /// <summary>
        /// 下载完成
        /// </summary>
        Finish,
        /// <summary>
        /// 下载错误
        /// </summary>
        Err
    }
}
