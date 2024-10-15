using System.Collections.Generic;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// 常量
    /// <para>作者：强辰</para>
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// 创建资源管理器配置文件的路径。此路径必须是 Resources 目录，且必须在根目录下保存。运行时状态下会自动从此路径下加载。
        /// </summary>
        public const string CONFIGURE_PATH = "Assets/Resources";
        /// <summary>
        /// 资源管理器配置文件名
        /// </summary>
        public const string CONFIGURE_FILENAME = "GAssetSettings";
        /// <summary>
        /// 资源库包名
        /// </summary>
        public const string PACKAGES_NAME = "com.cngraphi.gassets";
        /// <summary>
        /// 资源配置信息文件
        /// </summary>
        public const string MANIFEST = "manifest";
        /// <summary>
        /// 资源管理器显示名称
        /// </summary>
        public const string LIBRARY_DISPLAYNAME = "GAssets 资源管理框架";
        /// <summary>
        /// 文档标签/相对路径
        /// </summary>
        static public Dictionary<string, string> MDS = new Dictionary<string, string>()
        {
            {"文档","Documentation/gassets.md" },
            {"日志", "CHANGELOG.md" },
            {"许可","LICENSE.md" },
            {"自述","README.md" }
        };
    }

}

