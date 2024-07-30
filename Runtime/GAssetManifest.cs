using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using cngraphi.gassets.common;

namespace cngraphi.gassets
{
    /// <summary>
    /// ��Դ��������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class GAssetManifest
    {
        //�汾��
        static private string m_version = "";

        //��Դ�ܳߴ�
        static private int m_size = 0;

        //AssetBundle��Ϣ
        static private Dictionary<string, ABInfo> m_abs = new Dictionary<string, ABInfo>();

        //��Դӳ��AssetBundle
        //key=>��Դ�ļ�����xx.yy����value=>����ab�ļ�����xxx.ab)
        static private Dictionary<string, string> m_res2ab = new Dictionary<string, string>();


        /// <summary>
        /// ���ز�����
        /// </summary>
        /// <param name="path">Manifest�����ļ�·��</param>
        /// <returns>true�����ز������ɹ���false��ʧ��</returns>
        static public bool Load(string path)
        {
            if (string.IsNullOrEmpty(path)) { return false; }
            if (!File.Exists(path)) { return false; };

            /*
                ͬ����������
                
                �༭��ģʽ������ʹ��File�ļ�����ж�ȡ��
                ����ģʽ���ƶ�ƽ̨�������뽫manifest�ļ���ŵ�����Ŀ¼�£�GAssetManager��ʼ��������Ŀ¼�������������޷�ʹ��File�ļ�����ȡ����Դ���ã�
            */
            string contents = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            Str.Split(contents, "\n", out List<string> lines);


            //����
            m_res2ab.Clear();
            m_abs.Clear();

            //����
            m_version = lines[0];
            m_size = int.Parse(lines[1]);

            int len = lines.Count;
            for (int i = 2; i < len; i++)
            {
                Str.Split(lines[i], "|", out List<string> rs);
                ABInfo abinfo = new ABInfo();

                //������Ϣ
                abinfo.m_name = rs[0];
                abinfo.m_size = int.Parse(rs[1]);
                abinfo.m_hash = rs[2];

                //������Ϣ
                if (rs.Count > 3)
                {
                    Str.Split(rs[3], ",", out List<string> childs);

                    //��������Ϣ��ӳ��
                    int childslen = childs.Count;
                    for (int j = 0; j < childslen; j++)
                    {
                        string childname = childs[j].Substring(childs[j].LastIndexOf("/") + 1);

                        //TODO�����⴦�������決�Ĺ���ͼ��·����Ϊkeyֵ������
                        int _indx_ = childname.IndexOf(".");
                        if (_indx_ != -1 && childname.Substring(_indx_).ToLower() == ".exr")
                        {
                            childname = childs[j];
                        }
                        //END

                        if (m_res2ab.ContainsKey(childname))
                        {
                            throw new Exception("����Manifest�ļ��쳣����Դ���ظ���childname = " + childname);
                        }
                        //Log.Debug("childname = " + childname);
                        m_res2ab.Add(childname, abinfo.m_name);
                    }
                    abinfo.m_contains = childs;
                }

                //������Ϣ
                if (rs.Count > 4)
                {
                    Str.Split(rs[4], ",", out List<string> depends);
                    abinfo.m_depends = depends;
                }

                //��abinfo�����abs������
                m_abs.Add(abinfo.m_name, abinfo);
            }

            return true;
        }



        /// <summary>
        /// ��Դ�ܳߴ�
        /// </summary>
        /// <returns></returns>
        static public int Size { get { return m_size; } }



        /// <summary>
        /// ��Դ�汾��
        /// </summary>
        /// <returns></returns>
        static public string Version { get { return m_version; } }



        /// <summary>
        /// ͨ����Դ���ƻ�ȡ���ڵ�AssetBundle����
        /// </summary>
        /// <param name="resname">��Դ���ƣ������ļ�����׺��Сд��ע�⣺�����決�Ĺ���ͼӦ�������·�������磺assets/scenes/examplescene/lightmap-0_comp_light��</param>
        /// <returns></returns>
        static public string GetABNameByResName(string resname)
        {
            if (string.IsNullOrEmpty(resname)) { return null; }
            if (!m_res2ab.ContainsKey(resname)) { return null; }

            m_res2ab.TryGetValue(resname, out string abname);
            if (string.IsNullOrEmpty(abname)) { return null; }

            return abname;
        }



        /// <summary>
        /// ��ȡAssetBundle������������
        /// </summary>
        /// <param name="abname">AssetBundle���ƣ����ļ�����׺��</param>
        /// <param name="result">�����</param>
        static public void GetABDependsName(string abname, List<string> result)
        {
            GetABDependsName_(abname, abname, result);
        }
        static private void GetABDependsName_(string oriname, string abname, List<string> result)
        {
            if (string.IsNullOrEmpty(abname)) { return; }
            if (!m_abs.ContainsKey(abname)) { return; }

            m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (abinfo.m_depends == null) { return; }

            int len = abinfo.m_depends.Count;
            for (int i = 0; i < len; i++)
            {
                string child = abinfo.m_depends[i];
                m_abs.TryGetValue(child, out ABInfo childAbinfo);
                if (oriname == childAbinfo.m_name) { continue; }
                if (result.IndexOf(child) != -1) { continue; }

                if (childAbinfo.m_depends != null)
                    GetABDependsName_(oriname, child, result);
                result.Add(child);
            }
        }



        /// <summary>
        /// ���ٻ�ȡ������Ӧ��AssetBundle������������
        /// </summary>
        /// <param name="abname">AssetBundle���ƣ����ļ�����׺��</param>
        /// <returns>�����</returns>
        static public string[] GetABDependsNameFast(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }
            if (!m_abs.ContainsKey(abname)) { return null; }

            m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (abinfo.m_depends == null) { return null; }

            return abinfo.m_depends.ToArray();
        }



