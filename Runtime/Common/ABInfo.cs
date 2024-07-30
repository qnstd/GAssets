using System.Collections.Generic;
namespace cngraphi.gassets.common
{
    /// <summary>
    /// AssetBundle文件信息
    /// <para>作者：强辰</para>
    /// </summary>
    public class ABInfo
    {
        /// <summary>
        /// AssetBundle文件名（唯一索引）（例如：xxx.ab）
        /// </summary>
        public string m_name;
        /// <summary>
        /// 包体尺寸（单位：字节）
        /// </summary>
        public int m_size;
        /// <summary>
        /// Hash MD5值
        /// </summary>
        public string m_hash;
        /// <summary>
        /// 包含内容（以Assets开头的路径）
        /// </summary>
        public List<string> m_contains;
        /// <summary>
        /// 依赖项 AssetBundle 文件名
        /// </summary>
        public List<string> m_depends;
    }

}

