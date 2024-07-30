using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using cngraphi.gassets.common;

namespace cngraphi.gassets
{
    /// <summary>
    /// 资源管理配置
    /// <para>作者：强辰</para>
    /// </summary>
    public class GAssetManifest
    {
        //版本号
        static private string m_version = "";

        //资源总尺寸
        static private int m_size = 0;

        //AssetBundle信息
        static private Dictionary<string, ABInfo> m_abs = new Dictionary<string, ABInfo>();

        //资源映射AssetBundle
        //key=>资源文件名（xx.yy）；value=>所在ab文件名（xxx.ab)
        static private Dictionary<string, string> m_res2ab = new Dictionary<string, string>();


        /// <summary>
        /// 加载并解析
        /// </summary>
        /// <param name="path">Manifest配置文件路径</param>
        /// <returns>true：加载并解析成功；false：失败</returns>
        static public bool Load(string path)
        {
            if (string.IsNullOrEmpty(path)) { return false; }
            if (!File.Exists(path)) { return false; };

            /*
                同步加载配置
                
                编辑器模式：可以使用File文件类进行读取；
                发布模式（移动平台）：必须将manifest文件存放到数据目录下（GAssetManager初始化的数据目录参数），否则无法使用File文件流读取主资源配置；
            */
            string contents = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            Str.Split(contents, "\n", out List<string> lines);


            //清理
            m_res2ab.Clear();
            m_abs.Clear();

            //解析
            m_version = lines[0];
            m_size = int.Parse(lines[1]);

            int len = lines.Count;
            for (int i = 2; i < len; i++)
            {
                Str.Split(lines[i], "|", out List<string> rs);
                ABInfo abinfo = new ABInfo();

                //基础信息
                abinfo.m_name = rs[0];
                abinfo.m_size = int.Parse(rs[1]);
                abinfo.m_hash = rs[2];

                //包含信息
                if (rs.Count > 3)
                {
                    Str.Split(rs[3], ",", out List<string> childs);

                    //将包含信息反映射
                    int childslen = childs.Count;
                    for (int j = 0; j < childslen; j++)
                    {
                        string childname = childs[j].Substring(childs[j].LastIndexOf("/") + 1);

                        //TODO：特殊处理，场景烘焙的光贴图以路径作为key值做索引
                        int _indx_ = childname.IndexOf(".");
                        if (_indx_ != -1 && childname.Substring(_indx_).ToLower() == ".exr")
                        {
                            childname = childs[j];
                        }
                        //END

                        if (m_res2ab.ContainsKey(childname))
                        {
                            throw new Exception("解析Manifest文件异常！资源名重复。childname = " + childname);
                        }
                        //Log.Debug("childname = " + childname);
                        m_res2ab.Add(childname, abinfo.m_name);
                    }
                    abinfo.m_contains = childs;
                }

                //依赖信息
                if (rs.Count > 4)
                {
                    Str.Split(rs[4], ",", out List<string> depends);
                    abinfo.m_depends = depends;
                }

                //将abinfo添加至abs管理组
                m_abs.Add(abinfo.m_name, abinfo);
            }

            return true;
        }



        /// <summary>
        /// 资源总尺寸
        /// </summary>
        /// <returns></returns>
        static public int Size { get { return m_size; } }



        /// <summary>
        /// 资源版本号
        /// </summary>
        /// <returns></returns>
        static public string Version { get { return m_version; } }



        /// <summary>
        /// 通过资源名称获取所在的AssetBundle名称
        /// </summary>
        /// <param name="resname">资源名称（包含文件名后缀，小写。注意：场景烘焙的光贴图应包含相对路径。例如：assets/scenes/examplescene/lightmap-0_comp_light）</param>
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
        /// 获取AssetBundle的依赖项名称
        /// </summary>
        /// <param name="abname">AssetBundle名称（带文件名后缀）</param>
        /// <param name="result">结果集</param>
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
        /// 快速获取参数对应的AssetBundle的依赖项名称
        /// </summary>
        /// <param name="abname">AssetBundle名称（带文件名后缀）</param>
        /// <returns>结果集</returns>
        static public string[] GetABDependsNameFast(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }
            if (!m_abs.ContainsKey(abname)) { return null; }

            m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (abinfo.m_depends == null) { return null; }

            return abinfo.m_depends.ToArray();
        }



        /// <summary>
        /// 获取AssetBundle的依赖项信息
        /// </summary>
        /// <param name="abname">AssetBundle名称（带文件名后缀）</param>
        /// <param name="result">结果集</param>
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
        /// 快速获取AssetBundle的依赖项信息
        /// </summary>
        /// <param name="abname">AssetBundle名称（带文件名后缀）</param>
        /// <returns>结果集</returns>
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
        /// 获取AssetBundle信息
        /// </summary>
        /// <param name="abname">ab文件名称</param>
        /// <returns></returns>
        static public ABInfo GetABInfo(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }
            m_abs.TryGetValue(abname, out ABInfo value);
            return value;
        }



        /// <summary>
        /// 获取ab包内部包含的元素
        /// </summary>
        /// <param name="abname">ab文件名称（带文件名后缀）</param>
        /// <returns></returns>
        static public List<string> GetABChilds(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return null; }

            bool result = m_abs.TryGetValue(abname, out ABInfo abinfo);
            if (!result) { return null; }

            return abinfo.m_contains;
        }



        /// <summary>
        /// 通过资源名称查找其路径（以Assets开头）
        /// </summary>
        /// <param name="resname">资源文件名（带文件名后缀）</param>
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
        /// 是否包含参数对应的ab数据对象
        /// <para>ab数据对象包含以ab为扩展名及dll扩展名的文件</para>
        /// </summary>
        /// <param name="abname">ab文件名称（带文件名后缀）</param>
        /// <returns></returns>
        static public bool ExistAB(string abname)
        {
            if (string.IsNullOrEmpty(abname)) { return false; }
            if (!m_abs.ContainsKey(abname)) { return false; }
            return true;
        }

    }
}