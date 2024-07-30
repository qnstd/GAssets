using System.Collections.Generic;
using System.IO;
using System.Text;
using cngraphi.gassets.common;
using cngraphi.gassets.editor.common;
using UnityEditor;
using UnityEngine;


namespace cngraphi.gassets.editor
{
    /// <summary>
    /// ��Դ���컯�ȶ�
    /// <para>���ߣ�ǿ��</para>
    /// </summary>
    public partial class GAssetEditOperate : EditorWindow
    {
        // �汾���컯�ļ��洢��Ŀ¼���ƺ�׺ �� ���컯�ļ����õ��ļ���ǰ׺
        const string C_HotfixDirPrefix = "hotfix";
        // �汾���컯��������ʷ�汾���ļ��洢Ŀ¼���ƺ�׺
        const string C_HotfixDirPrefix_History = "hotfix_histroy";


        string m_diffverPath = "";
        bool m_isDelDiff = false;
        string m_diffpath = "";


        private void OnEnable_VerCompare() { }
        private void OnDisable_VerCompare() { }
        private void OnGUI_VerCompare()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("���컯�ȶ��嵥", Gui.LabelStyle, GUILayout.Width(90));
            if (GUILayout.Button("ѡ��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                string p = Dialog.File("ѡ����컯�ȶ��嵥");
                m_diffverPath = string.IsNullOrEmpty(p) ? m_diffverPath : p;
            }
            if (GUILayout.Button("ִ��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                GenDiffVersion();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            m_diffverPath = EditorGUILayout.TextField("", m_diffverPath, GUILayout.Height(30));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            m_isDelDiff = EditorGUILayout.Toggle(m_isDelDiff, GUILayout.Width(20), GUILayout.Height(20));
            EditorGUILayout.LabelField("<color=#d0a600>�Ƿ񽫲��컯�ļ��ӵ�ǰ��Դ����Ŀ¼��ɾ��</color>", Gui.LabelStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "<color=#999999>" +
                "* ���컯�ȶ��嵥��ָ�����汾��Ӧ�� Manifest �嵥�ļ�������ʱ�뵱ǰ���嵥�ļ����бȶԣ�\n" +
                "* �˹������ڵ�ǰ�嵥����һ�汾�Ŷ�Ӧ���嵥���бȶԣ�\n" +
                "* ���컯�ȶԲ���Ϊ������ʽ��ֻ���������޸��ļ����м�¼������ɾ�����ļ�����Ϊ��Դ���컯�ȶԷ��룻\n" +
                "* ÿ��ִ�в��컯�ȶ�ʱ���������һ�εıȶԽ��������ʱ�����⣻" +
                "</color>"
                , Gui.LabelStyle, GUILayout.Height(45));

            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);


            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("���컯�嵥Ŀ¼", Gui.LabelStyle, GUILayout.Width(90));
            if (GUILayout.Button("ѡ��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                string p = Dialog.Directory("ѡ����컯 Manifest �嵥Ŀ¼");
                m_diffpath = string.IsNullOrEmpty(p) ? m_diffpath : p;
            }
            if (GUILayout.Button("ִ��", Gui.BtnStyle, GUILayout.Width(50), GUILayout.Height(18)))
            {
                GenAllDiffVersionFiles();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            m_diffpath = EditorGUILayout.TextField("", m_diffpath, GUILayout.Height(30));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "<color=#999999>" +
                "* ���ݵ�ǰ�汾�� Manifest��Ϊ֮ǰÿһ���汾�� Manifest ���ɲ��컯�嵥��\n" +
                "* ֻ���ɲ��컯�嵥�������������컯�ļ���" +
                "</color>", Gui.LabelStyle, GUILayout.Height(34));
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }



        /// <summary>
        /// ���컯�ȶ�
        /// </summary>
        private void GenDiffVersion()
        {
            if (Dialog.Confirm("ȷ�Ͽ�ʼִ�в��컯�ȶ�?") != 0) { return; }
            if (string.IsNullOrEmpty(m_diffverPath)) { Dialog.Tip("��ѡ����컯�ȶ��ļ�"); return; }

            string outp = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (string.IsNullOrEmpty(outp)) { Dialog.Tip("�������������ù�����Դ��·����"); return; }

            //���ɴ洢��Ŀ¼
            outp = cngraphi.gassets.common.Paths.Replace(outp);
            int indx = outp.LastIndexOf("/");
            string diffdir = Path.Combine(
                                            outp.Substring(0, indx),
                                            outp.Substring(indx + 1) + "_" + C_HotfixDirPrefix
                                          );
            diffdir = cngraphi.gassets.common.Paths.Replace(diffdir);
            if (!System.IO.Directory.Exists(diffdir)) { System.IO.Directory.CreateDirectory(diffdir); }
            else { IO.DirClear(diffdir); }


            //��ȡ��ǰ���Ŀ¼�µ� Manifest �嵥
            string curpath = Path.Combine(outp, "manifest");
            if (!System.IO.File.Exists(curpath))
            {
                Dialog.Confirm("��ǰ��Դ�����Ŀ¼��δ�ҵ� Manifest �ļ�");
                return;
            }
            string[] news = System.IO.File.ReadAllLines(curpath);
            string newversion = news[0];

            //��ȡ��ѡ��� Manifest �嵥
            Dictionary<string, string> origins = new Dictionary<string, string>();
            string[] orign = System.IO.File.ReadAllLines(m_diffverPath);
            int len = orign.Length;
            for (int i = 2; i < len; i++)
            {
                Str.Split(orign[i], "|", out List<string> rs);
                origins.Add(rs[0], rs[2]);
            }

            //���бȶ�
            int newslen = news.Length;
            int allsize = 0;
            StringBuilder cnf_base = new StringBuilder();
            //cnf_base.Append(newversion + C_LineSeq); //�汾�ţ��뵱ǰ���� manifest �����еİ汾��һ�£�
            StringBuilder cnf_info = new StringBuilder();
            for (int i = 2; i < newslen; i++)
            {
                Str.Split(news[i], "|", out List<string> rs);
                string filename = rs[0];
                origins.TryGetValue(filename, out string originval);
                if (string.IsNullOrEmpty(originval) || originval != rs[2])
                {//���� or ��Դ���޸ġ���������������ǰ Manifest ������ɾ�����ļ�����Ϊ��Դ���µı�׼��
                    //�����ļ�
                    string srcfile = Path.Combine(outp, filename);
                    IO.CopyFile(srcfile, Path.Combine(diffdir, filename));
                    allsize += int.Parse(rs[1]);
                    cnf_info.Append(filename + C_LineSeq);
                    //ɾ���ļ�
                    if (m_isDelDiff)
                    {
                        System.IO.File.Delete(srcfile);
                        System.IO.File.Delete(srcfile + ".manifest");
                    }
                }
            }
            cnf_base.Append(allsize.ToString()); //���컯�ļ���С����λ���ֽڣ�

            //���ɱȶ��嵥
            string cnf = cnf_base.ToString().Trim() + C_LineSeq + cnf_info.ToString().Trim();
            cnf = cnf.Trim();
            string savefileName = C_HotfixDirPrefix + "_" + newversion + "_" + orign[0];
            System.IO.File.WriteAllBytes(Path.Combine(diffdir, savefileName), Encoding.UTF8.GetBytes(cnf));

            //�����µ���Դ�嵥Ҳ���������컯Ŀ¼
            IO.CopyFile(curpath, Path.Combine(diffdir, "manifest"));

            AssetDatabase.Refresh();
            Dialog.Tip("���컯�ȶ���ϣ�");
        }


        /// <summary>
        /// ����������ʷ�汾�Ĳ��컯����
        /// </summary>
        private void GenAllDiffVersionFiles()
        {
            if (string.IsNullOrEmpty(m_diffpath))
            {
                Dialog.Tip("δѡ����컯�嵥Ŀ¼");
                return;
            }

            string outp = cngraphi.gassets.common.Paths.StreamingPathAppend(settings.AssetRootPath);
            if (string.IsNullOrEmpty(outp)) { Dialog.Tip("�������������ù�����Դ��·����"); return; }
            if (Dialog.Confirm("ȷ�Ͽ�ʼִ��������ʷ�汾�Ĳ��컯�嵥�ȶ�?") != 0) { return; }

            List<string> files = new List<string>();
            IO.GetFiles(m_diffpath, files);
            int len = files.Count;
            if (len == 0)
            {
                Dialog.Tip("ѡ���Ŀ¼��δ�ҵ��κ� Manifest �嵥�ļ�");
                return;
            }

            //��ȡ��ǰ���Ŀ¼�µ����� Manifest �ļ�
            string curpath = Path.Combine(outp, "manifest");
            if (!System.IO.File.Exists(curpath))
            {
                Dialog.Confirm("��ǰ���Ŀ¼��δ�ҵ� Manifest �ļ�");
                return;
            }
            string[] news = System.IO.File.ReadAllLines(curpath);
            string newversion = news[0];
            int newslen = news.Length;

            //���ɴ洢Ŀ¼
            outp = cngraphi.gassets.common.Paths.Replace(outp);
            int indx = outp.LastIndexOf("/");
            string savedir = Path.Combine(
                                            outp.Substring(0, indx),
                                            outp.Substring(indx + 1) + "_" + C_HotfixDirPrefix_History
                                          );
            savedir = cngraphi.gassets.common.Paths.Replace(savedir);
            if (!System.IO.Directory.Exists(savedir)) { System.IO.Directory.CreateDirectory(savedir); }
            else { IO.DirClear(savedir); }

            //��ʼΪÿһ����ʷ�汾�� manifest ���в��컯�ȶ�
            for (int i = 0; i < len; i++)
            {
                //��ȡ��ʷ�汾
                string fname = files[i];
                Str.Split(fname, "_", out List<string> result);
                if (result[2] == newversion) { continue; } //��ʷ�汾�İ汾���뵱ǰ���� Manifest�� �İ汾��һ�£��򲻴���

                Dictionary<string, string> origins = new Dictionary<string, string>(); // ��Դab���ƣ���Դ��md5
                string[] orign = System.IO.File.ReadAllLines(fname);
                int _len = orign.Length;
                for (int j = 2; j < _len; j++)
                {
                    Str.Split(orign[j], "|", out List<string> rs);
                    origins.Add(rs[0], rs[2]);
                }

                //���бȶ�
                int allsize = 0;
                StringBuilder cnf_base = new StringBuilder();
                //cnf_base.Append(newversion + C_LineSeq); //�汾�ţ��뵱ǰ���� manifest �����еİ汾��һ�£�
                StringBuilder cnf_info = new StringBuilder();
                for (int k = 2; k < newslen; k++)
                {
                    Str.Split(news[k], "|", out List<string> rs);
                    string filename = rs[0];
                    origins.TryGetValue(filename, out string originval);
                    if (string.IsNullOrEmpty(originval) || originval != rs[2])
                    {//���� or ��Դ���޸ġ���������������ǰManifest������ɾ�����ļ�����Ϊ��Դ���µı�׼��
                        allsize += int.Parse(rs[1]);
                        cnf_info.Append(filename + C_LineSeq);
                    }
                }
                cnf_base.Append(allsize.ToString()); //���컯�ļ���С����λ���ֽڣ�

                //���ɱȶ�����
                string cnf = cnf_base.ToString().Trim() + C_LineSeq + cnf_info.ToString().Trim();
                cnf = cnf.Trim();
                string savefileName = C_HotfixDirPrefix + "_" + newversion + "_" + orign[0];
                System.IO.File.WriteAllBytes(Path.Combine(savedir, savefileName), Encoding.UTF8.GetBytes(cnf));
            }

            AssetDatabase.Refresh();
            Dialog.Tip("���컯�ȶ���ɣ�");
        }
    }

}