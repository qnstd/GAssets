using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// 字符串工具
    /// <para>作者：强辰</para>
    /// </summary>
    public class Str
    {
        /// <summary>
        /// 字符串拆分
        /// </summary>
        /// <param name="ssrc">源字符串</param>
        /// <param name="ssplit">拆分符</param>
        /// <param name="list">源字符串拆分后的字符数组</param>
        static public void Split(string ssrc, string ssplit, out List<string> list)
        {
            list = new List<string>();
            if (ssrc == null || ssrc == "" || ssrc.Length == 0)
            {
                list.Add(ssrc);
                return;
            }

            if (ssplit == null || ssplit == "" || ssplit.Length == 0)
            {
                list.Add(ssrc);
                return;
            }

            int ssplitLen = ssplit.Length;
            int i = 0, indx = 0;

            while (true)
            {
                indx = ssrc.IndexOf(ssplit, i);
                if (indx == -1)
                {
                    list.Add(ssrc.Substring(i));
                    break;
                }
                list.Add(ssrc.Substring(i, indx - i));
                i = indx + ssplitLen;
            }
        }


        /// <summary>
        /// 对文件生成md5值
        /// </summary>
        /// <param name="path">文件路径</param>
        static public string MD5_File(string path)
        {
            if (string.IsNullOrEmpty(path)) { return ""; }

            FileStream fs = new FileStream(path, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(fs);
            fs.Close();

            return __MD5(bytes);
        }


        /// <summary>
        /// 对字符串进行加密
        /// <para>生成32位小写字符串</para>
        /// <para>以UTF8进行编码</para>
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <returns></returns>
        static public string MD5_Str(string str) { return MD5_Str(str, Encoding.UTF8); }



        /// <summary>
        /// 对字符串进行加密
        /// <para>生成32位小写字符串</para>
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="ecode">字符编码</param>
        /// <returns></returns>
        static public string MD5_Str(string str, Encoding ecode)
        {
            using (MD5 mi = MD5.Create())
            {
                return __MD5(mi.ComputeHash(ecode.GetBytes(str)));
            }
        }

        static private string __MD5(byte[] byts)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < byts.Length; i++)
            {
                sb.Append(byts[i].ToString("x2"));
            }
            return sb.ToString();
        }


        /// <summary>
        /// 字节转B、KB、MB、GB等
        /// </summary>
        /// <param name="size">字节长度</param>
        /// <param name="format">字符格式</param>
        /// <returns></returns>
        static public string FromBytes(double size, string format = "f1")
        {
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return size.ToString(format) + units[i];
        }
    }
}