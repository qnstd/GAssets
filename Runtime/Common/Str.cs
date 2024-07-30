using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace cngraphi.gassets.common
{
    /// <summary>
    /// �ַ�������
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public class Str
    {
        /// <summary>
        /// �ַ������
        /// </summary>
        /// <param name="ssrc">Դ�ַ���</param>
        /// <param name="ssplit">��ַ�</param>
        /// <param name="list">Դ�ַ�����ֺ���ַ�����</param>
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
        /// ���ļ�����md5ֵ
        /// </summary>
        /// <param name="path">�ļ�·��</param>
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
        /// ���ַ������м���
        /// <para>����32λСд�ַ���</para>
        /// <para>��UTF8���б���</para>
        /// </summary>
        /// <param name="str">Դ�ַ���</param>
        /// <returns></returns>
        static public string MD5_Str(string str) { return MD5_Str(str, Encoding.UTF8); }



        /// <summary>
        /// ���ַ������м���
        /// <para>����32λСд�ַ���</para>
        /// </summary>
        /// <param name="str">Դ�ַ���</param>
        /// <param name="ecode">�ַ�����</param>
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
        /// �ֽ�תB��KB��MB��GB��
        /// </summary>
        /// <param name="size">�ֽڳ���</param>
        /// <param name="format">�ַ���ʽ</param>
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