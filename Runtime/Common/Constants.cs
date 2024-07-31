using System.Collections.Generic;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// ����
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// ������Դ�����������ļ���·������·�������� Resources Ŀ¼���ұ����ڸ�Ŀ¼�±��档����ʱ״̬�»��Զ��Ӵ�·���¼��ء�
        /// </summary>
        public const string CONFIGURE_PATH = "Assets/Resources";
        /// <summary>
        /// ��Դ�����������ļ���
        /// </summary>
        public const string CONFIGURE_FILENAME = "GAssetSettings";
        /// <summary>
        /// ��Դ�����
        /// </summary>
        public const string PACKAGES_NAME = "com.cngraphi.gassets";
        /// <summary>
        /// ��Դ������Ϣ�ļ�
        /// </summary>
        public const string MANIFEST = "manifest";
        /// <summary>
        /// ��Դ��������ʾ����
        /// </summary>
        public const string LIBRARY_DISPLAYNAME = "GAssets ��Դ������";
        /// <summary>
        /// �ĵ���ǩ/���·��
        /// </summary>
        static public Dictionary<string, string> MDS = new Dictionary<string, string>()
        {
            {"Documentation","Documentation/gassets.md" },
            {"Changelog", "CHANGELOG.md" },
            {"License","LICENSE.md" },
            {"Readme","README.md" }
        };
    }

}

