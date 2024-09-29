namespace cngraphi.gassets
{
    /// <summary>
    /// 加载状态
    /// <para>作者：强辰</para>
    /// </summary>
    public enum GLoaderStatus
    {
        /// <summary>
        /// 等待
        /// </summary>
        Wait,
        /// <summary>
        /// 正在加载
        /// </summary>
        Loading,
        /// <summary>
        /// 加载成功
        /// </summary>
        Suc,
        /// <summary>
        /// 加载失败
        /// </summary>
        Fail,
        /// <summary>
        /// 卸载
        /// </summary>
        Unloaded,
        /// <summary>
        /// 依赖加载
        /// </summary>
        DependsLoading
    }
}