        /// <summary>
        /// ��ȡAssetBundle����������Ϣ
        /// </summary>
        /// <param name="abname">AssetBundle���ƣ����ļ�����׺��</param>
        /// <param name="result">�����</param>
        static public void GetABDependsInfo(string abname, List<ABInfo> result)
        {
            GetABDependsInfo_(abname, abname, result);
        }
        static private void GetABDependsInfo_(string oriname, string abname, List<ABInfo> result)
        {
            if (string.IsNullOrEmpty(abname)) { return; }
            if (!m_abs.ContainsKey(abname)) { return; }

            m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (abinfo.m_depends == null) { return; }

            int len = abinfo.m_depends.Count;
            for (int i = 0; i < len; i++)
            {
                m_abs.TryGetValue(abinfo.m_depends[i], out ABInfo depABInfo);
                if (oriname == depABInfo.m_name) { continue; }
                if (result.IndexOf(depABInfo) != -1) { continue; }

                if (depABInfo.m_depends != null)
                    GetABDependsInfo_(oriname, depABInfo.m_name, result);
                result.Add(depABInfo);
            }
        }



        /// <summary>
        /// ���ٻ�ȡAssetBundle����������Ϣ
        /// </summary>
        /// <param name="abname">AssetBundle���ƣ����ļ�����׺��</param>
        /// <returns>�����</returns>
        static public ABInfo[] GetABDependsInfoFast(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }
            if (!m_abs.ContainsKey(abname)) { return null; }

            m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (abinfo.m_depends == null) { return null; }

            int len = abinfo.m_depends.Count;
            ABInfo[] infos = new ABInfo[len];
            for (int i = 0; i < len; i++)
            {
                m_abs.TryGetValue(abinfo.m_depends[i], out ABInfo depABInfo);
                infos.SetValue(depABInfo, i);
            }
            return infos;
        }



        /// <summary>
        /// ��ȡAssetBundle��Ϣ
        /// </summary>
        /// <param name="abname">ab�ļ�����</param>
        /// <returns></returns>
        static public ABInfo GetABInfo(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }
            m_abs.TryGetValue(abname, out ABInfo value);
            return value;
        }



        /// <summary>
        /// ��ȡab���ڲ�������Ԫ��
        /// </summary>
        /// <param name="abname">ab�ļ����ƣ����ļ�����׺��</param>
        /// <returns></returns>
        static public List<string> GetABChilds(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }

            bool result = m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (!result) { return null; }

            return abinfo.m_contains;
        }



        /// <summary>
        /// ͨ����Դ���Ʋ�����·������Assets��ͷ��
        /// </summary>
        /// <param name="resname">��Դ�ļ��������ļ�����׺��</param>
        /// <returns></returns>
        static public string GetResPathInAB(string resname)
        {
            if (string.IsNullOrEmpty(resname)) { return null; }

            string abname = GetABNameByResName(resname);
            if (abname == null) { return null; }

            List<string> lst = GetABChilds(abname);
            if (lst == null) { return null; }

            return lst.Find(delegate (string path)
            {
                return path.IndexOf(resname) != -1;
            }); ;
        }



        /// <summary>
        /// �Ƿ����������Ӧ��ab���ݶ���
        /// <para>ab���ݶ��������abΪ��չ����dll��չ�����ļ�</para>
        /// </summary>
        /// <param name="abname">ab�ļ����ƣ����ļ�����׺��</param>
        /// <returns></returns>
        static public bool ExistAB(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return false; }
            if (!m_abs.ContainsKey(abname)) { return false; }
            return true;
        }

    }
}