using System.Collections.Generic;
namespace cngraphi.gassets.common
{
    /// <summary>
    /// AssetBundle�ļ���Ϣ
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class ABInfo
    {
        /// <summary>
        /// AssetBundle�ļ�����Ψһ�����������磺xxx.ab��
        /// </summary>
        public string m_name;
        /// <summary>
        /// ����ߴ磨��λ���ֽڣ�
        /// </summary>
        public int m_size;
        /// <summary>
        /// Hash MD5ֵ
        /// </summary>
        public string m_hash;
        /// <summary>
        /// �������ݣ���Assets��ͷ��·����
        /// </summary>
        public List<string> m_contains;
        /// <summary>
        /// ������ AssetBundle �ļ���
        /// </summary>
        public List<string> m_depends;
    }

}

